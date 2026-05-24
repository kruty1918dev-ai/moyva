using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Grid.Runtime
{
    public sealed class OrthogonalGridProjection : IGridProjection
    {
        private readonly Vector2 _cellSize;
        private readonly float _heightScale;

        public GridProjectionMode ProjectionMode => GridProjectionMode.Orthographic2D;
        public GridTopology Topology => GridTopology.Orthogonal;

        public OrthogonalGridProjection()
            : this(null)
        {
        }

        public OrthogonalGridProjection(MoyvaProjectSettingsSO settings)
        {
            settings?.Normalize();
            _cellSize = settings != null ? settings.OrthogonalCellSize : Vector2.one;
            _heightScale = settings != null ? settings.HeightScale : 0.25f;
        }

        public Vector3 GridToWorld(Vector2Int gridPosition)
            => GridToWorld(gridPosition, 0f);

        public Vector3 GridToWorld(Vector2Int gridPosition, float elevation, float layerOffset = 0f)
        {
            return new Vector3(
                gridPosition.x * _cellSize.x,
                gridPosition.y * _cellSize.y,
                elevation * _heightScale + layerOffset);
        }

        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            return new Vector2Int(
                Mathf.RoundToInt(worldPosition.x / _cellSize.x),
                Mathf.RoundToInt(worldPosition.y / _cellSize.y));
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
            float safeWidth = Mathf.Max(1, width) * _cellSize.x;
            float safeHeight = Mathf.Max(1, height) * _cellSize.y;
            return new Bounds(
                new Vector3((safeWidth - _cellSize.x) * 0.5f, (safeHeight - _cellSize.y) * 0.5f, 0f),
                new Vector3(safeWidth, safeHeight, 1f));
        }
    }
}