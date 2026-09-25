using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    /// <summary>
    /// How a mask-producing step merges its result into the layer mask accumulated
    /// by previous steps. Transform steps ignore this value.
    /// </summary>
    public enum MaskCombineMode
    {
        Replace = 0,
        Add = 1,
        Subtract = 2,
        Intersect = 3,
        Xor = 4
    }

    /// <summary>
    /// One ordered mask step inside a <see cref="GeneratorMapLayer"/>.
    /// Source steps produce a bool mask that is merged via <see cref="Combine"/>;
    /// transform steps rewrite the accumulated mask in place.
    /// Concrete step types are resolved by stable polymorphic ids
    /// (see <c>JsonConfigTypeRegistry</c>).
    /// </summary>
    [System.Serializable]
    public abstract class GeneratorMaskStep
    {
        [Tooltip("How the produced mask merges into the accumulated layer mask. Ignored by transform steps.")]
        public MaskCombineMode Combine = MaskCombineMode.Add;

        public bool Enabled = true;
    }

    /// <summary>Base for steps that produce a mask from scratch.</summary>
    [System.Serializable]
    public abstract class GeneratorMaskSourceStep : GeneratorMaskStep
    {
        public abstract bool[,] GenerateMask(GeneratorMaskContext context);
    }

    /// <summary>Base for steps that transform the accumulated layer mask.</summary>
    [System.Serializable]
    public abstract class GeneratorMaskTransformStep : GeneratorMaskStep
    {
        /// <summary>Returns the transformed mask, or <paramref name="mask"/> when unchanged.</summary>
        public abstract bool[,] TransformMask(bool[,] mask, GeneratorMaskContext context);
    }

    /// <summary>Evaluation context passed to every mask step.</summary>
    public sealed class GeneratorMaskContext
    {
        public GeneratorMaskContext(
            int seed,
            Vector2Int mapSize,
            IReadOnlyDictionary<string, bool[,]> layerMasks,
            float[,] terrainHeightField = null)
        {
            Seed = seed;
            MapSize = mapSize;
            LayerMasks = layerMasks;
            TerrainHeightField = terrainHeightField;
        }

        public int Seed { get; }
        public Vector2Int MapSize { get; }

        /// <summary>Final masks of previously evaluated layers, keyed by layer id.</summary>
        public IReadOnlyDictionary<string, bool[,]> LayerMasks { get; }

        /// <summary>
        /// Optional deterministic per-cell terrain height in meters, produced by
        /// the recipe's <see cref="TerrainReliefConfig"/>. Read by
        /// <see cref="TerrainLevelMaskStep"/>; null when relief is disabled.
        /// </summary>
        public float[,] TerrainHeightField { get; }

        /// <summary>Shared per-evaluation cache; null when steps run standalone.</summary>
        internal Runtime.GeneratorMaskSession Session { get; set; }

        /// <summary>Id of the layer currently being evaluated; set by the evaluator.</summary>
        internal string CurrentLayerId { get; set; }
    }

    /// <summary>Deterministic tiled Perlin noise threshold mask.</summary>
    [System.Serializable]
    public sealed class PerlinNoiseMaskStep : GeneratorMaskSourceStep
    {
        [Min(0.0001f)] public float Scale = 20f;
        [Range(1, 12)] public int Octaves = 4;
        [Range(0.01f, 1f)] public float Persistence = 0.5f;
        [Min(1f)] public float Lacunarity = 2f;
        public Vector2 Offset = Vector2.zero;
        [Range(0f, 1f)] public float Threshold = 0.5f;

        public override bool[,] GenerateMask(GeneratorMaskContext context)
        {
            int width = Mathf.Max(1, context?.MapSize.x ?? 1);
            int height = Mathf.Max(1, context?.MapSize.y ?? 1);
            int seed = context?.Seed ?? 1;
            var mask = new bool[width, height];

            float scale = Mathf.Max(0.0001f, Scale);
            int octaves = Mathf.Max(1, Octaves);
            float persistence = Mathf.Clamp(Persistence, 0.01f, 1f);
            float lacunarity = Mathf.Max(1f, Lacunarity);

            var octaveOffsets = new Vector2[octaves];
            var random = new System.Random(seed);
            for (int i = 0; i < octaves; i++)
            {
                float offsetX = random.Next(-100000, 100000) + Offset.x;
                float offsetY = random.Next(-100000, 100000) + Offset.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }

            float minValue = float.MaxValue;
            float maxValue = float.MinValue;
            var noise = new float[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float amplitude = 1f;
                float frequency = 1f;
                float value = 0f;
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = x / scale * frequency + octaveOffsets[i].x;
                    float sampleY = y / scale * frequency + octaveOffsets[i].y;
                    value += Mathf.PerlinNoise(sampleX, sampleY) * amplitude;
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }
                noise[x, y] = value;
                if (value < minValue) minValue = value;
                if (value > maxValue) maxValue = value;
            }

            float range = Mathf.Max(0.0001f, maxValue - minValue);
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                mask[x, y] = Mathf.InverseLerp(minValue, maxValue, noise[x, y]) >= Threshold;

            return mask;
        }
    }

    /// <summary>Deterministic geometric mask (ellipse or rectangle falloff area).</summary>
    [System.Serializable]
    public sealed class ShapeMaskStep : GeneratorMaskSourceStep
    {
        public enum ShapeMaskKind { Ellipse = 0, Rectangle = 1 }

        public ShapeMaskKind Shape = ShapeMaskKind.Ellipse;
        [Tooltip("Normalized center (0..1 across the map).")]
        public Vector2 Center = new(0.5f, 0.5f);
        [Tooltip("Normalized size (0..1 of the map extents).")]
        public Vector2 Size = new(0.75f, 0.75f);
        public bool Invert;

        public override bool[,] GenerateMask(GeneratorMaskContext context)
        {
            int width = Mathf.Max(1, context?.MapSize.x ?? 1);
            int height = Mathf.Max(1, context?.MapSize.y ?? 1);
            var mask = new bool[width, height];

            float halfW = Mathf.Max(0.0001f, Size.x * 0.5f);
            float halfH = Mathf.Max(0.0001f, Size.y * 0.5f);
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float nx = width <= 1 ? 0f : (float)x / (width - 1);
                float ny = height <= 1 ? 0f : (float)y / (height - 1);
                float dx = (nx - Center.x) / halfW;
                float dy = (ny - Center.y) / halfH;
                bool inside = Shape == ShapeMaskKind.Rectangle
                    ? Mathf.Abs(dx) <= 1f && Mathf.Abs(dy) <= 1f
                    : dx * dx + dy * dy <= 1f;
                mask[x, y] = inside != Invert;
            }
            return mask;
        }
    }

    /// <summary>
    /// Selects cells whose terrain relief height (meters) falls inside
    /// [MinMeters, MaxMeters]. Requires the recipe's terrain relief field;
    /// produces an empty mask when relief is disabled.
    /// </summary>
    [System.Serializable]
    public sealed class TerrainLevelMaskStep : GeneratorMaskSourceStep
    {
        [Tooltip("Inclusive lower height bound in meters.")]
        public float MinMeters = float.NegativeInfinity;
        [Tooltip("Inclusive upper height bound in meters.")]
        public float MaxMeters = float.PositiveInfinity;
        public bool Invert;

        public override bool[,] GenerateMask(GeneratorMaskContext context)
        {
            int width = Mathf.Max(1, context?.MapSize.x ?? 1);
            int height = Mathf.Max(1, context?.MapSize.y ?? 1);
            var mask = new bool[width, height];
            float[,] field = context?.TerrainHeightField;
            if (field == null)
                return mask;

            int w = Mathf.Min(width, field.GetLength(0));
            int h = Mathf.Min(height, field.GetLength(1));
            const float epsilon = 0.0001f;
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                float value = field[x, y];
                bool inside = value >= MinMeters - epsilon
                              && value <= MaxMeters + epsilon;
                mask[x, y] = inside != Invert;
            }
            return mask;
        }
    }

    /// <summary>
    /// Marks river or lake cells traced on the relief height field by the
    /// recipe's <see cref="RecipeHydrologyConfig"/>. Rivers drain downhill into
    /// the configured sink (open-water mask or low terrain) or the map border;
    /// lakes fill flooded depressions. Registers the plan's per-cell water
    /// surface as the owning layer's surface-height override.
    /// </summary>
    [System.Serializable]
    public sealed class HydrologyMaskStep : GeneratorMaskSourceStep
    {
        public enum HydrologyChannel { River = 0, Lake = 1 }

        public HydrologyChannel Channel = HydrologyChannel.River;

        [Tooltip("Overrides RecipeHydrologyConfig.SinkLayerId when set.")]
        public string SinkLayerIdOverride;

        public override bool[,] GenerateMask(GeneratorMaskContext context)
        {
            int width = Mathf.Max(1, context?.MapSize.x ?? 1);
            int height = Mathf.Max(1, context?.MapSize.y ?? 1);
            var mask = new bool[width, height];

            var plan = context?.Session != null
                ? context.Session.GetHydrologyPlan(context, SinkLayerIdOverride)
                : null;
            if (plan == null)
                return mask;

            var channelMask = Channel == HydrologyChannel.Lake
                ? plan.LakeMask
                : plan.RiverMask;
            int w = Mathf.Min(width, channelMask.GetLength(0));
            int h = Mathf.Min(height, channelMask.GetLength(1));
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                mask[x, y] = channelMask[x, y];

            context.Session.SetSurfaceOverride(context.CurrentLayerId, plan.WaterSurface);
            context.Session.SetBedOverride(context.CurrentLayerId, plan.BedHeight);
            return mask;
        }
    }

    /// <summary>All-true or all-false mask.</summary>
    [System.Serializable]
    public sealed class BoolValueMaskStep : GeneratorMaskSourceStep
    {
        public bool Value = true;

        public override bool[,] GenerateMask(GeneratorMaskContext context)
        {
            int width = Mathf.Max(1, context?.MapSize.x ?? 1);
            int height = Mathf.Max(1, context?.MapSize.y ?? 1);
            var mask = new bool[width, height];
            if (!Value)
                return mask;
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                mask[x, y] = true;
            return mask;
        }
    }

    /// <summary>References the final mask of a previously evaluated layer.</summary>
    [System.Serializable]
    public sealed class LayerMaskReferenceStep : GeneratorMaskSourceStep
    {
        [Tooltip("Id of the recipe layer whose final mask is reused.")]
        public string SourceLayerId;
        public bool Invert;

        public override bool[,] GenerateMask(GeneratorMaskContext context)
        {
            int width = Mathf.Max(1, context?.MapSize.x ?? 1);
            int height = Mathf.Max(1, context?.MapSize.y ?? 1);
            var result = new bool[width, height];

            bool[,] source = null;
            if (!string.IsNullOrWhiteSpace(SourceLayerId))
                context?.LayerMasks?.TryGetValue(SourceLayerId, out source);
            if (source == null)
                return result;

            int w = Mathf.Min(width, source.GetLength(0));
            int h = Mathf.Min(height, source.GetLength(1));
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                result[x, y] = source[x, y] != Invert;
            return result;
        }
    }

    /// <summary>
    /// Runs a TileWorldCreator blueprint modifier. Generator-category modifiers
    /// (e.g. DotGrid, CellularAutomata sources) produce a new mask; modifier-category
    /// entries (Smooth, Shrink, ...) transform the accumulated mask.
    /// </summary>
    [System.Serializable]
    public sealed class TwcModifierMaskStep : GeneratorMaskSourceStep
    {
        [Tooltip("Full type name of the TWC BlueprintModifier (e.g. GiantGrey.TileWorldCreator.Smooth).")]
        public string ModifierTypeName;

        [Tooltip("Serialized modifier instance; created from ModifierTypeName when missing.")]
        public BlueprintModifier Modifier;

        public bool IsGenerator
        {
            get
            {
                string typeName = !string.IsNullOrWhiteSpace(ModifierTypeName)
                    ? ModifierTypeName
                    : Modifier?.GetType().FullName;
                return Runtime.TwcModifierCatalog.TryGet(typeName, out var entry) && entry.IsGenerator;
            }
        }

        public override bool[,] GenerateMask(GeneratorMaskContext context)
            => Runtime.RecipeTwcModifierRunner.Execute(this, null, context);

        /// <summary>Used by the evaluator when this step acts as a transform.</summary>
        public bool[,] TransformMask(bool[,] mask, GeneratorMaskContext context)
            => Runtime.RecipeTwcModifierRunner.Execute(this, mask, context);
    }

    /// <summary>Inverts the accumulated layer mask.</summary>
    [System.Serializable]
    public sealed class BoolInvertStep : GeneratorMaskTransformStep
    {
        public override bool[,] TransformMask(bool[,] mask, GeneratorMaskContext context)
        {
            int width = Mathf.Max(1, context?.MapSize.x ?? mask?.GetLength(0) ?? 1);
            int height = Mathf.Max(1, context?.MapSize.y ?? mask?.GetLength(1) ?? 1);
            var result = new bool[width, height];
            if (mask == null)
            {
                for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    result[x, y] = true;
                return result;
            }
            int w = Mathf.Min(width, mask.GetLength(0));
            int h = Mathf.Min(height, mask.GetLength(1));
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                result[x, y] = !mask[x, y];
            return result;
        }
    }

    /// <summary>Erode/dilate morphology over the accumulated layer mask.</summary>
    [System.Serializable]
    public sealed class MaskMorphologyStep : GeneratorMaskTransformStep
    {
        public enum MaskMorphologyOperation { Dilate = 0, Erode = 1 }

        public MaskMorphologyOperation Operation = MaskMorphologyOperation.Dilate;
        [Range(0, 8)] public int Radius = 1;
        [Range(1, 8)] public int Iterations = 1;

        public override bool[,] TransformMask(bool[,] mask, GeneratorMaskContext context)
        {
            if (mask == null)
                return null;

            var result = mask;
            int iterations = Mathf.Max(1, Iterations);
            int radius = Mathf.Max(0, Radius);
            for (int i = 0; i < iterations && radius > 0; i++)
                result = Operation == MaskMorphologyOperation.Dilate
                    ? Dilate(result, radius)
                    : Erode(result, radius);
            return result;
        }

        private static bool[,] Dilate(bool[,] source, int radius)
        {
            int width = source.GetLength(0);
            int height = source.GetLength(1);
            var result = new bool[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                bool value = false;
                for (int dx = -radius; dx <= radius && !value; dx++)
                for (int dy = -radius; dy <= radius && !value; dy++)
                {
                    int sx = x + dx;
                    int sy = y + dy;
                    if (sx >= 0 && sx < width && sy >= 0 && sy < height && source[sx, sy])
                        value = true;
                }
                result[x, y] = value;
            }
            return result;
        }

        private static bool[,] Erode(bool[,] source, int radius)
        {
            int width = source.GetLength(0);
            int height = source.GetLength(1);
            var result = new bool[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                bool value = true;
                for (int dx = -radius; dx <= radius && value; dx++)
                for (int dy = -radius; dy <= radius && value; dy++)
                {
                    int sx = x + dx;
                    int sy = y + dy;
                    if (sx < 0 || sx >= width || sy < 0 || sy >= height || !source[sx, sy])
                        value = false;
                }
                result[x, y] = value;
            }
            return result;
        }
    }
}
