using UnityEngine;

namespace ImageProcessingTools
{
    public struct Kernel
    {
        public Kernel(float[] kernelValues, KernelCreationFlags creationFlags, int kernelWidth)
        {
            _kernel = kernelValues;
            KernelWidth = kernelWidth;
            CreationFlags = creationFlags;
        }

        private readonly float[] _kernel;

        public KernelCreationFlags CreationFlags { get; }
        public int KernelWidth { get; }
        public readonly int KernelSize => _kernel.Length;
        public readonly int KernelExtend => KernelWidth / 2;


        private static readonly int K = Shader.PropertyToID("_Kernel");
        private static readonly int Size = Shader.PropertyToID("_KernelSize");
        private static readonly int Width = Shader.PropertyToID("_KernelWidth");
        private static readonly int Extend = Shader.PropertyToID("_KernelExtend");
        private static readonly int KernelOffset = Shader.PropertyToID("_KernelOffset");
        private static readonly int SummationMethod = Shader.PropertyToID("_SummationMethod");

        public readonly void SetupMaterial(Material convolutionMat, int kernelOffset)
        {
            var enabled = convolutionMat.enabledKeywords;
            foreach (var keyword in enabled)
            {
                convolutionMat.DisableKeyword(keyword);
            }

            convolutionMat.SetFloatArray(K, _kernel);
            convolutionMat.SetInteger(Size, KernelSize);
            convolutionMat.SetInteger(Width, KernelWidth);
            convolutionMat.SetInteger(Extend, KernelExtend);
            convolutionMat.SetInteger(KernelOffset, kernelOffset);
            if ((CreationFlags & KernelCreationFlags.EinsteinSummation) > 0)
            {
                convolutionMat.EnableKeyword("SUM_METHOD_EINSTEIN");
            }
        }

        public readonly void SetupComputeShader(ComputeShader cs, int kernelOffset)
        {
            cs.SetFloats(K, _kernel);
            cs.SetInt(Size, KernelSize);
            cs.SetInt(Width, KernelWidth);
            cs.SetInt(Extend, KernelExtend);
            cs.SetInt(KernelOffset, kernelOffset);
            cs.SetInt(SummationMethod, (CreationFlags & KernelCreationFlags.EinsteinSummation) > 0 ? 1 : 0);
        }
    }
}