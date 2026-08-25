using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class TileWorldCreatorLayerOcclusionOptimizer
    {
        private const string WorldGenDiagTag = "[MoyvaWorldGenDiag]";
        private const int TargetTileClusterBudget = 32;
        private const int MinimumClusterCellSize = 8;

        public static void GenerateCompleteMap(TileWorldCreatorManager manager, int chunkSizeTiles = 0)
        {
            GuardChunkFirstVisualBuild();
            if (manager == null || manager.configuration == null)
                return;

            GenerateBlueprintMap(manager);
            if (chunkSizeTiles > 0)
                TileWorldCreatorChunkBatchingUtility.Apply(manager.configuration, chunkSizeTiles, true, "graph-binding");
            else
                ApplyTileBatchingBudget(manager.configuration);
            manager.ExecuteBuildLayers(ExecutionMode.FromScratch);
            manager.OnMapReady?.Invoke();
        }

        private static void GuardChunkFirstVisualBuild()
        {
            if (!TileWorldCreatorChunkFirstGuard.IsActive)
                return;

            Debug.LogError($"{WorldGenDiagTag} ExecuteBuildLayers path reached through TileWorldCreatorLayerOcclusionOptimizer during chunk-first mode.");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            throw new System.InvalidOperationException("TWC visual build is forbidden during chunk-first generation.");
#endif
        }

        public static void GenerateBlueprintMap(TileWorldCreatorManager manager)
        {
            if (manager == null || manager.configuration == null)
                return;

            manager.ExecuteBlueprintLayers();
            CullOccludedTileCells(manager.configuration);
        }

        public static void CullOccludedTileCells(Configuration configuration)
        {
            var layers = GetBuildOrderedBlueprintLayers(configuration);
            if (layers.Count <= 1)
                return;

            var occupiedByHigherLayers = new HashSet<Vector2Int>();

            for (int i = layers.Count - 1; i >= 0; i--)
            {
                var layer = layers[i];
                if (layer?.allPositions == null || layer.allPositions.Count == 0)
                    continue;

                layer.allPositions.RemoveWhere(position => occupiedByHigherLayers.Contains(ToCellKey(position)));

                foreach (var position in layer.allPositions)
                    occupiedByHigherLayers.Add(ToCellKey(position));
            }

        }

        private static void ApplyTileBatchingBudget(Configuration configuration)
        {
            if (configuration == null)
                return;

            int activeTileLayerCount = 0;
            int maxLayerWidth = Mathf.Max(1, configuration.width);
            int maxLayerHeight = Mathf.Max(1, configuration.height);

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
                            buildLayer.mergeTiles = true;
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
