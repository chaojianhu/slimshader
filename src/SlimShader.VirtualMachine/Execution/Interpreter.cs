﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SlimShader.Chunks.Shex;
using SlimShader.Chunks.Shex.Tokens;
using SlimShader.VirtualMachine.Analysis.ExecutableInstructions;
using SlimShader.VirtualMachine.Registers;
using SlimShader.VirtualMachine.Resources;
using SlimShader.VirtualMachine.Util;

namespace SlimShader.VirtualMachine.Execution
{
    // NOTE: Gradient instructions can't be used in loops or branches, so all execution contexts must be active (right?).
    // AFAIK, this is enforced by the HLSL compiler. TODO: Check this.
	public class Interpreter : IShaderExecutor
	{
	    private readonly VirtualMachine _virtualMachine;
	    private readonly ExecutionContext[] _executionContexts;
		private readonly ExecutableInstruction[] _instructions;
		
        public Interpreter(VirtualMachine virtualMachine, ExecutionContext[] executionContexts, ExecutableInstruction[] instructions)
		{
            _virtualMachine = virtualMachine;
            _executionContexts = executionContexts;
			_instructions = instructions;
		}

		/// <summary>
		/// http://http.developer.nvidia.com/GPUGems2/gpugems2_chapter34.html
		/// http://people.maths.ox.ac.uk/gilesm/pp10/lec2_2x2.pdf
		/// http://stackoverflow.com/questions/10119796/how-does-cuda-compiler-know-the-divergence-behaviour-of-warps
		/// http://www.istc-cc.cmu.edu/publications/papers/2011/SIMD.pdf
		/// http://hal.archives-ouvertes.fr/docs/00/62/26/54/PDF/collange_sympa2011_en.pdf
		/// </summary>
		public IEnumerable<ExecutionResponse> Execute()
		{
		    var warp = new Warp(_executionContexts.Length);
		    var activeExecutionContexts = GetActiveExecutionContexts(warp.DivergenceStack.Peek());
			while (warp.DivergenceStack.Peek().NextPC < _instructions.Length)
			{
				var topOfDivergenceStack = warp.DivergenceStack.Peek();
				int pc = topOfDivergenceStack.NextPC;
				var instruction = _instructions[pc];

			    List<BitArray> activeMasks = null;

				switch (instruction.OpcodeType)
				{
				    case ExecutableOpcodeType.Add:
				        foreach (var thread in activeExecutionContexts)
				            Execute(thread, instruction, InstructionImplementations.Add);
				        break;
                    case ExecutableOpcodeType.And:
                        foreach (var thread in activeExecutionContexts)
                            Execute(thread, instruction, InstructionImplementations.And);
                        break;
				    case ExecutableOpcodeType.Branch:
				        break;
				    case ExecutableOpcodeType.BranchC:
                        activeMasks = new List<BitArray>
                        {
                            new BitArray(_executionContexts.Length),
                            new BitArray(_executionContexts.Length)
                        };
				        foreach (var thread in activeExecutionContexts)
				        {
				            var src0 = GetOperandValue(thread, instruction.Operands[0]);
				            bool result = TestCondition(ref src0, instruction.TestBoolean);
				            activeMasks[0][thread.Index] = result;
				            activeMasks[1][thread.Index] = !result;
				        }
				        break;
				    case ExecutableOpcodeType.Cut:
				    case ExecutableOpcodeType.CutStream:
				        yield return ExecutionResponse.Cut;
				        break;
				    case ExecutableOpcodeType.Div:
				        foreach (var thread in activeExecutionContexts)
                            Execute(thread, instruction, InstructionImplementations.Div);
				        break;
				    case ExecutableOpcodeType.Dp2:
				        foreach (var thread in activeExecutionContexts)
                            ExecuteScalar(thread, instruction, InstructionImplementations.Dp2);
				        break;
				    case ExecutableOpcodeType.Dp3:
				        foreach (var thread in activeExecutionContexts)
                            ExecuteScalar(thread, instruction, InstructionImplementations.Dp3);
				        break;
				    case ExecutableOpcodeType.Dp4:
				        foreach (var thread in activeExecutionContexts)
                            ExecuteScalar(thread, instruction, InstructionImplementations.Dp4);
				        break;
				    case ExecutableOpcodeType.Emit:
				    case ExecutableOpcodeType.EmitStream:
				        yield return ExecutionResponse.Emit;
				        break;
                    case ExecutableOpcodeType.Ge:
                        foreach (var thread in activeExecutionContexts)
                            Execute(thread, instruction, InstructionImplementations.Ge);
                        break;
				    case ExecutableOpcodeType.ILt:
				        foreach (var thread in activeExecutionContexts)
				            Execute(thread, instruction, InstructionImplementations.Ilt);
				        break;
				    case ExecutableOpcodeType.ItoF:
				        foreach (var thread in activeExecutionContexts)
				            Execute(thread, instruction, src => Number.FromFloat(Convert.ToSingle(src.Int), instruction.Saturate));
				        break;
				    case ExecutableOpcodeType.FtoI:
				        foreach (var thread in activeExecutionContexts)
				            Execute(thread, instruction, src => Number.FromInt(Convert.ToInt32(src.Float)));
				        break;
				    case ExecutableOpcodeType.FtoU:
				        foreach (var thread in activeExecutionContexts)
				            Execute(thread, instruction, src => Number.FromUInt(Convert.ToUInt32(src.Float)));
				        break;
				    case ExecutableOpcodeType.IAdd:
				        foreach (var thread in activeExecutionContexts)
				            Execute(thread, instruction, (src0, src1) => Number.FromInt(src0.Int + src1.Int));
				        break;
				    case ExecutableOpcodeType.IGe:
				        foreach (var thread in activeExecutionContexts)
				            Execute(thread, instruction,
				                (src0, src1) => Number.FromUInt((src0.Int >= src1.Int) ? 0xFFFFFFFF : 0x0000000));
				        break;
				    case ExecutableOpcodeType.Lt:
				        foreach (var thread in activeExecutionContexts)
				            Execute(thread, instruction, InstructionImplementations.Lt);
				        break;
				    case ExecutableOpcodeType.Mad:
				        foreach (var thread in activeExecutionContexts)
				            Execute(thread, instruction,
				                (src0, src1, src2) =>
				                    Number.FromFloat((src0.Float * src1.Float) + src2.Float, instruction.Saturate));
				        break;
				    case ExecutableOpcodeType.Max:
				        foreach (var thread in activeExecutionContexts)
				            Execute(thread, instruction, InstructionImplementations.Max);
				        break;
                    case ExecutableOpcodeType.Min:
                        foreach (var thread in activeExecutionContexts)
                            Execute(thread, instruction, InstructionImplementations.Min);
                        break;
				    case ExecutableOpcodeType.Mov:
				        foreach (var thread in activeExecutionContexts)
				            Execute(thread, instruction, InstructionImplementations.Mov);
				        break;
				    case ExecutableOpcodeType.MovC:
				        foreach (var thread in activeExecutionContexts)
				        {
				            // If src0, then dest = src1 else dest = src2
				            var src0 = GetOperandValue(thread, instruction.Operands[1]);
				            bool result = TestCondition(ref src0, instruction.TestBoolean);
				            SetRegisterValue(thread, instruction.Operands[0], result
				                ? GetOperandValue(thread, instruction.Operands[2])
				                : GetOperandValue(thread, instruction.Operands[3]));
				        }
				        break;
				    case ExecutableOpcodeType.Mul:
				        foreach (var thread in activeExecutionContexts)
				            Execute(thread, instruction, InstructionImplementations.Mul);
				        break;
				    case ExecutableOpcodeType.Ret:
				        yield return ExecutionResponse.Finished;
				        break;
				    case ExecutableOpcodeType.Rsq:
				        foreach (var thread in activeExecutionContexts)
				            Execute(thread, instruction, InstructionImplementations.Rsq);
				        break;
				    case ExecutableOpcodeType.RtxCoarse:
				        for (var i = 0; i < _executionContexts.Length; i += 4)
				        {
				            var topLeft = GetOperandValue(_executionContexts[i + 0], instruction.Operands[1]);
				            var topRight = GetOperandValue(_executionContexts[i + 1], instruction.Operands[1]);

				            var deltaX = Number4.Subtract(ref topRight, ref topLeft);

				            for (var j = i; j < i + 4; j++)
				                SetRegisterValue(_executionContexts[j], instruction.Operands[0], deltaX);
				        }
				        break;
				    case ExecutableOpcodeType.RtxFine:
                        for (var i = 0; i < _executionContexts.Length; i += 4)
                        {
                            var topLeft = GetOperandValue(_executionContexts[i + 0], instruction.Operands[1]);
                            var topRight = GetOperandValue(_executionContexts[i + 1], instruction.Operands[1]);
                            var bottomLeft = GetOperandValue(_executionContexts[i + 2], instruction.Operands[1]);
                            var bottomRight = GetOperandValue(_executionContexts[i + 3], instruction.Operands[1]);

                            var topDeltaX = Number4.Subtract(ref topRight, ref topLeft);
                            var bottomDeltaX = Number4.Subtract(ref bottomRight, ref bottomLeft);

                            SetRegisterValue(_executionContexts[i + 0], instruction.Operands[0], topDeltaX);
                            SetRegisterValue(_executionContexts[i + 1], instruction.Operands[0], topDeltaX);

                            SetRegisterValue(_executionContexts[i + 2], instruction.Operands[0], bottomDeltaX);
                            SetRegisterValue(_executionContexts[i + 3], instruction.Operands[0], bottomDeltaX);
                        }
				        break;
                    case ExecutableOpcodeType.RtyCoarse:
                        for (var i = 0; i < _executionContexts.Length; i += 4)
                        {
                            var topLeft = GetOperandValue(_executionContexts[i + 0], instruction.Operands[1]);
                            var bottomLeft = GetOperandValue(_executionContexts[i + 2], instruction.Operands[1]);

                            var deltaY = Number4.Subtract(ref bottomLeft, ref topLeft);

                            for (var j = i; j < i + 4; j++)
                                SetRegisterValue(_executionContexts[j], instruction.Operands[0], deltaY);
                        }
                        break;
                    case ExecutableOpcodeType.RtyFine:
                        for (var i = 0; i < _executionContexts.Length; i += 4)
                        {
                            var topLeft = GetOperandValue(_executionContexts[i + 0], instruction.Operands[1]);
                            var topRight = GetOperandValue(_executionContexts[i + 1], instruction.Operands[1]);
                            var bottomLeft = GetOperandValue(_executionContexts[i + 2], instruction.Operands[1]);
                            var bottomRight = GetOperandValue(_executionContexts[i + 3], instruction.Operands[1]);

                            var leftDeltaY = Number4.Subtract(ref bottomLeft, ref topLeft);
                            var rightDeltaY = Number4.Subtract(ref bottomRight, ref topRight);

                            SetRegisterValue(_executionContexts[i + 0], instruction.Operands[0], leftDeltaY);
                            SetRegisterValue(_executionContexts[i + 1], instruction.Operands[0], rightDeltaY);

                            SetRegisterValue(_executionContexts[i + 2], instruction.Operands[0], leftDeltaY);
                            SetRegisterValue(_executionContexts[i + 3], instruction.Operands[0], rightDeltaY);
                        }
                        break;
                    case ExecutableOpcodeType.Sample:
				    {
				        var srcResource = GetTexture(instruction.Operands[2]);
				        var srcSampler = GetSampler(instruction.Operands[3]);
				        for (var i = 0; i < _executionContexts.Length; i += 4)
				        {
                            var topLeft = GetOperandValue(_executionContexts[i + 0], instruction.Operands[1]);
                            var topRight = GetOperandValue(_executionContexts[i + 1], instruction.Operands[1]);
                            var bottomLeft = GetOperandValue(_executionContexts[i + 2], instruction.Operands[1]);
                            var bottomRight = GetOperandValue(_executionContexts[i + 3], instruction.Operands[1]);

                            var deltaX = Number4.Subtract(ref topRight, ref topLeft);
                            var deltaY = Number4.Subtract(ref bottomLeft, ref topLeft);

				            SetRegisterValue(_executionContexts[i + 0], instruction.Operands[0],
				                srcResource.SampleGrad(srcSampler, ref topLeft,
				                    ref deltaX, ref deltaY));
                            SetRegisterValue(_executionContexts[i + 1], instruction.Operands[0],
                                srcResource.SampleGrad(srcSampler, ref topRight,
                                    ref deltaX, ref deltaY));
                            SetRegisterValue(_executionContexts[i + 2], instruction.Operands[0],
                                srcResource.SampleGrad(srcSampler, ref bottomLeft,
                                    ref deltaX, ref deltaY));
                            SetRegisterValue(_executionContexts[i + 3], instruction.Operands[0],
                                srcResource.SampleGrad(srcSampler, ref bottomRight,
                                    ref deltaX, ref deltaY));
				        }
				        break;
				    }
				    case ExecutableOpcodeType.Sqrt:
				        foreach (var thread in activeExecutionContexts)
                            Execute(thread, instruction, InstructionImplementations.Sqrt);
				        break;
                    case ExecutableOpcodeType.Utof:
				        foreach (var thread in activeExecutionContexts)
				            Execute(thread, instruction, InstructionImplementations.Utof);
				        break;
				    case ExecutableOpcodeType.Xor:
				        foreach (var thread in activeExecutionContexts)
				            Execute(thread, instruction, InstructionImplementations.Xor);
				        break;
				    default:
				        throw new InvalidOperationException(instruction.OpcodeType + " is not yet supported.");
				}

			    // Algorithm from "Dynamic Warp Formation: Exploiting Thread Scheduling for Efficient MIMD Control Flow
				// on SIMD Graphics Hardware" by Wilson Wai Lun Fung -
				// https://circle.ubc.ca/bitstream/handle/2429/2268/ubc_2008_fall_fung_wilson_wai_lun.pdf?sequence=1
				// 
				// 3 possible cases:
				// - No Divergence (single next PC)
				//     => Update the next PC ﬁeld of the top of stack (TOS) entry to
				//        the next PC of all active threads in this warp.
				// - Divergence (multiple next PC)
				//     => Modify the next PC ﬁeld of the TOS entry to the reconvergence point. 
				//        For each unique next PC of the warp, push a
				//        new entry onto the stack with next PC ﬁeld being the unique
				//        next PC and the reconv. PC being the reconvergence point.
				//        The active mask of each entry denotes the threads branching
				//        to the next PC value of this entry.
				// - Reconvergence (next PC = reconv. PC of TOS)
				//     => Pop TOS entry from the stack.
				if (instruction.UpdateDivergenceStack(warp.DivergenceStack, activeMasks))
                    activeExecutionContexts = GetActiveExecutionContexts(topOfDivergenceStack);
			}
		}

