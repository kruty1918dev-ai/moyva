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
        private string BuildSummaryLog(
            bool isInitialBuild,
            bool contextChanged,
            bool wasFullRebuild,
            int requestedDirtyTiles)
        {
            var settings = GetSettings();
            var volume = settings?.Volume;
            int totalCells = Mathf.Max(1, _mapWidth * _mapHeight);
            int visibleCells = Mathf.Max(0, totalCells - _stateCache.UnexploredCellCount - _stateCache.ExploredCellCount);

            var sb = new StringBuilder(2048);
            sb.Append(LogTag)
                .Append(" TWC fog volume summary")
                .Append(" | operation=").Append(isInitialBuild ? "initial-build" : "update")
                .Append(" | rebuild=").Append(wasFullRebuild ? "full" : "dirty")
                .Append(" | dirtyRequested=").Append(requestedDirtyTiles)
                .Append(" | contextChanged=").Append(contextChanged)
                .Append(" | updateMode=").Append(_visualUpdateScheduleState.CurrentUpdateMode)
                .Append(" | interval=").Append(_visualUpdateScheduleState.CurrentIntervalSeconds.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)).Append('s')
                .AppendLine();

            sb.Append("  Scene: controller='").Append(_controller != null ? _controller.name : "null")
                .Append("', manager='").Append(_manager != null ? _manager.name : "null")
                .Append("', settings='").Append(settings != null ? settings.name : "null")
                .Append("', runtimeConfig='").Append(_runtimeConfiguration != null ? _runtimeConfiguration.name : "null")
                .AppendLine("'");

            sb.Append("  World: map=").Append(_mapWidth).Append('x').Append(_mapHeight)
                .Append(", cellSize=").Append((_runtimeConfiguration != null ? _runtimeConfiguration.cellSize : _context.CellSize).ToString("0.###", System.Globalization.CultureInfo.InvariantCulture))
                .Append(", projection=").Append(_context.ProjectionMode)
                .Append(", render=").Append(_context.RenderMode)
                .Append(", topology=").Append(_context.GridTopology)
                .Append(", bounds=").Append(FormatBounds(_context))
                .AppendLine();

            sb.Append("  Height: source=").Append(volume != null ? volume.HeightSource : FogVolumeHeightSource.TerrainLevelMapThenHeightMap)
                .Append(", heightMap=").Append(FormatMapSize(_context.HeightMap))
                .Append(", terrainLevelMap=").Append(FormatMapSize(_context.TerrainLevelMap))
                .Append(", snap=").Append(ResolveEffectiveHeightLayerSnap().ToString("0.###", System.Globalization.CultureInfo.InvariantCulture))
                .Append(", configuredSnap=").Append(ResolveConfiguredHeightLayerSnap().ToString("0.###", System.Globalization.CultureInfo.InvariantCulture))
                .Append(", clearance=").Append(((volume != null ? volume.TopClearance : 0.08f) + (_controller != null ? _controller.AdditionalTopClearance : 0f)).ToString("0.###", System.Globalization.CultureInfo.InvariantCulture))
                .Append(", heightKeys=").Append(_heightByKey.Count)
                .Append(FormatHeightRange())
                .AppendLine();

            sb.Append("  Fog Cells: total=").Append(totalCells)
                .Append(", unexplored=").Append(_stateCache.UnexploredCellCount)
                .Append(", explored=").Append(_stateCache.ExploredCellCount)
                .Append(", visible=").Append(visibleCells)
                .Append(", unexploredHeightLayers=").Append(_stateCache.CountNonEmptyHeightLayers(_stateCache.UnexploredCellsByHeight))
                .Append(", exploredHeightLayers=").Append(_stateCache.CountNonEmptyHeightLayers(_stateCache.ExploredCellsByHeight))
                .AppendLine();

            sb.Append("  State Config: ")
                .Append(FormatStateSettings("Unexplored", volume?.Unexplored))
                .Append(" | ")
                .Append(FormatStateSettings("Explored", volume?.Explored))
                .AppendLine();

            sb.Append("  Runtime Layers: count=").Append(_runtimeLayers.Count);
            for (int i = 0; i < _runtimeLayers.Count; i++)
            {
                var layer = _runtimeLayers[i];
                sb.AppendLine()
                    .Append("    - ").Append(layer.State)
                    .Append(" name='").Append(layer.BuildLayer != null ? layer.BuildLayer.layerName : "null")
                    .Append("', heightKey=").Append(layer.HeightKey)
                    .Append(", height=").Append(ResolveLayerHeight(layer.HeightKey).ToString("0.###", System.Globalization.CultureInfo.InvariantCulture))
                    .Append(", cells=").Append(ResolveCells(layer)?.Count ?? 0)
                    .Append(", dualGrid=").Append(layer.BuildLayer != null && layer.BuildLayer.useDualGrid)
                    .Append(", scaleToCell=").Append(layer.BuildLayer != null && layer.BuildLayer.scaleTileToCellSize)
                    .Append(", collider=").Append(layer.BuildLayer != null ? layer.BuildLayer.colliderType.ToString() : "null")
                    .Append(", presets=").Append(CountBuildLayerPresets(layer.BuildLayer));
            }

            return sb.ToString();
        }

        private static string FormatBounds(FogWorldVisualContext context)
        {
            if (!context.HasMapWorldBounds)
                return "none";

            var bounds = context.MapWorldBounds;
            return $"center={FormatVector(bounds.center)}, size={FormatVector(bounds.size)}";
        }

        private static string FormatVector(Vector3 value)
        {
            return $"({value.x.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}, {value.y.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}, {value.z.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)})";
        }

        private static string FormatMapSize(float[,] map)
            => map != null ? $"{map.GetLength(0)}x{map.GetLength(1)}" : "null";

        private static string FormatMapSize(int[,] map)
            => map != null ? $"{map.GetLength(0)}x{map.GetLength(1)}" : "null";

        private string FormatHeightRange()
        {
            if (_heightByKey.Count == 0)
                return ", range=none";

            bool hasValue = false;
            float min = 0f;
            float max = 0f;
            foreach (var height in _heightByKey.Values)
            {
                if (!hasValue)
                {
                    min = height;
                    max = height;
                    hasValue = true;
                    continue;
                }

                min = Mathf.Min(min, height);
                max = Mathf.Max(max, height);
            }

            return $", range={min.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}..{max.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}";
        }

        private static string FormatStateSettings(string label, FogVolumeStateTileSettings settings)
        {
            if (settings == null)
                return $"{label}=null";

            CountPresetVariants(settings, out int total, out int assigned, out int usable);
            return $"{label}[enabled={settings.Enabled}, layer='{settings.LayerName}', variants={total}, assigned={assigned}, usableDualGrid={usable}]";
        }

        private static void CountPresetVariants(FogVolumeStateTileSettings settings, out int total, out int assigned, out int usable)
        {
            total = 0;
            assigned = 0;
            usable = 0;
            if (settings?.TileVariants == null)
                return;

            total = settings.TileVariants.Count;
            for (int i = 0; i < settings.TileVariants.Count; i++)
            {
                var variant = settings.TileVariants[i];
                if (variant?.Preset == null)
                    continue;

                assigned++;
                if (FogOfWarSettings.HasUsableDualGridPreset(variant.Preset))
                    usable++;
            }
        }

        private static int CountBuildLayerPresets(TilesBuildLayer buildLayer)
        {
            if (buildLayer == null)
                return 0;

            return (buildLayer.tilePresetsTop?.Count ?? 0)
                + (buildLayer.tilePresetsMiddle?.Count ?? 0)
                + (buildLayer.tilePresetsBottom?.Count ?? 0);
        }

        private void CountAuthoritativeFogStates(
            IFogOfWarService fogService,
            out int visible,
            out int explored,
            out int unexplored,
            out int dirtyVisible,
            out int dirtyExplored,
            out int dirtyUnexplored,
            out int dirtyOutOfBounds)
        {
            visible = 0;
            explored = 0;
            unexplored = 0;
            dirtyVisible = 0;
            dirtyExplored = 0;
            dirtyUnexplored = 0;
            dirtyOutOfBounds = 0;

            if (fogService == null)
                return;

            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    switch (fogService.GetFogState(new Vector2Int(x, y)))
                    {
                        case FogStateType.Visible:
                            visible++;
                            break;
                        case FogStateType.Explored:
                            explored++;
                            break;
                        default:
                            unexplored++;
                            break;
                    }
                }
            }

            foreach (var tile in _pendingWorkState.PendingDirtyTiles)
            {
                if (!IsInBounds(tile))
                {
                    dirtyOutOfBounds++;
                    continue;
                }

                switch (fogService.GetFogState(tile))
                {
                    case FogStateType.Visible:
                        dirtyVisible++;
                        break;
                    case FogStateType.Explored:
                        dirtyExplored++;
                        break;
                    default:
                        dirtyUnexplored++;
                        break;
                }
            }
        }

        private string FormatPendingDirtySamples(IFogOfWarService fogService, int maxSamples = 8)
        {
            if (_pendingWorkState.DirtyTileCount == 0)
                return "none";

            var sb = new StringBuilder();
            int count = 0;
            foreach (var tile in _pendingWorkState.PendingDirtyTiles)
            {
                if (count > 0)
                    sb.Append(", ");

                sb.Append(tile)
                    .Append('=')
                    .Append(fogService != null ? fogService.GetFogState(tile).ToString() : "no-fog-service");

                count++;
                if (count >= maxSamples)
                    break;
            }

            if (_pendingWorkState.DirtyTileCount > count)
                sb.Append(", ...");

            return sb.ToString();
        }

        private string FormatRuntimeLayerSummary()
        {
            if (_runtimeLayers.Count == 0)
                return "none";

            var sb = new StringBuilder();
            for (int i = 0; i < _runtimeLayers.Count; i++)
            {
                if (i > 0)
                    sb.Append(" | ");

                var layer = _runtimeLayers[i];
                sb.Append(layer.State)
                    .Append(":heightKey=").Append(layer.HeightKey)
                    .Append(",cells=").Append(ResolveCells(layer)?.Count ?? 0)
                    .Append(",enabled=").Append(layer.BuildLayer != null && layer.BuildLayer.isEnabled)
                    .Append(",presets=").Append(CountBuildLayerPresets(layer.BuildLayer));
            }

            return sb.ToString();
        }

        private bool TryFindVisibleDirtyCacheMismatch(IFogOfWarService fogService, out Vector2Int tile, out string cache)
        {
            tile = default;
            cache = null;

            if (fogService == null)
                return false;

            foreach (var dirtyTile in _pendingWorkState.PendingDirtyTiles)
            {
                if (!IsInBounds(dirtyTile) || fogService.GetFogState(dirtyTile) != FogStateType.Visible)
                    continue;

                if (_stateCache.HasUnexploredCell(dirtyTile))
                {
                    tile = dirtyTile;
                    cache = "unexplored";
                    return true;
                }

                if (_stateCache.HasExploredCell(dirtyTile))
                {
                    tile = dirtyTile;
                    cache = "explored";
                    return true;
                }
            }

            return false;
        }

        private int CountGeneratedClusters()
        {
            return _manager != null
                ? _manager.GetComponentsInChildren<ClusterIdentifier>(true).Length
                : 0;
        }

        private int CountLayerObjects()
        {
            return _manager != null
                ? _manager.GetComponentsInChildren<LayerIdentifier>(true).Length
                : 0;
        }

        private int CountGeneratedOutputChildren()
            => _manager != null ? _manager.transform.childCount : 0;

        private int ClearGeneratedOutputBeforeBuild()
        {
            if (_manager == null || _outputCleaner == null)
                return 0;

            int removed = _outputCleaner.ClearGeneratedChildren(_manager, forceImmediate: true);
            ClearRuntimeBuildLayerClusterCaches();
            return removed;
        }

        private bool StopPendingTileWorldBuildCoroutines(int generatedChildrenBeforeClear, int layerObjectsBeforeBuild)
        {
            if (_manager == null || !Application.isPlaying)
                return false;

            if (!_hasBuiltAtLeastOnce && generatedChildrenBeforeClear <= 0 && layerObjectsBeforeBuild <= 0)
                return false;

            _manager.StopAllCoroutines();
            return true;
        }

        private void ClearRuntimeBuildLayerClusterCaches()
        {
            for (int i = 0; i < _runtimeLayers.Count; i++)
                _runtimeLayers[i]?.BuildLayer?.availableClusters?.Clear();
        }

        private void RequestVisualRebuild()
        {
            _pendingWorkRequests.RequestFullRebuildWhenFogServiceAvailable();
            LogUpdaterOnce(ref _loggedRebuildRequest, $"RequestVisualRebuild: hasLastFogService={_pendingWorkState.FogService != null}, pending={_pendingWorkState.HasPendingWork}, controller={(_controller != null ? _controller.name : "null")}, manager={(_manager != null ? _manager.name : "null")}.");
        }

        private void LogUpdaterOnce(ref bool logged, string message)
        {
            if (!ShouldLogLifecycle(logged))
                return;

            logged = true;
            Debug.Log($"{LogTag} {message}");
        }

        private bool ShouldLogLifecycle(bool alreadyLogged)
        {
            if (_controller != null)
            {
                if (!_controller.LogBuildSummary && !_controller.LogValidationWarnings)
                    return false;

                return !alreadyLogged || _controller.LogEveryVolumeUpdate;
            }

            return !alreadyLogged;
        }

    }
}
