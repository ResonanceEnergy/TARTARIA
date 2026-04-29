Shader "Tartaria/AetherVein"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.2, 0.4, 0.8, 1)
        _EmissionColor ("Emission Color", Color) = (0.5, 0.8, 1.0, 1)
        _EmissionIntensity ("Emission Intensity", Range(0, 10)) = 2.0
        _PulseSpeed ("Pulse Speed", Range(0, 5)) = 1.0
        _PulseAmplitude ("Pulse Amplitude", Range(0, 1)) = 0.3
        _WaveSpeed ("Wave Speed", Range(0, 5)) = 0.5
        _WaveAmplitude ("Wave Amplitude", Range(0, 0.5)) = 0.1
        _Metallic ("Metallic", Range(0, 1)) = 0.2
        _Smoothness ("Smoothness", Range(0, 1)) = 0.7
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
            float4 _EmissionColor;
            float _EmissionIntensity;
            float _PulseSpeed;
            float _PulseAmplitude;
            float _WaveSpeed;
            float _WaveAmplitude;
            float _Metallic;
            float _Smoothness;
        CBUFFER_END
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
                
                // Vertex wave displacement
                float wave = sin(_Time.y * _WaveSpeed + input.positionOS.y * 10.0) * _WaveAmplitude;
                float3 displacedPos = input.positionOS.xyz + input.normalOS * wave;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(displacedPos);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.uv = input.uv;
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Pulsing emission
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5; // 0-1 range
                float emissionStrength = lerp(1.0 - _PulseAmplitude, 1.0, pulse);
                
                // Base PBR lighting
                InputData lightingInput = (InputData)0;
                lightingInput.positionWS = input.positionWS;
                lightingInput.normalWS = normalize(input.normalWS);
                lightingInput.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                lightingInput.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = _BaseColor.rgb;
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Smoothness;
                surfaceData.emission = _EmissionColor.rgb * _EmissionIntensity * emissionStrength;
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
        
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            
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
