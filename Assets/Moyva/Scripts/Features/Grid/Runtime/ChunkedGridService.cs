using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;

namespace Kruty1918.Moyva.Grid.Runtime
{
    internal sealed class ChunkedGridService : IGridService, IGridResizeService
    {
        private readonly IChunkedTileStore _tiles;

        public ChunkedGridService(IChunkedTileStore tiles, int gridWidth, int gridHeight)
        {
            _tiles = tiles;
            _tiles.Resize(gridWidth, gridHeight);
        }

        public int GridWidth => _tiles.Width;
        public int GridHeight => _tiles.Height;

        public string GetTileData(Vector2Int position)
            => _tiles.Get(position);

        public bool TryGetTileData(Vector2Int position, out string tileTypeId)
            => _tiles.TryGet(position, out tileTypeId);

        public void SetTileData(Vector2Int position, string tileTypeId)
            => _tiles.Set(position, tileTypeId);

        public void Resize(int width, int height)
            => _tiles.Resize(width, height);
    }
}
