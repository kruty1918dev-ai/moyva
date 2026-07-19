using System;
using System.Collections.Generic;
using Unity.Collections;
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

        public static bool TryCreate(TileMeshSource source, out Mesh result)
        {
            result = null;
            if (!source.IsValid || !source.HasVisibleBottomY)
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

            using (Mesh.MeshDataArray meshDataArray = Mesh.AcquireReadOnlyMeshData(source.Mesh))
            {
                Mesh.MeshData meshData = meshDataArray[0];
                using (var vertices = new NativeArray<Vector3>(
                           meshData.vertexCount,
                           Allocator.Temp,
                           NativeArrayOptions.UninitializedMemory))
                {
                    meshData.GetVertices(vertices);
                    if (vertices.Length == 0)
                        return false;

                    // NativeArray declared by a using statement is readonly in C#.
                    // Copy vertex data to a mutable managed array before deformation.
                    Vector3[] mutableVertices = vertices.ToArray();
                    var originalRelativeY = new float[mutableVertices.Length];
                    float minY = float.PositiveInfinity;
                    float maxY = float.NegativeInfinity;
                    for (int i = 0; i < mutableVertices.Length; i++)
                    {
                        float y = linearMatrix.MultiplyPoint3x4(mutableVertices[i]).y;
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
                            meshData,
                            mutableVertices,
                            linearMatrix,
                            inverseLinear,
                            targetBottom);
                        return result != null;
                    }

                    targetBottom = Mathf.Min(targetBottom, maxY - HeightEpsilon);
                    bool extendDown = targetBottom < minY - HeightEpsilon;
                    bool clipHidden = targetBottom > minY + HeightEpsilon;
                    if (!extendDown && !clipHidden)
                        return false;

                    float bottomBand = minY + Mathf.Max(
                        HeightEpsilon * 10f,
                        meshHeight * 0.02f);

                    for (int i = 0; i < vertices.Length; i++)
                    {
                        Vector3 relative = linearMatrix.MultiplyPoint3x4(vertices[i]);
                        if (extendDown)
                        {
                            if (relative.y <= bottomBand)
                                relative.y = targetBottom;
                        }
                        else if (relative.y < targetBottom)
                        {
                            relative.y = targetBottom;
                        }

                        mutableVertices[i] = inverseLinear.MultiplyPoint3x4(relative);
                    }

                    result = CopyMesh(
                        source.Mesh,
                        meshData,
                        mutableVertices,
                        originalRelativeY,
                        clipHidden,
                        targetBottom);
                    return result != null;
                }
            }
        }

        private static Mesh CopyMesh(
            Mesh source,
            Mesh.MeshData meshData,
            Vector3[] vertices,
            IReadOnlyList<float> originalRelativeY,
            bool clipHidden,
            float targetBottom)
        {
            var mesh = new Mesh
            {
                name = source.name + "_VerticalFill",
                indexFormat = source.indexFormat
            };

            mesh.vertices = vertices;
            CopyVertexChannels(source, meshData, mesh);

            mesh.subMeshCount = meshData.subMeshCount;
            for (int subMesh = 0; subMesh < meshData.subMeshCount; subMesh++)
            {
                SubMeshDescriptor descriptor = meshData.GetSubMesh(subMesh);
                int[] indices = ReadIndices(source, meshData, subMesh, descriptor.indexCount);
                if (clipHidden && descriptor.topology == MeshTopology.Triangles)
                    indices = RemoveFullyHiddenTriangles(indices, originalRelativeY, targetBottom);

                mesh.SetIndices(indices, descriptor.topology, subMesh, false);
            }

            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh CreateFlatMeshWithSkirt(
            Mesh source,
            Mesh.MeshData meshData,
            Vector3[] vertices,
            Matrix4x4 linearMatrix,
            Matrix4x4 inverseLinear,
            float targetBottom)
        {
            Dictionary<EdgeKey, BoundaryEdge> edges = CollectBoundaryEdges(
                source,
                meshData);

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

                int start = skirtVertices.Count;
                skirtVertices.Add(inverseLinear.MultiplyPoint3x4(topA));
                skirtVertices.Add(inverseLinear.MultiplyPoint3x4(topB));
                skirtVertices.Add(inverseLinear.MultiplyPoint3x4(bottomA));
                skirtVertices.Add(inverseLinear.MultiplyPoint3x4(bottomB));

                float height = Mathf.Max(0.0001f, Mathf.Abs(topA.y - targetBottom));
                float width = Mathf.Max(0.0001f, Vector3.Distance(topA, topB));
                skirtUvs.Add(new Vector2(0f, height));
                skirtUvs.Add(new Vector2(width, height));
                skirtUvs.Add(new Vector2(0f, 0f));
                skirtUvs.Add(new Vector2(width, 0f));

                skirtTriangles.Add(start);
                skirtTriangles.Add(start + 2);
                skirtTriangles.Add(start + 1);
                skirtTriangles.Add(start + 1);
                skirtTriangles.Add(start + 2);
                skirtTriangles.Add(start + 3);

                skirtTriangles.Add(start + 1);
                skirtTriangles.Add(start + 2);
                skirtTriangles.Add(start);
                skirtTriangles.Add(start + 3);
                skirtTriangles.Add(start + 2);
                skirtTriangles.Add(start + 1);
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

        private static Dictionary<EdgeKey, BoundaryEdge> CollectBoundaryEdges(
            Mesh source,
            Mesh.MeshData meshData)
        {
            var edges = new Dictionary<EdgeKey, BoundaryEdge>();
            for (int subMesh = 0; subMesh < meshData.subMeshCount; subMesh++)
            {
                SubMeshDescriptor descriptor = meshData.GetSubMesh(subMesh);
                if (descriptor.topology != MeshTopology.Triangles)
                    continue;

                int[] indices = ReadIndices(source, meshData, subMesh, descriptor.indexCount);
                for (int i = 0; i + 2 < indices.Length; i += 3)
                {
                    AddEdge(edges, indices[i], indices[i + 1]);
                    AddEdge(edges, indices[i + 1], indices[i + 2]);
                    AddEdge(edges, indices[i + 2], indices[i]);
                }
            }

            return edges;
        }

        private static void AddEdge(
            Dictionary<EdgeKey, BoundaryEdge> edges,
            int a,
            int b)
        {
            var key = new EdgeKey(a, b);
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

        private static int[] ReadIndices(
            Mesh source,
            Mesh.MeshData meshData,
            int subMesh,
            int indexCount)
        {
            if (source.indexFormat == IndexFormat.UInt16)
            {
                using (var indices = new NativeArray<ushort>(
                           indexCount,
                           Allocator.Temp,
                           NativeArrayOptions.UninitializedMemory))
                {
                    meshData.GetIndices(indices, subMesh, true);
                    var result = new int[indexCount];
                    for (int i = 0; i < indexCount; i++)
                        result[i] = indices[i];
                    return result;
                }
            }

            using (var indices = new NativeArray<int>(
                       indexCount,
                       Allocator.Temp,
                       NativeArrayOptions.UninitializedMemory))
            {
                meshData.GetIndices(indices, subMesh, true);
                return indices.ToArray();
            }
        }

        private static void CopyVertexChannels(
            Mesh source,
            Mesh.MeshData meshData,
            Mesh destination)
        {
            if (source.HasVertexAttribute(VertexAttribute.Normal))
            {
                using (var normals = new NativeArray<Vector3>(
                           meshData.vertexCount,
                           Allocator.Temp,
                           NativeArrayOptions.UninitializedMemory))
                {
                    meshData.GetNormals(normals);
                    destination.normals = normals.ToArray();
                }
            }

            if (source.HasVertexAttribute(VertexAttribute.Tangent))
            {
                using (var tangents = new NativeArray<Vector4>(
                           meshData.vertexCount,
                           Allocator.Temp,
                           NativeArrayOptions.UninitializedMemory))
                {
                    meshData.GetTangents(tangents);
                    destination.tangents = tangents.ToArray();
                }
            }

            if (source.HasVertexAttribute(VertexAttribute.Color))
            {
                using (var colors = new NativeArray<Color32>(
                           meshData.vertexCount,
                           Allocator.Temp,
                           NativeArrayOptions.UninitializedMemory))
                {
                    meshData.GetColors(colors);
                    destination.colors32 = colors.ToArray();
                }
            }

            for (int channel = 0; channel < 8; channel++)
            {
                VertexAttribute attribute =
                    (VertexAttribute)((int)VertexAttribute.TexCoord0 + channel);
                if (!source.HasVertexAttribute(attribute))
                    continue;

                using (var uvs = new NativeArray<Vector4>(
                           meshData.vertexCount,
                           Allocator.Temp,
                           NativeArrayOptions.UninitializedMemory))
                {
                    meshData.GetUVs(channel, uvs);
                    destination.SetUVs(channel, new List<Vector4>(uvs.ToArray()));
                }
            }
        }

        private readonly struct EdgeKey : IEquatable<EdgeKey>
        {
            private readonly int _min;
            private readonly int _max;

            public EdgeKey(int a, int b)
            {
                _min = Mathf.Min(a, b);
                _max = Mathf.Max(a, b);
            }

            public bool Equals(EdgeKey other)
                => _min == other._min && _max == other._max;

            public override bool Equals(object obj)
                => obj is EdgeKey other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    return (_min * 397) ^ _max;
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