        private IList<ExecutionContext> GetActiveExecutionContexts(DivergenceStackEntry divergenceStackEntry)
		{
            return _executionContexts.Where(x => divergenceStackEntry.ActiveMask[x.Index]).ToList();
		}

		private static bool TestCondition(ref Number4 number, InstructionTestBoolean testBoolean)
		{
			switch (testBoolean)
			{
				case InstructionTestBoolean.Zero:
					return number.AllZero;
				case InstructionTestBoolean.NonZero:
					return number.AnyNonZero;
				default:
					throw new ArgumentOutOfRangeException("testBoolean");
			}
		}

		private static void GetRegister(ExecutionContext context, Operand operand, out Number4[] register, out int index)
		{
			var registerIndex = GetRegisterIndex(context, operand);
			context.GetRegister(operand.OperandType, registerIndex, out register, out index);
		}

		/// <summary>
		/// Gets potentially-swizzled value for use on RHS of an operation.
		/// </summary>
		private static Number4 GetOperandValue(ExecutionContext context, Operand operand)
		{
			switch (operand.OperandType)
			{
				case OperandType.Immediate32:
				case OperandType.Immediate64:
					return OperandUtility.ApplyOperandModifier(operand.ImmediateValues, operand.Modifier);
				case OperandType.ConstantBuffer:
				case OperandType.IndexableTemp:
				case OperandType.Input:
				case OperandType.Temp:
					Number4[] register;
					int index;
					GetRegister(context, operand, out register, out index);
					var swizzledNumber = OperandUtility.ApplyOperandSelectionMode(register[index], operand);
					return OperandUtility.ApplyOperandModifier(swizzledNumber, operand.Modifier);
				default:
					throw new ArgumentException("Unsupported operand type: " + operand.OperandType);
			}
		}

