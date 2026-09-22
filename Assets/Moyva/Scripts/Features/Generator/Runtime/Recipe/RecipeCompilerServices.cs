using System;
using System.Collections.Generic;
using System.Linq;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.JsonConfig;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>Result of synchronizing recipe layers into TWC blueprint layers.</summary>
    internal sealed class RecipeBlueprintSyncResult
    {
        public readonly List<CompiledLayerMap> CompiledLayers = new();
        public readonly List<GeneratorMapLayer> OrderedLayers = new();
        public readonly Dictionary<string, string> BlueprintGuidByLayerId = new();
        public readonly Dictionary<string, BlueprintLayer> BlueprintByLayerId = new();
        public readonly HashSet<string> UsedLayerGuids = new();
        public readonly List<BlueprintLayer> ExistingLayers = new();
    }

    internal static class RecipeCompilerLayerAssetUtility
    {
        public static void EnsureBlueprintRootFolder(Configuration config)
        {
            config.blueprintLayerFolders ??= new List<BlueprintLayerFolder>();
            if (config.blueprintLayerFolders.Count == 0)
                config.blueprintLayerFolders.Add(new BlueprintLayerFolder("Root"));
        }

        public static void EnsureBuildRootFolder(Configuration config)
        {
            config.buildLayerFolders ??= new List<BuildLayerFolder>();
            if (config.buildLayerFolders.Count == 0)
                config.buildLayerFolders.Add(new BuildLayerFolder("Root"));
        }

        public static void PrepareLayerAsset(Configuration config, ScriptableObject layer, string layerName)
        {
            layer.name = layerName;
            if (layer is BuildLayer buildLayer)
                buildLayer.layerName = layerName;
            if (layer is BlueprintLayer blueprintLayer)
                blueprintLayer.layerName = layerName;
            layer.hideFlags = IsPersistentAsset(config) ? HideFlags.HideInHierarchy : HideFlags.HideAndDontSave;
#if UNITY_EDITOR
            if (IsPersistentAsset(config))
                UnityEditor.AssetDatabase.AddObjectToAsset(layer, config);
#endif
        }

        public static bool IsPersistentAsset(Object obj)
        {
#if UNITY_EDITOR
            return obj != null && UnityEditor.AssetDatabase.Contains(obj);
#else
            return false;
#endif
        }
    }

    /// <summary>Applies recipe map size/seed to the TWC configuration.</summary>
    internal sealed class RecipeCompilerConfigurationService
    {
        private static readonly Vector2Int DefaultMapSize = new(50, 50);

        public Vector2Int ResolveRequestedSize(GeneratorMapRecipe recipe, Vector2Int? mapSizeOverride)
        {
            if (mapSizeOverride.HasValue)
                return Normalize(mapSizeOverride.Value);
            var sharedSize = recipe?.SharedSettings?.MapSize ?? DefaultMapSize;
            return Normalize(sharedSize);
        }

        public void Apply(GeneratorMapRecipe recipe, Configuration config, int seed, Vector2Int? mapSizeOverride)
        {
            Vector2Int size = ResolveRequestedSize(recipe, mapSizeOverride);
            config.width = size.x;
            config.height = size.y;
            config.useGlobalRandomSeed = true;
            config.globalRandomSeed = seed;
            config.currentRandomSeed = unchecked((uint)seed);
        }

        private static Vector2Int Normalize(Vector2Int size)
        {
            return new Vector2Int(Mathf.Max(1, size.x), Mathf.Max(1, size.y));
        }
    }

    /// <summary>Finds TWC tile build layers linked to a recipe layer.</summary>
    internal sealed class RecipeCompilerTileBuildLayerLookup
    {
        public TilesBuildLayer Find(Configuration config, string blueprintLayerGuid)
        {
            if (config?.buildLayerFolders == null || string.IsNullOrWhiteSpace(blueprintLayerGuid))
                return null;
            foreach (var folder in config.buildLayerFolders)
            {
                if (folder?.buildLayers == null)
                    continue;
                foreach (var layer in folder.buildLayers)
                {
                    if (layer is TilesBuildLayer buildLayer
                        && (string.Equals(buildLayer.assignedBlueprintLayerGuid, blueprintLayerGuid, StringComparison.Ordinal)
                            || string.Equals(buildLayer.currentBlueprintLayer?.guid, blueprintLayerGuid, StringComparison.Ordinal)))
                        return buildLayer;
                }
            }
            return null;
        }

        public string ResolveTileId(TilesBuildLayer buildLayer)
        {
            return ResolveTileId(buildLayer?.tilePresetsTop)
                   ?? ResolveTileId(buildLayer?.tilePresetsMiddle)
                   ?? ResolveTileId(buildLayer?.tilePresetsBottom);
        }

        private static string ResolveTileId(List<TilesBuildLayer.TilePresetSelection> selections)
        {
            if (selections == null)
                return null;
            foreach (var selection in selections)
            {
                string tileId = selection?.preset?.tileId;
                if (!string.IsNullOrWhiteSpace(tileId))
                    return tileId.Trim();
            }
            return null;
        }
    }

    /// <summary>Synchronizes recipe layers into TWC blueprint layers.</summary>
    internal sealed class RecipeCompilerBlueprintSyncService
    {
        private readonly RecipeCompilerTileBuildLayerLookup _buildLayerLookup;

        public RecipeCompilerBlueprintSyncService(RecipeCompilerTileBuildLayerLookup buildLayerLookup)
        {
            _buildLayerLookup = buildLayerLookup;
        }

        public RecipeBlueprintSyncResult Sync(GeneratorMapRecipe recipe, Configuration config, ISet<string> skippedLayerIds)
        {
            var result = new RecipeBlueprintSyncResult();
            result.ExistingLayers.AddRange(GetAllBlueprintLayers(config));
            result.OrderedLayers.AddRange(GeneratorMaskEvaluator.OrderedLayers(recipe, null)
                .Where(layer => skippedLayerIds == null || !skippedLayerIds.Contains(layer.Id)));
            for (int order = 0; order < result.OrderedLayers.Count; order++)
                SyncLayer(recipe, config, result.OrderedLayers[order], result, order);
            Reorder(config, result.OrderedLayers, result.BlueprintByLayerId);
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

        private void SyncLayer(GeneratorMapRecipe recipe, Configuration config, GeneratorMapLayer layerDef,
            RecipeBlueprintSyncResult result, int layerOrder)
        {
            var blueprint = FindByGuid(result.ExistingLayers, layerDef.BlueprintLayerGuid)
                            ?? FindByName(result.ExistingLayers, layerDef.Name)
                            ?? CreateBlueprintLayer(config, layerDef.Name);
            if (blueprint == null)
                return;
            ApplyLayerDefinition(layerDef, blueprint);
            result.UsedLayerGuids.Add(blueprint.guid);
            result.BlueprintGuidByLayerId[layerDef.Id] = blueprint.guid;
            result.BlueprintByLayerId[layerDef.Id] = blueprint;
            result.CompiledLayers.Add(CreateCompiledMap(recipe, config, layerDef, blueprint, layerOrder));
        }

        private CompiledLayerMap CreateCompiledMap(GeneratorMapRecipe recipe, Configuration config,
            GeneratorMapLayer layerDef, BlueprintLayer blueprint, int layerOrder)
        {
            var buildLayer = _buildLayerLookup.Find(config, blueprint.guid);
            return new CompiledLayerMap
            {
                LayerId = layerDef.Id,
                GridTileId = ResolveGridTileIdForLayer(recipe, config, layerDef, blueprint.guid),
                BlueprintLayerGuid = blueprint.guid,
                LayerName = blueprint.layerName,
                SortingOrder = layerDef.SortingOrder,
                LayerOrder = layerOrder,
                TerrainPriority = layerDef.SortingOrder,
                BuildLayerGuid = buildLayer?.guid,
                PresetId = ResolvePresetId(buildLayer),
                SourceLayerId = ResolveTileSourceId(layerDef),
                HasRenderableTileOutput = HasRenderableTileOutput(layerDef)
            };
        }

        private string ResolveGridTileIdForLayer(GeneratorMapRecipe recipe, Configuration config,
            GeneratorMapLayer layerDef, string blueprintLayerGuid)
        {
            if (!HasRenderableTileOutput(layerDef))
                return null;
            string resolvedTileId = layerDef.ResolveTileId();
            if (!string.IsNullOrWhiteSpace(resolvedTileId))
                return resolvedTileId;
            var buildLayer = _buildLayerLookup.Find(config, blueprintLayerGuid);
            if (buildLayer == null || buildLayer.generateFlatSurface)
                return layerDef.Id;
            return _buildLayerLookup.ResolveTileId(buildLayer) ?? layerDef.Id;
        }

        internal static bool HasRenderableTileOutput(GeneratorMapLayer layer)
        {
            return layer != null && layer.HasRenderableTileOutput();
        }

        private static string ResolvePresetId(TilesBuildLayer buildLayer)
        {
            var preset = buildLayer?.tilePresetsTop?.FirstOrDefault(selection => selection?.preset != null)?.preset
                         ?? buildLayer?.tilePresetsMiddle?.FirstOrDefault(selection => selection?.preset != null)?.preset
                         ?? buildLayer?.tilePresetsBottom?.FirstOrDefault(selection => selection?.preset != null)?.preset;
            return !string.IsNullOrWhiteSpace(preset?.tileId) ? preset.tileId.Trim() : preset != null ? preset.name : null;
        }

        private static string ResolveTileSourceId(GeneratorMapLayer layerDef)
        {
            if (!string.IsNullOrWhiteSpace(layerDef?.TileType?.JsonId))
                return layerDef.TileType.JsonId;
            return layerDef?.ResolveTileVariants().Find(v => v?.Preset != null)?.Preset?.name;
        }

        private static void ApplyLayerDefinition(GeneratorMapLayer layerDef, BlueprintLayer blueprint)
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
            RecipeCompilerLayerAssetUtility.EnsureBlueprintRootFolder(config);
            var layer = ScriptableObject.CreateInstance<BlueprintLayer>();
            RecipeCompilerLayerAssetUtility.PrepareLayerAsset(config, layer, layerName);
            config.blueprintLayerFolders[0].blueprintLayers.Add(layer);
            return layer;
        }

        private static void Reorder(Configuration config, List<GeneratorMapLayer> orderedLayers,
            Dictionary<string, BlueprintLayer> blueprintByLayerId)
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
                    && blueprintByLayerId.TryGetValue(layerDef.Id, out var blueprint)
                    && blueprint != null
                    && !orderedBlueprints.Contains(blueprint))
                    orderedBlueprints.Add(blueprint);
            }
            var recipeBlueprints = new HashSet<BlueprintLayer>(orderedBlueprints);
            var remainder = new List<BlueprintLayer>();
            foreach (var existing in root.blueprintLayers)
            {
                if (existing != null && !recipeBlueprints.Contains(existing))
                    remainder.Add(existing);
            }
            foreach (var folder in config.blueprintLayerFolders)
                folder?.blueprintLayers?.RemoveAll(layer => layer != null && recipeBlueprints.Contains(layer));
            foreach (var existing in remainder)
            {
                if (!orderedBlueprints.Contains(existing))
                    orderedBlueprints.Add(existing);
            }
            root.blueprintLayers = orderedBlueprints;
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

    /// <summary>Applies a recipe layer's tile data to a TWC TilesBuildLayer.</summary>
    internal static class RecipeTileBuildLayerWriter
    {
        public static void Apply(
            TilesBuildLayer buildLayer,
            GeneratorMapLayer layer,
            Configuration configuration,
            BlueprintLayer blueprintLayer)
        {
            if (buildLayer == null || layer == null)
                return;

            buildLayer.configuration = configuration;
            buildLayer.layerName = layer.Name;
            buildLayer.isEnabled = layer.Enabled;
            buildLayer.currentBlueprintLayer = blueprintLayer;
            if (blueprintLayer != null)
                buildLayer.SetBlueprintLayer(blueprintLayer);

            ApplyGeneralSettings(buildLayer, layer);
            ApplyPresetSelections(buildLayer, layer);
            EnsurePrimaryTileLayer(buildLayer, layer);
        }

        private static void ApplyGeneralSettings(TilesBuildLayer buildLayer, GeneratorMapLayer layer)
        {
            TileVisualConfig visual = layer.TileType?.Visual;
            var settings = layer.TileBuild ?? new GeneratorTileBuildSettings();
            buildLayer.useDualGrid = layer.ResolveUseDualGrid();
            buildLayer.scaleTileToCellSize = visual?.ScaleToCellSize ?? (settings.ScaleTileToCellSize || buildLayer.useDualGrid);
            buildLayer.layerYOffset = visual?.LayerYOffset ?? settings.LayerYOffset;
            buildLayer.scaleOffset = visual?.ScaleOffset ?? settings.ScaleOffset;
            buildLayer.generateFlatSurface = ResolveGenerateFlatSurface(layer, visual);
            buildLayer.flatSurfaceMaterial = visual?.FlatSurfaceMaterial ?? layer.FlatSurfaceMaterial;
            buildLayer.meshGenerationOverride = visual?.MeshGenerationOverride ?? settings.MeshGenerationOverride;
            buildLayer.mergeTiles = visual?.MergeTiles ?? settings.MergeTiles;
            buildLayer.shadowCastingMode = visual?.ShadowCastingMode ?? settings.ShadowCastingMode;
            buildLayer.objectLayer = visual?.ObjectLayer ?? settings.ObjectLayer;
            buildLayer.renderingLayer = visual?.RenderingLayer ?? settings.RenderingLayer;
            buildLayer.colliderType = visual?.ColliderType ?? settings.ColliderType;
            buildLayer.tileColliderHeight = Mathf.Max(0f, visual?.TileColliderHeight ?? settings.TileColliderHeight);
            buildLayer.tileColliderExtrusionHeight = Mathf.Max(0f, visual?.TileColliderExtrusionHeight ?? settings.TileColliderExtrusionHeight);
            buildLayer.invertCollisionWalls = visual?.InvertCollisionWalls ?? settings.InvertCollisionWalls;
        }

        private static void ApplyPresetSelections(TilesBuildLayer buildLayer, GeneratorMapLayer layer)
        {
            buildLayer.tilePresetsTop ??= new List<TilesBuildLayer.TilePresetSelection>();
            buildLayer.tilePresetsMiddle ??= new List<TilesBuildLayer.TilePresetSelection>();
            buildLayer.tilePresetsBottom ??= new List<TilesBuildLayer.TilePresetSelection>();
            buildLayer.tilePresetsTop.Clear();
            buildLayer.tilePresetsMiddle.Clear();
            buildLayer.tilePresetsBottom.Clear();

            if (ResolveGenerateFlatSurface(layer, layer.TileType?.Visual))
                return;

            var variants = layer.ResolveTileVariants();
            foreach (var variant in variants)
            {
                if (variant == null || variant.Preset == null)
                    continue;
                var selection = new TilesBuildLayer.TilePresetSelection
                {
                    preset = variant.Preset,
                    weight = variant.NormalizedWeight,
                    tileHeight = Mathf.Max(0f, variant.TileHeight)
                };
                switch (variant.Slot)
                {
                    case TilePresetSlot.Middle:
                        buildLayer.tilePresetsMiddle.Add(selection);
                        break;
                    case TilePresetSlot.Bottom:
                        buildLayer.tilePresetsBottom.Add(selection);
                        break;
                    default:
                        buildLayer.tilePresetsTop.Add(selection);
                        break;
                }
            }
        }

        private static void EnsurePrimaryTileLayer(TilesBuildLayer buildLayer, GeneratorMapLayer layer)
        {
            buildLayer.tileLayers ??= new List<TilesBuildLayer.TileLayers>();
            if (buildLayer.tileLayers.Count == 0)
                buildLayer.tileLayers.Add(new TilesBuildLayer.TileLayers());
            var first = buildLayer.tileLayers[0] ?? new TilesBuildLayer.TileLayers();
            first.name = string.IsNullOrWhiteSpace(first.name) ? "Main" : first.name;
            var settings = layer.TileBuild ?? new GeneratorTileBuildSettings();
            first.heightOffset = layer.TileType?.Visual?.TileLayerHeightOffset ?? settings.TileLayerHeightOffset;
            first.ignoreFillTiles = layer.TileType?.Visual?.IgnoreFillTiles ?? settings.IgnoreFillTiles;
            first.layerOverrides ??= new List<TilesBuildLayer.TilePresetOverride>();
            buildLayer.tileLayers[0] = first;
        }

        private static bool ResolveGenerateFlatSurface(GeneratorMapLayer layer, TileVisualConfig visual)
        {
            return visual != null
                ? visual.GridMode == TileGridMode.Flat
                : layer.GenerateFlatSurface;
        }
    }

    /// <summary>Synchronizes recipe tile build layers into the TWC configuration.</summary>
    internal sealed class RecipeCompilerTileBuildLayerSyncService
    {
        private readonly RecipeCompilerTileBuildLayerLookup _lookup;

        public RecipeCompilerTileBuildLayerSyncService(RecipeCompilerTileBuildLayerLookup lookup)
        {
            _lookup = lookup;
        }

        public void Sync(GeneratorMapRecipe recipe, Configuration config, TileWorldCreatorManager manager,
            RecipeBlueprintSyncResult blueprintSync, ISet<string> skippedLayerIds)
        {
            if (recipe == null || config == null || manager == null || blueprintSync == null)
                return;
            RecipeCompilerLayerAssetUtility.EnsureBuildRootFolder(config);
            var folder = config.buildLayerFolders[0];
            folder.buildLayers ??= new List<BuildLayer>();
            var orderedBuildLayers = new List<BuildLayer>();
            foreach (var layerDef in blueprintSync.OrderedLayers)
                SyncLayer(config, manager, blueprintSync, skippedLayerIds, folder, orderedBuildLayers, layerDef);
            PreserveGeneratedObjectLayers(folder, orderedBuildLayers);
            RemoveStaleTileBuildLayers(folder, orderedBuildLayers);
            folder.buildLayers = orderedBuildLayers;
        }

        private void SyncLayer(Configuration config, TileWorldCreatorManager manager,
            RecipeBlueprintSyncResult blueprintSync, ISet<string> skippedLayerIds, BuildLayerFolder folder,
            List<BuildLayer> orderedBuildLayers, GeneratorMapLayer layerDef)
        {
            if (layerDef == null || skippedLayerIds != null && skippedLayerIds.Contains(layerDef.Id))
                return;
            if (!RecipeCompilerBlueprintSyncService.HasRenderableTileOutput(layerDef))
                return;
            blueprintSync.BlueprintByLayerId.TryGetValue(layerDef.Id, out var blueprint);
            string blueprintGuid = blueprint?.guid ?? layerDef.BlueprintLayerGuid;
            var buildLayer = _lookup.Find(config, blueprintGuid)
                             ?? FindByName(folder, layerDef.Name, orderedBuildLayers);
            buildLayer = buildLayer == null
                ? MoyvaTerrainBuildLayerUpgradeUtility.CreateHeightAware(manager, layerDef.Name)
                : MoyvaTerrainBuildLayerUpgradeUtility.EnsureHeightAware(manager, config, buildLayer, layerDef.Name);
            RecipeTileBuildLayerWriter.Apply(buildLayer, layerDef, config, blueprint);
            if (!orderedBuildLayers.Contains(buildLayer))
                orderedBuildLayers.Add(buildLayer);
        }

        private static TilesBuildLayer FindByName(BuildLayerFolder folder, string layerName, List<BuildLayer> orderedBuildLayers)
        {
            return folder.buildLayers
                .OfType<TilesBuildLayer>()
                .FirstOrDefault(layer => layer != null && layer.layerName == layerName && !orderedBuildLayers.Contains(layer));
        }

        private static void PreserveGeneratedObjectLayers(BuildLayerFolder folder, List<BuildLayer> orderedBuildLayers)
        {
            foreach (var objectLayer in folder.buildLayers.Where(TWCObjectPlacementAdapter.IsGeneratedObjectLayer).ToList())
            {
                if (objectLayer != null && !orderedBuildLayers.Contains(objectLayer))
                    orderedBuildLayers.Add(objectLayer);
            }
        }

        private static void RemoveStaleTileBuildLayers(BuildLayerFolder folder, List<BuildLayer> orderedBuildLayers)
        {
            foreach (var stale in folder.buildLayers.ToList())
            {
                if (stale == null || orderedBuildLayers.Contains(stale) || TWCObjectPlacementAdapter.IsGeneratedObjectLayer(stale))
                    continue;
                folder.buildLayers.Remove(stale);
#if UNITY_EDITOR
                if (!UnityEditor.AssetDatabase.Contains(stale))
                    Object.DestroyImmediate(stale);
#else
                Object.Destroy(stale);
#endif
            }
        }
    }

    /// <summary>Attaches the authoritative evaluated mask to each blueprint layer.</summary>
    internal sealed class RecipeMaskBlueprintService
    {
        public void Apply(RecipeBlueprintSyncResult sync, Configuration config,
            IReadOnlyDictionary<string, bool[,]> layerMasks)
        {
            if (sync == null || layerMasks == null)
                return;
            foreach (var layerDef in sync.OrderedLayers)
            {
                if (!sync.BlueprintByLayerId.TryGetValue(layerDef.Id, out var blueprint)
                    || blueprint == null
                    || !layerMasks.TryGetValue(layerDef.Id, out var mask)
                    || mask == null)
                    continue;

                var modifier = JsonObjectFactory.Create<MoyvaPrecomputedMaskBlueprintModifier>();
                modifier.name = "Moyva Authoritative Recipe Output Mask";
                modifier.isEnabled = true;
                modifier.asset = config;
                modifier.sourceLayerId = layerDef.Id;
                modifier.sourceLayerName = layerDef.Name;
                modifier.SetPositions(EnumeratePositions(mask));
                blueprint.tileMapModifiers.Add(modifier);
            }
        }

        private static IEnumerable<Vector2> EnumeratePositions(bool[,] mask)
        {
            int width = mask.GetLength(0);
            int height = mask.GetLength(1);
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (mask[x, y])
                    yield return new Vector2(x, y);
            }
        }
    }
}
