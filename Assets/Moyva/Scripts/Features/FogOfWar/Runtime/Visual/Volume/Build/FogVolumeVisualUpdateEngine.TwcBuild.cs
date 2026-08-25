using System;
using System.Collections.Generic;
using System.Text;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed partial class FogVolumeVisualUpdateEngine
    {
        private void ExecuteTileWorldCreatorBuild(bool wasFullRebuild, int requestedDirtyTiles, string reason)
        {
            if (_manager == null || _runtimeConfiguration == null)
                return;

            bool isInitialBuild = !_hasBuiltAtLeastOnce;
            bool contextChanged = _worldContextChangedSinceBuild;
            int clustersBeforeBuild = CountGeneratedClusters();
            int layerObjectsBeforeBuild = CountLayerObjects();
            int generatedChildrenBeforeClear = CountGeneratedOutputChildren();
            bool stoppedPendingBuildCoroutines = StopPendingTileWorldBuildCoroutines(generatedChildrenBeforeClear, layerObjectsBeforeBuild);
            int clearedGeneratedChildren = ClearGeneratedOutputBeforeBuild();
            int generatedChildrenAfterClear = CountGeneratedOutputChildren();
            Debug.Log($"{StartupChainTag} FogVolume.TWCBuild BEFORE manager={_manager.name}, rebuild={(wasFullRebuild ? "full" : "dirty")}, reason={reason}, requestedDirty={requestedDirtyTiles}, runtimeLayers={_runtimeLayers.Count}, clusters={clustersBeforeBuild}, layerObjects={layerObjectsBeforeBuild}, generatedChildrenBeforeClear={generatedChildrenBeforeClear}, stoppedPendingBuildCoroutines={stoppedPendingBuildCoroutines}, clearedGeneratedChildren={clearedGeneratedChildren}, generatedChildrenAfterClear={generatedChildrenAfterClear}, config={_runtimeConfiguration.name}.");
            _manager.configuration = _runtimeConfiguration;
            ApplyFogBatchingBudget();
            _manager.ExecuteBuildLayers(ExecutionMode.FromScratch);
            _hasBuiltAtLeastOnce = true;
            _worldContextChangedSinceBuild = false;
            int clustersAfterBuild = CountGeneratedClusters();
            int layerObjectsAfterBuild = CountLayerObjects();
            Debug.Log($"{StartDiagTag} VolumeUpdater.ExecuteTileWorldCreatorBuild manager={_manager.name}, runtimeConfig={_runtimeConfiguration.name}, map={_mapWidth}x{_mapHeight}, rebuild={(wasFullRebuild ? "full" : "dirty")}, reason={reason}, dirtyRequested={requestedDirtyTiles}, runtimeLayers={_runtimeLayers.Count}, unexploredCells={_stateCache.UnexploredCellCount}, exploredCells={_stateCache.ExploredCellCount}.");
            Debug.Log($"{StartupChainTag} FogVolume.TWCBuild AFTER manager={_manager.name}, rebuild={(wasFullRebuild ? "full" : "dirty")}, reason={reason}, requestedDirty={requestedDirtyTiles}, runtimeLayers={_runtimeLayers.Count}, clustersBefore={clustersBeforeBuild}, clustersAfterImmediate={clustersAfterBuild}, layerObjectsBefore={layerObjectsBeforeBuild}, layerObjectsAfterImmediate={layerObjectsAfterBuild}, stoppedPendingBuildCoroutines={stoppedPendingBuildCoroutines}, clearedGeneratedChildren={clearedGeneratedChildren}.");
            Debug.Log($"{StartupChainTag} FogVolume.VisualBuildApplied visualFogDispersed={Mathf.Max(0, _mapWidth * _mapHeight - _stateCache.UnexploredCellCount - _stateCache.ExploredCellCount) > 0}, visibleCells={Mathf.Max(0, _mapWidth * _mapHeight - _stateCache.UnexploredCellCount - _stateCache.ExploredCellCount)}, exploredCells={_stateCache.ExploredCellCount}, unexploredCells={_stateCache.UnexploredCellCount}, runtimeLayers={_runtimeLayers.Count}, clustersAfter={clustersAfterBuild}, layerObjectsAfter={layerObjectsAfterBuild}, stoppedPendingBuildCoroutines={stoppedPendingBuildCoroutines}, clearedGeneratedChildren={clearedGeneratedChildren}, reason={reason}.");

            if (ShouldLogBuildSummary(contextChanged))
            {
                _loggedFirstBuild = true;
                Debug.Log(BuildSummaryLog(isInitialBuild, contextChanged, wasFullRebuild, requestedDirtyTiles));
            }
        }

        private void ApplyFogBatchingBudget()
        {
            if (_runtimeConfiguration == null)
                return;

            _runtimeConfiguration.mergeTiles = true;

            int activeLayerCount = 0;
            int maxLayerWidth = Mathf.Max(1, _runtimeConfiguration.width);
            int maxLayerHeight = Mathf.Max(1, _runtimeConfiguration.height);
            int mergeOverrideCount = 0;

            for (int i = 0; i < _runtimeLayers.Count; i++)
            {
                var runtimeLayer = _runtimeLayers[i];
                if (runtimeLayer?.BuildLayer == null || !runtimeLayer.BuildLayer.isEnabled)
                    continue;

                var cells = ResolveCells(runtimeLayer);
                if (cells == null || cells.Count == 0)
                    continue;

                activeLayerCount++;
                if (!runtimeLayer.BuildLayer.mergeTiles)
                {
                    runtimeLayer.BuildLayer.mergeTiles = true;
                    mergeOverrideCount++;
                }

                if (runtimeLayer.BlueprintLayer != null)
                {
                    maxLayerWidth = Mathf.Max(maxLayerWidth, _runtimeConfiguration.GetBlueprintLayerWidth(runtimeLayer.BlueprintLayer));
                    maxLayerHeight = Mathf.Max(maxLayerHeight, _runtimeConfiguration.GetBlueprintLayerHeight(runtimeLayer.BlueprintLayer));
                }
            }

            int safeLayerCount = Mathf.Max(1, activeLayerCount);
            int perLayerClusterBudget = Mathf.Max(1, TargetFogClusterBudget / safeLayerCount);
            int requestedClusterCellSize = ResolveClusterCellSizeForBudget(
                maxLayerWidth,
                maxLayerHeight,
                perLayerClusterBudget);
            requestedClusterCellSize = Mathf.Max(MinimumFogClusterCellSize, requestedClusterCellSize);

            bool clusterChanged = _runtimeConfiguration.clusterCellSize < requestedClusterCellSize;
            if (clusterChanged)
                _runtimeConfiguration.clusterCellSize = requestedClusterCellSize;

            if (clusterChanged || mergeOverrideCount > 0)
            {
                int estimatedClusters = EstimateClusterCount(maxLayerWidth, maxLayerHeight, _runtimeConfiguration.clusterCellSize) * safeLayerCount;
                Debug.Log(
                    $"{LogTag} Fog batching applied: mergeTiles={_runtimeConfiguration.mergeTiles}, " +
                    $"clusterCellSize={_runtimeConfiguration.clusterCellSize}, activeLayers={activeLayerCount}, " +
                    $"estimatedClusters={estimatedClusters}, target={TargetFogClusterBudget}, " +
                    $"layerMergeOverridesEnabled={mergeOverrideCount}.");
            }
        }

        private static int ResolveClusterCellSizeForBudget(int width, int height, int clusterBudget)
        {
            int safeWidth = Mathf.Max(1, width);
            int safeHeight = Mathf.Max(1, height);
            int safeBudget = Mathf.Max(1, clusterBudget);
            int maxSide = Mathf.Max(safeWidth, safeHeight);

            for (int cellSize = 1; cellSize <= maxSide; cellSize++)
            {
                if (EstimateClusterCount(safeWidth, safeHeight, cellSize) <= safeBudget)
                    return cellSize;
            }

            return maxSide;
        }

        private static int EstimateClusterCount(int width, int height, int clusterCellSize)
        {
            int safeClusterCellSize = Mathf.Max(1, clusterCellSize);
            return Mathf.CeilToInt(Mathf.Max(1, width) / (float)safeClusterCellSize)
                * Mathf.CeilToInt(Mathf.Max(1, height) / (float)safeClusterCellSize);
        }

        private bool ShouldLogBuildSummary(bool contextChanged)
        {
            if (_controller != null && !_controller.LogBuildSummary)
                return false;

            return !_loggedFirstBuild
                || contextChanged
                || (_controller != null && _controller.LogEveryVolumeUpdate);
        }

    }
}
