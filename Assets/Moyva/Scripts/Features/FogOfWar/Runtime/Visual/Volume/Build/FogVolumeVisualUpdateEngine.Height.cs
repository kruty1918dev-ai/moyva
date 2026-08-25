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
        private FogOfWarSettings GetSettings()
            => _controller != null && _controller.Settings != null ? _controller.Settings : _injectedSettings;

        private FogVolumeUpdateMode ResolveUpdateMode()
            => _controller != null ? _controller.EffectiveUpdateMode : (GetSettings()?.Volume.UpdateMode ?? FogVolumeUpdateMode.DebouncePerFrame);

        private float ResolveRebuildIntervalSeconds()
            => _controller != null ? _controller.EffectiveRebuildIntervalSeconds : Mathf.Max(0.02f, GetSettings()?.Volume.RebuildIntervalSeconds ?? 0.1f);

        private bool ShouldUseClusteredRuntimeRenderer()
            => GetSettings()?.Volume.UseClusteredRuntimeFogRenderer ?? false;

        private bool ResolveAllowFullRebuildFallback()
            => GetSettings()?.Volume.AllowFullRebuildFallback ?? true;

        private float ResolveFullRebuildDirtyClusterRatioThreshold()
            => Mathf.Clamp(GetSettings()?.Volume.FullRebuildDirtyClusterRatioThreshold ?? 0.35f, 0.01f, 1f);

        private float ResolveDirtyClusterRatio(int dirtyClusterCount)
        {
            int clusterSize = Mathf.Max(1, GetSettings()?.Volume.ClusterSize ?? 16);
            int clusterCountX = Mathf.Max(1, Mathf.CeilToInt(_mapWidth / (float)clusterSize));
            int clusterCountY = Mathf.Max(1, Mathf.CeilToInt(_mapHeight / (float)clusterSize));
            int totalComparableClusters = Mathf.Max(1, clusterCountX * clusterCountY);
            return Mathf.Clamp01(dirtyClusterCount / (float)totalComparableClusters);
        }

        private int ResolveHeightKey(Vector2Int tile)
        {
            float height = ResolveGeneratedSurfaceHeight(tile);
            float snap = ResolveEffectiveHeightLayerSnap();
            int key = Mathf.RoundToInt(height / snap);
            if (_heightByKey.TryGetValue(key, out float existingHeight))
            {
                if (height > existingHeight)
                    _heightByKey[key] = height;
            }
            else
            {
                _heightByKey.Add(key, height);
            }

            return key;
        }

        private float ResolveEffectiveHeightLayerSnap()
        {
            if (_cachedEffectiveHeightLayerSnap > 0f)
                return _cachedEffectiveHeightLayerSnap;

            float configuredSnap = ResolveConfiguredHeightLayerSnap();
            if (!TryResolveGeneratedHeightRange(out float minHeight, out float maxHeight))
            {
                _cachedEffectiveHeightLayerSnap = configuredSnap;
                return _cachedEffectiveHeightLayerSnap;
            }

            float heightRange = Mathf.Max(0f, maxHeight - minHeight);
            float budgetedSnap = heightRange > 0f
                ? heightRange / Mathf.Max(1, TargetFogHeightLayerBudget - 1)
                : configuredSnap;

            _cachedEffectiveHeightLayerSnap = Mathf.Max(configuredSnap, budgetedSnap);
            return _cachedEffectiveHeightLayerSnap;
        }

        private float ResolveConfiguredHeightLayerSnap()
            => Mathf.Max(0.001f, GetSettings()?.Volume.HeightLayerSnap ?? 0.01f);

        private bool TryResolveGeneratedHeightRange(out float minHeight, out float maxHeight)
        {
            minHeight = 0f;
            maxHeight = 0f;
            bool hasHeight = false;

            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    float height = ResolveGeneratedSurfaceHeight(new Vector2Int(x, y));
                    if (!IsFinite(height))
                        continue;

                    if (!hasHeight)
                    {
                        minHeight = height;
                        maxHeight = height;
                        hasHeight = true;
                        continue;
                    }

                    minHeight = Mathf.Min(minHeight, height);
                    maxHeight = Mathf.Max(maxHeight, height);
                }
            }

            return hasHeight;
        }

        private float ResolveLayerHeight(int heightKey)
        {
            if (!_heightByKey.TryGetValue(heightKey, out float surfaceHeight))
                surfaceHeight = 0f;

            float clearance = GetSettings()?.Volume.TopClearance ?? 0.08f;
            if (_controller != null)
                clearance += _controller.AdditionalTopClearance;

            float managerY = _manager != null ? _manager.transform.position.y : 0f;
            return surfaceHeight + Mathf.Max(0f, clearance) - managerY;
        }

        private float ResolveGeneratedSurfaceHeight(Vector2Int tile)
        {
            var settings = GetSettings()?.Volume;
            var source = settings?.HeightSource ?? FogVolumeHeightSource.TerrainLevelMapThenHeightMap;

            switch (source)
            {
                case FogVolumeHeightSource.HeightMapThenTerrainLevelMap:
                    if (TryResolveHeightMapValue(tile, out float heightMapValue))
                        return heightMapValue;
                    if (TryResolveTerrainLevelValue(tile, out float terrainHeightValue))
                        return terrainHeightValue;
                    break;
                case FogVolumeHeightSource.Flat:
                    return 0f;
                default:
                    if (TryResolveTerrainLevelValue(tile, out terrainHeightValue))
                        return terrainHeightValue;
                    if (TryResolveHeightMapValue(tile, out heightMapValue))
                        return heightMapValue;
                    break;
            }

            return 0f;
        }

        private bool TryResolveTerrainLevelValue(Vector2Int tile, out float height)
        {
            height = 0f;
            if (_context.TerrainLevelMap == null
                || tile.x < 0
                || tile.y < 0
                || tile.x >= _context.TerrainLevelMap.GetLength(0)
                || tile.y >= _context.TerrainLevelMap.GetLength(1))
            {
                return false;
            }

            float step = Mathf.Max(0.001f, GetSettings()?.Volume.TerrainLevelHeightStep ?? 1f);
            height = Mathf.Max(0, _context.TerrainLevelMap[tile.x, tile.y]) * step;
            return IsFinite(height);
        }

        private bool TryResolveHeightMapValue(Vector2Int tile, out float height)
        {
            height = 0f;
            if (_context.HeightMap == null
                || tile.x < 0
                || tile.y < 0
                || tile.x >= _context.HeightMap.GetLength(0)
                || tile.y >= _context.HeightMap.GetLength(1))
            {
                return false;
            }

            height = _context.HeightMap[tile.x, tile.y];
            return IsFinite(height);
        }

        private static void ConfigureManagerTransform(Transform managerTransform, FogWorldVisualContext context, float cellSize)
        {
            if (managerTransform == null || !context.HasMapWorldBounds)
                return;

            Bounds bounds = context.MapWorldBounds;
            float halfCell = Mathf.Max(0.0001f, cellSize) * 0.5f;
            managerTransform.position = new Vector3(
                bounds.min.x + halfCell,
                managerTransform.position.y,
                bounds.min.z + halfCell);
        }

    }
}
