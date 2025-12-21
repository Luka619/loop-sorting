Shader "Custom/BrickUnlit_AO_Curv_VertexColor_BuiltIn"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,1)
        _AO ("AO Strength", Range(0,1)) = 0.35
        _AOPower ("AO Power", Range(0.5,4)) = 2.0
        _Curv ("Curvature Strength", Range(0,1)) = 0.18
        _ViewLightStrength("View Light Strength", Range(0,1)) = 0.6
        _ViewPower("View Facing Power", Range(0.5,6)) = 1.6
        _ViewSideMin("View Side Min", Range(0.25,1)) = 0.7
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineStrength ("Outline Strength", Range(0,1)) = 0
        _OutlinePower ("Outline Power", Range(0.5,8)) = 1.4
        _OutlineThreshold ("Outline Threshold", Range(0,1)) = 0.35
        _OutlineSoftness ("Outline Softness", Range(0.0,0.5)) = 0.18
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color  : COLOR;   // RGB = AO, A = curvature
            };

            struct v2f
            {
                float4 pos   : SV_POSITION;
                float4 color : COLOR;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            fixed4 _Color;
            float  _AO;
            float  _AOPower;
            float  _Curv;
            float  _ViewLightStrength;
            float  _ViewPower;
            float  _ViewSideMin;
            fixed4 _OutlineColor;
            float  _OutlineStrength;
            float  _OutlinePower;
            float  _OutlineThreshold;
            float  _OutlineSoftness;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float ao = saturate(i.color.r);
                float curv = saturate(i.color.a);

                float3 baseColor = _Color.rgb;

                float aoPow = pow(max(0.0001, ao), _AOPower);
                float3 c = baseColor * lerp(1.0, aoPow, _AO);
                c += baseColor * curv * _Curv;

                float3 n = normalize(i.worldNormal);
                float3 v = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
                float ndotv = saturate(dot(n, v));
                float face = pow(max(0.0001, ndotv), _ViewPower);
                float viewLit = lerp(_ViewSideMin, 1.0, face);
                c *= lerp(1.0, viewLit, _ViewLightStrength);

                float rim = 1.0 - ndotv;
                float rimPow = pow(max(0.0001, rim), _OutlinePower);
                float outline = smoothstep(_OutlineThreshold, _OutlineThreshold + _OutlineSoftness, rimPow);
                c = lerp(c, _OutlineColor.rgb, outline * _OutlineStrength);

                return fixed4(saturate(c), 1.0);
            }
            ENDCG
        }
    }
}
