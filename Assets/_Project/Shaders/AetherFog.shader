Shader "Hidden/Tartaria/AetherFog"
{
    Properties
    {
        _AetherVolume ("Aether Volume", 3D) = "white" {}
        _Density ("Density Multiplier", Range(0, 10)) = 1.0
        _Color3Hz ("3Hz Tint", Color) = (0.3, 0.8, 1.0, 1.0)
        _Color6Hz ("6Hz Tint", Color) = (1.0, 0.3, 0.8, 1.0)
        _Color9Hz ("9Hz Tint", Color) = (1.0, 0.9, 0.3, 1.0)
        _AetherFieldOrigin ("Aether Field Origin", Vector) = (0, 0, 0, 0)
        _AetherFieldSize ("Aether Field Size", Float) = 500.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        ZWrite Off Cull Off ZTest Always
        Blend One One // Additive blending for volumetric fog

        Pass
        {
            Name "AetherFog"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.5
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE3D(_AetherVolume);
            SAMPLER(sampler_AetherVolume);
            
            CBUFFER_START(UnityPerMaterial)
                float _Density;
                half4 _Color3Hz;
                half4 _Color6Hz;
                half4 _Color9Hz;
                float3 _AetherFieldOrigin;
                float _AetherFieldSize;
            CBUFFER_END

            half4 Frag(Varyings input) : SV_Target
            {
                // Get scene depth from URP depth texture
                float deviceDepth = SampleSceneDepth(input.texcoord);
                float sceneDepth = LinearEyeDepth(deviceDepth, _ZBufferParams);
                
                // Reconstruct world position from depth
                float3 worldPos = ComputeWorldSpacePosition(input.texcoord, deviceDepth, UNITY_MATRIX_I_VP);
                float3 rayOrigin = _WorldSpaceCameraPos.xyz;
                float3 rayDir = normalize(worldPos - rayOrigin);
                float rayLength = min(sceneDepth, length(worldPos - rayOrigin));
                
                // Raymarch through the Aether volume (24 steps for quality)
                const int numSteps = 24;
                float stepSize = rayLength / (float)numSteps;
                
                half3 accumulatedColor = half3(0, 0, 0);
                half accumulatedAlpha = 0;
                
                for (int step = 0; step < numSteps; step++)
                {
                    float t = (step + 0.5) * stepSize;
                    float3 samplePos = rayOrigin + rayDir * t;
                    
                    // Remap world position to 0..1 texture coords (centered cube)
                    float3 uvw = (samplePos - _AetherFieldOrigin) / _AetherFieldSize + 0.5;
                    
                    // Skip samples outside the volume bounds
                    if (any(uvw < 0.0) || any(uvw > 1.0))
                        continue;
                    
                    // Sample the 3D Aether voxel texture
                    half4 aetherData = SAMPLE_TEXTURE3D(_AetherVolume, sampler_AetherVolume, uvw);
                    
                    // Composite band colors (R=3Hz, G=6Hz, B=9Hz, A=total intensity)
                    half3 bandColor = aetherData.r * _Color3Hz.rgb +
                                     aetherData.g * _Color6Hz.rgb +
                                     aetherData.b * _Color9Hz.rgb;
                    
                    // Beer-Lambert extinction law
                    half density = aetherData.a * _Density * 0.05;
                    half extinction = exp(-density * stepSize);
                    half transmittance = 1.0 - accumulatedAlpha;
                    
                    // Accumulate radiance and opacity
                    accumulatedColor += bandColor * density * transmittance;
                    accumulatedAlpha += transmittance * (1.0 - extinction);
                    
                    // Early out if fully opaque
                    if (accumulatedAlpha > 0.99)
                        break;
                }

                // Return additive contribution (blended One One)
                return half4(accumulatedColor * _Density, accumulatedAlpha);
            }
            ENDHLSL
        }
    }
}
