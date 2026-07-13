using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionBuildGridTileFilter
    {
        bool ShouldRender(Vector2Int position);
        ConstructionBuildGridTileVisualState ResolveVisualState(Vector2Int position);
        bool ShouldRenderForPlacement(Vector2Int position, string buildingId, Vector2Int? ignoredPendingPosition = null);
    }
}
