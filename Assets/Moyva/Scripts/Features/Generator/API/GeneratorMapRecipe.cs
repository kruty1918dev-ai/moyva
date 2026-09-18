using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Jsonization;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    /// <summary>
    /// JSON-authored map generation recipe: an ordered list of layers, each with
    /// a linear sequence of mask steps, plus object placements. This is the sole
    /// source of truth for procedural map generation (replaces the node graph).
    /// </summary>
    [System.Serializable]
    public sealed class GeneratorMapRecipe : MoyvaJsonConfigObject
    {
        [Tooltip("0 = resolve seed from the launch context; non-zero pins this recipe's seed.")]
        public int Seed;

        public GeneratorMapSharedSettings SharedSettings = new();

        [Tooltip("Tile registry used to resolve logical tile ids for generated layers.")]
        public TileRegistrySO TileRegistry;

        [Tooltip("Shared generator semantics (water-like tile ids, separators, ...).")]
        public SharedGeneratorSettingsSO SharedGeneratorSettings;

        [Tooltip("Ordered generator layers. Earlier layers are available as mask references to later ones.")]
        public List<GeneratorMapLayer> Layers = new();

        [Tooltip("Ordered object scatter passes evaluated after all layer masks.")]
        public List<GeneratorObjectPlacement> ObjectPlacements = new();

        public IEnumerable<GeneratorMapLayer> OrderedEnabledLayers()
        {
            if (Layers == null)
                yield break;

            foreach (var layer in Layers)
            {
                if (layer != null && layer.Enabled)
                    yield return layer;
            }
        }
    }
}