		private ITexture GetTexture(Operand operand)
		{
			switch (operand.OperandType)
			{
				case OperandType.Resource:
					return _virtualMachine.Textures[operand.Indices[0].Value];
				default:
					throw new ArgumentException("Unsupported operand type: " + operand.OperandType);
			}
		}

		private ISamplerState GetSampler(Operand operand)
		{
			switch (operand.OperandType)
			{
				case OperandType.Sampler:
                    return _virtualMachine.Samplers[operand.Indices[0].Value];
				default:
					throw new ArgumentException("Unsupported operand type: " + operand.OperandType);
			}
		}

		private static RegisterIndex GetRegisterIndex(ExecutionContext context, Operand operand)
		{
			var result = new RegisterIndex();
			switch (operand.IndexDimension)
			{
				case OperandIndexDimension._1D:
					result.Index1D = EvaluateOperandIndex(context, operand.Indices[0]);
					break;
				case OperandIndexDimension._2D:
					result.Index2D_0 = EvaluateOperandIndex(context, operand.Indices[0]);
					result.Index2D_1 = EvaluateOperandIndex(context, operand.Indices[1]);
					break;
			}
			return result;
		}

		private static ushort EvaluateOperandIndex(ExecutionContext context, OperandIndex index)
		{
			var result = (ushort)index.Value;
			switch (index.Representation)
			{
				case OperandIndexRepresentation.Immediate32PlusRelative:
				case OperandIndexRepresentation.Immediate64PlusRelative:
				case OperandIndexRepresentation.Relative:
					var operandValue = GetOperandValue(context, index.Register);
					result += (ushort)operandValue.GetMaskedNumber(index.Register.ComponentMask).UInt;
					break;
			}
			return result;
		}

