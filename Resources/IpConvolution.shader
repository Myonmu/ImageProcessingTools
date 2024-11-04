Shader "Custom/IpConvolution"
{
    Properties
    {
        [NoScaleOffset] _MainTex("MainTex", 2D) = "white" {}
    }
    SubShader
    {

        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"
        }
        Pass
        {
            Name "ImageProcessing"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #pragma multi_compile  _ OP_CONVOLVE OP_DILATE SUM_METHOD_EINSTEIN
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            SAMPLER(sampler_point_clamp);
            float4 _MainTex_TexelSize;


            // Supports 9x9 kernels at most.
            float _Kernel[81];

            /*    1  2  3   \
             *    4  5  6   width (number of rows or columns) 
             *    7  8  9   /
             *       \---|
             *        extend (number of separation lines bettwen the center column and the outer-most column)
             */

            int _KernelExtend = 1; // extend of the kernel (kernel width // 2)
            int _KernelWidth = 3;
            int _KernelSize = 9; // cells in the kernel ( NOT equal to _KernelWidth^2 due to Einstein summation method)
            int _KernelOffset = 1; // width of the separation lines


            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                #if SHADER_API_GLES
                float4 pos = input.positionOS;
                float2 uv  = input.uv;
                #else
                float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
                float2 uv = GetFullScreenTriangleTexCoord(input.vertexID);
                #endif

                output.positionCS = pos;
                output.texcoord = uv;
                return output;
            }

            float2 get_offset_coords(float2 center, const int i, const int j, const int offset)
            {
                return float2(
                    center.x + (i - _KernelExtend) * _MainTex_TexelSize.x * offset,
                    center.y + (j - _KernelExtend) * _MainTex_TexelSize.y * offset
                );
            }

            /* Einstein's summation convention, mainly used in Gaussian kernels
             * When using Einstein's summation convention, kernel is reduced to one row.
             * For example, a 3x3 Gaussian blur kernel becomes 3 floats.
             * We express convolution as in Einstein's summation convention:
             * 
             *       S = Km * Kn * Sample(Center, Dmn)
             *            |   |                    |___ Offset at m, n
             *     kernel values at m and n
             *     
             * Where the right hand side is summed for all possible combinations of m and n.
             *  m, n are possible row/column indices of the kernel.
             */
            #if SUM_METHOD_EINSTEIN
            float4 convolve(const float2 center, const int offset)
            {
                float4 sum = 0;
                for (int i = 0; i < _KernelSize; i++)
                {
                    for (int j = 0; j < _KernelSize; j++)
                    {
                        const float kernel_val = _Kernel[i] * _Kernel[j];
                        sum += kernel_val * SAMPLE_TEXTURE2D(_MainTex, sampler_point_clamp, get_offset_coords(center, i, j, offset));
                    }
                }
                return sum;
            }
            #else

            float4 convolve(const float2 center, const int offset)
            {
                float4 sum = 0;
                for (int i = 0; i < _KernelWidth; i++)
                {
                    for (int j = 0; j < _KernelWidth; j++)
                    {
                        const float kernel_val = _Kernel[i * _KernelWidth + j];
                        sum += kernel_val * SAMPLE_TEXTURE2D(_MainTex, sampler_point_clamp,
                                                             get_offset_coords(center, i, j, offset));
                    }
                }
                return sum;
            }
            #endif


            float4 dilate(const float2 center, const int offset)
            {
                float4 result = float4(0, 0, 0, 0);
                float4 cnt = float4(0, 0, 0, 0);
                for (int i = 0; i < _KernelWidth; i++)
                {
                    for (int j = 0; j < _KernelWidth; j++)
                    {
                        const float kernel_val = _Kernel[i * _KernelWidth + j];
                        if (kernel_val > 0)
                        {
                            float4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_point_clamp,
                                                        get_offset_coords(center, i, j, offset));
                            for (int k = 0; k < 4; k++)
                            {
                                if (c[k] > 0)
                                {
                                    result[k] += c[k];
                                    cnt[k]+=1;
                                }
                            }
                        }
                    }
                }

                for (int k = 0; k < 4; k++)
                {
                    if (cnt[k] > 0)
                    {
                        result[k] /= cnt[k];
                    }
                }
                return result;
            }

            float4 erode(const float2 center, const int offset)
            {
                float4 center_color = SAMPLE_TEXTURE2D(_MainTex, sampler_point_clamp,
                          get_offset_coords(center, 0, 0, offset));
                for (int i = 0; i < _KernelWidth; i++)
                {
                    for (int j = 0; j < _KernelWidth; j++)
                    {
                        const float kernel_val = _Kernel[i * _KernelWidth + j];
                        if (kernel_val > 0)
                        {
                            float4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_point_clamp,
                                       get_offset_coords(center, i, j, offset));
                            for (int k = 0; k < 4; k++)
                            {
                                center_color[k] *= c[k] > 0 ? 1 : 0;
                            }
                        }
                    }
                }
                return center_color;
            }

            float4 frag(Varyings input) : SV_Target
            {
                #if OP_CONVOLVE
                    return convolve(input.texcoord, _KernelOffset);
                #elif OP_DILATE
                    return dilate(input.texcoord, _KernelOffset);
                #else
                return erode(input.texcoord, _KernelOffset);
                #endif
            }
            ENDHLSL
        }
    }
}