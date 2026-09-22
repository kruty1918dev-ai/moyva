using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.JsonConfig;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// One visual theme of the atlas tile set. The high (0.5 m border) prefabs
    /// are read from the TilePreset dual-grid slots; this config adds the low
    /// (0.25 m border) variants and the authored stair module.
    /// </summary>
    [System.Serializable]
    public sealed class AtlasTileThemeConfig
    {
        [Tooltip("Pack theme id, e.g. grass, sand, rock_cliff.")]
        public string ThemeId;

        [Tooltip("Semantic tile id this theme renders (moyva.tile-type id).")]
        public string TileTypeId;

        [Tooltip("Dual-grid preset holding the five high-variant prefabs.")]
        public TilePreset Preset;

        public GameObject CornerLow;
        public GameObject EdgeLow;
        public GameObject InteriorLow;
        public GameObject MergedLow;
        public GameObject FillLow;
        public GameObject Stair;
    }

    /// <summary>
    /// JSON model (moyva.atlas-tile-set) describing the imported atlas tile pack:
    /// physical contract values and per-theme asset wiring.
    /// </summary>
    [System.Serializable]
    public sealed class AtlasTileSetConfig : JsonConfigObject
    {
        [Tooltip("Terrace quantum used by the pack, meters.")]
        [Min(0.01f)] public float HeightQuantumMeters = 0.25f;

        [Tooltip("Open-side drops up to this depth use _low prefab variants.")]
        [Min(0.01f)] public float LowBorderDropMaxMeters = 0.25f;

        [Tooltip("Rise covered by one authored stair module.")]
        [Min(0.01f)] public float StairModuleRiseMeters = 0.25f;

        [Tooltip("Walking surface used for traversal is this far below the module top.")]
        [Min(0f)] public float StairMidSurfaceDropMeters = 0.125f;

        [Tooltip("Surface offset for road/footpath overlay quads above the terrain surface.")]
        [Min(0f)] public float OverlaySurfaceOffsetMeters = 0.003f;

        public List<AtlasTileThemeConfig> Themes = new();
    }
}
