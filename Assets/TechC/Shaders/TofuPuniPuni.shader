Shader "Custom/TofuPuniPuni"
{
    Properties
    {
        _Color("Base Color", Color) = (1,1,1,1)
        _Smoothness("Smoothness", Range(0,1)) = 0.2
        _Squish("Squish Amount", Range(0,0.3)) = 0.1
        _NoiseTex("Noise Texture", 2D) = "white" {}
        _NoiseStrength("Noise Strength", Range(0,0.2)) = 0.05
        _RimColor("Rim Color", Color) = (1,0.95,0.9,1)
        _RimPower("Rim Power", Range(0.5,8)) = 3.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags{"LightMode"="UniversalForward"}

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            sampler2D _NoiseTex;
            float4 _Color;
            float _Smoothness;
            float _Squish;
            float _NoiseStrength;
            float4 _RimColor;
            float _RimPower;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;

                // 頂点をぷにっと揺らす（squish）
                float3 pos = IN.positionOS.xyz;
                pos.y *= (1.0 - _Squish); // 上下方向に押される
                pos.xz *= (1.0 + _Squish * 0.5); // 横に広がる

                OUT.positionWS = TransformObjectToWorld(pos);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionHCS = TransformWorldToHClip(OUT.positionWS);
                OUT.uv = IN.uv;

                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // 簡単なライティング
                Light light = GetMainLight();
                float3 N = normalize(IN.normalWS);
                float3 L = normalize(light.direction);
                float3 V = normalize(GetWorldSpaceViewDir(IN.positionWS));

                // ランバート
                float NdotL = saturate(dot(N, -L));
                float3 diffuse = _Color.rgb * NdotL * light.color;

                // リムライトで柔らかさ
                float rim = pow(1.0 - saturate(dot(N, V)), _RimPower);
                float3 rimCol = _RimColor.rgb * rim;

                // ノイズによるぷにぷに表面
                float noise = tex2D(_NoiseTex, IN.uv * 3.0).r;
                diffuse += noise * _NoiseStrength;

                float3 col = diffuse + rimCol;

                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}