using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IMenuPreviewKingdomPlacementGeometry
    {
        bool IsInBounds(MenuWorldPreviewKingdomPlacementContext context, Vector2Int position);
        int Manhattan(Vector2Int a, Vector2Int b);
        RectInt ClampToMap(MenuWorldPreviewKingdomPlacementContext context, RectInt zone);
    }
}
