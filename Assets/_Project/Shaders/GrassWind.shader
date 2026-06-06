Shader "Tartaria/GrassWind"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.4, 0.7, 0.3, 1)
        _BaseMap ("Base Texture", 2D) = "white" {}
        _Smoothness ("Smoothness", Range(0, 1)) = 0.3
        _Metallic ("Metallic", Range(0, 1)) = 0.0
        _WindStrength ("Wind Strength", Range(0, 1)) = 0.15
        _WindSpeed ("Wind Speed", Float) = 1.4
        _WindFrequency ("Wind Frequency", Float) = 0.3
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 color : COLOR; // Vertex color: R channel = wind influence (1=top, 0=base)
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float4 vertexColor : TEXCOORD3;
                float fogFactor : TEXCOORD4;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BaseMap_ST;
                float _Smoothness;
                float _Metallic;
                float _WindStrength;
                float _WindSpeed;
                float _WindFrequency;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                // World space position
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);

                // Wind displacement (vertex color R channel = wind influence)
                float windInfluence = input.color.r;
                float wave = sin(_Time.y * _WindSpeed + positionWS.x * _WindFrequency + positionWS.z * _WindFrequency * 0.7);
                float windOffset = wave * _WindStrength * windInfluence;

                // Apply wind to X and Z
                positionWS.x += windOffset;
                positionWS.z += windOffset * 0.5;

                output.positionWS = positionWS;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.vertexColor = input.color;
                output.fogFactor = ComputeFogFactor(output.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Sample base texture
                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;

                // Tint with vertex color
                albedo.rgb *= input.vertexColor.rgb;

                // URP lighting
                InputData lightingInput = (InputData)0;
                lightingInput.positionWS = input.positionWS;
                lightingInput.normalWS = normalize(input.normalWS);
                lightingInput.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                lightingInput.shadowCoord = TransformWorldToShadowCoord(input.positionWS);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo.rgb;
                surfaceData.alpha = albedo.a;
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Smoothness;
                surfaceData.normalTS = float3(0, 0, 1);

                half4 color = UniversalFragmentPBR(lightingInput, surfaceData);

                // Apply fog
                color.rgb = MixFog(color.rgb, input.fogFactor);

                return color;
            }
            ENDHLSL
        }

        // ShadowCaster pass for shadows
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        // DepthOnly pass for depth texture
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
