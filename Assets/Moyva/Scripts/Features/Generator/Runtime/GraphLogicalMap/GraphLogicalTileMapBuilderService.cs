using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
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
            if (!CanApplyLayer(graph, layerMap, out var graphLayer, out var layerKind))
                return;

            var buildLayer = _twcLookup.FindTilesBuildLayer(manager.configuration, layerMap.BlueprintLayerGuid);
            if (layerKind != LayerKind.MaskOnly && (buildLayer == null || !buildLayer.isEnabled))
                return;

            var blueprint = manager.GetBlueprintLayerByGuid(layerMap.BlueprintLayerGuid);
            if (blueprint == null)
                return;

            var data = CreateLayerData(
                graph,
                layerMap,
                graphLayer.Name,
                blueprint.defaultLayerHeight,
                _twcLookup.ResolveSurfaceHeight(blueprint, buildLayer),
                buildLayer,
                layerKind);
            if (buildLayer != null && buildLayer.generateFlatSurface)
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
            out GeneratorLayerDefinition graphLayer,
            out LayerKind layerKind)
        {
            graphLayer = null;
            layerKind = LayerKind.BaseTerrain;
            if (layerMap == null
                || string.IsNullOrEmpty(layerMap.GraphLayerId)
                || string.IsNullOrEmpty(layerMap.BlueprintLayerGuid))
                return false;

            layerKind = ResolveLayerKind(graph, layerMap.GraphLayerId);
            if (!layerMap.HasRenderableTileOutput && layerKind != LayerKind.MaskOnly)
                return false;

            graphLayer = graph.GetLayerById(layerMap.GraphLayerId);
            return graphLayer != null && graphLayer.Enabled;
        }

        private static GraphLogicalTileLayerData CreateLayerData(
            GraphAsset graph,
            CompiledLayerMap layerMap,
            string graphLayerName,
            float layerHeight,
            float surfaceHeight,
            TilesBuildLayer buildLayer,
            LayerKind layerKind)
        {
            string tileId = !string.IsNullOrWhiteSpace(layerMap.GridTileId)
                ? layerMap.GridTileId
                : layerMap.GraphLayerId;
            string layerName = !string.IsNullOrWhiteSpace(layerMap.LayerName)
                ? layerMap.LayerName
                : graphLayerName;
            return new GraphLogicalTileLayerData(
                layerMap.GraphLayerId,
                layerName,
                tileId,
                layerHeight,
                surfaceHeight,
                layerMap.BlueprintLayerGuid,
                !string.IsNullOrWhiteSpace(layerMap.BuildLayerGuid) ? layerMap.BuildLayerGuid : buildLayer?.guid,
                ResolvePresetId(buildLayer, layerMap),
                layerKind,
                layerMap.SortingOrder,
                layerMap.GraphLayerOrder,
                layerMap.TerrainPriority,
                !string.IsNullOrWhiteSpace(layerMap.SourceNodeId)
                    ? layerMap.SourceNodeId
                    : TileSettingsNode.GetNodesForLayer(graph, layerMap.GraphLayerId).Find(node => node != null)?.NodeId);
        }

        private static string ResolvePresetId(TilesBuildLayer buildLayer, CompiledLayerMap layerMap)
        {
            if (!string.IsNullOrWhiteSpace(layerMap.PresetId))
                return layerMap.PresetId;

            var preset = buildLayer?.tilePresetsTop?.Find(selection => selection?.preset != null)?.preset
                         ?? buildLayer?.tilePresetsMiddle?.Find(selection => selection?.preset != null)?.preset
                         ?? buildLayer?.tilePresetsBottom?.Find(selection => selection?.preset != null)?.preset;
            if (preset == null)
                return null;

            return !string.IsNullOrWhiteSpace(preset.tileId) ? preset.tileId.Trim() : preset.name;
        }

        private static LayerKind ResolveLayerKind(GraphAsset graph, string graphLayerId)
        {
            var output = GraphLayerRuntimeSemantics.GetLayerOutputNode(graph, graphLayerId);
            if (output == null)
                return LayerKind.BaseTerrain;

            return output.OutputKind switch
            {
                LayerOutputKind.Objects => LayerKind.ObjectSpawn,
                LayerOutputKind.Masks => LayerKind.MaskOnly,
                _ => LayerKind.BaseTerrain
            };
        }

        private static List<CompiledLayerMap> Order(IReadOnlyList<CompiledLayerMap> compiled)
        {
            var ordered = new List<CompiledLayerMap>(compiled);
            ordered.Sort((a, b) => a.SortingOrder.CompareTo(b.SortingOrder));
            return ordered;
        }
    }
}
