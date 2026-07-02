using System;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal delegate Vector2Int StartingPositionResolveRepairCenter(bool[,] snapshot, int width, int height);

    internal interface IStartingPositionCameraTargetResolver
    {
        Vector2Int ResolveStartupCameraTarget(
            int width,
            int height,
            bool preferStartTile,
            StartingPositionTryResolvePosition tryGetLocalSpawnPosition,
            StartingPositionResolveRepairCenter resolveRepairCenter);

        bool TryGetClosestUnitPosition(Vector2Int origin, out Vector2Int position);
    }

    internal sealed class StartingPositionCameraTargetResolver
        : IStartingPositionCameraTargetResolver
    {
        private readonly IFogOfWarService _fogOfWarService;
        private readonly IUnitService _unitService;
        private readonly IStartingPositionState _startingPositionState;

        public StartingPositionCameraTargetResolver(
            IFogOfWarService fogOfWarService,
            IUnitService unitService,
            IStartingPositionState startingPositionState)
        {
            _fogOfWarService = fogOfWarService;
            _unitService = unitService;
            _startingPositionState = startingPositionState;
        }

        public Vector2Int ResolveStartupCameraTarget(
            int width,
            int height,
            bool preferStartTile,
            StartingPositionTryResolvePosition tryGetLocalSpawnPosition,
            StartingPositionResolveRepairCenter resolveRepairCenter)
        {
            if (preferStartTile && _startingPositionState.IsSet)
                return StartingPositionMapUtility.ClampToMap(_startingPositionState.StartPosition, width, height);

            Vector2Int preferred = ResolvePreferredPlayerPosition(width, height, tryGetLocalSpawnPosition, resolveRepairCenter);
            return ResolveVisibleCameraTarget(preferred, width, height);
        }

        public Vector2Int ResolvePreferredPlayerPosition(
            int width,
            int height,
            StartingPositionTryResolvePosition tryGetLocalSpawnPosition,
            StartingPositionResolveRepairCenter resolveRepairCenter)
        {
            bool[,] snapshot = _fogOfWarService.GetExploredSnapshot();
            Vector2Int repairCenter = resolveRepairCenter != null
                ? resolveRepairCenter(snapshot, width, height)
                : StartingPositionMapUtility.FindRepairCenter(snapshot, width, height);
            if (TryGetClosestUnitPosition(repairCenter, out Vector2Int unitPosition))
                return StartingPositionMapUtility.ClampToMap(unitPosition, width, height);

            if (tryGetLocalSpawnPosition != null && tryGetLocalSpawnPosition(out Vector2Int localSpawn))
                return StartingPositionMapUtility.ClampToMap(localSpawn, width, height);

            if (_startingPositionState.IsSet)
                return StartingPositionMapUtility.ClampToMap(_startingPositionState.StartPosition, width, height);

            return StartingPositionMapUtility.FindRepairCenter(_fogOfWarService.GetExploredSnapshot(), width, height);
        }

        public bool TryGetClosestUnitPosition(Vector2Int origin, out Vector2Int position)
        {
            position = default;
            var unitIds = _unitService?.GetAllUnitIds();
            if (unitIds == null || unitIds.Count == 0)
                return false;

            int bestDistance = int.MaxValue;
            foreach (string unitId in unitIds)
            {
                if (!_unitService.TryGetUnitPosition(unitId, out Vector2Int candidate))
                    continue;

                int distance = Mathf.Abs(candidate.x - origin.x) + Mathf.Abs(candidate.y - origin.y);
                if (distance >= bestDistance)
                    continue;

                bestDistance = distance;
                position = candidate;
            }

            return bestDistance != int.MaxValue;
        }

        public Vector2Int ResolveVisibleCameraTarget(Vector2Int preferred, int width, int height)
        {
            Vector2Int clamped = StartingPositionMapUtility.ClampToMap(preferred, width, height);
            if (_fogOfWarService.IsVisible(clamped))
                return clamped;

            if (TryFindNearestVisibleTile(clamped, width, height, out Vector2Int visiblePosition))
            {
                Debug.LogWarning($"[Bootstrap] Камера мала стартувати над чорним туманом у {clamped}. Переміщено до найближчої видимої ділянки {visiblePosition}.");
                return visiblePosition;
            }

            if (TryFindNearestExploredTile(clamped, width, height, out Vector2Int exploredPosition))
            {
                Debug.LogWarning($"[Bootstrap] Видимих тайлів для старту камери не знайдено. Використано найближчу розвідану ділянку {exploredPosition}.");
                return exploredPosition;
            }

            return clamped;
        }

        public bool TryFindNearestVisibleTile(Vector2Int origin, int width, int height, out Vector2Int position)
            => TryFindNearestFogTile(origin, width, height, _fogOfWarService.IsVisible, out position);

        public bool TryFindNearestExploredTile(Vector2Int origin, int width, int height, out Vector2Int position)
            => TryFindNearestFogTile(origin, width, height, _fogOfWarService.IsExplored, out position);

        public static bool TryFindNearestFogTile(
            Vector2Int origin,
            int width,
            int height,
            Func<Vector2Int, bool> predicate,
            out Vector2Int position)
        {
            width = Mathf.Max(1, width);
            height = Mathf.Max(1, height);
            origin = StartingPositionMapUtility.ClampToMap(origin, width, height);

            int maxRadius = width + height;
            for (int radius = 0; radius <= maxRadius; radius++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    int dy = radius - Mathf.Abs(dx);
                    if (TryMatchFogTile(origin.x + dx, origin.y + dy, width, height, predicate, out position))
                        return true;

                    if (dy != 0 && TryMatchFogTile(origin.x + dx, origin.y - dy, width, height, predicate, out position))
                        return true;
                }
            }

            position = default;
            return false;
        }

        public static bool TryMatchFogTile(
            int x,
            int y,
            int width,
            int height,
            Func<Vector2Int, bool> predicate,
            out Vector2Int position)
        {
            position = default;
            if (x < 0 || x >= width || y < 0 || y >= height)
                return false;

            var candidate = new Vector2Int(x, y);
            if (!predicate(candidate))
                return false;

            position = candidate;
            return true;
        }
    }
}
