using UnityEngine;

namespace Kruty1918.Moyva.Grid.API
{
    public interface IGridService
    {
        public TileData GetTileData(Vector2Int position);
        public bool IsTileOccupied(Vector2Int position);
        public bool TryGetTileData(Vector2Int position, out TileData tileData);
        public void OccupyTile(Vector2Int position, string occupantId);
        public int GridWidth { get; }
        public int GridHeight { get; }
        public float TileSize { get; }
    }
}