		private static void SetRegisterValue(ExecutionContext context, Operand operand, Number4 value)
		{
			Number4[] register;
			int index;
			GetRegister(context, operand, out register, out index);
			register[index].WriteMaskedValue(value, operand.ComponentMask);
		}

		private static void Execute(ExecutionContext context, ExecutableInstruction instruction, Func<Number, Number> callback)
		{
			var src = GetOperandValue(context, instruction.Operands[1]);

			SetRegisterValue(context, instruction.Operands[0], new Number4
			{
				Number0 = callback(src.Number0),
				Number1 = callback(src.Number1),
				Number2 = callback(src.Number2),
				Number3 = callback(src.Number3)
			});
		}

        private static void Execute(ExecutionContext context, ExecutableInstruction instruction, Func<Number, Number, Number> callback)
		{
			var src0 = GetOperandValue(context, instruction.Operands[1]);
			var src1 = GetOperandValue(context, instruction.Operands[2]);

			SetRegisterValue(context, instruction.Operands[0], new Number4
			{
				Number0 = callback(src0.Number0, src1.Number0),
				Number1 = callback(src0.Number1, src1.Number1),
				Number2 = callback(src0.Number2, src1.Number2),
				Number3 = callback(src0.Number3, src1.Number3)
			});
		}

		private static void Execute(ExecutionContext context, ExecutableInstruction instruction, Func<Number, Number, Number, Number> callback)
		{
			var src0 = GetOperandValue(context, instruction.Operands[1]);
			var src1 = GetOperandValue(context, instruction.Operands[2]);
			var src2 = GetOperandValue(context, instruction.Operands[3]);

			SetRegisterValue(context, instruction.Operands[0], new Number4
			{
				Number0 = callback(src0.Number0, src1.Number0, src2.Number0),
				Number1 = callback(src0.Number1, src1.Number1, src2.Number1),
				Number2 = callback(src0.Number2, src1.Number2, src2.Number2),
				Number3 = callback(src0.Number3, src1.Number3, src2.Number3)
			});
		}

