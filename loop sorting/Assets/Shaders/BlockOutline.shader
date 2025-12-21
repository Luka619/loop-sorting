Shader "Hidden/BlockOutline"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" {}
        _MaskTex ("MaskTex", 2D) = "black" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineThickness ("Outline Thickness", Range(0.5, 6)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float4 _MaskTex_TexelSize;
            fixed4 _OutlineColor;
            float _OutlineThickness;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 baseCol = tex2D(_MainTex, i.uv);
                float2 texel = _MaskTex_TexelSize.xy * max(0.5, _OutlineThickness);
                float n1 = tex2D(_MaskTex, i.uv + float2(texel.x, 0)).r;
                float n2 = tex2D(_MaskTex, i.uv + float2(-texel.x, 0)).r;
                float n3 = tex2D(_MaskTex, i.uv + float2(0, texel.y)).r;
                float n4 = tex2D(_MaskTex, i.uv + float2(0, -texel.y)).r;

                float center = tex2D(_MaskTex, i.uv).r;
                float maxN = max(max(n1, n2), max(n3, n4));
                float edge = (1.0 - step(0.5, center)) * step(0.5, maxN);
                float alpha = edge * _OutlineColor.a;
                return lerp(baseCol, _OutlineColor, alpha);
            }
            ENDCG
        }
    }
}
