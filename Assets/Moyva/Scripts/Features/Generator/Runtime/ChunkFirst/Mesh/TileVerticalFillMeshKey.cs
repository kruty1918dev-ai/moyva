using System;
using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal readonly struct TileVerticalFillMeshKey : IEquatable<TileVerticalFillMeshKey>
    {
        private const float Quantization = 10000f;

        private readonly int _meshId;
        private readonly int _m00;
        private readonly int _m01;
        private readonly int _m02;
        private readonly int _m10;
        private readonly int _m11;
        private readonly int _m12;
        private readonly int _m20;
        private readonly int _m21;
        private readonly int _m22;
        private readonly int _relativeBottom;
        private readonly int _occludedSides;
        private readonly int _tileHalfExtent;
        private readonly int _authoredClosurePolicy;
        private readonly int _generateMissingClosure;
        private readonly int _northBottom;
        private readonly int _eastBottom;
        private readonly int _southBottom;
        private readonly int _westBottom;

        private TileVerticalFillMeshKey(TileMeshSource source)
        {
            Matrix4x4 matrix = source.LocalMatrix;
            _meshId = source.Mesh != null ? source.Mesh.GetInstanceID() : 0;
            _m00 = Quantize(matrix.m00);
            _m01 = Quantize(matrix.m01);
            _m02 = Quantize(matrix.m02);
            _m10 = Quantize(matrix.m10);
            _m11 = Quantize(matrix.m11);
            _m12 = Quantize(matrix.m12);
            _m20 = Quantize(matrix.m20);
            _m21 = Quantize(matrix.m21);
            _m22 = Quantize(matrix.m22);
            _relativeBottom = Quantize(source.VisibleBottomY - matrix.m13);
            _occludedSides = (int)source.OccludedSides;
            _tileHalfExtent = Quantize(source.TileHalfExtent);
            _authoredClosurePolicy = (int)source.AuthoredClosurePolicy;
            _generateMissingClosure = source.GenerateMissingClosure ? 1 : 0;
            _northBottom = Quantize(source.EdgeBottoms.Resolve(
                TileMeshOccludedSides.North,
                source.VisibleBottomY) - matrix.m13);
            _eastBottom = Quantize(source.EdgeBottoms.Resolve(
                TileMeshOccludedSides.East,
                source.VisibleBottomY) - matrix.m13);
            _southBottom = Quantize(source.EdgeBottoms.Resolve(
                TileMeshOccludedSides.South,
                source.VisibleBottomY) - matrix.m13);
            _westBottom = Quantize(source.EdgeBottoms.Resolve(
                TileMeshOccludedSides.West,
                source.VisibleBottomY) - matrix.m13);
        }

        public static TileVerticalFillMeshKey Create(TileMeshSource source)
            => new TileVerticalFillMeshKey(source);

        public bool Equals(TileVerticalFillMeshKey other)
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
                && _relativeBottom == other._relativeBottom
                && _occludedSides == other._occludedSides
                && _tileHalfExtent == other._tileHalfExtent
                && _authoredClosurePolicy == other._authoredClosurePolicy
                && _generateMissingClosure == other._generateMissingClosure
                && _northBottom == other._northBottom
                && _eastBottom == other._eastBottom
                && _southBottom == other._southBottom
                && _westBottom == other._westBottom;
        }

        public override bool Equals(object obj)
            => obj is TileVerticalFillMeshKey other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + _meshId;
                hash = hash * 31 + _m00;
                hash = hash * 31 + _m01;
                hash = hash * 31 + _m02;
                hash = hash * 31 + _m10;
                hash = hash * 31 + _m11;
                hash = hash * 31 + _m12;
                hash = hash * 31 + _m20;
                hash = hash * 31 + _m21;
                hash = hash * 31 + _m22;
                hash = hash * 31 + _relativeBottom;
                hash = hash * 31 + _occludedSides;
                hash = hash * 31 + _tileHalfExtent;
                hash = hash * 31 + _authoredClosurePolicy;
                hash = hash * 31 + _generateMissingClosure;
                hash = hash * 31 + _northBottom;
                hash = hash * 31 + _eastBottom;
                hash = hash * 31 + _southBottom;
                hash = hash * 31 + _westBottom;
                return hash;
            }
        }

        private static int Quantize(float value)
            => Mathf.RoundToInt(value * Quantization);
    }
}
