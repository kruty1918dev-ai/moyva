using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.MapChunks.API;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal sealed class ChunkTerrainMeshBuilder : IChunkTerrainMeshBuilder
    {
        private const string TerrainObjectName = "TerrainMesh";
        private readonly ChunkFirstRuntimeMeshRegistry _meshRegistry;
        private readonly ChunkFirstBuildDiagnostics _diagnostics;
        private readonly Dictionary<Material, List<CombineInstance>> _byMaterial = new Dictionary<Material, List<CombineInstance>>();
        private readonly Stack<List<CombineInstance>> _combineListPool = new Stack<List<CombineInstance>>();
        private readonly List<CombineInstance> _finalCombine = new List<CombineInstance>(16);
        private readonly List<Material> _materials = new List<Material>(16);
        private readonly List<TileMeshSource> _cellSources = new List<TileMeshSource>(4);
        private readonly Dictionary<TileVerticalFillMeshKey, Mesh> _verticalMeshCache =
            new Dictionary<TileVerticalFillMeshKey, Mesh>();
        private readonly HashSet<TileVerticalFillMeshKey> _verticalMeshPassthroughCache =
            new HashSet<TileVerticalFillMeshKey>();
        private readonly Dictionary<TileSurfaceOnlyMeshKey, Mesh>
            _surfaceOnlyMeshCache =
                new Dictionary<TileSurfaceOnlyMeshKey, Mesh>();
        private readonly HashSet<TileSurfaceOnlyMeshKey>
            _surfaceOnlyEmptyCache =
                new HashSet<TileSurfaceOnlyMeshKey>();
        private readonly HashSet<TileSurfaceOnlyMeshKey>
            _surfaceOnlyFailureCache =
                new HashSet<TileSurfaceOnlyMeshKey>();
        private int _sourceVertices;
        private int _sourceIndices;
        private int _processedVertices;
        private int _processedIndices;

        public ChunkTerrainMeshBuilder(
            ChunkFirstRuntimeMeshRegistry meshRegistry,
            ChunkFirstBuildDiagnostics diagnostics)
        {
            _meshRegistry = meshRegistry;
            _diagnostics = diagnostics;
        }

        public int Build(
            Transform chunkRoot,
            ChunkBuildArea area,
            IReadOnlyDictionary<Vector2Int, ResolvedTileComposition> resolvedCells,
            IResolvedTileMeshSource meshSource)
        {
            if (chunkRoot == null || resolvedCells == null || meshSource == null)
                return 0;

            var terrainRoot = EnsureTerrainRoot(chunkRoot);
            ClearExistingMesh(terrainRoot);
            RecycleCombineLists();
            _finalCombine.Clear();
            _materials.Clear();
            _sourceVertices = 0;
            _sourceIndices = 0;
            _processedVertices = 0;
            _processedIndices = 0;

            int fragmentCount = CollectFragments(area.CoreRect, resolvedCells, meshSource);
            if (fragmentCount == 0)
                return 0;

            Mesh combined = CombineByMaterial(terrainRoot.name, area);
            if (combined == null || combined.vertexCount == 0)
                return 0;

            var filter = terrainRoot.GetComponent<MeshFilter>();
            if (filter == null)
                filter = terrainRoot.gameObject.AddComponent<MeshFilter>();
            var renderer = terrainRoot.GetComponent<MeshRenderer>();
            if (renderer == null)
                renderer = terrainRoot.gameObject.AddComponent<MeshRenderer>();
            var collider = terrainRoot.GetComponent<MeshCollider>();
            if (collider == null)
                collider = terrainRoot.gameObject.AddComponent<MeshCollider>();

            filter.sharedMesh = combined;
            renderer.sharedMaterials = _materials.ToArray();
            // Rendering and physics must consume the exact same optimized mesh;
            // assigning null first forces Unity to recook a regenerated chunk.
            collider.sharedMesh = null;
            collider.cookingOptions = MeshColliderCookingOptions.None;
            collider.sharedMesh = combined;
            _meshRegistry.Register(combined);

            _diagnostics.LogChunkMesh(
                terrainRoot.name,
                _sourceVertices,
                _sourceIndices,
                _processedVertices,
                _processedIndices,
                combined.vertexCount,
                CountIndices(combined));
            return 1;
        }

        private int CollectFragments(
            RectInt coreRect,
            IReadOnlyDictionary<Vector2Int, ResolvedTileComposition> resolvedCells,
            IResolvedTileMeshSource meshSource)
        {
            int count = 0;
            for (int y = coreRect.yMin; y < coreRect.yMax; y++)
            for (int x = coreRect.xMin; x < coreRect.xMax; x++)
            {
                var cell = new Vector2Int(x, y);
                if (!resolvedCells.TryGetValue(cell, out var composition))
                    continue;

                _cellSources.Clear();
                int sourceCount = meshSource.CollectMeshSources(composition, _cellSources);
                for (int i = 0; i < sourceCount; i++)
                {
                    AddSource(_cellSources[i]);
                    count++;
                }
            }

            return count;
        }

        private void AddSource(TileMeshSource source)
        {
            if (!source.IsValid || source.Mesh.subMeshCount <= 0)
                return;

            Mesh mesh = ResolveVisibleMesh(source);
            if (mesh == null || mesh.subMeshCount <= 0)
                return;

            _sourceVertices += source.Mesh.vertexCount;
            _sourceIndices += CountIndices(source.Mesh);
            _processedVertices += mesh.vertexCount;
            _processedIndices += CountIndices(mesh);

            Material[] materials = source.Materials;
            int subMeshCount = mesh.subMeshCount;
            for (int subMesh = 0; subMesh < subMeshCount; subMesh++)
            {
                Material material = ResolveMaterial(materials, subMesh);
                if (material == null)
                    continue;

                if (!_byMaterial.TryGetValue(material, out var combines))
                {
                    combines = _combineListPool.Count > 0
                        ? _combineListPool.Pop()
                        : new List<CombineInstance>(64);
                    _byMaterial[material] = combines;
                }

                combines.Add(new CombineInstance
                {
                    mesh = mesh,
                    subMeshIndex = Mathf.Min(subMesh, mesh.subMeshCount - 1),
                    transform = source.LocalMatrix
                });
            }
        }

        private Mesh ResolveVisibleMesh(TileMeshSource source)
        {
            if (source.TileGeometryMode == TileGeometryMode.SurfaceOnly)
                return ResolveSurfaceOnlyMesh(source);

            if (!source.HasVisibleBottomY)
                return source.Mesh;

            TileVerticalFillMeshKey key = TileVerticalFillMeshKey.Create(source);
            if (_verticalMeshPassthroughCache.Contains(key))
                return source.Mesh;

            if (_verticalMeshCache.TryGetValue(key, out Mesh cached) && cached != null)
                return cached;

            if (!TileVerticalFillMeshUtility.TryCreate(source, out Mesh processed)
                || processed == null)
            {
                _verticalMeshPassthroughCache.Add(key);
                return source.Mesh;
            }

            _verticalMeshCache[key] = processed;
            _meshRegistry.Register(processed);
            return processed;
        }

        private Mesh ResolveSurfaceOnlyMesh(TileMeshSource source)
        {
            TileSurfaceOnlyMeshKey key =
                TileSurfaceOnlyMeshKey.Create(source);
            if (_surfaceOnlyEmptyCache.Contains(key)
                || _surfaceOnlyFailureCache.Contains(key))
            {
                return null;
            }

            if (_surfaceOnlyMeshCache.TryGetValue(key, out Mesh cached)
                && cached != null)
            {
                return cached;
            }

            SurfaceOnlyMeshBuildStatus status =
                TileSurfaceOnlyMeshUtility.Create(source, out Mesh processed);
            if (status == SurfaceOnlyMeshBuildStatus.Empty)
            {
                _surfaceOnlyEmptyCache.Add(key);
                return null;
            }

            if (status != SurfaceOnlyMeshBuildStatus.Created
                || processed == null)
            {
                if (_surfaceOnlyFailureCache.Add(key))
                {
                    Debug.LogError(
                        $"[MoyvaChunkFirst] SurfaceOnly mesh '{source.Mesh?.name ?? "<null>"}' " +
                        "could not be filtered. The authored volume was omitted to avoid emitting sides or a bottom.");
                }

                return null;
            }

            _surfaceOnlyMeshCache[key] = processed;
            _meshRegistry.Register(processed);
            return processed;
        }

        private Mesh CombineByMaterial(string meshName, ChunkBuildArea area)
        {
            long vertexCount = 0;
            foreach (var pair in _byMaterial)
            {
                if (pair.Value.Count == 0)
                    continue;

                var subMesh = new Mesh
                {
                    name = $"{meshName}_{_materials.Count}_SubMesh",
                    indexFormat = IndexFormat.UInt32
                };
                subMesh.CombineMeshes(pair.Value.ToArray(), true, true);
                vertexCount += subMesh.vertexCount;
                _meshRegistry.Register(subMesh);
                _materials.Add(pair.Key);
                _finalCombine.Add(new CombineInstance
                {
                    mesh = subMesh,
                    subMeshIndex = 0,
                    transform = Matrix4x4.identity
                });
            }

            if (_finalCombine.Count == 0)
                return null;

            var mesh = new Mesh
            {
                name = meshName,
                indexFormat = vertexCount > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16
            };
            mesh.CombineMeshes(_finalCombine.ToArray(), false, false);
            if (ExactVertexWeldMeshUtility.TryCreate(
                    mesh,
                    out Mesh welded))
            {
                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(mesh);
                else
                    UnityEngine.Object.DestroyImmediate(mesh);
                mesh = welded;
            }

            mesh.RecalculateBounds();
            mesh.bounds = CreateStableChunkBounds(area, mesh.bounds);
            if (!mesh.HasVertexAttribute(VertexAttribute.Normal))
                mesh.RecalculateNormals();
            return mesh;
        }

        private void RecycleCombineLists()
        {
            foreach (List<CombineInstance> combines in _byMaterial.Values)
            {
                combines.Clear();
                _combineListPool.Push(combines);
            }

            _byMaterial.Clear();
        }

        private static int CountIndices(Mesh mesh)
        {
            long count = 0;
            for (int subMesh = 0; subMesh < mesh.subMeshCount; subMesh++)
                count += (long)mesh.GetIndexCount(subMesh);

            return count > int.MaxValue ? int.MaxValue : (int)count;
        }

        private static Bounds CreateStableChunkBounds(ChunkBuildArea area, Bounds actualBounds)
        {
            RectInt core = area.CoreRect;
            float cellSize = area.CellSize > 0.0001f ? area.CellSize : 1f;
            float xMin = (core.xMin - 0.5f) * cellSize;
            float xMax = (core.xMax - 0.5f) * cellSize;
            float zMin = (core.yMin - 0.5f) * cellSize;
            float zMax = (core.yMax - 0.5f) * cellSize;
            float width = Mathf.Max(cellSize, xMax - xMin);
            float depth = Mathf.Max(cellSize, zMax - zMin);
            float height = Mathf.Max(1f, actualBounds.size.y);

            var stableBounds = new Bounds(
                new Vector3(
                    xMin + width * 0.5f,
                    actualBounds.center.y,
                    zMin + depth * 0.5f),
                new Vector3(width, height, depth));
            stableBounds.Encapsulate(actualBounds.min);
            stableBounds.Encapsulate(actualBounds.max);
            return stableBounds;
        }

        private static Transform EnsureTerrainRoot(Transform chunkRoot)
        {
            var existing = chunkRoot.Find(TerrainObjectName);
            if (existing != null)
                return existing;

            var gameObject = new GameObject(TerrainObjectName);
            var transform = gameObject.transform;
            transform.SetParent(chunkRoot, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            return transform;
        }

        private static void ClearExistingMesh(Transform terrainRoot)
        {
            var filter = terrainRoot.GetComponent<MeshFilter>();
            var collider = terrainRoot.GetComponent<MeshCollider>();
            if (collider != null)
                collider.sharedMesh = null;
            if (filter == null || filter.sharedMesh == null)
                return;

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(filter.sharedMesh);
            else
                UnityEngine.Object.DestroyImmediate(filter.sharedMesh);
            filter.sharedMesh = null;
        }

        private static Material ResolveMaterial(Material[] materials, int subMesh)
        {
            if (materials != null && materials.Length > 0)
                return materials[Mathf.Clamp(subMesh, 0, materials.Length - 1)];

            return null;
        }
    }

    /// <summary>
    /// Welds vertices only when their complete raw vertex-stream payload is
    /// byte-identical. This preserves hard normals, tangents, colors, skin data
    /// and UV seams while removing unreferenced vertices and exact duplicates
    /// introduced by mesh combining.
    /// </summary>
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
