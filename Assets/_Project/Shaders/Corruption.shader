Shader "Tartaria/Corruption"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.4, 0.2, 0.4, 1)
        _CorruptionColor ("Corruption Color", Color) = (0.15, 0.0, 0.2, 1)
        _IridescentTint ("Iridescent Tint", Color) = (0.5, 0.2, 0.8, 1)
        _CorruptionAmount ("Corruption Amount", Range(0, 1)) = 0.5
        _FractalScale ("Fractal Scale", Range(1, 20)) = 10.0
        _FractalSpeed ("Fractal Speed", Range(0, 2)) = 0.3
        _DissolveEdgeWidth ("Dissolve Edge Width", Range(0, 0.5)) = 0.1
        _DissolveEdgeColor ("Dissolve Edge Color", Color) = (1, 0.5, 0, 1)
        _Metallic ("Metallic", Range(0, 1)) = 0.6
        _Smoothness ("Smoothness", Range(0, 1)) = 0.8
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float4 _CorruptionColor;
            float4 _IridescentTint;
            float4 _DissolveEdgeColor;
            float _CorruptionAmount;
            float _FractalScale;
            float _FractalSpeed;
            float _DissolveEdgeWidth;
            float _Metallic;
            float _Smoothness;
        CBUFFER_END
        
        // Fractal noise function (Perlin-like)
        float hash(float2 p)
        {
            float h = dot(p, float2(127.1, 311.7));
            return frac(sin(h) * 43758.5453123);
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
        
        float fractalNoise(float2 uv, float scale, float time)
        {
            float n = 0.0;
            float amplitude = 1.0;
            float frequency = scale;
            for (int i = 0; i < 4; i++)
            {
                n += noise(uv * frequency + time) * amplitude;
                frequency *= 2.0;
                amplitude *= 0.5;
            }
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
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
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
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
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
                // Animated fractal corruption mask
                float time = _Time.y * _FractalSpeed;
                float corruptionMask = fractalNoise(input.uv, _FractalScale, time);
                corruptionMask = smoothstep(0.3, 0.7, corruptionMask);
                
                // Blend between clean and corrupted
                float3 baseAlbedo = lerp(_BaseColor.rgb, _CorruptionColor.rgb, 
                                        corruptionMask * _CorruptionAmount);
                
                // Iridescent rim effect on corruption
                float3 normalWS = normalize(input.normalWS);
                float fresnel = 1.0 - saturate(dot(normalWS, input.viewDirWS));
                fresnel = pow(fresnel, 3.0);
                float3 iridescence = _IridescentTint.rgb * fresnel * corruptionMask;
                
                // Dissolve edge glow
                float dissolveBoundary = abs(corruptionMask - 0.5);
                float edgeGlow = smoothstep(_DissolveEdgeWidth, 0.0, dissolveBoundary);
                float3 emission = _DissolveEdgeColor.rgb * edgeGlow * 2.0;
                
                // PBR lighting
                InputData lightingInput = (InputData)0;
                lightingInput.positionWS = input.positionWS;
                lightingInput.normalWS = normalWS;
                lightingInput.viewDirectionWS = input.viewDirWS;
                lightingInput.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = baseAlbedo + iridescence;
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Smoothness;
                surfaceData.emission = emission;
                surfaceData.alpha = 1.0;
                
                half4 color = UniversalFragmentPBR(lightingInput, surfaceData);
                
                return color;
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
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
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
