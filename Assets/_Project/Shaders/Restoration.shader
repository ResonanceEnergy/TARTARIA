Shader "Tartaria/Restoration"
{
    Properties
    {
        _MudColor ("Mud Color", Color) = (0.4, 0.3, 0.2, 1)
        _MudTexture ("Mud Texture", 2D) = "white" {}
        _CleanColor ("Clean Color", Color) = (0.9, 0.85, 0.7, 1)
        _CleanTexture ("Clean Texture", 2D) = "white" {}
        _RestorationProgress ("Restoration Progress", Range(0, 1)) = 0.0
        _TransitionSharpness ("Transition Sharpness", Range(1, 20)) = 5.0
        _GlowColor ("Restoration Glow", Color) = (1, 0.8, 0.3, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 5)) = 2.0
        _Metallic ("Metallic (Clean)", Range(0, 1)) = 0.3
        _Smoothness ("Smoothness (Clean)", Range(0, 1)) = 0.8
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
        
        TEXTURE2D(_MudTexture);
        SAMPLER(sampler_MudTexture);
        TEXTURE2D(_CleanTexture);
        SAMPLER(sampler_CleanTexture);
        
        CBUFFER_START(UnityPerMaterial)
            float4 _MudColor;
            float4 _MudTexture_ST;
            float4 _CleanColor;
            float4 _CleanTexture_ST;
            float4 _GlowColor;
            float _RestorationProgress;
            float _TransitionSharpness;
            float _GlowIntensity;
            float _Metallic;
            float _Smoothness;
        CBUFFER_END
        
        // Simple noise for restoration wave pattern
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
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Sample textures
                float2 mudUV = TRANSFORM_TEX(input.uv, _MudTexture);
                float2 cleanUV = TRANSFORM_TEX(input.uv, _CleanTexture);
                
                float4 mudSample = SAMPLE_TEXTURE2D(_MudTexture, sampler_MudTexture, mudUV);
                float4 cleanSample = SAMPLE_TEXTURE2D(_CleanTexture, sampler_CleanTexture, cleanUV);
                
                // Create restoration wave pattern (bottom-to-top + noise variation)
                float waveFront = input.uv.y + noise(input.uv * 5.0) * 0.2;
                float restorationMask = smoothstep(
                    waveFront - 0.1, 
                    waveFront + 0.1, 
                    _RestorationProgress
                );
                
                // Sharp transition at wave boundary
                float edgeMask = smoothstep(0.1, 0.0, abs(restorationMask - 0.5)) * 
                                 (1.0 - step(_RestorationProgress, 0.01)) *
                                 (1.0 - step(0.99, _RestorationProgress));
                
                // Blend materials
                float3 mudAlbedo = mudSample.rgb * _MudColor.rgb;
                float3 cleanAlbedo = cleanSample.rgb * _CleanColor.rgb;
                float3 albedo = lerp(mudAlbedo, cleanAlbedo, restorationMask);
                
                // Restoration edge glow
                float3 emission = _GlowColor.rgb * _GlowIntensity * edgeMask;
                
                // Lerp material properties during restoration
                float metallic = lerp(0.1, _Metallic, restorationMask);
                float smoothness = lerp(0.3, _Smoothness, restorationMask);
                
                // PBR lighting
                InputData lightingInput = (InputData)0;
                lightingInput.positionWS = input.positionWS;
                lightingInput.normalWS = normalize(input.normalWS);
                lightingInput.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                lightingInput.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo;
                surfaceData.metallic = metallic;
                surfaceData.smoothness = smoothness;
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
