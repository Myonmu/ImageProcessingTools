using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Object = UnityEngine.Object;

namespace ImageProcessingTools
{
    /// <summary>
    /// Provides Morphology operations applied to texture 2d and render texture.
    /// Designed to be used in the editor, runtime performance is not guaranteed.
    /// </summary>
    public static partial class ImageProcessing
    {
        private static Material _convolutionMaterial;
        private static ComputeShader _convolutionCs;
        private static readonly int Src = Shader.PropertyToID("src");
        private static readonly int Target = Shader.PropertyToID("target");

        public static ComputeShader ConvolutionCs
        {
            get
            {
                if (_convolutionCs == null)
                {
                    _convolutionCs = Resources.Load<ComputeShader>("IpConvolution");
                }

                return _convolutionCs;
            }
        }

        public static Material ConvolutionMaterial
        {
            get
            {
                if (_convolutionMaterial == null)
                {
                    _convolutionMaterial = CreateConvolutionMaterial();
                }

                return _convolutionMaterial;
            }
        }

        public static Material CreateConvolutionMaterial()
        {
            return new Material(Shader.Find("Custom/IpConvolution"));
        }

        public static Texture2D ProcessTexture2D(Texture2D source, in ImageProcessingPass pass, int kernelOffset = 1)
        {
            pass.Setup(ConvolutionMaterial, kernelOffset);
            var outputTex = Object.Instantiate(source);
            var desc = new RenderTextureDescriptor(source.width, source.height, source.graphicsFormat,
                GraphicsFormat.None);
            var rt = RenderTexture.GetTemporary(desc);
            Graphics.Blit(source, rt, ConvolutionMaterial, 0);
            var prevActive = RenderTexture.active;
            RenderTexture.active = rt;
            outputTex.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            RenderTexture.active = prevActive;
            outputTex.Apply();
            RenderTexture.ReleaseTemporary(rt);
            return outputTex;
        }

        /// <summary>
        /// Convolve a texture using the given kernel
        /// </summary>
        /// <param name="source">input texture</param>
        /// <param name="kernel">convolution kernel</param>
        /// <param name="operation"></param>
        /// <param name="kernelOffset">multiplier applied when calculating sampling offset</param>
        /// <param name="repeatTimes">how many times should the operation be repeated</param>
        /// <returns>a texture identical to source texture except its pixel values.</returns>
        public static Texture2D ProcessTexture2D(Texture2D source, Kernel kernel,
            KernelOperation operation = KernelOperation.Convolve, int kernelOffset = 1,
            int repeatTimes = 1)
        {
            var pass = new ImageProcessingPass(kernel, operation, repeatTimes);
            return ProcessTexture2D(source, pass, kernelOffset);
        }

        public static void ProcessTexture2DInPlace(Texture2D source, in ImageProcessingPass pass,
            int kernelOffset = 1)
        {
            pass.Setup(ConvolutionMaterial, kernelOffset);
            var desc = new RenderTextureDescriptor(source.width, source.height, source.graphicsFormat,
                GraphicsFormat.None);
            var rt = RenderTexture.GetTemporary(desc);
            Graphics.Blit(source, rt, ConvolutionMaterial, 0);
            var prevActive = RenderTexture.active;
            RenderTexture.active = rt;
            source.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            RenderTexture.active = prevActive;
            source.Apply();
        }

        /// <summary>
        /// Convolve a texture using the given kernel, write the result back to the source texture.
        /// </summary>
        /// <param name="source">input texture</param>
        /// <param name="kernel">convolution kernel</param>
        /// <param name="operation"></param>
        /// <param name="kernelOffset">multiplier applied when calculating sampling offset</param>
        /// <param name="repeatTimes">how many times should the operation be repeated</param>
        public static void ProcessTexture2DInPlace(Texture2D source, Kernel kernel,
            KernelOperation operation = KernelOperation.Convolve, int kernelOffset = 1, int repeatTimes = 1)
        {
            var pass = new ImageProcessingPass(kernel, operation, repeatTimes);
            ProcessTexture2DInPlace(source, pass, kernelOffset);
        }

