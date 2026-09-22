using System.Collections.Generic;
using System.Reflection;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.JsonConfig;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Executes a <see cref="TwcModifierMaskStep"/> by delegating the mask work to
    /// the wrapped TileWorldCreator <see cref="BlueprintModifier"/>. Generator
    /// modifiers receive an empty position set; regular modifiers receive the
    /// accumulated layer mask positions.
    /// </summary>
    internal static class RecipeTwcModifierRunner
    {
        public static bool[,] Execute(
            TwcModifierMaskStep step,
            bool[,] source,
            GeneratorMaskContext context)
        {
            var modifier = EnsureModifierInstance(step);
            if (modifier == null)
            {
                Debug.LogError(
                    $"[GeneratorRecipe] TWC modifier '{step?.ModifierTypeName}' is not initialized.");
                return source;
            }

            int width = Mathf.Max(1, context?.MapSize.x ?? source?.GetLength(0) ?? 1);
            int height = Mathf.Max(1, context?.MapSize.y ?? source?.GetLength(1) ?? 1);
            uint seed = NonZeroSeed(context, step.ModifierTypeName ?? modifier.GetType().Name);

            var config = ScriptableObject.CreateInstance<Configuration>();
            var layer = ScriptableObject.CreateInstance<BlueprintLayer>();
            var previousRandomState = UnityEngine.Random.state;
            try
            {
                UnityEngine.Random.InitState(unchecked((int)seed));
                config.width = width;
                config.height = height;
                config.useGlobalRandomSeed = true;
                config.globalRandomSeed = (int)seed;
                config.currentRandomSeed = seed;

                InitializeLayerRandom(layer, seed);

                var positions = ToPositions(source, width, height);

                modifier.asset = config;
                modifier.isEnabled = true;
                var result = modifier.Execute(positions, layer);
                if (result == null)
                    result = positions;

                return ToMask(result, width, height);
            }
            catch (System.Exception ex)
            {
                Debug.LogError(
                    $"[GeneratorRecipe] TWC modifier '{modifier.GetType().Name}' failed: {ex.Message}");
                return source;
            }
            finally
            {
                UnityEngine.Random.state = previousRandomState;
                Object.DestroyImmediate(layer);
                Object.DestroyImmediate(config);
            }
        }

        private static uint NonZeroSeed(GeneratorMaskContext context, string salt)
        {
            long raw = GlobalSeed.Combine(
                context?.Seed ?? 1,
                GlobalSeed.StableHash(salt ?? string.Empty));
            uint seed = unchecked((uint)raw);
            return seed == 0u ? 1u : seed;
        }

        private static void InitializeLayerRandom(BlueprintLayer layer, uint seed)
        {
            if (layer == null)
                return;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var layerType = layer.GetType();

            var randomField = layerType.GetField("random", flags);
            if (randomField != null)
            {
                object randomValue = CreateRandomValue(randomField.FieldType, seed);
                if (randomValue != null)
                    randomField.SetValue(layer, randomValue);
                return;
            }

            var randomProperty = layerType.GetProperty("random", flags);
            if (randomProperty != null && randomProperty.CanWrite)
            {
                object randomValue = CreateRandomValue(randomProperty.PropertyType, seed);
                if (randomValue != null)
                    randomProperty.SetValue(layer, randomValue, null);
            }
        }

        private static object CreateRandomValue(System.Type randomType, uint seed)
        {
            if (randomType == null)
                return null;

            try
            {
                return System.Activator.CreateInstance(randomType, new object[] { seed });
            }
            catch
            {
                return null;
            }
        }

        internal static BlueprintModifier EnsureModifierInstance(TwcModifierMaskStep step)
        {
            if (step == null)
                return null;
            if (step.Modifier != null)
                return step.Modifier;
            if (string.IsNullOrWhiteSpace(step.ModifierTypeName))
                return null;

            var modifierType = TwcModifierCatalog.ResolveType(step.ModifierTypeName);
            if (modifierType == null || !typeof(BlueprintModifier).IsAssignableFrom(modifierType))
                return null;

            step.Modifier = JsonObjectFactory.Create(modifierType) as BlueprintModifier;
            if (step.Modifier != null)
                step.Modifier.name = modifierType.Name;
            return step.Modifier;
        }

        private static HashSet<Vector2> ToPositions(bool[,] source, int width, int height)
        {
            var positions = new HashSet<Vector2>();
            if (source == null)
                return positions;

            int w = Mathf.Min(width, source.GetLength(0));
            int h = Mathf.Min(height, source.GetLength(1));
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (source[x, y])
                    positions.Add(new Vector2(x, y));
            }
            return positions;
        }

        private static bool[,] ToMask(HashSet<Vector2> positions, int width, int height)
        {
            var mask = new bool[width, height];
            if (positions == null)
                return mask;
            foreach (var pos in positions)
            {
                int x = Mathf.RoundToInt(pos.x);
                int y = Mathf.RoundToInt(pos.y);
                if (x >= 0 && x < width && y >= 0 && y < height)
                    mask[x, y] = true;
            }
            return mask;
        }
    }
}
