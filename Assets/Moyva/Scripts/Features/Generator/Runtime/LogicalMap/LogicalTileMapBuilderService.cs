using System;
using System.Collections.Generic;
using System.Linq;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ILogicalTileMapTwcLookup
    {
        TilesBuildLayer FindTilesBuildLayer(Configuration configuration, string blueprintLayerGuid);
        float ResolveSurfaceHeight(BlueprintLayer blueprint, TilesBuildLayer buildLayer);
    }

    internal interface ILogicalTileMapCellWriter
    {
        void Fill(LogicalTileMap map, LogicalTileLayerData layer);
        void Set(LogicalTileMap map, int x, int y, LogicalTileLayerData layer);
    }

    internal interface ILogicalTileMapBuilderService
    {
        LogicalTileMap Build(
            GeneratorMapRecipe recipe,
            TileWorldCreatorManager manager,
            IReadOnlyList<CompiledLayerMap> compiled,
            int width,
            int height);
    }

    internal sealed class LogicalTileMapTwcLookup : ILogicalTileMapTwcLookup
    {
        public TilesBuildLayer FindTilesBuildLayer(Configuration configuration, string blueprintLayerGuid)
        {
            if (configuration?.buildLayerFolders == null || string.IsNullOrWhiteSpace(blueprintLayerGuid))
                return null;
            foreach (var folder in configuration.buildLayerFolders)
            {
                if (folder?.buildLayers == null)
                    continue;
                foreach (var layer in folder.buildLayers)
                    if (layer is TilesBuildLayer buildLayer && Matches(buildLayer, blueprintLayerGuid))
                        return buildLayer;
            }
            return null;
        }

        public float ResolveSurfaceHeight(BlueprintLayer blueprint, TilesBuildLayer buildLayer)
            => TileWorldCreatorFillTileSurfaceHeightUtility.ResolveTilesBuildLayerTopHeight(blueprint, buildLayer);

        private static bool Matches(TilesBuildLayer buildLayer, string blueprintLayerGuid)
        {
            return string.Equals(buildLayer.assignedBlueprintLayerGuid, blueprintLayerGuid, StringComparison.Ordinal)
                   || string.Equals(buildLayer.currentBlueprintLayer?.guid, blueprintLayerGuid, StringComparison.Ordinal);
        }
    }

    internal sealed class LogicalTileMapCellWriter : ILogicalTileMapCellWriter
    {
        public void Fill(LogicalTileMap map, LogicalTileLayerData layer)
        {
            for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
                Set(map, x, y, layer);
        }

        public void Set(LogicalTileMap map, int x, int y, LogicalTileLayerData layer)
        {
            map.AddSample(x, y, layer.ToSample());
        }
    }

    internal sealed class LogicalTileMapBuilderService : ILogicalTileMapBuilderService
    {
        private readonly ILogicalTileMapTwcLookup _twcLookup;
        private readonly ILogicalTileMapCellWriter _cellWriter;

        public LogicalTileMapBuilderService(
            ILogicalTileMapTwcLookup twcLookup,
            ILogicalTileMapCellWriter cellWriter)
        {
            _twcLookup = twcLookup;
            _cellWriter = cellWriter;
        }

        public LogicalTileMap Build(
            GeneratorMapRecipe recipe,
            TileWorldCreatorManager manager,
            IReadOnlyList<CompiledLayerMap> compiled,
            int width,
            int height)
        {
            var map = new LogicalTileMap(width, height);
            if (recipe == null || manager == null || compiled == null)
                return map;
            foreach (var layerMap in Order(compiled))
                ApplyLayer(recipe, manager, map, layerMap);
            return map;
        }

        private void ApplyLayer(GeneratorMapRecipe recipe, TileWorldCreatorManager manager,
            LogicalTileMap map, CompiledLayerMap layerMap)
        {
            if (!CanApplyLayer(recipe, layerMap, out var recipeLayer, out var layerKind))
                return;
            var buildLayer = _twcLookup.FindTilesBuildLayer(manager.configuration, layerMap.BlueprintLayerGuid);
            if (layerKind != LayerKind.MaskOnly && (buildLayer == null || !buildLayer.isEnabled))
                return;
            var blueprint = manager.GetBlueprintLayerByGuid(layerMap.BlueprintLayerGuid);
            if (blueprint == null)
                return;
            // The recipe is the source of truth for the layer base height.
            // Preserve build-layer/prefab surface offsets even if the companion
            // BlueprintLayer has not yet been synchronized by the editor.
            float layerHeight = recipeLayer.DefaultHeight;
            float projectedSurfaceHeight = _twcLookup.ResolveSurfaceHeight(blueprint, buildLayer);
            float surfaceHeight = ResolveAuthoritativeSurfaceHeight(
                layerHeight,
                blueprint.defaultLayerHeight,
                projectedSurfaceHeight);
            var data = CreateLayerData(
                recipe,
                layerMap,
                recipeLayer.Name,
                layerHeight,
                surfaceHeight,
                buildLayer,
                layerKind);
            if (buildLayer != null && buildLayer.generateFlatSurface)
            {
                _cellWriter.Fill(map, data);
                return;
            }
            if (blueprint.allPositions == null || blueprint.allPositions.Count == 0)
                return;
            foreach (var position in blueprint.allPositions)
            {
                int x = Mathf.RoundToInt(position.x);
                int y = Mathf.RoundToInt(position.y);
                if (x >= 0 && x < map.Width && y >= 0 && y < map.Height)
                    _cellWriter.Set(map, x, y, ResolveCellData(layerMap, data, x, y));
            }
        }

        private static LogicalTileLayerData ResolveCellData(
            CompiledLayerMap layerMap,
            LogicalTileLayerData data,
            int x,
            int y)
        {
            var overrides = layerMap.SurfaceHeightOverride;
            if (overrides == null
                || x >= overrides.GetLength(0)
                || y >= overrides.GetLength(1))
            {
                return data;
            }
            float cellSurface = overrides[x, y];
            if (float.IsNaN(cellSurface) || float.IsInfinity(cellSurface))
                return data;

            float bed = data.LayerHeight;
            var beds = layerMap.BedHeightOverride;
            if (beds != null
                && x < beds.GetLength(0)
                && y < beds.GetLength(1))
            {
                float cellBed = beds[x, y];
                if (!float.IsNaN(cellBed) && !float.IsInfinity(cellBed))
                    bed = Mathf.Min(cellBed, cellSurface);
            }
            return data.WithHeights(bed, cellSurface);
        }

        private static bool CanApplyLayer(GeneratorMapRecipe recipe, CompiledLayerMap layerMap,
            out GeneratorMapLayer recipeLayer,
            out LayerKind layerKind)
        {
            recipeLayer = null;
            layerKind = LayerKind.BaseTerrain;
            if (layerMap == null
                || string.IsNullOrEmpty(layerMap.LayerId)
                || string.IsNullOrEmpty(layerMap.BlueprintLayerGuid))
                return false;
            recipeLayer = FindLayer(recipe, layerMap.LayerId);
            if (recipeLayer == null || !recipeLayer.Enabled)
                return false;
            layerKind = ResolveLayerKind(recipeLayer);
            if (!layerMap.HasRenderableTileOutput && layerKind != LayerKind.MaskOnly)
                return false;
            return true;
        }

        internal static float ResolveAuthoritativeSurfaceHeight(
            float layerHeight,
            float blueprintLayerHeight,
            float projectedSurfaceHeight)
            => layerHeight + (projectedSurfaceHeight - blueprintLayerHeight);

        private static LogicalTileLayerData CreateLayerData(
            GeneratorMapRecipe recipe,
            CompiledLayerMap layerMap,
            string recipeLayerName,
            float layerHeight,
            float surfaceHeight,
            TilesBuildLayer buildLayer,
            LayerKind layerKind)
        {
            string tileId = !string.IsNullOrWhiteSpace(layerMap.GridTileId)
                ? layerMap.GridTileId
                : layerMap.LayerId;
            string layerName = !string.IsNullOrWhiteSpace(layerMap.LayerName)
                ? layerMap.LayerName
                : recipeLayerName;
            GeneratorMapLayer recipeLayer = FindLayer(recipe, layerMap.LayerId);
            return new LogicalTileLayerData(
                layerMap.LayerId,
                layerName,
                tileId,
                layerHeight,
                surfaceHeight,
                layerMap.BlueprintLayerGuid,
                !string.IsNullOrWhiteSpace(layerMap.BuildLayerGuid) ? layerMap.BuildLayerGuid : buildLayer?.guid,
                ResolvePresetId(buildLayer, layerMap),
                layerKind,
                layerMap.SortingOrder,
                layerMap.LayerOrder,
                layerMap.TerrainPriority,
                layerMap.SourceLayerId,
                recipeLayer?.TileGeometryMode ?? TileGeometryMode.SolidTerrain,
                recipeLayer?.AuthoredClosurePolicy ?? AuthoredClosurePolicy.PreserveAuthored);
        }

        private static string ResolvePresetId(TilesBuildLayer buildLayer, CompiledLayerMap layerMap)
        {
            if (!string.IsNullOrWhiteSpace(layerMap.PresetId))
                return layerMap.PresetId;
            var preset = buildLayer?.tilePresetsTop?.Find(selection => selection?.preset != null)?.preset
                         ?? buildLayer?.tilePresetsMiddle?.Find(selection => selection?.preset != null)?.preset
                         ?? buildLayer?.tilePresetsBottom?.Find(selection => selection?.preset != null)?.preset;
            if (preset == null)
                return null;
            return !string.IsNullOrWhiteSpace(preset.tileId) ? preset.tileId.Trim() : preset.name;
        }

        private static GeneratorMapLayer FindLayer(GeneratorMapRecipe recipe, string layerId)
        {
            if (recipe?.Layers == null || string.IsNullOrEmpty(layerId))
                return null;
            foreach (var layer in recipe.Layers)
            {
                if (layer != null && string.Equals(layer.Id, layerId, StringComparison.Ordinal))
                    return layer;
            }
            return null;
        }

        private static LayerKind ResolveLayerKind(GeneratorMapLayer layer)
        {
            return layer?.OutputKind switch
            {
                LayerOutputKind.Objects => LayerKind.ObjectSpawn,
                LayerOutputKind.Masks => LayerKind.MaskOnly,
                _ => LayerKind.BaseTerrain
            };
        }

        private static List<CompiledLayerMap> Order(IReadOnlyList<CompiledLayerMap> compiled)
        {
            var ordered = new List<CompiledLayerMap>(compiled);
            ordered.Sort((a, b) => a.SortingOrder.CompareTo(b.SortingOrder));
            return ordered;
        }
    }

    /// <summary>Convenience facade over <see cref="ILogicalTileMapBuilderService"/>.</summary>
    internal static class LogicalTileMapBuilder
    {
        private static readonly ILogicalTileMapBuilderService Builder =
            new LogicalTileMapBuilderService(
                new LogicalTileMapTwcLookup(),
                new LogicalTileMapCellWriter());

        public static LogicalTileMap Build(
            GeneratorMapRecipe recipe,
            TileWorldCreatorManager manager,
            IReadOnlyList<CompiledLayerMap> compiled,
            int width,
            int height)
        {
            return Builder.Build(recipe, manager, compiled, width, height);
        }
    }
}
