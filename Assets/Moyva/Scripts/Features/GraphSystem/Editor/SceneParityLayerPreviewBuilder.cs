using System;
using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    /// <summary>
    /// Builds graph-editor layer preview through the same TWC compile/generate path
    /// used by scene generation. This keeps the final preview honest: helper/mask
    /// layers can feed other layers, but only renderable Tiles outputs become terrain.
    /// </summary>
    internal static class SceneParityLayerPreviewBuilder
    {
        public static bool TryBuildLayerMatrices(
            GraphAsset graph,
            int seed,
            Vector2Int mapSize,
            ISet<string> skippedLayerIds,
            out Dictionary<string, bool[,]> matrices,
            out int width,
            out int height,
            out string status)
        {
            matrices = new Dictionary<string, bool[,]>();
            width = Mathf.Max(1, mapSize.x);
            height = Mathf.Max(1, mapSize.y);
            status = null;

            if (graph == null)
            {
                status = "GraphAsset is null.";
                return false;
            }

            Configuration config = null;
            GameObject go = null;
            try
            {
                config = ScriptableObject.CreateInstance<Configuration>();
                config.name = graph.name + "_SceneParityPreview";
                config.hideFlags = HideFlags.HideAndDontSave;

                go = EditorUtility.CreateGameObjectWithHideFlags(
                    "__MoyvaSceneParityPreview",
                    HideFlags.HideAndDontSave,
                    typeof(TileWorldCreatorManager));

                var manager = go.GetComponent<TileWorldCreatorManager>();
                manager.configuration = config;

                var safeMapSize = new Vector2Int(width, height);
                var compiled = GraphToConfigurationCompiler.Compile(
                    graph,
                    manager,
                    seed,
                    skippedLayerIds,
                    safeMapSize);

                TileWorldCreatorLayerOcclusionOptimizer.GenerateCompleteMap(manager);
                BuildMatricesFromBlueprints(graph, manager, compiled, safeMapSize, matrices);

                status = matrices.Count > 0
                    ? "Scene parity preview"
                    : "Scene parity preview has no renderable tile layers.";
                return true;
            }
            catch (Exception ex)
            {
                status = ex.Message;
                return false;
            }
            finally
            {
                if (go != null)
                    Object.DestroyImmediate(go);
                if (config != null)
                    Object.DestroyImmediate(config);
            }
        }

        private static void BuildMatricesFromBlueprints(
            GraphAsset graph,
            TileWorldCreatorManager manager,
            IReadOnlyList<CompiledLayerMap> compiled,
            Vector2Int mapSize,
            Dictionary<string, bool[,]> matrices)
        {
            if (graph == null || manager == null || compiled == null || matrices == null)
                return;

            int width = Mathf.Max(1, mapSize.x);
            int height = Mathf.Max(1, mapSize.y);

            for (int i = 0; i < compiled.Count; i++)
            {
                var layerMap = compiled[i];
                if (layerMap == null
                    || !layerMap.HasRenderableTileOutput
                    || string.IsNullOrEmpty(layerMap.GraphLayerId)
                    || string.IsNullOrEmpty(layerMap.BlueprintLayerGuid))
                    continue;

                var layer = graph.GetLayerById(layerMap.GraphLayerId);
                if (layer == null || !layer.Enabled)
                    continue;

                var blueprint = manager.GetBlueprintLayerByGuid(layerMap.BlueprintLayerGuid);
                if (blueprint?.allPositions == null || blueprint.allPositions.Count == 0)
                    continue;

                var matrix = new bool[width, height];
                bool any = false;
                foreach (var position in blueprint.allPositions)
                {
                    int x = Mathf.RoundToInt(position.x);
                    int y = Mathf.RoundToInt(position.y);
                    if (x < 0 || x >= width || y < 0 || y >= height)
                        continue;

                    matrix[x, y] = true;
                    any = true;
                }

                if (any)
                    matrices[layerMap.GraphLayerId] = matrix;
            }
        }
    }
}
