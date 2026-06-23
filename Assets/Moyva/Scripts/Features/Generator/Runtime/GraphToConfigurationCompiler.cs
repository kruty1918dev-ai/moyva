using System.Collections.Generic;
using System.Linq;
using System;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.Generator.Runtime.Nodes.ObjectPlacement;
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
        public string GridTileId;
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
        public static List<CompiledLayerMap> Compile(
            GraphAsset graph,
            TileWorldCreatorManager manager,
            int seed,
            ISet<string> skippedLayerIds = null)
        {
            var result = new List<CompiledLayerMap>();
            if (graph == null || manager == null || manager.configuration == null)
                return result;

            var config = manager.configuration;
            graph.EnsureLayerGraphStates();

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
                .Where(l => skippedLayerIds == null || !skippedLayerIds.Contains(l.Id))
                .OrderBy(l => l.SortingOrder)
                .ToList();

            var existingLayers = GetAllBlueprintLayers(config);
            var usedLayerGuids = new HashSet<string>();
            var blueprintGuidByGraphLayerId = new Dictionary<string, string>();
            var blueprintByGraphLayerId = new Dictionary<string, BlueprintLayer>();

            for (int layerIndex = 0; layerIndex < orderedLayers.Count; layerIndex++)
            {
                var layerDef = orderedLayers[layerIndex];
                var blueprint = FindLayerByGuid(existingLayers, layerDef.BlueprintLayerGuid)
                                ?? FindLayerByName(existingLayers, layerDef.Name)
                                ?? manager.AddNewBlueprintLayer(layerDef.Name);
                if (blueprint == null)
                    continue;

                layerDef.BlueprintLayerGuid = blueprint.guid;
                blueprint.layerName = layerDef.Name;
                blueprint.isEnabled = layerDef.Enabled;
                blueprint.layerColor = layerDef.Color;
                blueprint.defaultLayerHeight = layerDef.DefaultHeight;
                blueprint.useZeroLayerPadding = layerDef.UseZeroLayerPadding;
                int zeroLayerPadding = layerDef.UseZeroLayerPadding
                    ? Configuration.ZeroLayerPaddingCells
                    : 0;
                blueprint.borderPaddingWidthCells = Mathf.Max(zeroLayerPadding, layerDef.ExtraWidthCells);
                blueprint.borderPaddingHeightCells = Mathf.Max(zeroLayerPadding, layerDef.ExtraLengthCells);
                blueprint.borderPaddingCells = Mathf.Max(
                    blueprint.borderPaddingWidthCells,
                    blueprint.borderPaddingHeightCells);
                blueprint.tileMapModifiers = new List<BlueprintModifier>();
                usedLayerGuids.Add(blueprint.guid);
                blueprintGuidByGraphLayerId[layerDef.Id] = blueprint.guid;
                blueprintByGraphLayerId[layerDef.Id] = blueprint;

                result.Add(new CompiledLayerMap
                {
                    GraphLayerId = layerDef.Id,
                    GridTileId = ResolveGridTileIdForLayer(graph, config, layerDef, blueprint.guid),
                    BlueprintLayerGuid = blueprint.guid,
                    LayerName = blueprint.layerName,
                    SortingOrder = layerDef.SortingOrder
                });
            }

            ReorderBlueprintLayers(config, orderedLayers, blueprintByGraphLayerId);
            SyncTileBuildLayersFromGraph(graph, config, manager, orderedLayers, blueprintByGraphLayerId, skippedLayerIds);

            for (int layerIndex = 0; layerIndex < orderedLayers.Count; layerIndex++)
            {
                var layerDef = orderedLayers[layerIndex];
                if (!blueprintByGraphLayerId.TryGetValue(layerDef.Id, out var blueprint) || blueprint == null)
                    continue;

                var scope = graph.CreateExecutionScope(layerDef.Id);
                var executionPlan = TopologicalSorter.BuildPlan(scope);
                if (!executionPlan.Success)
                {
                    Debug.LogWarning(
                        $"[GraphToConfigurationCompiler] Layer '{layerDef.Name}' skipped while compiling TWC modifiers: {executionPlan.ErrorMessage}");
                    continue;
                }

                AppendLayerModifiers(blueprint, layerDef.Id, executionPlan.NodesInExecutionOrder, config, blueprintGuidByGraphLayerId, graph);
            }

            DisableUnusedLayers(existingLayers, usedLayerGuids);
            var objectLayers = CollectObjectPlacementLayers(graph, seed, new Vector2Int(config.width, config.height), skippedLayerIds);
            TWCObjectPlacementAdapter.Apply(config, manager, objectLayers, result);

            return result;
        }

        private static IReadOnlyList<ObjectPlacementLayer> CollectObjectPlacementLayers(
            GraphAsset graph,
            int seed,
            Vector2Int mapSize,
            ISet<string> skippedLayerIds = null)
        {
            if (graph == null || graph.Nodes == null)
                return Array.Empty<ObjectPlacementLayer>();

            graph.EnsureLayerGraphStates();
            bool hasObjectOutput = graph.Nodes.Any(node => node is ObjectOutputToTWCNode);
            if (!hasObjectOutput)
                return Array.Empty<ObjectPlacementLayer>();

            var clampedMapSize = new Vector2Int(Mathf.Max(1, mapSize.x), Mathf.Max(1, mapSize.y));
            var orderedLayers = graph.Layers
                .Where(layer => layer != null && layer.Enabled)
                .Where(layer => skippedLayerIds == null || !skippedLayerIds.Contains(layer.Id))
                .OrderBy(layer => layer.SortingOrder)
                .ToList();

            var runner = new GraphRunner();
            var result = new List<ObjectPlacementLayer>();
            var layerMaskRegistry = new LayerMaskRegistry();
            LayerMaskPrewarmUtility.PrewarmAllLayerMasks(
                graph,
                seed,
                clampedMapSize,
                layerMaskRegistry,
                null,
                skippedLayerIds);

            foreach (var layerDef in orderedLayers)
            {
                var scope = graph.CreateExecutionScope(layerDef.Id);
                bool collectsObjects = scope.Nodes.Any(node => node is ObjectOutputToTWCNode);

                var layerContext = new NodeContext(seed == 0 ? 1 : seed)
                {
                    MapSize = clampedMapSize
                };
                layerContext.ApplySharedSettings(graph.SharedSettings);
                if (graph.SharedSettings != null)
                    layerContext.RegisterService(graph.SharedSettings);
                layerContext.RegisterService(layerMaskRegistry);

                var registry = new ObjectPlacementRegistry();
                layerContext.RegisterService(registry);

                var execution = runner.Execute(scope, layerContext);
                if (execution == null || !execution.Success)
                {
                    Debug.LogWarning(
                        $"[MoyvaObjectPlacement] Layer '{layerDef.Name ?? scope.LayerId}' was not executed for object placement masks: {execution?.ErrorMessage ?? "unknown graph execution error"}");
                    continue;
                }

                if (!collectsObjects)
                    continue;

                if (registry.Layers.Count == 0)
                {
                    Debug.LogWarning(
                        $"[MoyvaObjectPlacement] Layer '{layerDef.Name ?? scope.LayerId}' has Object Output nodes, but no ObjectPlacementLayer was registered.");
                }

                foreach (var layer in registry.Layers)
                {
                    if (layer != null && string.IsNullOrWhiteSpace(layer.TargetGraphLayerId))
                        layer.TargetGraphLayerId = scope.LayerId;
                    if (layer != null)
                        result.Add(layer);
                }
            }

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

        private static BlueprintLayer FindLayerByGuid(List<BlueprintLayer> layers, string layerGuid)
        {
            if (layers == null || string.IsNullOrWhiteSpace(layerGuid))
                return null;

            return layers.FirstOrDefault(layer =>
                layer != null &&
                string.Equals(layer.guid, layerGuid, StringComparison.Ordinal));
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
            IReadOnlyList<NodeBase> sortedNodes,
            Configuration config,
            Dictionary<string, string> blueprintGuidByGraphLayerId,
            GraphAsset graph)
        {
            var layerNodes = new List<NodeBase>();
            for (int i = 0; i < sortedNodes.Count; i++)
            {
                var node = sortedNodes[i];
                if (!BelongsToLayer(node, layerId))
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

        private static bool BelongsToLayer(NodeBase node, string layerId)
        {
            if (node == null)
                return false;

            if (!string.IsNullOrEmpty(node.LayerId))
                return node.LayerId == layerId;

            return GraphAsset.IsGlobalNode(node);
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

        private static string ResolveGridTileIdForLayer(
            GraphAsset graph,
            Configuration config,
            GeneratorLayerDefinition layerDef,
            string blueprintLayerGuid)
        {
            if (layerDef == null || config == null)
                return null;

            string nodeTileId = TileSettingsNode.ResolveFirstTileId(graph, layerDef);
            if (!string.IsNullOrWhiteSpace(nodeTileId))
                return nodeTileId.Trim();

            TilesBuildLayer buildLayer = FindTilesBuildLayer(config, layerDef.BuildLayerKey, blueprintLayerGuid);
            if (buildLayer == null || buildLayer.generateFlatSurface)
                return layerDef.Id;

            string tileId = ResolveTileIdFromBuildLayer(buildLayer);
            return !string.IsNullOrWhiteSpace(tileId)
                ? tileId.Trim()
                : layerDef.Id;
        }

        private static void SyncTileBuildLayersFromGraph(
            GraphAsset graph,
            Configuration config,
            TileWorldCreatorManager manager,
            IReadOnlyList<GeneratorLayerDefinition> orderedLayers,
            Dictionary<string, BlueprintLayer> blueprintByGraphLayerId,
            ISet<string> skippedLayerIds)
        {
            if (graph == null || config == null || manager == null || orderedLayers == null)
                return;

            config.buildLayerFolders ??= new List<BuildLayerFolder>();
            if (config.buildLayerFolders.Count == 0)
                config.buildLayerFolders.Add(new BuildLayerFolder("Root"));

            var folder = config.buildLayerFolders[0];
            folder.buildLayers ??= new List<BuildLayer>();

            var orderedBuildLayers = new List<BuildLayer>();

            for (int i = 0; i < orderedLayers.Count; i++)
            {
                var layerDef = orderedLayers[i];
                if (layerDef == null)
                    continue;

                if (skippedLayerIds != null && skippedLayerIds.Contains(layerDef.Id))
                    continue;

                var tileNodes = TileSettingsNode.GetNodesForLayer(graph, layerDef.Id);
                bool hasNodeTiles = tileNodes.Any(node => node != null && node.HasRenderableTileOutput);

                // TileSettingsNode is the only source of truth for renderable TWC tile output.
                // Layers without TileSettingsNode are helper mask/data layers: keep their blueprint mask pipeline
                // available for Layer Ref/TWC modifiers, but do not create a TilesBuildLayer/runtime tile GameObject.
                if (!hasNodeTiles)
                {
                    layerDef.BuildLayerKey = string.Empty;
                    continue;
                }

                blueprintByGraphLayerId.TryGetValue(layerDef.Id, out var blueprint);
                string blueprintGuid = blueprint?.guid ?? layerDef.BlueprintLayerGuid;

                var buildLayer = FindTilesBuildLayer(config, layerDef.BuildLayerKey, blueprintGuid);
                if (buildLayer == null)
                {
                    buildLayer = folder.buildLayers
                        .OfType<TilesBuildLayer>()
                        .FirstOrDefault(layer => layer != null && layer.layerName == layerDef.Name && !orderedBuildLayers.Contains(layer));
                }

                if (buildLayer == null)
                    buildLayer = manager.AddNewBuildLayer<TilesBuildLayer>(layerDef.Name);

                TileSettingsNode.ApplyNodesToBuildLayer(buildLayer, tileNodes, config, blueprint, layerDef);

                if (!string.IsNullOrWhiteSpace(buildLayer.guid))
                    layerDef.BuildLayerKey = buildLayer.guid;

                if (!orderedBuildLayers.Contains(buildLayer))
                    orderedBuildLayers.Add(buildLayer);
            }

            PreserveGeneratedObjectLayers(folder, orderedBuildLayers);
            RemoveStaleGraphTileBuildLayers(folder, orderedBuildLayers);
            folder.buildLayers = orderedBuildLayers;
        }

        private static void PreserveGeneratedObjectLayers(BuildLayerFolder folder, List<BuildLayer> orderedBuildLayers)
        {
            if (folder?.buildLayers == null || orderedBuildLayers == null)
                return;

            foreach (var generatedObjectLayer in folder.buildLayers
                         .Where(TWCObjectPlacementAdapter.IsGeneratedObjectLayer)
                         .ToList())
            {
                if (generatedObjectLayer != null && !orderedBuildLayers.Contains(generatedObjectLayer))
                    orderedBuildLayers.Add(generatedObjectLayer);
            }
        }

        private static void RemoveStaleGraphTileBuildLayers(BuildLayerFolder folder, List<BuildLayer> orderedBuildLayers)
        {
            if (folder?.buildLayers == null || orderedBuildLayers == null)
                return;

            foreach (var stale in folder.buildLayers.ToList())
            {
                if (stale == null || orderedBuildLayers.Contains(stale))
                    continue;

                if (TWCObjectPlacementAdapter.IsGeneratedObjectLayer(stale))
                    continue;

                folder.buildLayers.Remove(stale);
#if UNITY_EDITOR
                if (UnityEditor.AssetDatabase.Contains(stale))
                    UnityEditor.AssetDatabase.RemoveObjectFromAsset(stale);
                else
                    Object.DestroyImmediate(stale);
#else
                Object.Destroy(stale);
#endif
            }
        }

        private static TilesBuildLayer FindTilesBuildLayer(
            Configuration configuration,
            string buildLayerKey,
            string blueprintLayerGuid)
        {
            if (configuration?.buildLayerFolders == null)
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

                    if (!string.IsNullOrWhiteSpace(buildLayerKey)
                        && string.Equals(buildLayer.guid, buildLayerKey, StringComparison.Ordinal))
                    {
                        return buildLayer;
                    }

                    if (!string.IsNullOrWhiteSpace(blueprintLayerGuid)
                        && (string.Equals(buildLayer.assignedBlueprintLayerGuid, blueprintLayerGuid, StringComparison.Ordinal)
                            || string.Equals(buildLayer.currentBlueprintLayer?.guid, blueprintLayerGuid, StringComparison.Ordinal)))
                    {
                        return buildLayer;
                    }
                }
            }

            return null;
        }

        private static string ResolveTileIdFromBuildLayer(TilesBuildLayer buildLayer)
        {
            string tileId = ResolveTileIdFromPresetSelections(buildLayer.tilePresetsTop);
            if (!string.IsNullOrWhiteSpace(tileId))
                return tileId;

            tileId = ResolveTileIdFromPresetSelections(buildLayer.tilePresetsMiddle);
            if (!string.IsNullOrWhiteSpace(tileId))
                return tileId;

            return ResolveTileIdFromPresetSelections(buildLayer.tilePresetsBottom);
        }

        private static string ResolveTileIdFromPresetSelections(List<TilesBuildLayer.TilePresetSelection> selections)
        {
            if (selections == null)
                return null;

            for (int i = 0; i < selections.Count; i++)
            {
                var preset = selections[i]?.preset;
                if (preset == null || string.IsNullOrWhiteSpace(preset.tileId))
                    continue;

                return preset.tileId;
            }

            return null;
        }
    }
}