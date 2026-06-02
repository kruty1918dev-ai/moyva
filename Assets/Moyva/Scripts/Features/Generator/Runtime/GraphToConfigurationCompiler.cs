using System.Collections.Generic;
using System.Linq;
using System;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.Generator.Runtime.Nodes.Twc;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.GraphSystem.Runtime;
using Object = UnityEngine.Object;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Результат компіляції одного шару графа в blueprint-шар TileWorldCreator.
    /// Дозволяє зіставити gameplay-ідентифікатор шару графа з TWC-шаром.
    /// </summary>
    public sealed class CompiledLayerMap
    {
        public string GraphLayerId;
        public string BlueprintLayerGuid;
        public string LayerName;
        public int SortingOrder;
    }

    /// <summary>
    /// Компілює граф генератора у TileWorldCreator <see cref="Configuration"/>:
    /// кожен <see cref="GeneratorLayerDefinition"/> стає окремим blueprint-шаром,
    /// а вузли-обгортки <see cref="TwcModifierNode"/> його підграфа — стеком
    /// модифікаторів цього шару (у топологічному порядку).
    ///
    /// Граф є джерелом правди; саму генерацію виконує TileWorldCreator.
    /// </summary>
    public static class GraphToConfigurationCompiler
    {
        public static List<CompiledLayerMap> Compile(GraphAsset graph, TileWorldCreatorManager manager, int seed)
        {
            var result = new List<CompiledLayerMap>();
            if (graph == null || manager == null || manager.configuration == null)
                return result;

            var config = manager.configuration;

            // --- Розмір мапи ---
            var size = graph.SharedSettings != null ? graph.SharedSettings.MapSize : new Vector2Int(50, 50);
            config.width = size.x > 0 ? size.x : 50;
            config.height = size.y > 0 ? size.y : 50;

            // --- Seed (TWC/Unity.Mathematics не приймає 0) ---
            config.useGlobalRandomSeed = true;
            config.globalRandomSeed = seed == 0 ? 1 : seed;

            // --- Підготовка blueprint-шарів ---
            EnsureRootFolder(config);

            graph.EnsureDefaultLayer();

            var orderedLayers = graph.Layers
                .Where(l => l != null)
                .OrderBy(l => l.SortingOrder)
                .ToList();

            var existingLayers = GetAllBlueprintLayers(config);
            var usedLayerGuids = new HashSet<string>();
            var blueprintGuidByGraphLayerId = new Dictionary<string, string>();
            var blueprintByGraphLayerId = new Dictionary<string, BlueprintLayer>();

            var sortedNodes = TopologicalSorter.Sort(graph) ?? new List<NodeBase>(graph.Nodes);

            for (int layerIndex = 0; layerIndex < orderedLayers.Count; layerIndex++)
            {
                var layerDef = orderedLayers[layerIndex];
                var blueprint = FindLayerByName(existingLayers, layerDef.Name)
                                ?? manager.AddNewBlueprintLayer(layerDef.Name);
                if (blueprint == null)
                    continue;

                blueprint.layerName = layerDef.Name;
                blueprint.isEnabled = layerDef.Enabled;
                blueprint.layerColor = layerDef.Color;
                blueprint.defaultLayerHeight = layerDef.DefaultHeight;
                blueprint.tileMapModifiers = new List<BlueprintModifier>();
                usedLayerGuids.Add(blueprint.guid);
                blueprintGuidByGraphLayerId[layerDef.Id] = blueprint.guid;
                blueprintByGraphLayerId[layerDef.Id] = blueprint;

                result.Add(new CompiledLayerMap
                {
                    GraphLayerId = layerDef.Id,
                    BlueprintLayerGuid = blueprint.guid,
                    LayerName = blueprint.layerName,
                    SortingOrder = layerDef.SortingOrder
                });
            }

            ReorderBlueprintLayers(config, orderedLayers, blueprintByGraphLayerId);

            for (int layerIndex = 0; layerIndex < orderedLayers.Count; layerIndex++)
            {
                var layerDef = orderedLayers[layerIndex];
                if (!blueprintByGraphLayerId.TryGetValue(layerDef.Id, out var blueprint) || blueprint == null)
                    continue;

                bool includeUnassignedNodes = layerIndex == 0;
                AppendLayerModifiers(blueprint, layerDef.Id, sortedNodes, config, includeUnassignedNodes, blueprintGuidByGraphLayerId, graph);
            }

            DisableUnusedLayers(existingLayers, usedLayerGuids);

            return result;
        }

        private static void EnsureRootFolder(Configuration config)
        {
            if (config.blueprintLayerFolders == null)
                config.blueprintLayerFolders = new List<BlueprintLayerFolder>();

            if (config.blueprintLayerFolders.Count == 0)
                config.blueprintLayerFolders.Add(new BlueprintLayerFolder("Root"));
        }

        private static List<BlueprintLayer> GetAllBlueprintLayers(Configuration config)
        {
            var layers = new List<BlueprintLayer>();
            if (config?.blueprintLayerFolders == null)
                return layers;

            for (int i = 0; i < config.blueprintLayerFolders.Count; i++)
            {
                var folder = config.blueprintLayerFolders[i];
                if (folder?.blueprintLayers == null)
                    continue;

                for (int j = 0; j < folder.blueprintLayers.Count; j++)
                {
                    var layer = folder.blueprintLayers[j];
                    if (layer != null)
                        layers.Add(layer);
                }
            }

            return layers;
        }

        private static BlueprintLayer FindLayerByName(List<BlueprintLayer> layers, string layerName)
        {
            if (layers == null || string.IsNullOrWhiteSpace(layerName))
                return null;

            return layers.FirstOrDefault(layer =>
                layer != null &&
                string.Equals(layer.layerName, layerName, StringComparison.Ordinal));
        }

        private static void DisableUnusedLayers(List<BlueprintLayer> layers, HashSet<string> usedLayerGuids)
        {
            if (layers == null || usedLayerGuids == null)
                return;

            for (int i = 0; i < layers.Count; i++)
            {
                var layer = layers[i];
                if (layer == null)
                    continue;
                if (usedLayerGuids.Contains(layer.guid))
                    continue;

                layer.isEnabled = false;
                layer.tileMapModifiers ??= new List<BlueprintModifier>();
                layer.tileMapModifiers.Clear();
                layer.ClearLayer(false);
            }
        }

        private static void AppendLayerModifiers(
            BlueprintLayer blueprint,
            string layerId,
            List<NodeBase> sortedNodes,
            Configuration config,
            bool includeUnassignedNodes,
            Dictionary<string, string> blueprintGuidByGraphLayerId,
            GraphAsset graph)
        {
            var layerNodes = new List<NodeBase>();
            for (int i = 0; i < sortedNodes.Count; i++)
            {
                var node = sortedNodes[i];
                if (!BelongsToLayer(node, layerId, includeUnassignedNodes))
                    continue;

                layerNodes.Add(node);
            }

            for (int i = 0; i < layerNodes.Count; i++)
            {
                if (layerNodes[i] is not LayerMaskReferenceNode layerReferenceNode)
                    continue;

                var modifier = CreateLayerReferenceModifier(layerReferenceNode, blueprintGuidByGraphLayerId, graph, config);
                if (modifier != null)
                    blueprint.tileMapModifiers.Add(modifier);
            }

            for (int i = 0; i < layerNodes.Count; i++)
            {
                if (layerNodes[i] is LayerMaskReferenceNode)
                    continue;

                if (layerNodes[i] is not TwcModifierNode twcNode)
                {
                    continue;
                }

                var source = twcNode.Modifier;
                if (source == null)
                    continue;

                var clone = Object.Instantiate(source);
                clone.name = source.name;
                clone.hideFlags = HideFlags.HideInHierarchy;
                clone.isEnabled = true;
                clone.asset = config;
                blueprint.tileMapModifiers.Add(clone);
            }
        }

        private static bool BelongsToLayer(NodeBase node, string layerId, bool includeUnassignedNodes)
        {
            if (node == null)
                return false;

            if (!string.IsNullOrEmpty(node.LayerId))
                return node.LayerId == layerId;

            return includeUnassignedNodes;
        }

        private static MoyvaLayerReferenceBlueprintModifier CreateLayerReferenceModifier(
            LayerMaskReferenceNode node,
            Dictionary<string, string> blueprintGuidByGraphLayerId,
            GraphAsset graph,
            Configuration config)
        {
            if (node == null || string.IsNullOrEmpty(node.SourceLayerId))
                return null;

            if (blueprintGuidByGraphLayerId == null
                || !blueprintGuidByGraphLayerId.TryGetValue(node.SourceLayerId, out var sourceBlueprintGuid)
                || string.IsNullOrEmpty(sourceBlueprintGuid))
                return null;

            var sourceLayer = graph?.GetLayerById(node.SourceLayerId);
            var modifier = ScriptableObject.CreateInstance<MoyvaLayerReferenceBlueprintModifier>();
            modifier.name = "Moyva Layer Ref";
            modifier.hideFlags = HideFlags.HideInHierarchy;
            modifier.isEnabled = true;
            modifier.asset = config;
            modifier.sourceGraphLayerId = node.SourceLayerId;
            modifier.sourceBlueprintLayerGuid = sourceBlueprintGuid;
            modifier.sourceLayerName = sourceLayer?.Name;
            return modifier;
        }

        private static void ReorderBlueprintLayers(
            Configuration config,
            List<GeneratorLayerDefinition> orderedLayers,
            Dictionary<string, BlueprintLayer> blueprintByGraphLayerId)
        {
            if (config?.blueprintLayerFolders == null || config.blueprintLayerFolders.Count == 0)
                return;

            var root = config.blueprintLayerFolders[0];
            if (root == null)
                return;

            root.blueprintLayers ??= new List<BlueprintLayer>();

            var orderedBlueprints = new List<BlueprintLayer>();
            foreach (var layerDef in orderedLayers)
            {
                if (layerDef != null
                    && blueprintByGraphLayerId.TryGetValue(layerDef.Id, out var blueprint)
                    && blueprint != null
                    && !orderedBlueprints.Contains(blueprint))
                    orderedBlueprints.Add(blueprint);
            }

            var graphBlueprints = new HashSet<BlueprintLayer>(orderedBlueprints);
            var rootRemainder = new List<BlueprintLayer>();
            foreach (var existing in root.blueprintLayers)
            {
                if (existing != null && !graphBlueprints.Contains(existing))
                    rootRemainder.Add(existing);
            }

            for (int i = 0; i < config.blueprintLayerFolders.Count; i++)
            {
                var folder = config.blueprintLayerFolders[i];
                if (folder?.blueprintLayers == null)
                    continue;

                folder.blueprintLayers.RemoveAll(layer => layer != null && graphBlueprints.Contains(layer));
            }

            foreach (var existing in rootRemainder)
            {
                if (!orderedBlueprints.Contains(existing))
                    orderedBlueprints.Add(existing);
            }

            root.blueprintLayers = orderedBlueprints;
        }
    }
}
