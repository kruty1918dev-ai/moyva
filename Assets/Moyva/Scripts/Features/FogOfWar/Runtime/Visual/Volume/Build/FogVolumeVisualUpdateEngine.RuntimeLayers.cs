using System;
using System.Collections.Generic;
using System.Text;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed partial class FogVolumeVisualUpdateEngine
    {
        private bool EnsureRuntimeConfiguration()
        {
            if (_controller == null)
                return false;

            _manager = _controller.TileWorldCreatorManager;
            if (_manager == null)
                return false;

            if (GetSettings() == null)
                LogMissingSettingsOnce();

            if (!_runtimeConfigurationDirty && _runtimeConfiguration != null)
                return true;

            DisposeRuntimeConfiguration();
            _runtimeConfiguration = CreateRuntimeConfiguration(_context, _controller);

            _runtimeConfiguration.blueprintLayerFolders.Add(new BlueprintLayerFolder(FolderName));
            _runtimeConfiguration.buildLayerFolders.Add(new BuildLayerFolder(FolderName));
            CreateRuntimeLayers(
                _runtimeConfiguration,
                _stateCache.UnexploredCellsByHeight,
                FogRuntimeState.Unexplored,
                GetSettings()?.Volume?.Unexplored,
                "Fog_Unexplored");
            CreateRuntimeLayers(
                _runtimeConfiguration,
                _stateCache.ExploredCellsByHeight,
                FogRuntimeState.Explored,
                GetSettings()?.Volume?.Explored,
                "Fog_Explored");

            LogRuntimeLayerValidation();
            _manager.configuration = _runtimeConfiguration;
            ConfigureManagerTransform(_manager.transform, _context, _runtimeConfiguration.cellSize);
            _runtimeConfigurationDirty = false;
            return true;
        }

        private static Configuration CreateRuntimeConfiguration(
            FogWorldVisualContext context,
            FogOfWarVolumeController controller)
        {
            var configuration = ScriptableObject.CreateInstance<Configuration>();
            configuration.name = "FogOfWar_RuntimeConfiguration";
            configuration.width = Mathf.Max(1, context.Width);
            configuration.height = Mathf.Max(1, context.Height);
            configuration.cellSize = controller != null ? controller.ResolveCellSize(context.CellSize) : Mathf.Max(0.0001f, context.CellSize);
            configuration.lastCellSize = configuration.cellSize;
            configuration.clusterCellSize = MinimumFogClusterCellSize;
            configuration.useGlobalRandomSeed = true;
            configuration.globalRandomSeed = 1;
            configuration.currentRandomSeed = 1u;
            configuration.useParallel = false;
            configuration.mergeTiles = true;
            configuration.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            configuration.objectLayer = 0;
            configuration.renderingLayer = 1;
            configuration.colliderType = Configuration.ColliderType.none;
            configuration.tileColliderHeight = 0f;
            configuration.tileColliderExtrusionHeight = 0f;
            configuration.invertCollisionWalls = false;
            configuration.blueprintLayerFolders = new List<BlueprintLayerFolder>();
            configuration.buildLayerFolders = new List<BuildLayerFolder>();
            configuration.zeroLayerPaddingBlueprintLayerGuids = new List<string>();
            configuration.tileMapLayers = new List<BlueprintLayer>();
            return configuration;
        }

        private void CreateRuntimeLayers(
            Configuration configuration,
            IReadOnlyDictionary<int, HashSet<Vector2>> cellsByHeight,
            FogRuntimeState state,
            FogVolumeStateTileSettings settings,
            string fallbackLayerName)
        {
            if (cellsByHeight == null)
                return;

            var heightKeys = new List<int>(cellsByHeight.Keys);
            heightKeys.Sort();
            for (int i = 0; i < heightKeys.Count; i++)
            {
                int heightKey = heightKeys[i];
                if (!cellsByHeight.TryGetValue(heightKey, out var cells) || cells == null || cells.Count == 0)
                    continue;

                var runtimeLayer = CreateRuntimeLayer(
                    configuration,
                    settings,
                    $"{fallbackLayerName}_{FormatHeightKey(heightKey)}",
                    ResolveLayerHeight(heightKey),
                    state,
                    heightKey);
                _runtimeLayers.Add(runtimeLayer);
                configuration.blueprintLayerFolders[0].blueprintLayers.Add(runtimeLayer.BlueprintLayer);
                configuration.buildLayerFolders[0].buildLayers.Add(runtimeLayer.BuildLayer);
            }
        }

        private RuntimeLayer CreateRuntimeLayer(
            Configuration configuration,
            FogVolumeStateTileSettings settings,
            string fallbackLayerName,
            float defaultLayerHeight,
            FogRuntimeState state,
            int heightKey)
        {
            settings ??= new FogVolumeStateTileSettings();
            settings.EnsureDefaults(fallbackLayerName);

            var blueprintLayer = ScriptableObject.CreateInstance<BlueprintLayer>();
            blueprintLayer.name = fallbackLayerName;
            blueprintLayer.layerName = fallbackLayerName;
            blueprintLayer.isEnabled = settings.Enabled;
            blueprintLayer.defaultLayerHeight = defaultLayerHeight;

            var buildLayer = ScriptableObject.CreateInstance<TilesBuildLayer>();
            buildLayer.name = fallbackLayerName;
            buildLayer.layerName = fallbackLayerName;
            buildLayer.isEnabled = settings.Enabled;
            buildLayer.configuration = configuration;
            buildLayer.currentBlueprintLayer = blueprintLayer;
            buildLayer.SetBlueprintLayer(blueprintLayer);
            buildLayer.useDualGrid = true;
            buildLayer.scaleTileToCellSize = true;
            buildLayer.layerYOffset = settings.LayerYOffset;
            buildLayer.scaleOffset = ResolveRuntimeFogScaleOffset(settings);
            buildLayer.generateFlatSurface = false;
            buildLayer.meshGenerationOverride = true;
            buildLayer.mergeTiles = true;
            buildLayer.shadowCastingMode = settings.ShadowCastingMode;
            buildLayer.objectLayer = settings.ObjectLayer;
            buildLayer.renderingLayer = settings.RenderingLayer;
            buildLayer.colliderType = settings.ColliderType;
            buildLayer.tileColliderHeight = Mathf.Max(0f, settings.TileColliderHeight);
            buildLayer.tileColliderExtrusionHeight = Mathf.Max(0f, settings.TileColliderExtrusionHeight);
            buildLayer.invertCollisionWalls = settings.InvertCollisionWalls;
            ApplyPresetSelections(buildLayer, settings);
            EnsureTileLayer(buildLayer);
            return new RuntimeLayer(blueprintLayer, buildLayer, state, heightKey);
        }

        private Vector3 ResolveRuntimeFogScaleOffset(FogVolumeStateTileSettings settings)
        {
            Vector3 scale = settings != null ? settings.ScaleOffset : Vector3.one;
            float horizontalScale = 1f + Mathf.Clamp(GetSettings()?.Volume.HorizontalTileOverlap ?? 0.03f, 0f, 0.25f);
            scale.x = Mathf.Max(scale.x, horizontalScale);
            scale.z = Mathf.Max(scale.z, horizontalScale);
            return scale;
        }

        private static void ApplyPresetSelections(TilesBuildLayer buildLayer, FogVolumeStateTileSettings settings)
        {
            buildLayer.tilePresetsTop ??= new List<TilesBuildLayer.TilePresetSelection>();
            buildLayer.tilePresetsMiddle ??= new List<TilesBuildLayer.TilePresetSelection>();
            buildLayer.tilePresetsBottom ??= new List<TilesBuildLayer.TilePresetSelection>();
            buildLayer.tilePresetsTop.Clear();
            buildLayer.tilePresetsMiddle.Clear();
            buildLayer.tilePresetsBottom.Clear();

            if (settings?.TileVariants == null)
                return;

            for (int i = 0; i < settings.TileVariants.Count; i++)
            {
                var variant = settings.TileVariants[i];
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
                    case FogVolumeTilePresetSlot.Middle:
                        buildLayer.tilePresetsMiddle.Add(selection);
                        break;
                    case FogVolumeTilePresetSlot.Bottom:
                        buildLayer.tilePresetsBottom.Add(selection);
                        break;
                    default:
                        buildLayer.tilePresetsTop.Add(selection);
                        break;
                }
            }
        }

        private static void EnsureTileLayer(TilesBuildLayer buildLayer)
        {
            buildLayer.tileLayers ??= new List<TilesBuildLayer.TileLayers>();
            if (buildLayer.tileLayers.Count == 0)
                buildLayer.tileLayers.Add(new TilesBuildLayer.TileLayers());

            var first = buildLayer.tileLayers[0] ?? new TilesBuildLayer.TileLayers();
            first.name = string.IsNullOrWhiteSpace(first.name) ? "Main" : first.name;
            first.ignoreFillTiles = false;
            first.layerOverrides ??= new List<TilesBuildLayer.TilePresetOverride>();
            buildLayer.tileLayers[0] = first;
        }

        private void ApplyCellsToLayer(BlueprintLayer layer, HashSet<Vector2> cells)
        {
            if (layer == null)
                return;

            layer.ClearLayer(false);
            if (cells == null || cells.Count == 0)
                return;

            _scratchCells.Clear();
            foreach (var cell in cells)
                _scratchCells.Add(cell);

            layer.AddCells(_scratchCells);
            _scratchCells.Clear();
        }

        private void ApplyCellsToRuntimeLayers()
        {
            int visibleCells = Mathf.Max(0, _mapWidth * _mapHeight - _stateCache.UnexploredCellCount - _stateCache.ExploredCellCount);
            for (int i = 0; i < _runtimeLayers.Count; i++)
            {
                var runtimeLayer = _runtimeLayers[i];
                ApplyCellsToLayer(runtimeLayer.BlueprintLayer, ResolveCells(runtimeLayer));
            }
        }

        private HashSet<Vector2> ResolveCells(RuntimeLayer runtimeLayer)
        {
            if (runtimeLayer == null)
                return null;

            return runtimeLayer.State == FogRuntimeState.Unexplored
                ? _stateCache.ResolveUnexploredCells(runtimeLayer.HeightKey)
                : _stateCache.ResolveExploredCells(runtimeLayer.HeightKey);
        }

    }
}
