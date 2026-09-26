using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal sealed class ChunkFirstObjectSpawner : IChunkFirstObjectSpawner
    {
        private const string ObjectsRootName = "Objects";
        private readonly ITileWorldCreatorBuildEnvironment _environment;
        private readonly IMapChunkLayoutService _layout;
        private readonly IMapVisualChunkRootService _roots;
        private readonly IGeneratorTerrainLevelService _terrainLevels;
        private readonly ITerrainPlacementPolicy _placementPolicy;
        private readonly EnvironmentObjectPlacementResolver _placementResolver;
        private readonly FootprintRules _footprintRules;
        private readonly Dictionary<MapChunkCoord, Transform> _objectRoots = new Dictionary<MapChunkCoord, Transform>();
        private readonly Dictionary<Vector2Int, List<GameObject>> _propsByCell = new Dictionary<Vector2Int, List<GameObject>>();
        private readonly HashSet<Vector2Int> _clearedPropCells = new HashSet<Vector2Int>();

        public ChunkFirstObjectSpawner(
            ITileWorldCreatorBuildEnvironment environment,
            IMapChunkLayoutService layout,
            IMapVisualChunkRootService roots,
            [Zenject.InjectOptional] IGeneratorTerrainLevelService terrainLevels = null,
            [Zenject.InjectOptional] ITerrainPlacementPolicy placementPolicy = null,
            [Zenject.InjectOptional] EnvironmentObjectPlacementResolver placementResolver = null,
            [Zenject.InjectOptional] EnvironmentDecorationConfig decorationConfig = null)
        {
            _environment = environment;
            _layout = layout;
            _roots = roots;
            _terrainLevels = terrainLevels;
            _placementPolicy = placementPolicy;
            _placementResolver = placementResolver;
            _footprintRules = decorationConfig?.Footprint ?? new FootprintRules();
        }

        public int Spawn(GeneratedWorldData worldData)
        {
            // A fresh world build resets cleared-footprint exclusions; cleared
            // cells are re-recorded by the placement signals that follow.
            _clearedPropCells.Clear();
            Clear();
            if (worldData?.LogicalTileMap != null)
                return SpawnStacks(worldData.LogicalTileMap, worldData.Seed);

            var mapping = _environment?.Mapping;
            TryResolveLayer objectResolver = null;
            TryResolveLayer buildingResolver = null;
            if (mapping != null)
            {
                objectResolver = mapping.TryResolveObjectLayer;
                buildingResolver = mapping.TryResolveBuildingLayer;
            }

            int count = 0;
            count += SpawnMap(worldData?.ObjectMap, objectResolver, worldData?.Seed ?? 1, worldData, skipWaterCells: true);
            count += SpawnMap(worldData?.BuildingMap, buildingResolver, worldData?.Seed ?? 1, worldData, skipWaterCells: false);
            return count;
        }

        public void Clear()
        {
            foreach (var pair in _objectRoots)
            {
                if (pair.Value == null)
                    continue;

                for (int i = pair.Value.childCount - 1; i >= 0; i--)
                {
                    if (Application.isPlaying)
                        Object.Destroy(pair.Value.GetChild(i).gameObject);
                    else
                        Object.DestroyImmediate(pair.Value.GetChild(i).gameObject);
                }
            }

            _propsByCell.Clear();
        }

        public int ClearPropsInCells(IReadOnlyList<Vector2Int> cells)
        {
            if (cells == null || cells.Count == 0)
                return 0;

            _clearedPropCells.UnionWith(cells);
            int cleared = 0;
            for (int i = 0; i < cells.Count; i++)
            {
                if (!_propsByCell.TryGetValue(cells[i], out var props))
                    continue;

                for (int p = 0; p < props.Count; p++)
                {
                    GameObject prop = props[p];
                    if (prop == null)
                        continue;

                    if (Application.isPlaying)
                        Object.Destroy(prop);
                    else
                        Object.DestroyImmediate(prop);
                    cleared++;
                }

                _propsByCell.Remove(cells[i]);
            }

            return cleared;
        }

        private void TrackProp(Vector2Int cell, GameObject instance)
        {
            if (!_propsByCell.TryGetValue(cell, out var props))
            {
                props = new List<GameObject>(1);
                _propsByCell[cell] = props;
            }

            props.Add(instance);
        }

        private int SpawnStacks(LogicalTileMap map, int seed)
        {
            int spawned = 0;
            for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
            {
                var stack = map.GetCellStack(x, y);
                if (stack == null || stack.IsEmpty)
                    continue;

                bool waterCell = HasSurfaceOnlyTerrain(stack);
                string terrainTileId = map.TileIds[x, y];
                var cell = new Vector2Int(x, y);
                int candidateIndex = 0;
                for (int i = 0; i < stack.Samples.Count; i++)
                {
                    var sample = stack.Samples[i];
                    if (!IsObjectLike(sample.LayerKind))
                        continue;

                    // Props and decorations never sit on open water; a
                    // building sample on water (docks, watermill) or a
                    // water-authored prop (river strip, lily) stays.
                    if (waterCell
                        && sample.LayerKind != LayerKind.Building
                        && !IsWaterAuthoredObject(sample))
                    {
                        continue;
                    }

                    // Prop cells cleared by a committed building footprint
                    // stay clear on every respawn of this world.
                    if (_clearedPropCells.Contains(cell)
                        && sample.LayerKind != LayerKind.Building)
                    {
                        continue;
                    }

                    // Tile-tagged spawn/build blocks (shore sand) apply to the
                    // object layer as well as to decorations.
                    if (!AllowsPlacement(
                            terrainTileId,
                            sample.LayerKind == LayerKind.Building
                                ? TerrainPlacementOperation.Building
                                : TerrainPlacementOperation.ObjectSpawn))
                    {
                        continue;
                    }

                    if (TrySpawnSample(map, sample, cell, seed, candidateIndex))
                        spawned++;
                    candidateIndex++;
                }
            }

            return spawned;
        }

        /// <summary>
        /// A water cell renders a SurfaceOnly water sheet as its terrain; land
        /// cells keep a SolidTerrain main layer even when a shore sheet
        /// overlaps them.
        /// </summary>
        private static bool HasSurfaceOnlyTerrain(TileStackCell stack)
        {
            for (int i = 0; i < stack.Samples.Count; i++)
            {
                var sample = stack.Samples[i];
                if (sample.IsTerrainLike
                    && sample.TileGeometryMode == TileGeometryMode.SurfaceOnly)
                {
                    return true;
                }
            }

            return false;
        }

        private bool TrySpawnSample(
            LogicalTileMap map,
            TileLayerSample sample,
            Vector2Int cell,
            int seed,
            int candidateIndex)
        {
            if (!TryResolvePrefab(sample, out string resolvedId, out var mapping))
                return false;
            if (!_layout.TryGetChunkCoord(cell, out var coord))
                return false;

            GameObject prefab = mapping.RegistryVisualPrefab;
            float cellSize = Mathf.Max(0.0001f, _layout.CellSize);
            uint hash = ChunkFirstStableHash.ObjectVariant(
                seed,
                cell,
                !string.IsNullOrWhiteSpace(sample.LayerId) ? sample.LayerId : resolvedId,
                candidateIndex,
                prefab.name);
            Quaternion rotation = sample.LayerKind != LayerKind.Building
                ? ResolveSurfaceAlignment(cell, hash)
                : Quaternion.identity;
            var position = new Vector3(
                cell.x * cellSize,
                sample.SurfaceHeight,
                cell.y * cellSize);

            // Props sit fully on valid cells: the transformed footprint —
            // crown and LOD meshes included — must not overlap water or
            // spawn-blocked terrain. A prop that does not fit shifts within
            // a bounded radius; otherwise it is skipped. Buildings keep
            // their authored placement and gameplay footprint.
            if (sample.LayerKind != LayerKind.Building
                && _placementResolver != null
                && _footprintRules.ValidateHeavyFootprints
                && !IsWaterAuthoredObject(sample))
            {
                var request = new EnvironmentObjectPlacementResolver.Request(
                    prefab,
                    position,
                    rotation,
                    Vector3.one,
                    map.Width,
                    map.Height,
                    cellSize,
                    c => IsFootprintCellAcceptable(map, c),
                    c => SurfaceHeightOrNaN(map, c),
                    _footprintRules.MaxShiftCells,
                    _footprintRules.MaxGroundDeltaMeters,
                    _footprintRules.FootprintShrink);
                if (!_placementResolver.TryResolve(request, out Vector3 resolved))
                    return false;

                position = resolved;
            }

            // Ground non-building props by their lowest transformed point on
            // the lowest terrain under the footprint; pivots alone leave
            // rocks hovering over slopes.
            if (sample.LayerKind != LayerKind.Building && _placementResolver != null)
            {
                position.y = _placementResolver.ResolveGroundedY(
                    prefab,
                    position,
                    rotation,
                    Vector3.one,
                    cellSize,
                    c => SurfaceHeightOrNaN(map, c),
                    sample.SurfaceHeight,
                    _footprintRules.FootprintShrink);
            }

            Transform root = GetObjectRoot(coord);
            var instance = Object.Instantiate(prefab, root, false);
            instance.name = $"{prefab.name}_{hash:x8}";
            instance.transform.localPosition = position;
            if (sample.LayerKind != LayerKind.Building)
            {
                instance.transform.localRotation = rotation;
                TrackProp(cell, instance);
            }

            return true;
        }

        // Every cell under a prop footprint must be in bounds, land-only and
        // free of spawn-blocked terrain tags.
        private bool IsFootprintCellAcceptable(LogicalTileMap map, Vector2Int cell)
        {
            if (cell.x < 0 || cell.y < 0 || cell.x >= map.Width || cell.y >= map.Height)
                return false;

            var stack = map.GetCellStack(cell.x, cell.y);
            if (stack != null && HasSurfaceOnlyTerrain(stack))
                return false;

            return AllowsPlacement(map.TileIds[cell.x, cell.y], TerrainPlacementOperation.ObjectSpawn);
        }

        private static float SurfaceHeightOrNaN(LogicalTileMap map, Vector2Int cell)
        {
            var heights = map?.SurfaceHeights;
            if (heights == null
                || cell.x < 0 || cell.y < 0
                || cell.x >= heights.GetLength(0) || cell.y >= heights.GetLength(1))
            {
                return float.NaN;
            }

            return heights[cell.x, cell.y];
        }

        private static bool IsWaterAuthoredObject(TileLayerSample sample)
            => IsWaterObjectId(sample.TileId)
               || IsWaterObjectId(sample.PresetId)
               || IsWaterObjectId(sample.LayerId);

        /// <summary>
        /// Aligns props/decorations to the terrain surface normal and applies a
        /// deterministic yaw derived from the variant hash. Buildings stay
        /// upright; they flatten onto their footprint via the alignment
        /// service instead.
        /// </summary>
        private Quaternion ResolveSurfaceAlignment(Vector2Int cell, uint hash)
        {
            float yaw = (hash % 3600) / 10f;
            Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);
            if (_terrainLevels == null
                || !_terrainLevels.TryGetSurfaceNormal(cell, _layout.CellSize, out Vector3 normal)
                || normal.y > 0.9999f)
            {
                return yawRotation;
            }

            return Quaternion.FromToRotation(Vector3.up, normal) * yawRotation;
        }

        private int SpawnMap(
            string[,] map,
            TryResolveLayer resolveLayer,
            int seed,
            GeneratedWorldData worldData,
            bool skipWaterCells)
        {
            if (map == null || resolveLayer == null)
                return 0;

            int spawned = 0;
            for (int x = 0; x < map.GetLength(0); x++)
            for (int y = 0; y < map.GetLength(1); y++)
            {
                string id = map[x, y];
                if (string.IsNullOrWhiteSpace(id) || !resolveLayer(id, out var mapping) || mapping.RegistryVisualPrefab == null)
                    continue;

                if (skipWaterCells && IsWaterCell(worldData, x, y) && !IsWaterObjectId(id))
                    continue;

                if (!AllowsPlacement(
                        ResolveWorldTileId(worldData, x, y),
                        skipWaterCells
                            ? TerrainPlacementOperation.ObjectSpawn
                            : TerrainPlacementOperation.Building))
                {
                    continue;
                }

                var cell = new Vector2Int(x, y);
                if (skipWaterCells && _clearedPropCells.Contains(cell))
                    continue;

                if (!_layout.TryGetChunkCoord(cell, out var coord))
                    continue;

                GameObject prefab = mapping.RegistryVisualPrefab;
                float cellSize = Mathf.Max(0.0001f, _layout.CellSize);
                var position = new Vector3(x * cellSize, 0f, y * cellSize);
                Quaternion rotation = Quaternion.identity;

                // Legacy flat-map path gets the same footprint guarantee as
                // the logical-stack path when the map is available.
                LogicalTileMap logicalMap = worldData?.LogicalTileMap;
                if (skipWaterCells
                    && logicalMap != null
                    && _placementResolver != null
                    && _footprintRules.ValidateHeavyFootprints
                    && !IsWaterObjectId(id))
                {
                    var request = new EnvironmentObjectPlacementResolver.Request(
                        prefab,
                        position,
                        rotation,
                        Vector3.one,
                        logicalMap.Width,
                        logicalMap.Height,
                        cellSize,
                        c => IsFootprintCellAcceptable(logicalMap, c),
                        c => SurfaceHeightOrNaN(logicalMap, c),
                        _footprintRules.MaxShiftCells,
                        _footprintRules.MaxGroundDeltaMeters,
                        _footprintRules.FootprintShrink);
                    if (!_placementResolver.TryResolve(request, out Vector3 resolved))
                        continue;

                    position = resolved;
                    position.y = _placementResolver.ResolveGroundedY(
                        prefab,
                        position,
                        rotation,
                        Vector3.one,
                        cellSize,
                        c => SurfaceHeightOrNaN(logicalMap, c),
                        0f,
                        _footprintRules.FootprintShrink);
                }

                Transform root = GetObjectRoot(coord);
                uint hash = ChunkFirstStableHash.ObjectVariant(seed, cell, id, 0, prefab.name);
                var instance = Object.Instantiate(prefab, root, false);
                instance.name = $"{prefab.name}_{hash:x8}";
                instance.transform.localPosition = position;
                if (skipWaterCells)
                    TrackProp(cell, instance);
                spawned++;
            }

            return spawned;
        }

        private Transform GetObjectRoot(MapChunkCoord coord)
        {
            if (_objectRoots.TryGetValue(coord, out var existing) && existing != null)
                return existing;

            Transform chunkRoot = _roots.GetOrCreateRoot(coord);
            Transform root = chunkRoot.Find(ObjectsRootName);
            if (root == null)
            {
                var gameObject = new GameObject(ObjectsRootName);
                root = gameObject.transform;
                root.SetParent(chunkRoot, false);
                root.localPosition = Vector3.zero;
                root.localRotation = Quaternion.identity;
                root.localScale = Vector3.one;
            }

            _objectRoots[coord] = root;
            return root;
        }

        private bool TryResolvePrefab(
            TileLayerSample sample,
            out string resolvedId,
            out TileWorldCreatorIdMappingSO.LayerMapping mapping)
        {
            resolvedId = null;
            mapping = null;

            TryResolveLayer resolver = null;
            var mappingSource = _environment?.Mapping;
            if (mappingSource != null)
            {
                resolver = sample.LayerKind == LayerKind.Building
                    ? mappingSource.TryResolveBuildingLayer
                    : mappingSource.TryResolveObjectLayer;
            }

            if (resolver == null)
                return false;

            if (TryResolveId(resolver, sample.TileId, out resolvedId, out mapping)
                || TryResolveId(resolver, sample.PresetId, out resolvedId, out mapping)
                || TryResolveId(resolver, sample.LayerId, out resolvedId, out mapping))
            {
                return mapping?.RegistryVisualPrefab != null;
            }

            return false;
        }

        private static bool TryResolveId(
            TryResolveLayer resolver,
            string id,
            out string resolvedId,
            out TileWorldCreatorIdMappingSO.LayerMapping mapping)
        {
            resolvedId = null;
            mapping = null;
            if (string.IsNullOrWhiteSpace(id) || !resolver(id, out mapping))
                return false;

            resolvedId = id;
            return true;
        }

        private static bool IsObjectLike(LayerKind kind)
            => kind == LayerKind.ObjectSpawn
               || kind == LayerKind.Building
               || kind == LayerKind.Decoration;

        private bool AllowsPlacement(string tileTypeId, TerrainPlacementOperation operation)
            => _placementPolicy == null
               || _placementPolicy.AllowsPlacement(tileTypeId, operation);

        private static bool IsWaterCell(GeneratedWorldData worldData, int x, int y)
        {
            string tileId = ResolveWorldTileId(worldData, x, y);
            if (string.IsNullOrWhiteSpace(tileId))
                return false;

            tileId = tileId.ToLowerInvariant();
            return tileId.Contains("water") || tileId.Contains("river")
                || tileId.Contains("lake") || tileId.Contains("ocean");
        }

        private static string ResolveWorldTileId(GeneratedWorldData worldData, int x, int y)
        {
            string tileId = worldData?.GameplayTileMap != null
                            && x < worldData.GameplayTileMap.GetLength(0)
                            && y < worldData.GameplayTileMap.GetLength(1)
                ? worldData.GameplayTileMap[x, y]
                : null;
            if (string.IsNullOrWhiteSpace(tileId)
                && worldData?.BiomeMap != null
                && x < worldData.BiomeMap.GetLength(0)
                && y < worldData.BiomeMap.GetLength(1))
            {
                tileId = worldData.BiomeMap[x, y];
            }

            return tileId;
        }

        /// <summary>
        /// Objects authored for water (river strips, lilies) stay on water
        /// cells even though land props are filtered out.
        /// </summary>
        private static bool IsWaterObjectId(string objectId)
        {
            if (string.IsNullOrWhiteSpace(objectId))
                return false;

            objectId = objectId.ToLowerInvariant();
            return objectId.Contains("water") || objectId.Contains("river")
                || objectId.Contains("lily") || objectId.Contains("lake");
        }

        private delegate bool TryResolveLayer(string id, out TileWorldCreatorIdMappingSO.LayerMapping mapping);
    }
}
