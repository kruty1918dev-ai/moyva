using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Grid.Runtime
{
    public sealed class HexAxialGridProjection : IGridProjection
    {
        private static readonly Vector2Int[] NeighborOffsets =
        {
            new Vector2Int(1, 0),
            new Vector2Int(1, -1),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(-1, 1),
            new Vector2Int(0, 1),
        };

        private readonly float _radius;
        private readonly float _heightScale;
        private readonly HexOrientation _orientation;

        public GridProjectionMode ProjectionMode => _orientation == HexOrientation.PointyTop
            ? GridProjectionMode.HexPointy2D
            : GridProjectionMode.HexFlat2D;

        public GridTopology Topology => GridTopology.HexAxial;
        public GridWorldPlane WorldPlane => GridWorldPlane.XY;

        public HexAxialGridProjection()
            : this(null)
        {
        }

        public HexAxialGridProjection(MoyvaProjectSettingsSO settings)
        {
            settings?.Normalize();
            _radius = settings != null ? settings.HexRadius : 0.5f;
            _heightScale = settings != null ? settings.HeightScale : 0.25f;
            _orientation = settings?.HexOrientation ?? HexOrientation.PointyTop;
        }

        public Vector3 GridToWorld(Vector2Int gridPosition)
            => GridToWorld(gridPosition, 0f);

        public Vector3 GridToWorld(Vector2Int gridPosition, float elevation, float layerOffset = 0f)
        {
            float q = gridPosition.x;
            float r = gridPosition.y;
            float x;
            float y;

            if (_orientation == HexOrientation.PointyTop)
            {
                x = _radius * Mathf.Sqrt(3f) * (q + r * 0.5f);
                y = _radius * 1.5f * r;
            }
            else
            {
                x = _radius * 1.5f * q;
                y = _radius * Mathf.Sqrt(3f) * (r + q * 0.5f);
            }

            return new Vector3(x, y, elevation * _heightScale + layerOffset);
        }

        public Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            float q;
            float r;

            if (_orientation == HexOrientation.PointyTop)
            {
                q = (Mathf.Sqrt(3f) / 3f * worldPosition.x - 1f / 3f * worldPosition.y) / _radius;
                r = (2f / 3f * worldPosition.y) / _radius;
            }
            else
            {
                q = (2f / 3f * worldPosition.x) / _radius;
                r = (-1f / 3f * worldPosition.x + Mathf.Sqrt(3f) / 3f * worldPosition.y) / _radius;
            }

            return RoundAxial(q, r);
        }

        public IEnumerable<Vector2Int> GetNeighborCandidates(Vector2Int gridPosition)
        {
            for (int i = 0; i < NeighborOffsets.Length; i++)
                yield return gridPosition + NeighborOffsets[i];
        }

        public float GetStepDistance(Vector2Int from, Vector2Int to) => HexDistance(from, to);

        public float EstimateDistance(Vector2Int from, Vector2Int to) => HexDistance(from, to);

        public Bounds GetWorldBounds(int width, int height)
        {
            var bounds = new Bounds(GridToWorld(Vector2Int.zero), Vector3.zero);
            bounds.Encapsulate(GridToWorld(new Vector2Int(Mathf.Max(0, width - 1), 0)));
            bounds.Encapsulate(GridToWorld(new Vector2Int(0, Mathf.Max(0, height - 1))));
            bounds.Encapsulate(GridToWorld(new Vector2Int(Mathf.Max(0, width - 1), Mathf.Max(0, height - 1))));
            bounds.Expand(new Vector3(_radius * 2f, _radius * 2f, 1f));
            return bounds;
        }

        private static Vector2Int RoundAxial(float q, float r)
        {
            float x = q;
            float z = r;
            float y = -x - z;

            int roundedX = Mathf.RoundToInt(x);
            int roundedY = Mathf.RoundToInt(y);
            int roundedZ = Mathf.RoundToInt(z);

            float xDiff = Mathf.Abs(roundedX - x);
            float yDiff = Mathf.Abs(roundedY - y);
            float zDiff = Mathf.Abs(roundedZ - z);

            if (xDiff > yDiff && xDiff > zDiff)
                roundedX = -roundedY - roundedZ;
            else if (yDiff > zDiff)
                roundedY = -roundedX - roundedZ;
            else
                roundedZ = -roundedX - roundedY;

            return new Vector2Int(roundedX, roundedZ);
        }

        private static int HexDistance(Vector2Int from, Vector2Int to)
        {
            int dq = Mathf.Abs(from.x - to.x);
            int dr = Mathf.Abs(from.y - to.y);
            int ds = Mathf.Abs((-from.x - from.y) - (-to.x - to.y));
            return (dq + dr + ds) / 2;
        }
    }
}