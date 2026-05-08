using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    public readonly struct HillLevelTileData
    {
        public int X { get; }
        public int Y { get; }
        public string TileId { get; }
        public string SourceTileId { get; }
        public string DirectionId { get; }
        public float Height { get; }
        public int Level { get; }
        public bool WasModified { get; }

        public HillLevelTileData(int x, int y, string tileId, string sourceTileId,
            string directionId, float height, int level, bool wasModified)
        {
            X = x;
            Y = y;
            TileId = tileId;
            SourceTileId = sourceTileId;
            DirectionId = directionId;
            Height = height;
            Level = level;
            WasModified = wasModified;
        }

        public Vector2Int Position => new(X, Y);
    }

    public sealed class HillLevelDataMap
    {
        private readonly HillLevelTileData[,] _tiles;

        public int Width { get; }
        public int Height { get; }

        public HillLevelDataMap(HillLevelTileData[,] tiles)
        {
            _tiles = tiles != null
                ? (HillLevelTileData[,])tiles.Clone()
                : new HillLevelTileData[0, 0];

            Width = _tiles.GetLength(0);
            Height = _tiles.GetLength(1);
        }

        public bool TryGetTile(int x, int y, out HillLevelTileData data)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                data = default;
                return false;
            }

            data = _tiles[x, y];
            return true;
        }

        public HillLevelTileData GetTile(int x, int y) => _tiles[x, y];

        public HillLevelTileData[,] CopyTiles()
            => (HillLevelTileData[,])_tiles.Clone();

        public HillLevelDataMap Clone() => new(_tiles);
    }
}