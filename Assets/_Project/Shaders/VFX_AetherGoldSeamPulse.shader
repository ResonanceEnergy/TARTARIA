Shader "Tartaria/VFX/AetherGoldSeamPulse"
{
    // R293 — Aether-Gold seam pulse shader per R171 + Art Bible §2.
    // Used on hero building seam lines, leyline beams, restoration glow.
    // Pulses #FFD973 along worldspace lines with sin-based intensity.

    Properties
    {
        _BaseColor      ("Aether-Gold Color", Color) = (1, 0.85, 0.45, 1)
        _PulseSpeed     ("Pulse Speed (Hz)", Range(0.1, 5.0)) = 0.83  // ~432 Hz / 521 (visible)
        _PulseAmplitude ("Pulse Amplitude", Range(0.1, 4.0)) = 1.5
        _PulseMinimum   ("Pulse Minimum", Range(0.0, 1.0)) = 0.4
        _SeamMask       ("Seam Mask (R)", 2D) = "white" {}
        _SeamWidth      ("Seam Edge Width", Range(0.001, 0.1)) = 0.02
        _EmissionBoost  ("Emission HDR Boost", Range(0.0, 8.0)) = 3.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha One         // Additive — glows over base color
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 worldPos    : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float  _PulseSpeed;
                float  _PulseAmplitude;
                float  _PulseMinimum;
                float  _SeamWidth;
                float  _EmissionBoost;
            CBUFFER_END

            TEXTURE2D(_SeamMask); SAMPLER(sampler_SeamMask);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionHCS = pos.positionCS;
                OUT.worldPos = pos.positionWS;
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float seam = SAMPLE_TEXTURE2D(_SeamMask, sampler_SeamMask, IN.uv).r;
                // Pulse via sin, normalized to [_PulseMinimum..1]
                float pulse = _PulseMinimum + (1.0 - _PulseMinimum) *
                              (0.5 + 0.5 * sin(_Time.y * _PulseSpeed * 6.2832));
                pulse *= _PulseAmplitude;

                // Seam edge softness
                float edge = smoothstep(0.5 - _SeamWidth, 0.5 + _SeamWidth, seam);

                half4 col = _BaseColor;
                col.rgb *= pulse * _EmissionBoost;
                col.a = edge * pulse;
                return col;
            }
            ENDHLSL
        }
    }
}
