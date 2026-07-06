using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITileWorldCreatorLayerPositionCollector
    {
        TileWorldCreatorLayerPositionSet Collect(GeneratedWorldData worldData, Configuration configuration);
    }

    internal sealed class TileWorldCreatorLayerPositionCollector : ITileWorldCreatorLayerPositionCollector
    {
        private readonly TileWorldCreatorIdMappingSO _mapping;
        private readonly TileWorldCreatorBuildOptions _options;
        private readonly ITileWorldCreatorBlueprintLayerResolver _resolver;

        public TileWorldCreatorLayerPositionCollector(
            ITileWorldCreatorBuildEnvironment environment,
            ITileWorldCreatorBlueprintLayerResolver resolver)
        {
            _mapping = environment.Mapping;
            _options = environment.Options;
            _resolver = resolver;
        }

        public TileWorldCreatorLayerPositionSet Collect(GeneratedWorldData worldData, Configuration configuration)
        {
            var set = new TileWorldCreatorLayerPositionSet();
            CollectMappedPositions(worldData.BiomeMap, _mapping.TryResolveTerrainLayer, configuration, set.TerrainPositions, set.TerrainIds);

            if (_options.SendObjectIdsToTileWorldCreator)
                CollectMappedPositions(worldData.ObjectMap, _mapping.TryResolveObjectLayer, configuration, set.ObjectPositions, set.ObjectIds);

            if (_options.SendBuildingIdsToTileWorldCreator)
                CollectMappedPositions(worldData.BuildingMap, _mapping.TryResolveBuildingLayer, configuration, set.BuildingPositions, set.BuildingIds);

            return set;
        }

        private void CollectMappedPositions(
            string[,] sourceMap,
            TryResolveLayer resolveLayer,
            Configuration configuration,
            Dictionary<string, HashSet<Vector2>> positionsByLayerGuid,
            HashSet<string> mappedIds)
        {
            if (sourceMap == null)
                return;

            int width = sourceMap.GetLength(0);
            int height = sourceMap.GetLength(1);
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                string id = sourceMap[x, y];
                if (string.IsNullOrWhiteSpace(id) || !resolveLayer(id, out var mapping))
                    continue;
                if (!_resolver.TryResolve(configuration, mapping, out string layerGuid))
                    continue;

                if (!positionsByLayerGuid.TryGetValue(layerGuid, out var positions))
                {
                    positions = new HashSet<Vector2>();
                    positionsByLayerGuid[layerGuid] = positions;
                }

                positions.Add(new Vector2(x, y));
                mappedIds.Add(id);
            }
        }

        private delegate bool TryResolveLayer(string id, out TileWorldCreatorIdMappingSO.LayerMapping mapping);
    }
}
