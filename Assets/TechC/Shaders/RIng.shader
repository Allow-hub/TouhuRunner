Shader "Custom/URP/SpeedRing"
{
    Properties
    {
        _Color ("Base Color", Color) = (0, 0.5, 1, 1)        // 青
        _SecondColor ("Second Color", Color) = (0, 1, 1, 1)  // 水色
        _ThirdColor ("Accent Color", Color) = (1, 1, 1, 1)   // 白
        _EmissionStrength ("Emission Strength", Range(0, 10)) = 6.0
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 3.0
        _RotationSpeed ("Rotation Speed", Range(0, 10)) = 1.0
        _RingThickness ("Ring Thickness", Range(0.1, 0.8)) = 0.3
        _OuterGlow ("Outer Glow", Range(0, 3)) = 2.0
        _InnerGlow ("Inner Glow", Range(0, 3)) = 1.5
        
        // 矢印エフェクト用パラメータ
        _ArrowSpeed ("Arrow Speed", Range(0, 10)) = 4.0
        _ArrowCount ("Arrow Count", Range(3, 12)) = 8
        _ArrowThickness ("Arrow Thickness", Range(0.01, 0.1)) = 0.03
        _ArrowLength ("Arrow Length", Range(0.1, 0.5)) = 0.2
        _ArrowBrightness ("Arrow Brightness", Range(0, 5)) = 2.0
        
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _SecondColor;
                float4 _ThirdColor;
                float _EmissionStrength;
                float _PulseSpeed;
                float _RotationSpeed;
                float _RingThickness;
                float _OuterGlow;
                float _InnerGlow;
                float _ArrowSpeed;
                float _ArrowCount;
                float _ArrowThickness;
                float _ArrowLength;
                float _ArrowBrightness;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // 矢印の形状を計算する関数
            float GetArrowPattern(float2 uv, float time)
            {
                float2 center = float2(0.5, 0.5);
                float2 toCenter = uv - center;
                float angle = atan2(toCenter.y, toCenter.x);
                float distance = length(toCenter);
                
                float arrows = 0.0;
                
                // 複数の矢印を配置
                for(int i = 0; i < (int)_ArrowCount; i++)
                {
                    float angleOffset = (float(i) / _ArrowCount) * 6.283185; // 2π
                    float arrowAngle = angle - angleOffset;
                    
                    // 矢印の位置を時間で移動
                    float arrowPos = fmod(distance + time * _ArrowSpeed * 0.1, 0.4);
                    
                    // 矢印の形状（上向き）
                    float2 arrowUV = float2(
                        sin(arrowAngle) * distance,
                        cos(arrowAngle) * distance - arrowPos + 0.2
                    );
                    
                    // 矢印の本体
                    float arrowBody = 1.0 - smoothstep(0.0, _ArrowThickness, abs(arrowUV.x));
                    arrowBody *= smoothstep(0.0, _ArrowLength, arrowUV.y);
                    arrowBody *= 1.0 - smoothstep(_ArrowLength, _ArrowLength + 0.05, arrowUV.y);
                    
                    // 矢印の先端
                    float arrowHead = 1.0 - smoothstep(0.0, _ArrowThickness * 2.0, abs(arrowUV.x) - (_ArrowLength - arrowUV.y) * 0.5);
                    arrowHead *= smoothstep(_ArrowLength - 0.05, _ArrowLength, arrowUV.y);
                    arrowHead *= 1.0 - smoothstep(_ArrowLength, _ArrowLength + 0.03, arrowUV.y);
                    
                    arrows += max(arrowBody, arrowHead);
                }
                
                return saturate(arrows);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS = TransformWorldToHClip(OUT.positionWS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.viewDirWS = normalize(GetCameraPositionWS() - OUT.positionWS);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float time = _Time.y;
                
                // 中心からの距離とUV座標
                float2 center = float2(0.5, 0.5);
                float2 uv = IN.uv - center;
                float distanceFromCenter = length(uv);
                float angle = atan2(uv.y, uv.x);
                
                // 回転効果（ゆっくり）
                float rotatedAngle = angle + time * _RotationSpeed * 0.2;
                float2 rotatedUV = float2(cos(rotatedAngle), sin(rotatedAngle)) * distanceFromCenter + center;
                
                // パルス効果（青系の速度感）
                float speedPulse = sin(time * _PulseSpeed) * 0.5 + 0.5;
                float fastPulse = sin(time * _PulseSpeed * 3.0) * 0.2 + 0.8;
                
                // リング形状の計算
                float ringMask = 1.0 - abs(distanceFromCenter - 0.35) / _RingThickness;
                ringMask = saturate(ringMask);
                ringMask = pow(ringMask, 1.5);
                
                // 矢印エフェクトを取得
                float arrows = GetArrowPattern(IN.uv, time);
                
                // 内側と外側のグロー
                float innerGlow = exp(-pow(distanceFromCenter * 4.0, 2.0)) * _InnerGlow;
                float outerGlow = exp(-pow((distanceFromCenter - 0.5) * 3.0, 2.0)) * _OuterGlow;
                
                // 青系のグラデーション（速度感のある色合い）
                float3 baseColor = lerp(_Color.rgb, _SecondColor.rgb, speedPulse);
                baseColor = lerp(baseColor, _ThirdColor.rgb, fastPulse * 0.3);
                
                // 角度に基づく微細な色変化（氷のような質感）
                float colorShift = sin(angle * 2.0 + time * 0.5) * 0.1 + 0.9;
                float3 finalColor = baseColor * colorShift;
                
                // フレネル効果（縁の光り）
                float fresnel = 1.0 - saturate(dot(normalize(IN.normalWS), normalize(IN.viewDirWS)));
                fresnel = pow(fresnel, 2.0);
                
                // テクスチャ
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, rotatedUV);
                
                // 矢印の色（明るい青白）
                float3 arrowColor = _SecondColor.rgb * 2.0 + _ThirdColor.rgb;
                
                // エミッション計算
                float3 emission = finalColor * _EmissionStrength * speedPulse;
                emission += fresnel * _SecondColor.rgb * 1.5;
                emission += innerGlow * _Color.rgb;
                emission += outerGlow * _SecondColor.rgb * 0.8;
                emission += arrows * arrowColor * _ArrowBrightness * fastPulse;
                
                // アルファ値の計算
                float alpha = ringMask + innerGlow * 0.2 + outerGlow * 0.15 + arrows * 0.8;
                alpha *= tex.a * speedPulse;
                
                // 最終色の計算（URPでは Albedo + Emission を合成）
                float3 finalResult = finalColor * tex.rgb + emission;
                
                return half4(finalResult, saturate(alpha));
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
