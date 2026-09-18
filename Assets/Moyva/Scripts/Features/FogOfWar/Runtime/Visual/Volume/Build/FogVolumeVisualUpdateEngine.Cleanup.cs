using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed partial class FogVolumeVisualUpdateEngine
    {
        private void DisposeRuntimeConfiguration()
        {
            if (_manager != null && _manager.configuration == _runtimeConfiguration)
                _manager.configuration = _previousManagerConfiguration;

            for (int i = 0; i < _runtimeLayers.Count; i++)
            {
                DestroyRuntimeObject(_runtimeLayers[i]?.BuildLayer);
                DestroyRuntimeObject(_runtimeLayers[i]?.BlueprintLayer);
            }

            DestroyRuntimeObject(_runtimeConfiguration);

            _runtimeLayers.Clear();
            _runtimeConfiguration = null;
        }

        private static void DestroyRuntimeObject(UnityEngine.Object obj)
        {
            if (obj == null)
                return;

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(obj);
            else
                UnityEngine.Object.DestroyImmediate(obj);
        }

        private bool IsInBounds(Vector2Int tile)
            => tile.x >= 0 && tile.x < _mapWidth && tile.y >= 0 && tile.y < _mapHeight;

        private static bool IsStateEnabled(FogVolumeStateTileSettings settings, bool fallback)
            => settings != null ? settings.Enabled : fallback;

        private static FogWorldVisualContext CreateFallbackContext(int width, int height)
        {
            return new FogWorldVisualContext(
                width,
                height,
                Kruty1918.Moyva.Grid.API.GridTopology.Orthogonal,
                Kruty1918.Moyva.Grid.API.GridProjectionMode.Orthographic3D,
                Kruty1918.Moyva.Grid.API.GridRenderMode.Mesh3D,
                Kruty1918.Moyva.Grid.API.GridNeighborhoodMode.Moore8,
                1f,
                false,
                default,
                null,
                null);
        }

        private static bool ApproximatelyBounds(Bounds a, Bounds b)
            => ApproximatelyVector(a.center, b.center) && ApproximatelyVector(a.size, b.size);

        private static bool ApproximatelyVector(Vector3 a, Vector3 b)
            => Mathf.Approximately(a.x, b.x)
                && Mathf.Approximately(a.y, b.y)
                && Mathf.Approximately(a.z, b.z);

        private static bool IsFinite(float value)
            => !float.IsNaN(value) && !float.IsInfinity(value);

        private static string FormatHeightKey(int heightKey)
            => heightKey.ToString(System.Globalization.CultureInfo.InvariantCulture).Replace("-", "m");

        private void LogMissingSettingsOnce()
        {
            if (_loggedMissingSettings)
                return;

            _loggedMissingSettings = true;
            Debug.LogError($"{LogTag} FogOfWarSettings is missing. Runtime fog state can still update, but TWC fog volume has no configured TilePresets.");
        }

        private void LogRuntimeLayerValidation()
        {
            var volume = GetSettings()?.Volume;
            bool unexploredNeedsPreset = _stateCache.UnexploredCellCount > 0 && IsStateEnabled(volume?.Unexplored, true);
            bool exploredNeedsPreset = _stateCache.ExploredCellCount > 0 && IsStateEnabled(volume?.Explored, true);

            if (unexploredNeedsPreset && !HasUsablePreset(volume?.Unexplored) && !_loggedUnexploredPresetProblem)
            {
                _loggedUnexploredPresetProblem = true;
                Debug.LogError($"{LogTag} Unexplored fog has {_stateCache.UnexploredCellCount} cells, but no usable dual-grid TilePreset is configured. Assign at least one preset in FogOfWarSettings > TWC Volume > Unexplored Fog.");
            }

            if (exploredNeedsPreset && !HasUsablePreset(volume?.Explored) && !_loggedExploredPresetProblem)
            {
                _loggedExploredPresetProblem = true;
                Debug.LogError($"{LogTag} Explored fog has {_stateCache.ExploredCellCount} cells, but no usable dual-grid TilePreset is configured. Assign at least one preset in FogOfWarSettings > TWC Volume > Explored Fog.");
            }

        }

        private void ClearGeneratedOutputBeforeBuild()
        {
            if (_manager == null || _outputCleaner == null)
                return;

            _outputCleaner.ClearGeneratedChildren(
                _manager,
                forceImmediate: true);

            ClearRuntimeBuildLayerClusterCaches();
        }

        private void StopPendingTileWorldBuildCoroutines()
        {
            if (_manager == null || !Application.isPlaying)
                return;

            if (!_hasBuiltAtLeastOnce
                && _manager.transform.childCount == 0
                && _manager.GetComponentsInChildren<LayerIdentifier>(true).Length == 0)
            {
                return;
            }

            _manager.StopAllCoroutines();
        }

        private void ClearRuntimeBuildLayerClusterCaches()
        {
            for (int i = 0; i < _runtimeLayers.Count; i++)
                _runtimeLayers[i]?.BuildLayer?.availableClusters?.Clear();
        }

        private void RequestVisualRebuild()
            => _pendingWorkRequests.RequestFullRebuildWhenFogServiceAvailable();

        private static bool HasUsablePreset(FogVolumeStateTileSettings settings)
        {
            if (settings?.TileVariants == null)
                return false;

            for (int i = 0; i < settings.TileVariants.Count; i++)
            {
                var variant = settings.TileVariants[i];
                if (variant?.Preset != null && FogOfWarSettings.HasUsableDualGridPreset(variant.Preset))
                    return true;
            }

            return false;
        }

    }
}
