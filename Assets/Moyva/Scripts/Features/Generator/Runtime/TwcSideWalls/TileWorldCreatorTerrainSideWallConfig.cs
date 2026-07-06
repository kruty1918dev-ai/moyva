using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal readonly struct TileWorldCreatorTerrainSideWallConfig
    {
        public const float DefaultCellSize = 1f;

        public TileWorldCreatorTerrainSideWallConfig(
            Transform targetRoot,
            int[,] terrainLevelMap,
            float cellSize,
            int heightStep,
            float baseY,
            Material materialOverride,
            Color wallColor,
            bool includeMapBoundaryWalls)
        {
            TargetRoot = targetRoot;
            TerrainLevelMap = terrainLevelMap;
            CellSize = cellSize > 0.0001f ? cellSize : DefaultCellSize;
            HeightStep = Mathf.Max(1, heightStep);
            BaseY = baseY;
            MaterialOverride = materialOverride;
            WallColor = wallColor;
            IncludeMapBoundaryWalls = includeMapBoundaryWalls;
        }

        public Transform TargetRoot { get; }
        public int[,] TerrainLevelMap { get; }
        public float CellSize { get; }
        public int HeightStep { get; }
        public float BaseY { get; }
        public Material MaterialOverride { get; }
        public Color WallColor { get; }
        public bool IncludeMapBoundaryWalls { get; }
    }
}
