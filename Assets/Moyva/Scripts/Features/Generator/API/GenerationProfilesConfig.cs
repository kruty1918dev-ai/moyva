using System;
using Kruty1918.JsonConfig;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    /// <summary>
    /// JSON-authored generation profiles selecting the visual-composition
    /// behaviour of the chunk-first world build: terrain geometry mode,
    /// water surface mode and the decoration configuration document.
    /// The active profile is <see cref="DefaultProfileId"/>; new worlds use
    /// it and loaded saves rebuild visuals under it as well, because the
    /// profile only governs presentation, never gameplay data.
    /// </summary>
    [Serializable]
    public sealed class GenerationProfilesConfig : JsonConfigObject
    {
        /// <summary>Profile applied to every world build.</summary>
        public string DefaultProfileId = "simple-stable-v1";

        /// <summary>Available profiles keyed by <see cref="GenerationProfile.Id"/>.</summary>
        public GenerationProfile[] Profiles = Array.Empty<GenerationProfile>();
    }

    /// <summary>Water surface rendering mode for a generation profile.</summary>
    public enum GenerationProfileWaterMode
    {
        /// <summary>Authored water layers render through the full pipeline
        /// (shore washes, waterfall strips/curtains, seabed fields).</summary>
        Full = 0,

        /// <summary>Each water cell emits exactly one flat quad at its water
        /// surface height. No shore washes, waterfall strips or chunk water
        /// fields; the bed column still seals the underwater void.</summary>
        SimpleQuad = 1,
    }

    [Serializable]
    public sealed class GenerationProfile
    {
        /// <summary>Stable profile id, e.g. "simple-stable-v1".</summary>
        public string Id = "simple-stable-v1";

        /// <summary>Disabled profiles are skipped during resolution.</summary>
        public bool Enabled = true;

        /// <summary>Terrain visual-composition switches.</summary>
        public GenerationProfileTerrain Terrain = new();

        /// <summary>Water visual-composition switches.</summary>
        public GenerationProfileWater Water = new();

        /// <summary>Decoration configuration selection.</summary>
        public GenerationProfileDecorations Decorations = new();
    }

    [Serializable]
    public sealed class GenerationProfileTerrain
    {
        /// <summary>
        /// When true every terrain layer renders through the generated
        /// single-top beveled tile (<c>SolidBeveledTileMeshUtility</c>) instead
        /// of dual-grid fragments, so no central cross seam appears. Layers
        /// without a resolvable atlas theme keep their authored tiles.
        /// </summary>
        public bool SingleSolidTile = true;
    }

    [Serializable]
    public sealed class GenerationProfileWater
    {
        /// <summary>Surface rendering mode; see <see cref="GenerationProfileWaterMode"/>.</summary>
        public GenerationProfileWaterMode Mode = GenerationProfileWaterMode.SimpleQuad;

        /// <summary>
        /// Material applied to the simple water quad. Resolved from a $asset
        /// catalog reference in JSON; null keeps each layer's authored water
        /// material.
        /// </summary>
        public Material Material;

        /// <summary>
        /// Vertical offset applied to the authored water surface height so the
        /// quad sits slightly below the shore lip instead of z-fighting sand.
        /// </summary>
        public float SurfaceOffsetMeters = -0.05f;

        /// <summary>
        /// Emit the submerged sand-bed column under the quad. Keeps a
        /// transparent water material from revealing the void below.
        /// </summary>
        public bool EmitBed = true;
    }

    [Serializable]
    public sealed class GenerationProfileDecorations
    {
        /// <summary>
        /// Optional id of an <see cref="EnvironmentDecorationConfig"/> JSON
        /// document. When set it replaces the default decoration config so the
        /// profile owns decoration pools, densities and floating rules.
        /// </summary>
        public string ConfigId;
    }
}
