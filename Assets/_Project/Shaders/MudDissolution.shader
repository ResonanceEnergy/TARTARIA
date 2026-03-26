Shader "Tartaria/MudDissolution"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.35, 0.25, 0.15, 1)
        _MainTex ("Base Map", 2D) = "white" {}
        _NoiseTex ("Dissolution Noise", 2D) = "white" {}
        _DissolutionProgress ("Dissolution Progress", Range(0, 1)) = 0
        _EdgeWidth ("Edge Glow Width", Range(0, 0.1)) = 0.03
        _EdgeColor ("Edge Glow Color", Color) = (0.9, 0.75, 0.3, 1)
        _RumbleIntensity ("Surface Rumble", Range(0, 0.5)) = 0.1
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
            Name "MudDissolution"
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
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS   : TEXCOORD2;
                float  fogCoord   : TEXCOORD3;
            };

            TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseTex);   SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _MainTex_ST;
                float4 _NoiseTex_ST;
                float  _DissolutionProgress;
                float  _EdgeWidth;
                float4 _EdgeColor;
                float  _RumbleIntensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                // Vertex displacement — mud surface rumble during dissolution
                float3 pos = input.positionOS.xyz;
                float rumble = sin(_Time.y * 8.0 + pos.x * 3.0 + pos.z * 5.0)
                             * _RumbleIntensity * _DissolutionProgress;
                pos += input.normalOS * rumble;

                VertexPositionInputs posInputs = GetVertexPositionInputs(pos);
                output.positionCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.fogCoord = ComputeFogFactor(posInputs.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Sample noise for dissolution mask
                float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex,
                    input.uv * _NoiseTex_ST.xy + _NoiseTex_ST.zw).r;

                // Discard dissolved pixels
                float dissolveThreshold = _DissolutionProgress;
                clip(noise - dissolveThreshold);

                // Base color
                half4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 color = baseMap * _BaseColor;

                // Golden edge glow at dissolution boundary
                float edge = smoothstep(dissolveThreshold, dissolveThreshold + _EdgeWidth, noise);
                color = lerp(_EdgeColor, color, edge);

                // Basic lighting
                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(input.normalWS, mainLight.direction));
                color.rgb *= mainLight.color * NdotL * 0.7 + 0.3; // Ambient floor

                // Fog
                color.rgb = MixFog(color.rgb, input.fogCoord);

                return color;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
