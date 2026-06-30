using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    public readonly struct FogWorldVisualContext
    {
        public FogWorldVisualContext(
            int width,
            int height,
            GridTopology gridTopology,
            GridProjectionMode projectionMode,
            GridRenderMode renderMode,
            GridNeighborhoodMode neighborhoodMode,
            float cellSize,
            bool hasMapWorldBounds,
            Bounds mapWorldBounds,
            float[,] heightMap,
            int[,] terrainLevelMap)
        {
            Width = Mathf.Max(1, width);
            Height = Mathf.Max(1, height);
            GridTopology = gridTopology;
            ProjectionMode = projectionMode;
            RenderMode = renderMode;
            NeighborhoodMode = neighborhoodMode;
            CellSize = cellSize > 0.0001f ? cellSize : 1f;
            HasMapWorldBounds = hasMapWorldBounds;
            MapWorldBounds = mapWorldBounds;
            HeightMap = heightMap;
            TerrainLevelMap = terrainLevelMap;
        }

        public int Width { get; }
        public int Height { get; }
        public GridTopology GridTopology { get; }
        public GridProjectionMode ProjectionMode { get; }
        public GridRenderMode RenderMode { get; }
        public GridNeighborhoodMode NeighborhoodMode { get; }
        public float CellSize { get; }
        public bool HasMapWorldBounds { get; }
        public Bounds MapWorldBounds { get; }
        public float[,] HeightMap { get; }
        public int[,] TerrainLevelMap { get; }

        public bool IsValid => Width > 0 && Height > 0;

        public FogWorldVisualContext WithSize(int width, int height)
        {
            return new FogWorldVisualContext(
                width,
                height,
                GridTopology,
                ProjectionMode,
                RenderMode,
                NeighborhoodMode,
                CellSize,
                HasMapWorldBounds,
                MapWorldBounds,
                HeightMap,
                TerrainLevelMap);
        }
    }
}
