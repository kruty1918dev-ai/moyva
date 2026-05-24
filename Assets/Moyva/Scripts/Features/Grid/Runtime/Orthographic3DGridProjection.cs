using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Grid.Runtime
{
    public sealed class Orthographic3DGridProjection : IGridProjection
    {
        private readonly float _cellWidth;
        private readonly float _cellDepth;
        private readonly float _heightScale;

        public GridProjectionMode ProjectionMode => GridProjectionMode.Orthographic3D;
        public GridTopology Topology => GridTopology.Layered;
        public GridWorldPlane WorldPlane => GridWorldPlane.XZ;

        public Orthographic3DGridProjection()
            : this(null)
        {
        }

        public Orthographic3DGridProjection(MoyvaProjectSettingsSO settings)
        {
            settings?.Normalize();
            _cellWidth = settings != null ? settings.OrthogonalCellWidth : 1f;
            _cellDepth = settings != null ? settings.OrthogonalCellDepth : 1f;
            _heightScale = settings != null ? settings.HeightScale : 0.25f;
        }

        public Vector3 GridToWorld(Vector2Int gridPosition)
            => GridToWorld(gridPosition, 0f);

        public Vector3 GridToWorld(Vector2Int gridPosition, float elevation, float layerOffset = 0f)
        {
            return new Vector3(
                gridPosition.x * _cellWidth,
                elevation * _heightScale + layerOffset,
                gridPosition.y * _cellDepth);
        }

        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            return new Vector2Int(
                Mathf.RoundToInt(worldPosition.x / _cellWidth),
                Mathf.RoundToInt(worldPosition.z / _cellDepth));
        }

        public IEnumerable<Vector2Int> GetNeighborCandidates(Vector2Int gridPosition)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    yield return new Vector2Int(gridPosition.x + x, gridPosition.y + y);
                }
            }
        }

        public float GetStepDistance(Vector2Int from, Vector2Int to)
            => from.x != to.x && from.y != to.y ? 1.41421356f : 1f;

        public float EstimateDistance(Vector2Int from, Vector2Int to)
        {
            float dx = Mathf.Abs(from.x - to.x);
            float dy = Mathf.Abs(from.y - to.y);
            return (dx + dy) + (1.41421356f - 2f) * Mathf.Min(dx, dy);
        }

        public Bounds GetWorldBounds(int width, int height)
        {
            float safeWidth = Mathf.Max(1, width) * _cellWidth;
            float safeDepth = Mathf.Max(1, height) * _cellDepth;
            return new Bounds(
                new Vector3((safeWidth - _cellWidth) * 0.5f, 0f, (safeDepth - _cellDepth) * 0.5f),
                new Vector3(safeWidth, Mathf.Max(1f, _heightScale), safeDepth));
        }
    }
}
