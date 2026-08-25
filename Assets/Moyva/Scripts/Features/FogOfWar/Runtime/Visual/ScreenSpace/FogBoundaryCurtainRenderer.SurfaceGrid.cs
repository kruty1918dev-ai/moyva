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
        private void ResolveGridBasis(
            int width,
            int height,
            FogWorldVisualContext context,
            out Vector3 origin,
            out Vector3 xBasis,
            out Vector3 yBasis)
        {
            if (_gridProjection != null)
            {
                origin =
                    _gridProjection.GridToWorld(
                        Vector2Int.zero);

                xBasis =
                    _gridProjection.GridToWorld(
                        Vector2Int.right)
                    - origin;

                yBasis =
                    _gridProjection.GridToWorld(
                        Vector2Int.up)
                    - origin;

                if (HorizontalLength(xBasis)
                        > 0.0001f
                    && HorizontalLength(yBasis)
                        > 0.0001f)
                {
                    return;
                }
            }

            if (context.IsValid
                && context.HasMapWorldBounds)
            {
                Bounds bounds =
                    context.MapWorldBounds;

                float cellWidth =
                    Mathf.Max(
                        0.0001f,
                        bounds.size.x
                        / Mathf.Max(
                            1,
                            width));

                float cellDepth =
                    Mathf.Max(
                        0.0001f,
                        bounds.size.z
                        / Mathf.Max(
                            1,
                            height));

                origin =
                    new Vector3(
                        bounds.min.x
                            + cellWidth * 0.5f,
                        0f,
                        bounds.min.z
                            + cellDepth * 0.5f);

                xBasis =
                    new Vector3(
                        cellWidth,
                        0f,
                        0f);

                yBasis =
                    new Vector3(
                        0f,
                        0f,
                        cellDepth);

                return;
            }

            float cellSize =
                context.IsValid
                    ? Mathf.Max(
                        0.0001f,
                        context.CellSize)
                    : 1f;

            origin =
                Vector3.zero;

            xBasis =
                new Vector3(
                    cellSize,
                    0f,
                    0f);

            yBasis =
                new Vector3(
                    0f,
                    0f,
                    cellSize);
        }

        private Vector3 ResolveCellCenter(
            Vector2Int cell,
            Vector3 origin,
            Vector3 xBasis,
            Vector3 yBasis)
        {
            if (_gridProjection != null)
            {
                return _gridProjection
                    .GridToWorld(cell);
            }

            return origin
                   + xBasis * cell.x
                   + yBasis * cell.y;
        }

        private float ResolveSurfaceHeight(
            FogWorldVisualContext context,
            Vector2Int cell,
            float fallback)
        {
            if (!context.IsValid)
                return fallback;

            FogVolumeHeightSource source =
                _settings != null
                    && _settings.Volume != null
                    ? _settings.Volume.HeightSource
                    : FogVolumeHeightSource
                        .TerrainLevelMapThenHeightMap;

            switch (source)
            {
                case FogVolumeHeightSource
                    .HeightMapThenTerrainLevelMap:

                    if (TryResolveHeightMapValue(
                            context,
                            cell,
                            out float heightMapValue))
                    {
                        return heightMapValue;
                    }

                    if (TryResolveTerrainLevelValue(
                            context,
                            cell,
                            out float terrainHeightValue))
                    {
                        return terrainHeightValue;
                    }

                    break;

                case FogVolumeHeightSource.Flat:
                    return fallback;

                default:
                    if (TryResolveTerrainLevelValue(
                            context,
                            cell,
                            out terrainHeightValue))
                    {
                        return terrainHeightValue;
                    }

                    if (TryResolveHeightMapValue(
                            context,
                            cell,
                            out heightMapValue))
                    {
                        return heightMapValue;
                    }

                    break;
            }

            return fallback;
        }

        private bool TryResolveTerrainLevelValue(
            FogWorldVisualContext context,
            Vector2Int cell,
            out float height)
        {
            height = 0f;

            int[,] terrainLevels =
                context.TerrainLevelMap;

            if (terrainLevels == null
                || cell.x < 0
                || cell.y < 0
                || cell.x >= terrainLevels.GetLength(0)
                || cell.y >= terrainLevels.GetLength(1))
            {
                return false;
            }

            float heightStep =
                _settings != null
                && _settings.Volume != null
                    ? Mathf.Max(
                        0.001f,
                        _settings.Volume
                            .TerrainLevelHeightStep)
                    : 1f;

            height =
                Mathf.Max(
                    0,
                    terrainLevels[
                        cell.x,
                        cell.y])
                * heightStep;

            return IsFinite(height);
        }

        private static bool TryResolveHeightMapValue(
            FogWorldVisualContext context,
            Vector2Int cell,
            out float height)
        {
            height = 0f;

            float[,] heightMap =
                context.HeightMap;

            if (heightMap == null
                || cell.x < 0
                || cell.y < 0
                || cell.x >= heightMap.GetLength(0)
                || cell.y >= heightMap.GetLength(1))
            {
                return false;
            }

            height =
                heightMap[
                    cell.x,
                    cell.y];

            return IsFinite(height);
        }

        private static bool IsFinite(
            float value)
        {
            return !float.IsNaN(value)
                   && !float.IsInfinity(value);
        }

        private FogScreenSpaceSettings
            ResolveSettings()
        {
            FogScreenSpaceSettings result =
                _settings != null
                    ? _settings.ScreenSpace
                    : null;

            if (result == null)
            {
                result =
                    new FogScreenSpaceSettings();
            }

            result.EnsureDefaults();
            return result;
        }

        private static bool IsUnexplored(
            Color32[] fogPixels,
            int width,
            int x,
            int y)
        {
            int index =
                x + y * width;

            /*
             * G channel:
             * 255 = Unexplored
             * 0   = Explored або Visible
             */
            return fogPixels[index].g >= 128;
        }

        private static bool IsUnexplored(
            Color32[] fogPixels,
            int width,
            int height,
            Vector2Int cell)
        {
            if (!IsInBounds(
                    cell,
                    width,
                    height))
            {
                return true;
            }

            return IsUnexplored(
                fogPixels,
                width,
                cell.x,
                cell.y);
        }

        private static bool IsInBounds(
            Vector2Int cell,
            int width,
            int height)
        {
            return cell.x >= 0
                   && cell.x < width
                   && cell.y >= 0
                   && cell.y < height;
        }

        private static float HorizontalLength(
            Vector3 value)
        {
            return Mathf.Sqrt(
                value.x * value.x
                + value.z * value.z);
        }

    }
}
