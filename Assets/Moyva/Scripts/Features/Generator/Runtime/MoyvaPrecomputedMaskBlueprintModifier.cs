using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// TWC bridge modifier that injects a graph-executed output mask into a blueprint layer.
    /// This keeps regular graph nodes (bool math, WFC maps, layer refs) compatible with
    /// TileWorldCreator build layers, which otherwise only understand blueprint positions.
    /// </summary>
    public sealed class MoyvaPrecomputedMaskBlueprintModifier : BlueprintModifier
    {
        [HideInInspector, SerializeField] private List<Vector2> _positions = new();

        [HideInInspector] public string sourceGraphLayerId;
        [HideInInspector] public string sourceLayerName;

        public int PositionCount => _positions?.Count ?? 0;

        public void SetPositions(IEnumerable<Vector2> positions)
        {
            _positions ??= new List<Vector2>();
            _positions.Clear();

            if (positions == null)
                return;

            var seen = new HashSet<Vector2>();
            foreach (var position in positions)
            {
                var cell = new Vector2(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
                if (seen.Add(cell))
                    _positions.Add(cell);
            }
        }

        public override HashSet<Vector2> Execute(HashSet<Vector2> positions, BlueprintLayer layer)
        {
            return _positions != null
                ? new HashSet<Vector2>(_positions)
                : positions ?? new HashSet<Vector2>();
        }
    }
}
