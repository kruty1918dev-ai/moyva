using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Grid.Runtime
{
    /// <summary>
    /// Приватна реалізація сервісу сітки.
    /// </summary>
    internal sealed class GridService : IGridService
    {
        private readonly string[,] _grid;
        public int GridWidth { get; }
        public int GridHeight { get; }

        public GridService(int gridWidth, int gridHeight)
        {
            _grid = new string[gridWidth, gridHeight];
            GridWidth = gridWidth;
            GridHeight = gridHeight;
        }

        public string GetTileData(Vector2Int position)
        {
            if (IsValidPosition(position))
                return _grid[position.x, position.y];
            throw new System.ArgumentOutOfRangeException(nameof(position), "Position is out of grid bounds.");
        }

        public void SetTileData(Vector2Int position, string tileTypeId)
        {
            if (!IsValidPosition(position))
                throw new System.ArgumentOutOfRangeException(nameof(position), "Position is out of grid bounds.");

            _grid[position.x, position.y] = tileTypeId;
        }

        private bool IsValidPosition(Vector2Int position)
        {
            return position.x >= 0 && position.x < GridWidth && position.y >= 0 && position.y < GridHeight;
        }

        public bool TryGetTileData(Vector2Int position, out string tileTypeId)
        {
            if (IsValidPosition(position))
            {
                tileTypeId = _grid[position.x, position.y];
                return true;
            }

            tileTypeId = default;
            return false;
        }
    }
}
