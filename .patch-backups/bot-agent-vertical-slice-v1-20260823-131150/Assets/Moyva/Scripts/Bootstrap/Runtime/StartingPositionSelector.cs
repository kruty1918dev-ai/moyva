using System.Collections.Generic;
using Kruty1918.Moyva.Pathfinding.API;
using Kruty1918.Moyva.SaveSystem;
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

            bool isDirectGameplay =
                signal.Source == WorldGeneratedDataSource.DirectGameplayTest
                || GameLaunchContext.Mode == GameLaunchMode.DirectGameplayTest
                || GameLaunchContext.Source == GameLaunchSource.DirectGameplayTest;

            for (int positionIndex = 0; positionIndex < positionsCount; positionIndex++)
            {
                if (TryPickStartingPosition(signal, positions, attempts, out Vector2Int position))
                {
                    positions.Add(position);
                    continue;
                }

                if (isDirectGameplay &&
                    TryPickBestEffortDirectPosition(
                        signal,
                        positions,
                        requireValidHeight: true,
                        out position))
                {
                    positions.Add(position);
                    Debug.LogWarning(
                        $"{DirectDiagTag} Selector.DIRECT_FALLBACK_RELAX_DISTANCE " +
                        $"slot={positionIndex}, position={position}, selected={positions.Count}/{positionsCount}. " +
                        "Configured minimum inter-player distance could not be satisfied; " +
                        "selected the farthest unique valid-height start instead.");
                    continue;
                }

                if (isDirectGameplay &&
                    TryPickBestEffortDirectPosition(
                        signal,
                        positions,
                        requireValidHeight: false,
                        out position))
                {
                    positions.Add(position);
                    Debug.LogWarning(
                        $"{DirectDiagTag} Selector.DIRECT_FALLBACK_RELAX_HEIGHT " +
                        $"slot={positionIndex}, position={position}, selected={positions.Count}/{positionsCount}. " +
                        "No additional unique tile satisfied the configured height band; " +
                        "direct Human+Bot topology is preserved using the farthest in-bounds tile.");
                    continue;
                }

                Debug.LogWarning(
                    $"[Bootstrap] Не вдалось знайти стартову позицію #{positionIndex + 1} " +
                    "навіть після direct-mode recovery.");
            }

            if (positions.Count > 1)
                Debug.Log($"[Bootstrap] Host зарезервував стартові позиції: {string.Join(", ", positions)}");

            if (positions.Count == 0)
                Debug.Log($"{DirectDiagTag} Selector.FAIL reason=no-valid-positions requested={positionsCount}, candidates=0, map={signal.Width}x{signal.Height}.");

            Debug.Log($"{DirectDiagTag} Selector.RESULT selected={positions.Count}, selectedShort={FormatPositions(positions)}.");

            return positions;
        }

        private bool TryPickBestEffortDirectPosition(
            WorldGeneratedDataSignal signal,
            IReadOnlyList<Vector2Int> existingPositions,
            bool requireValidHeight,
            out Vector2Int position)
        {
            Vector2Int baseMapSize =
                StartingPositionMapUtility.ResolveBaseMapSize(signal);

            bool found = false;
            Vector2Int best = default;
            float bestDistanceScore = float.MinValue;

            for (int x = 0; x < baseMapSize.x; x++)
            {
                for (int y = 0; y < baseMapSize.y; y++)
                {
                    var candidate = new Vector2Int(x, y);

                    if (!IsInsideStartBounds(
                            candidate,
                            baseMapSize.x,
                            baseMapSize.y))
                    {
                        continue;
                    }

                    if (ContainsPosition(existingPositions, candidate))
                        continue;

                    if (requireValidHeight &&
                        !IsValidStartHeight(signal, candidate))
                    {
                        continue;
                    }

                    float score =
                        ResolveMinimumEuclideanDistance(
                            candidate,
                            existingPositions);

                    if (!found ||
                        score > bestDistanceScore ||
                        Mathf.Approximately(score, bestDistanceScore) &&
                        ComparePosition(candidate, best) < 0)
                    {
                        found = true;
                        best = candidate;
                        bestDistanceScore = score;
                    }
                }
            }

            position = best;
            return found;
        }

        private static float ResolveMinimumEuclideanDistance(
            Vector2Int candidate,
            IReadOnlyList<Vector2Int> existingPositions)
        {
            if (existingPositions == null ||
                existingPositions.Count == 0)
            {
                return float.MaxValue;
            }

            float best = float.MaxValue;
            for (int i = 0; i < existingPositions.Count; i++)
            {
                float distance =
                    Vector2.Distance(
                        candidate,
                        existingPositions[i]);

                if (distance < best)
                    best = distance;
            }

            return best;
        }

        private static bool ContainsPosition(
            IReadOnlyList<Vector2Int> positions,
            Vector2Int candidate)
        {
            if (positions == null)
                return false;

            for (int i = 0; i < positions.Count; i++)
            {
                if (positions[i] == candidate)
                    return true;
            }

            return false;
        }

        private static int ComparePosition(
            Vector2Int left,
            Vector2Int right)
        {
            int x = left.x.CompareTo(right.x);
            return x != 0
                ? x
                : left.y.CompareTo(right.y);
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
