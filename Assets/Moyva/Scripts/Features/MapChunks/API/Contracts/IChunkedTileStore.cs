using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.API
{
    public interface IChunkedTileStore
    {
        int Width { get; }
        int Height { get; }

        void Resize(int width, int height);
        string Get(Vector2Int position);
        bool TryGet(Vector2Int position, out string tileTypeId);
        void Set(Vector2Int position, string tileTypeId);
    }
}
