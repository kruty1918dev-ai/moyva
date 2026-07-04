using System;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{

    internal readonly struct FogVolumeHeightSampler
    {
        private const int TargetFogHeightLayerBudget = 8;
        private readonly FogWorldVisualContext _context;
        private readonly FogOfWarSettings _settings;
        private readonly float _effectiveSnap;

        public FogVolumeHeightSampler(FogWorldVisualContext context, FogOfWarSettings settings)
        {
            _context = context;
            _settings = settings;
            _effectiveSnap = ResolveEffectiveHeightLayerSnap(context, settings);
        }

        public int ResolveHeightKey(Vector2Int tile)
        {
            float height = ResolveGeneratedSurfaceHeight(tile);
            return Mathf.RoundToInt(height / Mathf.Max(0.001f, _effectiveSnap));
        }

        public float ResolveWorldHeight(int heightKey, FogVolumeStateTileSettings stateSettings)
        {
            float clearance = _settings?.Volume.TopClearance ?? 0.08f;
            float layerOffset = stateSettings != null ? stateSettings.LayerYOffset : 0f;
            return heightKey * Mathf.Max(0.001f, _effectiveSnap) + Mathf.Max(0f, clearance) + layerOffset;
        }

        public float ResolveGeneratedSurfaceHeight(Vector2Int tile)
        {
            var volume = _settings?.Volume;
            var source = volume?.HeightSource ?? FogVolumeHeightSource.TerrainLevelMapThenHeightMap;
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

            float step = Mathf.Max(0.001f, _settings?.Volume.TerrainLevelHeightStep ?? 1f);
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

        private static float ResolveEffectiveHeightLayerSnap(FogWorldVisualContext context, FogOfWarSettings settings)
        {
            float configuredSnap = Mathf.Max(0.001f, settings?.Volume.HeightLayerSnap ?? 0.01f);
            if (!TryResolveGeneratedHeightRange(context, settings, out float minHeight, out float maxHeight))
                return configuredSnap;

            float heightRange = Mathf.Max(0f, maxHeight - minHeight);
            float budgetedSnap = heightRange > 0f
                ? heightRange / Mathf.Max(1, TargetFogHeightLayerBudget - 1)
                : configuredSnap;

            return Mathf.Max(configuredSnap, budgetedSnap);
        }

        private static bool TryResolveGeneratedHeightRange(
            FogWorldVisualContext context,
            FogOfWarSettings settings,
            out float minHeight,
            out float maxHeight)
        {
            minHeight = 0f;
            maxHeight = 0f;
            if (!context.IsValid)
                return false;

            bool hasHeight = false;
            var sampler = new FogVolumeHeightSampler(context, settings, Mathf.Max(0.001f, settings?.Volume.HeightLayerSnap ?? 0.01f));
            for (int y = 0; y < context.Height; y++)
            {
                for (int x = 0; x < context.Width; x++)
                {
                    float height = sampler.ResolveGeneratedSurfaceHeight(new Vector2Int(x, y));
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

        private FogVolumeHeightSampler(FogWorldVisualContext context, FogOfWarSettings settings, float effectiveSnap)
        {
            _context = context;
            _settings = settings;
            _effectiveSnap = effectiveSnap;
        }

        private static bool IsFinite(float value)
            => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
