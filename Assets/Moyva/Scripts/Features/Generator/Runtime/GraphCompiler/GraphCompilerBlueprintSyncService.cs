using System;
using System.Collections.Generic;
using System.Linq;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphCompilerBlueprintSyncService
    {
        GraphCompilerBlueprintSyncResult Sync(GraphAsset graph, Configuration config, ISet<string> skippedLayerIds);
        void DisableUnused(List<BlueprintLayer> layers, HashSet<string> usedLayerGuids);
    }

    internal sealed class GraphCompilerBlueprintSyncService : IGraphCompilerBlueprintSyncService
    {
        private readonly IGraphCompilerTileBuildLayerLookup _buildLayerLookup;

        public GraphCompilerBlueprintSyncService(IGraphCompilerTileBuildLayerLookup buildLayerLookup)
        {
            _buildLayerLookup = buildLayerLookup;
        }

        public GraphCompilerBlueprintSyncResult Sync(GraphAsset graph, Configuration config, ISet<string> skippedLayerIds)
        {
            var result = new GraphCompilerBlueprintSyncResult();
            result.ExistingLayers.AddRange(GetAllBlueprintLayers(config));
            result.OrderedLayers.AddRange(graph.Layers
                .Where(layer => layer != null)
                .Where(layer => skippedLayerIds == null || !skippedLayerIds.Contains(layer.Id))
                .OrderBy(layer => layer.SortingOrder));

            foreach (var layerDef in result.OrderedLayers)
                SyncLayer(graph, config, layerDef, result);

            GraphCompilerBlueprintOrderUtility.Reorder(config, result.OrderedLayers, result.BlueprintByGraphLayerId);
            return result;
        }

        public void DisableUnused(List<BlueprintLayer> layers, HashSet<string> usedLayerGuids)
        {
            if (layers == null || usedLayerGuids == null)
                return;

            foreach (var layer in layers)
            {
                if (layer == null || usedLayerGuids.Contains(layer.guid))
                    continue;

                layer.isEnabled = false;
                layer.tileMapModifiers ??= new List<BlueprintModifier>();
                layer.tileMapModifiers.Clear();
                layer.ClearLayer(false);
            }
        }

        private void SyncLayer(GraphAsset graph, Configuration config, GeneratorLayerDefinition layerDef,
            GraphCompilerBlueprintSyncResult result)
        {
            var blueprint = FindByGuid(result.ExistingLayers, layerDef.BlueprintLayerGuid)
                            ?? FindByName(result.ExistingLayers, layerDef.Name)
                            ?? CreateBlueprintLayer(config, layerDef.Name);
            if (blueprint == null)
                return;

            ApplyLayerDefinition(layerDef, blueprint);
            result.UsedLayerGuids.Add(blueprint.guid);
            result.BlueprintGuidByGraphLayerId[layerDef.Id] = blueprint.guid;
            result.BlueprintByGraphLayerId[layerDef.Id] = blueprint;
            result.CompiledLayers.Add(CreateCompiledMap(graph, config, layerDef, blueprint));
        }

        private CompiledLayerMap CreateCompiledMap(GraphAsset graph, Configuration config,
            GeneratorLayerDefinition layerDef, BlueprintLayer blueprint)
        {
            return new CompiledLayerMap
            {
                GraphLayerId = layerDef.Id,
                GridTileId = ResolveGridTileIdForLayer(graph, config, layerDef, blueprint.guid),
                BlueprintLayerGuid = blueprint.guid,
                LayerName = blueprint.layerName,
                SortingOrder = layerDef.SortingOrder,
                HasRenderableTileOutput = GraphLayerRuntimeSemantics.HasRenderableTileOutput(graph, layerDef.Id)
            };
        }

        private string ResolveGridTileIdForLayer(GraphAsset graph, Configuration config,
            GeneratorLayerDefinition layerDef, string blueprintLayerGuid)
        {
            if (!GraphLayerRuntimeSemantics.HasRenderableTileOutput(graph, layerDef.Id))
                return null;

            string nodeTileId = TileSettingsNode.ResolveFirstTileId(graph, layerDef);
            if (!string.IsNullOrWhiteSpace(nodeTileId))
                return nodeTileId.Trim();

            var buildLayer = _buildLayerLookup.Find(config, layerDef.BuildLayerKey, blueprintLayerGuid);
            if (buildLayer == null || buildLayer.generateFlatSurface)
                return layerDef.Id;

            return _buildLayerLookup.ResolveTileId(buildLayer) ?? layerDef.Id;
        }

        private static void ApplyLayerDefinition(GeneratorLayerDefinition layerDef, BlueprintLayer blueprint)
        {
            layerDef.BlueprintLayerGuid = blueprint.guid;
            blueprint.layerName = layerDef.Name;
            blueprint.isEnabled = layerDef.Enabled;
            blueprint.layerColor = layerDef.Color;
            blueprint.defaultLayerHeight = layerDef.DefaultHeight;
            blueprint.useZeroLayerPadding = layerDef.UseZeroLayerPadding;
            int zeroPadding = layerDef.UseZeroLayerPadding ? Configuration.ZeroLayerPaddingCells : 0;
            blueprint.borderPaddingWidthCells = Mathf.Max(zeroPadding, layerDef.ExtraWidthCells);
            blueprint.borderPaddingHeightCells = Mathf.Max(zeroPadding, layerDef.ExtraLengthCells);
            blueprint.borderPaddingCells = Mathf.Max(blueprint.borderPaddingWidthCells, blueprint.borderPaddingHeightCells);
            blueprint.tileMapModifiers = new List<BlueprintModifier>();
        }

        private static BlueprintLayer CreateBlueprintLayer(Configuration config, string layerName)
        {
            GraphCompilerLayerAssetUtility.EnsureBlueprintRootFolder(config);
            var layer = ScriptableObject.CreateInstance<BlueprintLayer>();
            GraphCompilerLayerAssetUtility.PrepareLayerAsset(config, layer, layerName);
            config.blueprintLayerFolders[0].blueprintLayers.Add(layer);
            return layer;
        }

        private static BlueprintLayer FindByName(IEnumerable<BlueprintLayer> layers, string layerName)
        {
            return layers.FirstOrDefault(layer => layer != null && string.Equals(layer.layerName, layerName, StringComparison.Ordinal));
        }

        private static BlueprintLayer FindByGuid(IEnumerable<BlueprintLayer> layers, string layerGuid)
        {
            return layers.FirstOrDefault(layer => layer != null && string.Equals(layer.guid, layerGuid, StringComparison.Ordinal));
        }

        private static List<BlueprintLayer> GetAllBlueprintLayers(Configuration config)
        {
            return config?.blueprintLayerFolders?
                .Where(folder => folder?.blueprintLayers != null)
                .SelectMany(folder => folder.blueprintLayers)
                .Where(layer => layer != null)
                .ToList() ?? new List<BlueprintLayer>();
        }
    }
}
