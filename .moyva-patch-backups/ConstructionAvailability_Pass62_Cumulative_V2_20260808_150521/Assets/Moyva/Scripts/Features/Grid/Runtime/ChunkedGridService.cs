using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.MapChunks.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Grid.Runtime
{
    internal sealed class ChunkedGridService :
        IGridService,
        IGridResizeService,
        IGridTileIdentityQuery
    {
        private readonly IChunkedTileStore _tiles;
        private readonly SignalBus _signalBus;

        public ChunkedGridService(
            IChunkedTileStore tiles,
            int gridWidth,
            int gridHeight,
            [InjectOptional] SignalBus signalBus = null)
        {
            _tiles = tiles;
            _signalBus = signalBus;
            _tiles.Resize(gridWidth, gridHeight);
        }

        public int GridWidth => _tiles.Width;
        public int GridHeight => _tiles.Height;

        public string GetTileData(Vector2Int position)
            => _tiles.Get(position);

        public bool TryGetTileData(Vector2Int position, out string tileTypeId)
            => _tiles.TryGet(position, out tileTypeId);

        public bool ContainsCell(Vector2Int position)
            => position.x >= 0
                && position.y >= 0
                && position.x < GridWidth
                && position.y < GridHeight;

        public bool TryGetTileTypeId(
            Vector2Int position,
            out string tileTypeId)
        {
            tileTypeId = null;
            return _tiles.TryGet(
                    position,
                    out tileTypeId)
                && !string.IsNullOrWhiteSpace(tileTypeId);
        }

        public string GetTileTypeId(Vector2Int position)
        {
            if (!ContainsCell(position))
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(position),
                    "Position is out of grid bounds.");
            }

            if (TryGetTileTypeId(
                    position,
                    out string tileTypeId))
            {
                return tileTypeId;
            }

            throw new System.InvalidOperationException(
                $"Grid cell {position} does not have an assigned TileTypeId.");
        }

        public void SetTileData(Vector2Int position, string tileTypeId)
        {
            _tiles.TryGet(position, out string previousTileId);
            _tiles.Set(position, tileTypeId);
            if (string.Equals(previousTileId, tileTypeId, System.StringComparison.Ordinal))
                return;

            _signalBus?.Fire(new GridTileChangedSignal
            {
                Position = position,
                PreviousTileId = previousTileId,
                CurrentTileId = tileTypeId,
            });
        }

        public void Resize(int width, int height)
            => _tiles.Resize(width, height);
    }
}
