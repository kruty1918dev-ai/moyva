using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Grid.Runtime
{
    public sealed class IsometricGridProjection : IGridProjection
    {
        private readonly float _halfWidth;
        private readonly float _halfHeight;
        private readonly float _cellWidth;
        private readonly float _cellDepth;
        private readonly float _heightScale;
        private readonly GridProjectionMode _projectionMode;
        private readonly bool _uses3DWorldPlane;

        public GridProjectionMode ProjectionMode => _projectionMode;
        public GridTopology Topology => _uses3DWorldPlane ? GridTopology.Layered : GridTopology.Orthogonal;
        public GridWorldPlane WorldPlane => _uses3DWorldPlane ? GridWorldPlane.XZ : GridWorldPlane.XY;

        public IsometricGridProjection()
            : this(null)
        {
        }

        public IsometricGridProjection(MoyvaProjectSettingsSO settings)
        {
            settings?.Normalize();
            Vector2 tileSize = settings != null ? settings.IsometricTileSize : new Vector2(1f, 0.5f);
            _halfWidth = Mathf.Max(0.01f, tileSize.x * 0.5f);
            _halfHeight = Mathf.Max(0.01f, tileSize.y * 0.5f);
            _cellWidth = settings != null ? settings.OrthogonalCellWidth : 1f;
            _cellDepth = settings != null ? settings.OrthogonalCellDepth : 1f;
            _heightScale = settings != null ? settings.HeightScale : 0.25f;
            _projectionMode = settings != null && settings.DefaultProjectionMode == GridProjectionMode.Isometric3DPreview
                ? GridProjectionMode.Isometric3DPreview
                : GridProjectionMode.Isometric2D;
            _uses3DWorldPlane = _projectionMode == GridProjectionMode.Isometric3DPreview;
        }

        public Vector3 GridToWorld(Vector2Int gridPosition)
            => GridToWorld(gridPosition, 0f);

        public Vector3 GridToWorld(Vector2Int gridPosition, float elevation, float layerOffset = 0f)
        {
            float worldHeight = elevation * _heightScale + layerOffset;
            if (_uses3DWorldPlane)
            {
                return new Vector3(
                    gridPosition.x * _cellWidth,
                    worldHeight,
                    gridPosition.y * _cellDepth);
            }

            float isoX = (gridPosition.x - gridPosition.y) * _halfWidth;
            float isoY = (gridPosition.x + gridPosition.y) * _halfHeight;
            return new Vector3(isoX, isoY, worldHeight);
        }

        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            if (_uses3DWorldPlane)
            {
                return new Vector2Int(
                    Mathf.RoundToInt(worldPosition.x / _cellWidth),
                    Mathf.RoundToInt(worldPosition.z / _cellDepth));
            }

            float projectedX = worldPosition.x / _halfWidth;
            float projectedY = worldPosition.y / _halfHeight;
            return new Vector2Int(
                Mathf.RoundToInt((projectedY + projectedX) * 0.5f),
                Mathf.RoundToInt((projectedY - projectedX) * 0.5f));
        }

        public IEnumerable<Vector2Int> GetNeighborCandidates(Vector2Int gridPosition)
        {
            yield return new Vector2Int(gridPosition.x + 1, gridPosition.y);
            yield return new Vector2Int(gridPosition.x - 1, gridPosition.y);
            yield return new Vector2Int(gridPosition.x, gridPosition.y + 1);
            yield return new Vector2Int(gridPosition.x, gridPosition.y - 1);
        }

        public float GetStepDistance(Vector2Int from, Vector2Int to)
            => Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);

        public float EstimateDistance(Vector2Int from, Vector2Int to)
            => Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);

        public Bounds GetWorldBounds(int width, int height)
        {
            if (_uses3DWorldPlane)
            {
                float safeWidth = Mathf.Max(1, width) * _cellWidth;
                float safeDepth = Mathf.Max(1, height) * _cellDepth;
                return new Bounds(
                    new Vector3((safeWidth - _cellWidth) * 0.5f, 0f, (safeDepth - _cellDepth) * 0.5f),
                    new Vector3(safeWidth, Mathf.Max(1f, _heightScale), safeDepth));
            }

            Vector3 first = GridToWorld(Vector2Int.zero);
            Vector3 right = GridToWorld(new Vector2Int(Mathf.Max(0, width - 1), 0));
            Vector3 top = GridToWorld(new Vector2Int(0, Mathf.Max(0, height - 1)));
            Vector3 far = GridToWorld(new Vector2Int(Mathf.Max(0, width - 1), Mathf.Max(0, height - 1)));
            var bounds = new Bounds(first, Vector3.zero);
            bounds.Encapsulate(right);
            bounds.Encapsulate(top);
            bounds.Encapsulate(far);
            bounds.Expand(new Vector3(_halfWidth * 2f, _halfHeight * 2f, 1f));
            return bounds;
        }
    }
}