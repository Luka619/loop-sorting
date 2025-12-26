Shader "Hidden/BoxDepthMask"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            ColorMask R
            ZWrite On
            ZTest LEqual
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float eyeDepth : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.eyeDepth = -UnityObjectToViewPos(v.vertex).z;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float depth01 = saturate((i.eyeDepth - _ProjectionParams.y) / (_ProjectionParams.z - _ProjectionParams.y));
                return fixed4(depth01, 0, 0, 1);
            }
            ENDCG
        }
    }
}
