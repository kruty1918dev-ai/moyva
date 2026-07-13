using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal sealed class ChunkFirstWorldBuildService : IChunkFirstWorldBuildService
    {
        private const int NeighborhoodHalo = 1;
        private readonly ITileWorldCreatorBuildEnvironment _environment;
        private readonly IMapChunkSettingsProvider _chunkSettings;
        private readonly IMapVisualChunkRootService _roots;
        private readonly IMapVisualChunkRegistry _registry;
        private readonly IChunkBuildAreaPlanner _planner;
        private readonly ITileNeighborhoodFactory _neighborhoods;
        private readonly IResolvedTileCompositionResolver _resolver;
        private readonly IResolvedTileMeshSource _meshSource;
        private readonly IChunkFirstTwcVisualCleanupService _twcVisualCleanup;
        private readonly IChunkTerrainMeshBuilder _meshBuilder;
        private readonly IChunkFirstObjectSpawner _objectSpawner;
        private readonly ChunkFirstRuntimeMeshRegistry _meshRegistry;
        private readonly ChunkFirstBuildDiagnostics _diagnostics;
        private readonly Dictionary<Vector2Int, ResolvedTileComposition> _resolved = new Dictionary<Vector2Int, ResolvedTileComposition>();
        private readonly List<MapChunkCoord> _singleChunk = new List<MapChunkCoord>(1);

        public ChunkFirstWorldBuildService(
            ITileWorldCreatorBuildEnvironment environment,
            IMapChunkSettingsProvider chunkSettings,
            IMapVisualChunkRootService roots,
            IMapVisualChunkRegistry registry,
            IChunkBuildAreaPlanner planner,
            ITileNeighborhoodFactory neighborhoods,
            IResolvedTileCompositionResolver resolver,
            IResolvedTileMeshSource meshSource,
            IChunkFirstTwcVisualCleanupService twcVisualCleanup,
            IChunkTerrainMeshBuilder meshBuilder,
            IChunkFirstObjectSpawner objectSpawner,
            ChunkFirstRuntimeMeshRegistry meshRegistry,
            ChunkFirstBuildDiagnostics diagnostics)
        {
            _environment = environment;
            _chunkSettings = chunkSettings;
            _roots = roots;
            _registry = registry;
            _planner = planner;
            _neighborhoods = neighborhoods;
            _resolver = resolver;
            _meshSource = meshSource;
            _twcVisualCleanup = twcVisualCleanup;
            _meshBuilder = meshBuilder;
            _objectSpawner = objectSpawner;
            _meshRegistry = meshRegistry;
            _diagnostics = diagnostics;
        }

        public TileWorldCreatorWorldBuildResult Build(
            GeneratedWorldData worldData,
            Configuration configuration,
            TileWorldCreatorTerrainBuildPolicyResult terrainPolicy)
        {
            _diagnostics.LogStart(terrainPolicy.Mode, worldData, _chunkSettings.ChunkSize);
            if (worldData?.LogicalTileMap == null)
            {
                _diagnostics.LogFailure("Chunk-first selected but logical tile stack map is missing.");
                return TileWorldCreatorWorldBuildResult.Disabled;
            }

            using (TileWorldCreatorChunkFirstGuard.Enter())
            {
                if (worldData.Seed != 0)
                    GlobalSeed.Set(worldData.Seed);

                _twcVisualCleanup.ClearVisualBuildOutput(_environment.Manager);
                _meshRegistry.Clear();
                _registry.Clear();

                var areas = _planner.Build(
                    worldData.Width,
                    worldData.Height,
                    ResolveCellSize(worldData, configuration),
                    worldData.HasBaseMapWorldBounds,
                    worldData.BaseMapWorldBounds,
                    NeighborhoodHalo);

                ResolveCompositions(worldData.LogicalTileMap, areas);
                int objectCandidates = CountObjectLikeSamples(worldData.LogicalTileMap)
                                       + CountCells(worldData.ObjectMap)
                                       + CountCells(worldData.BuildingMap);
                _diagnostics.LogPlan(
                    areas.Count,
                    CountStackSamples(worldData.LogicalTileMap),
                    CountResolvedTerrain(),
                    objectCandidates);

                int chunksBuilt = BuildTerrainMeshes(areas);
                int objectsSpawned = _objectSpawner.Spawn(worldData);
                _diagnostics.LogComplete(chunksBuilt, objectsSpawned);

                return CreateResult(worldData, configuration);
            }
        }

        private void ResolveCompositions(GraphLogicalTileMap map, IReadOnlyList<ChunkBuildArea> areas)
        {
            _resolved.Clear();
            for (int areaIndex = 0; areaIndex < areas.Count; areaIndex++)
            {
                RectInt core = areas[areaIndex].CoreRect;
                for (int y = core.yMin; y < core.yMax; y++)
                for (int x = core.xMin; x < core.xMax; x++)
                {
                    var cell = new Vector2Int(x, y);
                    _resolved[cell] = _resolver.Resolve(cell, _neighborhoods.Create(map, cell));
                }
            }
        }

        private int BuildTerrainMeshes(IReadOnlyList<ChunkBuildArea> areas)
        {
            int built = 0;
            for (int i = 0; i < areas.Count; i++)
            {
                Transform chunkRoot = _roots.GetOrCreateRoot(areas[i].Coord);
                built += _meshBuilder.Build(chunkRoot, areas[i], _resolved, _meshSource);
                RegisterChunkRenderer(chunkRoot, areas[i].Coord);
            }

            return built;
        }

        private int CountResolvedTerrain()
        {
            int count = 0;
            foreach (var pair in _resolved)
            {
                if (pair.Value.HasMainTerrain)
                    count++;
            }

            return count;
        }

        private void RegisterChunkRenderer(Transform chunkRoot, MapChunkCoord coord)
        {
            Transform terrain = chunkRoot != null ? chunkRoot.Find("TerrainMesh") : null;
            var renderer = terrain != null ? terrain.GetComponent<Renderer>() : null;
            if (renderer != null)
            {
                _singleChunk.Clear();
                _singleChunk.Add(coord);
                _registry.Register(renderer, _singleChunk);
            }
        }

        private TileWorldCreatorWorldBuildResult CreateResult(GeneratedWorldData worldData, Configuration configuration)
        {
            CollectMappedIds(worldData.BiomeMap, _environment.Mapping.TryResolveTerrainLayer, out var terrainIds);
            CollectMappedIds(worldData.ObjectMap, _environment.Mapping.TryResolveObjectLayer, out var objectIds);
            CollectMappedIds(worldData.BuildingMap, _environment.Mapping.TryResolveBuildingLayer, out var buildingIds);
            CollectMappedStackIds(worldData.LogicalTileMap, LayerKind.ObjectSpawn, _environment.Mapping.TryResolveObjectLayer, objectIds);
            CollectMappedStackIds(worldData.LogicalTileMap, LayerKind.Decoration, _environment.Mapping.TryResolveObjectLayer, objectIds);
            CollectMappedStackIds(worldData.LogicalTileMap, LayerKind.Building, _environment.Mapping.TryResolveBuildingLayer, buildingIds);

            var options = _environment.Options;
            return new TileWorldCreatorWorldBuildResult(
                terrainIds,
                objectIds,
                buildingIds,
                options.ReplaceMappedTerrainVisuals,
                options.ReplaceMappedObjectVisuals,
                options.ReplaceMappedBuildingVisuals,
                options.SuppressMoyvaLayerDataWhenTerrainMapped && terrainIds.Count > 0,
                ResolveCellSize(worldData, configuration),
                worldData.HasBaseMapWorldBounds,
                worldData.BaseMapWorldBounds);
        }

        private static void CollectMappedIds(
            string[,] map,
            TryResolveLayer resolveLayer,
            out HashSet<string> ids)
        {
            ids = new HashSet<string>();
            if (map == null || resolveLayer == null)
                return;

            for (int x = 0; x < map.GetLength(0); x++)
            for (int y = 0; y < map.GetLength(1); y++)
            {
                string id = map[x, y];
                if (!string.IsNullOrWhiteSpace(id) && resolveLayer(id, out _))
                    ids.Add(id);
            }
        }

        private static int CountStackSamples(GraphLogicalTileMap map)
        {
            int count = 0;
            for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
                count += map.GetCellStack(x, y)?.Count ?? 0;
            return count;
        }

        private static int CountCells(string[,] map)
        {
            if (map == null)
                return 0;

            int count = 0;
            for (int x = 0; x < map.GetLength(0); x++)
            for (int y = 0; y < map.GetLength(1); y++)
                if (!string.IsNullOrWhiteSpace(map[x, y]))
                    count++;
            return count;
        }

        private static int CountObjectLikeSamples(GraphLogicalTileMap map)
        {
            if (map == null)
                return 0;

            int count = 0;
            for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
            {
                var stack = map.GetCellStack(x, y);
                if (stack == null)
                    continue;

                for (int i = 0; i < stack.Samples.Count; i++)
                {
                    var kind = stack.Samples[i].LayerKind;
                    if (kind == LayerKind.ObjectSpawn || kind == LayerKind.Building || kind == LayerKind.Decoration)
                        count++;
                }
            }

            return count;
        }

        private static float ResolveCellSize(GeneratedWorldData worldData, Configuration configuration)
        {
            if (worldData != null && worldData.CellSize > 0.0001f)
                return worldData.CellSize;
            return configuration != null && configuration.cellSize > 0.0001f ? configuration.cellSize : 1f;
        }

        private static void CollectMappedStackIds(
            GraphLogicalTileMap map,
            LayerKind kind,
            TryResolveLayer resolveLayer,
            HashSet<string> ids)
        {
            if (map == null || resolveLayer == null || ids == null)
                return;

            for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
            {
                var stack = map.GetCellStack(x, y);
                if (stack == null)
                    continue;

                for (int i = 0; i < stack.Samples.Count; i++)
                {
                    var sample = stack.Samples[i];
                    if (sample.LayerKind != kind)
                        continue;

                    TryCollectSampleId(sample.TileId, resolveLayer, ids);
                    TryCollectSampleId(sample.PresetId, resolveLayer, ids);
                    TryCollectSampleId(sample.GraphLayerId, resolveLayer, ids);
                }
            }
        }

        private static void TryCollectSampleId(string id, TryResolveLayer resolveLayer, HashSet<string> ids)
        {
            if (!string.IsNullOrWhiteSpace(id) && resolveLayer(id, out _))
                ids.Add(id);
        }

        private delegate bool TryResolveLayer(string id, out TileWorldCreatorIdMappingSO.LayerMapping mapping);
    }
}
