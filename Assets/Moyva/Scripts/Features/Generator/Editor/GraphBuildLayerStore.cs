using System.Collections.Generic;
using System.Linq;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.Generator.Editor
{
    /// <summary>
    /// Керує "компаньйон"-конфігурацією TileWorldCreator, що зберігається як під-асет
    /// усередині самого <see cref="GraphAsset"/>. Кожен шар графа отримує власний
    /// build-шар (<see cref="TilesBuildLayer"/>), у якому налаштовується візуал/тайли.
    ///
    /// Граф — джерело правди: blueprint-шари компілюються з графа, а build-шари
    /// синхронізуються 1:1 зі списком шарів графа.
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
        /// Перекомпільовує blueprint-шари з графа й синхронізує build-шари 1:1 зі шарами графа.
        /// Повертає актуальну конфігурацію (компаньйон графа).
        /// </summary>
        public static Configuration Sync(GraphAsset graph)
        {
            var config = GetCompanionConfiguration(graph, true);
            if (config == null)
                return null;

            // Тимчасовий менеджер потрібен лише для виклику API компіляції/створення шарів TWC.
            var go = EditorUtility.CreateGameObjectWithHideFlags(
                "__MoyvaBuildLayerSync", HideFlags.HideAndDontSave, typeof(TileWorldCreatorManager));
            var manager = go.GetComponent<TileWorldCreatorManager>();
            try
            {
                manager.configuration = config;

                // Компілюємо blueprint-шари (джерело — граф) у компаньйон-конфігурацію.
                var maps = GraphToConfigurationCompiler.Compile(graph, manager, 1);

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
                var map = maps?.FirstOrDefault(m => m != null && m.GraphLayerId == layerDef.Id);
                string blueprintGuid = map?.BlueprintLayerGuid;
                if (string.IsNullOrEmpty(blueprintGuid))
                    blueprintGuid = FindBlueprintGuidByLayerName(config, layerDef.Name);

                BuildLayer buildLayer = null;

                if (!string.IsNullOrEmpty(layerDef.BuildLayerKey))
                    buildLayer = folder.buildLayers.FirstOrDefault(
                        b => b != null && b.guid == layerDef.BuildLayerKey);

                if (buildLayer == null)
                    buildLayer = folder.buildLayers.FirstOrDefault(
                        b => b != null && b.layerName == layerDef.Name && !ordered.Contains(b));

                if (buildLayer == null)
                    buildLayer = manager.AddNewBuildLayer<TilesBuildLayer>(layerDef.Name);

                buildLayer.layerName = layerDef.Name;
                buildLayer.isEnabled = layerDef.Enabled;
                if (!string.IsNullOrEmpty(blueprintGuid))
                    buildLayer.assignedBlueprintLayerGuid = blueprintGuid;

                if (buildLayer is TilesBuildLayer tiles)
                {
                    tiles.configuration = config;
                    tiles.generateFlatSurface = layerDef.GenerateFlatSurface;
                    tiles.flatSurfaceMaterial = layerDef.FlatSurfaceMaterial;
                }

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
    }
}
