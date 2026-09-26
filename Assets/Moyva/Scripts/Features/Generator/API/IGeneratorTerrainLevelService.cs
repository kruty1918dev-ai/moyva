using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    public interface IGeneratorTerrainLevelService
    {
        bool HasLevelMap { get; }
        bool HasSurfaceHeightMap { get; }
        bool HasExplicitSurfaceHeightMap { get; }
        int Width { get; }
        int Height { get; }
        HillLevelDataMap CurrentHillLevelData { get; }

        void Clear();
        void SetLevelMap(int[,] levelMap);
        void SetSurfaceHeightMap(float[,] surfaceHeightMap);
        void SetHillLevelData(HillLevelDataMap data);
        bool TryGetLevel(Vector2Int position, out int level);
        bool TryGetSurfaceHeight(Vector2Int position, out float surfaceY);

        /// <summary>
        /// Approximates the terrain surface normal at a cell from the surface
        /// height map via central differences. <paramref name="cellSize"/> is
        /// the world-space size of one cell. Default implementation samples
        /// <see cref="TryGetSurfaceHeight"/>; implementers may override.
        /// </summary>
        bool TryGetSurfaceNormal(Vector2Int position, float cellSize, out Vector3 normal)
        {
            normal = Vector3.up;
            if (!TryGetSurfaceHeight(position, out float center))
                return false;

            float step = Mathf.Max(0.0001f, cellSize);
            float left = SampleOr(position + Vector2Int.left, center);
            float right = SampleOr(position + Vector2Int.right, center);
            float down = SampleOr(position + Vector2Int.down, center);
            float up = SampleOr(position + Vector2Int.up, center);

            float dx = (right - left) / (2f * step);
            float dy = (up - down) / (2f * step);
            normal = new Vector3(-dx, 1f, -dy).normalized;
            return true;

            float SampleOr(Vector2Int cell, float fallback)
                => TryGetSurfaceHeight(cell, out float h) ? h : fallback;
        }

        int GetLevelOrDefault(Vector2Int position, int fallback = 0);
        int[,] CopyLevelMap();
        float[,] CopySurfaceHeightMap();
    }

    /// <summary>
    /// Optional change-version contract for consumers that cache derived
    /// terrain data. Existing IGeneratorTerrainLevelService implementations
    /// remain source-compatible.
    /// </summary>
    public interface IGeneratorTerrainDataVersionProvider
    {
        int TerrainDataVersion { get; }
    }

    /// <summary>
    /// Optional water-surface contract: the generated world's wet mask —
    /// cells whose logical stack carries a SurfaceOnly terrain sample
    /// (sea, rivers and lakes rendered over land tile ids). Kept separate so
    /// existing IGeneratorTerrainLevelService implementations remain
    /// source-compatible.
    /// </summary>
    public interface IGeneratorTerrainWaterService
    {
        bool HasWaterMap { get; }
        void SetWaterMap(bool[,] waterMap);
        bool TryGetWaterCell(Vector2Int position, out bool isWater);
    }
}
