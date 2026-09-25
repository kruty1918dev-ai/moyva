using System;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    /// <summary>
    /// Cache key for the slope-warped variant of a processed tile mesh. Covers
    /// every input that changes the generated geometry: the input mesh, the
    /// linear transform (rotation/scale — translation is irrelevant because
    /// the warp operates in translation-free space), the footprint extent and
    /// the four absolute corner heights plus the flat reference height.
    /// </summary>
    internal readonly struct TileHeightWarpMeshKey : IEquatable<TileHeightWarpMeshKey>
    {
        private const float Quantization = 10000f;

        private readonly EntityId _meshId;
        private readonly int _m00;
        private readonly int _m01;
        private readonly int _m02;
        private readonly int _m10;
        private readonly int _m11;
        private readonly int _m12;
        private readonly int _m20;
        private readonly int _m21;
        private readonly int _m22;
        private readonly int _tileHalfExtent;
        private readonly int _northWest;
        private readonly int _northEast;
        private readonly int _southWest;
        private readonly int _southEast;
        private readonly int _reference;

        private TileHeightWarpMeshKey(TileMeshSource source, Mesh mesh)
        {
            Matrix4x4 matrix = source.LocalMatrix;
            _meshId = mesh != null ? mesh.GetEntityId() : default;
            _m00 = Quantize(matrix.m00);
            _m01 = Quantize(matrix.m01);
            _m02 = Quantize(matrix.m02);
            _m10 = Quantize(matrix.m10);
            _m11 = Quantize(matrix.m11);
            _m12 = Quantize(matrix.m12);
            _m20 = Quantize(matrix.m20);
            _m21 = Quantize(matrix.m21);
            _m22 = Quantize(matrix.m22);
            _tileHalfExtent = Quantize(source.TileHalfExtent);
            TileMeshCornerHeights corners = source.CornerHeights;
            _northWest = Quantize(corners.NorthWest);
            _northEast = Quantize(corners.NorthEast);
            _southWest = Quantize(corners.SouthWest);
            _southEast = Quantize(corners.SouthEast);
            _reference = Quantize(corners.Reference);
        }

        public static TileHeightWarpMeshKey Create(TileMeshSource source, Mesh mesh)
            => new TileHeightWarpMeshKey(source, mesh);

        public bool Equals(TileHeightWarpMeshKey other)
        {
            return _meshId == other._meshId
                && _m00 == other._m00
                && _m01 == other._m01
                && _m02 == other._m02
                && _m10 == other._m10
                && _m11 == other._m11
                && _m12 == other._m12
                && _m20 == other._m20
                && _m21 == other._m21
                && _m22 == other._m22
                && _tileHalfExtent == other._tileHalfExtent
                && _northWest == other._northWest
                && _northEast == other._northEast
                && _southWest == other._southWest
                && _southEast == other._southEast
                && _reference == other._reference;
        }

        public override bool Equals(object obj)
            => obj is TileHeightWarpMeshKey other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + _meshId.GetHashCode();
                hash = hash * 31 + _m00;
                hash = hash * 31 + _m01;
                hash = hash * 31 + _m02;
                hash = hash * 31 + _m10;
                hash = hash * 31 + _m11;
                hash = hash * 31 + _m12;
                hash = hash * 31 + _m20;
                hash = hash * 31 + _m21;
                hash = hash * 31 + _m22;
                hash = hash * 31 + _tileHalfExtent;
                hash = hash * 31 + _northWest;
                hash = hash * 31 + _northEast;
                hash = hash * 31 + _southWest;
                hash = hash * 31 + _southEast;
                hash = hash * 31 + _reference;
                return hash;
            }
        }

        private static int Quantize(float value)
            => Mathf.RoundToInt(value * Quantization);
    }
}
