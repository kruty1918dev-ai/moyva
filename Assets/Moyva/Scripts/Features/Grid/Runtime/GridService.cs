using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Grid.Runtime
{
    /// <summary>
    /// Приватна реалізація сервісу сітки.
    /// </summary>
    internal sealed class GridService : IGridService
    {
        private readonly TileData[,] _grid;
        public int GridWidth { get; }
        public int GridHeight { get; }
        public float TileSize { get; }

        private readonly SignalBus _signalBus;

        public GridService(int gridWidth, int gridHeight, float tileSize, SignalBus signalBus)
        {
            _grid = new TileData[gridWidth, gridHeight];
            GridWidth = gridWidth;
            GridHeight = gridHeight;
            TileSize = tileSize;
            _signalBus = signalBus;
        }

        public TileData GetTileData(Vector2Int position)
        {
            if (IsValidPosition(position))
                return _grid[position.x, position.y];
            throw new System.ArgumentOutOfRangeException(nameof(position), "Position is out of grid bounds.");
        }

        public void SetTileData(Vector2Int position, TileData data)
        {
            if (!IsValidPosition(position))
                throw new System.ArgumentOutOfRangeException(nameof(position), "Position is out of grid bounds.");

            _grid[position.x, position.y] = data;
            _signalBus.Fire(new OnTileChanged { Position = position });
        }

        private bool IsValidPosition(Vector2Int position)
        {
            return position.x >= 0 && position.x < GridWidth && position.y >= 0 && position.y < GridHeight;
        }

        public bool TryGetTileData(Vector2Int position, out TileData tileData)
        {
            if (IsValidPosition(position))
            {
                tileData = _grid[position.x, position.y];
                return true;
            }

            tileData = default;
            return false;
        }
    }
}
