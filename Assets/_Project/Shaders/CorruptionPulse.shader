Shader "Tartaria/CorruptionPulse"
{
    Properties
    {
        _BaseColor ("Corruption Color", Color) = (0.15, 0.05, 0.2, 0.7)
        _PulseColor ("Pulse Color", Color) = (0.6, 0.1, 0.8, 1)
        _EdgeColor ("Edge Color", Color) = (0.9, 0.2, 0.1, 1)
        _PulseSpeed ("Pulse Speed", Float) = 2.0
        _PulseWidth ("Pulse Width", Range(0.01, 0.5)) = 0.15
        _CorruptionIntensity ("Corruption Intensity", Range(0, 1)) = 1.0
        _NoiseTex ("Corruption Noise", 2D) = "white" {}
        _NoiseScale ("Noise Scale", Float) = 3.0
        _DistortionStrength ("Distortion Strength", Range(0, 0.3)) = 0.05
        _FresnelPower ("Fresnel Power", Range(0.5, 8)) = 3.0
        _DissonanceFrequency ("Dissonance Frequency Hz", Float) = 66.6
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "CorruptionPulse"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS   : TEXCOORD2;
                float3 viewDirWS  : TEXCOORD3;
                float  fogCoord   : TEXCOORD4;
            };

            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _PulseColor;
                float4 _EdgeColor;
                float  _PulseSpeed;
                float  _PulseWidth;
                float  _CorruptionIntensity;
                float4 _NoiseTex_ST;
                float  _NoiseScale;
                float  _DistortionStrength;
                float  _FresnelPower;
                float  _DissonanceFrequency;
            CBUFFER_END

            // Anti-golden ratio — deliberate dissonance
            static const float ANTI_PHI = 0.4142135; // sqrt(2) - 1

            Varyings vert(Attributes input)
            {
                Varyings output;

                // Vertex distortion — corruption warps geometry
                float3 pos = input.positionOS.xyz;
                float warp = sin(_Time.y * _PulseSpeed * ANTI_PHI + pos.y * 5.0)
                           * cos(_Time.y * _PulseSpeed * 0.7 + pos.x * 3.0);
                pos += input.normalOS * warp * _DistortionStrength * _CorruptionIntensity;

                VertexPositionInputs posInputs = GetVertexPositionInputs(pos);
                output.positionCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(posInputs.positionWS);
                output.uv = input.uv;
                output.fogCoord = ComputeFogFactor(posInputs.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Scrolling noise — corruption texture
                float2 noiseUV = input.positionWS.xz * _NoiseScale * 0.1;
                noiseUV += _Time.y * float2(0.1, -0.15) * _PulseSpeed;
                half noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV).r;

                // Second layer at different scale for detail
                float2 noiseUV2 = input.positionWS.xz * _NoiseScale * 0.27 + _Time.y * float2(-0.08, 0.12);
                half noise2 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV2).r;
                noise = noise * 0.6 + noise2 * 0.4;

                // Radial pulse from object center — dark energy rings
                float dist = length(input.positionWS.xz - float2(0, 0)); // Local space relative
                float pulse = frac(dist * 0.5 - _Time.y * _PulseSpeed);
                float ring = smoothstep(_PulseWidth, 0.0, abs(pulse - 0.5) * 2.0);

                // Dissonance flicker — deliberately non-harmonic
                float flicker = sin(_Time.y * _DissonanceFrequency * 0.01) *
                                sin(_Time.y * _DissonanceFrequency * 0.01 * ANTI_PHI);
                flicker = flicker * 0.3 + 0.7; // Keep mostly visible

                // Fresnel — corruption glows at edges
                float NdotV = saturate(dot(normalize(input.normalWS), normalize(input.viewDirWS)));
                float fresnel = pow(1.0 - NdotV, _FresnelPower);

                // Compose color
                half3 baseCol = _BaseColor.rgb * noise;
                half3 pulseCol = _PulseColor.rgb * ring * 2.0;
                half3 edgeCol = _EdgeColor.rgb * fresnel;

                half3 finalColor = baseCol + pulseCol + edgeCol;
                finalColor *= _CorruptionIntensity * flicker;

                // Alpha: noise-driven with pulse rings
                float alpha = _BaseColor.a * _CorruptionIntensity;
                alpha *= saturate(noise * 0.7 + ring * 0.5 + fresnel * 0.3);
                alpha *= flicker;

                half4 color = half4(finalColor, saturate(alpha));
                color.rgb = MixFog(color.rgb, input.fogCoord);

                return color;
            }
            ENDHLSL
        }
    }
}