        private static void ExecuteScalar(ExecutionContext context, ExecutableInstruction instruction, Number4Number4ToNumberCallback callback)
        {
            var src0 = GetOperandValue(context, instruction.Operands[1]);
            var src1 = GetOperandValue(context, instruction.Operands[2]);
            var result = callback(instruction.Saturate, ref src0, ref src1);

            SetRegisterValue(context, instruction.Operands[0], new Number4
            {
                Number0 = result,
                Number1 = result,
                Number2 = result,
                Number3 = result
            });
        }

	    private delegate Number Number4Number4ToNumberCallback(bool saturate, ref Number4 src0, ref Number4 src1);

        private static void Execute(ExecutionContext context, ExecutableInstruction instruction, BoolNumber4Number4ToNumber4Callback callback)
        {
            var src0 = GetOperandValue(context, instruction.Operands[1]);
            var src1 = GetOperandValue(context, instruction.Operands[2]);
            var result = callback(instruction.Saturate, ref src0, ref src1);

            SetRegisterValue(context, instruction.Operands[0], result);
        }

        private delegate Number4 BoolNumber4Number4ToNumber4Callback(bool saturate, ref Number4 src0, ref Number4 src1);

        private static void Execute(ExecutionContext context, ExecutableInstruction instruction, Number4Number4ToNumber4Callback callback)
        {
            var src0 = GetOperandValue(context, instruction.Operands[1]);
            var src1 = GetOperandValue(context, instruction.Operands[2]);
            var result = callback(ref src0, ref src1);

            SetRegisterValue(context, instruction.Operands[0], result);
        }

        private delegate Number4 Number4Number4ToNumber4Callback(ref Number4 src0, ref Number4 src1);

        private static void Execute(ExecutionContext context, ExecutableInstruction instruction, BoolNumber4ToNumber4Callback callback)
        {
            var src0 = GetOperandValue(context, instruction.Operands[1]);
            var result = callback(instruction.Saturate, ref src0);

            SetRegisterValue(context, instruction.Operands[0], result);
        }

        private delegate Number4 BoolNumber4ToNumber4Callback(bool saturate, ref Number4 src0);

        private static void Execute(ExecutionContext context, ExecutableInstruction instruction, Number4ToNumber4Callback callback)
        {
            var src0 = GetOperandValue(context, instruction.Operands[1]);
            var result = callback(ref src0);

            SetRegisterValue(context, instruction.Operands[0], result);
        }

        private delegate Number4 Number4ToNumber4Callback(ref Number4 src0);
	}
}