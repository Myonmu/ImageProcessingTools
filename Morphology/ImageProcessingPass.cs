using System;
using UnityEngine;

namespace ImageProcessingTools
{
    public struct ImageProcessingPass
    {
        public Kernel kernel;
        public KernelOperation operation;
        public int repeatTimes;

        public ImageProcessingPass(Kernel kernel, KernelOperation operation, int repeat = 1)
        {
            this.kernel = kernel;
            this.operation = operation;
            repeatTimes = repeat;
        }

        public readonly void Setup(Material material, int kernelOffset)
        {
            kernel.SetupMaterial(material, kernelOffset);
            switch (operation)
            {
                case KernelOperation.Convolve:
                    material.EnableKeyword("OP_CONVOLVE");
                    break;
                case KernelOperation.Dilate:
                    material.EnableKeyword("OP_DILATE");
                    break;
                case KernelOperation.Erode:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public readonly int Setup(ComputeShader cs, int kernelOffset)
        {
            kernel.SetupComputeShader(cs, kernelOffset);
            return cs.FindKernel(operation.ToString());
        }
    }
}