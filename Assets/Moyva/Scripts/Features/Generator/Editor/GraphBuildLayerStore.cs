using System;
using System.Collections.Generic;
using System.Linq;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.GraphSystem.Runtime;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.Generator.Editor
{
    /// <summary>
    /// Керує "компаньйон"-конфігурацією TileWorldCreator, що зберігається як під-асет
    /// усередині самого <see cref="GraphAsset"/>. Кожен шар графа отримує власний
    /// blueprint-шар. TilesBuildLayer створюється тільки тоді, коли у graph layer є
    /// Tile Settings node або legacy flat-surface режим.
    ///
    /// Граф — джерело правди: blueprint-шари й tile build-шари синхронізуються
    /// з node-based налаштувань, а не з окремого TWC inspector-вікна.
    /// </summary>
    public static class GraphBuildLayerStore
    {
        /// <summary>
        /// Повертає (або створює) <see cref="Configuration"/>, прив'язану до графа як під-асет.
        /// </summary>
        public static Configuration GetCompanionConfiguration(GraphAsset graph, bool create)
        {
            if (graph == null)
                return null;

            string path = AssetDatabase.GetAssetPath(graph);
            if (string.IsNullOrEmpty(path))
                return null;

            var existing = AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<Configuration>()
                .FirstOrDefault();

            if (existing != null || !create)
                return existing;

            var config = ScriptableObject.CreateInstance<Configuration>();
            config.name = graph.name + "_TWC";
            config.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(config, graph);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path);
            return config;
        }

        /// <summary>
        /// Перекомпільовує blueprint-шари з графа й синхронізує tile build-шари з Tile Settings nodes.
        /// Повертає актуальну конфігурацію (компаньйон графа).
        /// </summary>
        public static Configuration Sync(GraphAsset graph)
        {
            var config = GetCompanionConfiguration(graph, true);
            if (config == null)
                return null;

            graph.EnsureLayerGraphStates();
            var validationReport = new GraphValidator().ValidateDetailed(graph);
            var globalErrors = validationReport.Issues
                .Where(issue => issue.Severity == ValidationSeverity.Error && string.IsNullOrEmpty(issue.LayerId))
                .ToList();
            var layerErrorIds = new HashSet<string>(validationReport.Issues
                .Where(issue => issue.Severity == ValidationSeverity.Error && !string.IsNullOrEmpty(issue.LayerId))
                .Select(issue => issue.LayerId));

            if (globalErrors.Count > 0)
            {
                Debug.LogWarning(
                    $"[GraphBuildLayerStore] Build layer sync skipped: {globalErrors.Count} global validation error(s).");
                return config;
            }

            if (layerErrorIds.Count > 0)
            {
                Debug.LogWarning(
                    $"[GraphBuildLayerStore] {layerErrorIds.Count} layer(s) have validation errors; Blueprint/Build layer names will still be synced so selectors stay truthful.");
            }

            // Тимчасовий менеджер потрібен лише для виклику API компіляції/створення шарів TWC.
            var go = EditorUtility.CreateGameObjectWithHideFlags(
                "__MoyvaBuildLayerSync", HideFlags.HideAndDontSave, typeof(TileWorldCreatorManager));
            var manager = go.GetComponent<TileWorldCreatorManager>();
            try
            {
                manager.configuration = config;

                // Компілюємо blueprint-шари (джерело — граф) у компаньйон-конфігурацію.
                var maps = GraphToConfigurationCompiler.Compile(graph, manager, 1);

                SyncBlueprintLayers(graph, config, maps);

                // Синхронізуємо build-шари зі шарами графа.
                SyncBuildLayers(graph, config, manager, maps);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }

            EditorUtility.SetDirty(config);
            EditorUtility.SetDirty(graph);
            AssetDatabase.SaveAssets();
            return config;
        }

        public static GraphValidationReport ValidateBlueprintLayerSync(GraphAsset graph, Configuration config)
        {
            var report = new GraphValidationReport();
            if (graph == null)
            {
                report.Add(new GraphValidationIssue(
                    "BLUEPRINT_SYNC_GRAPH_NULL",
                    ValidationSeverity.Error,
                    "GraphAsset не задано."));
                return report;
            }

            if (config == null)
            {
                report.Add(new GraphValidationIssue(
                    "BLUEPRINT_SYNC_CONFIG_NULL",
                    ValidationSeverity.Error,
                    "Companion TileWorldCreator Configuration відсутня або не може бути створена."));
                return report;
            }

            graph.EnsureLayerGraphStates();
            var orderedLayers = graph.Layers
                .Where(layer => layer != null)
                .OrderBy(layer => layer.SortingOrder)
                .ToList();
            var allBlueprints = GetAllBlueprintLayers(config);
            var graphBlueprintGuids = new HashSet<string>(StringComparer.Ordinal);

            foreach (var layer in orderedLayers)
            {
                if (string.IsNullOrWhiteSpace(layer.BlueprintLayerGuid))
                {
                    report.Add(new GraphValidationIssue(
                        "BLUEPRINT_LAYER_GUID_MISSING",
                        ValidationSeverity.Error,
                        $"Шар '{layer.Name}' ще не має BlueprintLayerGuid. Запустіть синхронізацію build-шарів.",
                        layerId: layer.Id,
                        canAutoFix: true));
                    continue;
                }

                if (!graphBlueprintGuids.Add(layer.BlueprintLayerGuid))
                {
                    report.Add(new GraphValidationIssue(
                        "BLUEPRINT_LAYER_GUID_DUPLICATE",
                        ValidationSeverity.Error,
                        $"BlueprintLayerGuid дублюється між graph layers: '{layer.BlueprintLayerGuid}'.",
                        layerId: layer.Id));
                    continue;
                }

                var blueprint = FindBlueprintByGuid(allBlueprints, layer.BlueprintLayerGuid);
                if (blueprint == null)
                {
                    report.Add(new GraphValidationIssue(
                        "BLUEPRINT_LAYER_REFERENCE_MISSING",
                        ValidationSeverity.Error,
                        $"Шар '{layer.Name}' посилається на відсутній BlueprintLayer '{layer.BlueprintLayerGuid}'. Запустіть sync або перестворіть companion config.",
                        layerId: layer.Id,
                        canAutoFix: true));
                    continue;
                }

                if (!string.Equals(blueprint.layerName, layer.Name, StringComparison.Ordinal))
                {
                    report.Add(new GraphValidationIssue(
                        "BLUEPRINT_LAYER_NAME_MISMATCH",
                        ValidationSeverity.Error,
                        $"BlueprintLayer '{blueprint.layerName}' не відповідає назві graph layer '{layer.Name}'. Запустіть sync.",
                        layerId: layer.Id,
                        canAutoFix: true));
                }
            }

            ValidateBlueprintOrder(config, orderedLayers, allBlueprints, report);
            ValidateStaleBlueprintLayers(allBlueprints, graphBlueprintGuids, report);
            return report;
        }

        private static void SyncBlueprintLayers(
            GraphAsset graph,
            Configuration config,
            IReadOnlyList<CompiledLayerMap> maps)
        {
            if (graph == null || config == null)
                return;

            EnsureBlueprintRootFolder(config);
            var root = config.blueprintLayerFolders[0];
            root.blueprintLayers ??= new List<BlueprintLayer>();

            var allBlueprints = GetAllBlueprintLayers(config);
            var orderedGraphBlueprints = new List<BlueprintLayer>();
            var graphBlueprintSet = new HashSet<BlueprintLayer>();

            var orderedLayers = graph.Layers
                .Where(layer => layer != null)
                .OrderBy(layer => layer.SortingOrder)
                .ToList();

            foreach (var layer in orderedLayers)
            {
                string blueprintGuid = maps?
                    .FirstOrDefault(map => map != null && map.GraphLayerId == layer.Id)
                    ?.BlueprintLayerGuid;
                if (string.IsNullOrWhiteSpace(blueprintGuid))
                    blueprintGuid = layer.BlueprintLayerGuid;

                var blueprint = FindBlueprintByGuid(allBlueprints, blueprintGuid);
                if (blueprint == null)
                    continue;

                if (graphBlueprintSet.Add(blueprint))
                    orderedGraphBlueprints.Add(blueprint);
            }

            var generatedBlueprints = allBlueprints
                .Where(layer => layer != null
                    && !graphBlueprintSet.Contains(layer)
                    && TWCObjectPlacementAdapter.IsGeneratedBlueprintLayer(layer))
                .Distinct()
                .ToList();

            var keptBlueprints = orderedGraphBlueprints
                .Concat(generatedBlueprints)
                .ToHashSet();

            for (int i = 0; i < config.blueprintLayerFolders.Count; i++)
            {
                var folder = config.blueprintLayerFolders[i];
                if (folder?.blueprintLayers == null)
                    continue;

                for (int layerIndex = folder.blueprintLayers.Count - 1; layerIndex >= 0; layerIndex--)
                {
                    var blueprint = folder.blueprintLayers[layerIndex];
                    folder.blueprintLayers.RemoveAt(layerIndex);
                    if (blueprint != null && !keptBlueprints.Contains(blueprint))
                        RemoveSubAsset(blueprint);
                }
            }

            root.blueprintLayers = orderedGraphBlueprints
                .Concat(generatedBlueprints)
                .Distinct()
                .ToList();
        }

        private static void SyncBuildLayers(
            GraphAsset graph,
            Configuration config,
            TileWorldCreatorManager manager,
            List<CompiledLayerMap> maps)
        {
            if (config.buildLayerFolders == null)
                config.buildLayerFolders = new List<BuildLayerFolder>();
            if (config.buildLayerFolders.Count == 0)
                config.buildLayerFolders.Add(new BuildLayerFolder("Root"));

            var folder = config.buildLayerFolders[0];
            if (folder.buildLayers == null)
                folder.buildLayers = new List<BuildLayer>();

            var orderedLayers = graph.Layers
                .Where(l => l != null)
                .OrderBy(l => l.SortingOrder)
                .ToList();

            var ordered = new List<BuildLayer>();

            foreach (var layerDef in orderedLayers)
            {
                var tileNodes = TileSettingsNode.GetNodesForLayer(graph, layerDef.Id);
                bool hasNodeTiles = GraphLayerRuntimeSemantics.HasRenderableTileOutput(graph, layerDef.Id);

                // TileSettingsNode is the source of truth for TWC TilesBuildLayer creation.
                // Data-only/helper layers intentionally do not create runtime tile GameObjects.
                if (!hasNodeTiles)
                {
                    layerDef.BuildLayerKey = string.Empty;
                    continue;
                }

                var map = maps?.FirstOrDefault(m => m != null && m.GraphLayerId == layerDef.Id);
                string blueprintGuid = map?.BlueprintLayerGuid;
                if (string.IsNullOrEmpty(blueprintGuid))
                    blueprintGuid = FindBlueprintGuidByLayerName(config, layerDef.Name);

                var blueprint = !string.IsNullOrEmpty(blueprintGuid)
                    ? config.GetBlueprintLayerByGuid(blueprintGuid)
                    : null;

                TilesBuildLayer buildLayer = null;

                if (!string.IsNullOrEmpty(layerDef.BuildLayerKey))
                    buildLayer = folder.buildLayers.OfType<TilesBuildLayer>().FirstOrDefault(
                        b => b != null && b.guid == layerDef.BuildLayerKey);

                if (buildLayer == null)
                    buildLayer = folder.buildLayers.OfType<TilesBuildLayer>().FirstOrDefault(
                        b => b != null && b.layerName == layerDef.Name && !ordered.Contains(b));

                if (buildLayer == null)
                    buildLayer = manager.AddNewBuildLayer<TilesBuildLayer>(layerDef.Name);

                TileSettingsNode.ApplyNodesToBuildLayer(buildLayer, tileNodes, config, blueprint, layerDef);

                layerDef.BuildLayerKey = buildLayer.guid;
                ordered.Add(buildLayer);
                EditorUtility.SetDirty(buildLayer);
            }

            foreach (var generatedObjectLayer in folder.buildLayers
                         .Where(TWCObjectPlacementAdapter.IsGeneratedObjectLayer)
                         .ToList())
            {
                if (!ordered.Contains(generatedObjectLayer))
                    ordered.Add(generatedObjectLayer);
            }

            // Прибираємо build-шари, для яких більше немає відповідного шару графа.
            foreach (var stale in folder.buildLayers.ToList())
            {
                if (stale == null || !ordered.Contains(stale))
                {
                    folder.buildLayers.Remove(stale);
                    if (stale != null)
                        AssetDatabase.RemoveObjectFromAsset(stale);
                }
            }

            folder.buildLayers = ordered;
        }

        private static void ValidateBlueprintOrder(
            Configuration config,
            IReadOnlyList<GeneratorLayerDefinition> orderedLayers,
            IReadOnlyList<BlueprintLayer> allBlueprints,
            GraphValidationReport report)
        {
            if (config?.blueprintLayerFolders == null || config.blueprintLayerFolders.Count == 0)
            {
                report.Add(new GraphValidationIssue(
                    "BLUEPRINT_LAYER_FOLDER_MISSING",
                    ValidationSeverity.Error,
                    "Companion Configuration не має BlueprintLayerFolders.",
                    canAutoFix: true));
                return;
            }

            var rootLayers = config.blueprintLayerFolders[0]?.blueprintLayers ?? new List<BlueprintLayer>();
            for (int i = 0; i < orderedLayers.Count; i++)
            {
                var layer = orderedLayers[i];
                var expected = FindBlueprintByGuid(allBlueprints, layer.BlueprintLayerGuid);
                if (expected == null)
                    continue;

                if (i >= rootLayers.Count || rootLayers[i] != expected)
                {
                    report.Add(new GraphValidationIssue(
                        "BLUEPRINT_LAYER_ORDER_MISMATCH",
                        ValidationSeverity.Error,
                        $"BlueprintLayer для '{layer.Name}' не стоїть на позиції {i} у companion config. Запустіть sync.",
                        layerId: layer.Id,
                        canAutoFix: true));
                }
            }
        }

        private static void ValidateStaleBlueprintLayers(
            IReadOnlyList<BlueprintLayer> allBlueprints,
            HashSet<string> graphBlueprintGuids,
            GraphValidationReport report)
        {
            if (allBlueprints == null)
                return;

            foreach (var blueprint in allBlueprints)
            {
                if (blueprint == null)
                    continue;
                if (TWCObjectPlacementAdapter.IsGeneratedBlueprintLayer(blueprint))
                    continue;
                if (graphBlueprintGuids.Contains(blueprint.guid))
                    continue;

                report.Add(new GraphValidationIssue(
                    "BLUEPRINT_LAYER_STALE",
                    ValidationSeverity.Error,
                    $"Companion Configuration містить зайвий BlueprintLayer '{blueprint.layerName}', якого немає у GraphAsset.Layers. Запустіть sync.",
                    canAutoFix: true));
            }
        }

        private static string FindBlueprintGuidByLayerName(Configuration config, string layerName)
        {
            if (config?.blueprintLayerFolders == null || string.IsNullOrWhiteSpace(layerName))
                return null;

            foreach (var folder in config.blueprintLayerFolders)
            {
                if (folder?.blueprintLayers == null)
                    continue;

                foreach (var blueprint in folder.blueprintLayers)
                {
                    if (blueprint == null)
                        continue;

                    if (blueprint.layerName == layerName)
                        return blueprint.guid;
                }
            }

            return null;
        }

        private static void EnsureBlueprintRootFolder(Configuration config)
        {
            config.blueprintLayerFolders ??= new List<BlueprintLayerFolder>();
            if (config.blueprintLayerFolders.Count == 0)
                config.blueprintLayerFolders.Add(new BlueprintLayerFolder("Root"));
        }

        private static List<BlueprintLayer> GetAllBlueprintLayers(Configuration config)
        {
            var layers = new List<BlueprintLayer>();
            if (config?.blueprintLayerFolders == null)
                return layers;

            foreach (var folder in config.blueprintLayerFolders)
            {
                if (folder?.blueprintLayers == null)
                    continue;

                foreach (var layer in folder.blueprintLayers)
                {
                    if (layer != null && !layers.Contains(layer))
                        layers.Add(layer);
                }
            }

            return layers;
        }

        private static BlueprintLayer FindBlueprintByGuid(
            IReadOnlyList<BlueprintLayer> layers,
            string blueprintLayerGuid)
        {
            if (layers == null || string.IsNullOrWhiteSpace(blueprintLayerGuid))
                return null;

            return layers.FirstOrDefault(layer =>
                layer != null &&
                string.Equals(layer.guid, blueprintLayerGuid, StringComparison.Ordinal));
        }

        private static void RemoveSubAsset(Object asset)
        {
            if (asset == null)
                return;

            if (AssetDatabase.Contains(asset))
                AssetDatabase.RemoveObjectFromAsset(asset);
            else
                Object.DestroyImmediate(asset, true);
        }
    }
}
