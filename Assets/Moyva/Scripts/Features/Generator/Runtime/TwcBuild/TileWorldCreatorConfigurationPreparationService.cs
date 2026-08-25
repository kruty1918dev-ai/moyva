using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITileWorldCreatorConfigurationPreparationService
    {
        void Prepare(Configuration configuration, GeneratedWorldData worldData, TileWorldCreatorTerrainBuildPolicyResult terrainPolicy);
        void ConfigureTerrainHeightContext(GeneratedWorldData worldData, TileWorldCreatorTerrainBuildPolicyResult terrainPolicy);
    }

    internal sealed class TileWorldCreatorConfigurationPreparationService : ITileWorldCreatorConfigurationPreparationService
    {
        private const string LogTag = "[MoyvaTWCHeight]";
        private readonly TileWorldCreatorManager _manager;
        private readonly TileWorldCreatorBuildOptions _options;

        public TileWorldCreatorConfigurationPreparationService(ITileWorldCreatorBuildEnvironment environment)
        {
            _manager = environment.Manager;
            _options = environment.Options;
        }

        public void Prepare(Configuration configuration, GeneratedWorldData worldData, TileWorldCreatorTerrainBuildPolicyResult terrainPolicy)
        {

            if (_options.SyncConfigurationSize)
            {
                configuration.width = Mathf.Max(1, worldData.Width);
                configuration.height = Mathf.Max(1, worldData.Height);
            }

            if (_options.ConfigurationCellSizeOverride > 0f)
            {
                configuration.cellSize = _options.ConfigurationCellSizeOverride;
                configuration.lastCellSize = _options.ConfigurationCellSizeOverride;
            }

            if (_options.UseWorldSeed)
            {
                configuration.useGlobalRandomSeed = true;
                configuration.globalRandomSeed = NormalizeSeed(worldData.Seed);
            }

            if (configuration.currentRandomSeed == 0)
                configuration.currentRandomSeed = (uint)NormalizeSeed(worldData.Seed);

            if (terrainPolicy.UsesLegacyHeightProjection && configuration.mergeTiles)
            {
                configuration.mergeTiles = false;
            }
        }

        public void ConfigureTerrainHeightContext(GeneratedWorldData worldData, TileWorldCreatorTerrainBuildPolicyResult terrainPolicy)
        {
            var existing = _manager.GetComponent<MoyvaTerrainHeightContext>();
            if (!terrainPolicy.UsesPrecomputedHeights)
            {
                if (existing != null)
                    existing.Clear($"mode={terrainPolicy.Mode}");
                return;
            }

            var context = existing != null ? existing : _manager.gameObject.AddComponent<MoyvaTerrainHeightContext>();
            context.hideFlags = HideFlags.HideInInspector | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            context.Configure(worldData?.TerrainLevelMap, _options.TerrainHeightStep);
        }

        private static int NormalizeSeed(int seed) => seed == 0 ? 1 : seed;
    }
}
