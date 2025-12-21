Shader "Custom/URP/BrickUnlit_AO_Curv_VertexColor"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,1)
        _AO ("AO Strength", Range(0,1)) = 0.35
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
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // This shader is authored for URP, but we keep a small built-in fallback so the project can compile
            // even when the URP package isn't installed.
            #if defined(UNITY_RENDER_PIPELINE_UNIVERSAL) || defined(UNIVERSAL_RENDER_PIPELINE)
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #else
                #include "UnityCG.cginc"
                float4 TransformObjectToHClip(float3 positionOS)
                {
                    return UnityObjectToClipPos(float4(positionOS, 1.0));
                }
                float3 TransformObjectToWorld(float3 positionOS)
                {
                    return mul(unity_ObjectToWorld, float4(positionOS, 1.0)).xyz;
                }
                float3 TransformObjectToWorldNormal(float3 normalOS)
                {
                    return UnityObjectToWorldNormal(normalOS);
                }
                float3 GetCameraPositionWS()
                {
                    return _WorldSpaceCameraPos.xyz;
                }
            #endif

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 color      : COLOR; // RGB = AO, A = curvature
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color       : COLOR;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos    : TEXCOORD1;
            };

            float4 _Color;
            float  _AO;
            float  _Curv;
            float  _ViewLightStrength;
            float  _ViewPower;
            float  _ViewSideMin;
            float4 _OutlineColor;
            float  _OutlineStrength;
            float  _OutlinePower;
            float  _OutlineThreshold;
            float  _OutlineSoftness;

            Varyings vert (Attributes i)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(i.positionOS);
                o.color = i.color;
                o.worldNormal = TransformObjectToWorldNormal(i.normalOS);
                o.worldPos = TransformObjectToWorld(i.positionOS);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                half ao = i.color.r;
                half curv = i.color.a;

                half3 baseColor = (half3)_Color.rgb;

                // Darken only (kept subtle)
                half3 c = baseColor * lerp(1.0h, ao, (half)_AO);

                // Add a small convex-edge lift
                c += baseColor * curv * (half)_Curv;

                half3 n = normalize((half3)i.worldNormal);
                half3 v = normalize((half3)(GetCameraPositionWS() - i.worldPos));
                half ndotv = saturate(dot(n, v));
                half face = pow(max(0.0001h, ndotv), (half)_ViewPower);
                half viewLit = lerp((half)_ViewSideMin, 1.0h, face);
                c *= lerp(1.0h, viewLit, (half)_ViewLightStrength);

                half rim = 1.0h - ndotv;
                half rimPow = pow(max(0.0001h, rim), (half)_OutlinePower);
                half outline = smoothstep((half)_OutlineThreshold, (half)(_OutlineThreshold + _OutlineSoftness), rimPow);
                c = lerp(c, (half3)_OutlineColor.rgb, outline * (half)_OutlineStrength);

                return half4(saturate(c), 1.0h);
            }
            ENDHLSL
        }
    }
}
