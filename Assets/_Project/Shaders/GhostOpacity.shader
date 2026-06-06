Shader "Tartaria/GhostOpacity"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.75, 0.85, 1.0, 0.35)
        _EmissionColor ("Emission Color", Color) = (0.3, 0.4, 0.6, 1.0)
        _EmissionStrength ("Emission Strength", Range(0, 3)) = 0.5
        _Opacity ("Opacity", Range(0, 1)) = 0.35
        _FresnelPower ("Fresnel Power", Range(0.1, 10)) = 2.0
        _FresnelColor ("Fresnel Color", Color) = (0.5, 0.7, 1.0, 1.0)
        _PulseSpeed ("Pulse Speed", Range(0, 5)) = 1.0
        _PulseAmplitude ("Pulse Amplitude", Range(0, 0.5)) = 0.1
        _MainTex ("Texture (optional)", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "GhostPass"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float fogFactor : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _EmissionColor;
                half _EmissionStrength;
                half _Opacity;
                half _FresnelPower;
                half4 _FresnelColor;
                half _PulseSpeed;
                half _PulseAmplitude;
                float4 _MainTex_ST;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            Varyings vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs vpi = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs vni = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vpi.positionCS;
                output.normalWS = vni.normalWS;
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(vpi.positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.fogFactor = ComputeFogFactor(vpi.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Sample texture
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                // Fresnel rim effect (brighter at edges -- ghostly outline)
                half3 normalWS = normalize(input.normalWS);
                half3 viewDir = normalize(input.viewDirWS);
                half fresnel = pow(1.0 - saturate(dot(normalWS, viewDir)), _FresnelPower);

                // Pulsing opacity
                half pulse = sin(_Time.y * _PulseSpeed) * _PulseAmplitude;
                half finalOpacity = saturate(_Opacity + pulse);

                // Combine
                half3 baseColor = _BaseColor.rgb * texColor.rgb;
                half3 emission = _EmissionColor.rgb * _EmissionStrength;
                half3 fresnelContrib = _FresnelColor.rgb * fresnel;

                half3 finalColor = baseColor + emission + fresnelContrib;

                // Apply fog
                finalColor = MixFog(finalColor, input.fogFactor);

                return half4(finalColor, finalOpacity + fresnel * 0.3);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
