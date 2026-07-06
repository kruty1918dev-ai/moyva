using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MenuPreviewKingdomPlacementWriter : IMenuPreviewKingdomPlacementWriter
    {
        public void Place(MenuWorldPreviewKingdomPlacementContext context, Vector2Int position, string buildingId)
        {
            context.Data.BuildingMap[position.x, position.y] = buildingId;
            context.NewPlacements.Add(position);
        }
    }
}
