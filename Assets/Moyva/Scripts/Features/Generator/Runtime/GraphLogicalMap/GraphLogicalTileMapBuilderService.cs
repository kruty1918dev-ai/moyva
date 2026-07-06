using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class GraphLogicalTileMapBuilderService : IGraphLogicalTileMapBuilderService
    {
        private readonly IGraphLogicalTileMapTwcLookup _twcLookup;
        private readonly IGraphLogicalTileMapCellWriter _cellWriter;

        public GraphLogicalTileMapBuilderService(
            IGraphLogicalTileMapTwcLookup twcLookup,
            IGraphLogicalTileMapCellWriter cellWriter)
        {
            _twcLookup = twcLookup;
            _cellWriter = cellWriter;
        }

        public GraphLogicalTileMap Build(
            GraphAsset graph,
            TileWorldCreatorManager manager,
            IReadOnlyList<CompiledLayerMap> compiled,
            int width,
            int height)
        {
            var map = new GraphLogicalTileMap(width, height);
            if (graph == null || manager == null || compiled == null)
                return map;

            foreach (var layerMap in Order(compiled))
                ApplyLayer(graph, manager, map, layerMap);

            return map;
        }

        private void ApplyLayer(GraphAsset graph, TileWorldCreatorManager manager,
            GraphLogicalTileMap map, CompiledLayerMap layerMap)
        {
            if (!CanApplyLayer(graph, layerMap, out var graphLayer))
                return;

            var buildLayer = _twcLookup.FindTilesBuildLayer(manager.configuration, layerMap.BlueprintLayerGuid);
            if (buildLayer == null || !buildLayer.isEnabled)
                return;

            var blueprint = manager.GetBlueprintLayerByGuid(layerMap.BlueprintLayerGuid);
            if (blueprint == null)
                return;

            var data = CreateLayerData(layerMap, graphLayer.Name, blueprint.defaultLayerHeight,
                _twcLookup.ResolveSurfaceHeight(blueprint, buildLayer));
            if (buildLayer.generateFlatSurface)
            {
                _cellWriter.Fill(map, data);
                return;
            }

            if (blueprint.allPositions == null || blueprint.allPositions.Count == 0)
                return;

            foreach (var position in blueprint.allPositions)
            {
                int x = Mathf.RoundToInt(position.x);
                int y = Mathf.RoundToInt(position.y);
                if (x >= 0 && x < map.Width && y >= 0 && y < map.Height)
                    _cellWriter.Set(map, x, y, data);
            }
        }

        private static bool CanApplyLayer(GraphAsset graph, CompiledLayerMap layerMap,
            out GeneratorLayerDefinition graphLayer)
        {
            graphLayer = null;
            if (layerMap == null
                || !layerMap.HasRenderableTileOutput
                || string.IsNullOrEmpty(layerMap.GraphLayerId)
                || string.IsNullOrEmpty(layerMap.BlueprintLayerGuid))
                return false;

            graphLayer = graph.GetLayerById(layerMap.GraphLayerId);
            return graphLayer != null && graphLayer.Enabled;
        }

        private static GraphLogicalTileLayerData CreateLayerData(
            CompiledLayerMap layerMap,
            string graphLayerName,
            float layerHeight,
            float surfaceHeight)
        {
            string tileId = !string.IsNullOrWhiteSpace(layerMap.GridTileId)
                ? layerMap.GridTileId
                : layerMap.GraphLayerId;
            string layerName = !string.IsNullOrWhiteSpace(layerMap.LayerName)
                ? layerMap.LayerName
                : graphLayerName;
            return new GraphLogicalTileLayerData(layerMap.GraphLayerId, layerName, tileId, layerHeight, surfaceHeight);
        }

        private static List<CompiledLayerMap> Order(IReadOnlyList<CompiledLayerMap> compiled)
        {
            var ordered = new List<CompiledLayerMap>(compiled);
            ordered.Sort((a, b) => a.SortingOrder.CompareTo(b.SortingOrder));
            return ordered;
        }
    }
}
