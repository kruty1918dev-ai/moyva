Shader "Moyva/2D/TileBlend"
{
    Properties
    {
        _MainTex     ("Sprite Texture", 2D) = "white" {}
        [MaterialToggle] _ZWrite("ZWrite", Float) = 0

        // Legacy sprite properties for SpriteRenderer compatibility.
        [HideInInspector] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _AlphaTex ("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha ("Enable External Alpha", Float) = 0

        _AtlasTex    ("Tile Atlas", 2D)     = "white" {}
        _BlendWidth  ("Blend Width", Range(0.05, 0.4)) = 0.2
        _Color       ("Tint", Color)        = (1,1,1,1)
        _ShorelineColor ("Shoreline Color", Color) = (1,1,1,1)
        _ShorelineWidth ("Shoreline Width", Range(0.005, 0.2)) = 0.035
        _ShorelineSoftness ("Shoreline Softness", Range(0.001, 0.15)) = 0.02
        _ShorelineBaseInset ("Shoreline Base Inset", Range(0.0, 0.3)) = 0.0
        _ShorelineTravel ("Shoreline Travel", Range(0.0, 0.45)) = 0.12
        _ShorelineWaveAmplitude ("Shoreline Wave Amplitude", Range(0.0, 0.15)) = 0.03
        _ShorelineWaveFrequency ("Shoreline Wave Frequency", Range(1.0, 32.0)) = 10.0
        _ShorelineSpeed ("Shoreline Speed", Range(0.0, 5.0)) = 1.4
        _ShorelineIntensity ("Shoreline Intensity", Range(0.0, 1.5)) = 1.0

        // Per-renderer properties set by MaterialPropertyBlock.
        [HideInInspector] _TileRect("Tile Rect", Vector) = (0,0,1,1)
        [HideInInspector] _NeighborRectN("Neighbor Rect N", Vector) = (0,0,1,1)
        [HideInInspector] _NeighborRectE("Neighbor Rect E", Vector) = (0,0,1,1)
        [HideInInspector] _NeighborRectS("Neighbor Rect S", Vector) = (0,0,1,1)
        [HideInInspector] _NeighborRectW("Neighbor Rect W", Vector) = (0,0,1,1)
        [HideInInspector] _NeighborMask("Neighbor Mask", Float) = 0
        [HideInInspector] _WaterNeighborMask("Water Neighbor Mask", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Transparent"
            "Queue"          = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite [_ZWrite]

        Pass
        {
            Name "TileBlend"

            HLSLPROGRAM
            #pragma vertex   TileBlendVertex
            #pragma fragment TileBlendFragment
            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY SKINNED_SPRITE

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            TEXTURE2D(_AtlasTex);
            SAMPLER(sampler_AtlasTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _ShorelineColor;
                float _BlendWidth;
                float _ShorelineWidth;
                float _ShorelineSoftness;
                float _ShorelineBaseInset;
                float _ShorelineTravel;
                float _ShorelineWaveAmplitude;
                float _ShorelineWaveFrequency;
                float _ShorelineSpeed;
                float _ShorelineIntensity;
                float4 _TileRect;
                float4 _NeighborRectN;
                float4 _NeighborRectE;
                float4 _NeighborRectS;
                float4 _NeighborRectW;
                float _NeighborMask;
                float _WaterNeighborMask;
            CBUFFER_END

            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_OUTPUTS
                half4 color : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/2DCommon.hlsl"

            float2 RemapUV(float2 localUV, float4 rect)
            {
                return rect.xy + localUV * rect.zw;
            }

            float PixelNoise(float2 p)
            {
                p = floor(p);
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float ShoreBand(float edgeDistance, float tangentCoord, float time)
            {
                float ripple = sin(tangentCoord * _ShorelineWaveFrequency + time)
                    * _ShorelineWaveAmplitude;
                float detail = (PixelNoise(float2(tangentCoord * 32.0, time * 5.0)) - 0.5)
                    * (_ShorelineWaveAmplitude * 0.5);
                float travel = abs(sin(tangentCoord * (_ShorelineWaveFrequency * 0.65) - time * 0.8))
                    * _ShorelineTravel;
                float center = max(0.0, _ShorelineBaseInset + travel + ripple + detail);
                float delta = abs(edgeDistance - center);
                return 1.0 - smoothstep(_ShorelineWidth, _ShorelineWidth + _ShorelineSoftness, delta);
            }

            Varyings TileBlendVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

                Varyings o = CommonUnlitVertex(input);
                o.color = input.color * _Color * unity_SpriteColor;
                return o;
            }

            half4 TileBlendFragment(Varyings input) : SV_Target
            {
                half4 spriteSample = CommonUnlitFragment(input, input.color);
                float spriteAlpha = spriteSample.a;
                if (spriteAlpha <= 0.0001)
                    return half4(0, 0, 0, 0);

                float4 tileRect = _TileRect;
                int mask = (int)_NeighborMask;
                int waterMask = (int)_WaterNeighborMask;

                float2 uv = input.uv;
                float2 atlasUV = RemapUV(uv, tileRect);
                half4 baseColor = SAMPLE_TEXTURE2D(_AtlasTex, sampler_AtlasTex, atlasUV);

                float bw = _BlendWidth;

                // North blend (uv.y near 1)
                if (mask & 1)
                {
                    float4 nRect = _NeighborRectN;
                    if (nRect.z > 0.0001 && nRect.w > 0.0001)
                    {
                        float t = smoothstep(1.0 - bw, 1.0, uv.y);
                        float2 nUV = RemapUV(float2(uv.x, uv.y - 1.0 + bw), nRect);
                        half4 nCol = SAMPLE_TEXTURE2D(_AtlasTex, sampler_AtlasTex, nUV);
                        baseColor = lerp(baseColor, nCol, t);
                    }
                }

                // East blend (uv.x near 1)
                if (mask & 2)
                {
                    float4 eRect = _NeighborRectE;
                    if (eRect.z > 0.0001 && eRect.w > 0.0001)
                    {
                        float t = smoothstep(1.0 - bw, 1.0, uv.x);
                        float2 eUV = RemapUV(float2(uv.x - 1.0 + bw, uv.y), eRect);
                        half4 eCol = SAMPLE_TEXTURE2D(_AtlasTex, sampler_AtlasTex, eUV);
                        baseColor = lerp(baseColor, eCol, t);
                    }
                }

                // South blend (uv.y near 0)
                if (mask & 4)
                {
                    float4 sRect = _NeighborRectS;
                    if (sRect.z > 0.0001 && sRect.w > 0.0001)
                    {
                        float t = smoothstep(bw, 0.0, uv.y);
                        float2 sUV = RemapUV(float2(uv.x, 1.0 - bw + uv.y), sRect);
                        half4 sCol = SAMPLE_TEXTURE2D(_AtlasTex, sampler_AtlasTex, sUV);
                        baseColor = lerp(baseColor, sCol, t);
                    }
                }

                // West blend (uv.x near 0)
                if (mask & 8)
                {
                    float4 wRect = _NeighborRectW;
                    if (wRect.z > 0.0001 && wRect.w > 0.0001)
                    {
                        float t = smoothstep(bw, 0.0, uv.x);
                        float2 wUV = RemapUV(float2(1.0 - bw + uv.x, uv.y), wRect);
                        half4 wCol = SAMPLE_TEXTURE2D(_AtlasTex, sampler_AtlasTex, wUV);
                        baseColor = lerp(baseColor, wCol, t);
                    }
                }

                float shorelineMask = 0.0;
                float time = _Time.y * _ShorelineSpeed;

                if (waterMask & 1)
                    shorelineMask = max(shorelineMask, ShoreBand(1.0 - uv.y, uv.x, time));
                if (waterMask & 2)
                    shorelineMask = max(shorelineMask, ShoreBand(1.0 - uv.x, uv.y, time + 0.9));
                if (waterMask & 4)
                    shorelineMask = max(shorelineMask, ShoreBand(uv.y, uv.x, time + 1.8));
                if (waterMask & 8)
                    shorelineMask = max(shorelineMask, ShoreBand(uv.x, uv.y, time + 2.7));

                if ((waterMask & 16) != 0 && (waterMask & 1) == 0 && (waterMask & 2) == 0)
                {
                    float tangent = dot(uv, normalize(float2(1.0, -1.0)));
                    shorelineMask = max(shorelineMask, ShoreBand(length(float2(1.0 - uv.x, 1.0 - uv.y)), tangent, time + 0.35) * 0.35);
                }
                if ((waterMask & 32) != 0 && (waterMask & 2) == 0 && (waterMask & 4) == 0)
                {
                    float tangent = dot(uv, normalize(float2(1.0, 1.0)));
                    shorelineMask = max(shorelineMask, ShoreBand(length(float2(1.0 - uv.x, uv.y)), tangent, time + 0.7) * 0.35);
                }
                if ((waterMask & 64) != 0 && (waterMask & 4) == 0 && (waterMask & 8) == 0)
                {
                    float tangent = dot(uv, normalize(float2(-1.0, 1.0)));
                    shorelineMask = max(shorelineMask, ShoreBand(length(float2(uv.x, uv.y)), tangent, time + 1.05) * 0.35);
                }
                if ((waterMask & 128) != 0 && (waterMask & 8) == 0 && (waterMask & 1) == 0)
                {
                    float tangent = dot(uv, normalize(float2(-1.0, -1.0)));
                    shorelineMask = max(shorelineMask, ShoreBand(length(float2(uv.x, 1.0 - uv.y)), tangent, time + 1.4) * 0.35);
                }

                shorelineMask = saturate(shorelineMask * _ShorelineIntensity);
                float edgeDist = min(min(uv.x, 1.0 - uv.x), min(uv.y, 1.0 - uv.y));
                float edgeGate = 1.0 - smoothstep(0.03, 0.45, edgeDist);
                float shorelineAlpha = saturate(shorelineMask * edgeGate * _ShorelineColor.a * 0.16);

                baseColor.rgb *= input.color.rgb;
                baseColor.rgb = lerp(baseColor.rgb, _ShorelineColor.rgb, shorelineAlpha);
                baseColor.a = spriteAlpha;
                return baseColor;
            }
            ENDHLSL
        }
    }

    FallBack "Sprites/Default"
}
