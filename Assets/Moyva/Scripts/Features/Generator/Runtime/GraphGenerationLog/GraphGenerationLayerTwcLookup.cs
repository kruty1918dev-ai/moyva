using System;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphGenerationLayerTwcLookup
    {
        int CountGeneratedCells(TileWorldCreatorManager manager, CompiledLayerMap compiled);
        string ResolveBlueprintName(TileWorldCreatorManager manager, CompiledLayerMap compiled);
        string ResolveBuildLayerName(Configuration configuration, string buildLayerKey, string blueprintLayerGuid);
    }

    internal sealed class GraphGenerationLayerTwcLookup : IGraphGenerationLayerTwcLookup
    {
        public int CountGeneratedCells(TileWorldCreatorManager manager, CompiledLayerMap compiled)
        {
            if (manager == null || compiled == null || string.IsNullOrEmpty(compiled.BlueprintLayerGuid))
                return -1;

            var blueprint = manager.GetBlueprintLayerByGuid(compiled.BlueprintLayerGuid);
            return blueprint?.allPositions?.Count ?? 0;
        }

        public string ResolveBlueprintName(TileWorldCreatorManager manager, CompiledLayerMap compiled)
        {
            if (compiled == null)
                return null;

            if (manager != null && !string.IsNullOrEmpty(compiled.BlueprintLayerGuid))
            {
                var blueprint = manager.GetBlueprintLayerByGuid(compiled.BlueprintLayerGuid);
                if (!string.IsNullOrWhiteSpace(blueprint?.layerName))
                    return blueprint.layerName;
            }

            return compiled.LayerName;
        }

        public string ResolveBuildLayerName(Configuration configuration, string buildLayerKey, string blueprintLayerGuid)
        {
            return FindTilesBuildLayer(configuration, buildLayerKey, blueprintLayerGuid)?.layerName;
        }

        private static TilesBuildLayer FindTilesBuildLayer(Configuration configuration, string buildLayerKey, string blueprintLayerGuid)
        {
            if (configuration?.buildLayerFolders == null)
                return null;

            foreach (var folder in configuration.buildLayerFolders)
            {
                if (folder?.buildLayers == null)
                    continue;

                foreach (var layer in folder.buildLayers)
                {
                    if (layer is TilesBuildLayer buildLayer && Matches(buildLayer, buildLayerKey, blueprintLayerGuid))
                        return buildLayer;
                }
            }

            return null;
        }

        private static bool Matches(TilesBuildLayer layer, string buildLayerKey, string blueprintLayerGuid)
        {
            if (!string.IsNullOrWhiteSpace(buildLayerKey)
                && string.Equals(layer.guid, buildLayerKey, StringComparison.Ordinal))
                return true;

            return !string.IsNullOrWhiteSpace(blueprintLayerGuid)
                   && (string.Equals(layer.assignedBlueprintLayerGuid, blueprintLayerGuid, StringComparison.Ordinal)
                       || string.Equals(layer.currentBlueprintLayer?.guid, blueprintLayerGuid, StringComparison.Ordinal));
        }
    }
}
