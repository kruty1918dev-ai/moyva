using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IMenuPreviewKingdomPlacementValidator
    {
        bool IsValid(
            MenuWorldPreviewKingdomPlacementContext context,
            Vector2Int position,
            string buildingId,
            Vector2Int? farFrom,
            int minDistance);
    }
}
