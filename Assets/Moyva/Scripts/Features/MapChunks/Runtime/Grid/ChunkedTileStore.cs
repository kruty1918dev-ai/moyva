using System;
using System.Collections.Generic;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.Runtime
{
    internal sealed class ChunkedTileStore : IChunkedTileStore
    {
        private readonly IMapChunkSettingsProvider _settings;
        private readonly Dictionary<MapChunkCoord, string[,]> _chunks = new();

        public ChunkedTileStore(IMapChunkSettingsProvider settings)
        {
            _settings = settings;
        }

        public int Width { get; private set; } = 1;
        public int Height { get; private set; } = 1;
        private int ChunkSize => Mathf.Max(1, _settings.ChunkSize);

        public void Resize(int width, int height)
        {
            int newWidth = Mathf.Max(1, width);
            int newHeight = Mathf.Max(1, height);
            var oldValues = Snapshot();

            Width = newWidth;
            Height = newHeight;
            _chunks.Clear();

            foreach (var item in oldValues)
            {
                if (IsValid(item.Key))
                    Set(item.Key, item.Value);
            }

            Debug.Log($"[MoyvaMapChunks] TileStore resized map={Width}x{Height} tiles, chunk={ChunkSize}x{ChunkSize} tiles.");
        }

        public string Get(Vector2Int position)
        {
            if (TryGet(position, out string tileTypeId))
                return tileTypeId;

            throw new ArgumentOutOfRangeException(nameof(position), "Position is out of grid bounds.");
        }

        public bool TryGet(Vector2Int position, out string tileTypeId)
        {
            tileTypeId = default;
            if (!IsValid(position))
                return false;

            var chunk = GetOrCreateChunk(ToChunk(position));
            Vector2Int local = ToLocal(position);
            tileTypeId = chunk[local.x, local.y];
            return true;
        }

        public void Set(Vector2Int position, string tileTypeId)
        {
            if (!IsValid(position))
                throw new ArgumentOutOfRangeException(nameof(position), "Position is out of grid bounds.");

            var chunk = GetOrCreateChunk(ToChunk(position));
            Vector2Int local = ToLocal(position);
            chunk[local.x, local.y] = tileTypeId;
        }

        private Dictionary<Vector2Int, string> Snapshot()
        {
            var values = new Dictionary<Vector2Int, string>();
            foreach (var pair in _chunks)
            {
                int baseX = pair.Key.X * ChunkSize;
                int baseY = pair.Key.Y * ChunkSize;
                for (int x = 0; x < pair.Value.GetLength(0); x++)
                for (int y = 0; y < pair.Value.GetLength(1); y++)
                {
                    var pos = new Vector2Int(baseX + x, baseY + y);
                    if (IsValid(pos))
                        values[pos] = pair.Value[x, y];
                }
            }

            return values;
        }

        private string[,] GetOrCreateChunk(MapChunkCoord coord)
        {
            if (_chunks.TryGetValue(coord, out var chunk))
                return chunk;

            chunk = new string[ChunkSize, ChunkSize];
            _chunks[coord] = chunk;
            return chunk;
        }

        private bool IsValid(Vector2Int position)
            => position.x >= 0 && position.y >= 0 && position.x < Width && position.y < Height;

        private MapChunkCoord ToChunk(Vector2Int position)
            => new MapChunkCoord(position.x / ChunkSize, position.y / ChunkSize);

        private Vector2Int ToLocal(Vector2Int position)
            => new Vector2Int(position.x % ChunkSize, position.y % ChunkSize);
    }
}
