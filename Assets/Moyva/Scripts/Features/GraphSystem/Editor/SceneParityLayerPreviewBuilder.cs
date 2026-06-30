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
            out Dictionary<string, Color> layerPreviewColors,
            out int width,
            out int height,
            out string status)
        {
            matrices = new Dictionary<string, bool[,]>();
            layerPreviewColors = new Dictionary<string, Color>();
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
            var layerState = CaptureLayerState(graph);
            try
            {
                config = CreateTransientPreviewConfiguration(graph);

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

                TileWorldCreatorLayerOcclusionOptimizer.GenerateBlueprintMap(manager);
                var logicalMap = GraphLogicalTileMapBuilder.Build(
                    graph,
                    manager,
                    compiled,
                    safeMapSize.x,
                    safeMapSize.y);
                GraphLogicalTileMapDiagnostics.EmitAndCompare(
                    "Preview mask",
                    graph,
                    seed,
                    logicalMap);
                matrices = logicalMap.BuildLayerMatrices();
                BuildLayerPreviewColors(graph, manager, compiled, layerPreviewColors);

                status = matrices.Count > 0
                    ? "Final scene grid parity preview"
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
                EditorUtility.ClearProgressBar();
                RestoreLayerState(layerState);
                if (go != null)
                    Object.DestroyImmediate(go);
                if (config != null)
                    Object.DestroyImmediate(config);
            }
        }

        private static Configuration CreateTransientPreviewConfiguration(GraphAsset graph)
        {
            string graphName = string.IsNullOrWhiteSpace(graph?.name)
                ? "Graph"
                : SanitizeAssetName(graph.name);

            var config = ScriptableObject.CreateInstance<Configuration>();
            config.name = graphName + "_SceneParityPreview";
            config.hideFlags = HideFlags.HideAndDontSave;
            return config;
        }

        private static List<LayerState> CaptureLayerState(GraphAsset graph)
        {
            var result = new List<LayerState>();
            if (graph?.Layers == null)
                return result;

            foreach (var layer in graph.Layers)
            {
                if (layer == null)
                    continue;

                result.Add(new LayerState
                {
                    Layer = layer,
                    BlueprintLayerGuid = layer.BlueprintLayerGuid,
                    BuildLayerKey = layer.BuildLayerKey
                });
            }

            return result;
        }

        private static void RestoreLayerState(List<LayerState> states)
        {
            if (states == null)
                return;

            for (int i = 0; i < states.Count; i++)
            {
                var state = states[i];
                if (state.Layer == null)
                    continue;

                state.Layer.BlueprintLayerGuid = state.BlueprintLayerGuid;
                state.Layer.BuildLayerKey = state.BuildLayerKey;
            }
        }

        private static string SanitizeAssetName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "Graph";

            var invalid = System.IO.Path.GetInvalidFileNameChars();
            var chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (Array.IndexOf(invalid, chars[i]) >= 0)
                    chars[i] = '_';
            }

            return new string(chars);
        }

        private sealed class LayerState
        {
            public GeneratorLayerDefinition Layer;
            public string BlueprintLayerGuid;
            public string BuildLayerKey;
        }

        private static void BuildLayerPreviewColors(
            GraphAsset graph,
            TileWorldCreatorManager manager,
            IReadOnlyList<CompiledLayerMap> compiled,
            Dictionary<string, Color> layerPreviewColors)
        {
            if (graph == null || manager == null || compiled == null || layerPreviewColors == null)
                return;

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

                var buildLayer = FindTilesBuildLayer(manager.configuration, layerMap.BlueprintLayerGuid);
                if (buildLayer == null || !buildLayer.isEnabled)
                    continue;

                if (layerPreviewColors != null)
                    layerPreviewColors[layerMap.GraphLayerId] = ResolveBuildLayerPreviewColorSafe(buildLayer, layer.Color);
            }
        }

        private static TilesBuildLayer FindTilesBuildLayer(Configuration configuration, string blueprintLayerGuid)
        {
            if (configuration?.buildLayerFolders == null || string.IsNullOrWhiteSpace(blueprintLayerGuid))
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

                    if (string.Equals(buildLayer.assignedBlueprintLayerGuid, blueprintLayerGuid, StringComparison.Ordinal)
                        || string.Equals(buildLayer.currentBlueprintLayer?.guid, blueprintLayerGuid, StringComparison.Ordinal))
                    {
                        return buildLayer;
                    }
                }
            }

            return null;
        }

        private static Color ResolveBuildLayerPreviewColor(TilesBuildLayer buildLayer, Color fallback)
        {
            if (buildLayer == null)
                return fallback;

            if (buildLayer.generateFlatSurface
                && TryResolveMaterialColor(buildLayer.flatSurfaceMaterial, out var flatColor))
            {
                return flatColor;
            }

            if (TryResolvePresetListColor(buildLayer.tilePresetsTop, out var topColor)
                || TryResolvePresetListColor(buildLayer.tilePresetsMiddle, out topColor)
                || TryResolvePresetListColor(buildLayer.tilePresetsBottom, out topColor))
            {
                return topColor;
            }

            return fallback;
        }

        private static Color ResolveBuildLayerPreviewColorSafe(TilesBuildLayer buildLayer, Color fallback)
        {
            try
            {
                return ResolveBuildLayerPreviewColor(buildLayer, fallback);
            }
            catch (Exception)
            {
                fallback.a = Mathf.Approximately(fallback.a, 0f) ? 1f : fallback.a;
                return fallback;
            }
        }

        private static bool TryResolvePresetListColor(
            IReadOnlyList<TilesBuildLayer.TilePresetSelection> selections,
            out Color color)
        {
            color = default;
            if (selections == null || selections.Count == 0)
                return false;

            TilesBuildLayer.TilePresetSelection best = null;
            for (int i = 0; i < selections.Count; i++)
            {
                var selection = selections[i];
                if (selection?.preset == null)
                    continue;

                if (best == null || selection.weight > best.weight)
                    best = selection;
            }

            return best?.preset != null && TryResolvePresetColor(best.preset, out color);
        }

        private static bool TryResolvePresetColor(TilePreset preset, out Color color)
        {
            color = default;
            if (preset == null)
                return false;

            if (TryResolveMaterialColor(preset.GetMaterialOverride(), out color))
                return true;

            if (TryResolveTextureAverageColor(preset.previewThumbnail, out color))
                return true;

            var prefab = ResolveRepresentativePrefab(preset);
            if (prefab == null)
                return false;

            var renderer = prefab.GetComponentInChildren<Renderer>(true);
            if (renderer == null)
                return false;

            var materials = renderer.sharedMaterials;
            if (materials != null)
            {
                for (int i = 0; i < materials.Length; i++)
                {
                    if (TryResolveMaterialColor(materials[i], out color))
                        return true;
                }
            }

            return TryResolveMaterialColor(renderer.sharedMaterial, out color);
        }

        private static GameObject ResolveRepresentativePrefab(TilePreset preset)
        {
            if (preset == null)
                return null;

            return preset.gridtype == TilePreset.GridType.dual
                ? preset.DUALGRD_fillTile
                  ?? preset.DUALGRD_edgeTile
                  ?? preset.DUALGRD_cornerTile
                  ?? preset.DUALGRD_invertedCornerTile
                  ?? preset.DUALGRD_doubleInteriorCornerTile
                : preset.NRMGRD_fillTile
                  ?? preset.NRMGRD_singleTile
                  ?? preset.NRMGRD_fourWayTile
                  ?? preset.NRMGRD_edgeFillTile
                  ?? preset.NRMGRD_cornerFillTile
                  ?? preset.NRMGRD_interiorCornerTile
                  ?? preset.NRMGRD_deadEndTile;
        }

        private static bool TryResolveMaterialColor(Material material, out Color color)
        {
            color = default;
            if (material == null)
                return false;

            if (material.HasProperty("_BaseColor"))
            {
                color = material.GetColor("_BaseColor");
                color.a = 1f;
                return true;
            }

            if (material.HasProperty("_Color"))
            {
                color = material.GetColor("_Color");
                color.a = 1f;
                return true;
            }

            return false;
        }

        private static bool TryResolveTextureAverageColor(Texture2D texture, out Color color)
        {
            color = default;
            if (texture == null)
                return false;

            try
            {
                var pixels = texture.GetPixels32();
                if (pixels == null || pixels.Length == 0)
                    return false;

                double r = 0d;
                double g = 0d;
                double b = 0d;
                double weight = 0d;
                for (int i = 0; i < pixels.Length; i++)
                {
                    var pixel = pixels[i];
                    if (pixel.a <= 8)
                        continue;

                    double alpha = pixel.a / 255d;
                    r += pixel.r * alpha;
                    g += pixel.g * alpha;
                    b += pixel.b * alpha;
                    weight += alpha;
                }

                if (weight <= 0.0001d)
                    return false;

                color = new Color(
                    (float)(r / weight / 255d),
                    (float)(g / weight / 255d),
                    (float)(b / weight / 255d),
                    1f);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
