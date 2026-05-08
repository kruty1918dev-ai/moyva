Shader "Moyva/FogOfWar"
{
    Properties
    {
        _FogTex                  ("Fog Visibility Texture (R8)", 2D)    = "black" {}
        _FogTileTex              ("Fog Tile Texture", 2D)              = "white" {}
        _FogIconTex              ("Fog Icon Texture", 2D)              = "white" {}
        _FogTileUVRect           ("Fog Tile UV Rect", Vector)          = (0, 0, 1, 1)
        _FogIconUVRect           ("Fog Icon UV Rect", Vector)          = (0, 0, 1, 1)
        _FogTileSpritePixelSize  ("Fog Tile Sprite Pixel Size", Vector)= (16, 16, 0, 0)
        _FogTileSizeInCells      ("Fog Tile Size In Cells", Vector)    = (1, 1, 0, 0)
        _FogTileSeamOverlapPixels("Fog Tile Seam Overlap Pixels", Float)= 1
        _UnexploredColor         ("Unexplored Color", Color)           = (0, 0, 0, 1)
        _ExploredColor           ("Explored Color",   Color)           = (0, 0, 0, 0.5)
        _FogTileTiling           ("Fog Tile Tiling", Float)            = 1.0
        _FogIconScale            ("Fog Icon Scale", Float)             = 0.5
        _FogIconIntensity        ("Icon Blend Intensity", Float)       = 0.5
        _UseFogIcons             ("Use Fog Icons", Float)              = 0
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

            CBUFFER_START(UnityPerMaterial)
                float4 _FogTex_ST;
                float4 _FogTex_TexelSize;
                float4 _FogTileTex_ST;
                float4 _FogIconTex_ST;
                float4 _FogTileUVRect;
                float4 _FogIconUVRect;
                float4 _FogTileSpritePixelSize;
                float4 _FogTileSizeInCells;
                float4 _FogIconGridSize;
                float4 _UnexploredColor;
                float4 _ExploredColor;
                float _FogTileTiling;
                float _FogTileSeamOverlapPixels;
                float _FogIconScale;
                float _FogIconIntensity;
                float _UseFogIcons;
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

            float4 BuildFogState(float fogVal)
            {
                float isVisible = step(0.9, fogVal);
                float isExplored = step(0.3, fogVal) * (1.0 - isVisible);
                float isUnexplored = 1.0 - step(0.3, fogVal);
                return float4(isVisible, isExplored, isUnexplored, isExplored + isUnexplored);
            }

            half4 TintTileByFog(half4 tileSample, float4 fogState)
            {
                float3 fogTint = _UnexploredColor.rgb * fogState.z + _ExploredColor.rgb * fogState.y;
                float tintWeight = fogState.w;
                float3 tintedRgb = lerp(tileSample.rgb, tileSample.rgb * fogTint, tintWeight);
                float alpha = (_UnexploredAlpha * fogState.z + _ExploredAlpha * fogState.y) * tileSample.a;
                alpha *= (1.0 - fogState.x);
                return half4(tintedRgb, alpha);
            }

            half4 BlendWithoutAlphaAccumulation(half4 bottom, half4 top)
            {
                half outAlpha = max(bottom.a, top.a);
                half topWeight = top.a > 0.0001 ? saturate(top.a / max(0.0001, bottom.a + top.a)) : 0.0;
                half3 outRgb = lerp(bottom.rgb, top.rgb, topWeight);
                return half4(outRgb, outAlpha);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 fogGridSize = max(1.0.xx, rcp(_FogTex_TexelSize.xy));
                float2 fogCoord = IN.uv * fogGridSize;
                float2 baseCell = floor(fogCoord);
                float2 tileSize = max(0.001.xx, _FogTileSizeInCells.xy);
                float2 tileHalfTexel = 0.5 / max(1.0.xx, _FogTileSpritePixelSize.xy);
                float2 seamOverlapUV = _FogTileSeamOverlapPixels.xx / max(1.0.xx, _FogTileSpritePixelSize.xy);
                half4 blended = half4(0.0, 0.0, 0.0, 0.0);

                for (int y = -4; y <= 4; y++)
                {
                    for (int x = -4; x <= 4; x++)
                    {
                        float2 cell = baseCell + float2(x, y);
                        float inBounds = step(0.0, cell.x) * step(0.0, cell.y) *
                                         step(cell.x, fogGridSize.x - 1.0) * step(cell.y, fogGridSize.y - 1.0);

                        float2 spriteUVInCells = (fogCoord - (cell + 0.5.xx)) / tileSize + 0.5.xx;
                        float inside = step(-seamOverlapUV.x, spriteUVInCells.x) * step(spriteUVInCells.x, 1.0 + seamOverlapUV.x) *
                                       step(-seamOverlapUV.y, spriteUVInCells.y) * step(spriteUVInCells.y, 1.0 + seamOverlapUV.y) * inBounds;

                        float2 clampedSpriteUV = min(saturate(spriteUVInCells), 0.9999.xx);
                        float2 tiledUV = frac(clampedSpriteUV * _FogTileTiling);
                        tiledUV = lerp(tileHalfTexel, 1.0.xx - tileHalfTexel, tiledUV);
                        float2 tileSpriteUV = _FogTileUVRect.xy + tiledUV * _FogTileUVRect.zw;
                        half4 tileSample = SAMPLE_TEXTURE2D(_FogTileTex, sampler_FogTileTex, tileSpriteUV) * inside;

                        float2 fogSampleUV = (cell + 0.5.xx) / fogGridSize;
                        float4 fogState = BuildFogState(SAMPLE_TEXTURE2D(_FogTex, sampler_FogTex, fogSampleUV).r);
                        blended = BlendWithoutAlphaAccumulation(blended, TintTileByFog(tileSample, fogState));
                    }
                }

                // ─── Sample icon texture with independent icon grid ─────────────
                float2 iconGridSize = max(1.0.xx, _FogIconGridSize.xy);
                float2 iconCellFrac = frac(IN.uv * iconGridSize);
                
                // Scale cell fractional to icon size and center within icon cell
                float2 iconUVInSprite = iconCellFrac * _FogIconScale;
                iconUVInSprite += float2(0.5 - _FogIconScale * 0.5, 0.5 - _FogIconScale * 0.5);

                // Sample exact sprite rect from atlas texture
                float2 iconUV = _FogIconUVRect.xy + iconUVInSprite * _FogIconUVRect.zw;
                
                half4 iconSample = SAMPLE_TEXTURE2D(_FogIconTex, sampler_FogIconTex, iconUV);
                iconSample *= step(0.5, _UseFogIcons);
                float4 currentFogState = BuildFogState(SAMPLE_TEXTURE2D(_FogTex, sampler_FogTex, IN.uv).r);
                iconSample.a *= (_UnexploredAlpha * currentFogState.z + _ExploredAlpha * currentFogState.y) * (1.0 - currentFogState.x);

                // ─── Blend tile and icon ──────────────────────────────────────
                iconSample.a *= _FogIconIntensity;
                blended = BlendWithoutAlphaAccumulation(blended, iconSample);
                return blended;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
