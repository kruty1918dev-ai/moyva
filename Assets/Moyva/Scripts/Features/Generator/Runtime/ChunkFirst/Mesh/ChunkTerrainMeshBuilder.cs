using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
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
        private readonly Dictionary<Material, List<CombineInstance>> _byMaterial = new Dictionary<Material, List<CombineInstance>>();
        private readonly Stack<List<CombineInstance>> _combineListPool = new Stack<List<CombineInstance>>();
        private readonly List<CombineInstance> _finalCombine = new List<CombineInstance>(16);
        private readonly List<Material> _materials = new List<Material>(16);
        private readonly List<TileMeshSource> _cellSources = new List<TileMeshSource>(4);
        /*
         * Every provider source is generated exactly once for the whole map,
         * then assigned to the chunk containing its physical TileCenterXZ.
         */
        private readonly Dictionary<
            MapChunkCoord,
            List<CanonicalTileMeshSource>>
            _canonicalSourcesByChunk =
                new Dictionary<
                    MapChunkCoord,
                    List<CanonicalTileMeshSource>>();

        private readonly Stack<List<CanonicalTileMeshSource>>
            _canonicalSourceListPool =
                new Stack<List<CanonicalTileMeshSource>>();

        private IReadOnlyDictionary<
            Vector2Int,
            ResolvedTileComposition>
            _canonicalResolvedCells;

        private IResolvedTileMeshSource
            _canonicalMeshSource;

        private int _canonicalChunkSize;
        private int _canonicalMapWidth;
        private int _canonicalMapHeight;
        private float _canonicalCellSize = 1f;
        private readonly Dictionary<(Mesh mesh, Vector2 center), Mesh>
            _borderClampedMeshCache =
                new Dictionary<(Mesh mesh, Vector2 center), Mesh>();
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
        private readonly Dictionary<TileHeightWarpMeshKey, Mesh>
            _warpedMeshCache =
                new Dictionary<TileHeightWarpMeshKey, Mesh>();
        private readonly HashSet<TileHeightWarpMeshKey>
            _warpedPassthroughCache =
                new HashSet<TileHeightWarpMeshKey>();
        public ChunkTerrainMeshBuilder(ChunkFirstRuntimeMeshRegistry meshRegistry)
        {
            _meshRegistry = meshRegistry;
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
            EnsureCanonicalSourcePlan(
                area,
                resolvedCells,
                meshSource);

            int fragmentCount =
                CollectFragments(
                    area,
                    resolvedCells,
                    meshSource);
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

            filter.sharedMesh =
                combined;

            renderer.sharedMaterials =
                _materials.ToArray();

            ConfigureTerrainCollider(
                terrainRoot,
                combined);

            _meshRegistry.Register(combined);

            return 1;
        }

        private int CollectFragments(
            ChunkBuildArea area,
            IReadOnlyDictionary<Vector2Int, ResolvedTileComposition> resolvedCells,
            IResolvedTileMeshSource meshSource)
        {
            if (!_canonicalSourcesByChunk.TryGetValue(
                    area.Coord,
                    out List<CanonicalTileMeshSource> plannedSources)
                || plannedSources == null)
            {
                return 0;
            }

            int count = 0;

            for (int i = 0;
                 i < plannedSources.Count;
                 i++)
            {
                CanonicalTileMeshSource planned =
                    plannedSources[i];

                AddSource(
                    planned.Source);

                count++;
            }

            return count;
        }

        private void EnsureCanonicalSourcePlan(
            ChunkBuildArea area,
            IReadOnlyDictionary<Vector2Int, ResolvedTileComposition> resolvedCells,
            IResolvedTileMeshSource meshSource)
        {
            int chunkSize =
                ResolveCanonicalChunkSize(
                    area);

            bool firstChunkOfBuild =
                area.Coord.X == 0
                && area.Coord.Y == 0;

            bool planIsCurrent =
                object.ReferenceEquals(
                    _canonicalResolvedCells,
                    resolvedCells)
                && object.ReferenceEquals(
                    _canonicalMeshSource,
                    meshSource)
                && _canonicalChunkSize
                   == chunkSize
                && _canonicalSourcesByChunk.Count > 0;

            if (!firstChunkOfBuild
                && planIsCurrent)
            {
                return;
            }

            BuildCanonicalSourcePlan(
                resolvedCells,
                meshSource,
                area.CellSize,
                chunkSize);
        }

        private static int ResolveCanonicalChunkSize(
            ChunkBuildArea area)
        {
            /*
             * MOYVA_FULL_CHUNKS_16_PASS76: map dimensions are cropped before
             * logical generation, so runtime CoreRects are full 16x16 chunks.
             * Keep stride recovery defensive for legacy/dev data.
             */
            int resolved =
                Mathf.Max(
                    1,
                    Mathf.Max(
                        area.CoreRect.width,
                        area.CoreRect.height));

            resolved =
                Mathf.Max(
                    resolved,
                    ResolveAxisStride(
                        area.Coord.X,
                        area.CoreRect.xMin));

            resolved =
                Mathf.Max(
                    resolved,
                    ResolveAxisStride(
                        area.Coord.Y,
                        area.CoreRect.yMin));

            return resolved;
        }

        private static int ResolveAxisStride(
            int chunkCoordinate,
            int coreMinimum)
        {
            if (chunkCoordinate <= 0
                || coreMinimum <= 0)
            {
                return 0;
            }

            int stride =
                coreMinimum
                / chunkCoordinate;

            if (stride <= 0
                || stride * chunkCoordinate
                   != coreMinimum)
            {
                return 0;
            }

            return stride;
        }

        private void BuildCanonicalSourcePlan(
            IReadOnlyDictionary<Vector2Int, ResolvedTileComposition> resolvedCells,
            IResolvedTileMeshSource meshSource,
            float cellSize,
            int chunkSize)
        {
            RecycleCanonicalSourcePlan();

            _canonicalResolvedCells =
                resolvedCells;

            _canonicalMeshSource =
                meshSource;

            _canonicalChunkSize =
                Mathf.Max(
                    1,
                    chunkSize);

            ResolveMapDimensions(
                resolvedCells,
                out _canonicalMapWidth,
                out _canonicalMapHeight);

            _canonicalCellSize =
                cellSize > 0.0001f ? cellSize : 1f;

            if (_canonicalMapWidth <= 0
                || _canonicalMapHeight <= 0)
            {
                Debug.LogError(
                    "[MOYVA_CHUNK_OWNERSHIP] PLAN_FAILED " +
                    "reason=empty-resolved-map");

                return;
            }

            int chunkCountX =
                Mathf.CeilToInt(
                    _canonicalMapWidth
                    / (float)_canonicalChunkSize);

            int chunkCountY =
                Mathf.CeilToInt(
                    _canonicalMapHeight
                    / (float)_canonicalChunkSize);

            int resolvedCellCount = 0;
            int sourceCount = 0;
            int validSourceCount = 0;
            int invalidSourceCount = 0;
            int reassignedSourceCount = 0;
            int outOfMapSourceCount = 0;

            /*
             * Preserve the old generation order:
             * chunk row -> chunk column -> local row -> local column.
             */
            for (int chunkY = 0;
                 chunkY < chunkCountY;
                 chunkY++)
            {
                int yMin =
                    chunkY
                    * _canonicalChunkSize;

                int yMax =
                    Mathf.Min(
                        yMin + _canonicalChunkSize,
                        _canonicalMapHeight);

                for (int chunkX = 0;
                     chunkX < chunkCountX;
                     chunkX++)
                {
                    int xMin =
                        chunkX
                        * _canonicalChunkSize;

                    int xMax =
                        Mathf.Min(
                            xMin + _canonicalChunkSize,
                            _canonicalMapWidth);

                    for (int y = yMin;
                         y < yMax;
                         y++)
                    {
                        for (int x = xMin;
                             x < xMax;
                             x++)
                        {
                            var logicalCell =
                                new Vector2Int(
                                    x,
                                    y);

                            if (!resolvedCells.TryGetValue(
                                    logicalCell,
                                    out ResolvedTileComposition composition))
                            {
                                continue;
                            }

                            resolvedCellCount++;

                            _cellSources.Clear();

                            int collected =
                                meshSource.CollectMeshSources(
                                    composition,
                                    _cellSources);

                            int safeCount =
                                Mathf.Min(
                                    collected,
                                    _cellSources.Count);

                            MapChunkCoord logicalOwner =
                                ResolveChunkCoord(
                                    logicalCell,
                                    _canonicalChunkSize);

                            for (int sourceIndex = 0;
                                 sourceIndex < safeCount;
                                 sourceIndex++)
                            {
                                TileMeshSource source =
                                    _cellSources[sourceIndex];

                                sourceCount++;

                                if (source.IsValid)
                                    validSourceCount++;
                                else
                                    invalidSourceCount++;

                                if (!TryResolvePhysicalCell(
                                        logicalCell,
                                        source,
                                        cellSize,
                                        _canonicalMapWidth,
                                        _canonicalMapHeight,
                                        out Vector2Int physicalCell))
                                {
                                    outOfMapSourceCount++;
                                    continue;
                                }

                                MapChunkCoord physicalOwner =
                                    ResolveChunkCoord(
                                        physicalCell,
                                        _canonicalChunkSize);

                                if (!physicalOwner.Equals(
                                        logicalOwner))
                                {
                                    reassignedSourceCount++;
                                }

                                GetCanonicalSourceBucket(
                                        physicalOwner)
                                    .Add(
                                        new CanonicalTileMeshSource(
                                            logicalCell,
                                            physicalCell,
                                            source));
                            }
                        }
                    }
                }
            }

            int assignedSourceCount = 0;

            foreach (KeyValuePair<
                         MapChunkCoord,
                         List<CanonicalTileMeshSource>> pair
                     in _canonicalSourcesByChunk)
            {
                assignedSourceCount +=
                    pair.Value?.Count ?? 0;
            }
        }

        private List<CanonicalTileMeshSource>
            GetCanonicalSourceBucket(
                MapChunkCoord coord)
        {
            if (_canonicalSourcesByChunk.TryGetValue(
                    coord,
                    out List<CanonicalTileMeshSource> bucket)
                && bucket != null)
            {
                return bucket;
            }

            bucket =
                _canonicalSourceListPool.Count > 0
                    ? _canonicalSourceListPool.Pop()
                    : new List<CanonicalTileMeshSource>(128);

            _canonicalSourcesByChunk[coord] =
                bucket;

            return bucket;
        }

        private void RecycleCanonicalSourcePlan()
        {
            foreach (KeyValuePair<
                         MapChunkCoord,
                         List<CanonicalTileMeshSource>> pair
                     in _canonicalSourcesByChunk)
            {
                List<CanonicalTileMeshSource> bucket =
                    pair.Value;

                if (bucket == null)
                    continue;

                bucket.Clear();
                _canonicalSourceListPool.Push(
                    bucket);
            }

            _canonicalSourcesByChunk.Clear();
        }

        private static void ResolveMapDimensions(
            IReadOnlyDictionary<Vector2Int, ResolvedTileComposition> resolvedCells,
            out int width,
            out int height)
        {
            width = 0;
            height = 0;

            if (resolvedCells == null)
                return;

            foreach (KeyValuePair<
                         Vector2Int,
                         ResolvedTileComposition> pair
                     in resolvedCells)
            {
                width =
                    Mathf.Max(
                        width,
                        pair.Key.x + 1);

                height =
                    Mathf.Max(
                        height,
                        pair.Key.y + 1);
            }
        }

        private static bool TryResolvePhysicalCell(
            Vector2Int logicalCell,
            TileMeshSource source,
            float cellSize,
            int mapWidth,
            int mapHeight,
            out Vector2Int physicalCell)
        {
            float safeCellSize =
                Mathf.Max(
                    0.0001f,
                    cellSize);

            if (!source.HasTileFootprint
                || !IsFinite(source.TileCenterXZ.x)
                || !IsFinite(source.TileCenterXZ.y))
            {
                physicalCell = logicalCell;

                return physicalCell.x >= 0
                    && physicalCell.y >= 0
                    && physicalCell.x < mapWidth
                    && physicalCell.y < mapHeight;
            }

            int x =
                Mathf.FloorToInt(
                    (
                        source.TileCenterXZ.x
                        + safeCellSize * 0.5f
                    )
                    / safeCellSize);

            int y =
                Mathf.FloorToInt(
                    (
                        source.TileCenterXZ.y
                        + safeCellSize * 0.5f
                    )
                    / safeCellSize);

            /*
             * Dual-grid fragments center on the vertices between logical
             * cells, so their physical index lattice spans
             * 0..mapWidth / 0..mapHeight — one slot more than the cell
             * count. Border vertices still cover the map's outer half
             * cells: clamp them onto the edge cell so they join the owning
             * chunk instead of being discarded.
             */
            if (x < 0
                || y < 0
                || x > mapWidth
                || y > mapHeight)
            {
                physicalCell = logicalCell;

                return false;
            }

            physicalCell =
                new Vector2Int(
                    Mathf.Min(x, mapWidth - 1),
                    Mathf.Min(y, mapHeight - 1));

            return true;
        }

        private static MapChunkCoord ResolveChunkCoord(
            Vector2Int cell,
            int chunkSize)
        {
            int safeChunkSize =
                Mathf.Max(
                    1,
                    chunkSize);

            return new MapChunkCoord(
                cell.x / safeChunkSize,
                cell.y / safeChunkSize);
        }

        private static bool IsFinite(
            float value)
        {
            return !float.IsNaN(value)
                && !float.IsInfinity(value);
        }

        private readonly struct CanonicalTileMeshSource
        {
            public CanonicalTileMeshSource(
                Vector2Int logicalCell,
                Vector2Int physicalCell,
                TileMeshSource source)
            {
                LogicalCell =
                    logicalCell;

                PhysicalCell =
                    physicalCell;

                Source =
                    source;
            }

            public Vector2Int LogicalCell { get; }
            public Vector2Int PhysicalCell { get; }
            public TileMeshSource Source { get; }
        }

        private void AddSource(TileMeshSource source)
        {
            if (!source.IsValid || source.Mesh.subMeshCount <= 0)
                return;

            Mesh mesh = ResolveVisibleMesh(source);

            if (mesh == null || mesh.subMeshCount <= 0)
                return;

            Material[] materials = source.Materials;
            int subMeshCount = mesh.subMeshCount;

            for (int subMesh = 0; subMesh < subMeshCount; subMesh++)
            {
                Material material = ResolveMaterial(
                    materials,
                    subMesh);

                if (material == null)
                    continue;

                if (!_byMaterial.TryGetValue(
                        material,
                        out List<CombineInstance> combines))
                {
                    combines = _combineListPool.Count > 0
                        ? _combineListPool.Pop()
                        : new List<CombineInstance>(64);

                    _byMaterial[material] = combines;
                }

                combines.Add(new CombineInstance
                {
                    mesh = mesh,
                    subMeshIndex = Mathf.Min(
                        subMesh,
                        mesh.subMeshCount - 1),
                    transform = source.LocalMatrix
                });

            }
        }

        private Mesh ResolveVisibleMesh(TileMeshSource source)
        {
            Mesh mesh =
                source.TileGeometryMode == TileGeometryMode.SurfaceOnly
                    ? ResolveSurfaceOnlyMesh(source)
                    : ResolveSolidTerrainMesh(source);

            mesh = ResolveWarpedMesh(source, mesh);
            return ResolveBorderClampedMesh(source, mesh);
        }

        /// <summary>
        /// Dual-grid border fragments span a quad centred on a vertex half a
        /// cell outside the map rect; their authored drop aprons then protrude
        /// past the world edge and read as detached plates. Vertices of
        /// footprint-bearing sources that cross the map boundary are clamped
        /// onto the rect so the outer apron folds into a flush rim wall.
        /// </summary>
        private Mesh ResolveBorderClampedMesh(TileMeshSource source, Mesh mesh)
        {
            if (mesh == null
                || !source.HasTileFootprint
                || _canonicalMapWidth <= 0)
            {
                return mesh;
            }

            float cs = _canonicalCellSize;
            float minX = -0.5f * cs;
            float minZ = -0.5f * cs;
            float maxX = (_canonicalMapWidth - 0.5f) * cs;
            float maxZ = (_canonicalMapHeight - 0.5f) * cs;

            float extent = source.TileHalfExtent;
            float cx = source.TileCenterXZ.x;
            float cz = source.TileCenterXZ.y;
            if (cx - extent >= minX && cx + extent <= maxX
                && cz - extent >= minZ && cz + extent <= maxZ)
            {
                return mesh;
            }

            var key = (mesh, source.TileCenterXZ);
            if (_borderClampedMeshCache.TryGetValue(key, out Mesh cached)
                && cached != null)
            {
                return cached;
            }

            Vector3[] verts = mesh.vertices;
            Matrix4x4 toWorld = source.LocalMatrix;
            Matrix4x4 toLocal = toWorld.inverse;
            var clamped = new Vector3[verts.Length];
            bool changed = false;
            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 world = toWorld.MultiplyPoint3x4(verts[i]);
                float nx = Mathf.Clamp(world.x, minX, maxX);
                float nz = Mathf.Clamp(world.z, minZ, maxZ);
                if (nx != world.x || nz != world.z)
                {
                    changed = true;
                    world.x = nx;
                    world.z = nz;
                    clamped[i] = toLocal.MultiplyPoint3x4(world);
                }
                else
                {
                    clamped[i] = verts[i];
                }
            }

            if (!changed)
            {
                _borderClampedMeshCache[key] = mesh;
                return mesh;
            }

            Mesh copy = UnityEngine.Object.Instantiate(mesh);
            copy.name = mesh.name + "_borderClamp";
            copy.vertices = clamped;
            copy.RecalculateNormals();
            copy.RecalculateBounds();
            _meshRegistry.Register(copy);
            _borderClampedMeshCache[key] = copy;
            return copy;
        }

        private Mesh ResolveSolidTerrainMesh(TileMeshSource source)
        {
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

        /// <summary>
        /// Applies the corner-height slope warp to the resolved mesh. Runs
        /// after surface-only filtering and vertical fill so overlays and
        /// generated closure skirts tilt together with the surface. Sources
        /// without corner heights (normal-grid tiles, stair passages) pass
        /// through untouched.
        /// </summary>
        private Mesh ResolveWarpedMesh(TileMeshSource source, Mesh mesh)
        {
            // SurfaceOnly sheets (water) must keep a flat plane; only solid
            // terrain fragments are sheared onto the corner-height field.
            if (mesh == null
                || !source.HasCornerHeights
                || source.TileGeometryMode != TileGeometryMode.SolidTerrain)
            {
                return mesh;
            }

            TileHeightWarpMeshKey key =
                TileHeightWarpMeshKey.Create(source, mesh);
            if (_warpedPassthroughCache.Contains(key))
                return mesh;

            if (_warpedMeshCache.TryGetValue(key, out Mesh cached)
                && cached != null)
            {
                return cached;
            }

            if (!TileSurfaceHeightWarpUtility.TryCreate(
                    source,
                    mesh,
                    out Mesh warped)
                || warped == null)
            {
                _warpedPassthroughCache.Add(key);
                return mesh;
            }

            _warpedMeshCache[key] = warped;
            _meshRegistry.Register(warped);
            return warped;
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
            /*
             * ExactVertexWeldMeshUtility preserves the final appearance while
             * removing unreferenced vertices and exact duplicates introduced
             * by mesh combining. It runs in every mode: with slope-warped
             * fragments the shared border vertices carry identical payloads
             * and welding eliminates the coincident-edge seams that skipping
             * it left visible during Editor Play Mode.
             */
            if (ExactVertexWeldMeshUtility.TryCreate(
                    mesh,
                    out Mesh welded))
            {
                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(mesh);
                else
                    UnityEngine.Object.DestroyImmediate(mesh);

                mesh =
                    welded;
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

        private static void ConfigureTerrainCollider(
            Transform terrainRoot,
            Mesh combined)
        {
#if UNITY_EDITOR && !MOYVA_EDITOR_RUNTIME_TERRAIN_COLLIDERS
            /*
             * Construction pointer mapping and tile clicks use the
             * generated terrain-height map and mathematical grid planes.
             * They do not require a cooked PhysX representation.
             *
             * Skip the expensive per-chunk MeshCollider cooking during
             * ordinary Editor Play Mode. Player builds remain unchanged.
             */
            if (Application.isPlaying)
            {
                var editorCollider =
                    terrainRoot.GetComponent<MeshCollider>();

                if (editorCollider != null)
                {
                    editorCollider.sharedMesh =
                        null;

                    editorCollider.enabled =
                        false;
                }

                return;
            }
#endif

            var collider =
                terrainRoot.GetComponent<MeshCollider>();

            if (collider == null)
            {
                collider =
                    terrainRoot.gameObject
                        .AddComponent<MeshCollider>();
            }

            collider.enabled =
                true;

            if (collider.sharedMesh != null)
            {
                collider.sharedMesh =
                    null;
            }

            collider.cookingOptions =
                MeshColliderCookingOptions.None;

            collider.sharedMesh =
                combined;
        }

        private static void ClearExistingMesh(Transform terrainRoot)
        {
            var filter =
                terrainRoot.GetComponent<MeshFilter>();

            var collider =
                terrainRoot.GetComponent<MeshCollider>();

            if (collider != null
                && collider.sharedMesh != null)
            {
                collider.sharedMesh =
                    null;
            }

            if (filter == null
                || filter.sharedMesh == null)
            {
                return;
            }

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
}
