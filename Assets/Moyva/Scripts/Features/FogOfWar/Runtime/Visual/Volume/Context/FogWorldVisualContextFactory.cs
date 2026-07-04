using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Builds fog visual context and visibility height data from generated world signals.
    /// Keeps world-generation data shaping out of <see cref="FogOfWarService"/>.
    /// </summary>
    internal static class FogWorldVisualContextFactory
    {
        public static float[,] BuildVisibilityHeightMap(int[,] terrainLevelMap, float[,] fallbackHeightMap)
        {
            if (terrainLevelMap == null)
                return fallbackHeightMap;

            int width = terrainLevelMap.GetLength(0);
            int height = terrainLevelMap.GetLength(1);
            var heightMap = new float[width, height];

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                heightMap[x, y] = Mathf.Max(0, terrainLevelMap[x, y]);

            return heightMap;
        }

        public static FogWorldVisualContext CreateFromSignal(WorldGeneratedDataSignal signal, int width, int height)
        {
            bool hasBounds = FogWorldSignalUtility.TryResolveMapWorldBounds(signal, out Bounds bounds);
            return new FogWorldVisualContext(
                width,
                height,
                (GridTopology)signal.GridTopology,
                (GridProjectionMode)signal.ProjectionMode,
                (GridRenderMode)signal.RenderMode,
                (GridNeighborhoodMode)signal.NeighborhoodMode,
                signal.CellSize,
                hasBounds,
                bounds,
                signal.HeightMap,
                signal.TerrainLevelMap);
        }

        public static FogWorldVisualContext CreateFallback(int width, int height)
        {
            return new FogWorldVisualContext(
                width,
                height,
                GridTopology.Orthogonal,
                GridProjectionMode.Orthographic3D,
                GridRenderMode.Mesh3D,
                GridNeighborhoodMode.Moore8,
                1f,
                false,
                default,
                null,
                null);
        }
    }
}
