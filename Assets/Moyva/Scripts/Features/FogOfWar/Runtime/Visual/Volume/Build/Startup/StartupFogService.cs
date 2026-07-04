using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed class StartupFogService : IFogOfWarService
    {
        private readonly int _width;
        private readonly int _height;
        private readonly bool _hasVisibleReveal;
        private readonly Vector2Int _visibleCenter;
        private readonly int _visibleRadius;
        private readonly FogRevealShape _visibleShape;

        public StartupFogService(int width, int height)
        {
            _width = Mathf.Max(1, width);
            _height = Mathf.Max(1, height);
        }

        public StartupFogService(int width, int height, Vector2Int visibleCenter, int visibleRadius, FogRevealShape visibleShape, bool keepVisible)
            : this(width, height)
        {
            _hasVisibleReveal = keepVisible;
            _visibleCenter = visibleCenter;
            _visibleRadius = Mathf.Max(0, visibleRadius);
            _visibleShape = visibleShape;
        }

        public void Initialize(int width, int height) { }
        public void RegisterUnit(string unitId, Vector2Int position, int visionRange) { }
        public void UpdateUnitVisionRange(string unitId, int visionRange) { }
        public void RegisterFixedVisionArea(string areaId, Vector2Int position, int visionRange, FogRevealShape shape) { }
        public void RevealArea(Vector2Int center, int radius, FogRevealShape shape, bool keepVisible, string visibleAreaId = null) { }
        public void UpdateUnitPosition(string unitId, Vector2Int newPosition) { }
        public void UnregisterUnit(string unitId) { }

        public FogStateType GetFogState(Vector2Int position)
        {
            if (_hasVisibleReveal && IsInsideVisibleReveal(position))
                return FogStateType.Visible;

            return FogStateType.Unexplored;
        }

        public bool IsVisible(Vector2Int position) => GetFogState(position) == FogStateType.Visible;
        public bool IsExplored(Vector2Int position) => IsVisible(position);
        public bool[,] GetExploredSnapshot() => new bool[_width, _height];
        public void LoadFromSnapshot(bool[,] explored) { }
        public IReadOnlyCollection<Vector2Int> GetLastDirtyTiles() => Array.Empty<Vector2Int>();

        private bool IsInsideVisibleReveal(Vector2Int position)
        {
            if (position.x < 0 || position.y < 0 || position.x >= _width || position.y >= _height)
                return false;

            int dx = position.x - _visibleCenter.x;
            int dy = position.y - _visibleCenter.y;
            float radiusWithCellCoverage = _visibleRadius + 0.5f;
            float sqrRadius = radiusWithCellCoverage * radiusWithCellCoverage;
            return _visibleShape switch
            {
                FogRevealShape.Square => Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) <= _visibleRadius,
                FogRevealShape.Diamond => Mathf.Abs(dx) + Mathf.Abs(dy) <= _visibleRadius,
                _ => dx * dx + dy * dy <= sqrRadius,
            };
        }
    }
}
