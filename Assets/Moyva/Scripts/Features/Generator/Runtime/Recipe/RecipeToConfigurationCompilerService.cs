using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IRecipeToConfigurationCompilerService
    {
        List<CompiledLayerMap> Compile(
            GeneratorMapRecipe recipe,
            TileWorldCreatorManager manager,
            int seed,
            ISet<string> skippedLayerIds = null,
            Vector2Int? mapSizeOverride = null);
    }

    /// <summary>
    /// Compiles a <see cref="GeneratorMapRecipe"/> into the companion TWC
    /// configuration: evaluates layer masks, synchronizes blueprint/build layers,
    /// attaches the authoritative precomputed-mask modifiers and generated
    /// object layers, and disables blueprint layers no longer used by the recipe.
    /// </summary>
    internal sealed class RecipeToConfigurationCompilerService
        : IRecipeToConfigurationCompilerService
    {
        private readonly RecipeCompilerConfigurationService _configuration;
        private readonly RecipeCompilerBlueprintSyncService _blueprints;
        private readonly RecipeCompilerTileBuildLayerSyncService _buildLayers;
        private readonly RecipeMaskBlueprintService _masks;

        public RecipeToConfigurationCompilerService()
            : this(
                new RecipeCompilerConfigurationService(),
                new RecipeCompilerBlueprintSyncService(new RecipeCompilerTileBuildLayerLookup()),
                new RecipeCompilerTileBuildLayerSyncService(new RecipeCompilerTileBuildLayerLookup()),
                new RecipeMaskBlueprintService())
        {
        }

        internal RecipeToConfigurationCompilerService(
            RecipeCompilerConfigurationService configuration,
            RecipeCompilerBlueprintSyncService blueprints,
            RecipeCompilerTileBuildLayerSyncService buildLayers,
            RecipeMaskBlueprintService masks)
        {
            _configuration = configuration;
            _blueprints = blueprints;
            _buildLayers = buildLayers;
            _masks = masks;
        }

        public List<CompiledLayerMap> Compile(
            GeneratorMapRecipe recipe,
            TileWorldCreatorManager manager,
            int seed,
            ISet<string> skippedLayerIds = null,
            Vector2Int? mapSizeOverride = null)
        {
            int effectiveSeed = GlobalSeed.Normalize(seed);
            using var randomScope = new DeterministicRandomScope(effectiveSeed);
            var result = new List<CompiledLayerMap>();
            if (recipe == null || manager == null || manager.configuration == null)
                return result;

            Configuration config = manager.configuration;
            _configuration.Apply(recipe, config, effectiveSeed, mapSizeOverride);
            var mapSize = new Vector2Int(config.width, config.height);

            var masks = GeneratorMaskEvaluator.EvaluateMasks(
                recipe,
                effectiveSeed,
                mapSize,
                skippedLayerIds);

            RecipeBlueprintSyncResult sync = _blueprints.Sync(recipe, config, skippedLayerIds);
            _buildLayers.Sync(recipe, config, manager, sync, skippedLayerIds);
            _masks.Apply(sync, config, masks);
            _blueprints.DisableUnused(sync.ExistingLayers, sync.UsedLayerGuids);

            var objectLayers = GeneratorMaskEvaluator.CollectObjectPlacements(
                recipe,
                masks,
                effectiveSeed);
            TWCObjectPlacementAdapter.Apply(config, manager, objectLayers, sync.CompiledLayers);
            return sync.CompiledLayers;
        }
    }
}
