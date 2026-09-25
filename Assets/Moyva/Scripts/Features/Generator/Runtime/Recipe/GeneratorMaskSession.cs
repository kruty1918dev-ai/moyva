using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Mutable state shared by every layer evaluation inside one
    /// <see cref="GeneratorMaskEvaluator.EvaluateMasks"/> call: caches the
    /// hydrology plan (keyed by resolved sink layer id) so river and lake
    /// steps share one flood/accumulation pass, and collects per-cell surface
    /// height overrides that steps register for their owning layer.
    /// </summary>
    internal sealed class GeneratorMaskSession
    {
        private readonly GeneratorMapRecipe _recipe;
        private readonly Dictionary<string, RecipeHydrologyPlan> _hydrologyPlans =
            new(System.StringComparer.Ordinal);
        private readonly Dictionary<string, float[,]> _surfaceOverrides =
            new(System.StringComparer.Ordinal);
        private readonly Dictionary<string, float[,]> _bedOverrides =
            new(System.StringComparer.Ordinal);

        public GeneratorMaskSession(GeneratorMapRecipe recipe)
        {
            _recipe = recipe;
        }

        /// <summary>Per-layer surface height overrides (meters) registered by mask steps.</summary>
        public IReadOnlyDictionary<string, float[,]> SurfaceOverrides => _surfaceOverrides;

        /// <summary>Per-layer bed height overrides (meters) registered by mask steps.</summary>
        public IReadOnlyDictionary<string, float[,]> BedOverrides => _bedOverrides;

        /// <summary>
        /// All hydrology plans evaluated during this session merged into one:
        /// masks are OR-ed, per-cell data takes the first valid value.
        /// Null when hydrology is disabled or never evaluated.
        /// </summary>
        public RecipeHydrologyPlan MergedHydrologyPlan =>
            RecipeHydrologyPlanner.Merge(_hydrologyPlans.Values);

        public RecipeHydrologyPlan GetHydrologyPlan(
            GeneratorMaskContext context,
            string sinkLayerIdOverride)
        {
            var config = _recipe?.Hydrology;
            if (config == null || !config.Enabled || context?.TerrainHeightField == null)
                return null;

            string sinkId = !string.IsNullOrWhiteSpace(sinkLayerIdOverride)
                ? sinkLayerIdOverride.Trim()
                : config.SinkLayerId?.Trim() ?? string.Empty;
            if (_hydrologyPlans.TryGetValue(sinkId, out var cached))
                return cached;

            bool[,] sinkMask = null;
            if (!string.IsNullOrEmpty(sinkId))
                context.LayerMasks?.TryGetValue(sinkId, out sinkMask);

            var plan = RecipeHydrologyPlanner.Build(
                context.TerrainHeightField,
                sinkMask,
                context.MapSize,
                config,
                unchecked(context.Seed + config.SeedSalt));
            _hydrologyPlans[sinkId] = plan;
            return plan;
        }

        public void SetSurfaceOverride(string layerId, float[,] surfaceHeights)
        {
            if (string.IsNullOrWhiteSpace(layerId) || surfaceHeights == null)
                return;
            _surfaceOverrides[layerId] = surfaceHeights;
        }

        public void SetBedOverride(string layerId, float[,] bedHeights)
        {
            if (string.IsNullOrWhiteSpace(layerId) || bedHeights == null)
                return;
            _bedOverrides[layerId] = bedHeights;
        }
    }
}