        public static void ProcessRenderTexture(RenderTexture source, RenderTexture destination,
            ImageProcessingPass pass, int kernelOffset = 1)
        {
            pass.Setup(ConvolutionMaterial, kernelOffset);
            Graphics.Blit(source, destination, ConvolutionMaterial, 0);
        }

        public static void ProcessRenderTextureInPlace(RenderTexture source, ImageProcessingPass pass,
            int kernelOffset = 1)
        {
            pass.Setup(ConvolutionMaterial, kernelOffset);
            var desc = new RenderTextureDescriptor(source.width, source.height, source.graphicsFormat,
                GraphicsFormat.None);
            var rt = RenderTexture.GetTemporary(desc);
            Graphics.Blit(source, rt, ConvolutionMaterial, 0);
            Graphics.Blit(rt, source);
            RenderTexture.ReleaseTemporary(rt);
        }


        /// <summary>
        /// Convolve a source texture using the given kernel, output to destination texture
        /// </summary>
        /// <param name="source">source rt</param>
        /// <param name="destination">destination rt</param>
        /// <param name="kernel">convolution kernel</param>
        /// <param name="operation"></param>
        /// <param name="kernelOffset">multiplier applied when calculating sampling offset</param>
        /// <param name="repeatTimes"></param>
        public static void ProcessRenderTexture(RenderTexture source, RenderTexture destination,
            Kernel kernel, KernelOperation operation = KernelOperation.Convolve,
            int kernelOffset = 1, int repeatTimes = 1)
        {
            var pass = new ImageProcessingPass(kernel, operation, repeatTimes);
            ProcessRenderTexture(source, destination, pass, kernelOffset);
        }

        /// <summary>
        /// Convolve a source texture with multiple kernels in sequential order, write the
        /// result to the destination texture.
        /// </summary>
        /// <param name="source">source rt</param>
        /// <param name="destination">destination rt</param>
        /// <param name="passes">convolution kernel</param>
        /// <param name="offsetGetter">multiplier applied when calculating sampling offset</param>
        public static void ProcessRenderTextureMultiPass(RenderTexture source, RenderTexture destination,
            IEnumerable<ImageProcessingPass> passes, Func<int, int> offsetGetter = null)
        {
            var prevActive = RenderTexture.active;
            offsetGetter ??= _ => 1;
            var intermediateRt = new RenderTexture(source);
            Graphics.Blit(source, intermediateRt);
            var currentSrc = intermediateRt;
            var currentDest = destination;
            var counter = 0;
            foreach (var pass in passes)
            {
                for (int i = 0; i < pass.repeatTimes; i++)
                {
                    var offset = offsetGetter.Invoke(counter);
                    ProcessRenderTexture(currentSrc, currentDest, pass, offset);
                    (currentSrc, currentDest) = (currentDest, currentSrc);
                    counter++;
                }
            }

            if (currentDest != destination)
            {
                Graphics.Blit(currentDest, destination);
            }

            RenderTexture.active = prevActive;

            if (!Application.isPlaying)
                Object.DestroyImmediate(intermediateRt);
            else
                Object.Destroy(intermediateRt);
        }

        public static Texture2D ProcessTexture2DMultiPass(Texture2D source, IEnumerable<ImageProcessingPass> passes,
            Func<int, int> offsetGetter = null)
        {
            var desc = new RenderTextureDescriptor(source.width, source.height, source.graphicsFormat,
                GraphicsFormat.None);
            var rt = RenderTexture.GetTemporary(desc);
            Graphics.Blit(source, rt);
            ProcessRenderTextureMultiPass(rt, rt, passes, offsetGetter);
            var outputTex = Object.Instantiate(source);
            var prevActive = RenderTexture.active;
            RenderTexture.active = rt;
            outputTex.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            RenderTexture.active = prevActive;
            outputTex.Apply();
            RenderTexture.ReleaseTemporary(rt);
            return outputTex;
        }

