using UnityEngine;

namespace Kruty1918.Moyva.Grid.API
{
    public interface IGridService
    {
        public string GetTileData(Vector2Int position);
        public bool TryGetTileData(Vector2Int position, out string tileTypeId);
        public void SetTileData(Vector2Int position, string tileTypeId);
        public int GridWidth { get; }
        public int GridHeight { get; }
    }
}
