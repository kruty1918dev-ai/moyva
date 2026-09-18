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
            float2 mapSize =
                max(
                    _MoyvaFogMapSize.xy,
                    1.0.xx);

            float2 insideLower =
                step(
                    0.0.xx,
                    cell);

            float2 insideUpper =
                step(
                    cell,
                    mapSize - 1.0.xx);

            float insideMap =
                insideLower.x
                * insideLower.y
                * insideUpper.x
                * insideUpper.y;

            float2 uv =
                ResolveFogTexelUv(cell);

            float2 sampled =
                SAMPLE_TEXTURE2D_LOD(
                _MoyvaFogStateTexture,
                sampler_MoyvaFogStateTexture,
                uv,
                0).rg;

            return lerp(
                1.0.xx,
                sampled,
                insideMap);
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

            /*
             * No fallback plane in the final presentation.
             * Empty background has no physical fog surface and therefore
             * cannot generate a camera-dependent diagonal boundary.
             */
            worldPosition = 0.0.xxx;
            eyeDepth = 0.0;
            surfaceValid = 0.0;
            return false;
        }

        float4 SampleRawStatePoint(float2 screenUv)
        {
            return SAMPLE_TEXTURE2D_X(
                _MoyvaFogScreenStateRawTexture,
                sampler_PointClamp,
                saturate(screenUv));
        }

        float IsFogCellInsideMap(
            float2 cell)
        {
            float2 mapSize =
                max(
                    _MoyvaFogMapSize.xy,
                    1.0.xx);

            return step(-0.5, cell.x)
                   * step(cell.x, mapSize.x - 0.5)
                   * step(-0.5, cell.y)
                   * step(cell.y, mapSize.y - 0.5);
        }

        float SampleDiscreteUnexploredCell(
            float2 cell)
        {
            /*
             * Cells outside the logical fog map count as Unexplored.
             * This prevents the hidden terrain silhouette from producing
             * an artificial edge along the external map perimeter.
             */
            if (IsFogCellInsideMap(cell) < 0.5)
                return 1.0;

            return saturate(
                SampleFogMasks(cell).g);
        }

        float ResolveFogSideWeight(
            float neighbourUnexplored,
            float distanceToSide,
            float widthCells)
        {
            float neighbourVisible =
                1.0
                - step(
                    0.5,
                    neighbourUnexplored);

            return neighbourVisible
                   * (
                       1.0
                       - smoothstep(
                           0.0,
                           max(
                               0.0001,
                               widthCells),
                           max(
                               0.0,
                               distanceToSide))
                   );
        }

        float ResolveFogSoftUnion(
            float firstWeight,
            float secondWeight)
        {
            /*
             * Smooth probabilistic union.
             * Unlike max(), it does not leave a visible crease where
             * two perpendicular edge gradients meet.
             */
            return saturate(
                firstWeight
                + secondWeight
                - firstWeight
                * secondWeight);
        }

        float ResolveFogCornerJoinWeight(
            float diagonalUnexplored,
            float horizontalUnexplored,
            float verticalUnexplored,
            float horizontalDistance,
            float verticalDistance,
            float horizontalWidth,
            float verticalWidth)
        {
            float diagonalVisible =
                1.0
                - step(
                    0.5,
                    diagonalUnexplored);

            float horizontalVisible =
                1.0
                - step(
                    0.5,
                    horizontalUnexplored);

            float verticalVisible =
                1.0
                - step(
                    0.5,
                    verticalUnexplored);

            /*
             * Two cases need an explicit corner cap:
             *
             * 1. Both adjacent cardinal sides are boundaries.
             * 2. A visible cell touches this unexplored cell only
             *    diagonally, which otherwise leaves a pinhole.
             */
            float cardinalJoin =
                horizontalVisible
                * verticalVisible;

            float diagonalOnlyJoin =
                diagonalVisible
                * (
                    1.0
                    - max(
                        horizontalVisible,
                        verticalVisible)
                );

            float joinRequired =
                saturate(
                    cardinalJoin
                    + diagonalOnlyJoin);

            if (joinRequired <= 0.0001)
                return 0.0;

            /*
             * Slightly overlap the side widths so independently evaluated
             * cells meet without a sub-pixel crack. The cap remains radial,
             * so it cannot produce a long miter spike.
             */
            float widthScale =
                lerp(
                    0.78,
                    1.16,
                    cardinalJoin);

            float2 normalizedDistance =
                float2(
                    horizontalDistance
                    / max(
                        0.0001,
                        horizontalWidth
                        * widthScale),
                    verticalDistance
                    / max(
                        0.0001,
                        verticalWidth
                        * widthScale));

            float radialDistance =
                length(
                    normalizedDistance);

            float cornerWeight =
                1.0
                - smoothstep(
                    0.68,
                    1.08,
                    radialDistance);

            return saturate(
                cornerWeight
                * joinRequired);
        }

        float ResolveSurfaceLockedGridEdge(
            float2 gridPosition,
            float surfaceValid)
        {
            if (_MoyvaFogVirtualDepthEnabled < 0.5
                || surfaceValid < 0.5)
            {
                return 0.0;
            }

            float2 currentCell =
                floor(
                    gridPosition
                    + 0.5.xx);

            float currentUnexplored =
                SampleDiscreteUnexploredCell(
                    currentCell);

            /*
             * The bevel exists only inside a discrete Unexplored cell.
             * Smoothed coverage on the visible side can never receive it.
             */
            if (currentUnexplored < 0.5)
                return 0.0;

            float2 localPosition =
                clamp(
                    gridPosition
                    - currentCell,
                    -0.5.xx,
                    0.5.xx);

            float2 gridUnitsPerPixel =
                max(
                    fwidth(gridPosition),
                    0.00001.xx);

            float widthPixels =
                clamp(
                    _MoyvaFogVirtualDepthMaxPixels,
                    2.0,
                    10.0);

            /*
             * Fixed screen thickness converted into grid units.
             * The logical boundary remains fixed in world space.
             */
            float widthX =
                clamp(
                    gridUnitsPerPixel.x
                    * widthPixels,
                    0.002,
                    0.48);

            float widthY =
                clamp(
                    gridUnitsPerPixel.y
                    * widthPixels,
                    0.002,
                    0.48);

            float leftDistance =
                localPosition.x + 0.5;

            float rightDistance =
                0.5 - localPosition.x;

            float downDistance =
                localPosition.y + 0.5;

            float upDistance =
                0.5 - localPosition.y;

            float leftState =
                SampleDiscreteUnexploredCell(
                    currentCell
                    + float2(-1.0, 0.0));

            float rightState =
                SampleDiscreteUnexploredCell(
                    currentCell
                    + float2(1.0, 0.0));

            float downState =
                SampleDiscreteUnexploredCell(
                    currentCell
                    + float2(0.0, -1.0));

            float upState =
                SampleDiscreteUnexploredCell(
                    currentCell
                    + float2(0.0, 1.0));

            float downLeftState =
                SampleDiscreteUnexploredCell(
                    currentCell
                    + float2(-1.0, -1.0));

            float upLeftState =
                SampleDiscreteUnexploredCell(
                    currentCell
                    + float2(-1.0, 1.0));

            float downRightState =
                SampleDiscreteUnexploredCell(
                    currentCell
                    + float2(1.0, -1.0));

            float upRightState =
                SampleDiscreteUnexploredCell(
                    currentCell
                    + float2(1.0, 1.0));

            float leftWeight =
                ResolveFogSideWeight(
                    leftState,
                    leftDistance,
                    widthX);

            float rightWeight =
                ResolveFogSideWeight(
                    rightState,
                    rightDistance,
                    widthX);

            float downWeight =
                ResolveFogSideWeight(
                    downState,
                    downDistance,
                    widthY);

            float upWeight =
                ResolveFogSideWeight(
                    upState,
                    upDistance,
                    widthY);

            /*
             * Smoothly unite cardinal sides. This preserves the Pass 6.11
             * width and falloff while removing the hard seam at 90° joins.
             */
            float horizontalUnion =
                ResolveFogSoftUnion(
                    leftWeight,
                    rightWeight);

            float verticalUnion =
                ResolveFogSoftUnion(
                    downWeight,
                    upWeight);

            float edgeWeight =
                ResolveFogSoftUnion(
                    horizontalUnion,
                    verticalUnion);

            /*
             * Radial corner caps seal all four vertices:
             * - no gap where two sides meet;
             * - no missing pixel at diagonal-only contact;
             * - no infinitely extended miter.
             */
            float downLeftCorner =
                ResolveFogCornerJoinWeight(
                    downLeftState,
                    leftState,
                    downState,
                    leftDistance,
                    downDistance,
                    widthX,
                    widthY);

            float upLeftCorner =
                ResolveFogCornerJoinWeight(
                    upLeftState,
                    leftState,
                    upState,
                    leftDistance,
                    upDistance,
                    widthX,
                    widthY);

            float downRightCorner =
                ResolveFogCornerJoinWeight(
                    downRightState,
                    rightState,
                    downState,
                    rightDistance,
                    downDistance,
                    widthX,
                    widthY);

            float upRightCorner =
                ResolveFogCornerJoinWeight(
                    upRightState,
                    rightState,
                    upState,
                    rightDistance,
                    upDistance,
                    widthX,
                    widthY);

            float cornerUnion =
                ResolveFogSoftUnion(
                    ResolveFogSoftUnion(
                        downLeftCorner,
                        upLeftCorner),
                    ResolveFogSoftUnion(
                        downRightCorner,
                        upRightCorner));

            edgeWeight =
                ResolveFogSoftUnion(
                    edgeWeight,
                    cornerUnion);

            edgeWeight =
                pow(
                    saturate(edgeWeight),
                    max(
                        0.25,
                        _MoyvaFogVirtualDepthGradientPower));

            return saturate(edgeWeight);
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

                bool resolvedWorld =
                    validSurface > 0.5
                    && TryResolveWorldFromEyeDepth(
                        screenUv,
                        surfaceEyeDepth,
                        worldPosition);

                if (!resolvedWorld)
                {
                    /*
                     * Empty screen background is fully Unexplored,
                     * but carries no depth and cannot form a bevel.
                     */
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

                /*
                 * One central sample only.
                 *
                 * Fog AA is already calculated analytically in
                 * ResolveSmoothedFogMasks via fwidth(gridPosition).
                 * Additional screen-space samples mixed unrelated surfaces:
                 * terrain, water, props and empty background.
                 */
                return ResolveScreenStateSample(
                    input.texcoord);
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

                float4 rawStatePoint =
                    SampleRawStatePoint(
                        screenUv);
                float surfaceEyeDepth;
                float surfaceValid;

                /*
                 * Fog shape comes directly from the point-stable state mask.
                 */
                float hiddenWeight =
                    saturate(
                        rawStatePoint.r);

                float unexploredWeight =
                    saturate(
                        min(
                            rawStatePoint.g,
                            hiddenWeight));

                float exploredWeight =
                    saturate(
                        hiddenWeight
                        - unexploredWeight);

                float3 worldPosition = 0.0.xxx;

                bool worldResolved =
                    TryResolveStableWorldPosition(
                        screenUv,
                        worldPosition,
                        surfaceEyeDepth,
                        surfaceValid);

                float2 gridPosition =
                    worldResolved
                        ? WorldToFogGrid(
                            worldPosition)
                        : 0.0.xx;

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

                float innerFogEdgeWeight =
                    (
                        worldResolved
                            ? ResolveSurfaceLockedGridEdge(
                                gridPosition,
                                surfaceValid)
                            : 0.0
                    )
                    * min(
                        saturate(
                            _MoyvaFogVirtualDepthOpacity),
                        0.88);

                float3 unexploredTargetColor =
                    lerp(
                        _MoyvaFogUnexploredColor.rgb,
                        _MoyvaFogVirtualDepthColor.rgb,
                        innerFogEdgeWeight);

                float unexploredBlend =
                    unexploredWeight
                    * saturate(
                        _MoyvaFogUnexploredOpacity);

                if (unexploredWeight >= 0.999
                    && _MoyvaFogUnexploredOpacity >= 0.999)
                {
                    result =
                        unexploredTargetColor;
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
                            unexploredTargetColor,
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
