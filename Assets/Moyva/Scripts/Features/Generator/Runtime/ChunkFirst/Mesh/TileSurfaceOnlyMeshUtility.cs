using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal enum SurfaceOnlyMeshBuildStatus
    {
        Failed = 0,
        Empty = 1,
        Created = 2
    }

    internal readonly struct TileSurfaceOnlyMeshKey : IEquatable<TileSurfaceOnlyMeshKey>
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

        private TileSurfaceOnlyMeshKey(TileMeshSource source)
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
        }

        public static TileSurfaceOnlyMeshKey Create(TileMeshSource source)
            => new TileSurfaceOnlyMeshKey(source);

        public bool Equals(TileSurfaceOnlyMeshKey other)
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
                && _m22 == other._m22;
        }

        public override bool Equals(object obj)
            => obj is TileSurfaceOnlyMeshKey other && Equals(other);

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
                return hash;
            }
        }

        private static int Quantize(float value)
            => Mathf.RoundToInt(value * Quantization);
    }

    /// <summary>
    /// Produces a referenced-only copy containing upward-facing authored
    /// surface triangles. Submesh order and the raw vertex-stream payload are
    /// retained so material slots, hard normals, tangents, colors and UV seams
    /// remain unchanged.
    /// </summary>
    internal static class TileSurfaceOnlyMeshUtility
    {
        private const float DeterminantEpsilon = 0.000001f;
        private const float UpwardNormalEpsilon = 0.0001f;

        public static SurfaceOnlyMeshBuildStatus Create(
            TileMeshSource source,
            out Mesh result)
        {
            result = null;
            Mesh sourceMesh = source.Mesh;
            if (!source.IsValid
                || sourceMesh.subMeshCount <= 0
                || !sourceMesh.isReadable)
            {
                return SurfaceOnlyMeshBuildStatus.Failed;
            }

            Matrix4x4 linearMatrix = source.LocalMatrix;
            linearMatrix.m03 = 0f;
            linearMatrix.m13 = 0f;
            linearMatrix.m23 = 0f;
            if (Mathf.Abs(linearMatrix.determinant) <= DeterminantEpsilon)
                return SurfaceOnlyMeshBuildStatus.Failed;

            try
            {
                Vector3[] vertices = sourceMesh.vertices;
                Matrix4x4 normalMatrix = linearMatrix.inverse.transpose;
                var indicesBySubMesh = new int[sourceMesh.subMeshCount][];
                var topologyBySubMesh =
                    new MeshTopology[sourceMesh.subMeshCount];
                var referenced = new bool[sourceMesh.vertexCount];
                int referencedCount = 0;

                for (int subMesh = 0;
                     subMesh < sourceMesh.subMeshCount;
                     subMesh++)
                {
                    MeshTopology topology = sourceMesh.GetTopology(subMesh);
                    topologyBySubMesh[subMesh] = topology;
                    int[] indices = sourceMesh.GetIndices(
                        subMesh,
                        applyBaseVertex: true);
                    if (topology == MeshTopology.Triangles)
                    {
                        indices = KeepUpwardTriangles(
                            indices,
                            vertices,
                            normalMatrix);
                    }

                    indicesBySubMesh[subMesh] = indices;
                    for (int index = 0; index < indices.Length; index++)
                    {
                        int vertex = indices[index];
                        if (vertex < 0
                            || vertex >= referenced.Length
                            || referenced[vertex])
                        {
                            continue;
                        }

                        referenced[vertex] = true;
                        referencedCount++;
                    }
                }

                if (referencedCount == 0)
                    return SurfaceOnlyMeshBuildStatus.Empty;

                result = CreateCompactedMesh(
                    sourceMesh,
                    indicesBySubMesh,
                    topologyBySubMesh,
                    referenced,
                    referencedCount);
                return result != null
                    ? SurfaceOnlyMeshBuildStatus.Created
                    : SurfaceOnlyMeshBuildStatus.Failed;
            }
            catch (Exception)
            {
                DestroyMesh(result);
                result = null;
                return SurfaceOnlyMeshBuildStatus.Failed;
            }
        }

        private static int[] KeepUpwardTriangles(
            IReadOnlyList<int> indices,
            IReadOnlyList<Vector3> vertices,
            Matrix4x4 normalMatrix)
        {
            var surface = new List<int>(indices.Count);
            for (int index = 0; index + 2 < indices.Count; index += 3)
            {
                int a = indices[index];
                int b = indices[index + 1];
                int c = indices[index + 2];
                if (a < 0
                    || b < 0
                    || c < 0
                    || a >= vertices.Count
                    || b >= vertices.Count
                    || c >= vertices.Count)
                {
                    continue;
                }

                Vector3 authoredNormal = Vector3.Cross(
                    vertices[b] - vertices[a],
                    vertices[c] - vertices[a]);
                if (authoredNormal.sqrMagnitude <= DeterminantEpsilon)
                    continue;

                Vector3 transformedNormal =
                    normalMatrix.MultiplyVector(authoredNormal).normalized;
                if (transformedNormal.y <= UpwardNormalEpsilon)
                    continue;

                surface.Add(a);
                surface.Add(b);
                surface.Add(c);
            }

            return surface.ToArray();
        }

        private static Mesh CreateCompactedMesh(
            Mesh source,
            IReadOnlyList<int[]> indicesBySubMesh,
            IReadOnlyList<MeshTopology> topologyBySubMesh,
            IReadOnlyList<bool> referenced,
            int referencedCount)
        {
            var remap = new int[source.vertexCount];
            var representatives = new int[referencedCount];
            int next = 0;
            for (int vertex = 0; vertex < source.vertexCount; vertex++)
            {
                remap[vertex] = -1;
                if (!referenced[vertex])
                    continue;

                remap[vertex] = next;
                representatives[next] = vertex;
                next++;
            }

            Mesh compacted = null;
            using Mesh.MeshDataArray readOnlyMeshData =
                Mesh.AcquireReadOnlyMeshData(source);
            try
            {
                compacted = new Mesh
                {
                    name = source.name + "_SurfaceOnly",
                    indexFormat = referencedCount > ushort.MaxValue
                        ? IndexFormat.UInt32
                        : IndexFormat.UInt16
                };
                compacted.SetVertexBufferParams(
                    referencedCount,
                    source.GetVertexAttributes());

                Mesh.MeshData sourceData = readOnlyMeshData[0];
                MeshUpdateFlags updateFlags =
                    MeshUpdateFlags.DontRecalculateBounds
                    | MeshUpdateFlags.DontValidateIndices
                    | MeshUpdateFlags.DontNotifyMeshUsers;
                for (int stream = 0;
                     stream < source.vertexBufferCount;
                     stream++)
                {
                    int stride = source.GetVertexBufferStride(stream);
                    if (stride <= 0)
                        throw new InvalidOperationException(
                            "Surface-only mesh has an invalid vertex stride.");

                    NativeArray<byte> sourceBytes =
                        sourceData.GetVertexData<byte>(stream);
                    var destinationBytes = new NativeArray<byte>(
                        referencedCount * stride,
                        Allocator.Temp,
                        NativeArrayOptions.UninitializedMemory);
                    try
                    {
                        for (int compactedIndex = 0;
                             compactedIndex < referencedCount;
                             compactedIndex++)
                        {
                            int sourceOffset =
                                representatives[compactedIndex] * stride;
                            int destinationOffset = compactedIndex * stride;
                            for (int offset = 0; offset < stride; offset++)
                            {
                                destinationBytes[destinationOffset + offset] =
                                    sourceBytes[sourceOffset + offset];
                            }
                        }

                        compacted.SetVertexBufferData(
                            destinationBytes,
                            0,
                            0,
                            destinationBytes.Length,
                            stream,
                            updateFlags);
                    }
                    finally
                    {
                        destinationBytes.Dispose();
                    }
                }

                compacted.subMeshCount = source.subMeshCount;
                for (int subMesh = 0;
                     subMesh < source.subMeshCount;
                     subMesh++)
                {
                    int[] remapped =
                        (int[])indicesBySubMesh[subMesh].Clone();
                    for (int index = 0; index < remapped.Length; index++)
                        remapped[index] = remap[remapped[index]];

                    compacted.SetIndices(
                        remapped,
                        topologyBySubMesh[subMesh],
                        subMesh,
                        calculateBounds: false);
                }

                compacted.bindposes = source.bindposes;
                compacted.RecalculateBounds();
                Mesh result = compacted;
                compacted = null;
                return result;
            }
            finally
            {
                DestroyMesh(compacted);
            }
        }

        private static void DestroyMesh(Mesh mesh)
        {
            if (mesh == null)
                return;

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(mesh);
            else
                UnityEngine.Object.DestroyImmediate(mesh);
        }
    }
}
