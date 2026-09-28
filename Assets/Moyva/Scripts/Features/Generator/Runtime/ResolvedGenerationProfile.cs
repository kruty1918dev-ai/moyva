using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Frozen runtime snapshot of the active <see cref="API.GenerationProfile"/>.
    /// Consumed by the chunk-first visual build (terrain mesh source, water
    /// sources, chunk water fields) and by the decoration bindings. Immutable:
    /// created once at composition time and shared by every consumer.
    /// </summary>
    internal sealed class ResolvedGenerationProfile
    {
        public ResolvedGenerationProfile(
            string id,
            bool singleSolidTile,
            bool simpleWater,
            Material waterMaterial,
            float waterSurfaceOffset,
            bool waterEmitBed,
            string decorationConfigId)
        {
            Id = id ?? string.Empty;
            SingleSolidTile = singleSolidTile;
            SimpleWater = simpleWater;
            WaterMaterial = waterMaterial;
            WaterSurfaceOffset = waterSurfaceOffset;
            WaterEmitBed = waterEmitBed;
            DecorationConfigId = decorationConfigId ?? string.Empty;
        }

        /// <summary>Stable profile id, e.g. "simple-stable-v1".</summary>
        public string Id { get; }

        /// <summary>Render every terrain layer as one solid beveled tile.</summary>
        public bool SingleSolidTile { get; }

        /// <summary>Render each water surface as exactly one flat quad.</summary>
        public bool SimpleWater { get; }

        /// <summary>Simple water quad material; null = authored layer material.</summary>
        public Material WaterMaterial { get; }

        /// <summary>Offset applied to the authored water surface height.</summary>
        public float WaterSurfaceOffset { get; }

        /// <summary>Emit the submerged bed column under simple water quads.</summary>
        public bool WaterEmitBed { get; }

        /// <summary>Optional <see cref="API.EnvironmentDecorationConfig"/> document id.</summary>
        public string DecorationConfigId { get; }
    }
}
