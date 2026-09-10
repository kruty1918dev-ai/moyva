using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.MapChunks.API;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal static class ExactVertexWeldMeshUtility
    {
        public static bool TryCreate(Mesh source, out Mesh result)
        {
            result = null;
            if (source == null
                || !source.isReadable
                || source.vertexCount <= 0
                || source.subMeshCount <= 0)
            {
                return false;
            }

            int streamCount = source.vertexBufferCount;
            if (streamCount <= 0)
                return false;

            var sourceStreams = new NativeArray<byte>[streamCount];
            var strides = new int[streamCount];
            Mesh welded = null;
            using Mesh.MeshDataArray readOnlyMeshData =
                Mesh.AcquireReadOnlyMeshData(source);
            Mesh.MeshData sourceMeshData = readOnlyMeshData[0];
            try
            {
                if (!TryCollectReferencedVertices(
                        source,
                        out SourceSubMesh[] sourceSubMeshes,
                        out bool[] referenced,
                        out int[] minimumOutputIndices))
                {
                    return false;
                }

                for (int stream = 0; stream < streamCount; stream++)
                {
                    int stride = source.GetVertexBufferStride(stream);
                    if (stride <= 0)
                        return false;

                    strides[stream] = stride;
                    sourceStreams[stream] =
                        sourceMeshData.GetVertexData<byte>(stream);
                }

                var vertexToGroup = new int[source.vertexCount];
                for (int vertex = 0;
                     vertex < vertexToGroup.Length;
                     vertex++)
                {
                    vertexToGroup[vertex] = -1;
                }

                var representatives = new List<int>(source.vertexCount);
                var groupMinimumOutputIndices =
                    new List<int>(source.vertexCount);
                var candidatesByHash =
                    new Dictionary<ulong, List<int>>(source.vertexCount);

                for (int vertex = 0; vertex < source.vertexCount; vertex++)
                {
                    if (!referenced[vertex])
                        continue;

                    ulong hash = HashVertex(
                        sourceStreams,
                        strides,
                        vertex);
                    if (candidatesByHash.TryGetValue(
                            hash,
                            out List<int> candidates))
                    {
                        int matchedIndex = FindMatchingRepresentative(
                            sourceStreams,
                            strides,
                            vertex,
                            candidates,
                            representatives);
                        if (matchedIndex >= 0)
                        {
                            vertexToGroup[vertex] = matchedIndex;
                            groupMinimumOutputIndices[matchedIndex] =
                                Mathf.Max(
                                    groupMinimumOutputIndices[matchedIndex],
                                    minimumOutputIndices[vertex]);
                            continue;
                        }
                    }
                    else
                    {
                        candidates = new List<int>(1);
                        candidatesByHash.Add(hash, candidates);
                    }

                    int weldedIndex = representatives.Count;
                    representatives.Add(vertex);
                    groupMinimumOutputIndices.Add(
                        minimumOutputIndices[vertex]);
                    candidates.Add(weldedIndex);
                    vertexToGroup[vertex] = weldedIndex;
                }

                if (representatives.Count == source.vertexCount)
                    return false;

                if (!TryCreateOutputLayout(
                        source,
                        sourceSubMeshes,
                        vertexToGroup,
                        representatives,
                        groupMinimumOutputIndices,
                        out int[] groupToOutput,
                        out int[] outputBaseVertices))
                {
                    return false;
                }

                var outputToRepresentative =
                    new int[representatives.Count];
                for (int group = 0;
                     group < representatives.Count;
                     group++)
                {
                    outputToRepresentative[groupToOutput[group]] =
                        representatives[group];
                }

                welded = new Mesh
                {
                    name = source.name + "_ExactWeld",
                    indexFormat = source.indexFormat,
                };
                welded.SetVertexBufferParams(
                    representatives.Count,
                    source.GetVertexAttributes());

                MeshUpdateFlags updateFlags =
                    MeshUpdateFlags.DontRecalculateBounds
                    | MeshUpdateFlags.DontValidateIndices
                    | MeshUpdateFlags.DontNotifyMeshUsers;
                for (int stream = 0; stream < streamCount; stream++)
                {
                    int stride = strides[stream];
                    var destination = new NativeArray<byte>(
                        representatives.Count * stride,
                        Allocator.Temp,
                        NativeArrayOptions.UninitializedMemory);
                    try
                    {
                        for (int weldedIndex = 0;
                             weldedIndex < representatives.Count;
                             weldedIndex++)
                        {
                            int sourceOffset =
                                outputToRepresentative[weldedIndex]
                                * stride;
                            int destinationOffset = weldedIndex * stride;
                            for (int offset = 0; offset < stride; offset++)
                            {
                                destination[destinationOffset + offset] =
                                    sourceStreams[stream][sourceOffset + offset];
                            }
                        }

                        welded.SetVertexBufferData(
                            destination,
                            0,
                            0,
                            destination.Length,
                            stream,
                            updateFlags);
                    }
                    finally
                    {
                        destination.Dispose();
                    }
                }

                welded.subMeshCount = source.subMeshCount;
                for (int subMesh = 0;
                     subMesh < source.subMeshCount;
                     subMesh++)
                {
                    SourceSubMesh sourceSubMesh =
                        sourceSubMeshes[subMesh];
                    int outputBaseVertex =
                        outputBaseVertices[subMesh];
                    int[] indices =
                        new int[sourceSubMesh.AbsoluteIndices.Length];
                    for (int index = 0;
                         index < indices.Length;
                         index++)
                    {
                        int sourceVertex =
                            sourceSubMesh.AbsoluteIndices[index];
                        int outputVertex =
                            groupToOutput[
                                vertexToGroup[sourceVertex]];
                        indices[index] =
                            outputVertex - outputBaseVertex;
                    }

                    welded.SetIndices(
                        indices,
                        sourceSubMesh.Topology,
                        subMesh,
                        calculateBounds: false,
                        baseVertex: outputBaseVertex);

                    SubMeshDescriptor descriptor =
                        welded.GetSubMesh(subMesh);
                    descriptor.bounds = sourceSubMesh.Bounds;
                    welded.SetSubMesh(
                        subMesh,
                        descriptor,
                        updateFlags);
                }

                welded.bindposes = source.bindposes;
                welded.bounds = source.bounds;
                result = welded;
                welded = null;
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
            finally
            {
                if (welded != null)
                {
                    if (Application.isPlaying)
                        UnityEngine.Object.Destroy(welded);
                    else
                        UnityEngine.Object.DestroyImmediate(welded);
                }
            }
        }

        private static bool TryCollectReferencedVertices(
            Mesh source,
            out SourceSubMesh[] subMeshes,
            out bool[] referenced,
            out int[] minimumOutputIndices)
        {
            subMeshes = new SourceSubMesh[source.subMeshCount];
            referenced = new bool[source.vertexCount];
            minimumOutputIndices = new int[source.vertexCount];
            for (int subMesh = 0;
                 subMesh < source.subMeshCount;
                 subMesh++)
            {
                int baseVertex = checked((int)source.GetBaseVertex(subMesh));
                int[] absoluteIndices = source.GetIndices(
                    subMesh,
                    applyBaseVertex: true);
                for (int index = 0;
                     index < absoluteIndices.Length;
                     index++)
                {
                    int vertex = absoluteIndices[index];
                    if ((uint)vertex >= (uint)source.vertexCount)
                        return false;

                    referenced[vertex] = true;
                    minimumOutputIndices[vertex] = Mathf.Max(
                        minimumOutputIndices[vertex],
                        baseVertex);
                }

                subMeshes[subMesh] = new SourceSubMesh(
                    absoluteIndices,
                    source.GetTopology(subMesh),
                    baseVertex,
                    source.GetSubMesh(subMesh).bounds);
            }

            return true;
        }

        private static bool TryCreateOutputLayout(
            Mesh source,
            IReadOnlyList<SourceSubMesh> subMeshes,
            IReadOnlyList<int> vertexToGroup,
            IReadOnlyList<int> representatives,
            IReadOnlyList<int> groupMinimumOutputIndices,
            out int[] groupToOutput,
            out int[] outputBaseVertices)
        {
            int groupCount = representatives.Count;
            var orderedGroups = new List<int>(groupCount);
            for (int group = 0; group < groupCount; group++)
                orderedGroups.Add(group);
            orderedGroups.Sort((first, second) =>
            {
                int comparison = groupMinimumOutputIndices[first]
                    .CompareTo(groupMinimumOutputIndices[second]);
                return comparison != 0
                    ? comparison
                    : representatives[first].CompareTo(
                        representatives[second]);
            });

            groupToOutput = new int[groupCount];
            bool canPreserveBaseVertices = true;
            for (int output = 0;
                 output < orderedGroups.Count;
                 output++)
            {
                int group = orderedGroups[output];
                if (groupMinimumOutputIndices[group] > output)
                {
                    canPreserveBaseVertices = false;
                    break;
                }

                groupToOutput[group] = output;
            }

            outputBaseVertices = new int[subMeshes.Count];
            if (canPreserveBaseVertices)
            {
                for (int subMesh = 0;
                     subMesh < subMeshes.Count;
                     subMesh++)
                {
                    outputBaseVertices[subMesh] =
                        subMeshes[subMesh].BaseVertex;
                }

                canPreserveBaseVertices = IndicesFitFormat(
                    source.indexFormat,
                    subMeshes,
                    vertexToGroup,
                    groupToOutput,
                    outputBaseVertices);
            }

            if (canPreserveBaseVertices)
                return true;

            // Some authored meshes contain unused padding before a non-zero
            // baseVertex. Removing that padding makes the original numeric
            // base impossible to retain. Keep vertices in stable source order
            // and choose the narrowest valid base for only those submeshes.
            for (int group = 0; group < groupCount; group++)
                groupToOutput[group] = group;

            for (int subMesh = 0;
                 subMesh < subMeshes.Count;
                 subMesh++)
            {
                SourceSubMesh sourceSubMesh = subMeshes[subMesh];
                int sourceBaseVertex = sourceSubMesh.BaseVertex;
                if (SubMeshIndicesFit(
                        source.indexFormat,
                        sourceSubMesh,
                        vertexToGroup,
                        groupToOutput,
                        sourceBaseVertex))
                {
                    outputBaseVertices[subMesh] = sourceBaseVertex;
                    continue;
                }

                outputBaseVertices[subMesh] = FindMinimumOutputVertex(
                    sourceSubMesh,
                    vertexToGroup,
                    groupToOutput);
            }

            return IndicesFitFormat(
                source.indexFormat,
                subMeshes,
                vertexToGroup,
                groupToOutput,
                outputBaseVertices);
        }

        private static bool IndicesFitFormat(
            IndexFormat indexFormat,
            IReadOnlyList<SourceSubMesh> subMeshes,
            IReadOnlyList<int> vertexToGroup,
            IReadOnlyList<int> groupToOutput,
            IReadOnlyList<int> baseVertices)
        {
            for (int subMesh = 0;
                 subMesh < subMeshes.Count;
                 subMesh++)
            {
                if (!SubMeshIndicesFit(
                        indexFormat,
                        subMeshes[subMesh],
                        vertexToGroup,
                        groupToOutput,
                        baseVertices[subMesh]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool SubMeshIndicesFit(
            IndexFormat indexFormat,
            SourceSubMesh subMesh,
            IReadOnlyList<int> vertexToGroup,
            IReadOnlyList<int> groupToOutput,
            int baseVertex)
        {
            if (subMesh.AbsoluteIndices.Length == 0)
            {
                return baseVertex == 0
                    || (baseVertex > 0
                        && baseVertex < groupToOutput.Count);
            }

            int maximumIndex = indexFormat == IndexFormat.UInt16
                ? ushort.MaxValue
                : int.MaxValue;
            for (int index = 0;
                 index < subMesh.AbsoluteIndices.Length;
                 index++)
            {
                int sourceVertex = subMesh.AbsoluteIndices[index];
                int group = vertexToGroup[sourceVertex];
                int localIndex =
                    groupToOutput[group] - baseVertex;
                if (localIndex < 0 || localIndex > maximumIndex)
                    return false;
            }

            return true;
        }

        private static int FindMinimumOutputVertex(
            SourceSubMesh subMesh,
            IReadOnlyList<int> vertexToGroup,
            IReadOnlyList<int> groupToOutput)
        {
            if (subMesh.AbsoluteIndices.Length == 0)
            {
                return subMesh.BaseVertex >= 0
                       && subMesh.BaseVertex < groupToOutput.Count
                    ? subMesh.BaseVertex
                    : 0;
            }

            int minimum = int.MaxValue;
            for (int index = 0;
                 index < subMesh.AbsoluteIndices.Length;
                 index++)
            {
                int sourceVertex = subMesh.AbsoluteIndices[index];
                minimum = Mathf.Min(
                    minimum,
                    groupToOutput[vertexToGroup[sourceVertex]]);
            }

            return minimum;
        }

        private static int FindMatchingRepresentative(
            IReadOnlyList<NativeArray<byte>> streams,
            IReadOnlyList<int> strides,
            int vertex,
            IReadOnlyList<int> candidates,
            IReadOnlyList<int> representatives)
        {
            for (int candidateIndex = 0;
                 candidateIndex < candidates.Count;
                 candidateIndex++)
            {
                int weldedIndex = candidates[candidateIndex];
                int representative = representatives[weldedIndex];
                if (VertexEquals(
                        streams,
                        strides,
                        vertex,
                        representative))
                {
                    return weldedIndex;
                }
            }

            return -1;
        }

        private static bool VertexEquals(
            IReadOnlyList<NativeArray<byte>> streams,
            IReadOnlyList<int> strides,
            int first,
            int second)
        {
            for (int stream = 0; stream < streams.Count; stream++)
            {
                int stride = strides[stream];
                int firstOffset = first * stride;
                int secondOffset = second * stride;
                for (int offset = 0; offset < stride; offset++)
                {
                    if (streams[stream][firstOffset + offset]
                        != streams[stream][secondOffset + offset])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static ulong HashVertex(
            IReadOnlyList<NativeArray<byte>> streams,
            IReadOnlyList<int> strides,
            int vertex)
        {
            const ulong offsetBasis = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;
            ulong hash = offsetBasis;
            for (int stream = 0; stream < streams.Count; stream++)
            {
                int stride = strides[stream];
                int start = vertex * stride;
                for (int offset = 0; offset < stride; offset++)
                {
                    hash ^= streams[stream][start + offset];
                    hash *= prime;
                }
            }

            return hash;
        }

        private readonly struct SourceSubMesh
        {
            public SourceSubMesh(
                int[] absoluteIndices,
                MeshTopology topology,
                int baseVertex,
                Bounds bounds)
            {
                AbsoluteIndices = absoluteIndices;
                Topology = topology;
                BaseVertex = baseVertex;
                Bounds = bounds;
            }

            public int[] AbsoluteIndices { get; }
            public MeshTopology Topology { get; }
            public int BaseVertex { get; }
            public Bounds Bounds { get; }
        }
    }
}
