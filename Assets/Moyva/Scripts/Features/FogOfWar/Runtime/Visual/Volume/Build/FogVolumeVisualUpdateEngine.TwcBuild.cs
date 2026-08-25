using GiantGrey.TileWorldCreator;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed partial class FogVolumeVisualUpdateEngine
    {
        private void ExecuteTileWorldCreatorBuild()
        {
            if (_manager == null || _runtimeConfiguration == null)
                return;

            StopPendingTileWorldBuildCoroutines();
            ClearGeneratedOutputBeforeBuild();
            _manager.configuration = _runtimeConfiguration;
            ApplyFogBatchingBudget();
            _manager.ExecuteBuildLayers(ExecutionMode.FromScratch);
            _hasBuiltAtLeastOnce = true;
            _worldContextChangedSinceBuild = false;
        }

        private void ApplyFogBatchingBudget()
        {
            if (_runtimeConfiguration == null)
                return;

            _runtimeConfiguration.mergeTiles = true;

            int activeLayerCount = 0;
            int maxLayerWidth = Mathf.Max(1, _runtimeConfiguration.width);
            int maxLayerHeight = Mathf.Max(1, _runtimeConfiguration.height);

            for (int i = 0; i < _runtimeLayers.Count; i++)
            {
                var runtimeLayer = _runtimeLayers[i];
                if (runtimeLayer?.BuildLayer == null || !runtimeLayer.BuildLayer.isEnabled)
                    continue;

                var cells = ResolveCells(runtimeLayer);
                if (cells == null || cells.Count == 0)
                    continue;

                activeLayerCount++;
                runtimeLayer.BuildLayer.mergeTiles = true;

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

            if (_runtimeConfiguration.clusterCellSize < requestedClusterCellSize)
                _runtimeConfiguration.clusterCellSize = requestedClusterCellSize;
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

    }
}
