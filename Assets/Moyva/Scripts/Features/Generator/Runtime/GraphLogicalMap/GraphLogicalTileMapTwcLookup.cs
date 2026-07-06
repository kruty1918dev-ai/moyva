using System;
using GiantGrey.TileWorldCreator;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class GraphLogicalTileMapTwcLookup : IGraphLogicalTileMapTwcLookup
    {
        public TilesBuildLayer FindTilesBuildLayer(Configuration configuration, string blueprintLayerGuid)
        {
            if (configuration?.buildLayerFolders == null || string.IsNullOrWhiteSpace(blueprintLayerGuid))
                return null;

            foreach (var folder in configuration.buildLayerFolders)
            {
                if (folder?.buildLayers == null)
                    continue;
                foreach (var layer in folder.buildLayers)
                    if (layer is TilesBuildLayer buildLayer && Matches(buildLayer, blueprintLayerGuid))
                        return buildLayer;
            }

            return null;
        }

        public float ResolveSurfaceHeight(BlueprintLayer blueprint, TilesBuildLayer buildLayer)
        {
            float baseHeight = blueprint != null ? blueprint.defaultLayerHeight : 0f;
            if (buildLayer == null)
                return baseHeight;

            float layerBaseHeight = baseHeight + buildLayer.layerYOffset;
            if (buildLayer.tileLayers == null || buildLayer.tileLayers.Count == 0)
                return layerBaseHeight;

            bool hasTileLayer = false;
            float topHeight = layerBaseHeight;
            foreach (var tileLayer in buildLayer.tileLayers)
            {
                if (tileLayer == null)
                    continue;
                float candidate = layerBaseHeight + tileLayer.heightOffset;
                topHeight = hasTileLayer ? Mathf.Max(topHeight, candidate) : candidate;
                hasTileLayer = true;
            }

            return hasTileLayer ? topHeight : layerBaseHeight;
        }

        private static bool Matches(TilesBuildLayer buildLayer, string blueprintLayerGuid)
        {
            return string.Equals(buildLayer.assignedBlueprintLayerGuid, blueprintLayerGuid, StringComparison.Ordinal)
                   || string.Equals(buildLayer.currentBlueprintLayer?.guid, blueprintLayerGuid, StringComparison.Ordinal);
        }
    }
}
