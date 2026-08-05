Shader "Moyva/2D/WaterLayerMipLodContour"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Shoreline)]
        _ContourColor ("Shoreline Color", Color) = (0.78, 0.92, 1.0, 1)
        _ContourWidth ("Shoreline Width", Range(0.5, 8)) = 1
        _ContourThreshold ("Shoreline Threshold", Range(0, 0.5)) = 0.01
        _ContourSharpness ("Shoreline Sharpness", Range(1, 32)) = 12
        _ContourOpacity ("Shoreline Opacity", Range(0, 1)) = 1

        [Header(Mask and Stencil)]
        _LandMaskTex ("Land Mask (auto)", 2D) = "black" {}
        [HideInInspector] _MoyvaMobileFillRatePressure ("Mobile Fill Rate Pressure", Range(0, 1)) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _WaterStencilComp ("Water Stencil Compare", Float) = 6
        _WaterStencilRef ("Water Stencil Ref", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent+50"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
            "RenderPipeline"="UniversalPipeline"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        LOD 200

        Stencil
        {
            Ref [_WaterStencilRef]
            Comp [_WaterStencilComp]
            Pass Keep
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            TEXTURE2D(_LandMaskTex);
            SAMPLER(sampler_LandMaskTex);
            float4 _LandMaskTex_TexelSize;

            TEXTURE2D(_MoyvaFogTex);
            SAMPLER(sampler_MoyvaFogTex);
            float4 _MoyvaFogMapParams;
            float _MoyvaFogCullEnabled;
            float _MoyvaFogCullThreshold;
            float _MoyvaFogWorldPlane;

            half4 _Color;
            half4 _ContourColor;
            float _ContourWidth;
            float _ContourThreshold;
            float _ContourSharpness;
            float _ContourOpacity;
            float _MoyvaMobileFillRatePressure;

            struct appdata
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                float2 worldXY : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                float3 worldPos = TransformObjectToWorld(v.positionOS);
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
                float fogValue = SAMPLE_TEXTURE2D(_MoyvaFogTex, sampler_MoyvaFogTex, fogUV).r;
                clip((1.0 - inside) + fogValue - _MoyvaFogCullThreshold);
            }

            float ComputeShorelineMask(float2 uv)
            {
                float center = SAMPLE_TEXTURE2D(_LandMaskTex, sampler_LandMaskTex, uv).r;
                if (center > 0.5)
                    return 0.0;

                float2 stepUV = _LandMaskTex_TexelSize.xy * _ContourWidth;
                float n = SAMPLE_TEXTURE2D(_LandMaskTex, sampler_LandMaskTex, uv + float2(0.0, stepUV.y)).r;
                float s = SAMPLE_TEXTURE2D(_LandMaskTex, sampler_LandMaskTex, uv - float2(0.0, stepUV.y)).r;
                float e = SAMPLE_TEXTURE2D(_LandMaskTex, sampler_LandMaskTex, uv + float2(stepUV.x, 0.0)).r;
                float w = SAMPLE_TEXTURE2D(_LandMaskTex, sampler_LandMaskTex, uv - float2(stepUV.x, 0.0)).r;

                float neighborLand = max(max(n, s), max(e, w));
                return saturate((neighborLand - _ContourThreshold) * _ContourSharpness);
            }

            half4 frag(v2f i) : SV_Target
            {
                ClipHiddenByFog(i.worldXY);
                half4 baseCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float shoreline = 0.0;
                if (_MoyvaMobileFillRatePressure < 0.65)
                {
                    float shorelineFade = saturate(1.0 - _MoyvaMobileFillRatePressure * 1.5);
                    shoreline = ComputeShorelineMask(i.uv) * _ContourOpacity * baseCol.a * shorelineFade;
                }

                half3 rgb = lerp(baseCol.rgb, _ContourColor.rgb, saturate(shoreline));

                half4 outCol = half4(rgb, baseCol.a) * i.color;
                outCol.rgb *= outCol.a;
                return outCol;
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
