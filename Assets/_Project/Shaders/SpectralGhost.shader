Shader "Tartaria/SpectralGhost"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.7, 0.9, 1.0, 1)
        _RimColor ("Rim Glow Color", Color) = (0.8, 1.0, 1.0, 1)
        _RimPower ("Rim Power", Range(0.1, 8.0)) = 3.0
        _RimIntensity ("Rim Intensity", Range(0, 5)) = 2.0
        _Transparency ("Transparency", Range(0, 1)) = 0.6
        _ShimmerSpeed ("Shimmer Speed", Range(0, 5)) = 1.5
        _ShimmerScale ("Shimmer Scale", Range(1, 50)) = 20.0
        _ShimmerIntensity ("Shimmer Intensity", Range(0, 1)) = 0.3
        _DistortionStrength ("Distortion Strength", Range(0, 0.1)) = 0.02
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
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _RimColor;
            float _RimPower;
            float _RimIntensity;
            float _Transparency;
            float _ShimmerSpeed;
            float _ShimmerScale;
            float _ShimmerIntensity;
            float _DistortionStrength;
        CBUFFER_END
        
        // Shimmer noise function
        float hash(float2 p)
        {
            return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
        }
        
        float noise(float2 p)
        {
            float2 i = floor(p);
            float2 f = frac(p);
            f = f * f * (3.0 - 2.0 * f);
            float a = hash(i);
            float b = hash(i + float2(1.0, 0.0));
            float c = hash(i + float2(0.0, 1.0));
            float d = hash(i + float2(1.0, 1.0));
            return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
        }
        
        float shimmer(float2 uv, float time, float scale)
        {
            float n = 0.0;
            n += noise((uv + time * 0.5) * scale) * 0.5;
            n += noise((uv - time * 0.3) * scale * 2.0) * 0.25;
            n += noise((uv + time * 0.7) * scale * 4.0) * 0.125;
            return n;
        }
        ENDHLSL
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
            };
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Subtle vertex distortion
                float time = _Time.y * _ShimmerSpeed;
                float distortion = noise(input.uv * 10.0 + time) * _DistortionStrength;
                float3 distortedPos = input.positionOS.xyz + input.normalOS * distortion;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(distortedPos);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.uv = input.uv;
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                
                // Rim lighting (Fresnel)
                float fresnel = 1.0 - saturate(dot(normalWS, input.viewDirWS));
                fresnel = pow(fresnel, _RimPower);
                float3 rimGlow = _RimColor.rgb * fresnel * _RimIntensity;
                
                // Animated shimmer
                float time = _Time.y * _ShimmerSpeed;
                float shimmerValue = shimmer(input.uv, time, _ShimmerScale);
                shimmerValue = shimmerValue * 2.0 - 1.0; // -1 to 1 range
                float3 shimmerColor = _BaseColor.rgb * (1.0 + shimmerValue * _ShimmerIntensity);
                
                // Combine base color with shimmer and rim
                float3 finalColor = shimmerColor + rimGlow;
                
                // Modulate alpha based on rim (more opaque at edges)
                float alpha = lerp(_Transparency, 1.0, fresnel * 0.5);
                
                // Apply main light
                Light mainLight = GetMainLight();
                finalColor *= mainLight.color * mainLight.distanceAttenuation;
                
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
