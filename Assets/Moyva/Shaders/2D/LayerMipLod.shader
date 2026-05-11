Shader "Moyva/2D/LayerMipLod"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _MipBias ("Base Mip Bias", Range(0, 4)) = 0
        _ZoomLodStrength ("Zoom LOD Strength", Range(0, 2)) = 1
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
            float _MoyvaTexLodBias;
            sampler2D _MoyvaFogTex;
            float4 _MoyvaFogMapParams;
            float _MoyvaFogCullEnabled;
            float _MoyvaFogCullThreshold;

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
                o.worldXY = mul(unity_ObjectToWorld, v.vertex).xy;
                return o;
            }

            void ClipHiddenByFog(float2 worldXY)
            {
                if (_MoyvaFogCullEnabled < 0.5)
                    return;

                float2 invMapSize = max(_MoyvaFogMapParams.zw, float2(0.000001, 0.000001));
                float2 fogUV = (worldXY + float2(0.5, 0.5)) * invMapSize;
                float inside = step(0.0, fogUV.x) * step(fogUV.x, 1.0) * step(0.0, fogUV.y) * step(fogUV.y, 1.0);
                float fogValue = tex2D(_MoyvaFogTex, saturate(fogUV)).r;
                clip((1.0 - inside) + fogValue - _MoyvaFogCullThreshold);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                ClipHiddenByFog(i.worldXY);
                float mipBias = _MipBias + (_MoyvaTexLodBias * _ZoomLodStrength);
                fixed4 texColor = tex2Dbias(_MainTex, float4(i.uv, 0, mipBias));
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
            sampler2D _MoyvaFogTex;
            float4 _MoyvaFogMapParams;
            float _MoyvaFogCullEnabled;
            float _MoyvaFogCullThreshold;

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
                o.worldXY = mul(unity_ObjectToWorld, v.vertex).xy;
                return o;
            }

            void ClipHiddenByFog(float2 worldXY)
            {
                if (_MoyvaFogCullEnabled < 0.5)
                    return;

                float2 invMapSize = max(_MoyvaFogMapParams.zw, float2(0.000001, 0.000001));
                float2 fogUV = (worldXY + float2(0.5, 0.5)) * invMapSize;
                float inside = step(0.0, fogUV.x) * step(fogUV.x, 1.0) * step(0.0, fogUV.y) * step(fogUV.y, 1.0);
                float fogValue = tex2D(_MoyvaFogTex, saturate(fogUV)).r;
                clip((1.0 - inside) + fogValue - _MoyvaFogCullThreshold);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                ClipHiddenByFog(i.worldXY);
                fixed4 c = tex2D(_MainTex, i.uv) * i.color;
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }

    Fallback "Sprites/Default"
}
