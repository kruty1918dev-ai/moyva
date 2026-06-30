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
        public static TileWorldCreatorLayerOcclusionResult GenerateCompleteMap(TileWorldCreatorManager manager)
        {
            if (manager == null || manager.configuration == null)
                return default;

            var result = GenerateBlueprintMap(manager);
            LogOcclusionResult(result, "GenerateCompleteMap");
            manager.ExecuteBuildLayers(ExecutionMode.FromScratch);
            manager.OnMapReady?.Invoke();
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
