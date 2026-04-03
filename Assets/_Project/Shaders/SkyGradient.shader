Shader "Tartaria/SkyGradient"
{
    Properties
    {
        _TopColor ("Sky Top Color", Color) = (0.15, 0.25, 0.55, 1)
        _HorizonColor ("Horizon Color", Color) = (0.75, 0.65, 0.5, 1)
        _GroundColor ("Ground Color", Color) = (0.2, 0.18, 0.15, 1)
        _SunColor ("Sun Glow Color", Color) = (1.0, 0.85, 0.5, 1)
        _SunSize ("Sun Size", Range(0.001, 0.2)) = 0.05
        _SunGlowFalloff ("Sun Glow Falloff", Range(1, 64)) = 8.0
        _RSProgress ("Resonance Score 0-1", Range(0, 1)) = 0.0
        _GoldenTint ("Golden Tint (high RS)", Color) = (0.95, 0.85, 0.4, 1)
        _CorruptionTint ("Corruption Tint", Color) = (0.3, 0.15, 0.35, 1)
        _CorruptionAmount ("Corruption Amount", Range(0, 1)) = 0.0
        _CloudNoise ("Cloud Noise", 2D) = "gray" {}
        _CloudDensity ("Cloud Density", Range(0, 1)) = 0.3
        _CloudSpeed ("Cloud Speed", Float) = 0.02
        _TimeOfDay ("Time of Day 0-1", Range(0, 1)) = 0.5
        _StarDensity ("Star Density", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Background"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Background"
            "PreviewType" = "Skybox"
        }

        Cull Off
        ZWrite Off

        Pass
        {
            Name "SkyGradient"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 viewDirWS  : TEXCOORD0;
            };

            TEXTURE2D(_CloudNoise); SAMPLER(sampler_CloudNoise);

            CBUFFER_START(UnityPerMaterial)
                float4 _TopColor;
                float4 _HorizonColor;
                float4 _GroundColor;
                float4 _SunColor;
                float  _SunSize;
                float  _SunGlowFalloff;
                float  _RSProgress;
                float4 _GoldenTint;
                float4 _CorruptionTint;
                float  _CorruptionAmount;
                float4 _CloudNoise_ST;
                float  _CloudDensity;
                float  _CloudSpeed;
                float  _TimeOfDay;
                float  _StarDensity;
            CBUFFER_END

            static const float PHI = 1.6180339887;
            // PI is already defined by URP Core.hlsl — use it directly

            // Simple hash for procedural stars
            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.viewDirWS = TransformObjectToWorld(input.positionOS.xyz);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 dir = normalize(input.viewDirWS);
                float y = dir.y;

                // ── Day/night cycle ──
                // TimeOfDay: 0 = midnight, 0.25 = sunrise, 0.5 = noon, 0.75 = sunset
                float sunAngle = (_TimeOfDay - 0.25) * 2.0 * PI;
                float3 sunDir = normalize(float3(cos(sunAngle), sin(sunAngle), 0.3));
                float sunHeight = sunDir.y;
                float dayFactor = saturate(sunHeight * 2.0 + 0.5); // 0 at night, 1 at day

                // ── Sky gradient ──
                // Upper sky
                float skyGrad = saturate(y);
                half3 dayTopColor = _TopColor.rgb;
                half3 dayHorizonColor = _HorizonColor.rgb;

                // RS-reactive golden shift
                half3 rsTopTint = lerp(dayTopColor, _GoldenTint.rgb * 0.7, _RSProgress * 0.4);
                half3 rsHorizonTint = lerp(dayHorizonColor, _GoldenTint.rgb, _RSProgress * 0.3);

                // Corruption darkening
                rsTopTint = lerp(rsTopTint, _CorruptionTint.rgb, _CorruptionAmount * 0.5);
                rsHorizonTint = lerp(rsHorizonTint, _CorruptionTint.rgb * 0.8, _CorruptionAmount * 0.3);

                // Night colors — deep blue to black
                half3 nightTop = half3(0.02, 0.02, 0.08);
                half3 nightHorizon = half3(0.05, 0.04, 0.1);

                // Blend day/night
                half3 topColor = lerp(nightTop, rsTopTint, dayFactor);
                half3 horizonColor = lerp(nightHorizon, rsHorizonTint, dayFactor);

                // Sky dome lerp — using pow for non-linear falloff
                half3 skyColor = lerp(horizonColor, topColor, pow(skyGrad, 0.8));

                // Below horizon: ground fog
                float groundGrad = saturate(-y * 4.0);
                skyColor = lerp(skyColor, _GroundColor.rgb * dayFactor, groundGrad);

                // ── Sun disc + glow ──
                float sunDot = saturate(dot(dir, sunDir));
                float sunDisc = smoothstep(1.0 - _SunSize * 0.01, 1.0, sunDot);
                float sunGlow = pow(sunDot, _SunGlowFalloff);

                // Sunrise/sunset warmth at low sun angles
                float sunsetFactor = saturate(1.0 - abs(sunHeight) * 3.0);
                half3 sunsetColor = half3(1.0, 0.4, 0.15) * sunsetFactor;

                skyColor += _SunColor.rgb * (sunDisc * 3.0 + sunGlow * 0.4) * dayFactor;
                skyColor += sunsetColor * saturate(1.0 - abs(y) * 2.0) * sunsetFactor;

                // ── Stars (night only) ──
                float nightFactor = 1.0 - dayFactor;
                if (nightFactor > 0.01 && y > 0.0)
                {
                    float2 starUV = dir.xz / (y + 0.001) * 50.0;
                    float2 starCell = floor(starUV);
                    float starVal = hash21(starCell);
                    float star = step(1.0 - _StarDensity * 0.01, starVal);

                    // Twinkle
                    float twinkle = sin(_Time.y * (starVal * 3.0 + 1.0) * PHI) * 0.5 + 0.5;
                    star *= twinkle;

                    skyColor += star * nightFactor * 0.8;
                }

                // ── Clouds (thin, wispy) ──
                if (_CloudDensity > 0.001)
                {
                    float2 cloudUV = dir.xz / max(y + 0.3, 0.01) * 0.3;
                    cloudUV += _Time.y * _CloudSpeed * float2(1, 0.3);
                    half cloudSample = SAMPLE_TEXTURE2D(_CloudNoise, sampler_CloudNoise, cloudUV).r;

                    // Second layer at different speed
                    float2 cloudUV2 = cloudUV * 1.618 + _Time.y * _CloudSpeed * float2(-0.5, 0.2);
                    half cloudSample2 = SAMPLE_TEXTURE2D(_CloudNoise, sampler_CloudNoise, cloudUV2).r;

                    float cloud = saturate((cloudSample * 0.6 + cloudSample2 * 0.4 - (1.0 - _CloudDensity)) * 3.0);
                    cloud *= saturate(y * 5.0); // Fade at horizon

                    // Clouds colored by sun
                    half3 cloudColor = lerp(half3(0.6, 0.6, 0.7), _SunColor.rgb * 0.8, sunGlow * 0.5);
                    // Corruption tints clouds
                    cloudColor = lerp(cloudColor, _CorruptionTint.rgb, _CorruptionAmount * 0.3);

                    skyColor = lerp(skyColor, cloudColor * dayFactor + half3(0.05, 0.05, 0.08) * nightFactor, cloud * 0.5);
                }

                // ── RS pulse at high resonance (75+) ──
                float highRS = saturate((_RSProgress - 0.75) * 4.0);
                if (highRS > 0.0)
                {
                    float aurPulse = sin(_Time.y * PHI + dir.x * 3.0) *
                                     sin(_Time.y * PHI * 0.618 + dir.z * 2.0);
                    aurPulse = aurPulse * 0.5 + 0.5;
                    skyColor += _GoldenTint.rgb * aurPulse * highRS * 0.15;
                }

                return half4(skyColor, 1.0);
            }
            ENDHLSL
        }
    }
}
