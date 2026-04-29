Shader "Tartaria/AetherVeinStone"
{
    Properties
    {
        _BaseColor ("Stone Base Color", Color) = (0.70, 0.66, 0.58, 1)
        _MainTex ("Stone Albedo", 2D) = "white" {}
        _VeinMask ("Vein Mask", 2D) = "gray" {}
        _DetailNoise ("Detail Noise", 2D) = "gray" {}

        _TriplanarScale ("Triplanar Scale", Range(0.01, 8)) = 1.2
        _VeinScale ("Vein Scale", Range(0.01, 8)) = 1.8

        _VeinColorCold ("Vein Color Cold", Color) = (0.25, 0.55, 0.95, 1)
        _VeinColorHot ("Vein Color Hot", Color) = (1.0, 0.86, 0.48, 1)
        _AetherIntensity ("Aether Intensity", Range(0, 6)) = 1.5
        _AetherPulseSpeed ("Aether Pulse Speed", Range(0, 8)) = 1.618
        _AetherShift ("Aether Shift", Range(0, 1)) = 0.0

        _Smoothness ("Smoothness", Range(0, 1)) = 0.36
        _Metallic ("Metallic", Range(0, 1)) = 0.04
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }

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
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float fogCoord : TEXCOORD2;
            };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_VeinMask); SAMPLER(sampler_VeinMask);
            TEXTURE2D(_DetailNoise); SAMPLER(sampler_DetailNoise);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _TriplanarScale;
                float _VeinScale;
                float4 _VeinColorCold;
                float4 _VeinColorHot;
                float _AetherIntensity;
                float _AetherPulseSpeed;
                float _AetherShift;
                float _Smoothness;
                float _Metallic;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings o;
                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs nrm = GetVertexNormalInputs(input.normalOS);
                o.positionCS = pos.positionCS;
                o.positionWS = pos.positionWS;
                o.normalWS = normalize(nrm.normalWS);
                o.fogCoord = ComputeFogFactor(pos.positionCS.z);
                return o;
            }

            float3 TriplanarSample(TEXTURE2D_PARAM(tex, samp), float3 wsPos, float3 wsNrm, float scale)
            {
                float3 blend = pow(abs(wsNrm), 4.0);
                blend /= max(blend.x + blend.y + blend.z, 0.0001);

                float2 uvX = wsPos.zy * scale;
                float2 uvY = wsPos.xz * scale;
                float2 uvZ = wsPos.xy * scale;

                float3 x = SAMPLE_TEXTURE2D(tex, samp, uvX).rgb;
                float3 y = SAMPLE_TEXTURE2D(tex, samp, uvY).rgb;
                float3 z = SAMPLE_TEXTURE2D(tex, samp, uvZ).rgb;
                return x * blend.x + y * blend.y + z * blend.z;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float3 n = normalize(i.normalWS);

                float3 baseTex = TriplanarSample(TEXTURE2D_ARGS(_MainTex, sampler_MainTex), i.positionWS, n, _TriplanarScale);
                float3 detail = TriplanarSample(TEXTURE2D_ARGS(_DetailNoise, sampler_DetailNoise), i.positionWS, n, _TriplanarScale * 2.0);
                float3 veinMask = TriplanarSample(TEXTURE2D_ARGS(_VeinMask, sampler_VeinMask), i.positionWS, n, _VeinScale);

                float vein = saturate((veinMask.r * 1.3) - 0.35);
                float pulse = sin((_Time.y * _AetherPulseSpeed) + dot(i.positionWS, float3(0.2, 0.6, 0.2))) * 0.5 + 0.5;
                float veinEnergy = vein * pulse * _AetherIntensity;

                float3 stone = baseTex * _BaseColor.rgb;
                stone *= lerp(0.82, 1.15, detail.r);

                float3 veinColor = lerp(_VeinColorCold.rgb, _VeinColorHot.rgb, saturate(_AetherShift));
                float3 albedo = lerp(stone, stone * 1.08, vein * 0.18);

                InputData inputData = (InputData)0;
                inputData.positionWS = i.positionWS;
                inputData.normalWS = n;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(i.positionWS);
                inputData.fogCoord = i.fogCoord;

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo;
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Smoothness;
                surfaceData.normalTS = half3(0, 0, 1);
                surfaceData.occlusion = 1.0;
                surfaceData.alpha = 1.0;
                surfaceData.emission = veinColor * veinEnergy;

                half4 c = UniversalFragmentPBR(inputData, surfaceData);
                c.rgb = MixFog(c.rgb, i.fogCoord);
                return c;
            }
            ENDHLSL
        }

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
