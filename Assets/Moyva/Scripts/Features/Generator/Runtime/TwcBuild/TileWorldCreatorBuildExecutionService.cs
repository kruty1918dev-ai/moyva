using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
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

            Debug.Log($"{LogTag} ResetConfiguration before build. Renderers before reset={_diagnostics.CountComponentsInManager<Renderer>()}, transforms before reset={_diagnostics.CountManagerChildren()}.");
            _manager.ResetConfiguration();
        }

        private static void CullOccludedCells(Configuration configuration)
        {
            var occlusion = TileWorldCreatorLayerOcclusionOptimizer.CullOccludedTileCells(configuration);
            if (occlusion.RemovedCellCount > 0)
            {
                Debug.Log($"{LogTag} Removed {occlusion.RemovedCellCount} occluded TWC tile cells across {occlusion.ProcessedLayerCount} build layers before spawning. occupied={occlusion.OccupiedCellCount}, skipped={occlusion.SkippedLayerCount}.");
            }
        }

        private void ApplyChunkAlignedBatching(Configuration configuration, TileWorldCreatorTerrainBuildPolicyResult terrainPolicy)
        {
            Debug.Log($"{LogTag} ApplyChunkAlignedBatching requested: chunkSizeTiles={terrainPolicy.ChunkSizeTiles}, forceMergeTiles={terrainPolicy.ForceMergeTiles}, applyIntegerHeights={_options.ApplyIntegerTerrainHeights}, mode={terrainPolicy.Mode}, configClusterSizeBefore={configuration.clusterCellSize}, configMergeTilesBefore={configuration.mergeTiles}.");
            _terrainPolicyService.Apply(configuration, terrainPolicy, "runtime-bridge");
            Debug.Log($"{LogTag} ApplyChunkAlignedBatching result: configClusterSizeAfter={configuration.clusterCellSize}, configMergeTilesAfter={configuration.mergeTiles}, layers={TileWorldCreatorChunkBatchingUtility.DescribeActiveTileLayers(configuration)}.");
        }

        private void ExecuteBuildLayers(Configuration configuration, int terrainLayerCount)
        {
            Debug.Log($"{LogTag} Calling ExecuteBuildLayers(FromScratch). Config size={configuration.width}x{configuration.height}, cellSize={configuration.cellSize}, mergeTiles={configuration.mergeTiles}, terrainLayers={terrainLayerCount}.");
            Debug.Log($"{WorldGenDiagTag} TWCBuild.START manager={_manager.name}, config={configuration.name}, map={configuration.width}x{configuration.height}, frame={Time.frameCount}, childrenBefore={_diagnostics.CountManagerChildren()}, asyncHint=coroutine/delayed");

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _manager.ExecuteBuildLayers(ExecutionMode.FromScratch);
            stopwatch.Stop();

            Debug.Log($"{LogTag} ExecuteBuildLayers returned. Immediate renderers={_diagnostics.CountComponentsInManager<Renderer>()}, meshFilters={_diagnostics.CountComponentsInManager<MeshFilter>()}, childTransforms={_diagnostics.CountManagerChildren()}.");
            Debug.Log($"{WorldGenDiagTag} TWCBuild.RETURN manager={_manager.name}, frame={Time.frameCount}, elapsedMs={stopwatch.ElapsedMilliseconds}, childrenAfterReturn={_diagnostics.CountManagerChildren()}, mayContinueAsync=true");
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
