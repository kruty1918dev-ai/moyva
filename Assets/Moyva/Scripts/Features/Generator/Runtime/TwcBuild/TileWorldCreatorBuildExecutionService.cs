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
        private const string WorldGenDiagTag = "[MoyvaWorldGenDiag]";
        private readonly TileWorldCreatorManager _manager;
        private readonly TileWorldCreatorBuildOptions _options;
        private readonly ITileWorldCreatorLayerPositionApplier _positionApplier;
        private readonly ITileWorldCreatorTerrainBuildPolicyService _terrainPolicyService;
        private readonly ITileWorldCreatorBuildDiagnosticsService _diagnostics;

        public TileWorldCreatorBuildExecutionService(
            ITileWorldCreatorBuildEnvironment environment,
            ITileWorldCreatorLayerPositionApplier positionApplier,
            ITileWorldCreatorTerrainBuildPolicyService terrainPolicyService,
            ITileWorldCreatorBuildDiagnosticsService diagnostics)
        {
            _manager = environment.Manager;
            _options = environment.Options;
            _positionApplier = positionApplier;
            _terrainPolicyService = terrainPolicyService;
            _diagnostics = diagnostics;
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
            ExecuteBuildLayers(configuration, positions.TerrainPositions.Count);
            ReportChunkAudit(configuration);
        }

        private void ResetIfNeeded()
        {
            if (!_options.ResetConfigurationBeforeBuild)
                return;
            _manager.ResetConfiguration();
        }

        private static void CullOccludedCells(Configuration configuration)
        {
            var occlusion = TileWorldCreatorLayerOcclusionOptimizer.CullOccludedTileCells(configuration);
            if (occlusion.RemovedCellCount > 0)
            {
            }
        }

        private void ApplyChunkAlignedBatching(Configuration configuration, TileWorldCreatorTerrainBuildPolicyResult terrainPolicy)
        {
            _terrainPolicyService.Apply(configuration, terrainPolicy, "runtime-bridge");
        }

        private void ExecuteBuildLayers(Configuration configuration, int terrainLayerCount)
        {

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _manager.ExecuteBuildLayers(ExecutionMode.FromScratch);
            stopwatch.Stop();
        }

        private void ReportChunkAudit(Configuration configuration)
        {
            var reporter = _manager.GetComponent<TileWorldCreatorChunkAuditReporter>();
            if (reporter == null)
                reporter = _manager.gameObject.AddComponent<TileWorldCreatorChunkAuditReporter>();

            reporter.hideFlags = HideFlags.HideInInspector | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            reporter.Report(_manager, configuration, "after-execute-return");
            reporter.RequestDelayedReport(_manager, configuration, "after-twc-coroutines");
        }
    }
}
