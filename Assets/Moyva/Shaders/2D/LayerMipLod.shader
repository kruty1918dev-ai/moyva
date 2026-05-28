Shader "Moyva/2D/LayerMipLod"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _MipBias ("Base Mip Bias", Range(0, 4)) = 0
        _ZoomLodStrength ("Zoom LOD Strength", Range(0, 2)) = 1
<<<<<<< HEAD
        _GlobalMipBiasWeight ("Global Zoom Mip Bias Weight", Range(0, 1)) = 1
=======
>>>>>>> origin/main
        _AlphaStabilization ("Alpha Stabilization", Range(0, 1)) = 1
        _AlphaClipThreshold ("Alpha Clip Threshold", Range(0, 1)) = 0.2
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        LOD 300

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _MipBias;
            float _ZoomLodStrength;
            float _GlobalMipBiasWeight;
            float _MoyvaTexLodBias;
            float _AlphaStabilization;
            float _AlphaClipThreshold;
            sampler2D _MoyvaFogTex;
            float4 _MoyvaFogMapParams;
            float _MoyvaFogCullEnabled;
            float _MoyvaFogCullThreshold;
            float _MoyvaFogWorldPlane;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
                float2 worldXY : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldXY = _MoyvaFogWorldPlane > 0.5 ? worldPos.xz : worldPos.xy;
                return o;
            }

            void ClipHiddenByFog(float2 worldXY)
            {
                if (_MoyvaFogCullEnabled < 0.5)
                    return;

                float2 mapSize = max(_MoyvaFogMapParams.xy, float2(1.0, 1.0));
                float2 invMapSize = max(_MoyvaFogMapParams.zw, float2(0.000001, 0.000001));

                // Стабільний fog-семпл: квантуємо до індексу тайла і семплимо
                // строго з центру fog-пікселя, щоб уникнути мерехтіння на межах.
                float2 fogCell = floor(worldXY + float2(0.5, 0.5));
                float inside = step(0.0, fogCell.x) * step(fogCell.x, mapSize.x - 1.0)
                             * step(0.0, fogCell.y) * step(fogCell.y, mapSize.y - 1.0);
                float2 fogUV = (clamp(fogCell, float2(0.0, 0.0), mapSize - 1.0) + float2(0.5, 0.5)) * invMapSize;
                float fogValue = tex2D(_MoyvaFogTex, fogUV).r;
                clip((1.0 - inside) + fogValue - _MoyvaFogCullThreshold);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                ClipHiddenByFog(i.worldXY);
                float mipBias = _MipBias + (_MoyvaTexLodBias * _ZoomLodStrength * _GlobalMipBiasWeight);
                fixed4 texColor = tex2Dbias(_MainTex, float4(i.uv, 0, mipBias));

                // Стабілізуємо альфу для mip-level sampling, щоб уникати tile bleeding
                // та «дірчастих» артефактів при сильному zoom-out.
                float stabilizedAlpha = smoothstep(_AlphaClipThreshold, _AlphaClipThreshold + 0.08, texColor.a);
                texColor.a = lerp(texColor.a, stabilizedAlpha, _AlphaStabilization);

                fixed4 c = texColor * i.color;
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }

    // Fallback сабшейдер для старих/слабких GPU.
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _AlphaStabilization;
            float _AlphaClipThreshold;
            sampler2D _MoyvaFogTex;
            float4 _MoyvaFogMapParams;
            float _MoyvaFogCullEnabled;
            float _MoyvaFogCullThreshold;
            float _MoyvaFogWorldPlane;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
                float2 worldXY : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldXY = _MoyvaFogWorldPlane > 0.5 ? worldPos.xz : worldPos.xy;
                return o;
            }

            void ClipHiddenByFog(float2 worldXY)
            {
                if (_MoyvaFogCullEnabled < 0.5)
                    return;

                float2 mapSize = max(_MoyvaFogMapParams.xy, float2(1.0, 1.0));
                float2 invMapSize = max(_MoyvaFogMapParams.zw, float2(0.000001, 0.000001));

                float2 fogCell = floor(worldXY + float2(0.5, 0.5));
                float inside = step(0.0, fogCell.x) * step(fogCell.x, mapSize.x - 1.0)
                             * step(0.0, fogCell.y) * step(fogCell.y, mapSize.y - 1.0);
                float2 fogUV = (clamp(fogCell, float2(0.0, 0.0), mapSize - 1.0) + float2(0.5, 0.5)) * invMapSize;
                float fogValue = tex2D(_MoyvaFogTex, fogUV).r;
                clip((1.0 - inside) + fogValue - _MoyvaFogCullThreshold);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                ClipHiddenByFog(i.worldXY);
                fixed4 texColor = tex2D(_MainTex, i.uv);
                float stabilizedAlpha = smoothstep(_AlphaClipThreshold, _AlphaClipThreshold + 0.08, texColor.a);
                texColor.a = lerp(texColor.a, stabilizedAlpha, _AlphaStabilization);
                fixed4 c = texColor * i.color;
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }

    Fallback "Sprites/Default"
}
