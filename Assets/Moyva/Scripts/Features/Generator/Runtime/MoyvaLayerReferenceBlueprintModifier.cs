using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Службовий TWC-модифікатор, який дає blueprint-шару стартову маску з іншого blueprint-шару.
    /// Використовується компілятором графа для ноди Layer Ref.
    /// </summary>
    public sealed class MoyvaLayerReferenceBlueprintModifier : BlueprintModifier
    {
        [HideInInspector] public string sourceBlueprintLayerGuid;
        [HideInInspector] public string sourceGraphLayerId;
        [HideInInspector] public string sourceLayerName;

        public override HashSet<Vector2> Execute(HashSet<Vector2> positions, BlueprintLayer layer)
        {
            if (asset == null || string.IsNullOrEmpty(sourceBlueprintLayerGuid))
                return positions ?? new HashSet<Vector2>();

            var sourceLayer = asset.GetBlueprintLayerByGuid(sourceBlueprintLayerGuid);
            if (sourceLayer?.allPositions == null)
                return positions ?? new HashSet<Vector2>();

            return new HashSet<Vector2>(sourceLayer.allPositions);
        }
    }
}
