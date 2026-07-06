using System;
using System.Collections.Generic;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.Runtime
{
    internal sealed class ChunkedObjectStore : IChunkedObjectStore
    {
        private readonly IMapChunkSettingsProvider _settings;
        private readonly Dictionary<Vector2Int, string> _occupants = new();
        private readonly Dictionary<string, Vector2Int> _positions = new();
        private readonly Dictionary<MapChunkCoord, HashSet<Vector2Int>> _chunkPositions = new();

        public ChunkedObjectStore(IMapChunkSettingsProvider settings)
        {
            _settings = settings;
        }

        private int ChunkSize => Mathf.Max(1, _settings.ChunkSize);

        public bool IsOccupied(Vector2Int position) => _occupants.ContainsKey(position);

        public bool TryGetOccupant(Vector2Int position, out string occupantId)
            => _occupants.TryGetValue(position, out occupantId);

        public bool TryGetPosition(string occupantId, out Vector2Int position)
            => _positions.TryGetValue(occupantId, out position);

        public void Register(Vector2Int position, string occupantId)
        {
            if (_occupants.TryGetValue(position, out string existing))
                throw new InvalidOperationException($"Position {position} is already occupied by '{existing}'.");

            _occupants[position] = occupantId;
            _positions[occupantId] = position;
            GetChunkSet(ToChunk(position)).Add(position);
        }

        public void Move(Vector2Int from, Vector2Int to)
        {
            if (!_occupants.TryGetValue(from, out string occupantId))
                throw new InvalidOperationException($"Cannot move: position {from} is empty.");
            if (_occupants.ContainsKey(to))
                throw new InvalidOperationException($"Cannot move '{occupantId}' to {to}: destination is occupied.");

            Unregister(from);
            Register(to, occupantId);
        }

        public void Unregister(Vector2Int position)
        {
            if (!_occupants.TryGetValue(position, out string occupantId))
                return;

            _occupants.Remove(position);
            _positions.Remove(occupantId);
            if (_chunkPositions.TryGetValue(ToChunk(position), out var set))
                set.Remove(position);
        }

        public void Clear()
        {
            _occupants.Clear();
            _positions.Clear();
            _chunkPositions.Clear();
        }

        private HashSet<Vector2Int> GetChunkSet(MapChunkCoord coord)
        {
            if (_chunkPositions.TryGetValue(coord, out var set))
                return set;

            set = new HashSet<Vector2Int>();
            _chunkPositions[coord] = set;
            return set;
        }

        private MapChunkCoord ToChunk(Vector2Int position)
            => new MapChunkCoord(position.x / ChunkSize, position.y / ChunkSize);
    }
}
