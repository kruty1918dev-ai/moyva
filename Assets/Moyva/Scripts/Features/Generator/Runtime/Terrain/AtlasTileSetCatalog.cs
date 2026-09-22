using System;
using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.JsonConfig;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Resolved view of one atlas theme: the five high prefabs come from the
    /// preset's dual-grid slots, the low variants and stair from the JSON config.
    /// </summary>
    internal sealed class AtlasTileTheme
    {
        public string ThemeId;
        public string TileTypeId;
        public TilePreset Preset;
        public GameObject Stair;
        public GameObject[] HighForms;
        public GameObject[] LowForms;

        public GameObject ResolveForm(AtlasTileForm form, bool lowVariant)
        {
            var forms = lowVariant ? LowForms : HighForms;
            GameObject prefab = forms?[(int)form];
            if (prefab != null)
                return prefab;

            // Missing low variant falls back to the high form; never the reverse.
            return HighForms?[(int)form];
        }
    }

    internal interface IAtlasTileSetCatalog
    {
        bool IsLoaded { get; }
        float LowBorderDropMaxMeters { get; }
        float OverlaySurfaceOffsetMeters { get; }
        float StairModuleRiseMeters { get; }
        float StairMidSurfaceDropMeters { get; }
        bool TryGetByPreset(TilePreset preset, out AtlasTileTheme theme);
        bool TryGetByPresetId(string presetId, out AtlasTileTheme theme);
        bool TryGetByTileId(string tileTypeId, out AtlasTileTheme theme);
        bool TryGetByThemeId(string themeId, out AtlasTileTheme theme);
    }

    /// <summary>
    /// Loads the JSON atlas tile-set config once and indexes its themes for the
    /// chunk-first mesh provider. Missing config means "no atlas set" — the
    /// provider then falls back to plain preset slots.
    /// </summary>
    internal sealed class AtlasTileSetCatalog : IAtlasTileSetCatalog
    {
        private readonly Dictionary<TilePreset, AtlasTileTheme> _byPreset = new();
        private readonly Dictionary<string, AtlasTileTheme> _byPresetId =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, AtlasTileTheme> _byTileId =
            new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, AtlasTileTheme> _byThemeId =
            new(StringComparer.OrdinalIgnoreCase);

        public AtlasTileSetCatalog()
        {
            AtlasTileSetConfig config = LoadConfig();
            if (config == null)
                return;

            LowBorderDropMaxMeters = Mathf.Max(0.01f, config.LowBorderDropMaxMeters);
            OverlaySurfaceOffsetMeters = Mathf.Max(0f, config.OverlaySurfaceOffsetMeters);
            StairModuleRiseMeters = Mathf.Max(0.01f, config.StairModuleRiseMeters);
            StairMidSurfaceDropMeters = Mathf.Clamp(config.StairMidSurfaceDropMeters, 0f, StairModuleRiseMeters);

            foreach (AtlasTileThemeConfig entry in config.Themes ?? new List<AtlasTileThemeConfig>())
            {
                AtlasTileTheme theme = Resolve(entry);
                if (theme == null)
                    continue;

                _byPreset[theme.Preset] = theme;
                if (!string.IsNullOrWhiteSpace(theme.Preset.tileId))
                    _byPresetId[theme.Preset.tileId.Trim()] = theme;
                if (!string.IsNullOrWhiteSpace(theme.Preset.name))
                    _byPresetId[theme.Preset.name] = theme;
                if (!string.IsNullOrWhiteSpace(theme.TileTypeId))
                    _byTileId[theme.TileTypeId.Trim()] = theme;
                if (!string.IsNullOrWhiteSpace(theme.ThemeId))
                    _byThemeId[theme.ThemeId.Trim()] = theme;
            }

            IsLoaded = _byPreset.Count > 0;
        }

        public bool IsLoaded { get; }
        public float LowBorderDropMaxMeters { get; } = 0.25f;
        public float OverlaySurfaceOffsetMeters { get; } = 0.003f;
        public float StairModuleRiseMeters { get; } = 0.25f;
        public float StairMidSurfaceDropMeters { get; } = 0.125f;

        public bool TryGetByPreset(TilePreset preset, out AtlasTileTheme theme)
        {
            theme = null;
            return preset != null && _byPreset.TryGetValue(preset, out theme);
        }

        public bool TryGetByPresetId(string presetId, out AtlasTileTheme theme)
        {
            theme = null;
            return !string.IsNullOrWhiteSpace(presetId)
                   && _byPresetId.TryGetValue(presetId.Trim(), out theme);
        }

        public bool TryGetByTileId(string tileTypeId, out AtlasTileTheme theme)
        {
            theme = null;
            return !string.IsNullOrWhiteSpace(tileTypeId)
                   && _byTileId.TryGetValue(tileTypeId.Trim(), out theme);
        }

        public bool TryGetByThemeId(string themeId, out AtlasTileTheme theme)
        {
            theme = null;
            return !string.IsNullOrWhiteSpace(themeId)
                   && _byThemeId.TryGetValue(themeId.Trim(), out theme);
        }

        private static AtlasTileSetConfig LoadConfig()
        {
            try
            {
                IReadOnlyList<AtlasTileSetConfig> all =
                    JsonConfigRuntime.GetAll<AtlasTileSetConfig>();
                if (all == null || all.Count == 0)
                    return null;
                if (all.Count > 1)
                    Debug.LogWarning("[AtlasTileSet] Multiple atlas tile-set configs found; using the first.");
                return all[0];
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AtlasTileSet] Config not available: {ex.Message}");
                return null;
            }
        }

        private static AtlasTileTheme Resolve(AtlasTileThemeConfig config)
        {
            if (config?.Preset == null)
                return null;

            var high = new GameObject[5];
            high[(int)AtlasTileForm.Corner] = config.Preset.DUALGRD_cornerTile;
            high[(int)AtlasTileForm.Edge] = config.Preset.DUALGRD_edgeTile;
            high[(int)AtlasTileForm.Interior] = config.Preset.DUALGRD_invertedCornerTile;
            high[(int)AtlasTileForm.Merged] = config.Preset.DUALGRD_doubleInteriorCornerTile;
            high[(int)AtlasTileForm.Fill] = config.Preset.DUALGRD_fillTile;

            var low = new GameObject[5];
            low[(int)AtlasTileForm.Corner] = config.CornerLow;
            low[(int)AtlasTileForm.Edge] = config.EdgeLow;
            low[(int)AtlasTileForm.Interior] = config.InteriorLow;
            low[(int)AtlasTileForm.Merged] = config.MergedLow;
            low[(int)AtlasTileForm.Fill] = config.FillLow;

            return new AtlasTileTheme
            {
                ThemeId = config.ThemeId,
                TileTypeId = config.TileTypeId,
                Preset = config.Preset,
                Stair = config.Stair,
                HighForms = high,
                LowForms = low,
            };
        }
    }
}
