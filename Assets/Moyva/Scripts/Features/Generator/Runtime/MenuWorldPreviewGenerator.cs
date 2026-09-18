using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Menu-only utility that evaluates a <see cref="GeneratorMapRecipe"/> without
    /// running a full world build. Returns final maps suitable for rendering into
    /// a preview Texture2D.
    /// </summary>
    public static class MenuWorldPreviewGenerator
    {
        public static bool TryGenerate(
            GeneratorMapRecipe recipe,
            int width,
            int height,
            int seed,
            out MenuWorldPreviewData previewData,
            out string errorMessage)
        {
            previewData = null;
            errorMessage = null;

            if (recipe == null)
            {
                errorMessage = "GeneratorMapRecipe is not assigned.";
                return false;
            }

            Vector2Int mapSize = ResolveMapSize(recipe, width, height);
            int previousSeed = GlobalSeed.Current;
            var previousRandomState = UnityEngine.Random.state;

            try
            {
                GlobalSeed.Set(seed);
                UnityEngine.Random.InitState(seed);

                var validation = GeneratorMapRecipeValidator.Validate(recipe);
                if (validation.HasGlobalErrors)
                {
                    errorMessage = string.Join("; ", validation.GlobalErrors);
                    return false;
                }

                var masks = GeneratorMaskEvaluator.EvaluateMasks(
                    recipe,
                    GlobalSeed.Normalize(seed),
                    mapSize,
                    validation.SkippedLayerIds);

                BuildPreviewMaps(
                    recipe,
                    masks,
                    mapSize,
                    out var biomeMap,
                    out var heightMap);

                previewData = new MenuWorldPreviewData(
                    mapSize.x,
                    mapSize.y,
                    seed,
                    biomeMap,
                    new string[mapSize.x, mapSize.y],
                    heightMap,
                    new string[mapSize.x, mapSize.y]);
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
            finally
            {
                GlobalSeed.Set(previousSeed);
                UnityEngine.Random.state = previousRandomState;
            }
        }

        /// <summary>
        /// Composites the preview maps from evaluated layer masks: for each cell
        /// the enabled Tiles layer with the highest sorting order wins; its tile
        /// id feeds the biome map and its height feeds the height map.
        /// </summary>
        private static void BuildPreviewMaps(
            GeneratorMapRecipe recipe,
            IReadOnlyDictionary<string, bool[,]> masks,
            Vector2Int mapSize,
            out string[,] biomeMap,
            out float[,] heightMap)
        {
            biomeMap = new string[mapSize.x, mapSize.y];
            heightMap = new float[mapSize.x, mapSize.y];

            var layers = GeneratorMaskEvaluator.OrderedLayers(recipe, null);
            for (int i = 0; i < layers.Count; i++)
            {
                var layer = layers[i];
                if (layer.OutputKind != LayerOutputKind.Tiles)
                    continue;
                if (!masks.TryGetValue(layer.Id, out var mask) || mask == null)
                    continue;

                string tileId = ResolvePreviewTileId(layer);
                int w = Mathf.Min(mapSize.x, mask.GetLength(0));
                int h = Mathf.Min(mapSize.y, mask.GetLength(1));
                for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    if (!mask[x, y])
                        continue;
                    biomeMap[x, y] = tileId;
                    heightMap[x, y] = layer.DefaultHeight;
                }
            }
        }

        private static string ResolvePreviewTileId(GeneratorMapLayer layer)
        {
            string tileId = layer?.ResolveTileId();
            return !string.IsNullOrWhiteSpace(tileId) ? tileId : layer?.Id;
        }

        private static Vector2Int ResolveMapSize(GeneratorMapRecipe recipe, int width, int height)
        {
            if (width > 0 && height > 0)
                return new Vector2Int(width, height);

            if (recipe?.SharedSettings != null && recipe.SharedSettings.HasMapSize)
                return recipe.SharedSettings.MapSize;

            return new Vector2Int(Mathf.Max(1, width), Mathf.Max(1, height));
        }
    }
}
