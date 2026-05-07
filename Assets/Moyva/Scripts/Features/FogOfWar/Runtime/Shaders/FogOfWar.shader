Shader "Moyva/FogOfWar"
{
    Properties
    {
        _FogTex                  ("Fog Visibility Texture (R8)", 2D)    = "black" {}
        _FogTileTex              ("Fog Tile Texture", 2D)              = "white" {}
        _FogIconTex              ("Fog Icon Texture", 2D)              = "white" {}
        _UnexploredColor         ("Unexplored Color", Color)           = (0, 0, 0, 1)
        _ExploredColor           ("Explored Color",   Color)           = (0, 0, 0, 0.5)
        _FogTileTiling           ("Fog Tile Tiling", Float)            = 1.0
        _FogIconScale            ("Fog Icon Scale", Float)             = 0.5
        _FogIconIntensity        ("Icon Blend Intensity", Float)       = 0.5
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
                float4 _FogTileTex_ST;
                float4 _FogIconTex_ST;
                float4 _UnexploredColor;
                float4 _ExploredColor;
                float _FogTileTiling;
                float _FogIconScale;
                float _FogIconIntensity;
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

                // ─── Sample tile texture with tiling ────────────────────────────
                float2 tiledUV = IN.uv * _FogTileTiling;
                half4 tileSample = SAMPLE_TEXTURE2D(_FogTileTex, sampler_FogTileTex, tiledUV);

                // ─── Sample icon texture with cell-based indexing ───────────────
                // Determine which cell we are in (based on tiling)
                float2 cellCoord = floor(IN.uv * _FogTileTiling);
                
                // Create deterministic pattern from cell coordinates
                // Use modulo to cycle through icons in a regular pattern
                float iconIndex = mod(cellCoord.x + cellCoord.y * 2.0, _IconGridSize * _IconGridSize);
                
                // Convert icon index to atlas UV coordinates (assuming NxN grid)
                float gridCols = _IconGridSize;
                float gridRows = _IconGridSize;
                float atlasX = mod(iconIndex, gridCols) / gridCols;
                float atlasY = floor(iconIndex / gridCols) / gridRows;
                
                // Cell fractional coordinate (position within cell)
                float2 cellFrac = frac(IN.uv * _FogTileTiling);
                
                // Scale cell fractional to icon size
                float2 iconUVInAtlas = cellFrac * _FogIconScale;
                iconUVInAtlas += float2(0.5 - _FogIconScale * 0.5, 0.5 - _FogIconScale * 0.5); // center within atlas cell
                
                // Scale to atlas cell coordinates
                float2 iconUV = atlasX + iconUVInAtlas / gridCols;
                iconUV.y = atlasY + iconUVInAtlas.y / gridRows;
                
                half4 iconSample = SAMPLE_TEXTURE2D(_FogIconTex, sampler_FogIconTex, iconUV);

                // ─── Blend tile and icon ──────────────────────────────────────
                half4 tileCol = tileSample;
                half4 blended = lerp(tileCol, iconSample, iconSample.a * _FogIconIntensity);

                // ─── Apply fog state coloring ────────────────────────────────
                half4 finalCol;
                finalCol.rgb = _UnexploredColor.rgb * isUnexplored + _ExploredColor.rgb * isExplored;
                
                // Blend with tile texture when not visible
                finalCol.rgb = lerp(finalCol.rgb, blended.rgb, (isExplored + isUnexplored) * 0.3);

                // ─── Apply transparency based on fog state ────────────────────
                finalCol.a = _UnexploredAlpha * isUnexplored + _ExploredAlpha * isExplored;
                
                // Fully transparent if visible
                finalCol.a *= (1.0 - isVisible);

                return finalCol;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
