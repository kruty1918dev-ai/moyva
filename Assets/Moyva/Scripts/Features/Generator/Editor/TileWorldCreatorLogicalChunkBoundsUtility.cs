using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    internal static class TileWorldCreatorLogicalChunkBoundsUtility
    {
        public static bool TryCollectAll(TileWorldCreatorManager manager, List<Bounds> results)
        {
            results?.Clear();
            if (results == null || !TryResolveGrid(manager, out Grid grid))
                return false;

            int countX = Mathf.CeilToInt(grid.Width / (float)grid.ChunkSize);
            int countY = Mathf.CeilToInt(grid.Height / (float)grid.ChunkSize);
            for (int y = 0; y < countY; y++)
            for (int x = 0; x < countX; x++)
                results.Add(CreateBounds(grid, x, y));

            return results.Count > 0;
        }

        public static bool TryCollectAt(TileWorldCreatorManager manager, Vector3 worldPosition, out Bounds bounds)
        {
            bounds = default;
            if (!TryResolveGrid(manager, out Grid grid))
                return false;

            float localX = worldPosition.x - grid.Origin.x;
            float localZ = worldPosition.z - grid.Origin.z;
            if (localX < 0f || localZ < 0f)
                return false;

            int chunkX = Mathf.FloorToInt(localX / (grid.CellSize * grid.ChunkSize));
            int chunkY = Mathf.FloorToInt(localZ / (grid.CellSize * grid.ChunkSize));
            int countX = Mathf.CeilToInt(grid.Width / (float)grid.ChunkSize);
            int countY = Mathf.CeilToInt(grid.Height / (float)grid.ChunkSize);
            if (chunkX < 0 || chunkY < 0 || chunkX >= countX || chunkY >= countY)
                return false;

            bounds = CreateBounds(grid, chunkX, chunkY);
            return true;
        }

        private static Bounds CreateBounds(in Grid grid, int chunkX, int chunkY)
        {
            int tileX = chunkX * grid.ChunkSize;
            int tileY = chunkY * grid.ChunkSize;
            int width = Mathf.Min(grid.ChunkSize, grid.Width - tileX);
            int height = Mathf.Min(grid.ChunkSize, grid.Height - tileY);
            var size = new Vector3(width * grid.CellSize, grid.HeightSize, height * grid.CellSize);
            var center = new Vector3(
                grid.Origin.x + (tileX + width * 0.5f) * grid.CellSize,
                grid.CenterY,
                grid.Origin.z + (tileY + height * 0.5f) * grid.CellSize);
            return new Bounds(center, size);
        }

        private static bool TryResolveGrid(TileWorldCreatorManager manager, out Grid grid)
        {
            grid = default;
            Configuration config = manager != null ? manager.configuration : null;
            if (config == null || config.width <= 0 || config.height <= 0)
                return false;

            float cellSize = config.cellSize > 0.0001f ? config.cellSize : 1f;
            int chunkSize = Mathf.Max(1, config.clusterCellSize);
            Bounds mapBounds = TileWorldCreatorMapBoundsCollector.Collect(manager);
            Vector3 origin = manager.transform.position;
            float centerY = mapBounds.size.y > 0.0001f ? mapBounds.center.y : manager.transform.position.y;
            float heightSize = mapBounds.size.y > 0.0001f ? mapBounds.size.y : 1f;
            grid = new Grid(config.width, config.height, chunkSize, cellSize, origin, centerY, heightSize);
            return true;
        }

        private readonly struct Grid
        {
            public Grid(int width, int height, int chunkSize, float cellSize, Vector3 origin, float centerY, float heightSize)
            {
                Width = width;
                Height = height;
                ChunkSize = chunkSize;
                CellSize = cellSize;
                Origin = origin;
                CenterY = centerY;
                HeightSize = heightSize;
            }

            public int Width { get; }
            public int Height { get; }
            public int ChunkSize { get; }
            public float CellSize { get; }
            public Vector3 Origin { get; }
            public float CenterY { get; }
            public float HeightSize { get; }
        }
    }
}
