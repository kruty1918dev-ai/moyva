using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITileWorldCreatorTerrainBuildLayerConfigurationService
    {
        void Configure(Configuration configuration);
    }

    internal sealed class TileWorldCreatorTerrainBuildLayerConfigurationService : ITileWorldCreatorTerrainBuildLayerConfigurationService
    {
        private const string LogTag = "[MoyvaTWCHeight]";
        private readonly TileWorldCreatorManager _manager;
        private readonly TileWorldCreatorIdMappingSO _mapping;
        private readonly TileWorldCreatorBuildOptions _options;
        private readonly ITileWorldCreatorBlueprintLayerResolver _resolver;
        private readonly HashSet<string> _loggedInvalidBuildLayers = new();

        public TileWorldCreatorTerrainBuildLayerConfigurationService(
            ITileWorldCreatorBuildEnvironment environment,
            ITileWorldCreatorBlueprintLayerResolver resolver)
        {
            _manager = environment.Manager;
            _mapping = environment.Mapping;
            _options = environment.Options;
            _resolver = resolver;
        }

        public void Configure(Configuration configuration)
        {
            if (configuration == null || _mapping?.TerrainLayers == null)
                return;

            var configuredLayerGuids = new HashSet<string>();
            foreach (var mapping in _mapping.TerrainLayers)
            {
                if (mapping == null || mapping.TilePreset == null)
                    continue;
                if (!_resolver.TryResolve(configuration, mapping, out string blueprintLayerGuid))
                    continue;
                if (!configuredLayerGuids.Add(blueprintLayerGuid))
                    continue;

                ConfigureLayer(configuration, mapping, blueprintLayerGuid);
            }
        }

        private void ConfigureLayer(Configuration configuration, TileWorldCreatorIdMappingSO.LayerMapping mapping, string blueprintLayerGuid)
        {
            var buildLayer = TileWorldCreatorBuildLayerLookup.FindTilesBuildLayer(configuration, blueprintLayerGuid);
            if (buildLayer == null)
            {
                LogInvalidBuildLayerOnce(mapping, "no TilesBuildLayer is assigned to the resolved blueprint layer");
                return;
            }

            if (_options.TerrainBuildMode != TileWorldCreatorTerrainBuildMode.LegacyPostBuildHeightProjection)
                buildLayer = MoyvaTerrainBuildLayerUpgradeUtility.EnsureHeightAware(_manager, configuration, buildLayer, buildLayer.layerName);

            bool useDualGrid = TileWorldCreatorTerrainPresetUtility.ShouldUseDualGrid(mapping.TilePreset, mapping.UseDualGrid);
            if (!TileWorldCreatorTerrainPresetUtility.HasUsableTilePreset(mapping.TilePreset, useDualGrid))
            {
                LogInvalidBuildLayerOnce(mapping, $"tile preset '{mapping.TilePreset.name}' has no usable {(useDualGrid ? "dual" : "normal")} prefab references");
                return;
            }

            bool oldUseDualGrid = buildLayer.useDualGrid;
            bool oldScaleToCell = buildLayer.scaleTileToCellSize;
            bool oldLayerMerge = buildLayer.mergeTiles;
            float oldLayerYOffset = buildLayer.layerYOffset;

            buildLayer.SetBlueprintLayer(blueprintLayerGuid);
            buildLayer.SetNewTilePreset(mapping.TilePreset);
            buildLayer.useDualGrid = useDualGrid;
            buildLayer.scaleTileToCellSize = mapping.ScaleTileToCellSize || useDualGrid;
            buildLayer.layerYOffset = 0f;
            DisableLegacyMergeIfNeeded(buildLayer);
            NormalizeTileLayerHeights(buildLayer);

            Debug.Log($"{LogTag} Prepared terrain build layer: idPattern='{mapping.IdPattern}', blueprintGuid='{blueprintLayerGuid}', blueprintName='{mapping.BlueprintLayerName}', buildLayer='{buildLayer.layerName}', preset='{mapping.TilePreset.name}', presetGrid={mapping.TilePreset.gridtype}, useDualGrid {oldUseDualGrid}->{buildLayer.useDualGrid}, scaleToCell {oldScaleToCell}->{buildLayer.scaleTileToCellSize}, layerYOffset {oldLayerYOffset}->{buildLayer.layerYOffset}, buildMerge {oldLayerMerge}->{buildLayer.mergeTiles}, tileLayers={buildLayer.tileLayers.Count}.");
        }

        private void DisableLegacyMergeIfNeeded(TilesBuildLayer buildLayer)
        {
            if (!_options.ApplyIntegerTerrainHeights
                || _options.TerrainBuildMode != TileWorldCreatorTerrainBuildMode.LegacyPostBuildHeightProjection
                || !buildLayer.meshGenerationOverride
                || !buildLayer.mergeTiles)
            {
                return;
            }

            Debug.LogWarning($"{LogTag} Disabling buildLayer.mergeTiles for '{buildLayer.layerName}' because meshGenerationOverride was forcing merged cluster meshes.");
            buildLayer.mergeTiles = false;
        }

        private static void NormalizeTileLayerHeights(TilesBuildLayer buildLayer)
        {
            buildLayer.tileLayers ??= new List<TilesBuildLayer.TileLayers>();
            if (buildLayer.tileLayers.Count == 0)
                buildLayer.tileLayers.Add(new TilesBuildLayer.TileLayers());

            for (int i = 0; i < buildLayer.tileLayers.Count; i++)
            {
                buildLayer.tileLayers[i] ??= new TilesBuildLayer.TileLayers();
                buildLayer.tileLayers[i].heightOffset = 0f;
            }
        }

        private void LogInvalidBuildLayerOnce(TileWorldCreatorIdMappingSO.LayerMapping mapping, string reason)
        {
            string key = $"{mapping?.IdPattern}:{reason}";
            if (_loggedInvalidBuildLayers.Add(key))
                Debug.LogWarning($"{LogTag} Cannot prepare TWC terrain layer for ID pattern '{mapping?.IdPattern}': {reason}.");
        }
    }
}
