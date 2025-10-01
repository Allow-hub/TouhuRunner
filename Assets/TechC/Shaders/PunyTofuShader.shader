Shader "Custom/URP/PunyTofuShader"
{
    Properties
    {
        _BaseMap ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 0.95, 1)
        _SubsurfaceColor ("Subsurface Color", Color) = (1, 0.9, 0.8, 1)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.9
        _SubsurfaceStrength ("Subsurface Strength", Range(0, 1)) = 0.5
        _FresnelPower ("Fresnel Power", Range(0, 5)) = 3
        _JiggleAmount ("Jiggle Amount", Range(0, 0.1)) = 0.02
        _JiggleSpeed ("Jiggle Speed", Range(0, 5)) = 1.5
        _WobbleFrequency ("Wobble Frequency", Range(0, 10)) = 3
        
        [Header(Grid Pattern)]
        _GridColor ("Grid Color", Color) = (0.8, 0.8, 0.75, 1)
        _GridDivisions ("Grid Divisions", Vector) = (4, 4, 0, 0)
        _GridThickness ("Grid Thickness", Range(0, 0.1)) = 0.02
        _GridDepth ("Grid Depth", Range(0, 1)) = 0.3
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        LOD 100

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
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _SubsurfaceColor;
                float _Smoothness;
                float _SubsurfaceStrength;
                float _FresnelPower;
                float _JiggleAmount;
                float _JiggleSpeed;
                float _WobbleFrequency;
                float4 _GridColor;
                float4 _GridDivisions;
                float _GridThickness;
                float _GridDepth;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // ぷにぷに揺れエフェクト
                float time = _Time.y * _JiggleSpeed;
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                
                // 波打つような動き
                float wobble = sin(worldPos.x * _WobbleFrequency + time) * 
                              cos(worldPos.z * _WobbleFrequency + time * 0.7);
                float jiggle = sin(time * 3 + worldPos.y * 5) * 0.5 + 0.5;
                
                // 法線方向に揺らす
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
                IN.positionOS.xyz += IN.normalOS * wobble * jiggle * _JiggleAmount;

                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS = TransformWorldToHClip(OUT.positionWS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // テクスチャサンプリング
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half3 albedo = baseMap.rgb * _BaseColor.rgb;
                
                // グリッド模様の計算
                float2 gridUV = IN.uv * _GridDivisions.xy;
                float2 gridFrac = frac(gridUV);
                
                // 枠線の計算（各セルの端）
                float gridX = step(gridFrac.x, _GridThickness) + step(1.0 - _GridThickness, gridFrac.x);
                float gridY = step(gridFrac.y, _GridThickness) + step(1.0 - _GridThickness, gridFrac.y);
                float grid = saturate(gridX + gridY);
                
                // 外枠の計算
                float borderX = step(IN.uv.x, _GridThickness) + step(1.0 - _GridThickness, IN.uv.x);
                float borderY = step(IN.uv.y, _GridThickness) + step(1.0 - _GridThickness, IN.uv.y);
                float border = saturate(borderX + borderY);
                
                // グリッドと外枠を合成
                float finalGrid = saturate(grid + border);
                
                // ライティングの準備
                float3 normalWS = normalize(IN.normalWS);
                float3 viewDirWS = normalize(GetCameraPositionWS() - IN.positionWS);
                
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(IN.positionWS));
                float3 lightDir = normalize(mainLight.direction);
                
                // Lambert拡散反射
                float NdotL = saturate(dot(normalWS, lightDir));
                float3 diffuse = albedo * NdotL * mainLight.color;
                
                // Subsurface Scattering（簡易版）
                float subsurface = saturate(dot(viewDirWS, -lightDir));
                subsurface = pow(subsurface, 3) * _SubsurfaceStrength;
                float3 sss = _SubsurfaceColor.rgb * subsurface * mainLight.color * albedo;
                
                // Fresnel（リムライト効果）
                float fresnel = 1.0 - saturate(dot(normalWS, viewDirWS));
                fresnel = pow(fresnel, _FresnelPower);
                float3 rim = fresnel * albedo * 0.5;
                
                // Specular（ぷにぷに感のあるハイライト）
                float3 halfDir = normalize(lightDir + viewDirWS);
                float NdotH = saturate(dot(normalWS, halfDir));
                float specular = pow(NdotH, (1.0 - _Smoothness) * 128 + 1) * _Smoothness;
                float3 spec = specular * mainLight.color;
                
                // 環境光
                float3 ambient = albedo * 0.3;
                
                // 影
                float shadow = mainLight.shadowAttenuation;
                
                // グリッド色の適用（少し暗くする）
                float3 gridAlbedo = lerp(albedo, _GridColor.rgb * 0.7, finalGrid * _GridDepth);
                
                // 最終合成（グリッド部分を考慮）
                diffuse = gridAlbedo * NdotL * mainLight.color;
                sss = _SubsurfaceColor.rgb * subsurface * mainLight.color * gridAlbedo;
                rim = fresnel * gridAlbedo * 0.5;
                ambient = gridAlbedo * 0.3;
                
                float3 color = ambient + (diffuse + sss + spec) * shadow + rim;
                
                return half4(color, _BaseColor.a * baseMap.a);
            }
            ENDHLSL
        }

        // シャドウキャスターパス
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            float4 GetShadowPositionHClip(Attributes input)
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _MainLightPosition.xyz));
                return positionCS;
            }

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                output.positionCS = GetShadowPositionHClip(input);
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}