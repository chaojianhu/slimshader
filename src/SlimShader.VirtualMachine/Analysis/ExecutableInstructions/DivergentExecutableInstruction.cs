﻿using System.Collections;
using System.Collections.Generic;
using SlimShader.VirtualMachine.Execution;
using SlimShader.VirtualMachine.Util;

namespace SlimShader.VirtualMachine.Analysis.ExecutableInstructions
{
    public class DivergentExecutableInstruction : ExecutableInstruction
    {
        public List<int> NextPCs { get; set; }
        public int ReconvergencePC { get; set; }

        public override void UpdateDivergenceStack(DivergenceStack divergenceStack, IList<BitArray> activeMasks)
        {
            divergenceStack.Peek().NextPC = ReconvergencePC;
            for (int i = 0; i < NextPCs.Count; i++)
                if (activeMasks[i].Any())
                    divergenceStack.Push(NextPCs[i], activeMasks[i], ReconvergencePC);
        }
    }
}