        public static void ProcessTexture2DMultiPassInPlace(Texture2D source,
            IEnumerable<ImageProcessingPass> passes, Func<int, int> offsetGetter = null)
        {
            var desc = new RenderTextureDescriptor(source.width, source.height, source.graphicsFormat,
                GraphicsFormat.None);
            var rt = RenderTexture.GetTemporary(desc);
            Graphics.Blit(source, rt);
            ProcessRenderTextureMultiPass(rt, rt, passes, offsetGetter);
            var prevActive = RenderTexture.active;
            RenderTexture.active = rt;
            source.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            RenderTexture.active = prevActive;
            source.Apply();
            RenderTexture.ReleaseTemporary(rt);
        }

        public static void ProcessTextureCs(Texture source, Texture destination, ImageProcessingPass pass,
            int kernelOffset = 1)
        {
            var k = pass.Setup(ConvolutionCs, kernelOffset);
            ConvolutionCs.SetTexture(k, Src, source);
            ConvolutionCs.SetTexture(k, Target, destination);
            ConvolutionCs.SetVector(TextureDimensions,
                new Vector4(source.width, source.height, destination.width, destination.height));
            ConvolutionCs.GetKernelThreadGroupSizes(k, out var x, out var y, out var z);
            ConvolutionCs.Dispatch(k, Mathf.CeilToInt(source.width / (float)x),
                Mathf.CeilToInt(source.height / (float)y), (int)z);
        }

        public static void ProcessTextureCsInPlace(Texture source, ImageProcessingPass pass,
            int kernelOffset = 1)
        {
            var desc = new RenderTextureDescriptor(source.width, source.height, source.graphicsFormat,
                GraphicsFormat.None)
            {
                enableRandomWrite = true
            };
            var rt = RenderTexture.GetTemporary(desc);
            ProcessTextureCs(source, rt, pass, kernelOffset);
            Graphics.CopyTexture(rt, source);
            RenderTexture.ReleaseTemporary(rt);
        }

        public static void ProcessTextureCsMultiPass(Texture source, Texture destination,
            IEnumerable<ImageProcessingPass> passes, Func<int, int> offsetGetter = null)
        {
            var prevActive = RenderTexture.active;
            offsetGetter ??= _ => 1;
            var intermediateRt =
                new RenderTexture(source.width, source.height, source.graphicsFormat, GraphicsFormat.None)
                {
                    enableRandomWrite = true
                };
            intermediateRt.Create();
            Graphics.CopyTexture(source, intermediateRt);
            Texture currentSrc = intermediateRt;
            var currentDest = destination;
            var counter = 0;
            foreach (var pass in passes)
            {
                for (int i = 0; i < pass.repeatTimes; i++)
                {
                    var offset = offsetGetter.Invoke(counter);
                    ProcessTextureCs(currentSrc, currentDest, pass, offset);
                    (currentSrc, currentDest) = (currentDest, currentSrc);
                    counter++;
                }
            }

            if (currentDest != destination)
            {
                Graphics.CopyTexture(currentDest, destination);
            }

            RenderTexture.active = prevActive;

            if (!Application.isPlaying)
                Object.DestroyImmediate(intermediateRt);
            else
                Object.Destroy(intermediateRt);
            
        }

        /// <summary>
        /// Multi-pass morphology transformation that uses the source texture as intermediate
        /// texture.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="passes"></param>
        /// <param name="offsetGetter"></param>
        public static void ProcessTextureCsMultiPassNoAlloc(Texture source, Texture destination,
            IEnumerable<ImageProcessingPass> passes, Func<int, int> offsetGetter = null)
        {
            var prevActive = RenderTexture.active;
            offsetGetter ??= _ => 1;
            var currentSrc = source;
            var currentDest = destination;
            var counter = 0;
            foreach (var pass in passes)
            {
                for (int i = 0; i < pass.repeatTimes; i++)
                {
                    var offset = offsetGetter.Invoke(counter);
                    ProcessTextureCs(currentSrc, currentDest, pass, offset);
                    (currentSrc, currentDest) = (currentDest, currentSrc);
                    counter++;
                }
            }

            if (currentDest != destination)
            {
                Graphics.CopyTexture(currentDest, destination);
            }
            RenderTexture.active = prevActive;
        }
    }
}