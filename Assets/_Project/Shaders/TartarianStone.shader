Shader "Tartaria/TartarianStone"
{
    Properties
    {
        _BaseColor ("Stone Color", Color) = (0.72, 0.68, 0.58, 1)
        _MainTex ("Base Map", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0, 2)) = 1.0
        _Smoothness ("Smoothness", Range(0, 1)) = 0.3
        _Metallic ("Metallic", Range(0, 1)) = 0.0
        _GoldenTint ("Golden Accent Color", Color) = (0.9, 0.8, 0.3, 1)
        _GoldenStrength ("Golden Strength", Range(0, 1)) = 0.0
        _EmissionColor ("Emission Color", Color) = (0.9, 0.75, 0.3, 1)
        _EmissionStrength ("Emission Strength", Range(0, 3)) = 0.0
        _AetherPulse ("Aether Pulse Speed", Range(0, 5)) = 1.618
        _RestorationProgress ("Restoration Progress", Range(0, 1)) = 1.0
        _DetailNoise ("Detail Noise", 2D) = "gray" {}
        _DetailScale ("Detail Scale", Float) = 10.0
        _WeatheringAmount ("Weathering", Range(0, 1)) = 0.3
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "TartarianStone"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 positionWS  : TEXCOORD1;
                float3 normalWS    : TEXCOORD2;
                float3 tangentWS   : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
                float  fogCoord    : TEXCOORD5;
            };

            TEXTURE2D(_MainTex);      SAMPLER(sampler_MainTex);
            TEXTURE2D(_NormalMap);     SAMPLER(sampler_NormalMap);
            TEXTURE2D(_DetailNoise);   SAMPLER(sampler_DetailNoise);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _MainTex_ST;
                float4 _NormalMap_ST;
                float  _NormalStrength;
                float  _Smoothness;
                float  _Metallic;
                float4 _GoldenTint;
                float  _GoldenStrength;
                float4 _EmissionColor;
                float  _EmissionStrength;
                float  _AetherPulse;
                float  _RestorationProgress;
                float4 _DetailNoise_ST;
                float  _DetailScale;
                float  _WeatheringAmount;
            CBUFFER_END

            static const float PHI = 1.6180339887;

            // Simple value noise for weathering variation
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS  = posInputs.positionCS;
                output.positionWS  = posInputs.positionWS;
                output.normalWS    = normInputs.normalWS;
                output.tangentWS   = normInputs.tangentWS;
                output.bitangentWS = normInputs.bitangentWS;
                output.uv          = TRANSFORM_TEX(input.uv, _MainTex);
                output.fogCoord    = ComputeFogFactor(posInputs.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Base texture sample
                half4 baseTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half3 baseColor = baseTex.rgb * _BaseColor.rgb;

                // Normal mapping
                half3 normalTS = UnpackNormalScale(
                    SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv),
                    _NormalStrength);
                float3x3 TBN = float3x3(
                    normalize(input.tangentWS),
                    normalize(input.bitangentWS),
                    normalize(input.normalWS));
                float3 normalWS = normalize(mul(normalTS, TBN));

                // Weathering — darkens crevices based on noise + normal direction
                float2 detailUV = input.positionWS.xz * _DetailScale * 0.01;
                half detail = SAMPLE_TEXTURE2D(_DetailNoise, sampler_DetailNoise, detailUV).r;
                float upFacing = saturate(dot(normalWS, float3(0, 1, 0)));
                float weathering = lerp(1.0, detail, _WeatheringAmount * (1.0 - upFacing * 0.5));
                baseColor *= weathering;

                // Golden accent — intensifies with restoration progress
                float goldenMask = saturate(_GoldenStrength * _RestorationProgress);
                // Phi-based shimmer
                float shimmer = sin(_Time.y * _AetherPulse * PHI + input.positionWS.y * 3.0) * 0.5 + 0.5;
                goldenMask *= lerp(0.8, 1.0, shimmer);
                baseColor = lerp(baseColor, _GoldenTint.rgb * baseTex.rgb, goldenMask * 0.4);

                // Lighting
                InputData lightInputData = (InputData)0;
                lightInputData.positionWS = input.positionWS;
                lightInputData.normalWS = normalWS;
                lightInputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                lightInputData.fogCoord = input.fogCoord;

                SurfaceData surfData = (SurfaceData)0;
                surfData.albedo = baseColor;
                surfData.metallic = _Metallic;
                surfData.smoothness = _Smoothness * lerp(0.6, 1.0, _RestorationProgress);
                surfData.normalTS = normalTS;
                surfData.occlusion = weathering;
                surfData.alpha = 1.0;

                // Emission — Aether veins glow when restored
                float3 emission = _EmissionColor.rgb * _EmissionStrength * _RestorationProgress;
                emission *= shimmer;
                surfData.emission = emission;

                half4 color = UniversalFragmentPBR(lightInputData, surfData);
                color.rgb = MixFog(color.rgb, input.fogCoord);

                return color;
            }
            ENDHLSL
        }

        // Shadow caster pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        // Depth pass
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On ColorMask R

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }
}
