Shader "Hidden/BlockOutline"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" {}
        _MaskTex ("MaskTex", 2D) = "black" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineThickness ("Outline Thickness", Range(0.5, 6)) = 1
        _DebugMode ("Debug Mode", Range(0, 5)) = 0
        _DebugDepthBands ("Debug Depth Bands", Range(1, 2000)) = 200
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
            float _DebugMode;
            float _DebugDepthBands;

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

            float BoxDepth01(float2 uv)
            {
                return tex2D(_MaskTex, uv).r;
            }

            float BlockPresence(float2 uv)
            {
                return tex2D(_MaskTex, uv).g;
            }

            float BlockDepth01(float2 uv)
            {
                return tex2D(_MaskTex, uv).a;
            }

            float DebugDepth01(float d)
            {
                d = saturate(d);
                return pow(d, 0.25);
            }

            float DebugBands(float d)
            {
                return frac(saturate(d) * _DebugDepthBands);
            }

            float VisibleMask(float2 uv)
            {
                float hasBlock = step(0.5, BlockPresence(uv));
                float blockDepth = BlockDepth01(uv);
                float boxDepth = BoxDepth01(uv);
                float visible = step(blockDepth, boxDepth);
                return hasBlock * visible;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 baseCol = tex2D(_MainTex, i.uv);
                float blockDepth = BlockDepth01(i.uv);
                float boxDepth = BoxDepth01(i.uv);
                float hasBlock = step(0.5, BlockPresence(i.uv));
                float hasBox = step(boxDepth, 0.999);
                if (_DebugMode > 0.5 && _DebugMode < 1.5)
                {
                    float m = VisibleMask(i.uv);
                    return fixed4(m, m, m, 1);
                }
                if (_DebugMode > 1.5 && _DebugMode < 2.5)
                {
                    float d = DebugDepth01(blockDepth) * hasBlock;
                    return fixed4(d, d, d, 1);
                }
                if (_DebugMode > 2.5 && _DebugMode < 3.5)
                {
                    float d = DebugDepth01(boxDepth) * hasBox;
                    return fixed4(d, d, d, 1);
                }
                if (_DebugMode > 3.5 && _DebugMode < 4.5)
                {
                    float d = DebugBands(blockDepth) * hasBlock;
                    return fixed4(d, d, d, 1);
                }
                if (_DebugMode > 4.5 && _DebugMode < 5.5)
                {
                    float d = DebugBands(boxDepth) * hasBox;
                    return fixed4(d, d, d, 1);
                }
                float2 texel = _MaskTex_TexelSize.xy * max(0.5, _OutlineThickness);
                float n1 = VisibleMask(i.uv + float2(texel.x, 0));
                float n2 = VisibleMask(i.uv + float2(-texel.x, 0));
                float n3 = VisibleMask(i.uv + float2(0, texel.y));
                float n4 = VisibleMask(i.uv + float2(0, -texel.y));

                float center = VisibleMask(i.uv);
                float maxN = max(max(n1, n2), max(n3, n4));
                float edge = (1.0 - step(0.5, center)) * step(0.5, maxN);
                float alpha = edge * _OutlineColor.a;
                return lerp(baseCol, _OutlineColor, alpha);
            }
            ENDCG
        }
    }
}
