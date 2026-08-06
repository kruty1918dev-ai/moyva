Shader "Moyva/FogOfWar/ScreenSpace"
{
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
        }

        ZWrite Off
        ZTest Always
        Cull Off

        HLSLINCLUDE

        #pragma target 3.5

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        TEXTURE2D(_MoyvaFogStateTexture);
        SAMPLER(sampler_MoyvaFogStateTexture);

        TEXTURE2D_X_FLOAT(_MoyvaFogSurfaceEyeDepthTexture);
        SAMPLER(sampler_MoyvaFogSurfaceEyeDepthTexture);

        TEXTURE2D_X(_MoyvaFogScreenStateRawTexture);
        SAMPLER(sampler_MoyvaFogScreenStateRawTexture);

        TEXTURE2D_X(_MoyvaFogScreenStateTexture);
        SAMPLER(sampler_MoyvaFogScreenStateTexture);

        float4 _MoyvaFogMapSize;
        float4 _MoyvaFogGridOrigin;
        float4 _MoyvaFogWorldToGrid;

        float4 _MoyvaFogUnexploredColor;
        float4 _MoyvaFogExploredColor;
        float4 _MoyvaFogVirtualDepthColor;

        float _MoyvaFogEnabled;
        float _MoyvaFogFlipY;
        float _MoyvaFogUnexploredOpacity;
        float _MoyvaFogExploredOpacity;
        float _MoyvaFogUnexploredSaturation;
        float _MoyvaFogExploredSaturation;
        float _MoyvaFogEdgeSoftness;
        float _MoyvaFogEdgeNoiseStrength;

        float _MoyvaFogVirtualDepthEnabled;
        float _MoyvaFogVirtualDepthOpacity;
        float _MoyvaFogVirtualDepthWorld;
        float _MoyvaFogVirtualDepthMinPixels;
        float _MoyvaFogVirtualDepthMaxPixels;
        float _MoyvaFogVirtualDepthSamples;
        float _MoyvaFogVirtualDepthOcclusionBias;
        float _MoyvaFogVirtualDepthGradientPower;

        float _MoyvaFogScreenCloseRadiusPixels;
        float _MoyvaFogScreenBoundarySoftnessPixels;
        float4 _MoyvaFogScreenStateTexelSize;

        float _MoyvaFogDebugMode;
        float _MoyvaFogDebugGridLineWidthPixels;

        float2 ResolveFogTexelUv(float2 cell)
        {
            float2 mapSize =
                max(
                    _MoyvaFogMapSize.xy,
                    1.0.xx);

            cell =
                clamp(
                    cell,
                    0.0.xx,
                    mapSize - 1.0.xx);

            return (cell + 0.5.xx) / mapSize;
        }

        float2 SampleFogMasks(float2 cell)
        {
            float2 uv =
                ResolveFogTexelUv(cell);

            return SAMPLE_TEXTURE2D_LOD(
                _MoyvaFogStateTexture,
                sampler_MoyvaFogStateTexture,
                uv,
                0).rg;
        }

        float2 WorldToFogGrid(float3 worldPosition)
        {
            float2 deltaXZ =
                worldPosition.xz
                - _MoyvaFogGridOrigin.xy;

            float2 gridPosition;

            gridPosition.x =
                dot(
                    deltaXZ,
                    _MoyvaFogWorldToGrid.xy);

            gridPosition.y =
                dot(
                    deltaXZ,
                    _MoyvaFogWorldToGrid.zw);

            if (_MoyvaFogFlipY > 0.5)
            {
                gridPosition.y =
                    (_MoyvaFogMapSize.y - 1.0)
                    - gridPosition.y;
            }

            return gridPosition;
        }

        float HashNoise(float2 position)
        {
            return frac(
                sin(
                    dot(
                        position,
                        float2(12.9898, 78.233)))
                * 43758.5453);
        }

        float2 ResolveSmoothedFogMasks(
            float2 gridPosition,
            float3 worldPosition)
        {
            float2 baseCell =
                floor(gridPosition);

            float2 fraction =
                frac(gridPosition);

            float softness =
                saturate(
                    _MoyvaFogEdgeSoftness);

            /*
             * Zoom-stable analytical antialiasing.
             *
             * Коли клітинка стає меншою на екрані, fwidth(gridPosition)
             * автоматично розширює transition у межах одного пікселя.
             * Тому fog edge не мерехтить і не розсипається при zoom out.
             */
            float2 gridFootprint =
                max(
                    fwidth(gridPosition),
                    0.0001.xx);

            float2 halfWidth =
                max(
                    0.0001.xx,
                    max(
                        softness.xx,
                        gridFootprint)
                    * 0.5);

            float maximumFootprint =
                max(
                    gridFootprint.x,
                    gridFootprint.y);

            /*
             * World-noise корисний зблизька, але на віддаленні стає
             * sub-pixel шумом. Плавно вимикаємо його при великому footprint.
             */
            float noiseVisibility =
                1.0
                - smoothstep(
                    0.35,
                    1.35,
                    maximumFootprint);

            float noise =
                (HashNoise(worldPosition.xz * 0.37) - 0.5)
                * _MoyvaFogEdgeNoiseStrength
                * noiseVisibility;

            float2 blendFactor =
                smoothstep(
                    0.5.xx - halfWidth + noise.xx,
                    0.5.xx + halfWidth + noise.xx,
                    fraction);

            float2 masks00 =
                SampleFogMasks(baseCell);

            float2 masks10 =
                SampleFogMasks(baseCell + float2(1.0, 0.0));

            float2 masks01 =
                SampleFogMasks(baseCell + float2(0.0, 1.0));

            float2 masks11 =
                SampleFogMasks(baseCell + float2(1.0, 1.0));

            float2 masksBottom =
                lerp(
                    masks00,
                    masks10,
                    blendFactor.x);

            float2 masksTop =
                lerp(
                    masks01,
                    masks11,
                    blendFactor.x);

            return saturate(
                lerp(
                    masksBottom,
                    masksTop,
                    blendFactor.y));
        }

        float3 ApplySaturation(
            float3 color,
            float saturation)
        {
            float luminance =
                dot(
                    color,
                    float3(0.2126, 0.7152, 0.0722));

            return lerp(
                luminance.xxx,
                color,
                saturate(saturation));
        }

        half4 ResolveInfiniteUnexploredFog()
        {
            return half4(
                _MoyvaFogUnexploredColor.rgb,
                1.0);
        }

        void ResolveCameraRay(
            float2 screenUv,
            out float3 nearWorld,
            out float3 rayDirection)
        {
            #if UNITY_REVERSED_Z
                float nearClipDepth = 1.0;
                float farClipDepth = 0.0;
            #else
                float nearClipDepth =
                    UNITY_NEAR_CLIP_VALUE;
                float farClipDepth = 1.0;
            #endif

            nearWorld =
                ComputeWorldSpacePosition(
                    screenUv,
                    nearClipDepth,
                    UNITY_MATRIX_I_VP);

            float3 farWorld =
                ComputeWorldSpacePosition(
                    screenUv,
                    farClipDepth,
                    UNITY_MATRIX_I_VP);

            float3 rayVector =
                farWorld - nearWorld;

            float rayLengthSquared =
                max(
                    dot(rayVector, rayVector),
                    0.0000001);

            rayDirection =
                rayVector
                * rsqrt(rayLengthSquared);
        }

        bool TryResolveFallbackWorldPosition(
            float2 screenUv,
            out float3 worldPosition,
            out float eyeDepth)
        {
            float3 nearWorld;
            float3 rayDirection;

            ResolveCameraRay(
                screenUv,
                nearWorld,
                rayDirection);

            if (abs(rayDirection.y) <= 0.00001)
            {
                worldPosition = 0.0.xxx;
                eyeDepth = 0.0;
                return false;
            }

            float distanceToPlane =
                (_MoyvaFogGridOrigin.z - nearWorld.y)
                / rayDirection.y;

            if (distanceToPlane <= 0.0)
            {
                worldPosition = 0.0.xxx;
                eyeDepth = 0.0;
                return false;
            }

            worldPosition =
                nearWorld
                + rayDirection
                * distanceToPlane;

            float3 positionVS =
                TransformWorldToView(
                    worldPosition);

            eyeDepth =
                max(
                    0.0,
                    -positionVS.z);

            return true;
        }

        bool TryResolveWorldFromEyeDepth(
            float2 screenUv,
            float eyeDepth,
            out float3 worldPosition)
        {
            if (eyeDepth <= 0.0001)
            {
                float fallbackDepth;

                return TryResolveFallbackWorldPosition(
                    screenUv,
                    worldPosition,
                    fallbackDepth);
            }

            float3 nearWorld;
            float3 rayDirection;

            ResolveCameraRay(
                screenUv,
                nearWorld,
                rayDirection);

            float3 nearView =
                TransformWorldToView(
                    nearWorld);

            float3 directionView =
                mul(
                    (float3x3)UNITY_MATRIX_V,
                    rayDirection);

            float eyeDepthPerWorldUnit =
                -directionView.z;

            if (eyeDepthPerWorldUnit <= 0.00001)
            {
                worldPosition = 0.0.xxx;
                return false;
            }

            float nearEyeDepth =
                max(
                    0.0,
                    -nearView.z);

            float distanceAlongRay =
                (eyeDepth - nearEyeDepth)
                / eyeDepthPerWorldUnit;

            if (distanceAlongRay < 0.0)
            {
                worldPosition = 0.0.xxx;
                return false;
            }

            worldPosition =
                nearWorld
                + rayDirection
                * distanceAlongRay;

            return true;
        }

        float2 ResolveFogStateAtWorld(
            float3 worldPosition,
            out float insideMap)
        {
            float2 gridPosition =
                WorldToFogGrid(
                    worldPosition);

            float2 mapSize =
                max(
                    _MoyvaFogMapSize.xy,
                    1.0.xx);

            insideMap =
                step(-0.5, gridPosition.x)
                * step(gridPosition.x, mapSize.x - 0.5)
                * step(-0.5, gridPosition.y)
                * step(gridPosition.y, mapSize.y - 0.5);

            if (insideMap < 0.5)
                return 1.0.xx;

            float2 masks =
                ResolveSmoothedFogMasks(
                    gridPosition,
                    worldPosition);

            float hidden =
                saturate(masks.r);

            float unexplored =
                saturate(
                    min(
                        masks.g,
                        hidden));

            return float2(
                hidden,
                unexplored);
        }

        float2 ProjectWorldToScreenUv(float3 worldPosition)
        {
            float4 clip =
                TransformWorldToHClip(
                    worldPosition);

            float inverseW =
                rcp(
                    max(
                        abs(clip.w),
                        0.00001));

            float2 uv =
                clip.xy
                * inverseW
                * 0.5
                + 0.5;

            #if UNITY_UV_STARTS_AT_TOP
                uv.y = 1.0 - uv.y;
            #endif

            return uv;
        }

        float SampleUnexploredCellOrOutside(
            float2 cell,
            out float insideCell)
        {
            float2 mapSize =
                max(
                    _MoyvaFogMapSize.xy,
                    1.0.xx);

            insideCell =
                step(-0.5, cell.x)
                * step(cell.x, mapSize.x - 0.5)
                * step(-0.5, cell.y)
                * step(cell.y, mapSize.y - 0.5);

            if (insideCell < 0.5)
                return 1.0;

            return saturate(
                SampleFogMasks(cell).g);
        }

        float2 FogGridDeltaToWorldXZ(
            float2 gridDelta)
        {
            /*
             * WorldToGrid:
             * [ a b ] * worldXZ = gridX
             * [ c d ]             gridY
             *
             * Invert the 2x2 matrix so boundary filtering can operate
             * in stable world/grid units instead of screen pixels.
             */
            if (_MoyvaFogFlipY > 0.5)
            {
                gridDelta.y =
                    -gridDelta.y;
            }

            float a =
                _MoyvaFogWorldToGrid.x;

            float b =
                _MoyvaFogWorldToGrid.y;

            float c =
                _MoyvaFogWorldToGrid.z;

            float d =
                _MoyvaFogWorldToGrid.w;

            float determinant =
                a * d - b * c;

            if (abs(determinant) <= 0.000001)
            {
                return gridDelta;
            }

            float inverseDeterminant =
                rcp(determinant);

            return float2(
                (
                    d * gridDelta.x
                    - b * gridDelta.y
                )
                * inverseDeterminant,

                (
                    -c * gridDelta.x
                    + a * gridDelta.y
                )
                * inverseDeterminant);
        }

        float ResolveProjectedGridPixels(
            float3 worldPosition,
            float2 gridDelta)
        {
            float2 worldDeltaXZ =
                FogGridDeltaToWorldXZ(
                    gridDelta);

            float2 baseUv =
                ProjectWorldToScreenUv(
                    worldPosition);

            float2 offsetUv =
                ProjectWorldToScreenUv(
                    worldPosition
                    + float3(
                        worldDeltaXZ.x,
                        0.0,
                        worldDeltaXZ.y));

            return max(
                0.001,
                length(
                    (offsetUv - baseUv)
                    * _ScaledScreenParams.xy));
        }

        float ResolveSideBoundaryWeight(
            float stateDifference,
            float distanceCells,
            float widthCells)
        {
            if (stateDifference <= 0.001)
                return 0.0;

            return stateDifference
                   * (
                       1.0
                       - smoothstep(
                           0.0,
                           max(
                               0.001,
                               widthCells),
                           max(
                               0.0,
                               distanceCells))
                   );
        }

        float ResolveWorldBoundaryAtWorld(
            float3 worldPosition,
            out float sourceUnexplored)
        {
            float2 gridPosition =
                WorldToFogGrid(
                    worldPosition);

            float2 currentCell =
                floor(
                    gridPosition
                    + 0.5.xx);

            float2 localPosition =
                clamp(
                    gridPosition
                    - currentCell,
                    -0.5.xx,
                    0.5.xx);

            float currentInside;

            sourceUnexplored =
                SampleUnexploredCellOrOutside(
                    currentCell,
                    currentInside);

            /*
             * The dark virtual depth originates only on the unexplored
             * side of a world-grid boundary.
             */
            if (sourceUnexplored < 0.5)
                return 0.0;

            float ignoredInside;

            float leftState =
                SampleUnexploredCellOrOutside(
                    currentCell
                    + float2(-1.0, 0.0),
                    ignoredInside);

            float rightState =
                SampleUnexploredCellOrOutside(
                    currentCell
                    + float2(1.0, 0.0),
                    ignoredInside);

            float downState =
                SampleUnexploredCellOrOutside(
                    currentCell
                    + float2(0.0, -1.0),
                    ignoredInside);

            float upState =
                SampleUnexploredCellOrOutside(
                    currentCell
                    + float2(0.0, 1.0),
                    ignoredInside);

            float downLeftState =
                SampleUnexploredCellOrOutside(
                    currentCell
                    + float2(-1.0, -1.0),
                    ignoredInside);

            float upLeftState =
                SampleUnexploredCellOrOutside(
                    currentCell
                    + float2(-1.0, 1.0),
                    ignoredInside);

            float downRightState =
                SampleUnexploredCellOrOutside(
                    currentCell
                    + float2(1.0, -1.0),
                    ignoredInside);

            float upRightState =
                SampleUnexploredCellOrOutside(
                    currentCell
                    + float2(1.0, 1.0),
                    ignoredInside);

            float gridXPixels =
                ResolveProjectedGridPixels(
                    worldPosition,
                    float2(1.0, 0.0));

            float gridYPixels =
                ResolveProjectedGridPixels(
                    worldPosition,
                    float2(0.0, 1.0));

            /*
             * Approximately 1.25 screen pixels of analytical AA,
             * expressed in grid-cell units. The boundary position remains
             * fixed in world space while its coverage stays antialiased.
             */
            float widthX =
                clamp(
                    max(
                        _MoyvaFogEdgeSoftness * 0.35,
                        1.25 / gridXPixels),
                    0.025,
                    0.48);

            float widthY =
                clamp(
                    max(
                        _MoyvaFogEdgeSoftness * 0.35,
                        1.25 / gridYPixels),
                    0.025,
                    0.48);

            float leftDistance =
                0.5 + localPosition.x;

            float rightDistance =
                0.5 - localPosition.x;

            float downDistance =
                0.5 + localPosition.y;

            float upDistance =
                0.5 - localPosition.y;

            float boundary = 0.0;

            boundary =
                max(
                    boundary,
                    ResolveSideBoundaryWeight(
                        abs(
                            sourceUnexplored
                            - leftState),
                        leftDistance,
                        widthX));

            boundary =
                max(
                    boundary,
                    ResolveSideBoundaryWeight(
                        abs(
                            sourceUnexplored
                            - rightState),
                        rightDistance,
                        widthX));

            boundary =
                max(
                    boundary,
                    ResolveSideBoundaryWeight(
                        abs(
                            sourceUnexplored
                            - downState),
                        downDistance,
                        widthY));

            boundary =
                max(
                    boundary,
                    ResolveSideBoundaryWeight(
                        abs(
                            sourceUnexplored
                            - upState),
                        upDistance,
                        widthY));

            /*
             * Corner coverage prevents tiny gaps or thickness changes
             * where two orthogonal grid boundaries meet.
             */
            float diagonalWidth =
                max(widthX, widthY)
                * 1.41421356;

            float downLeftDistance =
                length(
                    float2(
                        leftDistance,
                        downDistance));

            float upLeftDistance =
                length(
                    float2(
                        leftDistance,
                        upDistance));

            float downRightDistance =
                length(
                    float2(
                        rightDistance,
                        downDistance));

            float upRightDistance =
                length(
                    float2(
                        rightDistance,
                        upDistance));

            boundary =
                max(
                    boundary,
                    ResolveSideBoundaryWeight(
                        abs(
                            sourceUnexplored
                            - downLeftState),
                        downLeftDistance,
                        diagonalWidth));

            boundary =
                max(
                    boundary,
                    ResolveSideBoundaryWeight(
                        abs(
                            sourceUnexplored
                            - upLeftState),
                        upLeftDistance,
                        diagonalWidth));

            boundary =
                max(
                    boundary,
                    ResolveSideBoundaryWeight(
                        abs(
                            sourceUnexplored
                            - downRightState),
                        downRightDistance,
                        diagonalWidth));

            boundary =
                max(
                    boundary,
                    ResolveSideBoundaryWeight(
                        abs(
                            sourceUnexplored
                            - upRightState),
                        upRightDistance,
                        diagonalWidth));

            return saturate(boundary);
        }

        float SampleSurfaceEyeDepth(float2 screenUv)
        {
            return SAMPLE_TEXTURE2D_X(
                _MoyvaFogSurfaceEyeDepthTexture,
                sampler_PointClamp,
                saturate(screenUv)).r;
        }

        bool TryResolveStableWorldPosition(
            float2 screenUv,
            out float3 worldPosition,
            out float eyeDepth,
            out float surfaceValid)
        {
            eyeDepth =
                SampleSurfaceEyeDepth(
                    screenUv);

            surfaceValid =
                step(
                    0.0001,
                    eyeDepth);

            if (surfaceValid > 0.5
                && TryResolveWorldFromEyeDepth(
                    screenUv,
                    eyeDepth,
                    worldPosition))
            {
                return true;
            }

            return TryResolveFallbackWorldPosition(
                screenUv,
                worldPosition,
                eyeDepth);
        }

        float ResolveWorldBoundaryAtScreenUv(
            float2 screenUv,
            out float sourceUnexplored,
            out float sourceEyeDepth,
            out float sourceSurfaceValid)
        {
            float3 sourceWorldPosition;

            if (!TryResolveStableWorldPosition(
                    screenUv,
                    sourceWorldPosition,
                    sourceEyeDepth,
                    sourceSurfaceValid))
            {
                sourceUnexplored = 1.0;
                return 0.0;
            }

            return ResolveWorldBoundaryAtWorld(
                sourceWorldPosition,
                sourceUnexplored);
        }

        float4 SampleClosedState(float2 screenUv)
        {
            return SAMPLE_TEXTURE2D_X(
                _MoyvaFogScreenStateTexture,
                sampler_LinearClamp,
                saturate(screenUv));
        }

        float4 SampleRawState(float2 screenUv)
        {
            return SAMPLE_TEXTURE2D_X(
                _MoyvaFogScreenStateRawTexture,
                sampler_LinearClamp,
                saturate(screenUv));
        }

        float4 SampleRawStatePoint(float2 screenUv)
        {
            return SAMPLE_TEXTURE2D_X(
                _MoyvaFogScreenStateRawTexture,
                sampler_PointClamp,
                saturate(screenUv));
        }

        float4 SampleClosedStatePoint(float2 screenUv)
        {
            return SAMPLE_TEXTURE2D_X(
                _MoyvaFogScreenStateTexture,
                sampler_PointClamp,
                saturate(screenUv));
        }

        float ResolveVirtualDepthWeight(
            float2 screenUv,
            float3 worldPosition,
            float currentEyeDepth,
            float currentSurfaceValid,
            float currentUnexplored)
        {
            if (_MoyvaFogVirtualDepthEnabled < 0.5)
                return 0.0;

            currentUnexplored =
                saturate(
                    currentUnexplored);

            if (currentUnexplored > 0.98)
                return 0.0;

            float2 topUv =
                ProjectWorldToScreenUv(
                    worldPosition);

            float2 bottomUv =
                ProjectWorldToScreenUv(
                    worldPosition
                    - float3(
                        0.0,
                        max(
                            0.1,
                            _MoyvaFogVirtualDepthWorld),
                        0.0));

            float2 projectedDown =
                bottomUv - topUv;

            float2 projectedPixelsVector =
                projectedDown
                * _ScaledScreenParams.xy;

            float projectedPixels =
                length(
                    projectedPixelsVector);

            if (projectedPixels <= 0.35)
                return 0.0;

            float maximumPixels =
                max(
                    1.0,
                    _MoyvaFogVirtualDepthMaxPixels);

            /*
             * Не примушуємо edge завжди бути мінімум 5 px.
             * На zoom out він фізично стає тоншим і плавно згасає,
             * замість того щоб перетворюватися на широку темну пляму.
             */
            float extrusionPixels =
                min(
                    projectedPixels,
                    maximumPixels);

            float fullOpacityPixels =
                max(
                    1.0,
                    _MoyvaFogVirtualDepthMinPixels);

            float zoomVisibility =
                smoothstep(
                    0.55,
                    fullOpacityPixels,
                    projectedPixels);

            float2 directionPixels =
                normalize(
                    projectedPixelsVector);

            float2 directionUv =
                directionPixels
                / max(
                    _ScaledScreenParams.xy,
                    1.0.xx);

            float2 perpendicularUv =
                float2(
                    -directionPixels.y,
                    directionPixels.x)
                / max(
                    _ScaledScreenParams.xy,
                    1.0.xx)
                * 0.45;

            int configuredSamples =
                (int)clamp(
                    floor(
                        _MoyvaFogVirtualDepthSamples
                        + 0.5),
                    4.0,
                    24.0);

            int sampleCount =
                (int)clamp(
                    ceil(
                        extrusionPixels
                        * 1.25),
                    3.0,
                    (float)configuredSamples);

            float strongest = 0.0;

            [unroll(24)]
            for (int sampleIndex = 1;
                 sampleIndex <= 24;
                 sampleIndex++)
            {
                if (sampleIndex > sampleCount)
                    break;

                /*
                 * Центри sample-інтервалів дають стабільніший результат,
                 * ніж вибірка точно на їхніх межах.
                 */
                float normalizedDistance =
                    (
                        (float)sampleIndex
                        - 0.5
                    )
                    / max(
                        1.0,
                        (float)sampleCount);

                float2 sourceUv =
                    screenUv
                    - directionUv
                    * extrusionPixels
                    * normalizedDistance;

                if (any(sourceUv < 0.0.xx)
                    || any(sourceUv > 1.0.xx))
                {
                    continue;
                }

                float sourceUnexplored;
                float sourceEyeDepth;
                float sourceSurfaceValid;

                float centerBoundary =
                    ResolveWorldBoundaryAtScreenUv(
                        sourceUv,
                        sourceUnexplored,
                        sourceEyeDepth,
                        sourceSurfaceValid);

                float sideAUnexplored;
                float sideAEyeDepth;
                float sideAValid;

                float sideABoundary =
                    ResolveWorldBoundaryAtScreenUv(
                        sourceUv
                        + perpendicularUv,
                        sideAUnexplored,
                        sideAEyeDepth,
                        sideAValid);

                float sideBUnexplored;
                float sideBEyeDepth;
                float sideBValid;

                float sideBBoundary =
                    ResolveWorldBoundaryAtScreenUv(
                        sourceUv
                        - perpendicularUv,
                        sideBUnexplored,
                        sideBEyeDepth,
                        sideBValid);

                /*
                 * All three samples evaluate the same world-grid boundary.
                 * Their maximum only stabilizes sub-pixel diagonal coverage;
                 * it no longer changes the shape according to screen
                 * morphology or zoom-dependent pixel neighborhoods.
                 */
                float stableBoundary =
                    max(
                        centerBoundary,
                        max(
                            sideABoundary,
                            sideBBoundary));

                sourceUnexplored =
                    max(
                        sourceUnexplored,
                        max(
                            sideAUnexplored,
                            sideBUnexplored));

                float depthVisible = 1.0;

                if (currentSurfaceValid > 0.5
                    && sourceSurfaceValid > 0.5)
                {
                    depthVisible =
                        step(
                            sourceEyeDepth
                            - max(
                                0.0,
                                _MoyvaFogVirtualDepthOcclusionBias),
                            currentEyeDepth);
                }

                float gradient =
                    pow(
                        saturate(
                            1.0
                            - normalizedDistance),
                        max(
                            0.25,
                            _MoyvaFogVirtualDepthGradientPower));

                float sourceContribution =
                    stableBoundary
                    * depthVisible
                    * gradient;

                strongest =
                    max(
                        strongest,
                        sourceContribution);

            }

            float softnessPixels =
                min(
                    max(
                        0.0,
                        _MoyvaFogScreenBoundarySoftnessPixels),
                    max(
                        0.35,
                        extrusionPixels * 0.25));

            if (softnessPixels > 0.001)
            {
                strongest =
                    smoothstep(
                        0.0,
                        saturate(
                            1.0
                            / (
                                1.0
                                + softnessPixels
                            )),
                        strongest);
            }

            return saturate(
                strongest
                * zoomVisibility);
        }

        ENDHLSL

        Pass
        {
            Name "BuildScreenState"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment FragBuildScreenState

            float4 ResolveScreenStateSample(
                float2 screenUv)
            {
                float surfaceEyeDepth =
                    SAMPLE_TEXTURE2D_X(
                        _BlitTexture,
                        sampler_PointClamp,
                        screenUv).r;

                float3 worldPosition;
                float resolvedEyeDepth =
                    surfaceEyeDepth;

                float validSurface =
                    step(
                        0.0001,
                        surfaceEyeDepth);

                bool resolvedWorld = false;

                if (validSurface > 0.5)
                {
                    resolvedWorld =
                        TryResolveWorldFromEyeDepth(
                            screenUv,
                            surfaceEyeDepth,
                            worldPosition);
                }
                else
                {
                    resolvedWorld =
                        TryResolveFallbackWorldPosition(
                            screenUv,
                            worldPosition,
                            resolvedEyeDepth);
                }

                if (!resolvedWorld)
                {
                    return float4(
                        1.0,
                        1.0,
                        0.0,
                        0.0);
                }

                float insideMap;

                float2 fogState =
                    ResolveFogStateAtWorld(
                        worldPosition,
                        insideMap);

                return float4(
                    fogState.x,
                    fogState.y,
                    resolvedEyeDepth,
                    validSurface);
            }

            float4 FragBuildScreenState(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 screenUv =
                    input.texcoord;

                float4 center =
                    ResolveScreenStateSample(
                        screenUv);

                /*
                 * Sub-pixel screen-state coverage.
                 * При zoom out одна fog boundary може займати менше пікселя.
                 * Чотири додаткові sample згладжують silhouette без temporal
                 * jitter і без розмиття eye depth.
                 */
                float2 subPixel =
                    _BlitTexture_TexelSize.xy
                    * 0.35;

                float2 coverage =
                    center.rg;

                coverage +=
                    ResolveScreenStateSample(
                        screenUv
                        + float2(
                            subPixel.x,
                            subPixel.y)).rg;

                coverage +=
                    ResolveScreenStateSample(
                        screenUv
                        + float2(
                            -subPixel.x,
                            subPixel.y)).rg;

                coverage +=
                    ResolveScreenStateSample(
                        screenUv
                        + float2(
                            subPixel.x,
                            -subPixel.y)).rg;

                coverage +=
                    ResolveScreenStateSample(
                        screenUv
                        - subPixel).rg;

                coverage *=
                    0.2;

                return float4(
                    saturate(
                        coverage),
                    center.b,
                    center.a);
            }

            ENDHLSL
        }

        Pass
        {
            Name "DilateScreenState"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment FragDilate

            float4 FragDilate(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float4 center =
                    SAMPLE_TEXTURE2D_X(
                        _BlitTexture,
                        sampler_PointClamp,
                        input.texcoord);

                int radius =
                    (int)clamp(
                        floor(
                            _MoyvaFogScreenCloseRadiusPixels
                            + 0.5),
                        0.0,
                        3.0);

                float hidden = center.r;
                float unexplored = center.g;

                [unroll]
                for (int y = -3; y <= 3; y++)
                {
                    [unroll]
                    for (int x = -3; x <= 3; x++)
                    {
                        if (abs(x) > radius
                            || abs(y) > radius)
                        {
                            continue;
                        }

                        float2 uv =
                            input.texcoord
                            + float2((float)x, (float)y)
                            * _BlitTexture_TexelSize.xy;

                        float2 sampleState =
                            SAMPLE_TEXTURE2D_X(
                                _BlitTexture,
                                sampler_PointClamp,
                                uv).rg;

                        hidden =
                            max(hidden, sampleState.r);

                        unexplored =
                            max(unexplored, sampleState.g);
                    }
                }

                /*
                 * Only RG are authoritative after morphology.
                 * BA remain debug-only center data.
                 */
                return float4(
                    hidden,
                    min(unexplored, hidden),
                    center.b,
                    center.a);
            }

            ENDHLSL
        }

        Pass
        {
            Name "ErodeScreenState"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment FragErode

            float4 FragErode(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float4 center =
                    SAMPLE_TEXTURE2D_X(
                        _BlitTexture,
                        sampler_PointClamp,
                        input.texcoord);

                int radius =
                    (int)clamp(
                        floor(
                            _MoyvaFogScreenCloseRadiusPixels
                            + 0.5),
                        0.0,
                        3.0);

                float hidden = center.r;
                float unexplored = center.g;

                [unroll]
                for (int y = -3; y <= 3; y++)
                {
                    [unroll]
                    for (int x = -3; x <= 3; x++)
                    {
                        if (abs(x) > radius
                            || abs(y) > radius)
                        {
                            continue;
                        }

                        float2 uv =
                            input.texcoord
                            + float2((float)x, (float)y)
                            * _BlitTexture_TexelSize.xy;

                        float2 sampleState =
                            SAMPLE_TEXTURE2D_X(
                                _BlitTexture,
                                sampler_PointClamp,
                                uv).rg;

                        hidden =
                            min(hidden, sampleState.r);

                        unexplored =
                            min(unexplored, sampleState.g);
                    }
                }

                /*
                 * Only RG are authoritative after morphology.
                 * BA remain debug-only center data.
                 */
                return float4(
                    hidden,
                    min(unexplored, hidden),
                    center.b,
                    center.a);
            }

            ENDHLSL
        }

        Pass
        {
            Name "DepthAwareComposite"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment FragComposite

            half4 FragComposite(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 screenUv = input.texcoord;

                half4 source =
                    SAMPLE_TEXTURE2D_X(
                        _BlitTexture,
                        sampler_LinearClamp,
                        screenUv);

                if (_MoyvaFogEnabled < 0.5)
                    return source;

                float4 rawState =
                    SampleRawState(screenUv);

                float4 state =
                    SampleClosedState(screenUv);

                float4 stateData =
                    SampleClosedStatePoint(
                        screenUv);

                /*
                 * BA in the morphed screen state are not geometry data.
                 * Geometry always comes from the immutable surface depth.
                 */
                float surfaceEyeDepth = 0.0;
                float surfaceValid = 0.0;

                float3 worldPosition = 0.0.xxx;

                bool worldResolved =
                    TryResolveStableWorldPosition(
                        screenUv,
                        worldPosition,
                        surfaceEyeDepth,
                        surfaceValid);

                float insideMap = 0.0;

                float2 directFogState =
                    worldResolved
                        ? ResolveFogStateAtWorld(
                            worldPosition,
                            insideMap)
                        : saturate(
                            rawState.rg);

                /*
                 * Dedicated surface depth is authoritative where available.
                 * If no physical terrain/water surface was tagged at this
                 * pixel, use the already-built RawScreenState instead of
                 * turning the frame into infinite unexplored fog.
                 */
                float useDirectWorldState =
                    worldResolved
                    && surfaceValid > 0.5
                        ? 1.0
                        : 0.0;

                float2 finalFogState =
                    lerp(
                        saturate(
                            rawState.rg),
                        directFogState,
                        useDirectWorldState);

                float hiddenWeight =
                    saturate(
                        finalFogState.r);

                float unexploredWeight =
                    saturate(
                        min(
                            finalFogState.g,
                            hiddenWeight));

                float exploredWeight =
                    saturate(
                        hiddenWeight
                        - unexploredWeight);

                float2 gridPosition =
                    worldResolved
                        ? WorldToFogGrid(
                            worldPosition)
                        : 0.0.xx;

                if (_MoyvaFogDebugMode > 0.5)
                {
                    if (_MoyvaFogDebugMode < 1.5)
                    {
                        float3 stateColor =
                            float3(0.1, 0.9, 0.2);

                        stateColor =
                            lerp(
                                stateColor,
                                float3(1.0, 0.85, 0.05),
                                exploredWeight);

                        stateColor =
                            lerp(
                                stateColor,
                                float3(1.0, 0.05, 0.05),
                                unexploredWeight);

                        return half4(stateColor, 1.0);
                    }

                    if (!worldResolved)
                    {
                        return half4(
                            rawState.r,
                            rawState.g,
                            0.0,
                            1.0);
                    }

                    if (_MoyvaFogDebugMode < 2.5)
                    {
                        float2 cellFraction =
                            frac(gridPosition + 0.5.xx);

                        float2 edgeDistance =
                            min(
                                cellFraction,
                                1.0.xx - cellFraction);

                        float2 footprint =
                            max(
                                fwidth(gridPosition),
                                0.00001.xx);

                        float lineWidth =
                            max(
                                0.5,
                                _MoyvaFogDebugGridLineWidthPixels);

                        float cellLine =
                            1.0
                            - saturate(
                                min(
                                    edgeDistance.x / footprint.x,
                                    edgeDistance.y / footprint.y)
                                / lineWidth);

                        return half4(
                            lerp(
                                float3(0.08, 0.12, 0.16),
                                1.0.xxx,
                                cellLine),
                            1.0);
                    }

                    if (_MoyvaFogDebugMode < 3.5)
                    {
                        float depthView =
                            1.0 - exp(-surfaceEyeDepth * 0.025);

                        return half4(
                            depthView.xxx,
                            1.0);
                    }

                    if (_MoyvaFogDebugMode < 4.5)
                    {
                        float2 fractionalGrid =
                            frac(gridPosition * 0.1);

                        return half4(
                            fractionalGrid.x,
                            fractionalGrid.y,
                            0.25,
                            1.0);
                    }

                    if (_MoyvaFogDebugMode < 5.5)
                    {
                        float valid = surfaceValid;
                        float depthView =
                            1.0 - exp(-surfaceEyeDepth * 0.025);

                        return half4(
                            depthView,
                            valid,
                            1.0 - valid,
                            1.0);
                    }

                    if (_MoyvaFogDebugMode < 6.5)
                    {
                        return half4(
                            rawState.r,
                            rawState.g,
                            rawState.a,
                            1.0);
                    }

                    if (_MoyvaFogDebugMode < 7.5)
                    {
                        return half4(
                            state.r,
                            state.g,
                            state.a,
                            1.0);
                    }

                    float boundarySourceUnexplored;

                    float boundaryStrength =
                        ResolveWorldBoundaryAtWorld(
                            worldPosition,
                            boundarySourceUnexplored);

                    if (_MoyvaFogDebugMode < 8.5)
                    {
                        return half4(
                            boundaryStrength.xxx,
                            1.0);
                    }

                    float virtualDepthDebug =
                        worldResolved
                            ? ResolveVirtualDepthWeight(
                                screenUv,
                                worldPosition,
                                surfaceEyeDepth,
                                surfaceValid,
                                unexploredWeight)
                            : 0.0;

                    return half4(
                        virtualDepthDebug,
                        0.1,
                        1.0 - virtualDepthDebug,
                        1.0);
                }

                float3 exploredSource =
                    ApplySaturation(
                        source.rgb,
                        _MoyvaFogExploredSaturation);

                float3 exploredTinted =
                    exploredSource
                    * _MoyvaFogExploredColor.rgb;

                float3 result =
                    lerp(
                        source.rgb,
                        exploredTinted,
                        exploredWeight
                        * saturate(
                            _MoyvaFogExploredOpacity));

                float virtualDepthWeight =
                    worldResolved
                        ? ResolveVirtualDepthWeight(
                            screenUv,
                            worldPosition,
                            surfaceEyeDepth,
                            surfaceValid,
                            unexploredWeight)
                        : 0.0;

                result =
                    lerp(
                        result,
                        _MoyvaFogVirtualDepthColor.rgb,
                        virtualDepthWeight
                        * saturate(
                            _MoyvaFogVirtualDepthOpacity));

                float unexploredBlend =
                    unexploredWeight
                    * saturate(
                        _MoyvaFogUnexploredOpacity);

                if (unexploredWeight >= 0.999
                    && _MoyvaFogUnexploredOpacity >= 0.999)
                {
                    result =
                        _MoyvaFogUnexploredColor.rgb;
                }
                else
                {
                    float3 unexploredSource =
                        ApplySaturation(
                            result,
                            _MoyvaFogUnexploredSaturation);

                    float3 unexploredTinted =
                        lerp(
                            unexploredSource,
                            _MoyvaFogUnexploredColor.rgb,
                            0.92);

                    result =
                        lerp(
                            result,
                            unexploredTinted,
                            unexploredBlend);
                }

                return half4(result, 1.0);
            }

            ENDHLSL
        }

        Pass
        {
            Name "CopyBack"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment FragCopy

            half4 FragCopy(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                return SAMPLE_TEXTURE2D_X(
                    _BlitTexture,
                    sampler_LinearClamp,
                    input.texcoord);
            }

            ENDHLSL
        }
    }

    FallBack Off
}