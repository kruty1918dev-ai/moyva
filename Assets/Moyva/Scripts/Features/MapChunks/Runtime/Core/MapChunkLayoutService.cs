using System.Collections.Generic;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.Runtime
{
    public sealed class MapChunkLayoutService : IMapChunkLayoutService
    {
        private readonly IMapChunkSettingsProvider _settings;
        private readonly List<MapChunkDescriptor> _chunks = new();
        private readonly Dictionary<MapChunkCoord, MapChunkDescriptor> _byCoord = new();

        public MapChunkLayoutService(IMapChunkSettingsProvider settings)
        {
            _settings = settings;
        }

        public bool IsConfigured { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int ChunkSize => Mathf.Max(1, _settings.ChunkSize);
        public float CellSize { get; private set; } = 1f;
        public IReadOnlyList<MapChunkDescriptor> Chunks => _chunks;

        public void Configure(int width, int height, float cellSize, bool hasWorldBounds, Bounds worldBounds)
        {
            int requestedWidth = Mathf.Max(1, width);
            int requestedHeight = Mathf.Max(1, height);
            int chunkSize = ChunkSize;
            Vector2Int effective = FullChunkMapSizeUtility.CropDown(
                requestedWidth,
                requestedHeight,
                chunkSize);

            Width = effective.x;
            Height = effective.y;

            CellSize = ResolveCellSize(
                requestedWidth,
                requestedHeight,
                cellSize,
                hasWorldBounds,
                worldBounds);

            Bounds bounds = hasWorldBounds
                ? CreateCroppedBounds(worldBounds, Width, Height, CellSize)
                : CreateFallbackBounds(Width, Height, CellSize);

            _chunks.Clear();
            _byCoord.Clear();

            int countX = Width / chunkSize;
            int countY = Height / chunkSize;

            for (int y = 0; y < countY; y++)
            for (int x = 0; x < countX; x++)
                AddChunk(new MapChunkCoord(x, y), CellSize, bounds);

            IsConfigured = true;

            if (requestedWidth != Width || requestedHeight != Height)
            {
                Debug.LogWarning(
                    "[MOYVA_CHUNK_SIZE] LAYOUT_CROPPED " +
                    $"requested={requestedWidth}x{requestedHeight} " +
                    $"effective={Width}x{Height} " +
                    $"chunkSize={chunkSize} " +
                    $"trimmed={requestedWidth - Width}x{requestedHeight - Height} " +
                    "partialChunks=0");
            }

            Debug.Log(
                $"[MoyvaMapChunks] Layout configured map={Width}x{Height} tiles, " +
                $"chunk={chunkSize}x{chunkSize} tiles, chunks={countX}x{countY} ({_chunks.Count}), " +
                $"cellSize={CellSize:0.###}.");
        }

        public bool TryGetChunkCoord(Vector2Int tile, out MapChunkCoord coord)
        {
            coord = default;
            if (tile.x < 0 || tile.y < 0 || tile.x >= Width || tile.y >= Height)
                return false;

            coord = new MapChunkCoord(tile.x / ChunkSize, tile.y / ChunkSize);
            return true;
        }

        public bool TryGetDescriptor(MapChunkCoord coord, out MapChunkDescriptor descriptor)
            => _byCoord.TryGetValue(coord, out descriptor);

        public int GetChunksOverlapping(Bounds worldBounds, List<MapChunkCoord> results)
        {
            results?.Clear();
            if (results == null || !IsConfigured)
                return 0;

            for (int i = 0; i < _chunks.Count; i++)
            {
                if (_chunks[i].WorldBounds.Intersects(worldBounds))
                    results.Add(_chunks[i].Coord);
            }

            return results.Count;
        }

        private void AddChunk(MapChunkCoord coord, float cellSize, Bounds mapBounds)
        {
            int xMin = coord.X * ChunkSize;
            int yMin = coord.Y * ChunkSize;
            var tileRect = new RectInt(xMin, yMin, ChunkSize, ChunkSize);
            var descriptor = new MapChunkDescriptor(
                coord,
                tileRect,
                CreateWorldBounds(tileRect, cellSize, mapBounds));
            _chunks.Add(descriptor);
            _byCoord[coord] = descriptor;
        }

        private static Bounds CreateWorldBounds(RectInt rect, float cellSize, Bounds mapBounds)
        {
            Vector3 min = mapBounds.min;
            float xMin = min.x + rect.xMin * cellSize;
            float zMin = min.z + rect.yMin * cellSize;
            var size = new Vector3(rect.width * cellSize, mapBounds.size.y, rect.height * cellSize);
            var center = new Vector3(xMin + size.x * 0.5f, mapBounds.center.y, zMin + size.z * 0.5f);
            return new Bounds(center, size);
        }

        private static Bounds CreateCroppedBounds(
            Bounds requestedBounds,
            int width,
            int height,
            float cellSize)
        {
            Vector3 min = requestedBounds.min;
            Vector3 size = new Vector3(
                width * cellSize,
                requestedBounds.size.y,
                height * cellSize);
            Vector3 center = new Vector3(
                min.x + size.x * 0.5f,
                requestedBounds.center.y,
                min.z + size.z * 0.5f);
            return new Bounds(center, size);
        }

        private static Bounds CreateFallbackBounds(int width, int height, float cellSize)
            => new Bounds(
                new Vector3(width * cellSize * 0.5f, 0.5f, height * cellSize * 0.5f),
                new Vector3(width * cellSize, 1f, height * cellSize));

        private static float ResolveCellSize(int width, int height, float signalCellSize, bool hasWorldBounds, Bounds worldBounds)
        {
            if (hasWorldBounds)
            {
                float cellX = Mathf.Abs(worldBounds.size.x) / Mathf.Max(1, width);
                float cellZ = Mathf.Abs(worldBounds.size.z) / Mathf.Max(1, height);
                if (cellX > 0.0001f && cellZ > 0.0001f)
                    return Mathf.Min(cellX, cellZ);
            }

            return Mathf.Max(0.0001f, signalCellSize);
        }
    }
}
