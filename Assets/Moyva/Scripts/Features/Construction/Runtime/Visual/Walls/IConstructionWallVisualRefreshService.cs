using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IConstructionWallVisualRefreshService
    {
        void RefreshPlacedNeighborhood(Vector2Int center);
        void RefreshPreviewNeighborhood(Vector2Int center, string buildingId);
    }
}
