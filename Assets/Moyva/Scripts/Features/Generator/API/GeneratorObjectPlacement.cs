using System.Collections.Generic;
using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    /// <summary>
    /// One object scatter pass authored inside a <see cref="GeneratorMapRecipe"/>.
    /// The scatter seeds from the final mask of <see cref="TargetLayerId"/>
    /// (or the owning layer when empty) and produces a generated TWC object layer.
    /// </summary>
    [System.Serializable]
    public sealed class GeneratorObjectPlacement
    {
        [Tooltip("Name of the generated TWC blueprint/build object layer.")]
        public string LayerName = "Props";

        [Tooltip("Recipe layer whose final mask seeds the scatter. Empty = the owning layer.")]
        public string TargetLayerId;

        [Tooltip("Optional recipe layer whose final mask excludes cells.")]
        public string ExclusionLayerId;

        [Tooltip("Prefab variants used by the generated TWC Object Build Layer.")]
        public List<ObjectPrefabEntry> Prefabs = new();

        [Tooltip("Scatter rules mapped into the generated TWC Object Build Layer.")]
        public ObjectPlacementRule Rule = new();

        [Tooltip("Cluster scatter settings; Enabled switches to cluster scatter.")]
        public ClusterSettings Cluster = new();

        [Tooltip("Optional generated grass-card settings appended as a prefab variant.")]
        public GrassCardSettings Grass;
    }
}
