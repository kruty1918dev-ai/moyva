using System;
using System.Collections.Generic;
using System.Linq;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kruty1918.Moyva.Generator.Runtime.ObjectPlacement
{
    public static class TWCObjectPlacementAdapter
    {
        public const string GeneratedLayerNamePrefix = "[Moyva Objects] ";

        public static void Apply(
            Configuration config,
            TileWorldCreatorManager manager,
            IReadOnlyList<ObjectPlacementLayer> objectLayers,
            IReadOnlyList<CompiledLayerMap> terrainLayers = null)
        {
            if (config == null || manager == null)
                return;

            EnsureFolders(config);

            var activeGeneratedNames = new HashSet<string>(StringComparer.Ordinal);
            if (objectLayers != null)
            {
                for (int i = 0; i < objectLayers.Count; i++)
                {
                    var layer = objectLayers[i];
                    if (!CanApply(layer))
                        continue;

                    string generatedName = BuildGeneratedLayerName(layer.LayerName);
                    activeGeneratedNames.Add(generatedName);
                    ApplyLayer(config, manager, layer, generatedName, terrainLayers);
                }
            }

            RemoveStaleGeneratedLayers(config, activeGeneratedNames);
        }

        public static bool IsGeneratedObjectLayer(BuildLayer layer)
        {
            return layer is ObjectBuildLayer
                && !string.IsNullOrEmpty(layer.layerName)
                && layer.layerName.StartsWith(GeneratedLayerNamePrefix, StringComparison.Ordinal);
        }

        public static bool IsGeneratedBlueprintLayer(BlueprintLayer layer)
        {
            return layer != null
                && !string.IsNullOrEmpty(layer.layerName)
                && layer.layerName.StartsWith(GeneratedLayerNamePrefix, StringComparison.Ordinal);
        }

        public static string BuildGeneratedLayerName(string layerName)
        {
            string clean = string.IsNullOrWhiteSpace(layerName) ? "Object Layer" : layerName.Trim();
            return clean.StartsWith(GeneratedLayerNamePrefix, StringComparison.Ordinal)
                ? clean
                : GeneratedLayerNamePrefix + clean;
        }

        private static bool CanApply(ObjectPlacementLayer layer)
        {
            return layer != null
                && layer.Rule != null
                && layer.Rule.UseTWCObjectLayer
                && layer.Prefabs != null
                && layer.Prefabs.Any(p => p?.Prefab != null)
                && layer.Candidates != null
                && layer.Candidates.Count > 0;
        }

        private static void ApplyLayer(
            Configuration config,
            TileWorldCreatorManager manager,
            ObjectPlacementLayer source,
            string generatedName,
            IReadOnlyList<CompiledLayerMap> terrainLayers)
        {
            var blueprint = FindBlueprintLayer(config, generatedName)
                            ?? manager.AddNewBlueprintLayer(generatedName);
            if (blueprint == null)
                return;

            blueprint.layerName = generatedName;
            blueprint.isEnabled = true;
            blueprint.layerColor = new Color(0.52f, 0.84f, 0.32f, 1f);
            blueprint.defaultLayerHeight = ResolveTargetHeight(config, source.TargetGraphLayerId, terrainLayers);
            blueprint.useZeroLayerPadding = false;
            blueprint.borderPaddingCells = 0;
            blueprint.borderPaddingWidthCells = 0;
            blueprint.borderPaddingHeightCells = 0;
            blueprint.tileMapModifiers ??= new List<BlueprintModifier>();
            blueprint.tileMapModifiers.Clear();
            blueprint.ClearLayer(false);
            blueprint.AddCells(BuildCellPositions(source.Candidates, config.width, config.height));

            var buildLayer = FindObjectBuildLayer(config, generatedName)
                             ?? manager.AddNewBuildLayer<ObjectBuildLayer>(generatedName);
            if (buildLayer == null)
                return;

            ConfigureObjectBuildLayer(config, buildLayer, source, blueprint, generatedName);
            MarkDirty(blueprint);
            MarkDirty(buildLayer);
        }

        private static HashSet<Vector2> BuildCellPositions(
            IReadOnlyList<ScatterCandidate> candidates,
            int mapWidth,
            int mapHeight)
        {
            var positions = new HashSet<Vector2>();
            if (candidates == null)
                return positions;

            int w = Mathf.Max(1, mapWidth);
            int h = Mathf.Max(1, mapHeight);
            for (int i = 0; i < candidates.Count; i++)
            {
                var cell = candidates[i].Cell;
                if (cell.x < 0 || cell.y < 0 || cell.x >= w || cell.y >= h)
                    continue;

                positions.Add(new Vector2(cell.x, cell.y));
            }

            return positions;
        }

        private static void ConfigureObjectBuildLayer(
            Configuration config,
            ObjectBuildLayer buildLayer,
            ObjectPlacementLayer source,
            BlueprintLayer blueprint,
            string generatedName)
        {
            var rule = source.Rule ?? new ObjectPlacementRule();
            buildLayer.layerName = generatedName;
            buildLayer.isEnabled = true;
            buildLayer.assignedBlueprintLayerGuid = blueprint.guid;
            buildLayer.currentBlueprintLayer = blueprint;
            buildLayer.prefabObjects ??= new List<ObjectBuildLayer.PrefabObject>();
            buildLayer.prefabObjects.Clear();

            for (int i = 0; i < source.Prefabs.Count; i++)
            {
                var entry = source.Prefabs[i];
                if (entry?.Prefab == null)
                    continue;

                buildLayer.prefabObjects.Add(new ObjectBuildLayer.PrefabObject
                {
                    prefabObject = entry.Prefab,
                    weight = Mathf.Clamp(entry.Weight <= 0f ? 0.001f : entry.Weight, 0.001f, 1f)
                });
            }

            float cellSize = Mathf.Max(0.0001f, config.cellSize);
            buildLayer.objectRNDPositionOffsetRadius = Mathf.Max(0f, rule.Jitter) * cellSize;
            buildLayer.useRndRotation = rule.RotationRandomization > 0f || source.Prefabs.Any(p => p != null && p.RandomYaw);
            float yaw = Mathf.Max(0f, rule.RotationRandomization);
            buildLayer.objectRNDMinRotation = new Vector3(0f, -yaw, 0f);
            buildLayer.objectRNDMaxRotation = new Vector3(0f, yaw, 0f);

            ResolveScaleRange(source, rule, out float minScale, out float maxScale);
            buildLayer.useRndScale = Mathf.Abs(maxScale - minScale) > 0.0001f;
            buildLayer.uniformScale = true;
            buildLayer.uniformMinScale = minScale;
            buildLayer.uniformMaxScale = maxScale;
            buildLayer.objectRNDMinScale = Vector3.one * minScale;
            buildLayer.objectRNDMaxScale = Vector3.one * maxScale;

            buildLayer.layerOffset = new Vector3(0f, ResolveYOffset(source) - rule.EmbedIntoGround, 0f);
            buildLayer.meshGenerationOverride = true;
            buildLayer.mergeObjects = rule.MergeInTWC;
            buildLayer.shadowCastingMode = ShadowCastingMode.On;
            buildLayer.colliderType = Configuration.ColliderType.none;
            buildLayer.assignMeshCollider = false;
            buildLayer.tileColliderHeight = 0f;
            buildLayer.tileColliderExtrusionHeight = 0f;
            buildLayer.invertCollisionWalls = false;
        }

        private static void ResolveScaleRange(
            ObjectPlacementLayer source,
            ObjectPlacementRule rule,
            out float minScale,
            out float maxScale)
        {
            minScale = Mathf.Min(rule.ScaleRandomization.x, rule.ScaleRandomization.y);
            maxScale = Mathf.Max(rule.ScaleRandomization.x, rule.ScaleRandomization.y);

            if (source.Prefabs != null && source.Prefabs.Count > 0)
            {
                for (int i = 0; i < source.Prefabs.Count; i++)
                {
                    var entry = source.Prefabs[i];
                    if (entry == null)
                        continue;

                    minScale = Mathf.Min(minScale, entry.MinScale, entry.MaxScale);
                    maxScale = Mathf.Max(maxScale, entry.MinScale, entry.MaxScale);
                }
            }

            minScale = Mathf.Max(0.01f, minScale);
            maxScale = Mathf.Max(minScale, maxScale);
        }

        private static float ResolveYOffset(ObjectPlacementLayer source)
        {
            if (source?.Prefabs == null || source.Prefabs.Count == 0)
                return 0f;

            float sum = 0f;
            int count = 0;
            for (int i = 0; i < source.Prefabs.Count; i++)
            {
                if (source.Prefabs[i] == null)
                    continue;

                sum += source.Prefabs[i].YOffset;
                count++;
            }

            return count > 0 ? sum / count : 0f;
        }

        private static float ResolveTargetHeight(
            Configuration config,
            string targetGraphLayerId,
            IReadOnlyList<CompiledLayerMap> terrainLayers)
        {
            if (string.IsNullOrWhiteSpace(targetGraphLayerId)
                || terrainLayers == null
                || config?.blueprintLayerFolders == null)
                return 0f;

            for (int i = 0; i < terrainLayers.Count; i++)
            {
                var map = terrainLayers[i];
                if (map == null || map.GraphLayerId != targetGraphLayerId)
                    continue;

                var blueprint = config.GetBlueprintLayerByGuid(map.BlueprintLayerGuid);
                if (blueprint != null)
                    return blueprint.defaultLayerHeight;
            }

            return 0f;
        }

        private static void RemoveStaleGeneratedLayers(
            Configuration config,
            HashSet<string> activeGeneratedNames)
        {
            if (config == null)
                return;

            if (config.blueprintLayerFolders != null)
            {
                for (int i = 0; i < config.blueprintLayerFolders.Count; i++)
                {
                    var folder = config.blueprintLayerFolders[i];
                    if (folder?.blueprintLayers == null)
                        continue;

                    for (int l = folder.blueprintLayers.Count - 1; l >= 0; l--)
                    {
                        var layer = folder.blueprintLayers[l];
                        if (!IsGeneratedBlueprintLayer(layer)
                            || activeGeneratedNames.Contains(layer.layerName))
                            continue;

                        folder.blueprintLayers.RemoveAt(l);
                        RemoveSubAsset(layer);
                    }
                }
            }

            if (config.buildLayerFolders != null)
            {
                for (int i = 0; i < config.buildLayerFolders.Count; i++)
                {
                    var folder = config.buildLayerFolders[i];
                    if (folder?.buildLayers == null)
                        continue;

                    for (int l = folder.buildLayers.Count - 1; l >= 0; l--)
                    {
                        var layer = folder.buildLayers[l];
                        if (!IsGeneratedObjectLayer(layer)
                            || activeGeneratedNames.Contains(layer.layerName))
                            continue;

                        folder.buildLayers.RemoveAt(l);
                        RemoveSubAsset(layer);
                    }
                }
            }
        }

        private static void EnsureFolders(Configuration config)
        {
            config.blueprintLayerFolders ??= new List<BlueprintLayerFolder>();
            if (config.blueprintLayerFolders.Count == 0)
                config.blueprintLayerFolders.Add(new BlueprintLayerFolder("Root"));

            config.buildLayerFolders ??= new List<BuildLayerFolder>();
            if (config.buildLayerFolders.Count == 0)
                config.buildLayerFolders.Add(new BuildLayerFolder("Root"));
        }

        private static BlueprintLayer FindBlueprintLayer(Configuration config, string layerName)
        {
            if (config?.blueprintLayerFolders == null)
                return null;

            for (int i = 0; i < config.blueprintLayerFolders.Count; i++)
            {
                var folder = config.blueprintLayerFolders[i];
                if (folder?.blueprintLayers == null)
                    continue;

                for (int l = 0; l < folder.blueprintLayers.Count; l++)
                {
                    var layer = folder.blueprintLayers[l];
                    if (layer != null && layer.layerName == layerName)
                        return layer;
                }
            }

            return null;
        }

        private static ObjectBuildLayer FindObjectBuildLayer(Configuration config, string layerName)
        {
            if (config?.buildLayerFolders == null)
                return null;

            for (int i = 0; i < config.buildLayerFolders.Count; i++)
            {
                var folder = config.buildLayerFolders[i];
                if (folder?.buildLayers == null)
                    continue;

                for (int l = 0; l < folder.buildLayers.Count; l++)
                {
                    if (folder.buildLayers[l] is ObjectBuildLayer objectLayer
                        && objectLayer.layerName == layerName)
                        return objectLayer;
                }
            }

            return null;
        }

        private static void MarkDirty(UnityEngine.Object obj)
        {
#if UNITY_EDITOR
            if (obj != null && !Application.isPlaying)
                EditorUtility.SetDirty(obj);
#endif
        }

        private static void RemoveSubAsset(UnityEngine.Object obj)
        {
#if UNITY_EDITOR
            if (obj != null && !Application.isPlaying)
                AssetDatabase.RemoveObjectFromAsset(obj);
#endif
        }
    }
}
