Shader "Moyva/Overlay/ConstructionBuildGrid"
{
    Properties
    {
        _LineColor ("Line Color", Color) = (0.70, 0.95, 1.00, 0.22)
        _FillColor ("Fill Color", Color) = (0.70, 0.95, 1.00, 0.045)
        _ValidLineColor ("Valid Line Color", Color) = (0.28, 1.00, 0.42, 0.22)
        _ValidFillColor ("Valid Fill Color", Color) = (0.20, 0.82, 0.32, 0.045)
        _InvalidLineColor ("Invalid Line Color", Color) = (1.00, 0.26, 0.22, 0.22)
        _InvalidFillColor ("Invalid Fill Color", Color) = (0.92, 0.12, 0.10, 0.045)
        _LineWidth ("Line Width", Range(0.005, 0.49)) = 0.035
        _EdgeMask ("Edge Mask", Vector) = (1, 1, 1, 1)
        _GridOriginXZ ("Grid Origin XZ", Vector) = (0, 0, 0, 0)
        _CellSizeXZ ("Cell Size XZ", Vector) = (1, 1, 0, 0)
        _ChunkTileOrigin ("Chunk Tile Origin", Vector) = (0, 0, 0, 0)
        _ChunkTileSize ("Chunk Tile Size", Vector) = (1, 1, 0, 0)
        _SurfaceLift ("Surface Lift", Range(0, 0.5)) = 0
        _MinUpNormalY ("Min Up Normal Y", Range(0, 1)) = 0.2
        _UseCellMask ("Use Cell Mask", Float) = 0
        [NoScaleOffset] _CellMaskTex ("Cell Mask", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Transparent"
            "Queue"          = "Transparent+990"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ConstructionBuildGrid"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off
            Offset -2, -2

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _LineColor;
                float4 _FillColor;
                float4 _ValidLineColor;
                float4 _ValidFillColor;
                float4 _InvalidLineColor;
                float4 _InvalidFillColor;
                float4 _EdgeMask;
                float4 _GridOriginXZ;
                float4 _CellSizeXZ;
                float4 _ChunkTileOrigin;
                float4 _ChunkTileSize;
                float _LineWidth;
                float _SurfaceLift;
                float _MinUpNormalY;
                float _UseCellMask;
            CBUFFER_END

            TEXTURE2D(_CellMaskTex);
            SAMPLER(sampler_CellMaskTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 worldXZ : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                float3 worldNormal = SafeNormalize(TransformObjectToWorldNormal(IN.normalOS));
                worldPos += worldNormal * max(_SurfaceLift, 0.0);

                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.worldXZ = worldPos.xz;
                OUT.normalWS = worldNormal;
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float2 cellSize = max(_CellSizeXZ.xy, float2(0.0001, 0.0001));
                float2 gridCoords = (IN.worldXZ - _GridOriginXZ.xy) / cellSize;
                float2 cellUv = IN.uv;

                float4 lineColor = _LineColor;
                float4 fillColor = _FillColor;
                if (_UseCellMask > 0.5)
                {
                    clip(IN.normalWS.y - _MinUpNormalY);
                    cellUv = frac(gridCoords);
                    float2 chunkSize = max(_ChunkTileSize.xy, float2(1.0, 1.0));
                    float2 tileCoord = floor(gridCoords);
                    float2 localTile = tileCoord - _ChunkTileOrigin.xy;
                    float2 insideMask = step(0.0, localTile) * step(localTile, chunkSize - 0.001);
                    clip(insideMask.x * insideMask.y - 0.5);

                    float2 maskUv = (localTile + 0.5) / chunkSize;
                    float mask = SAMPLE_TEXTURE2D(_CellMaskTex, sampler_CellMaskTex, maskUv).r;
                    clip(mask - 0.16);

                    // R8 values: 1/3 = General, 2/3 = Invalid, 1 = Valid.
                    float invalidWeight = step(0.50, mask) * (1.0 - step(0.84, mask));
                    float validWeight = step(0.84, mask);
                    lineColor = lerp(lineColor, _InvalidLineColor, invalidWeight);
                    fillColor = lerp(fillColor, _InvalidFillColor, invalidWeight);
                    lineColor = lerp(lineColor, _ValidLineColor, validWeight);
                    fillColor = lerp(fillColor, _ValidFillColor, validWeight);
                }

                float feather = max(fwidth(cellUv.x) + fwidth(cellUv.y), 0.0015);
                float leftLine = 1.0 - smoothstep(_LineWidth - feather, _LineWidth + feather, cellUv.x);
                float bottomLine = 1.0 - smoothstep(_LineWidth - feather, _LineWidth + feather, cellUv.y);
                float rightLine = 1.0 - smoothstep(_LineWidth - feather, _LineWidth + feather, 1.0 - cellUv.x);
                float topLine = 1.0 - smoothstep(_LineWidth - feather, _LineWidth + feather, 1.0 - cellUv.y);
                float lineMask = max(
                    max(leftLine * _EdgeMask.x, bottomLine * _EdgeMask.y),
                    max(rightLine * _EdgeMask.z, topLine * _EdgeMask.w));

                float3 rgb = lerp(fillColor.rgb, lineColor.rgb, lineMask);
                float alpha = max(fillColor.a, lineColor.a * lineMask);
                return float4(rgb, alpha);
            }
            ENDHLSL
        }
    }
}
