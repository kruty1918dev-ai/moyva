using GiantGrey.TileWorldCreator;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class TileWorldCreatorBuildLayerLookup
    {
        public static TilesBuildLayer FindTilesBuildLayer(Configuration configuration, string blueprintLayerGuid)
        {
            if (configuration?.buildLayerFolders == null || string.IsNullOrWhiteSpace(blueprintLayerGuid))
                return null;

            for (int folderIndex = 0; folderIndex < configuration.buildLayerFolders.Count; folderIndex++)
            {
                var folder = configuration.buildLayerFolders[folderIndex];
                if (folder?.buildLayers == null)
                    continue;

                for (int layerIndex = 0; layerIndex < folder.buildLayers.Count; layerIndex++)
                {
                    if (folder.buildLayers[layerIndex] is not TilesBuildLayer buildLayer)
                        continue;

                    if (string.Equals(buildLayer.assignedBlueprintLayerGuid, blueprintLayerGuid, System.StringComparison.Ordinal)
                        || string.Equals(buildLayer.currentBlueprintLayer?.guid, blueprintLayerGuid, System.StringComparison.Ordinal))
                        return buildLayer;
                }
            }

            return null;
        }
    }
}
