Shader "Hidden/Tartaria/ColorblindCorrection"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        ZWrite Off Cull Off ZTest Always

        Pass
        {
            Name "ColorblindCorrection"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float4x4 _ColorMatrix;

            half4 Frag(Varyings input) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.texcoord);
                half3 corrected = mul((float3x3)_ColorMatrix, color.rgb);
                return half4(corrected, color.a);
            }
            ENDHLSL
        }
    }
}
