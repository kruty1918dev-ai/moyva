using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IMenuPreviewKingdomPlacementWriter
    {
        void Place(MenuWorldPreviewKingdomPlacementContext context, Vector2Int position, string buildingId);
    }
}
