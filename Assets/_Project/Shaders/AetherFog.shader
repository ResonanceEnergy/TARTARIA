Shader "Tartaria/AetherFog"
{
    Properties
    {
        _AetherTexture ("Aether Volume", 3D) = "white" {}
        _Density ("Density Multiplier", Range(0, 10)) = 1.0
        _Color3Hz ("3Hz Tint", Color) = (0.3, 0.8, 1.0, 1.0)
        _Color6Hz ("6Hz Tint", Color) = (1.0, 0.3, 0.8, 1.0)
        _Color9Hz ("9Hz Tint", Color) = (1.0, 0.9, 0.3, 1.0)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler3D _AetherTexture;
            float _Density;
            float4 _Color3Hz;
            float4 _Color6Hz;
            float4 _Color9Hz;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Simple 3D texture sample (placeholder ray-march)
                float3 uvw = (i.worldPos + 250.0) / 500.0; // Remap world to 0..1
                uvw = saturate(uvw);

                float4 aetherData = tex3D(_AetherTexture, uvw);
                float3 bandColor = aetherData.r * _Color3Hz.rgb +
                                   aetherData.g * _Color6Hz.rgb +
                                   aetherData.b * _Color9Hz.rgb;

                float alpha = aetherData.a * _Density;
                return float4(bandColor, alpha);
            }
            ENDCG
        }
    }
}
