using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITileWorldCreatorTerrainBaseHeightResolver
    {
        float ResolveTerrainBaseHeight(Configuration configuration);
        float ResolveMappedTerrainBaseHeight(Configuration configuration, TileWorldCreatorIdMappingSO.LayerMapping mapping);
    }

    internal sealed class TileWorldCreatorTerrainBaseHeightResolver : ITileWorldCreatorTerrainBaseHeightResolver
    {
        private readonly TileWorldCreatorIdMappingSO _mapping;
        private readonly ITileWorldCreatorBlueprintLayerResolver _resolver;

        public TileWorldCreatorTerrainBaseHeightResolver(
            ITileWorldCreatorBuildEnvironment environment,
            ITileWorldCreatorBlueprintLayerResolver resolver)
        {
            _mapping = environment.Mapping;
            _resolver = resolver;
        }

        public float ResolveTerrainBaseHeight(Configuration configuration)
        {
            if (configuration == null || _mapping?.TerrainLayers == null)
                return 0f;

            bool hasBaseHeight = false;
            float baseHeight = 0f;
            foreach (var terrainMapping in _mapping.TerrainLayers)
            {
                if (!_resolver.TryResolve(configuration, terrainMapping, out string layerGuid))
                    continue;

                var layer = configuration.GetBlueprintLayerByGuid(layerGuid);
                if (layer == null)
                    continue;

                baseHeight = hasBaseHeight ? Mathf.Min(baseHeight, layer.defaultLayerHeight) : layer.defaultLayerHeight;
                hasBaseHeight = true;
            }

            return hasBaseHeight ? baseHeight : 0f;
        }

        public float ResolveMappedTerrainBaseHeight(Configuration configuration, TileWorldCreatorIdMappingSO.LayerMapping mapping)
        {
            if (configuration == null || mapping == null)
                return 0f;
            if (!_resolver.TryResolve(configuration, mapping, out string blueprintLayerGuid))
                return 0f;

            var blueprint = configuration.GetBlueprintLayerByGuid(blueprintLayerGuid);
            var buildLayer = TileWorldCreatorBuildLayerLookup.FindTilesBuildLayer(configuration, blueprintLayerGuid);
            return ResolveTilesBuildLayerTopHeight(blueprint, buildLayer);
        }

        private static float ResolveTilesBuildLayerTopHeight(BlueprintLayer blueprint, TilesBuildLayer buildLayer)
        {
            float baseHeight = blueprint != null ? blueprint.defaultLayerHeight : 0f;
            if (buildLayer == null)
                return baseHeight;

            float layerBaseHeight = baseHeight + buildLayer.layerYOffset;
            if (buildLayer.tileLayers == null || buildLayer.tileLayers.Count == 0)
                return layerBaseHeight;

            bool hasTileLayer = false;
            float topHeight = layerBaseHeight;
            for (int i = 0; i < buildLayer.tileLayers.Count; i++)
            {
                var tileLayer = buildLayer.tileLayers[i];
                if (tileLayer == null)
                    continue;

                float candidate = layerBaseHeight + tileLayer.heightOffset;
                topHeight = hasTileLayer ? Mathf.Max(topHeight, candidate) : candidate;
                hasTileLayer = true;
            }

            return hasTileLayer ? topHeight : layerBaseHeight;
        }
    }
}
