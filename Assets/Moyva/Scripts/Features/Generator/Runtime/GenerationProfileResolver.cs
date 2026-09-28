using Kruty1918.JsonConfig;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Load -&gt; Validate -&gt; Resolve -&gt; Freeze for generation profiles:
    /// reads the <see cref="GenerationProfilesConfig"/> JSON document, picks
    /// <see cref="GenerationProfilesConfig.DefaultProfileId"/> and produces the
    /// immutable <see cref="ResolvedGenerationProfile"/> consumed by the world
    /// build. A missing or invalid document yields null — callers then keep
    /// legacy behaviour, so older content is never forced through the new path.
    /// </summary>
    internal static class GenerationProfileResolver
    {
        public const string DocumentId = "generation-profiles";

        public static ResolvedGenerationProfile ResolveActive()
        {
            GenerationProfilesConfig config;
            try
            {
                config = JsonConfigRuntime.Get<GenerationProfilesConfig>(DocumentId);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning(
                    $"[GenerationProfile] Failed to load '{DocumentId}': {ex.Message}");
                return null;
            }

            return Resolve(config, config?.DefaultProfileId);
        }

        internal static ResolvedGenerationProfile Resolve(
            GenerationProfilesConfig config,
            string profileId)
        {
            if (config?.Profiles == null || string.IsNullOrWhiteSpace(profileId))
                return null;

            GenerationProfile profile = null;
            foreach (var candidate in config.Profiles)
            {
                if (candidate == null
                    || !candidate.Enabled
                    || !string.Equals(candidate.Id, profileId, System.StringComparison.Ordinal))
                {
                    continue;
                }

                profile = candidate;
                break;
            }

            if (profile == null)
            {
                Debug.LogWarning(
                    $"[GenerationProfile] Profile '{profileId}' is missing or disabled in '{DocumentId}'.");
                return null;
            }

            GenerationProfileWater water = profile.Water ?? new GenerationProfileWater();
            GenerationProfileTerrain terrain = profile.Terrain ?? new GenerationProfileTerrain();
            GenerationProfileDecorations decorations =
                profile.Decorations ?? new GenerationProfileDecorations();

            return new ResolvedGenerationProfile(
                profile.Id,
                terrain.SingleSolidTile,
                water.Mode == GenerationProfileWaterMode.SimpleQuad,
                water.Material,
                water.SurfaceOffsetMeters,
                water.EmitBed,
                decorations.ConfigId);
        }
    }
}
