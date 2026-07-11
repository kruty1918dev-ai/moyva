using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TileWorldCreatorWorldBuildBridge : ITileWorldCreatorWorldBuildBridge
    {
        private const string LogTag = "[MoyvaTWCHeight]";

        private readonly ITileWorldCreatorBuildEnvironment _environment;
        private readonly IGeneratorTerrainLevelService _terrainLevelService;
        private readonly IMapChunkSettingsProvider _chunkSettings;
        private readonly ITileWorldCreatorTerrainBuildPolicyService _terrainPolicyService;
        private readonly ITileWorldCreatorTerrainLevelMapService _terrainLevels;
        private readonly ITileWorldCreatorShoreBandService _shoreBand;
        private readonly ITileWorldCreatorConfigurationPreparationService _configurationPreparation;
        private readonly ITileWorldCreatorTerrainBuildLayerConfigurationService _buildLayerConfiguration;
        private readonly ITileWorldCreatorLayerPositionCollector _positionCollector;
        private readonly ITileWorldCreatorBuildDiagnosticsService _diagnostics;
        private readonly ITileWorldCreatorBuildExecutionService _execution;
        private readonly ITileWorldCreatorTerrainVisualPostProcessor _visualPostProcessor;
        private readonly ITileWorldCreatorTerrainHeightPublisher _heightPublisher;
        private readonly IChunkFirstWorldBuildService _chunkFirstBuild;

        public TileWorldCreatorWorldBuildBridge(
            ITileWorldCreatorBuildEnvironment environment,
            ITileWorldCreatorTerrainBuildPolicyService terrainPolicyService,
            ITileWorldCreatorTerrainLevelMapService terrainLevels,
            ITileWorldCreatorShoreBandService shoreBand,
            ITileWorldCreatorConfigurationPreparationService configurationPreparation,
            ITileWorldCreatorTerrainBuildLayerConfigurationService buildLayerConfiguration,
            ITileWorldCreatorLayerPositionCollector positionCollector,
            ITileWorldCreatorBuildDiagnosticsService diagnostics,
            ITileWorldCreatorBuildExecutionService execution,
            ITileWorldCreatorTerrainVisualPostProcessor visualPostProcessor,
            ITileWorldCreatorTerrainHeightPublisher heightPublisher,
            [InjectOptional] IGeneratorTerrainLevelService terrainLevelService = null,
            [InjectOptional] IMapChunkSettingsProvider chunkSettings = null,
            [InjectOptional] IChunkFirstWorldBuildService chunkFirstBuild = null)
        {
            _environment = environment;
            _terrainPolicyService = terrainPolicyService;
            _terrainLevels = terrainLevels;
            _shoreBand = shoreBand;
            _configurationPreparation = configurationPreparation;
            _buildLayerConfiguration = buildLayerConfiguration;
            _positionCollector = positionCollector;
            _diagnostics = diagnostics;
            _execution = execution;
            _visualPostProcessor = visualPostProcessor;
            _heightPublisher = heightPublisher;
            _terrainLevelService = terrainLevelService;
            _chunkSettings = chunkSettings;
            _chunkFirstBuild = chunkFirstBuild;
        }

        public TileWorldCreatorWorldBuildResult Build(GeneratedWorldData worldData)
        {
            TileWorldCreatorManager manager = _environment?.Manager;
            TileWorldCreatorIdMappingSO mapping = _environment?.Mapping;
            TileWorldCreatorBuildOptions options = _environment?.Options;
            if (manager == null || mapping == null || worldData == null)
                return TileWorldCreatorWorldBuildResult.Disabled;

            _terrainLevelService?.Clear();
            Configuration configuration = manager.configuration;
            if (configuration == null)
            {
                Debug.LogWarning($"{LogTag} TileWorldCreatorManager has no configuration assigned.");
                return TileWorldCreatorWorldBuildResult.Disabled;
            }

            TileWorldCreatorTerrainBuildPolicyResult terrainPolicy = worldData.ForceChunkFirstCompositeBuild
                ? new TileWorldCreatorTerrainBuildPolicyResult(
                    TileWorldCreatorTerrainBuildMode.ChunkFirstCompositeMesh,
                    _chunkSettings?.ChunkSize ?? 0,
                    options.ApplyIntegerTerrainHeights)
                : _terrainPolicyService.Resolve(options, _chunkSettings?.ChunkSize ?? 0);
            _diagnostics.LogBuildStart(worldData, configuration);
            PrepareTerrainData(worldData, options);

            if (terrainPolicy.UsesChunkFirstComposite)
                return BuildChunkFirst(worldData, configuration, terrainPolicy);

            TileWorldCreatorLayerPositionSet positions = _positionCollector.Collect(worldData, configuration);
            LogPositionSummaries(positions, configuration);
            if (!positions.HasAnyMappedLayer)
            {
                Debug.LogWarning($"{LogTag} TWC build disabled: no mapped terrain/object/building positions were collected. BiomeMap={TileWorldCreatorMapFormatUtility.FormatMapSize(worldData.BiomeMap)}.");
                return TileWorldCreatorWorldBuildResult.Disabled;
            }

            try
            {
                _configurationPreparation.Prepare(configuration, worldData, terrainPolicy);
                _buildLayerConfiguration.Configure(configuration);
                _configurationPreparation.ConfigureTerrainHeightContext(worldData, terrainPolicy);
                _execution.Execute(configuration, positions, terrainPolicy);
                _visualPostProcessor.Apply(worldData, configuration, terrainPolicy);
                _heightPublisher.Publish(worldData, configuration);

                bool hasBounds = GeneratedWorldBoundsUtility.TryCreateTileWorldBounds(
                    manager.transform,
                    worldData.Width,
                    worldData.Height,
                    configuration.cellSize,
                    out Bounds baseMapWorldBounds);

                return TileWorldCreatorWorldBuildResult.FromPositions(
                    positions,
                    options,
                    configuration.cellSize,
                    hasBounds,
                    baseMapWorldBounds);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"{LogTag} Build failed: {ex}");
                return TileWorldCreatorWorldBuildResult.Disabled;
            }
        }

        private TileWorldCreatorWorldBuildResult BuildChunkFirst(
            GeneratedWorldData worldData,
            Configuration configuration,
            TileWorldCreatorTerrainBuildPolicyResult terrainPolicy)
        {
            if (_chunkFirstBuild == null)
            {
                Debug.LogError($"{LogTag} Chunk-first build selected but IChunkFirstWorldBuildService is not bound.");
                return TileWorldCreatorWorldBuildResult.Disabled;
            }

            _configurationPreparation.Prepare(configuration, worldData, terrainPolicy);
            TileWorldCreatorWorldBuildResult result = _chunkFirstBuild.Build(worldData, configuration, terrainPolicy);
            PublishChunkFirstSurfaceHeights(worldData);
            return result;
        }

        private void PublishChunkFirstSurfaceHeights(GeneratedWorldData worldData)
        {
            if (_terrainLevelService == null || worldData == null)
                return;

            if (worldData.TerrainLevelMap != null)
                _terrainLevelService.SetLevelMap(worldData.TerrainLevelMap);

            if (worldData.LogicalTileMap?.SurfaceHeights != null)
                _terrainLevelService.SetSurfaceHeightMap(worldData.LogicalTileMap.SurfaceHeights);
        }

        private void PrepareTerrainData(GeneratedWorldData worldData, TileWorldCreatorBuildOptions options)
        {
            if (options.ApplyIntegerTerrainHeights)
                _terrainLevels.Ensure(worldData);

            _diagnostics.LogLevelMap("after EnsureTerrainLevelMap", worldData?.TerrainLevelMap);
            if (options.NormalizeTerrainLevelsForTileWorldCreator)
            {
                _diagnostics.LogLevelMap("before NormalizeTerrainLevelsForTileWorldCreator", worldData?.TerrainLevelMap);
                _terrainLevels.NormalizeForTileWorldCreator(worldData);
                _diagnostics.LogLevelMap("after NormalizeTerrainLevelsForTileWorldCreator", worldData?.TerrainLevelMap);
            }

            if (options.ExpandSandShoreBand)
                _shoreBand.Expand(worldData);

            if (options.ApplyIntegerTerrainHeights && options.NormalizeTerrainLevelsForTileWorldCreator)
            {
                _diagnostics.LogLevelMap("before post-shore NormalizeTerrainLevelsForTileWorldCreator", worldData?.TerrainLevelMap);
                _terrainLevels.NormalizeForTileWorldCreator(worldData);
                _diagnostics.LogLevelMap("after post-shore NormalizeTerrainLevelsForTileWorldCreator", worldData?.TerrainLevelMap);
            }
        }

        private void LogPositionSummaries(TileWorldCreatorLayerPositionSet positions, Configuration configuration)
        {
            _diagnostics.LogMappedLayerSummary("terrain", positions.TerrainPositions, positions.TerrainIds, configuration);
            _diagnostics.LogMappedLayerSummary("objects", positions.ObjectPositions, positions.ObjectIds, configuration);
            _diagnostics.LogMappedLayerSummary("buildings", positions.BuildingPositions, positions.BuildingIds, configuration);
        }

    }
}
