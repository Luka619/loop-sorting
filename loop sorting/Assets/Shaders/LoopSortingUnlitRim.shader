Shader "LoopSorting/UnlitRim"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _RimColor("Rim Color", Color) = (1,1,1,1)
        _RimPower("Rim Power", Range(0.5, 8.0)) = 2.5
        _RimStrength("Rim Strength", Range(0.0, 1.5)) = 0.35
        _EdgeDarken("Edge Darken", Range(0.0, 1.0)) = 0.15
        _FakeLightDir("Fake Light Dir (XYZ)", Vector) = (0.25, 0.85, -0.45, 0)
        _FakeLightStrength("Fake Light Strength", Range(0.0, 1.0)) = 0.25
        _Ambient("Ambient", Range(0.0, 1.5)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            Cull Back
            ZWrite On
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            fixed4 _RimColor;
            float _RimPower;
            float _RimStrength;
            float _EdgeDarken;
            float4 _FakeLightDir;
            float _FakeLightStrength;
            float _Ambient;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 N = normalize(i.worldNormal);
                float3 V = normalize(UnityWorldSpaceViewDir(i.worldPos));

                // Rim (silhouette) term to make edges readable without real lights.
                float ndv = saturate(dot(N, V));
                float rim = pow(1.0 - ndv, _RimPower);

                // A tiny fake directional term (still unlit: constant light dir, no shadows).
                float3 L = normalize(_FakeLightDir.xyz);
                float ndl = saturate(dot(N, L));
                float fakeLit = lerp(1.0, (0.6 + 0.4 * ndl), _FakeLightStrength);

                // Optional edge darken to increase contrast.
                float edgeDark = 1.0 - (_EdgeDarken * rim);

                fixed3 baseCol = _Color.rgb * _Ambient;
                fixed3 col = baseCol * fakeLit * edgeDark;
                col += _RimColor.rgb * (rim * _RimStrength);

                return fixed4(col, _Color.a);
            }
            ENDCG
        }
    }
}

