using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>Result of validating a <see cref="GeneratorMapRecipe"/>.</summary>
    internal sealed class MapRecipeValidationResult
    {
        public readonly List<string> GlobalErrors = new();
        public readonly List<string> Warnings = new();
        public readonly HashSet<string> SkippedLayerIds = new();
        public bool HasGlobalErrors => GlobalErrors.Count > 0;
    }

    /// <summary>
    /// Validates a recipe before it is compiled: layers must be present and
    /// uniquely identified, mask-step layer references must point at previously
    /// evaluated layers, and tile layers must provide a tile source.
    /// </summary>
    internal static class GeneratorMapRecipeValidator
    {
        public static MapRecipeValidationResult Validate(GeneratorMapRecipe recipe)
        {
            var result = new MapRecipeValidationResult();
            if (recipe == null)
            {
                result.GlobalErrors.Add("GeneratorMapRecipe is missing.");
                return result;
            }
            if (recipe.Layers == null || recipe.Layers.Count == 0)
            {
                result.GlobalErrors.Add("GeneratorMapRecipe has no layers.");
                return result;
            }

            var seenIds = new HashSet<string>();
            var evaluatedIds = new HashSet<string>();
            var ordered = GeneratorMaskEvaluator.OrderedLayers(recipe, null);
            bool hasTilesLayer = false;

            foreach (var layer in ordered)
            {
                if (string.IsNullOrWhiteSpace(layer.Id))
                {
                    result.Warnings.Add($"Layer '{layer.Name}' has no id; it cannot be referenced by other layers.");
                }
                else if (!seenIds.Add(layer.Id))
                {
                    result.GlobalErrors.Add($"Duplicate layer id '{layer.Id}' ({layer.Name}).");
                }

                if (RecipeCompilerBlueprintSyncService.HasRenderableTileOutput(layer))
                    hasTilesLayer = true;
                else if (layer.OutputKind == LayerOutputKind.Tiles)
                    result.Warnings.Add(
                        $"Tiles layer '{layer.Name}' has no tile source (TileType or TileVariants); it will render nothing.");

                if (layer.Steps == null || layer.Steps.Count == 0)
                    result.Warnings.Add($"Layer '{layer.Name}' has no mask steps; it produces an empty mask.");

                if (layer.Steps != null)
                {
                    foreach (var step in layer.Steps)
                    {
                        if (step is LayerMaskReferenceStep reference
                            && !string.IsNullOrWhiteSpace(reference.SourceLayerId)
                            && !evaluatedIds.Contains(reference.SourceLayerId.Trim()))
                        {
                            result.Warnings.Add(
                                $"Layer '{layer.Name}' references mask of layer '{reference.SourceLayerId}' " +
                                "which does not exist or is evaluated later; the reference resolves to an empty mask.");
                        }

                        if (step is HydrologyMaskStep hydrologyStep)
                        {
                            if (recipe.Hydrology == null || !recipe.Hydrology.Enabled)
                            {
                                result.Warnings.Add(
                                    $"Layer '{layer.Name}' uses a hydrology mask step but recipe.Hydrology is disabled; it produces an empty mask.");
                            }
                            else if (recipe.TerrainRelief == null || !recipe.TerrainRelief.Enabled)
                            {
                                result.Warnings.Add(
                                    $"Layer '{layer.Name}' uses a hydrology mask step but recipe.TerrainRelief is disabled; it produces an empty mask.");
                            }

                            string sinkId = !string.IsNullOrWhiteSpace(hydrologyStep.SinkLayerIdOverride)
                                ? hydrologyStep.SinkLayerIdOverride.Trim()
                                : recipe.Hydrology?.SinkLayerId?.Trim();
                            if (!string.IsNullOrEmpty(sinkId) && !evaluatedIds.Contains(sinkId))
                            {
                                result.Warnings.Add(
                                    $"Layer '{layer.Name}' hydrology sink '{sinkId}' does not exist or is evaluated later; rivers drain to the map border only.");
                            }
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(layer.Id))
                    evaluatedIds.Add(layer.Id);
            }

            if (!hasTilesLayer)
                result.Warnings.Add("GeneratorMapRecipe has no layer that renders tiles.");

            return result;
        }
    }
}
