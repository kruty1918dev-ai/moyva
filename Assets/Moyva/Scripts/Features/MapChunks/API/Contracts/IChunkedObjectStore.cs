using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.API
{
    public interface IChunkedObjectStore
    {
        bool IsOccupied(Vector2Int position);
        bool TryGetOccupant(Vector2Int position, out string occupantId);
        bool TryGetPosition(string occupantId, out Vector2Int position);
        void Register(Vector2Int position, string occupantId);
        void Move(Vector2Int from, Vector2Int to);
        void Unregister(Vector2Int position);
        void Clear();
    }
}
