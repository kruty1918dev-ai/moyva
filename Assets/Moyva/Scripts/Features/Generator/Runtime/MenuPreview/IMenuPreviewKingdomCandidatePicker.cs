using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IMenuPreviewKingdomCandidatePicker
    {
        Vector2Int? PickInZone(
            MenuWorldPreviewKingdomPlacementContext context,
            RectInt zone,
            string buildingId,
            Vector2Int? farFrom,
            int minDistance);

        Vector2Int? PickNear(
            MenuWorldPreviewKingdomPlacementContext context,
            Vector2Int center,
            int radius,
            string buildingId);

        Vector2Int? PickSmallTown(MenuWorldPreviewKingdomPlacementContext context);
    }
}
