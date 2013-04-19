﻿using System.Collections.Generic;
using System.Linq;
using SlimShader.VirtualMachine.Analysis.ExplicitBranching;

namespace SlimShader.VirtualMachine.Analysis
{
	public class ControlFlowGraph
	{
		public static ControlFlowGraph FromInstructions(IEnumerable<InstructionBase> instructions)
		{
			var result = new ControlFlowGraph();

			// The following algorithm is from "Analyzing Control Flow in Java Bytecode" by Jianjun Zhao.
			// Determine the basic blocks by finding the set of leaders:
			// - The first instruction is a leader.
			// - Each instruction that is the target of an unconditional branch is a leader.
			// - Each instruction that is the target of a conditional branch is a leader.
			// - Each instruction that immediately follows a conditional or unconditional branch is a leader.
			// - TODO: switch statements.
			// Each leader gives rise to a basic block consisting of all instructions up to the next leader
			// or the end of the bytecode.

			var instructionsList = instructions.ToList();
			var blockInstructions = new List<InstructionBase>();
			int position = 0;
			for (int i = 0; i < instructionsList.Count; i++)
			{
				if (IsLeader(instructionsList, i) && blockInstructions.Any())
				{
					result.BasicBlocks.Add(new BasicBlock(position++, blockInstructions));
					blockInstructions = new List<InstructionBase>();
				}
				blockInstructions.Add(instructionsList[i]);
			}
			result.BasicBlocks.Add(new BasicBlock(position, blockInstructions));

			// Now we can use the following rules to construct the CFG.
			// Given that u and v are basic blocks:
			// - Add an edge (u,v) if v follows u in the bytecode and u does not terminate in an unconditional branch.
			// - Add an edge (u,v) if the last instruction of u is a conditional or unconditional branch to the first
			//   instruction of v.
			// - TODO: switch statements.
			foreach (var u in result.BasicBlocks)
			{
				foreach (var v in result.BasicBlocks.Where(x => x != u))
				{
					if ((v.Position == u.Position + 1 && !u.Instructions.Last().IsUnconditionalBranch)
						|| u.Instructions.Last().BranchesTo(v.Instructions.First()))
						u.Successors.Add(v);
				}
			}

			return result;
		}

		private static bool IsLeader(IList<InstructionBase> instructions, int index)
		{
			if (index == 0)
				return true;

			var instruction = instructions[index];
			if (instructions.OfType<BranchingInstruction>().Any(x => x.BranchTarget == instruction))
				return true;

			if (index > 0 && instructions[index - 1] is BranchingInstruction)
				return true;

			return false;
		}

		public List<BasicBlock> BasicBlocks { get; private set; }

		private ControlFlowGraph()
		{
			BasicBlocks = new List<BasicBlock>();
		}
	}
}