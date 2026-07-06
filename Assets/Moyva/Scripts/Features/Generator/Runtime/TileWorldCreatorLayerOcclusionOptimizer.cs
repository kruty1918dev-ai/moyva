using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal readonly struct TileWorldCreatorLayerOcclusionResult
    {
        public readonly int ProcessedLayerCount;
        public readonly int RemovedCellCount;
        public readonly int OccupiedCellCount;
        public readonly int SkippedLayerCount;

        public TileWorldCreatorLayerOcclusionResult(int processedLayerCount, int removedCellCount, int occupiedCellCount = 0, int skippedLayerCount = 0)
        {
            ProcessedLayerCount = processedLayerCount;
            RemovedCellCount = removedCellCount;
            OccupiedCellCount = occupiedCellCount;
            SkippedLayerCount = skippedLayerCount;
        }
    }

    internal static class TileWorldCreatorLayerOcclusionOptimizer
    {
        private const string WorldGenDiagTag = "[MoyvaWorldGenDiag]";
        private const int TargetTileClusterBudget = 32;
        private const int MinimumClusterCellSize = 8;

        public static TileWorldCreatorLayerOcclusionResult GenerateCompleteMap(TileWorldCreatorManager manager, int chunkSizeTiles = 0)
        {
            if (manager == null || manager.configuration == null)
                return default;

            int childrenBefore = manager.transform.childCount;
            Debug.Log(
                $"{WorldGenDiagTag} TWCBuild.START manager={manager.name}, config={manager.configuration.name}, " +
                $"map={manager.configuration.width}x{manager.configuration.height}, frame={Time.frameCount}, childrenBefore={childrenBefore}, asyncHint=unknown");
            var result = GenerateBlueprintMap(manager);
            LogOcclusionResult(result, "GenerateCompleteMap");
            if (chunkSizeTiles > 0)
                TileWorldCreatorChunkBatchingUtility.Apply(manager.configuration, chunkSizeTiles, true, "graph-binding");
            else
                ApplyTileBatchingBudget(manager.configuration);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            manager.ExecuteBuildLayers(ExecutionMode.FromScratch);
            stopwatch.Stop();
            manager.OnMapReady?.Invoke();
            Debug.Log(
                $"{WorldGenDiagTag} TWCBuild.RETURN manager={manager.name}, frame={Time.frameCount}, elapsedMs={stopwatch.ElapsedMilliseconds}, " +
                $"childrenAfterReturn={manager.transform.childCount}, mayContinueAsync=unknown");
            return result;
        }

        public static TileWorldCreatorLayerOcclusionResult GenerateBlueprintMap(TileWorldCreatorManager manager)
        {
            if (manager == null || manager.configuration == null)
                return default;

            manager.ExecuteBlueprintLayers();
            return CullOccludedTileCells(manager.configuration);
        }

        public static TileWorldCreatorLayerOcclusionResult CullOccludedTileCells(Configuration configuration)
        {
            var layers = GetBuildOrderedBlueprintLayers(configuration);
            if (layers.Count <= 1)
                return new TileWorldCreatorLayerOcclusionResult(layers.Count, 0);

            var occupiedByHigherLayers = new HashSet<Vector2Int>();
            int removedCount = 0;
            int skippedCount = 0;

            for (int i = layers.Count - 1; i >= 0; i--)
            {
                var layer = layers[i];
                if (layer?.allPositions == null || layer.allPositions.Count == 0)
                {
                    skippedCount++;
                    continue;
                }

                int before = layer.allPositions.Count;
                layer.allPositions.RemoveWhere(position => occupiedByHigherLayers.Contains(ToCellKey(position)));
                removedCount += before - layer.allPositions.Count;

                foreach (var position in layer.allPositions)
                    occupiedByHigherLayers.Add(ToCellKey(position));
            }

            return new TileWorldCreatorLayerOcclusionResult(layers.Count, removedCount, occupiedByHigherLayers.Count, skippedCount);
        }

        private static void ApplyTileBatchingBudget(Configuration configuration)
        {
            if (configuration == null)
                return;

            int activeTileLayerCount = 0;
            int maxLayerWidth = Mathf.Max(1, configuration.width);
            int maxLayerHeight = Mathf.Max(1, configuration.height);
            int mergeOverrideCount = 0;

            if (configuration.buildLayerFolders != null)
            {
                for (int folderIndex = 0; folderIndex < configuration.buildLayerFolders.Count; folderIndex++)
                {
                    var folder = configuration.buildLayerFolders[folderIndex];
                    if (folder?.buildLayers == null)
                        continue;

                    for (int layerIndex = 0; layerIndex < folder.buildLayers.Count; layerIndex++)
                    {
                        if (folder.buildLayers[layerIndex] is not TilesBuildLayer buildLayer || !buildLayer.isEnabled)
                            continue;

                        var blueprint = ResolveBlueprintLayer(configuration, buildLayer);
                        if (blueprint == null || !blueprint.isEnabled || blueprint.allPositions == null || blueprint.allPositions.Count == 0)
                            continue;

                        activeTileLayerCount++;
                        maxLayerWidth = Mathf.Max(maxLayerWidth, configuration.GetBlueprintLayerWidth(blueprint));
                        maxLayerHeight = Mathf.Max(maxLayerHeight, configuration.GetBlueprintLayerHeight(blueprint));

                        if (buildLayer.meshGenerationOverride && !buildLayer.mergeTiles)
                        {
                            buildLayer.mergeTiles = true;
                            mergeOverrideCount++;
                        }
                    }
                }
            }

            int safeLayerCount = Mathf.Max(1, activeTileLayerCount);
            int perLayerClusterBudget = Mathf.Max(1, TargetTileClusterBudget / safeLayerCount);
            int requestedClusterCellSize = ResolveClusterCellSizeForBudget(
                maxLayerWidth,
                maxLayerHeight,
                perLayerClusterBudget);
            requestedClusterCellSize = Mathf.Max(MinimumClusterCellSize, requestedClusterCellSize);

            bool mergeChanged = !configuration.mergeTiles;
            bool clusterChanged = configuration.clusterCellSize < requestedClusterCellSize;
            if (mergeChanged)
                configuration.mergeTiles = true;
            if (clusterChanged)
                configuration.clusterCellSize = requestedClusterCellSize;

            if (mergeChanged || clusterChanged || mergeOverrideCount > 0)
            {
                int estimatedClusters = EstimateClusterCount(maxLayerWidth, maxLayerHeight, configuration.clusterCellSize) * safeLayerCount;
                Debug.Log(
                    $"[Moyva TWC Batching] Tile batching applied: mergeTiles={configuration.mergeTiles}, " +
                    $"clusterCellSize={configuration.clusterCellSize}, activeTileLayers={activeTileLayerCount}, " +
                    $"estimatedTileClusters={estimatedClusters}, target={TargetTileClusterBudget}, " +
                    $"layerMergeOverridesEnabled={mergeOverrideCount}.");
            }
        }

        private static BlueprintLayer ResolveBlueprintLayer(Configuration configuration, TilesBuildLayer buildLayer)
        {
            if (configuration == null || buildLayer == null)
                return null;

            var blueprintGuid = !string.IsNullOrWhiteSpace(buildLayer.assignedBlueprintLayerGuid)
                ? buildLayer.assignedBlueprintLayerGuid
                : buildLayer.currentBlueprintLayer?.guid;
            if (string.IsNullOrWhiteSpace(blueprintGuid))
                return buildLayer.currentBlueprintLayer;

            return configuration.GetBlueprintLayerByGuid(blueprintGuid) ?? buildLayer.currentBlueprintLayer;
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

        private static Vector2Int ToCellKey(Vector2 position)
            => new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));

        private static void LogOcclusionResult(TileWorldCreatorLayerOcclusionResult result, string context)
        {
            if (result.RemovedCellCount <= 0)
                return;

            Debug.Log($"[Moyva TWC Occlusion] {context}: removed {result.RemovedCellCount} lower-layer cells, processed={result.ProcessedLayerCount}, occupied={result.OccupiedCellCount}, skipped={result.SkippedLayerCount}.");
        }

        private static List<BlueprintLayer> GetBuildOrderedBlueprintLayers(Configuration configuration)
        {
            var layers = new List<BlueprintLayer>();
            if (configuration?.buildLayerFolders == null)
                return layers;

            var seenBlueprintGuids = new HashSet<string>();
            for (int folderIndex = 0; folderIndex < configuration.buildLayerFolders.Count; folderIndex++)
            {
                var folder = configuration.buildLayerFolders[folderIndex];
                if (folder?.buildLayers == null)
                    continue;

                for (int buildLayerIndex = 0; buildLayerIndex < folder.buildLayers.Count; buildLayerIndex++)
                {
                    if (folder.buildLayers[buildLayerIndex] is not TilesBuildLayer buildLayer || !buildLayer.isEnabled)
                        continue;

                    var blueprintGuid = !string.IsNullOrWhiteSpace(buildLayer.assignedBlueprintLayerGuid)
                        ? buildLayer.assignedBlueprintLayerGuid
                        : buildLayer.currentBlueprintLayer?.guid;
                    if (string.IsNullOrWhiteSpace(blueprintGuid) || !seenBlueprintGuids.Add(blueprintGuid))
                        continue;

                    var blueprint = configuration.GetBlueprintLayerByGuid(blueprintGuid) ?? buildLayer.currentBlueprintLayer;
                    if (blueprint == null || !blueprint.isEnabled)
                        continue;

                    layers.Add(blueprint);
                }
            }

            return layers;
        }
    }
}
