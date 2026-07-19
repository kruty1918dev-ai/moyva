using System;
using System.Collections.Generic;
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
                && _relativeBottom == other._relativeBottom;
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
                return hash;
            }
        }

        private static int Quantize(float value)
            => Mathf.RoundToInt(value * Quantization);
    }

    internal static class TileVerticalFillMeshUtility
    {
        private const float HeightEpsilon = 0.0001f;
        private const float FlatMeshEpsilon = 0.001f;
        private const float EdgeWeldPrecision = 10000f;

        public static bool TryCreate(TileMeshSource source, out Mesh result)
        {
            result = null;
            if (!source.IsValid || !source.HasVisibleBottomY)
                return false;

            Vector3[] sourceVertices = source.Mesh.vertices;
            if (sourceVertices == null || sourceVertices.Length == 0)
                return false;

            Matrix4x4 linearMatrix = source.LocalMatrix;
            float translationY = linearMatrix.m13;
            linearMatrix.m03 = 0f;
            linearMatrix.m13 = 0f;
            linearMatrix.m23 = 0f;

            if (Mathf.Abs(linearMatrix.determinant) <= HeightEpsilon)
                return false;

            Matrix4x4 inverseLinear = linearMatrix.inverse;
            float targetBottom = source.VisibleBottomY - translationY;

            var originalRelativeY = new float[sourceVertices.Length];
            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;
            for (int i = 0; i < sourceVertices.Length; i++)
            {
                float y = linearMatrix.MultiplyPoint3x4(sourceVertices[i]).y;
                originalRelativeY[i] = y;
                minY = Mathf.Min(minY, y);
                maxY = Mathf.Max(maxY, y);
            }

            float meshHeight = maxY - minY;
            if (meshHeight <= FlatMeshEpsilon)
            {
                if (targetBottom >= minY - HeightEpsilon)
                    return false;

                result = CreateFlatMeshWithSkirt(
                    source.Mesh,
                    sourceVertices,
                    linearMatrix,
                    inverseLinear,
                    targetBottom);
                return result != null;
            }

            if (targetBottom < minY - HeightEpsilon)
            {
                Vector3[] deformed = (Vector3[])sourceVertices.Clone();
                float bottomBand = minY + Mathf.Max(
                    HeightEpsilon * 10f,
                    meshHeight * 0.02f);

                for (int i = 0; i < deformed.Length; i++)
                {
                    Vector3 relative = linearMatrix.MultiplyPoint3x4(deformed[i]);
                    if (relative.y <= bottomBand)
                    {
                        relative.y = targetBottom;
                        deformed[i] = inverseLinear.MultiplyPoint3x4(relative);
                    }
                }

                result = CopyMesh(
                    source.Mesh,
                    deformed,
                    originalRelativeY,
                    removeFullyHiddenTriangles: false,
                    targetBottom);
                return result != null;
            }

            if (targetBottom > minY + HeightEpsilon)
            {
                // Do not collapse crossing triangles onto the floor. That produced
                // overlapping faces and bright white bloom artifacts. We only remove
                // triangles that are completely hidden below global Y=0.
                result = CopyMesh(
                    source.Mesh,
                    sourceVertices,
                    originalRelativeY,
                    removeFullyHiddenTriangles: true,
                    targetBottom);
                return result != null;
            }

            return false;
        }

        private static Mesh CopyMesh(
            Mesh source,
            Vector3[] vertices,
            IReadOnlyList<float> originalRelativeY,
            bool removeFullyHiddenTriangles,
            float targetBottom)
        {
            var mesh = new Mesh
            {
                name = source.name + "_VerticalFill",
                indexFormat = source.indexFormat,
                vertices = vertices
            };

            CopyVertexChannels(source, mesh);

            mesh.subMeshCount = source.subMeshCount;
            for (int subMesh = 0; subMesh < source.subMeshCount; subMesh++)
            {
                MeshTopology topology = source.GetTopology(subMesh);
                int[] indices = source.GetIndices(subMesh);
                if (removeFullyHiddenTriangles && topology == MeshTopology.Triangles)
                {
                    indices = RemoveFullyHiddenTriangles(
                        indices,
                        originalRelativeY,
                        targetBottom);
                }

                mesh.SetIndices(indices, topology, subMesh, false);
            }

            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateFlatMeshWithSkirt(
            Mesh source,
            IReadOnlyList<Vector3> vertices,
            Matrix4x4 linearMatrix,
            Matrix4x4 inverseLinear,
            float targetBottom)
        {
            Dictionary<GeometricEdgeKey, BoundaryEdge> edges =
                CollectBoundaryEdges(source, vertices, linearMatrix);

            var skirtVertices = new List<Vector3>();
            var skirtUvs = new List<Vector2>();
            var skirtTriangles = new List<int>();

            foreach (BoundaryEdge edge in edges.Values)
            {
                if (edge.Count != 1)
                    continue;

                Vector3 topA = linearMatrix.MultiplyPoint3x4(vertices[edge.A]);
                Vector3 topB = linearMatrix.MultiplyPoint3x4(vertices[edge.B]);
                Vector3 bottomA = new Vector3(topA.x, targetBottom, topA.z);
                Vector3 bottomB = new Vector3(topB.x, targetBottom, topB.z);

                AddTwoSidedQuad(
                    skirtVertices,
                    skirtUvs,
                    skirtTriangles,
                    inverseLinear.MultiplyPoint3x4(topA),
                    inverseLinear.MultiplyPoint3x4(topB),
                    inverseLinear.MultiplyPoint3x4(bottomA),
                    inverseLinear.MultiplyPoint3x4(bottomB),
                    Mathf.Max(0.0001f, Vector3.Distance(topA, topB)),
                    Mathf.Max(0.0001f, Mathf.Abs(topA.y - targetBottom)));
            }

            if (skirtVertices.Count == 0)
                return null;

            var skirt = new Mesh
            {
                name = source.name + "_VerticalSkirt",
                indexFormat = skirtVertices.Count > 65535
                    ? IndexFormat.UInt32
                    : IndexFormat.UInt16
            };
            skirt.SetVertices(skirtVertices);
            skirt.SetUVs(0, skirtUvs);
            skirt.SetTriangles(skirtTriangles, 0, false);
            skirt.RecalculateNormals();
            skirt.RecalculateBounds();

            var combines = new CombineInstance[source.subMeshCount + 1];
            for (int subMesh = 0; subMesh < source.subMeshCount; subMesh++)
            {
                combines[subMesh] = new CombineInstance
                {
                    mesh = source,
                    subMeshIndex = subMesh,
                    transform = Matrix4x4.identity
                };
            }

            combines[combines.Length - 1] = new CombineInstance
            {
                mesh = skirt,
                subMeshIndex = 0,
                transform = Matrix4x4.identity
            };

            var combined = new Mesh
            {
                name = source.name + "_VerticalFill",
                indexFormat = source.vertexCount + skirt.vertexCount > 65535
                    ? IndexFormat.UInt32
                    : IndexFormat.UInt16
            };
            combined.CombineMeshes(combines, false, true);
            combined.RecalculateBounds();

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(skirt);
            else
                UnityEngine.Object.DestroyImmediate(skirt);

            return combined;
        }

        private static void AddTwoSidedQuad(
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<int> triangles,
            Vector3 topA,
            Vector3 topB,
            Vector3 bottomA,
            Vector3 bottomB,
            float width,
            float height)
        {
            int front = vertices.Count;
            vertices.Add(topA);
            vertices.Add(topB);
            vertices.Add(bottomA);
            vertices.Add(bottomB);
            AddQuadUvs(uvs, width, height);
            triangles.Add(front);
            triangles.Add(front + 2);
            triangles.Add(front + 1);
            triangles.Add(front + 1);
            triangles.Add(front + 2);
            triangles.Add(front + 3);

            // Separate vertices for the back face prevent opposite normals from
            // cancelling each other during RecalculateNormals.
            int back = vertices.Count;
            vertices.Add(topA);
            vertices.Add(topB);
            vertices.Add(bottomA);
            vertices.Add(bottomB);
            AddQuadUvs(uvs, width, height);
            triangles.Add(back + 1);
            triangles.Add(back + 2);
            triangles.Add(back);
            triangles.Add(back + 3);
            triangles.Add(back + 2);
            triangles.Add(back + 1);
        }

        private static void AddQuadUvs(
            List<Vector2> uvs,
            float width,
            float height)
        {
            uvs.Add(new Vector2(0f, height));
            uvs.Add(new Vector2(width, height));
            uvs.Add(new Vector2(0f, 0f));
            uvs.Add(new Vector2(width, 0f));
        }

        private static Dictionary<GeometricEdgeKey, BoundaryEdge> CollectBoundaryEdges(
            Mesh source,
            IReadOnlyList<Vector3> vertices,
            Matrix4x4 linearMatrix)
        {
            var edges = new Dictionary<GeometricEdgeKey, BoundaryEdge>();
            for (int subMesh = 0; subMesh < source.subMeshCount; subMesh++)
            {
                if (source.GetTopology(subMesh) != MeshTopology.Triangles)
                    continue;

                int[] indices = source.GetIndices(subMesh);
                for (int i = 0; i + 2 < indices.Length; i += 3)
                {
                    AddEdge(edges, vertices, linearMatrix, indices[i], indices[i + 1]);
                    AddEdge(edges, vertices, linearMatrix, indices[i + 1], indices[i + 2]);
                    AddEdge(edges, vertices, linearMatrix, indices[i + 2], indices[i]);
                }
            }

            return edges;
        }

        private static void AddEdge(
            Dictionary<GeometricEdgeKey, BoundaryEdge> edges,
            IReadOnlyList<Vector3> vertices,
            Matrix4x4 linearMatrix,
            int a,
            int b)
        {
            Vector3 positionA = linearMatrix.MultiplyPoint3x4(vertices[a]);
            Vector3 positionB = linearMatrix.MultiplyPoint3x4(vertices[b]);
            var key = new GeometricEdgeKey(positionA, positionB);

            if (edges.TryGetValue(key, out BoundaryEdge existing))
            {
                existing.Count++;
                edges[key] = existing;
                return;
            }

            edges[key] = new BoundaryEdge(a, b, 1);
        }

        private static int[] RemoveFullyHiddenTriangles(
            IReadOnlyList<int> indices,
            IReadOnlyList<float> originalRelativeY,
            float targetBottom)
        {
            var visible = new List<int>(indices.Count);
            for (int i = 0; i + 2 < indices.Count; i += 3)
            {
                int a = indices[i];
                int b = indices[i + 1];
                int c = indices[i + 2];

                bool hidden = originalRelativeY[a] < targetBottom - HeightEpsilon
                    && originalRelativeY[b] < targetBottom - HeightEpsilon
                    && originalRelativeY[c] < targetBottom - HeightEpsilon;
                if (hidden)
                    continue;

                visible.Add(a);
                visible.Add(b);
                visible.Add(c);
            }

            return visible.ToArray();
        }

        private static void CopyVertexChannels(Mesh source, Mesh destination)
        {
            Vector3[] normals = source.normals;
            if (normals != null && normals.Length == source.vertexCount)
                destination.normals = normals;

            Vector4[] tangents = source.tangents;
            if (tangents != null && tangents.Length == source.vertexCount)
                destination.tangents = tangents;

            Color32[] colors = source.colors32;
            if (colors != null && colors.Length == source.vertexCount)
                destination.colors32 = colors;

            BoneWeight[] boneWeights = source.boneWeights;
            if (boneWeights != null && boneWeights.Length == source.vertexCount)
                destination.boneWeights = boneWeights;

            Matrix4x4[] bindposes = source.bindposes;
            if (bindposes != null && bindposes.Length > 0)
                destination.bindposes = bindposes;

            for (int channel = 0; channel < 8; channel++)
            {
                var uvs = new List<Vector4>(source.vertexCount);
                source.GetUVs(channel, uvs);
                if (uvs.Count == source.vertexCount)
                    destination.SetUVs(channel, uvs);
            }
        }

        private readonly struct QuantizedPoint : IEquatable<QuantizedPoint>,
            IComparable<QuantizedPoint>
        {
            private readonly int _x;
            private readonly int _y;
            private readonly int _z;

            public QuantizedPoint(Vector3 value)
            {
                _x = Mathf.RoundToInt(value.x * EdgeWeldPrecision);
                _y = Mathf.RoundToInt(value.y * EdgeWeldPrecision);
                _z = Mathf.RoundToInt(value.z * EdgeWeldPrecision);
            }

            public int CompareTo(QuantizedPoint other)
            {
                int result = _x.CompareTo(other._x);
                if (result != 0)
                    return result;

                result = _y.CompareTo(other._y);
                return result != 0 ? result : _z.CompareTo(other._z);
            }

            public bool Equals(QuantizedPoint other)
                => _x == other._x && _y == other._y && _z == other._z;

            public override bool Equals(object obj)
                => obj is QuantizedPoint other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = _x;
                    hash = hash * 397 ^ _y;
                    hash = hash * 397 ^ _z;
                    return hash;
                }
            }
        }

        private readonly struct GeometricEdgeKey : IEquatable<GeometricEdgeKey>
        {
            private readonly QuantizedPoint _min;
            private readonly QuantizedPoint _max;

            public GeometricEdgeKey(Vector3 a, Vector3 b)
            {
                var pointA = new QuantizedPoint(a);
                var pointB = new QuantizedPoint(b);
                if (pointA.CompareTo(pointB) <= 0)
                {
                    _min = pointA;
                    _max = pointB;
                }
                else
                {
                    _min = pointB;
                    _max = pointA;
                }
            }

            public bool Equals(GeometricEdgeKey other)
                => _min.Equals(other._min) && _max.Equals(other._max);

            public override bool Equals(object obj)
                => obj is GeometricEdgeKey other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    return (_min.GetHashCode() * 397) ^ _max.GetHashCode();
                }
            }
        }

        private struct BoundaryEdge
        {
            public BoundaryEdge(int a, int b, int count)
            {
                A = a;
                B = b;
                Count = count;
            }

            public int A;
            public int B;
            public int Count;
        }
    }
}
