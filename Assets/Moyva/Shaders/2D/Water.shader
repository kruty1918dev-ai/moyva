Shader "Moyva/2D/Water"
{
    Properties
    {
        _MainTex       ("Sprite Texture", 2D) = "white" {}
        _AtlasTex      ("Atlas Texture", 2D) = "white" {}
        [MaterialToggle] _ZWrite("ZWrite", Float) = 0

        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _AlphaTex ("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha ("Enable External Alpha", Float) = 0

        [Header(Depth Gradient)]
        _ShallowColor  ("Shallow Color", Color) = (0.21, 0.42, 0.35, 1)
        _MidColor      ("Mid Color", Color)     = (0.13, 0.24, 0.25, 1)
        _DeepColor     ("Deep Color", Color)    = (0.12, 0.16, 0.25, 1)
        _DepthEnd      ("Depth End", Range(1, 12)) = 5
        _DepthExponent ("Depth Curve", Range(0.3, 4.0)) = 1.0
        _MidPoint      ("Mid Color Position", Range(0.05, 0.95)) = 0.25

        [Header(Caustics)]
        _CausticColor          ("Caustic Color", Color) = (0.55, 1.0, 0.97, 0.008)
        _CausticHighlightColor ("Highlight Color", Color) = (0.74, 0.89, 0.90, 0.02)
        _CausticScale          ("Scale", Range(0.5, 20)) = 3.5
        _CausticSpeed          ("Speed", Range(0.01, 3)) = 0.35
        _CausticMovement       ("Movement Amount", Range(0, 0.15)) = 0.08
        _CausticMovementScale  ("Movement Noise Scale", Range(0.1, 5)) = 1.63
        _CausticSquash         ("Y Squash", Range(0.5, 4)) = 1.68
        _CausticFadeEnd        ("Fade End Depth", Range(0.5, 12)) = 6.0

        [Header(Specular)]
        _SpecularColor     ("Color", Color) = (1, 1, 1, 0.06)
        _SpecularSpeed     ("Speed", Range(0.01, 2)) = 0.13
        _SpecularScale1    ("Scale 1", Range(1, 40)) = 5
        _SpecularScale2    ("Scale 2", Range(1, 40)) = 3
        _SpecularThreshold ("Threshold", Range(0.80, 0.999)) = 0.96

        [Header(Foam)]
        _FoamColor     ("Foam Color", Color) = (1, 1, 1, 0.22)
        _FoamWidth     ("Foam Width", Range(0.01, 0.5)) = 0.10
        _FoamSoftness  ("Foam Softness", Range(0.01, 0.5)) = 0.25
        _FoamWaveSpeed ("Foam Wave Speed", Range(0.0, 5.0)) = 1.2
        _FoamWaveAmplitude ("Foam Wave Amplitude", Range(0.0, 0.15)) = 0.04
        _FoamWaveFrequency ("Foam Wave Frequency", Range(1.0, 32.0)) = 8.0

        [Header(Shore Blend)]
        _ShoreBlendWidth ("Shore Blend Width", Range(0.1, 3.0)) = 1.35
        _TransitionWidth ("Transition Width", Range(0.05, 0.5)) = 0.28
        _TransitionSmooth("Transition Corner Smooth", Range(0.01, 0.15)) = 0.06
        _DiagonalWeight ("Diagonal Blend Weight", Range(0.1, 1.0)) = 0.72
        _ContactWidth ("Cardinal Contact Width", Range(0.03, 0.45)) = 0.16
        _DiagonalContactWidth ("Diagonal Contact Width", Range(0.02, 0.3)) = 0.09
        _CardinalBlendStrength ("Cardinal Blend Strength", Range(0.0, 2.0)) = 1.0
        _DiagonalBlendStrength ("Diagonal Blend Strength", Range(0.0, 2.0)) = 0.65
        _DepthSmoothing ("Depth Interpolation", Range(0.0, 1.0)) = 0.88
        _ShoreDistSmoothing ("Shore Dist Smoothing", Range(0.0, 1.0)) = 0.32
        _AtlasInset ("Atlas Inset", Range(0.0, 0.05)) = 0.006

        [Header(Core Stabilization)]
        _CoreStartDist ("Core Start Distance", Range(0.0, 8.0)) = 2.6
        _CoreFullDist ("Core Full Distance", Range(0.1, 12.0)) = 4.5
        _CoreNeighborSuppression ("Core Neighbor Suppression", Range(0.0, 1.0)) = 1.0
        _CoreNoiseSuppression ("Core Noise Suppression", Range(0.0, 1.0)) = 0.85
        _CoreFoamSuppression ("Core Foam Suppression", Range(0.0, 1.0)) = 1.0
        _CoreCausticSuppression ("Core Caustic Suppression", Range(0.0, 1.0)) = 1.0
        _CoreLightingSuppression ("Core Lighting Suppression", Range(0.0, 1.0)) = 0.75

        [Header(Pixel Art)]
        _Pixelization  ("Pixelization", Float) = 0
        _TileSize      ("Tile Size (World Units)", Float) = 1

        [HideInInspector] _ShoreCorners ("Shore Corners", Vector) = (0,0,0,0)
        [HideInInspector] _ShoreFlow ("Shore Flow", Vector) = (0,1,0,0)

        [Header(Global Map)]
        _TileIdMap     ("Tile ID Map", 2D) = "black" {}
        _ShoreDistMap  ("Shore Distance Map", 2D) = "black" {}
        _TileUVLookup  ("Tile UV Lookup", 2D) = "black" {}
        _WaterTileId   ("Water Tile ID", Float) = 0
        _TileCount     ("Tile Type Count", Float) = 1
        _MapSize       ("Map Size", Vector) = (64, 64, 0, 0)
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
            Name "Water"

            HLSLPROGRAM
            #pragma vertex   WaterVertex
            #pragma fragment WaterFragment
            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY SKINNED_SPRITE

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _ShallowColor;
                half4 _MidColor;
                half4 _DeepColor;
                half4 _CausticColor;
                half4 _CausticHighlightColor;
                half4 _SpecularColor;
                half4 _FoamColor;
                float _DepthEnd;
                float _DepthExponent;
                float _MidPoint;
                float _CausticScale;
                float _CausticSpeed;
                float _CausticMovement;
                float _CausticMovementScale;
                float _CausticSquash;
                float _CausticFadeEnd;
                float _SpecularSpeed;
                float _SpecularScale1;
                float _SpecularScale2;
                float _SpecularThreshold;
                float _FoamWidth;
                float _FoamSoftness;
                float _FoamWaveSpeed;
                float _FoamWaveAmplitude;
                float _FoamWaveFrequency;
                float _ShoreBlendWidth;
                float _TransitionWidth;
                float _TransitionSmooth;
                float _DiagonalWeight;
                float _ContactWidth;
                float _DiagonalContactWidth;
                float _CardinalBlendStrength;
                float _DiagonalBlendStrength;
                float _DepthSmoothing;
                float _ShoreDistSmoothing;
                float _AtlasInset;
                float _CoreStartDist;
                float _CoreFullDist;
                float _CoreNeighborSuppression;
                float _CoreNoiseSuppression;
                float _CoreFoamSuppression;
                float _CoreCausticSuppression;
                float _CoreLightingSuppression;
                float _Pixelization;
                float _TileSize;
                float _WaterTileId;
                float _TileCount;
                float4 _MapSize;
            CBUFFER_END

            TEXTURE2D(_AtlasTex);
            SAMPLER(sampler_AtlasTex);
            TEXTURE2D(_TileIdMap);
            TEXTURE2D(_ShoreDistMap);
            TEXTURE2D(_TileUVLookup);
            SAMPLER(sampler_TileUVLookup);

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
                float2 worldPos : TEXCOORD2;
                float2 tileCenter : TEXCOORD3;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/2DCommon.hlsl"

            // ─── Pixelization (matches reference: floor(pos*pix)/pix) ───
            float2 Pixelate(float2 v, float ppu)
            {
                return floor(v * ppu) / ppu;
            }

            // ─── Gradient Noise (Perlin-like, matches Unity GradientNoise node) ───
            float2 GradNoiseDir(float2 p)
            {
                p = fmod(p, 289.0);
                float x = fmod((34.0 * p.x + 1.0) * p.x, 289.0) + p.y;
                x = fmod((34.0 * x + 1.0) * x, 289.0);
                x = frac(x / 41.0) * 2.0 - 1.0;
                return normalize(float2(x - floor(x + 0.5), abs(x) - 0.5));
            }

            float GradNoise(float2 uv, float scale)
            {
                float2 p = uv * scale;
                float2 ip = floor(p);
                float2 fp = frac(p);
                float2 u = fp * fp * fp * (fp * (fp * 6.0 - 15.0) + 10.0);
                float d00 = dot(GradNoiseDir(ip), fp);
                float d01 = dot(GradNoiseDir(ip + float2(0, 1)), fp - float2(0, 1));
                float d10 = dot(GradNoiseDir(ip + float2(1, 0)), fp - float2(1, 0));
                float d11 = dot(GradNoiseDir(ip + float2(1, 1)), fp - float2(1, 1));
                return lerp(lerp(d00, d10, u.x), lerp(d01, d11, u.x), u.y) + 0.5;
            }

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float GlobalCellNoise(float2 worldPos, float cellsPerTile)
            {
                float safeTileSize = max(_TileSize, 0.0001);
                float safeCellsPerTile = max(cellsPerTile, 0.0001);
                float2 cellPos = worldPos * (safeCellsPerTile / safeTileSize);
                float2 cell = floor(cellPos);
                float2 fracPos = frac(cellPos);
                float2 blend = fracPos * fracPos * (3.0 - 2.0 * fracPos);

                float n00 = Hash21(cell);
                float n10 = Hash21(cell + float2(1.0, 0.0));
                float n01 = Hash21(cell + float2(0.0, 1.0));
                float n11 = Hash21(cell + float2(1.0, 1.0));

                return lerp(lerp(n00, n10, blend.x), lerp(n01, n11, blend.x), blend.y);
            }

            float4 SampleAtlasRect(float4 rect, float2 uv)
            {
                float inset = saturate(_AtlasInset);
                float2 safeUV = lerp(float2(inset, inset), float2(1.0 - inset, 1.0 - inset), saturate(uv));
                float2 atlasUV = rect.xy + safeUV * rect.zw;
                return SAMPLE_TEXTURE2D(_AtlasTex, sampler_AtlasTex, atlasUV);
            }

            // ─── Tile map texture reads ─────────────────────
            float ReadTileId(int2 coord, int2 mapSz)
            {
                if (coord.x < 0 || coord.y < 0 || coord.x >= mapSz.x || coord.y >= mapSz.y)
                    return -1.0;
                return LOAD_TEXTURE2D(_TileIdMap, coord).r;
            }

            float ReadShoreDist(int2 coord, int2 mapSz)
            {
                if (coord.x < 0 || coord.y < 0 || coord.x >= mapSz.x || coord.y >= mapSz.y)
                    return 0.0;
                return LOAD_TEXTURE2D(_ShoreDistMap, coord).r;
            }

            float4 GetAtlasRect(float tileId)
            {
                if (tileId < -0.5) return float4(0, 0, 0, 0);
                float2 luv = float2((tileId + 0.5) / _TileCount, 0.5);
                return SAMPLE_TEXTURE2D_LOD(_TileUVLookup, sampler_TileUVLookup, luv, 0);
            }

            // ─── Smooth min for soft corner transitions ──────
            float SmoothMin(float a, float b, float k)
            {
                float h = saturate(0.5 + 0.5 * (b - a) / max(k, 0.0001));
                return lerp(b, a, h) - k * h * (1.0 - h);
            }

            // ─── Multi-octave noise for natural variation ───
            float WaterDetailNoise(float2 worldUV, float time)
            {
                float2 base = worldUV;
                float n = 0.0;
                n += GradNoise(base + float2(0.0, time * 0.02), 3.0) * 0.5;
                n += GradNoise(base + float2(5.2, 1.3) + float2(time * 0.015, 0.0), 6.0) * 0.25;
                n += GradNoise(base + float2(9.7, 4.1) + float2(0.0, time * 0.01), 12.0) * 0.125;
                n += GradNoise(base + float2(14.3, 7.8) + float2(time * 0.008, time * 0.005), 24.0) * 0.0625;
                return n * (1.0 / 0.9375);
            }

            // ─── Hash ───────────────────────────────────────
            float2 Hash22(float2 p)
            {
                float3 a = frac(p.xyx * float3(123.34, 234.34, 345.65));
                a += dot(a, a + 34.45);
                return frac(float2(a.x * a.y, a.y * a.z));
            }

            // ─── Voronoi caustic web pattern ────────────────
            float VoronoiCaustic(float2 uv, float time)
            {
                float2 g = floor(uv);
                float2 f = frac(uv);
                float f1 = 8.0;
                float f2 = 8.0;

                for (int oy = -1; oy <= 1; oy++)
                {
                    for (int ox = -1; ox <= 1; ox++)
                    {
                        float2 off = float2(ox, oy);
                        float2 h = Hash22(g + off);
                        h = 0.5 + 0.4 * sin(time + 6.2831853 * h);
                        float d = length(off + h - f);
                        if (d < f1) { f2 = f1; f1 = d; }
                        else if (d < f2) { f2 = d; }
                    }
                }

                return smoothstep(0.25, 0.0, f2 - f1);
            }

            // ─── Vertex ─────────────────────────────────────
            Varyings WaterVertex(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                SetUpSpriteInstanceProperties();
                input.positionOS = UnityFlipSprite(input.positionOS, unity_SpriteProps.xy);

                Varyings o = CommonUnlitVertex(input);
                o.color = input.color * _Color * unity_SpriteColor;
                o.worldPos = mul(unity_ObjectToWorld, float4(input.positionOS.xyz, 1.0)).xy;
                o.tileCenter = mul(unity_ObjectToWorld, float4(0.0, 0.0, 0.0, 1.0)).xy;
                return o;
            }

            // ─── Edge mask for a single direction ──────────
            // Returns a 0..1 mask fading from the tile edge inward.
            float EdgeMask(float2 uv, float tw, int dir)
            {
                // dir: 0=N, 1=E, 2=S, 3=W
                float d = 0.0;
                d += (dir == 0) ? uv.y : 0.0;      // N: edge at y=1
                d += (dir == 1) ? uv.x : 0.0;      // E: edge at x=1
                d += (dir == 2) ? (1.0 - uv.y) : 0.0; // S: edge at y=0
                d += (dir == 3) ? (1.0 - uv.x) : 0.0; // W: edge at x=0
                return smoothstep(1.0 - tw, 1.0, d);
            }

            // ─── Fragment ───────────────────────────────────
            half4 WaterFragment(Varyings input) : SV_Target
            {
                half4 spriteSample = CommonUnlitFragment(input, input.color);
                float spriteAlpha = spriteSample.a;
                if (spriteAlpha <= 0.0001)
                    return half4(0, 0, 0, 0);

                float2 worldUV  = input.worldPos;
                float time      = _Time.y;
                float safeTileSize = max(_TileSize, 0.0001);
                float2 localTileUV = (worldUV - input.tileCenter) / safeTileSize + 0.5;
                float2 localTileUV01 = saturate(localTileUV);

                // ── Tile coordinate & per-tile hash ──
                int2 tileCoord = int2(round(input.tileCenter));
                int2 mapSize = int2(_MapSize.xy);

                // ── Read 8 neighbors from tile ID map ──
                float wid = _WaterTileId;
                float id_N  = ReadTileId(tileCoord + int2(0,1), mapSize);
                float id_E  = ReadTileId(tileCoord + int2(1,0), mapSize);
                float id_S  = ReadTileId(tileCoord + int2(0,-1), mapSize);
                float id_W  = ReadTileId(tileCoord + int2(-1,0), mapSize);
                float id_NE = ReadTileId(tileCoord + int2(1,1), mapSize);
                float id_SE = ReadTileId(tileCoord + int2(1,-1), mapSize);
                float id_SW = ReadTileId(tileCoord + int2(-1,-1), mapSize);
                float id_NW = ReadTileId(tileCoord + int2(-1,1), mapSize);

                int neighborMask = 0;
                // id > -0.5 means the tile is valid (in-map). Out-of-bounds returns -1.0
                // and must NOT be treated as a non-water neighbour (it is just void).
                neighborMask |= (id_N  > -0.5 && abs(id_N  - wid) > 0.5) ? 1   : 0;
                neighborMask |= (id_E  > -0.5 && abs(id_E  - wid) > 0.5) ? 2   : 0;
                neighborMask |= (id_S  > -0.5 && abs(id_S  - wid) > 0.5) ? 4   : 0;
                neighborMask |= (id_W  > -0.5 && abs(id_W  - wid) > 0.5) ? 8   : 0;
                neighborMask |= (id_NE > -0.5 && abs(id_NE - wid) > 0.5) ? 16  : 0;
                neighborMask |= (id_SE > -0.5 && abs(id_SE - wid) > 0.5) ? 32  : 0;
                neighborMask |= (id_SW > -0.5 && abs(id_SW - wid) > 0.5) ? 64  : 0;
                neighborMask |= (id_NW > -0.5 && abs(id_NW - wid) > 0.5) ? 128 : 0;
                int shoreMask = neighborMask;

                // ── Shore data from global maps ──
                float shoreDist = ReadShoreDist(tileCoord, mapSize);
                float sd_N  = ReadShoreDist(tileCoord + int2(0,1), mapSize);
                float sd_E  = ReadShoreDist(tileCoord + int2(1,0), mapSize);
                float sd_S  = ReadShoreDist(tileCoord + int2(0,-1), mapSize);
                float sd_W  = ReadShoreDist(tileCoord + int2(-1,0), mapSize);
                float sd_NE = ReadShoreDist(tileCoord + int2(1,1), mapSize);
                float sd_SE = ReadShoreDist(tileCoord + int2(1,-1), mapSize);
                float sd_SW = ReadShoreDist(tileCoord + int2(-1,-1), mapSize);
                float sd_NW = ReadShoreDist(tileCoord + int2(-1,1), mapSize);

                float4 shoreCorners;
                shoreCorners.x = (shoreDist + sd_W + sd_N + sd_NW) * 0.25;
                shoreCorners.y = (shoreDist + sd_E + sd_N + sd_NE) * 0.25;
                shoreCorners.z = (shoreDist + sd_E + sd_S + sd_SE) * 0.25;
                shoreCorners.w = (shoreDist + sd_W + sd_S + sd_SW) * 0.25;

                float2 shoreFlowVec;
                shoreFlowVec.x = sd_E - sd_W;
                shoreFlowVec.y = sd_N - sd_S;
                float flowLen = length(shoreFlowVec);
                shoreFlowVec = (flowLen > 0.001) ? (shoreFlowVec / flowLen) : float2(0, 1);

                // ── Bilinear shore depth from corners ──
                float northDepth = lerp(shoreCorners.x, shoreCorners.y, localTileUV01.x);
                float southDepth = lerp(shoreCorners.w, shoreCorners.z, localTileUV01.x);
                float interpolatedShoreDist = lerp(southDepth, northDepth, localTileUV01.y);
                // Always use a portion of bilinear interpolation across the tile,
                // otherwise center pixels keep flat per-tile shoreDist and form visible blocks.
                float visualShoreDist = lerp(shoreDist, interpolatedShoreDist, saturate(_DepthSmoothing));
                float shoreAvg9 = (shoreDist + sd_N + sd_E + sd_S + sd_W + sd_NE + sd_SE + sd_SW + sd_NW) * (1.0 / 9.0);
                visualShoreDist = lerp(visualShoreDist, shoreAvg9, saturate(_ShoreDistSmoothing));

                // Core mask: far-from-shore pixels are treated as stable deep-water core.
                float coreMask = smoothstep(_CoreStartDist, max(_CoreFullDist, _CoreStartDist + 0.001), visualShoreDist);

                // ═══ 1. DEPTH GRADIENT (3-stop) ═══
                float depthT = saturate(visualShoreDist / max(_DepthEnd, 0.001));
                depthT = pow(depthT, _DepthExponent);

                half3 waterCol;
                float mid = _MidPoint;
                float belowMid = step(depthT, mid);
                half3 shallowToMid = lerp(_ShallowColor.rgb, _MidColor.rgb, depthT / max(mid, 0.001));
                half3 midToDeep = lerp(_MidColor.rgb, _DeepColor.rgb, (depthT - mid) / max(1.0 - mid, 0.001));
                waterCol = lerp(midToDeep, shallowToMid, belowMid);

                // Multi-octave detail noise to break flat gradient and reduce tiling
                float detailNoise = WaterDetailNoise(worldUV, time);
                float detailFactor = lerp(0.97, 1.03, detailNoise);
                float detailApply = 1.0 - coreMask * saturate(_CoreNoiseSuppression);
                waterCol *= lerp(1.0, detailFactor, detailApply);
                // Subtle deepening only in the very deep zone (depthT > 0.55);
                // removed the wide 0.24..0.92 range that was double-applying _DeepColor
                // and making the center of even small lakes look unnaturally bright.
                float deepCore = smoothstep(0.55, 1.0, depthT);
                waterCol = lerp(waterCol, _DeepColor.rgb * 0.92, deepCore * 0.18);

                // ═══ 2. CAUSTICS (subtle, world-space) ═══
                float cTime = time * _CausticSpeed;
                float2 squashedCoords = worldUV * float2(1.0, _CausticSquash);
                float2 causticBaseUV = (squashedCoords + float2(13.37, 7.91)) * _CausticScale;
                float noiseVal = GradNoise(squashedCoords + cTime, _CausticMovementScale);
                float2 causticUV = lerp(causticBaseUV, float2(noiseVal, noiseVal), _CausticMovement);

                float c1 = VoronoiCaustic(causticUV, cTime);
                float c2 = VoronoiCaustic(causticUV * 1.4 + 3.7, cTime * 0.8 + 1.5);

                // Caustics show near shore only, fade out at depth AND far from coast.
                float rawCausticFade = 1.0 - saturate(visualShoreDist / max(_CausticFadeEnd, 0.001));
                // Kill caustics in deep open water to prevent brightening.
                // Tighter range (0.08–0.28) vs old (0.15–0.50) — caustics vanish faster with depth.
                float causticFade = rawCausticFade * (1.0 - smoothstep(0.08, 0.28, depthT));
                causticFade *= (1.0 - coreMask * saturate(_CoreCausticSuppression));
                float causticPulse = GradNoise(worldUV, 0.17) * 0.03;
                float causticAlpha = saturate(causticFade - causticPulse);

                half3 causticRGB = c1 * _CausticColor.rgb * _CausticColor.a
                                 + c2 * _CausticHighlightColor.rgb * _CausticHighlightColor.a;
                waterCol += causticRGB * causticAlpha;

                // ═══ 3. SPECULAR SPARKLES ═══
                float2 specUV1 = worldUV + float2(time * _SpecularSpeed, 0.0);
                float2 specUV2 = worldUV + float2(0.0, time * _SpecularSpeed * 0.7);
                float spec1 = GradNoise(specUV1, _SpecularScale1);
                float spec2 = GradNoise(specUV2, _SpecularScale2);
                float specMask = smoothstep(_SpecularThreshold, 1.0, max(spec1, spec2));
                // Specular only near shore, vanishes in deep water.
                specMask = specMask * specMask * causticFade * 0.15;
                waterCol = lerp(waterCol, _SpecularColor.rgb, specMask * _SpecularColor.a);

                // Pseudo normal from shore gradient to avoid broken lighting along the coast.
                float3 pseudoNormal = normalize(float3(shoreFlowVec * 0.35, 1.0));
                float3 lightDir = normalize(float3(-0.28, 0.34, 0.90));
                float ndl = saturate(dot(pseudoNormal, lightDir));
                float lightingFactor = lerp(0.92, 1.04, ndl);
                float lightingApply = 1.0 - coreMask * saturate(_CoreLightingSuppression);
                waterCol *= lerp(1.0, lightingFactor, lightingApply);

                // ═══ 4. TRANSITION MASKS (8-direction, branchless smoothmin) ═══
                float tw = max(_TransitionWidth, 0.001);
                float sk = max(_TransitionSmooth, 0.001);

                // Cardinal masks
                float mN = ((neighborMask & 1) != 0) ? EdgeMask(localTileUV01, tw, 0) : 0.0;
                float mE = ((neighborMask & 2) != 0) ? EdgeMask(localTileUV01, tw, 1) : 0.0;
                float mS = ((neighborMask & 4) != 0) ? EdgeMask(localTileUV01, tw, 2) : 0.0;
                float mW = ((neighborMask & 8) != 0) ? EdgeMask(localTileUV01, tw, 3) : 0.0;

                // Round L-shaped junctions where adjacent cardinals overlap
                {
                    float sN = 1.0, sE = 1.0, sS = 1.0, sW = 1.0;
                    if (mN > 0.001 && mE > 0.001)
                    { float c = 1.0 - SmoothMin(1.0-mN,1.0-mE,sk); float s = min(1.0,c/(mN+mE)); sN = min(sN,s); sE = min(sE,s); }
                    if (mE > 0.001 && mS > 0.001)
                    { float c = 1.0 - SmoothMin(1.0-mE,1.0-mS,sk); float s = min(1.0,c/(mE+mS)); sE = min(sE,s); sS = min(sS,s); }
                    if (mS > 0.001 && mW > 0.001)
                    { float c = 1.0 - SmoothMin(1.0-mS,1.0-mW,sk); float s = min(1.0,c/(mS+mW)); sS = min(sS,s); sW = min(sW,s); }
                    if (mW > 0.001 && mN > 0.001)
                    { float c = 1.0 - SmoothMin(1.0-mW,1.0-mN,sk); float s = min(1.0,c/(mW+mN)); sW = min(sW,s); sN = min(sN,s); }
                    mN *= sN; mE *= sE; mS *= sS; mW *= sW;
                }

                // Diagonal masks: smooth union of two edges via SmoothMin
                float diagTW = tw * 1.15;
                float mNE = 0.0, mSE = 0.0, mSW = 0.0, mNW = 0.0;
                if ((neighborMask & 16) != 0)
                {
                    float dN = EdgeMask(localTileUV01, diagTW, 0);
                    float dE = EdgeMask(localTileUV01, diagTW, 1);
                    mNE = 1.0 - SmoothMin(1.0 - dN, 1.0 - dE, sk);
                    // Suppress where cardinal already covers
                    mNE *= (1.0 - mN) * (1.0 - mE);
                    mNE *= saturate(_DiagonalWeight);
                }
                if ((neighborMask & 32) != 0)
                {
                    float dE = EdgeMask(localTileUV01, diagTW, 1);
                    float dS = EdgeMask(localTileUV01, diagTW, 2);
                    mSE = 1.0 - SmoothMin(1.0 - dE, 1.0 - dS, sk);
                    mSE *= (1.0 - mE) * (1.0 - mS);
                    mSE *= saturate(_DiagonalWeight);
                }
                if ((neighborMask & 64) != 0)
                {
                    float dS = EdgeMask(localTileUV01, diagTW, 2);
                    float dW = EdgeMask(localTileUV01, diagTW, 3);
                    mSW = 1.0 - SmoothMin(1.0 - dS, 1.0 - dW, sk);
                    mSW *= (1.0 - mS) * (1.0 - mW);
                    mSW *= saturate(_DiagonalWeight);
                }
                if ((neighborMask & 128) != 0)
                {
                    float dW = EdgeMask(localTileUV01, diagTW, 3);
                    float dN = EdgeMask(localTileUV01, diagTW, 0);
                    mNW = 1.0 - SmoothMin(1.0 - dW, 1.0 - dN, sk);
                    mNW *= (1.0 - mW) * (1.0 - mN);
                    mNW *= saturate(_DiagonalWeight);
                }

                // Total mask weight before local contact falloff.
                float totalNeighborWeight = mN + mE + mS + mW + mNE + mSE + mSW + mNW;

                // Local contact weights define how far land texture can intrude into water.
                // This keeps only a thin border strip and a tiny diagonal corner patch.
                float cardWidth = max(_ContactWidth, 0.001);
                float diagWidth = max(_DiagonalContactWidth, 0.001);
                float cardStrength = saturate(_CardinalBlendStrength);
                float diagStrength = saturate(_DiagonalBlendStrength);

                float edgeN = 1.0 - smoothstep(0.0, cardWidth, 1.0 - localTileUV01.y);
                float edgeE = 1.0 - smoothstep(0.0, cardWidth, 1.0 - localTileUV01.x);
                float edgeS = 1.0 - smoothstep(0.0, cardWidth, localTileUV01.y);
                float edgeW = 1.0 - smoothstep(0.0, cardWidth, localTileUV01.x);

                float cornerNE = 1.0 - smoothstep(0.0, diagWidth, length(float2(1.0 - localTileUV01.x, 1.0 - localTileUV01.y)));
                float cornerSE = 1.0 - smoothstep(0.0, diagWidth, length(float2(1.0 - localTileUV01.x, localTileUV01.y)));
                float cornerSW = 1.0 - smoothstep(0.0, diagWidth, length(localTileUV01));
                float cornerNW = 1.0 - smoothstep(0.0, diagWidth, length(float2(localTileUV01.x, 1.0 - localTileUV01.y)));

                float wN  = mN  * edgeN    * cardStrength;
                float wE  = mE  * edgeE    * cardStrength;
                float wS  = mS  * edgeS    * cardStrength;
                float wW  = mW  * edgeW    * cardStrength;
                float wNE = mNE * cornerNE * diagStrength;
                float wSE = mSE * cornerSE * diagStrength;
                float wSW = mSW * cornerSW * diagStrength;
                float wNW = mNW * cornerNW * diagStrength;
                float totalContactWeight = wN + wE + wS + wW + wNE + wSE + wSW + wNW;

                // ═══ 5. SAMPLE NEIGHBOR TEXTURES & NORMALIZED BLEND ═══
                float4 rectN  = GetAtlasRect(id_N);
                float4 rectE  = GetAtlasRect(id_E);
                float4 rectS  = GetAtlasRect(id_S);
                float4 rectW  = GetAtlasRect(id_W);
                float4 rectNE = GetAtlasRect(id_NE);
                float4 rectSE = GetAtlasRect(id_SE);
                float4 rectSW = GetAtlasRect(id_SW);
                float4 rectNW = GetAtlasRect(id_NW);

                // Each neighbor is sampled with a mirrored UV so the face of the
                // neighbor texture that is closest to us appears at our tile edge:
                //   N/S: y is flipped   (0 of neighbor = edge towards us)
                //   E/W: x is flipped
                //   Diagonals: both x and y are flipped
                float2 flipV  = float2(localTileUV01.x,       1.0 - localTileUV01.y);
                float2 flipH  = float2(1.0 - localTileUV01.x, localTileUV01.y      );
                float2 flipHV = float2(1.0 - localTileUV01.x, 1.0 - localTileUV01.y);

                float3 cN  = SampleAtlasRect(rectN,  flipV ).rgb;
                float3 cE  = SampleAtlasRect(rectE,  flipH ).rgb;
                float3 cS  = SampleAtlasRect(rectS,  flipV ).rgb;
                float3 cW  = SampleAtlasRect(rectW,  flipH ).rgb;
                float3 cNE = SampleAtlasRect(rectNE, flipHV).rgb;
                float3 cSE = SampleAtlasRect(rectSE, flipHV).rgb;
                float3 cSW = SampleAtlasRect(rectSW, flipHV).rgb;
                float3 cNW = SampleAtlasRect(rectNW, flipHV).rgb;

                // Normalized weighted blend: sum all neighbor contributions first,
                // then lerp water toward the weighted average in one step.
                // This avoids sequential-lerp order dependency which caused dark grass:
                //   seq-lerp with N=0.4 then E=0.4 would leave waterCol with only
                //   (1-0.4)*(1-0.4)=36% original weight instead of the intended 60%.
                if (totalContactWeight > 0.001)
                {
                    float shoreBlend = 1.0 - smoothstep(0.0, max(_ShoreBlendWidth, 0.001), visualShoreDist);
                    float depthFade  = saturate(1.0 - depthT);
                    float globalScale = shoreBlend * depthFade;

                    float3 neighborSum  = float3(0, 0, 0);
                    float  neighborTotalW = 0.0;
                    if (wN  > 0.001) { neighborSum += cN  * wN;  neighborTotalW += wN;  }
                    if (wE  > 0.001) { neighborSum += cE  * wE;  neighborTotalW += wE;  }
                    if (wS  > 0.001) { neighborSum += cS  * wS;  neighborTotalW += wS;  }
                    if (wW  > 0.001) { neighborSum += cW  * wW;  neighborTotalW += wW;  }
                    if (wNE > 0.001) { neighborSum += cNE * wNE; neighborTotalW += wNE; }
                    if (wSE > 0.001) { neighborSum += cSE * wSE; neighborTotalW += wSE; }
                    if (wSW > 0.001) { neighborSum += cSW * wSW; neighborTotalW += wSW; }
                    if (wNW > 0.001) { neighborSum += cNW * wNW; neighborTotalW += wNW; }

                    float3 neighborAvg = neighborSum / max(neighborTotalW, 0.001);
                    // Keep water dominant in the center, while allowing clear border contact.
                    float blendFactor = saturate(min(neighborTotalW, 1.0) * globalScale * 0.92);
                    blendFactor *= (1.0 - coreMask * saturate(_CoreNeighborSuppression));
                    waterCol = lerp(waterCol, neighborAvg, blendFactor);
                }

                // ═══ 6. FOAM / SHORELINE with wave animation ═══
                float fw = _FoamWidth;
                float fs = _FoamSoftness;
                float hasBoundary = (shoreMask != 0 && shoreDist <= 2.5) ? 1.0 : 0.0;

                // Build signed-distance shore field from 8 neighbors
                float shoreFieldEdge = 10.0;
                float fN = (shoreMask & 1)   ? (1.0 - localTileUV01.y) : 10.0;
                float fE = (shoreMask & 2)   ? (1.0 - localTileUV01.x) : 10.0;
                float fS = (shoreMask & 4)   ? localTileUV01.y         : 10.0;
                float fW = (shoreMask & 8)   ? localTileUV01.x         : 10.0;
                shoreFieldEdge = min(min(fN, fE), min(fS, fW));

                // Diagonal corners via SmoothMin for rounded curves
                float shoreFieldCorner = 10.0;
                // NE corner only when both cardinals are present; diagonal-only foam created white wedges.
                if ((shoreMask & 3) == 3)
                    shoreFieldCorner = SmoothMin(shoreFieldCorner, length(float2(1.0 - localTileUV01.x, 1.0 - localTileUV01.y)), sk);
                // SE corner
                if ((shoreMask & 6) == 6)
                    shoreFieldCorner = SmoothMin(shoreFieldCorner, length(float2(1.0 - localTileUV01.x, localTileUV01.y)), sk);
                // SW corner
                if ((shoreMask & 12) == 12)
                    shoreFieldCorner = SmoothMin(shoreFieldCorner, length(localTileUV01), sk);
                // NW corner
                if ((shoreMask & 9) == 9)
                    shoreFieldCorner = SmoothMin(shoreFieldCorner, length(float2(localTileUV01.x, 1.0 - localTileUV01.y)), sk);

                float shoreField = SmoothMin(shoreFieldEdge, shoreFieldCorner, sk);

                // Smooth shore flow direction
                float2 shoreDir = shoreFlowVec;
                float shoreDirLen = length(shoreDir);
                if (shoreDirLen > 0.0001)
                {
                    shoreDir /= shoreDirLen;
                    float2 tileCentered = localTileUV01 - 0.5;
                    float directionalField = saturate(0.5 - dot(tileCentered, shoreDir));
                    shoreField = lerp(shoreField, directionalField, 0.35);
                }

                // Noise distortion on shore edge (multi-octave + animated)
                float shoreDistortion = (GradNoise(worldUV + float2(13.1, 4.7), 3.5) - 0.5) * 0.10;
                shoreDistortion += (GradNoise(worldUV + float2(2.4, 9.2), 7.2) - 0.5) * 0.05;
                shoreDistortion += (GradNoise(worldUV + float2(6.8, 2.1) + float2(time * 0.04, 0.0), 5.0) - 0.5) * 0.04;
                shoreField += shoreDistortion;

                // Wave animation for foam: oscillate foam threshold over time (per-tile phase)
                float phaseNoise = GradNoise(worldUV + float2(4.2, 9.7), 0.35) * 6.2831853;
                float waveOffset = sin(time * _FoamWaveSpeed + phaseNoise + worldUV.x * _FoamWaveFrequency * 0.5)
                                 * _FoamWaveAmplitude;
                waveOffset += sin(time * _FoamWaveSpeed * 0.7 + phaseNoise * 0.5 + worldUV.y * _FoamWaveFrequency)
                            * _FoamWaveAmplitude * 0.6;

                float foamMask = 1.0 - smoothstep(fw + waveOffset, fw + fs + waveOffset, shoreField);
                // Multi-octave foam noise
                float foamNoise = GradNoise(worldUV + float2(21.7, 8.3), 8.0) * 0.5
                                + GradNoise(worldUV + float2(7.3, 15.1) + float2(time * 0.03, 0.0), 16.0) * 0.3
                                + GradNoise(worldUV + float2(3.8, 11.2) + float2(0.0, time * 0.02), 32.0) * 0.2;
                foamNoise = lerp(0.6, 1.0, foamNoise);
                foamMask *= hasBoundary;
                foamMask *= saturate(1.0 - totalContactWeight * 0.65);
                foamMask *= saturate(1.0 - depthT * 0.5);
                foamMask *= 1.0 - smoothstep(0.18, 0.55, depthT);
                foamMask *= foamNoise;
                foamMask *= (1.0 - coreMask * saturate(_CoreFoamSuppression));
                foamMask = saturate(foamMask);

                waterCol = lerp(waterCol, _FoamColor.rgb, foamMask * _FoamColor.a);

                // Water is always fully opaque — transparency was making the
                // background bleed through at shore edges. The visual shore
                // transition is handled entirely by the neighbor texture blend above.

                // ═══ OUTPUT ═══
                return half4(waterCol, spriteAlpha);
            }
            ENDHLSL
        }
    }

    FallBack "Sprites/Default"
}
