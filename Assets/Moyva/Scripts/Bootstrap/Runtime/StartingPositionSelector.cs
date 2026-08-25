using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Pathfinding.API;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal interface IStartingPositionSelector
    {
        List<Vector2Int> PickStartingPositions(
            WorldGeneratedDataSignal signal,
            int positionsCount);

        Vector2Int PickStartingPosition(Vector2Int baseMapSize);
    }

    internal sealed class StartingPositionSelector :
        IStartingPositionSelector
    {
        private const string DirectDiagTag = "[MoyvaDirectStartDiag]";

        private readonly StartingPositionInitializerSettings _settings;
        private readonly IPathfinder _pathfinder;
        private readonly StartingPositionTerrainQualityEvaluator _terrain;

        public StartingPositionSelector(
            StartingPositionInitializerSettings settings,
            IPathfinder pathfinder)
        {
            _settings =
                settings ??
                new StartingPositionInitializerSettings();

            _pathfinder = pathfinder;
            _terrain =
                new StartingPositionTerrainQualityEvaluator(_settings);
        }

        public List<Vector2Int> PickStartingPositions(
            WorldGeneratedDataSignal signal,
            int positionsCount)
        {
            positionsCount = Mathf.Max(0, positionsCount);
            var positions = new List<Vector2Int>(positionsCount);
            int attempts = Mathf.Max(
                1,
                _settings.startCandidateAttempts);

            Debug.Log(
                $"{DirectDiagTag} Selector.ENTER requestedCount={positionsCount}, " +
                $"map={signal.Width}x{signal.Height}, " +
                $"minDistance={Mathf.Max(1, _settings.minAStarDistanceBetweenPlayers)}, " +
                $"terrainRadius={Mathf.Max(1, _settings.startTerrainSampleRadius)}, " +
                $"minLandRatio={Mathf.Clamp01(_settings.minimumLandRatioAroundStart):0.00}, " +
                $"preferWater={_settings.preferWaterNearStart}, attempts={attempts}.");

            bool isDirectGameplay =
                signal.Source == WorldGeneratedDataSource.DirectGameplayTest ||
                GameLaunchContext.Mode == GameLaunchMode.DirectGameplayTest ||
                GameLaunchContext.Source == GameLaunchSource.DirectGameplayTest;

            for (int positionIndex = 0;
                 positionIndex < positionsCount;
                 positionIndex++)
            {
                if (TryPickStartingPosition(
                        signal,
                        positions,
                        attempts,
                        out Vector2Int position))
                {
                    positions.Add(position);
                    continue;
                }

                // Keep the topology alive, but relax constraints in an explicit
                // order. Every participant slot goes through the same selector.
                if (isDirectGameplay &&
                    TryPickBestEffortDirectPosition(
                        signal,
                        positions,
                        requireTerrainQuality: true,
                        requireValidHeight: true,
                        out position))
                {
                    positions.Add(position);
                    Debug.LogWarning(
                        $"{DirectDiagTag} Selector.DIRECT_FALLBACK_RELAX_DISTANCE " +
                        $"slot={positionIndex}, position={position}. " +
                        "Terrain quality and valid height were preserved; only " +
                        "the configured inter-player path distance was relaxed.");
                    continue;
                }

                if (isDirectGameplay &&
                    TryPickBestEffortDirectPosition(
                        signal,
                        positions,
                        requireTerrainQuality: false,
                        requireValidHeight: true,
                        out position))
                {
                    positions.Add(position);
                    Debug.LogWarning(
                        $"{DirectDiagTag} Selector.DIRECT_FALLBACK_RELAX_TERRAIN_QUALITY " +
                        $"slot={positionIndex}, position={position}. " +
                        "Valid center height is preserved, but the regional land-ratio " +
                        "threshold could not be satisfied.");
                    continue;
                }

                if (isDirectGameplay &&
                    TryPickBestEffortDirectPosition(
                        signal,
                        positions,
                        requireTerrainQuality: false,
                        requireValidHeight: false,
                        out position))
                {
                    positions.Add(position);
                    Debug.LogWarning(
                        $"{DirectDiagTag} Selector.DIRECT_FALLBACK_LAST_RESORT " +
                        $"slot={positionIndex}, position={position}. " +
                        "No unique tile satisfied normal terrain constraints. " +
                        "This is a degraded startup and should be investigated.");
                    continue;
                }

                Debug.LogError(
                    $"[Bootstrap] Failed to find start position " +
                    $"#{positionIndex + 1}/{positionsCount}.");
            }

            if (positions.Count > 1)
            {
                Debug.Log(
                    $"[Bootstrap] Reserved terrain-aware start positions: " +
                    $"{string.Join(", ", positions)}");
            }

            Debug.Log(
                $"{DirectDiagTag} Selector.RESULT selected={positions.Count}, " +
                $"selectedShort={FormatPositions(positions)}.");

            return positions;
        }

        public bool TryPickStartingPosition(
            WorldGeneratedDataSignal signal,
            IReadOnlyList<Vector2Int> existingPositions,
            int attempts,
            out Vector2Int position)
        {
            Vector2Int baseMapSize =
                StartingPositionMapUtility.ResolveBaseMapSize(signal);

            int rejectedBounds = 0;
            int rejectedHeight = 0;
            int rejectedDistance = 0;
            int rejectedTerrain = 0;

            bool found = false;
            Vector2Int best = default;
            int bestScore = int.MinValue;
            StartingPositionTerrainQuality bestQuality = default;

            attempts = Mathf.Max(1, attempts);

            // Randomness lives only in candidate sampling. The winner of the
            // sampled set is selected by deterministic utility.
            for (int attempt = 0; attempt < attempts; attempt++)
            {
                Vector2Int candidate =
                    PickStartingPosition(baseMapSize);

                if (!IsInsideStartBounds(
                        candidate,
                        baseMapSize.x,
                        baseMapSize.y))
                {
                    rejectedBounds++;
                    continue;
                }

                if (!IsValidStartHeight(signal, candidate))
                {
                    rejectedHeight++;
                    continue;
                }

                if (!HasRequiredDistance(
                        candidate,
                        existingPositions))
                {
                    rejectedDistance++;
                    continue;
                }

                StartingPositionTerrainQuality quality =
                    _terrain.Evaluate(signal, candidate);

                if (!quality.HardValid)
                {
                    rejectedTerrain++;
                    continue;
                }

                int score =
                    quality.Utility +
                    ScoreInterPlayerSeparation(
                        candidate,
                        existingPositions);

                if (!found ||
                    score > bestScore ||
                    score == bestScore &&
                    ComparePosition(candidate, best) < 0)
                {
                    found = true;
                    best = candidate;
                    bestScore = score;
                    bestQuality = quality;
                }
            }

            if (!found)
            {
                // Exhaustive deterministic fallback keeps the same hard criteria
                // instead of accepting the first coordinate in scan order.
                for (int x = 0; x < baseMapSize.x; x++)
                {
                    for (int y = 0; y < baseMapSize.y; y++)
                    {
                        var candidate = new Vector2Int(x, y);

                        if (!IsInsideStartBounds(
                                candidate,
                                baseMapSize.x,
                                baseMapSize.y) ||
                            !IsValidStartHeight(signal, candidate) ||
                            !HasRequiredDistance(
                                candidate,
                                existingPositions))
                        {
                            continue;
                        }

                        StartingPositionTerrainQuality quality =
                            _terrain.Evaluate(signal, candidate);

                        if (!quality.HardValid)
                            continue;

                        int score =
                            quality.Utility +
                            ScoreInterPlayerSeparation(
                                candidate,
                                existingPositions);

                        if (!found ||
                            score > bestScore ||
                            score == bestScore &&
                            ComparePosition(candidate, best) < 0)
                        {
                            found = true;
                            best = candidate;
                            bestScore = score;
                            bestQuality = quality;
                        }
                    }
                }
            }

            Debug.Log(
                $"{DirectDiagTag} Selector.Candidates " +
                $"accepted={(found ? 1 : 0)}, rejectedOutOfBounds={rejectedBounds}, " +
                $"rejectedHeight={rejectedHeight}, rejectedDistance={rejectedDistance}, " +
                $"rejectedTerrain={rejectedTerrain}, " +
                $"best={(found ? best.ToString() : "none")}, " +
                $"bestScore={(found ? bestScore : int.MinValue)}, " +
                $"terrain={(found ? bestQuality.Reason : "none")}.");

            position = best;
            return found;
        }

        public Vector2Int PickStartingPosition(
            int width,
            int height)
            => PickStartingPosition(
                new Vector2Int(width, height));

        public Vector2Int PickStartingPosition(
            Vector2Int baseMapSize)
        {
            int width = Mathf.Max(0, baseMapSize.x);
            int height = Mathf.Max(0, baseMapSize.y);

            if (width <= 0 || height <= 0)
                return Vector2Int.zero;

            Vector2Int position =
                StartingPositionMapUtility.PickRuntimeRandomPoint(
                    width,
                    height,
                    _settings.minMarginFromBorder,
                    _settings.relativeMarginFactor,
                    out int seed);

            Debug.Log(
                $"{DirectDiagTag} Selector.PickRandom " +
                $"position={position}, seed={seed}, map={width}x{height}.");

            return position;
        }

        public bool IsInsideStartBounds(
            Vector2Int position,
            int width,
            int height)
        {
            if (width <= 0 || height <= 0)
                return false;

            int minSide = Mathf.Min(width, height);
            int relativeMargin =
                Mathf.FloorToInt(
                    minSide *
                    Mathf.Clamp01(_settings.relativeMarginFactor));

            int margin = Mathf.Max(
                _settings.minMarginFromBorder,
                relativeMargin);

            int xMin = Mathf.Clamp(margin, 0, width - 1);
            int xMax = Mathf.Clamp(
                width - margin - 1,
                xMin,
                width - 1);

            int yMin = Mathf.Clamp(margin, 0, height - 1);
            int yMax = Mathf.Clamp(
                height - margin - 1,
                yMin,
                height - 1);

            return
                position.x >= xMin &&
                position.x <= xMax &&
                position.y >= yMin &&
                position.y <= yMax;
        }

        public bool IsValidStartHeight(
            WorldGeneratedDataSignal signal,
            Vector2Int position)
        {
            if (signal.HeightMap == null)
                return !_settings.requireHeightMapForStart;

            if (position.x < 0 ||
                position.x >= signal.HeightMap.GetLength(0) ||
                position.y < 0 ||
                position.y >= signal.HeightMap.GetLength(1))
            {
                return false;
            }

            float minHeight = Mathf.Min(
                _settings.startMinHeight,
                _settings.startMaxHeight);

            float maxHeight = Mathf.Max(
                _settings.startMinHeight,
                _settings.startMaxHeight);

            float height =
                signal.HeightMap[position.x, position.y];

            return
                height >= minHeight &&
                height <= maxHeight;
        }

        public bool HasRequiredDistance(
            Vector2Int candidate,
            IReadOnlyList<Vector2Int> existingPositions)
        {
            if (existingPositions == null ||
                existingPositions.Count == 0)
            {
                return true;
            }

            int minDistance = Mathf.Max(
                1,
                _settings.minAStarDistanceBetweenPlayers);

            for (int index = 0;
                 index < existingPositions.Count;
                 index++)
            {
                int distance =
                    ResolveStartDistance(
                        candidate,
                        existingPositions[index]);

                if (distance < minDistance)
                    return false;
            }

            return true;
        }

        public int ResolveStartDistance(
            Vector2Int first,
            Vector2Int second)
        {
            if (_pathfinder != null)
            {
                List<Vector2Int> path =
                    _pathfinder.FindPath(first, second);

                if (path != null && path.Count > 0)
                    return Mathf.Max(0, path.Count - 1);
            }

            return Mathf.CeilToInt(
                Vector2.Distance(first, second));
        }

        private bool TryPickBestEffortDirectPosition(
            WorldGeneratedDataSignal signal,
            IReadOnlyList<Vector2Int> existingPositions,
            bool requireTerrainQuality,
            bool requireValidHeight,
            out Vector2Int position)
        {
            Vector2Int baseMapSize =
                StartingPositionMapUtility.ResolveBaseMapSize(signal);

            bool found = false;
            Vector2Int best = default;
            int bestScore = int.MinValue;

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

                    if (ContainsPosition(
                            existingPositions,
                            candidate))
                    {
                        continue;
                    }

                    if (requireValidHeight &&
                        !IsValidStartHeight(signal, candidate))
                    {
                        continue;
                    }

                    StartingPositionTerrainQuality quality =
                        _terrain.Evaluate(signal, candidate);

                    if (requireTerrainQuality &&
                        !quality.HardValid)
                    {
                        continue;
                    }

                    int score =
                        quality.Utility +
                        ScoreInterPlayerSeparation(
                            candidate,
                            existingPositions);

                    if (!found ||
                        score > bestScore ||
                        score == bestScore &&
                        ComparePosition(candidate, best) < 0)
                    {
                        found = true;
                        best = candidate;
                        bestScore = score;
                    }
                }
            }

            position = best;
            return found;
        }

        private int ScoreInterPlayerSeparation(
            Vector2Int candidate,
            IReadOnlyList<Vector2Int> existingPositions)
        {
            if (existingPositions == null ||
                existingPositions.Count == 0)
            {
                return 0;
            }

            int minimum = int.MaxValue;

            for (int i = 0; i < existingPositions.Count; i++)
            {
                int distance =
                    ResolveStartDistance(
                        candidate,
                        existingPositions[i]);

                if (distance < minimum)
                    minimum = distance;
            }

            if (minimum == int.MaxValue)
                return 0;

            // Distance remains a soft preference after the hard minimum passes.
            return Mathf.Min(300, minimum * 6);
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

        private static string FormatPositions(
            IReadOnlyList<Vector2Int> positions)
        {
            if (positions == null ||
                positions.Count == 0)
            {
                return "[]";
            }

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
