using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITileWorldCreatorBuildExecutionService
    {
        void Execute(Configuration configuration, TileWorldCreatorLayerPositionSet positions, TileWorldCreatorTerrainBuildPolicyResult terrainPolicy);
    }

    internal sealed class TileWorldCreatorBuildExecutionService : ITileWorldCreatorBuildExecutionService
    {
        private const string LogTag = "[MoyvaTWCHeight]";
        private readonly TileWorldCreatorManager _manager;
        private readonly TileWorldCreatorBuildOptions _options;
        private readonly ITileWorldCreatorLayerPositionApplier _positionApplier;
        private readonly ITileWorldCreatorTerrainBuildPolicyService _terrainPolicyService;

        public TileWorldCreatorBuildExecutionService(
            ITileWorldCreatorBuildEnvironment environment,
            ITileWorldCreatorLayerPositionApplier positionApplier,
            ITileWorldCreatorTerrainBuildPolicyService terrainPolicyService)
        {
            _manager = environment.Manager;
            _options = environment.Options;
            _positionApplier = positionApplier;
            _terrainPolicyService = terrainPolicyService;
        }

        public void Execute(Configuration configuration, TileWorldCreatorLayerPositionSet positions, TileWorldCreatorTerrainBuildPolicyResult terrainPolicy)
        {
            if (TileWorldCreatorChunkFirstGuard.IsActive)
            {
                Debug.LogError($"{LogTag} ExecuteBuildLayers path reached through TileWorldCreatorBuildExecutionService during chunk-first mode.");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                throw new System.InvalidOperationException("TWC visual build is forbidden during chunk-first generation.");
#endif
            }

            ResetIfNeeded();
            _positionApplier.Apply(positions.TerrainPositions);
            _positionApplier.Apply(positions.ObjectPositions);
            _positionApplier.Apply(positions.BuildingPositions);
            CullOccludedCells(configuration);
            ApplyChunkAlignedBatching(configuration, terrainPolicy);
            ExecuteBuildLayers();
        }

        private void ResetIfNeeded()
        {
            if (!_options.ResetConfigurationBeforeBuild)
                return;
            _manager.ResetConfiguration();
        }

        private static void CullOccludedCells(Configuration configuration)
        {
            TileWorldCreatorLayerOcclusionOptimizer.CullOccludedTileCells(configuration);
        }

        private void ApplyChunkAlignedBatching(Configuration configuration, TileWorldCreatorTerrainBuildPolicyResult terrainPolicy)
        {
            _terrainPolicyService.Apply(configuration, terrainPolicy, "runtime-bridge");
        }

        private void ExecuteBuildLayers()
        {
            _manager.ExecuteBuildLayers(ExecutionMode.FromScratch);
        }
    }
}
