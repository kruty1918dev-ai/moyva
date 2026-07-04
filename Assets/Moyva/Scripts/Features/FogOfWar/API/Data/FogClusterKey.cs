using System;

namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Унікальний spatial key runtime fog mesh cluster-а.
    /// Fog state і height bucket є вмістом mesh-а, а не частиною identity cluster-а.
    /// </summary>
    public readonly struct FogClusterKey : IEquatable<FogClusterKey>
    {
        public FogClusterKey(int clusterX, int clusterY)
        {
            ClusterX = clusterX;
            ClusterY = clusterY;
        }

        public int ClusterX { get; }
        public int ClusterY { get; }

        public bool Equals(FogClusterKey other)
            => ClusterX == other.ClusterX && ClusterY == other.ClusterY;

        public override bool Equals(object obj)
            => obj is FogClusterKey other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + ClusterX;
                hash = hash * 31 + ClusterY;
                return hash;
            }
        }

        public override string ToString()
            => $"({ClusterX},{ClusterY})";
    }
}
