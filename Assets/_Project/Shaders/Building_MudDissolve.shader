Shader "Tartaria/Building/MudDissolve"
{
    // R295 — Mud dissolution shader per docs/15 §8.
    // Animates from BURIED (mud covering stone) → ACTIVE (clean stone) over 5 seconds.
    // Edge glows with Aether-Gold at dissolution front. Used on Dome/Fountain/Spire restoration.

    Properties
    {
        _StoneColor       ("Stone Color (revealed)", Color) = (0.62, 0.52, 0.40, 1)
        _MudColor         ("Mud Color (start)", Color) = (0.28, 0.20, 0.12, 1)
        _GoldenEmission   ("Dissolution Edge (Aether-Gold)", Color) = (1.0, 0.85, 0.45, 1)
        _DissolveProgress ("Dissolve Progress 0-1", Range(0.0, 1.0)) = 0.0
        _BuildingBase     ("Building Base Y", Float) = -8.0
        _BuildingHeight   ("Building Height", Float) = 18.0
        _NoiseTex         ("Dissolve Noise (R)", 2D) = "white" {}
        _NoiseScale       ("Noise Scale", Range(0.5, 8.0)) = 2.0
        _EdgeWidth        ("Glow Edge Width", Range(0.001, 0.2)) = 0.05
        _EdgeIntensity    ("Glow Edge Intensity", Range(0.5, 8.0)) = 3.0
        _Smoothness       ("Smoothness", Range(0, 1)) = 0.10
        _Metallic         ("Metallic", Range(0, 1)) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
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
                float4 positionHCS : SV_POSITION;
                float3 worldPos    : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float2 uv          : TEXCOORD2;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _StoneColor;
                float4 _MudColor;
                float4 _GoldenEmission;
                float  _DissolveProgress;
                float  _BuildingBase;
                float  _BuildingHeight;
                float  _NoiseScale;
                float  _EdgeWidth;
                float  _EdgeIntensity;
                float  _Smoothness;
                float  _Metallic;
            CBUFFER_END

            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs nrm = GetVertexNormalInputs(IN.normalOS);
                OUT.positionHCS = pos.positionCS;
                OUT.worldPos = pos.positionWS;
                OUT.normalWS = nrm.normalWS;
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Per spec §8 formula
                float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, IN.uv * _NoiseScale).r;
                float heightFactor = saturate((IN.worldPos.y - _BuildingBase) / _BuildingHeight);
                float dissolve = _DissolveProgress + heightFactor * 0.3;

                // Color lerp between mud and stone
                float edge = smoothstep(dissolve - 0.05, dissolve + 0.05, noise);
                half3 baseCol = lerp(_StoneColor.rgb, _MudColor.rgb, edge);

                // Golden edge glow at dissolution front
                float glowMask = smoothstep(dissolve - _EdgeWidth, dissolve, noise)
                               - smoothstep(dissolve, dissolve + _EdgeWidth, noise);
                half3 glow = _GoldenEmission.rgb * glowMask * _EdgeIntensity;

                // Basic PBR-style lighting using URP main light
                Light mainLight = GetMainLight();
                float NdotL = saturate(dot(IN.normalWS, mainLight.direction));
                half3 lit = baseCol * (mainLight.color * NdotL * 0.7 + 0.3);  // 0.3 ambient

                return half4(lit + glow, 1);
            }
            ENDHLSL
        }
    }
    Fallback "Universal Render Pipeline/Lit"
}
