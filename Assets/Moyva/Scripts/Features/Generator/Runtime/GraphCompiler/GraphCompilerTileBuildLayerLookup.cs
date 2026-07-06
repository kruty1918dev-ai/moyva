using System;
using System.Collections.Generic;
using GiantGrey.TileWorldCreator;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphCompilerTileBuildLayerLookup
    {
        TilesBuildLayer Find(Configuration config, string buildLayerKey, string blueprintLayerGuid);
        string ResolveTileId(TilesBuildLayer buildLayer);
    }

    internal sealed class GraphCompilerTileBuildLayerLookup : IGraphCompilerTileBuildLayerLookup
    {
        public TilesBuildLayer Find(Configuration config, string buildLayerKey, string blueprintLayerGuid)
        {
            if (config?.buildLayerFolders == null)
                return null;

            foreach (var folder in config.buildLayerFolders)
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

        public string ResolveTileId(TilesBuildLayer buildLayer)
        {
            return ResolveTileId(buildLayer?.tilePresetsTop)
                   ?? ResolveTileId(buildLayer?.tilePresetsMiddle)
                   ?? ResolveTileId(buildLayer?.tilePresetsBottom);
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

        private static string ResolveTileId(List<TilesBuildLayer.TilePresetSelection> selections)
        {
            if (selections == null)
                return null;

            foreach (var selection in selections)
            {
                string tileId = selection?.preset?.tileId;
                if (!string.IsNullOrWhiteSpace(tileId))
                    return tileId.Trim();
            }

            return null;
        }
    }
}
