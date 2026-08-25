using System;
using System.Collections.Generic;
using System.Text;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Jsonization;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed partial class FogBoundaryCurtainRenderer
    {
        private SurfaceCalibrationResult ResolveSurfaceCalibration(
            Color32[] fogPixels,
            int width,
            int height,
            FogWorldVisualContext context,
            Vector3 gridOrigin,
            Vector3 xBasis,
            Vector3 yBasis,
            FogScreenSpaceSettings settings)
        {
            float fallbackOffset =
                settings.CurtainSurfaceOffsetY;

            if (!settings.CurtainAutoCalibrateSurfaceOffset)
            {
                return new SurfaceCalibrationResult(
                    fallbackOffset,
                    0,
                    float.NaN,
                    float.NaN,
                    false);
            }

            _surfaceOffsetSamples.Clear();

            int targetSamples =
                Mathf.Clamp(
                    settings.CurtainSurfaceCalibrationSamples,
                    4,
                    64);

            int totalCells =
                width * height;

            int stride =
                Mathf.Max(
                    1,
                    totalCells
                    / Mathf.Max(
                        1,
                        targetSamples * 3));

            for (int linearIndex = 0;
                 linearIndex < totalCells
                 && _surfaceOffsetSamples.Count
                 < targetSamples;
                 linearIndex += stride)
            {
                int x =
                    linearIndex % width;

                int y =
                    linearIndex / width;

                if (IsUnexplored(
                        fogPixels,
                        width,
                        x,
                        y))
                {
                    continue;
                }

                var cell =
                    new Vector2Int(
                        x,
                        y);

                Vector3 cellCenter =
                    ResolveCellCenter(
                        cell,
                        gridOrigin,
                        xBasis,
                        yBasis);

                float logicalHeight =
                    ResolveSurfaceHeight(
                        context,
                        cell,
                        cellCenter.y);

                if (!TryProbeActualSurfaceHeight(
                        cellCenter,
                        logicalHeight,
                        settings,
                        out float actualHeight))
                {
                    continue;
                }

                float offset =
                    actualHeight
                    - logicalHeight;

                if (Mathf.Abs(offset)
                    > settings
                        .CurtainSurfaceCalibrationMaxOffset)
                {
                    continue;
                }

                _surfaceOffsetSamples.Add(
                    offset);
            }

            /*
             * Якщо stride пропустив revealed-область,
             * виконуємо короткий повний scan до набору мінімум 3 проб.
             */
            if (_surfaceOffsetSamples.Count < 3)
            {
                for (int y = 0;
                     y < height
                     && _surfaceOffsetSamples.Count
                     < targetSamples;
                     y++)
                {
                    for (int x = 0;
                         x < width
                         && _surfaceOffsetSamples.Count
                         < targetSamples;
                         x++)
                    {
                        if (IsUnexplored(
                                fogPixels,
                                width,
                                x,
                                y))
                        {
                            continue;
                        }

                        var cell =
                            new Vector2Int(
                                x,
                                y);

                        Vector3 cellCenter =
                            ResolveCellCenter(
                                cell,
                                gridOrigin,
                                xBasis,
                                yBasis);

                        float logicalHeight =
                            ResolveSurfaceHeight(
                                context,
                                cell,
                                cellCenter.y);

                        if (!TryProbeActualSurfaceHeight(
                                cellCenter,
                                logicalHeight,
                                settings,
                                out float actualHeight))
                        {
                            continue;
                        }

                        float offset =
                            actualHeight
                            - logicalHeight;

                        if (Mathf.Abs(offset)
                            > settings
                                .CurtainSurfaceCalibrationMaxOffset)
                        {
                            continue;
                        }

                        _surfaceOffsetSamples.Add(
                            offset);
                    }
                }
            }

            if (_surfaceOffsetSamples.Count == 0)
            {
                return new SurfaceCalibrationResult(
                    fallbackOffset,
                    0,
                    float.NaN,
                    float.NaN,
                    false);
            }

            _surfaceOffsetSamples.Sort();

            int middle =
                _surfaceOffsetSamples.Count / 2;

            float median =
                (_surfaceOffsetSamples.Count & 1) == 0
                    ? (_surfaceOffsetSamples[middle - 1]
                       + _surfaceOffsetSamples[middle])
                      * 0.5f
                    : _surfaceOffsetSamples[middle];

            return new SurfaceCalibrationResult(
                median,
                _surfaceOffsetSamples.Count,
                _surfaceOffsetSamples[0],
                _surfaceOffsetSamples[
                    _surfaceOffsetSamples.Count - 1],
                true);
        }

        private bool TryProbeActualSurfaceHeight(
            Vector3 cellCenter,
            float logicalHeight,
            FogScreenSpaceSettings settings,
            out float actualHeight)
        {
            actualHeight =
                0f;

            Vector3 rayOrigin =
                new Vector3(
                    cellCenter.x,
                    logicalHeight
                        + settings.CurtainSurfaceProbeHeight,
                    cellCenter.z);

            int hitCount =
                Physics.RaycastNonAlloc(
                    rayOrigin,
                    Vector3.down,
                    _surfaceProbeHits,
                    settings.CurtainSurfaceProbeDistance,
                    settings.CurtainSurfaceProbeMask,
                    QueryTriggerInteraction.Ignore);

            if (hitCount <= 0)
                return false;

            float nearestDistance =
                float.PositiveInfinity;

            bool found =
                false;

            for (int i = 0;
                 i < hitCount;
                 i++)
            {
                RaycastHit hit =
                    _surfaceProbeHits[i];

                if (hit.collider == null)
                    continue;

                if (!IsAcceptedTerrainSurface(
                        hit.collider))
                {
                    continue;
                }

                if (hit.distance >= nearestDistance)
                    continue;

                nearestDistance =
                    hit.distance;

                actualHeight =
                    hit.point.y;

                found =
                    true;
            }

            return found;
        }

        private static bool IsAcceptedTerrainSurface(
            Collider collider)
        {
            Transform current =
                collider != null
                    ? collider.transform
                    : null;

            while (current != null)
            {
                string currentName =
                    current.name;

                if (currentName.IndexOf(
                        "Terrain",
                        StringComparison.OrdinalIgnoreCase)
                    >= 0
                    || currentName.IndexOf(
                        "MapVisualChunk",
                        StringComparison.OrdinalIgnoreCase)
                    >= 0
                    || string.Equals(
                        currentName,
                        "TilesRoot",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                current =
                    current.parent;
            }

            return false;
        }

        private readonly struct SurfaceCalibrationResult
        {
            public readonly float OffsetY;
            public readonly int SampleCount;
            public readonly float Minimum;
            public readonly float Maximum;
            public readonly bool WasCalibrated;

            public SurfaceCalibrationResult(
                float offsetY,
                int sampleCount,
                float minimum,
                float maximum,
                bool wasCalibrated)
            {
                OffsetY =
                    offsetY;

                SampleCount =
                    sampleCount;

                Minimum =
                    minimum;

                Maximum =
                    maximum;

                WasCalibrated =
                    wasCalibrated;
            }
        }

    }
}
