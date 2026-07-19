using System.Collections.Generic;
using Kruty1918.Moyva.MapChunks.API;
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

            filter.sharedMesh = combined;
            renderer.sharedMaterials = _materials.ToArray();
            _meshRegistry.Register(combined);

            _diagnostics.LogChunkMesh(terrainRoot.name, combined.vertexCount, CountIndices(combined));
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
            if (source.Mesh.subMeshCount <= 0)
                return;

            Material[] materials = source.Materials;
            int subMeshCount = source.Mesh.subMeshCount;
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
                    mesh = source.Mesh,
                    subMeshIndex = Mathf.Min(subMesh, source.Mesh.subMeshCount - 1),
                    transform = source.LocalMatrix
                });
            }
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
            float xMin = core.xMin * cellSize;
            float xMax = core.xMax * cellSize;
            float zMin = core.yMin * cellSize;
            float zMax = core.yMax * cellSize;
            float width = Mathf.Max(cellSize, xMax - xMin);
            float depth = Mathf.Max(cellSize, zMax - zMin);
            float height = Mathf.Max(1f, actualBounds.size.y);

            return new Bounds(
                new Vector3(
                    xMin + width * 0.5f,
                    actualBounds.center.y,
                    zMin + depth * 0.5f),
                new Vector3(width, height, depth));
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
            if (filter == null || filter.sharedMesh == null)
                return;

            if (Application.isPlaying)
                Object.Destroy(filter.sharedMesh);
            else
                Object.DestroyImmediate(filter.sharedMesh);
            filter.sharedMesh = null;
        }

        private static Material ResolveMaterial(Material[] materials, int subMesh)
        {
            if (materials != null && materials.Length > 0)
                return materials[Mathf.Clamp(subMesh, 0, materials.Length - 1)];

            return null;
        }
    }
}
