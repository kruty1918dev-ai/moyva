Shader "Moyva/FogOfWar"
{
    Properties
    {
        _FogTex                  ("Fog Visibility Texture (R8)", 2D)    = "black" {}
        _FogTileTex              ("Fog Tile Texture", 2D)              = "white" {}
        _FogIconTex              ("Fog Icon Texture", 2D)              = "white" {}
        _FogTileUVRect           ("Fog Tile UV Rect", Vector)          = (0, 0, 1, 1)
        _FogIconUVRect           ("Fog Icon UV Rect", Vector)          = (0, 0, 1, 1)
        _FogIconRectCount        ("Fog Icon Rect Count", Float)        = 1
        _UnexploredColor         ("Unexplored Color", Color)           = (0, 0, 0, 1)
        _ExploredColor           ("Explored Color",   Color)           = (0, 0, 0, 0.5)
        _FogTileTiling           ("Fog Tile Tiling", Float)            = 1.0
        _FogIconScale            ("Fog Icon Scale", Float)             = 0.5
        _FogIconSeed             ("Fog Icon Seed", Float)              = 1918
        _FogIconDensity          ("Fog Icon Density", Range(0, 1))     = 0.85
        _FogIconIntensity        ("Icon Blend Intensity", Float)       = 0.5
        _FogIconGridSize         ("Icon Grid Size XY", Vector)         = (10, 10, 0, 0)
        _UnexploredAlpha         ("Unexplored Alpha", Range(0, 1))    = 1.0
        _ExploredAlpha           ("Explored Alpha", Range(0, 1))      = 0.5
        _UseIconAtlas            ("Use Icon Atlas (1=yes)", Float)     = 0
        _IconGridSize            ("Icon Grid Size (atlas cols)", Float) = 4
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Transparent"
            "Queue"          = "Overlay"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "FogOverlay"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_FogTex);
            SAMPLER(sampler_FogTex);
            TEXTURE2D(_FogTileTex);
            SAMPLER(sampler_FogTileTex);
            TEXTURE2D(_FogIconTex);
            SAMPLER(sampler_FogIconTex);
            float4 _FogIconUVRects[64];

            CBUFFER_START(UnityPerMaterial)
                float4 _FogTex_ST;
                float4 _FogTex_TexelSize;
                float4 _FogTileTex_ST;
                float4 _FogIconTex_ST;
                float4 _FogTileUVRect;
                float4 _FogIconUVRect;
                float4 _FogIconGridSize;
                float4 _UnexploredColor;
                float4 _ExploredColor;
                float _FogTileTiling;
                float _FogIconScale;
                float _FogIconSeed;
                float _FogIconDensity;
                float _FogIconIntensity;
                float _FogIconRectCount;
                float _UnexploredAlpha;
                float _ExploredAlpha;
                float _UseIconAtlas;
                float _IconGridSize;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 screenPos   : TEXCOORD1;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _FogTex);
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                return OUT;
            }

            float2 Hash22(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * float3(0.1031, 0.1030, 0.0973));
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.xx + p3.yz) * p3.zy);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Sample fog visibility: 0 = unexplored, ~0.5 = explored, 1.0 = visible
                float fogVal = SAMPLE_TEXTURE2D(_FogTex, sampler_FogTex, IN.uv).r;

                // Determine fog state
                float isVisible = step(0.9, fogVal);
                float isExplored = step(0.3, fogVal) * (1.0 - isVisible);
                float isUnexplored = 1.0 - step(0.3, fogVal);

                // ─── Build fog-cell coordinates from fog texture resolution ─────
                // _FogTex_TexelSize.xy = (1/width, 1/height), so inverse gives grid size.
                float2 fogGridSize = max(1.0.xx, rcp(_FogTex_TexelSize.xy));
                float2 cellFrac = frac(IN.uv * fogGridSize);

                // ─── Sample tile texture once per fog cell ───────────────────────
                // FogTileTiling controls detail inside each cell (1 = one full sprite per cell).
                float2 tiledUV = frac(cellFrac * _FogTileTiling);
                float2 tileSpriteUV = _FogTileUVRect.xy + tiledUV * _FogTileUVRect.zw;
                half4 tileSample = SAMPLE_TEXTURE2D(_FogTileTex, sampler_FogTileTex, tileSpriteUV);

                // ─── Sample icon texture with independent icon grid ─────────────
                float2 iconGridSize = max(1.0.xx, _FogIconGridSize.xy);
                float2 iconGridCoord = floor(IN.uv * iconGridSize);
                float2 iconGridMax = max(0.0.xx, iconGridSize - 1.0.xx);
                iconGridCoord = clamp(iconGridCoord, 0.0.xx, iconGridMax);
                float2 iconCellFrac = frac(IN.uv * iconGridSize);

                float hasIconRects = step(0.5, _FogIconRectCount);
                float iconRectCount = max(1.0, _FogIconRectCount);
                float2 seededIconCoord = iconGridCoord + _FogIconSeed.xx;
                float2 iconRand = Hash22(seededIconCoord);
                float iconWave = 0.5 + 0.5 * sin(dot(iconGridCoord, float2(0.73, 1.37)) + _FogIconSeed * 0.071);
                float iconSignal = frac(iconRand.x * 0.65 + iconWave * 0.35);
                float iconPresence = step(iconSignal, saturate(_FogIconDensity));
                float iconIndexPattern = iconGridCoord.x * 3.0 + iconGridCoord.y * 5.0 + floor(iconWave * 7.0) + _FogIconSeed;
                float iconIndexF = fmod(iconIndexPattern, iconRectCount);
                int iconIndex = (int)iconIndexF;
                iconIndex = clamp(iconIndex, 0, 63);
                float4 iconUvRect = _FogIconUVRects[iconIndex];
                if (iconUvRect.z <= 0.0001 || iconUvRect.w <= 0.0001)
                    iconUvRect = _FogIconUVRect;
                
                // Icons stay on the grid; only visibility and variant selection are patterned.
                float2 iconCenter = 0.5.xx;
                float2 iconUVInSprite = (iconCellFrac - iconCenter) / max(0.001, _FogIconScale) + 0.5.xx;
                float iconInside = step(0.0, iconUVInSprite.x) * step(iconUVInSprite.x, 1.0) *
                                   step(0.0, iconUVInSprite.y) * step(iconUVInSprite.y, 1.0);
                iconUVInSprite = saturate(iconUVInSprite);

                // Sample exact sprite rect from atlas texture
                float2 iconUV = iconUvRect.xy + iconUVInSprite * iconUvRect.zw;
                
                half4 iconSample = SAMPLE_TEXTURE2D(_FogIconTex, sampler_FogIconTex, iconUV);
                iconSample *= iconInside * iconPresence;

                // ─── Blend tile and icon ──────────────────────────────────────
                half4 tileCol = tileSample;
                half4 blended = lerp(tileCol, iconSample, iconSample.a * _FogIconIntensity * hasIconRects);

                // ─── Apply fog state tint ───────────────────────────────────
                // White tint keeps the sprite color unchanged; other colors tint it.
                half4 finalCol;
                float fogStateWeight = (isExplored + isUnexplored);
                float3 fogTint = _UnexploredColor.rgb * isUnexplored + _ExploredColor.rgb * isExplored;
                float patternAlpha = saturate(blended.a);
                finalCol.rgb = blended.rgb * lerp(1.0.xxx, fogTint, fogStateWeight);

                // ─── Apply transparency based on fog state ────────────────────
                finalCol.a = _UnexploredAlpha * isUnexplored + _ExploredAlpha * isExplored;
                finalCol.a *= patternAlpha;
                
                // Fully transparent if visible
                finalCol.a *= (1.0 - isVisible);

                return finalCol;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
