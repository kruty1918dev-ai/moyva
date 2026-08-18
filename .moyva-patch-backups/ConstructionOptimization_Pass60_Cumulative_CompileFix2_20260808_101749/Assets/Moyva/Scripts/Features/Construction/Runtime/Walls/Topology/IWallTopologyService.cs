using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IWallTopologyService
    {
        bool IsWallOrGate(string buildingId);
        bool IsWall(string buildingId);
        bool IsGate(string buildingId);
        bool TryBuildPlacedMask(Vector2Int position, string buildingId, out WallCollectionDefinition collection, out TopologyNeighborMask mask);
        bool TryBuildPreviewMask(Vector2Int position, string buildingId, out WallCollectionDefinition collection, out TopologyNeighborMask mask);
        bool IsHorizontalWallSegment(Vector2Int position, WallCollectionDefinition collection, bool includePendingNeighbors);
    }
}
