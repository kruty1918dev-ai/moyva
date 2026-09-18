using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Jsonization;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime.Geography
{
    /// <summary>
    /// Authoritative runtime world generator. When enabled by
    /// <see cref="WorldGenerationConfig"/> it replaces the graph compile step:
    /// the deterministic geography engine produces the logical tile map,
    /// authored terrain levels, object map and fairness-validated spawn hints,
    /// while visual layers are provisioned from the canonical TWC id mapping.
    /// The graph path remains the fallback when the config is missing or
    /// disabled.
    /// </summary>
    internal sealed class GeographyMapGenerationStep
    {
        private const string ConfigId = "world-generation-config";
        private const string LogTag = "[MoyvaWorldGen]";

        private readonly WorldGeographyEngine _engine;
        private readonly GeographyBuildLayerProvisioner _provisioner;
        private readonly GeographyLogicalMapFactory _logicalMapFactory;
        private readonly ITileWorldCreatorBuildEnvironment _buildEnvironment;
        private readonly IGeneratorTerrainLevelService _terrainLevelService;

        private WorldGenerationConfig _config;
        private bool _configLoaded;

        public GeographyMapGenerationStep(
            WorldGeographyEngine engine,
            GeographyBuildLayerProvisioner provisioner,
            GeographyLogicalMapFactory logicalMapFactory,
            [InjectOptional] ITileWorldCreatorBuildEnvironment buildEnvironment = null,
            [InjectOptional] IGeneratorTerrainLevelService terrainLevelService = null)
        {
            _engine = engine;
            _provisioner = provisioner;
            _logicalMapFactory = logicalMapFactory;
            _buildEnvironment = buildEnvironment;
            _terrainLevelService = terrainLevelService;
        }

        public bool TryGenerate(
            GraphTwcMapGenerationRequest request,
            int seed,
            Vector2Int mapSize,
            out GraphTwcMapGenerationResult result)
        {
            result = null;
            var config = LoadConfig();
            if (config == null || !config.Enabled || request.Manager == null)
                return false;

            var geographyRequest = BuildRequest(config, seed, mapSize);
            var geography = _engine.Generate(geographyRequest);
            var visuals = _provisioner.Ensure(
                request.Manager,
                _buildEnvironment?.Mapping,
                CollectTileIds(geography));
            var logicalMap = _logicalMapFactory.Build(geography, visuals);
            var compiledLayers = _logicalMapFactory.BuildCompiledLayers(visuals);

            PublishTerrainLevels(geography, logicalMap);
            LogReport(geography);

            float cellSize = request.Manager.configuration != null
                             && request.Manager.configuration.cellSize > 0.0001f
                ? request.Manager.configuration.cellSize
                : 1f;

            bool hasBounds = GeneratedWorldBoundsUtility.TryCreateTileWorldBounds(
                request.Manager.transform, mapSize.x, mapSize.y, cellSize, out var bounds);

            result = new GraphTwcMapGenerationResult
            {
                BiomeMap = geography.TileMap,
                ObjectMap = geography.ObjectMap
                            ?? new string[mapSize.x, mapSize.y],
                HeightMap = geography.HeightMap,
                BuildingMap = new string[mapSize.x, mapSize.y],
                LogicalMap = logicalMap,
                CompiledLayers = compiledLayers,
                CellSize = cellSize,
                HasBaseMapWorldBounds = hasBounds,
                BaseMapWorldBounds = bounds,
                TerrainLevelMap = geography.TerrainLevelMap,
                ForceChunkFirst = true,
                HasAuthoredGeography = true,
                SpawnHints = geography.SpawnHints,
                GeographyReport = geography.Report,
            };
            return true;
        }

        private WorldGenerationRequest BuildRequest(
            WorldGenerationConfig config,
            int seed,
            Vector2Int mapSize)
        {
            var options = _buildEnvironment?.Options;
            int mapType = GameLaunchContext.HasWorldSettings ? GameLaunchContext.MapType : -1;
            var archetype = mapType >= 0
                ? WorldArchetypeResolver.FromMapType(mapType, seed)
                : WorldArchetype.Balanced;
            int playerCount = GameLaunchContext.HasWorldSettings
                ? Mathf.Max(1, GameLaunchContext.MaxPlayers)
                : 2;

            var levels = config.TerrainLevels ?? new WorldGenerationConfig.TerrainLevelSettings();
            return new WorldGenerationRequest(
                seed: seed,
                width: Mathf.Max(1, mapSize.x),
                height: Mathf.Max(1, mapSize.y),
                archetype: archetype,
                playerCount: playerCount,
                config: config,
                waterLevel: options?.WaterTerrainLevel ?? levels.WaterLevel,
                shoreLevel: options?.ShoreTerrainLevel ?? levels.ShoreLevel,
                landLevel: options?.LandTerrainLevel ?? levels.LandLevel,
                hillLevel: options?.HillTerrainLevel ?? levels.HillLevel,
                maxLevel: options?.MaxTerrainLevel ?? levels.MaxLevel,
                heightStep: options?.TerrainHeightStep ?? levels.HeightStep,
                waterSurfaceOffset: levels.WaterSurfaceOffset);
        }

        private void PublishTerrainLevels(
            WorldGeographyResult geography,
            GraphLogicalTileMap logicalMap)
        {
            if (_terrainLevelService == null)
                return;

            if (geography.TerrainLevelMap != null)
                _terrainLevelService.SetLevelMap(geography.TerrainLevelMap);
            if (logicalMap?.SurfaceHeights != null)
                _terrainLevelService.SetSurfaceHeightMap(logicalMap.SurfaceHeights);
        }

        private static IEnumerable<string> CollectTileIds(WorldGeographyResult geography)
        {
            var ids = new HashSet<string>(System.StringComparer.Ordinal);
            var tiles = geography.TileMap;
            for (int x = 0; x < tiles.GetLength(0); x++)
            for (int y = 0; y < tiles.GetLength(1); y++)
                if (!string.IsNullOrWhiteSpace(tiles[x, y]))
                    ids.Add(tiles[x, y]);
            return ids;
        }

        private WorldGenerationConfig LoadConfig()
        {
            if (_configLoaded)
                return _config;
            _configLoaded = true;

            try
            {
                _config = MoyvaJsonRuntime.Get<WorldGenerationConfig>(ConfigId);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"{LogTag} Failed to load '{ConfigId}': {ex.Message}");
            }

            if (_config == null)
                Debug.LogWarning($"{LogTag} '{ConfigId}' not found; graph generator stays active.");
            return _config;
        }

        private static void LogReport(WorldGeographyResult geography)
        {
            var report = geography?.Report;
            if (report == null)
                return;

            if (report.Accepted)
                Debug.Log($"{LogTag} {report.ToSummary()}");
            else
                Debug.LogWarning($"{LogTag} best-effort world: {report.ToSummary()}");
        }
    }
}
