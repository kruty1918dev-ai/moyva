using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Grid.API
{
    public interface IGridProjection
    {
        GridProjectionMode ProjectionMode { get; }
        GridTopology Topology { get; }

        Vector3 GridToWorld(Vector2Int gridPosition);
        Vector3 GridToWorld(Vector2Int gridPosition, float elevation, float layerOffset = 0f);
        Vector2Int WorldToGrid(Vector3 worldPosition);
        IEnumerable<Vector2Int> GetNeighborCandidates(Vector2Int gridPosition);
        float GetStepDistance(Vector2Int from, Vector2Int to);
        float EstimateDistance(Vector2Int from, Vector2Int to);
        Bounds GetWorldBounds(int width, int height);
    }
}