using System.Collections.Generic;
using Kruty1918.Moyva.Pathfinding.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal interface IStartingPositionSelector
    {
        List<Vector2Int> PickStartingPositions(WorldGeneratedDataSignal signal, int positionsCount);
        Vector2Int PickStartingPosition(Vector2Int baseMapSize);
    }

    internal sealed class StartingPositionSelector
        : IStartingPositionSelector
    {
        private const string DirectDiagTag = "[MoyvaDirectStartDiag]";
        private readonly StartingPositionInitializerSettings _settings;
        private readonly IPathfinder _pathfinder;

        public StartingPositionSelector(StartingPositionInitializerSettings settings, IPathfinder pathfinder)
        {
            _settings = settings;
            _pathfinder = pathfinder;
        }

        public List<Vector2Int> PickStartingPositions(WorldGeneratedDataSignal signal, int positionsCount)
        {
            var positions = new List<Vector2Int>(positionsCount);
            int attempts = Mathf.Max(1, _settings.startCandidateAttempts);
            Debug.Log($"{DirectDiagTag} Selector.ENTER requestedCount={positionsCount}, map={signal.Width}x{signal.Height}, hasHeightMap={signal.HeightMap != null}, minHeight={Mathf.Min(_settings.startMinHeight, _settings.startMaxHeight)}, maxHeight={Mathf.Max(_settings.startMinHeight, _settings.startMaxHeight)}, minDistance={Mathf.Max(1, _settings.minAStarDistanceBetweenPlayers)}, attempts={attempts}.");

            for (int positionIndex = 0; positionIndex < positionsCount; positionIndex++)
            {
                if (TryPickStartingPosition(signal, positions, attempts, out Vector2Int position))
                    positions.Add(position);
                else
                    Debug.LogWarning($"[Bootstrap] Не вдалось знайти стартову позицію #{positionIndex + 1} із заданими обмеженнями.");
            }

            if (positions.Count > 1)
                Debug.Log($"[Bootstrap] Host зарезервував стартові позиції: {string.Join(", ", positions)}");

            if (positions.Count == 0)
                Debug.Log($"{DirectDiagTag} Selector.FAIL reason=no-valid-positions requested={positionsCount}, candidates=0, map={signal.Width}x{signal.Height}.");

            Debug.Log($"{DirectDiagTag} Selector.RESULT selected={positions.Count}, selectedShort={FormatPositions(positions)}.");

            return positions;
        }

        public bool TryPickStartingPosition(
            WorldGeneratedDataSignal signal,
            IReadOnlyList<Vector2Int> existingPositions,
            int attempts,
            out Vector2Int position)
        {
            Vector2Int baseMapSize = StartingPositionMapUtility.ResolveBaseMapSize(signal);
            int rejectedHeight = 0;
            int rejectedDistance = 0;
            for (int attempt = 0; attempt < attempts; attempt++)
            {
                Vector2Int candidate = PickStartingPosition(baseMapSize);
                if (!IsValidStartHeight(signal, candidate))
                {
                    rejectedHeight++;
                    continue;
                }

                if (!HasRequiredDistance(candidate, existingPositions))
                {
                    rejectedDistance++;
                    continue;
                }

                Debug.Log($"{DirectDiagTag} Selector.Candidates count=1, rejectedOutOfBounds=0, rejectedHeight={rejectedHeight}, rejectedDistance={rejectedDistance}, rejectedWater=0.");
                position = candidate;
                return true;
            }

            for (int x = 0; x < baseMapSize.x; x++)
            {
                for (int y = 0; y < baseMapSize.y; y++)
                {
                    Vector2Int candidate = new Vector2Int(x, y);
                    if (IsInsideStartBounds(candidate, baseMapSize.x, baseMapSize.y) &&
                        IsValidStartHeight(signal, candidate) &&
                        HasRequiredDistance(candidate, existingPositions))
                    {
                        Debug.Log($"{DirectDiagTag} Selector.Candidates count=1, rejectedOutOfBounds=0, rejectedHeight={rejectedHeight}, rejectedDistance={rejectedDistance}, rejectedWater=0.");
                        position = candidate;
                        return true;
                    }
                }
            }

            Debug.Log($"{DirectDiagTag} Selector.Candidates count=0, rejectedOutOfBounds=0, rejectedHeight={rejectedHeight}, rejectedDistance={rejectedDistance}, rejectedWater=0.");
            position = Vector2Int.zero;
            return false;
        }

        public Vector2Int PickStartingPosition(int width, int height)
        {
            return PickStartingPosition(new Vector2Int(width, height));
        }

        public Vector2Int PickStartingPosition(Vector2Int baseMapSize)
        {
            int width = Mathf.Max(0, baseMapSize.x);
            int height = Mathf.Max(0, baseMapSize.y);
            if (width <= 0 || height <= 0)
                return Vector2Int.zero;

            Vector2Int position = StartingPositionMapUtility.PickRuntimeRandomPoint(
                width,
                height,
                _settings.minMarginFromBorder,
                _settings.relativeMarginFactor,
                out int seed);
            Debug.Log($"{DirectDiagTag} Selector.PickRandom position={position}, seed={seed}, map={width}x{height}.");
            return position;
        }

        public bool IsInsideStartBounds(Vector2Int position, int width, int height)
        {
            if (width <= 0 || height <= 0)
                return false;

            int minSide = Mathf.Min(width, height);
            int relativeMargin = Mathf.FloorToInt(minSide * Mathf.Clamp01(_settings.relativeMarginFactor));
            int margin = Mathf.Max(_settings.minMarginFromBorder, relativeMargin);

            int xMin = Mathf.Clamp(margin, 0, width - 1);
            int xMax = Mathf.Clamp(width - margin - 1, xMin, width - 1);
            int yMin = Mathf.Clamp(margin, 0, height - 1);
            int yMax = Mathf.Clamp(height - margin - 1, yMin, height - 1);

            return position.x >= xMin && position.x <= xMax && position.y >= yMin && position.y <= yMax;
        }

        public bool IsValidStartHeight(WorldGeneratedDataSignal signal, Vector2Int position)
        {
            if (signal.HeightMap == null)
                return !_settings.requireHeightMapForStart;

            if (position.x < 0 || position.x >= signal.HeightMap.GetLength(0) ||
                position.y < 0 || position.y >= signal.HeightMap.GetLength(1))
            {
                return false;
            }

            float minHeight = Mathf.Min(_settings.startMinHeight, _settings.startMaxHeight);
            float maxHeight = Mathf.Max(_settings.startMinHeight, _settings.startMaxHeight);
            float height = signal.HeightMap[position.x, position.y];
            return height >= minHeight && height <= maxHeight;
        }

        public bool HasRequiredDistance(Vector2Int candidate, IReadOnlyList<Vector2Int> existingPositions)
        {
            if (existingPositions == null || existingPositions.Count == 0)
                return true;

            int minDistance = Mathf.Max(1, _settings.minAStarDistanceBetweenPlayers);
            for (int index = 0; index < existingPositions.Count; index++)
            {
                int distance = ResolveStartDistance(candidate, existingPositions[index]);
                if (distance < minDistance)
                    return false;
            }

            return true;
        }

        public int ResolveStartDistance(Vector2Int first, Vector2Int second)
        {
            if (_pathfinder != null)
            {
                List<Vector2Int> path = _pathfinder.FindPath(first, second);
                if (path != null && path.Count > 0)
                    return Mathf.Max(0, path.Count - 1);
            }

            return Mathf.CeilToInt(Vector2.Distance(first, second));
        }

        private static string FormatPositions(IReadOnlyList<Vector2Int> positions)
        {
            if (positions == null || positions.Count == 0)
                return "[]";

            int count = Mathf.Min(positions.Count, 4);
            var parts = new string[count];
            for (int index = 0; index < count; index++)
                parts[index] = positions[index].ToString();

            return positions.Count > count
                ? $"[{string.Join(", ", parts)}, ...]"
                : $"[{string.Join(", ", parts)}]";
        }
    }
}
