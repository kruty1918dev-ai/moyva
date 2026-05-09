using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Grid.Runtime
{
    /// <summary>
    /// Приватна реалізація сервісу сітки.
    /// </summary>
    internal sealed class GridService : IGridService, IGridResizeService
    {
        private string[,] _grid;
        public int GridWidth { get; private set; }
        public int GridHeight { get; private set; }

        public GridService(int gridWidth, int gridHeight)
        {
            GridWidth = Mathf.Max(1, gridWidth);
            GridHeight = Mathf.Max(1, gridHeight);
            _grid = new string[GridWidth, GridHeight];
        }

        public void Resize(int width, int height)
        {
            int safeWidth = Mathf.Max(1, width);
            int safeHeight = Mathf.Max(1, height);
            if (safeWidth == GridWidth && safeHeight == GridHeight)
                return;

            var resized = new string[safeWidth, safeHeight];
            int copyWidth = Mathf.Min(GridWidth, safeWidth);
            int copyHeight = Mathf.Min(GridHeight, safeHeight);
            for (int x = 0; x < copyWidth; x++)
                for (int y = 0; y < copyHeight; y++)
                    resized[x, y] = _grid[x, y];

            _grid = resized;
            GridWidth = safeWidth;
            GridHeight = safeHeight;
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
