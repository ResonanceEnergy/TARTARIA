Shader "Tartaria/AetherFlow"
{
    Properties
    {
        _BaseColor ("Aether Color", Color) = (0.2, 0.5, 0.9, 0.3)
        _FlowSpeed ("Flow Speed", Float) = 1.0
        _FlowScale ("Flow Scale", Float) = 2.0
        _Intensity ("Glow Intensity", Range(0, 5)) = 1.5
        _PulseSpeed ("Pulse Speed", Float) = 1.618
        _FresnelPower ("Fresnel Power", Float) = 3.0
        _NoiseTex ("Flow Noise", 2D) = "white" {}
        _Band ("Aether Band (3/6/9)", Float) = 6.0
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
            Name "AetherFlow"
            Tags { "LightMode" = "UniversalForward" }

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
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS   : TEXCOORD2;
                float3 viewDirWS  : TEXCOORD3;
            };

            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float  _FlowSpeed;
                float  _FlowScale;
                float  _Intensity;
                float  _PulseSpeed;
                float  _FresnelPower;
                float4 _NoiseTex_ST;
                float  _Band;
            CBUFFER_END

            // Golden ratio constant
            static const float PHI = 1.6180339887;

            Varyings vert(Attributes input)
            {
                Varyings output;

                // Subtle vertex animation — φ-modulated wave
                float3 pos = input.positionOS.xyz;
                float wave = sin(_Time.y * _PulseSpeed * PHI + pos.y * _FlowScale) * 0.02;
                pos += input.normalOS * wave;

                VertexPositionInputs posInputs = GetVertexPositionInputs(pos);
                output.positionCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(posInputs.positionWS);
                output.uv = input.uv;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Flowing UV distortion
                float2 flowUV = input.uv * _FlowScale;
                flowUV.y += _Time.y * _FlowSpeed;
                flowUV.x += sin(_Time.y * _FlowSpeed * 0.618) * 0.1; // φ-inverse drift

                float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, flowUV).r;

                // Band-specific color tinting
                // Band 3 (Telluric) = deep blue, Band 6 (Harmonic) = gold, Band 9 (Celestial) = white
                float3 bandColor = _BaseColor.rgb;
                if (_Band < 4.0)
                    bandColor = float3(0.1, 0.3, 0.8); // Telluric blue
                else if (_Band < 7.0)
                    bandColor = float3(0.9, 0.75, 0.2); // Harmonic gold
                else
                    bandColor = float3(0.8, 0.85, 1.0); // Celestial white

                // Fresnel glow
                float fresnel = pow(1.0 - saturate(dot(input.normalWS, input.viewDirWS)), _FresnelPower);

                // Pulsing intensity at golden-ratio rate
                float pulse = sin(_Time.y * _PulseSpeed) * 0.3 + 0.7;

                // Final color
                float3 color = bandColor * _Intensity * (noise * 0.5 + 0.5) * pulse;
                float alpha = _BaseColor.a * (fresnel * 0.6 + noise * 0.4) * pulse;

                return half4(color, saturate(alpha));
            }
            ENDHLSL
        }
    }
    FallBack Off
}
