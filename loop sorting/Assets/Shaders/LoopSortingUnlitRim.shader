Shader "LoopSorting/UnlitRim"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _AO ("AO Strength", Range(0,1)) = 0.35
        _AOPower ("AO Power", Range(0.5,4)) = 2.0
        _Curv ("Curvature Strength", Range(0,1)) = 0.18
        _UseVertexColor ("Use Vertex Color", Range(0,1)) = 0
        _RimColor("Rim Color", Color) = (1,1,1,1)
        _RimPower("Rim Power", Range(0.5, 8.0)) = 2.5
        _RimStrength("Rim Strength", Range(0.0, 1.5)) = 0.35
        _EdgeDarken("Edge Darken", Range(0.0, 1.0)) = 0.15
        _FakeLightDir("Fake Light Dir (XYZ)", Vector) = (0, 0, 1, 0)
        _FakeLightStrength("Fake Light Strength", Range(0.0, 1.0)) = 0.25
        _TopLightDir("Top Light Dir (Local XYZ)", Vector) = (0, 0, 1, 0)
        _ViewLightStrength("View Light Strength", Range(0,1)) = 0
        _ViewPower("View Facing Power", Range(0.5,6)) = 1.6
        _ViewSideMin("View Side Min", Range(0.25,1)) = 0.7
        _Ambient("Ambient", Range(0.0, 1.5)) = 1.0
        _Cull("Cull", Float) = 2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            Cull [_Cull]
            ZWrite On
            ZTest LEqual

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float _AO;
            float _AOPower;
            float _Curv;
            float _UseVertexColor;
            fixed4 _RimColor;
            float _RimPower;
            float _RimStrength;
            float _EdgeDarken;
            float4 _FakeLightDir;
            float _FakeLightStrength;
            float4 _TopLightDir;
            float _ViewLightStrength;
            float _ViewPower;
            float _ViewSideMin;
            float _Ambient;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 N = normalize(i.worldNormal);
                float3 V = normalize(UnityWorldSpaceViewDir(i.worldPos));
                if (dot(N, V) < 0.0)
                {
                    N = -N;
                }

                float useVc = saturate(_UseVertexColor);
                float ao = lerp(1.0, saturate(i.color.r), useVc);
                float curv = saturate(i.color.a) * useVc;
                float aoPow = pow(max(0.0001, ao), _AOPower);

                // Rim (silhouette) term to make edges readable without real lights.
                float3 Nraw = normalize(i.worldNormal);
                float ndv = saturate(dot(N, V));
                float rim = pow(1.0 - ndv, _RimPower);

                // Up-facing light: brightest when normal points to the brick's local +Z (stud direction).
                float3 topDir = normalize(mul((float3x3)unity_ObjectToWorld, _TopLightDir.xyz));
                float ndu = saturate(dot(Nraw, topDir));
                float face = pow(max(0.0001, ndu), _ViewPower);
                float upLit = lerp(_ViewSideMin, 1.0, face);
                float upScale = lerp(1.0, upLit, _ViewLightStrength);

                // A tiny fake directional term (still unlit: constant light dir, no shadows).
                float3 L = normalize(mul((float3x3)unity_ObjectToWorld, _FakeLightDir.xyz));
                float ndl = saturate(dot(Nraw, L));
                float fakeLit = lerp(1.0, (0.6 + 0.4 * ndl), _FakeLightStrength);

                // Optional edge darken to increase contrast.
                float edgeDark = 1.0 - (_EdgeDarken * rim);

                fixed3 baseCol = _Color.rgb * _Ambient;
                baseCol = baseCol * lerp(1.0, aoPow, _AO);
                baseCol += _Color.rgb * curv * _Curv;
                fixed3 col = baseCol * upScale * fakeLit * edgeDark;
                col += _RimColor.rgb * (rim * _RimStrength);

                return fixed4(col, _Color.a);
            }
            ENDCG
        }
    }
}
