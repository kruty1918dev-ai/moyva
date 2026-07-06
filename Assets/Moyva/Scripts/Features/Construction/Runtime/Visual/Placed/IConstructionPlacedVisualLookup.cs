using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionPlacedVisualLookup
    {
        bool TryGetPlacedVisual(Vector2Int position, out GameObject visual);
    }
}
