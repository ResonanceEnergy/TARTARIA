Shader "Tartaria/VFX/MudBubble"
{
    // R294 — Mud bubble dissolve shader per R171 mandate.
    // Used on MudPool surfaces + Mud Golem dissolution + 17th-hour corruption.
    // Generates animated bubble pattern with rim glow.

    Properties
    {
        _BaseColor      ("Mud Base Color", Color) = (0.35, 0.25, 0.18, 1)
        _RimColor       ("Bubble Rim Color", Color) = (0.65, 0.45, 0.28, 1)
        _BubbleSpeed    ("Bubble Animation Speed", Range(0.1, 5.0)) = 1.5
        _BubbleScale    ("Bubble Pattern Scale", Range(0.5, 10.0)) = 4.0
        _BubbleNoise    ("Bubble Noise Tex (R)", 2D) = "gray" {}
        _RimPower       ("Rim Falloff", Range(1.0, 8.0)) = 3.0
        _EmissionBoost  ("Rim Emission Boost", Range(0.0, 4.0)) = 1.2
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS    : TEXCOORD0;
                float3 viewDirWS   : TEXCOORD1;
                float2 uv          : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _RimColor;
                float  _BubbleSpeed;
                float  _BubbleScale;
                float  _RimPower;
                float  _EmissionBoost;
            CBUFFER_END

            TEXTURE2D(_BubbleNoise); SAMPLER(sampler_BubbleNoise);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs nrm = GetVertexNormalInputs(IN.normalOS);
                OUT.positionHCS = pos.positionCS;
                OUT.normalWS = nrm.normalWS;
                OUT.viewDirWS = GetWorldSpaceViewDir(pos.positionWS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uvAnim = IN.uv * _BubbleScale + float2(0, _Time.y * _BubbleSpeed * 0.1);
                float noise = SAMPLE_TEXTURE2D(_BubbleNoise, sampler_BubbleNoise, uvAnim).r;

                // Bubble animation — punctuated rises
                float bubble = step(0.7, frac(noise + _Time.y * _BubbleSpeed * 0.3));

                // Fresnel rim
                float3 N = normalize(IN.normalWS);
                float3 V = normalize(IN.viewDirWS);
                float rim = pow(1.0 - saturate(dot(N, V)), _RimPower);

                half3 col = lerp(_BaseColor.rgb, _RimColor.rgb, max(bubble, rim));
                col += _RimColor.rgb * rim * _EmissionBoost;
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
