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
                float2 cellCoord = floor(IN.uv * fogGridSize);
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

                // Pick icon rect sequentially with mirror symmetry across X/Y axes.
                float2 symCoord = min(iconGridCoord, iconGridMax - iconGridCoord);
                float symWidth = floor((iconGridSize.x + 1.0) * 0.5);
                float iconRectCount = max(1.0, _FogIconRectCount);
                float iconIndexF = fmod(symCoord.x + symCoord.y * symWidth, iconRectCount);
                int iconIndex = (int)iconIndexF;
                iconIndex = clamp(iconIndex, 0, 63);
                float4 iconUvRect = _FogIconUVRects[iconIndex];
                if (iconUvRect.z <= 0.0001 || iconUvRect.w <= 0.0001)
                    iconUvRect = _FogIconUVRect;
                
                // Scale cell fractional to icon size and center within icon cell
                float2 iconUVInSprite = iconCellFrac * _FogIconScale;
                iconUVInSprite += float2(0.5 - _FogIconScale * 0.5, 0.5 - _FogIconScale * 0.5);

                // Sample exact sprite rect from atlas texture
                float2 iconUV = iconUvRect.xy + iconUVInSprite * iconUvRect.zw;
                
                half4 iconSample = SAMPLE_TEXTURE2D(_FogIconTex, sampler_FogIconTex, iconUV);

                // ─── Blend tile and icon ──────────────────────────────────────
                half4 tileCol = tileSample;
                half4 blended = lerp(tileCol, iconSample, iconSample.a * _FogIconIntensity);

                // ─── Apply fog state coloring ────────────────────────────────
                half4 finalCol;
                finalCol.rgb = _UnexploredColor.rgb * isUnexplored + _ExploredColor.rgb * isExplored;
                float fogStateWeight = (isExplored + isUnexplored);
                float patternAlpha = saturate(blended.a);
                
                // Blend with tile texture when not visible
                finalCol.rgb = lerp(finalCol.rgb, blended.rgb, fogStateWeight * patternAlpha);

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
