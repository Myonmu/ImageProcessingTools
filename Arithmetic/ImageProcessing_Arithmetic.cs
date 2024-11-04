using System;
using UnityEngine;

namespace ImageProcessingTools
{
    public static partial class ImageProcessing
    {
        private static ComputeShader _arithmeticCS;

        private static readonly int Background = Shader.PropertyToID("background");
        private static readonly int Foreground = Shader.PropertyToID("foreground");
        private static readonly int TextureDimensions = Shader.PropertyToID("_TextureDimensions");
        private static readonly int Offset = Shader.PropertyToID("_Offset");
        private static readonly int ChannelMask = Shader.PropertyToID("_ChannelMask");
        private static readonly int SkipConditions = Shader.PropertyToID("_SkipConditions");
        private static readonly int ForegroundLinearTransform = Shader.PropertyToID("_ForegroundLinearTransform");

        public static void RunArithmeticCS(ComputeShader cs, Texture background, Texture foreground, string kernelName,
            Vector2Int offset, int channelMask, int skipConditions, Vector2 linearTransform, Action<int> extraOp = null)
        {
            if (_arithmeticCS == null) _arithmeticCS = Resources.Load<ComputeShader>("IpArithmetic");
            var kernel = cs.FindKernel(kernelName);
            extraOp?.Invoke(kernel);
            cs.SetTexture(kernel, Background, background);
            cs.SetTexture(kernel, Foreground, foreground);
            cs.SetVector(TextureDimensions,
                new Vector4(background.width, background.height, foreground.width, foreground.height));
            cs.SetVector(Offset, new Vector4(offset.x, offset.y, 0, 0));
            cs.SetInt(ChannelMask, channelMask);
            cs.SetInt(SkipConditions, skipConditions);
            cs.SetVector(ForegroundLinearTransform, linearTransform);
            cs.GetKernelThreadGroupSizes(kernel, out var w, out var h, out var z);
            cs.Dispatch(kernel,
                Mathf.CeilToInt(background.width / (float)w),
                Mathf.CeilToInt(background.height / (float)h), (int)z);
        }

        public static void ExecuteArithmeticOperation(Texture background, Texture foreground, ArithmeticOp operation,
            Vector2Int offset = default, int channelMask = 15, int skipConditions = 0,
            Vector2 linearTransform = default)
        {
            RunArithmeticCS(_arithmeticCS, background, foreground, operation.ToString(), offset, channelMask, skipConditions,
                linearTransform);
        }

        public static void ExecuteArithmeticOperation(Texture background, Texture foreground, ArithmeticOp operation,
            Vector2Int offset = default,
            ChannelMask mask = ImageProcessingTools.ChannelMask.RGBA,
            SkipCondition skipConditions = SkipCondition.None, Vector2 linearTransform = default)
        {
            RunArithmeticCS(_arithmeticCS, background, foreground, operation.ToString(), offset, (int)mask, (int)skipConditions,
                linearTransform);
        }
    }
}