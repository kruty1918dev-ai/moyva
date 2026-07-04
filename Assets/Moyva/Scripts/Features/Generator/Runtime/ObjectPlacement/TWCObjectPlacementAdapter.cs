using System;
using System.Collections.Generic;
using System.Linq;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.Generator.Runtime.Noise;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Kruty1918.Moyva.Generator.Runtime.ObjectPlacement
{
    public static class TWCObjectPlacementAdapter
    {
        public const string GeneratedLayerNamePrefix = "[Moyva Objects] ";
        public const string DirectObjectsRootName = "[Moyva Graph Objects]";

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
            var activeDirectNames = new HashSet<string>(StringComparer.Ordinal);
            var validLayers = new List<(ObjectPlacementLayer Layer, string BaseName)>();
            if (objectLayers != null)
            {
                for (int i = 0; i < objectLayers.Count; i++)
                {
                    var layer = objectLayers[i];
                    if (!CanApply(layer, out string reason))
                    {
                        Debug.LogWarning($"[MoyvaObjectPlacement] Skipped object layer '{layer?.LayerName ?? "<null>"}': {reason}");
                        continue;
                    }

                    validLayers.Add((layer, BuildGeneratedLayerName(layer.LayerName)));
                }
            }

            var duplicateBaseNames = validLayers
                .GroupBy(item => item.BaseName, StringComparer.Ordinal)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToHashSet(StringComparer.Ordinal);

            for (int i = 0; i < validLayers.Count; i++)
            {
                var layer = validLayers[i].Layer;
                string generatedName = duplicateBaseNames.Contains(validLayers[i].BaseName)
                    ? BuildGeneratedLayerName(layer, terrainLayers, includeLayerSuffix: true)
                    : validLayers[i].BaseName;

                activeGeneratedNames.Add(generatedName);
                ApplyLayer(config, manager, layer, generatedName, terrainLayers);
            }

            RemoveStaleGeneratedLayers(config, activeGeneratedNames);
            RemoveStaleDirectLayers(manager, activeDirectNames);
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

        private static string BuildGeneratedLayerName(
            ObjectPlacementLayer layer,
            IReadOnlyList<CompiledLayerMap> terrainLayers,
            bool includeLayerSuffix)
        {
            string baseName = BuildGeneratedLayerName(layer?.LayerName);
            if (!includeLayerSuffix)
                return baseName;

            string suffix = ResolveGraphLayerLabel(layer?.TargetGraphLayerId, terrainLayers);
            return string.IsNullOrWhiteSpace(suffix)
                ? baseName
                : $"{baseName} @ {suffix}";
        }

        private static string ResolveGraphLayerLabel(
            string graphLayerId,
            IReadOnlyList<CompiledLayerMap> terrainLayers)
        {
            if (string.IsNullOrWhiteSpace(graphLayerId))
                return null;

            if (terrainLayers != null)
            {
                for (int i = 0; i < terrainLayers.Count; i++)
                {
                    var map = terrainLayers[i];
                    if (map == null || map.GraphLayerId != graphLayerId)
                        continue;

                    if (!string.IsNullOrWhiteSpace(map.LayerName))
                        return SanitizeNameSuffix(map.LayerName);
                }
            }

            string compactId = graphLayerId.Replace("-", string.Empty);
            return compactId.Length <= 8
                ? compactId
                : compactId.Substring(0, 8);
        }

        private static string SanitizeNameSuffix(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var invalid = System.IO.Path.GetInvalidFileNameChars();
            var chars = value.Trim().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (Array.IndexOf(invalid, chars[i]) >= 0)
                    chars[i] = '_';
            }

            return new string(chars);
        }

        private static bool CanApply(ObjectPlacementLayer layer, out string reason)
        {
            if (layer == null)
            {
                reason = "layer is null";
                return false;
            }

            if (layer.Rule == null)
            {
                reason = "placement rule is null";
                return false;
            }

            if (!layer.Rule.UseTWCObjectLayer)
            {
                reason = "UseTWCObjectLayer is disabled";
                return false;
            }

            if (layer.Prefabs == null || !layer.Prefabs.Any(p => p?.Prefab != null))
            {
                reason = "no prefab variants are assigned";
                return false;
            }

            if (layer.Candidates == null || layer.Candidates.Count == 0)
            {
                reason = "scatter produced 0 candidates";
                return false;
            }

            reason = null;
            return true;
        }

        private static bool ShouldSpawnDirectly(
            ObjectPlacementLayer layer,
            IReadOnlyList<CompiledLayerMap> terrainLayers)
        {
            // Legacy standalone adapter calls expect generated TWC layers even when MergeInTWC is false.
            // Direct spawning is used only during compiled graph flows where terrain-layer context exists.
            if (terrainLayers == null || terrainLayers.Count == 0)
                return false;

            return RequiresExactGraphSpawn(layer);
        }

        private static bool RequiresExactGraphSpawn(ObjectPlacementLayer layer)
        {
            var rule = layer.Rule;

            if (layer.Prefabs != null)
            {
                int validPrefabCount = 0;
                for (int i = 0; i < layer.Prefabs.Count; i++)
                {
                    var entry = layer.Prefabs[i];
                    if (entry?.Prefab == null)
                        continue;

                    validPrefabCount++;
                    if (entry.MaterialOverride != null
                        || Mathf.Abs(entry.YOffset) > 0.0001f
                        || HasCustomScaleRange(entry.MinScale, entry.MaxScale))
                    {
                        return true;
                    }
                }

                if (validPrefabCount > 1)
                    return true;
            }

            if (layer.Candidates == null)
                return false;

            for (int i = 0; i < layer.Candidates.Count; i++)
            {
                var candidate = layer.Candidates[i];
                if (candidate.PrefabIndex > 0
                    || Mathf.Abs(candidate.LocalOffset.x) > 0.0001f
                    || Mathf.Abs(candidate.LocalOffset.y) > 0.0001f
                    || Mathf.Abs(candidate.RotationY) > 0.0001f
                    || Mathf.Abs(candidate.Scale - 1f) > 0.0001f)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ApplyDirectLayer(
            Configuration config,
            TileWorldCreatorManager manager,
            ObjectPlacementLayer source,
            string generatedName,
            IReadOnlyList<CompiledLayerMap> terrainLayers)
        {
            var rule = source.Rule ?? new ObjectPlacementRule();
            var root = EnsureDirectRoot(manager, rule);
            var existing = root.Find(generatedName);
            if (existing != null)
                DestroyObject(existing.gameObject);

            var layerObject = new GameObject(generatedName);
            var layerRoot = layerObject.transform;
            layerRoot.SetParent(root, false);
            layerRoot.localPosition = Vector3.zero;
            layerRoot.localRotation = Quaternion.identity;
            layerRoot.localScale = Vector3.one;

            float cellSize = Mathf.Max(0.0001f, config.cellSize);
            float baseHeight = ResolveTargetHeight(config, source.TargetGraphLayerId, terrainLayers);
            int mapWidth = Mathf.Max(1, config.width);
            int mapHeight = Mathf.Max(1, config.height);
            int spawned = 0;
            int outOfBounds = 0;
            int missingPrefab = 0;
            int instantiateFailed = 0;

            for (int i = 0; i < source.Candidates.Count; i++)
            {
                var candidate = source.Candidates[i];
                if (candidate.Cell.x < 0
                    || candidate.Cell.y < 0
                    || candidate.Cell.x >= mapWidth
                    || candidate.Cell.y >= mapHeight)
                {
                    outOfBounds++;
                    continue;
                }

                var entry = ResolvePrefabEntry(source, candidate);
                if (entry?.Prefab == null)
                {
                    missingPrefab++;
                    continue;
                }

                var instance = InstantiateDirectPrefab(entry.Prefab, layerRoot);
                if (instance == null)
                {
                    instantiateFailed++;
                    continue;
                }

                instance.name = $"{entry.Prefab.name}_{spawned:0000}";

                var localPosition = new Vector3(
                    (candidate.Cell.x + candidate.LocalOffset.x) * cellSize,
                    baseHeight + entry.YOffset - rule.EmbedIntoGround,
                    (candidate.Cell.y + candidate.LocalOffset.y) * cellSize);

                instance.transform.localPosition = localPosition;
                instance.transform.localRotation = Quaternion.Euler(0f, ResolveDirectYaw(candidate, entry, rule), 0f);

                float scale = ResolveDirectScale(candidate, entry, rule);
                var prefabScale = instance.transform.localScale;
                instance.transform.localScale = new Vector3(
                    prefabScale.x * scale,
                    prefabScale.y * scale,
                    prefabScale.z * scale);

                ApplyMaterialOverride(instance, entry.MaterialOverride);
                spawned++;
            }

            MarkSceneDirty(manager);
            if (spawned == 0)
            {
                DestroyObject(layerObject);
                Debug.LogWarning(
                    $"[MoyvaObjectPlacement] Direct object layer '{source.LayerName}' created no instances. " +
                    $"Candidates={source.Candidates.Count}, outOfBounds={outOfBounds}, missingPrefab={missingPrefab}, " +
                    $"instantiateFailed={instantiateFailed}, map={mapWidth}x{mapHeight}, targetLayer='{source.TargetGraphLayerId ?? "<none>"}'.");
            }
        }

        private static void ApplyLayer(
            Configuration config,
            TileWorldCreatorManager manager,
            ObjectPlacementLayer source,
            string generatedName,
            IReadOnlyList<CompiledLayerMap> terrainLayers)
        {
            var positions = BuildCellPositions(source.Candidates, config.width, config.height, out int outOfBounds);
            if (positions.Count == 0)
            {
                RemoveGeneratedLayerByName(config, generatedName);
                MarkSceneDirty(manager);
                Debug.LogWarning(
                    $"[MoyvaObjectPlacement] TWC object layer '{source.LayerName}' has no in-bounds cells after filtering. " +
                    $"Candidates={source.Candidates.Count}, outOfBounds={outOfBounds}, map={Mathf.Max(1, config.width)}x{Mathf.Max(1, config.height)}, " +
                    $"targetLayer='{source.TargetGraphLayerId ?? "<none>"}'.");
                return;
            }

            var blueprint = FindBlueprintLayer(config, generatedName)
                            ?? CreateBlueprintLayer(config, generatedName);
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
            blueprint.AddCells(positions);

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
            int mapHeight,
            out int outOfBounds)
        {
            var positions = new HashSet<Vector2>();
            outOfBounds = 0;
            if (candidates == null)
                return positions;

            int w = Mathf.Max(1, mapWidth);
            int h = Mathf.Max(1, mapHeight);
            for (int i = 0; i < candidates.Count; i++)
            {
                var cell = candidates[i].Cell;
                if (cell.x < 0 || cell.y < 0 || cell.x >= w || cell.y >= h)
                {
                    outOfBounds++;
                    continue;
                }

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

        private static ObjectPrefabEntry ResolvePrefabEntry(ObjectPlacementLayer source, ScatterCandidate candidate)
        {
            if (source?.Prefabs == null || source.Prefabs.Count == 0)
                return null;

            int index = candidate.PrefabIndex;
            if (index < 0 || index >= source.Prefabs.Count)
                index = 0;

            var entry = source.Prefabs[index];
            if (entry?.Prefab != null)
                return entry;

            for (int i = 0; i < source.Prefabs.Count; i++)
            {
                if (source.Prefabs[i]?.Prefab != null)
                    return source.Prefabs[i];
            }

            return null;
        }

        private static float ResolveDirectYaw(
            ScatterCandidate candidate,
            ObjectPrefabEntry entry,
            ObjectPlacementRule rule)
        {
            if (entry != null && !entry.RandomYaw && Mathf.Approximately(rule.RotationRandomization, 0f))
                return 0f;

            return candidate.RotationY;
        }

        private static float ResolveDirectScale(
            ScatterCandidate candidate,
            ObjectPrefabEntry entry,
            ObjectPlacementRule rule)
        {
            if (entry != null && HasCustomScaleRange(entry.MinScale, entry.MaxScale))
            {
                float t = ProceduralNoiseUtility.Hash01(
                    candidate.Cell.x,
                    candidate.Cell.y,
                    unchecked((rule.RandomSeed * 397) ^ 0x6d2b79f5));
                return Mathf.Lerp(
                    Mathf.Max(0.01f, Mathf.Min(entry.MinScale, entry.MaxScale)),
                    Mathf.Max(0.01f, Mathf.Max(entry.MinScale, entry.MaxScale)),
                    t);
            }

            if (rule != null && HasCustomScaleRange(rule.ScaleRandomization.x, rule.ScaleRandomization.y))
            {
                float t = ProceduralNoiseUtility.Hash01(
                    candidate.Cell.x,
                    candidate.Cell.y,
                    unchecked((rule.RandomSeed * 733) ^ 0x51e4a1cd));
                return Mathf.Lerp(
                    Mathf.Max(0.01f, Mathf.Min(rule.ScaleRandomization.x, rule.ScaleRandomization.y)),
                    Mathf.Max(0.01f, Mathf.Max(rule.ScaleRandomization.x, rule.ScaleRandomization.y)),
                    t);
            }

            return Mathf.Max(0.01f, candidate.Scale);
        }

        private static bool HasCustomScaleRange(float min, float max)
        {
            return Mathf.Abs(min - 0.9f) > 0.0001f
                || Mathf.Abs(max - 1.1f) > 0.0001f;
        }

        private static void ApplyMaterialOverride(GameObject instance, Material material)
        {
            if (instance == null || material == null)
                return;

            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
                renderers[i].sharedMaterial = material;
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

        private static Transform EnsureDirectRoot(TileWorldCreatorManager manager, ObjectPlacementRule rule)
        {
            var parent = manager.transform;
            string rootName = ResolveDirectRootName(rule);
            var root = parent.Find(rootName);
            if (root != null)
                return root;

            var rootObject = new GameObject(rootName);
            root = rootObject.transform;
            root.SetParent(parent, false);
            root.localPosition = Vector3.zero;
            root.localRotation = Quaternion.identity;
            root.localScale = Vector3.one;
            return root;
        }

        private static string ResolveDirectRootName(ObjectPlacementRule rule)
        {
            string configured = rule?.ParentContainer;
            return string.IsNullOrWhiteSpace(configured)
                ? DirectObjectsRootName
                : configured.Trim();
        }

        private static GameObject InstantiateDirectPrefab(GameObject prefab, Transform parent)
        {
            if (prefab == null)
                return null;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (PrefabUtility.InstantiatePrefab(prefab, parent) is GameObject prefabInstance)
                    return prefabInstance;
            }
#endif

            return UnityEngine.Object.Instantiate(prefab, parent);
        }

        private static void RemoveStaleDirectLayers(
            TileWorldCreatorManager manager,
            HashSet<string> activeGeneratedNames)
        {
            if (manager == null)
                return;

            for (int rootIndex = manager.transform.childCount - 1; rootIndex >= 0; rootIndex--)
            {
                var root = manager.transform.GetChild(rootIndex);
                if (root == null)
                    continue;

                bool containsGeneratedChildren = false;
                for (int i = root.childCount - 1; i >= 0; i--)
                {
                    var child = root.GetChild(i);
                    if (child == null || !child.name.StartsWith(GeneratedLayerNamePrefix, StringComparison.Ordinal))
                        continue;

                    containsGeneratedChildren = true;
                    if (!activeGeneratedNames.Contains(child.name))
                        DestroyObject(child.gameObject);
                }

                if (containsGeneratedChildren && root.childCount == 0)
                    DestroyObject(root.gameObject);
            }
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

        private static void RemoveGeneratedLayerByName(Configuration config, string generatedName)
        {
            if (config == null || string.IsNullOrEmpty(generatedName))
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
                        if (!IsGeneratedBlueprintLayer(layer) || layer.layerName != generatedName)
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
                        if (!IsGeneratedObjectLayer(layer) || layer.layerName != generatedName)
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

        private static BlueprintLayer CreateBlueprintLayer(Configuration config, string layerName)
        {
            if (config == null)
                return null;

            EnsureFolders(config);

            var layer = ScriptableObject.CreateInstance<BlueprintLayer>();
            layer.layerName = layerName;
            layer.hideFlags = IsPersistentAsset(config)
                ? HideFlags.HideInHierarchy
                : HideFlags.HideAndDontSave;

#if UNITY_EDITOR
            if (IsPersistentAsset(config))
                AssetDatabase.AddObjectToAsset(layer, config);
#endif

            config.blueprintLayerFolders[0].blueprintLayers.Add(layer);
            return layer;
        }

        private static T CreateBuildLayer<T>(Configuration config, string layerName) where T : BuildLayer
        {
            if (config == null)
                return null;

            EnsureFolders(config);

            var layer = ScriptableObject.CreateInstance<T>();
            layer.layerName = layerName;
            layer.hideFlags = IsPersistentAsset(config)
                ? HideFlags.HideInHierarchy
                : HideFlags.HideAndDontSave;

#if UNITY_EDITOR
            if (IsPersistentAsset(config))
                AssetDatabase.AddObjectToAsset(layer, config);
#endif

            config.buildLayerFolders[0].buildLayers.Add(layer);
            return layer;
        }

        private static bool IsPersistentAsset(UnityEngine.Object obj)
        {
#if UNITY_EDITOR
            return obj != null && AssetDatabase.Contains(obj);
#else
            return false;
#endif
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

        private static void MarkSceneDirty(TileWorldCreatorManager manager)
        {
#if UNITY_EDITOR
            if (manager != null && !Application.isPlaying)
                EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
#endif
        }

        private static void RemoveSubAsset(UnityEngine.Object obj)
        {
#if UNITY_EDITOR
            if (obj == null || Application.isPlaying)
                return;

            if (AssetDatabase.Contains(obj))
                AssetDatabase.RemoveObjectFromAsset(obj);
            else
                UnityEngine.Object.DestroyImmediate(obj);
#endif
        }

        private static void DestroyObject(UnityEngine.Object obj)
        {
            if (obj == null)
                return;

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(obj);
            else
                UnityEngine.Object.DestroyImmediate(obj);
        }
    }
}
