using UnityEngine;

namespace Kruty1918.Moyva.Grid.API
{
    public interface IGridService
    {
        public TileData GetTileData(Vector2Int position);
        public bool TryGetTileData(Vector2Int position, out TileData tileData);
        public void SetTileData(Vector2Int position, TileData data);
        public int GridWidth { get; }
        public int GridHeight { get; }
    }
}
