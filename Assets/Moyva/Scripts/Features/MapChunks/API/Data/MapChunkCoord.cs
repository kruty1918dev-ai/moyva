using System;

namespace Kruty1918.Moyva.MapChunks.API
{
    public readonly struct MapChunkCoord : IEquatable<MapChunkCoord>
    {
        public MapChunkCoord(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }

        public bool Equals(MapChunkCoord other) => X == other.X && Y == other.Y;

        public override bool Equals(object obj)
            => obj is MapChunkCoord other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public override string ToString() => $"({X},{Y})";
    }
}
