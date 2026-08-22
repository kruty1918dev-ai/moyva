using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotObjectivePlanner : IBotObjectivePlanner
    {
        internal const int MaxUnitsPlanned = 3;
        internal const int MaxTilesPerUnit = 64;

        private readonly IUnitMovementQuery _movementQuery;

        [Inject]
        public BotObjectivePlanner([InjectOptional] IUnitMovementQuery movementQuery = null)
        {
            _movementQuery = movementQuery;
        }

        public IReadOnlyList<BotActionCandidate> Generate(
            BotWorldSnapshot snapshot,
            BotStrategicContext strategy)
        {
            if (snapshot == null || _movementQuery == null || snapshot.OwnUnits.Count == 0)
                return Array.Empty<BotActionCandidate>();

            if (!TryResolveAnchor(snapshot, strategy, out Vector2Int anchor, out bool isObjective, out string anchorReason))
                return Array.Empty<BotActionCandidate>();

            var candidates = new List<BotActionCandidate>();
            int planned = 0;
            for (int index = 0; index < snapshot.OwnUnits.Count && planned < MaxUnitsPlanned; index++)
            {
                BotUnitSnapshot unit = snapshot.OwnUnits[index];
                if (!TryChooseTile(unit, anchor, out Vector2Int target, out int tileScore))
                    continue;

                BotActionKind kind = isObjective ? BotActionKind.Move : BotActionKind.ScoutMove;
                int score = (isObjective ? 760 : 430) + tileScore;
                candidates.Add(new BotActionCandidate(
                    $"objective:{kind}:{unit.UnitId}:{target.x},{target.y}",
                    kind,
                    isObjective ? BotStrategicPosture.Siege : strategy.Posture,
                    new BotActionScore(score, anchorReason),
                    actorId: unit.UnitId,
                    targetCell: target,
                    reason: "Objective/search movement through canonical movement query."));
                planned++;
            }

            candidates.Sort(CompareCandidate);
            return candidates;
        }

        private static bool TryResolveAnchor(
            BotWorldSnapshot snapshot,
            BotStrategicContext strategy,
            out Vector2Int anchor,
            out bool isObjective,
            out string reason)
        {
            if (TryGetVisibleObjective(snapshot, out anchor))
            {
                isObjective = true;
                reason = "Visible enemy objective.";
                return true;
            }

            if (TryGetRememberedObjective(snapshot, out anchor))
            {
                isObjective = true;
                reason = "Remembered enemy objective.";
                return true;
            }

            if (TryGetRecentMemory(snapshot, out anchor))
            {
                isObjective = false;
                reason = "Memory-based search.";
                return true;
            }

            if (strategy.Posture == BotStrategicPosture.Search || snapshot.VisibleEnemyUnits.Count == 0)
            {
                anchor = ResolveSearchAnchor(snapshot);
                isObjective = false;
                reason = "Fog frontier search.";
                return true;
            }

            anchor = default;
            isObjective = false;
            reason = null;
            return false;
        }

        private bool TryChooseTile(
            BotUnitSnapshot unit,
            Vector2Int anchor,
            out Vector2Int target,
            out int score)
        {
            target = default;
            score = int.MinValue;
            IReadOnlyList<UnitMovementTileSnapshot> tiles = _movementQuery.GetMovementTiles(unit.UnitId);
            if (tiles == null || tiles.Count == 0)
                return false;

            int evaluated = 0;
            for (int index = 0; index < tiles.Count && evaluated < MaxTilesPerUnit; index++)
            {
                UnitMovementTileSnapshot tile = tiles[index];
                if (!tile.IsReachable || tile.Position == unit.Position)
                    continue;

                evaluated++;
                int candidateScore = ScoreTile(anchor, tile);
                if (candidateScore > score
                    || (candidateScore == score && BotDeterministicGeometry.ComparePosition(tile.Position, target) < 0))
                {
                    target = tile.Position;
                    score = candidateScore;
                }
            }

            return score > int.MinValue;
        }

        private static bool TryGetVisibleObjective(BotWorldSnapshot snapshot, out Vector2Int position)
        {
            for (int index = 0; index < snapshot.VisibleEnemyBuildings.Count; index++)
            {
                BotBuildingSnapshot building = snapshot.VisibleEnemyBuildings[index];
                if (IsLikelyCastle(building.BuildingId))
                {
                    position = building.Position;
                    return true;
                }
            }

            position = default;
            return false;
        }

        private static bool TryGetRememberedObjective(BotWorldSnapshot snapshot, out Vector2Int position)
        {
            for (int index = 0; index < snapshot.Memory.Count; index++)
            {
                BotKnownEntityMemory memory = snapshot.Memory[index];
                if (memory.Kind == BotKnownEntityKind.Objective || IsLikelyCastle(memory.TypeId))
                {
                    position = memory.LastKnownPosition;
                    return true;
                }
            }

            position = default;
            return false;
        }

        private static bool TryGetRecentMemory(BotWorldSnapshot snapshot, out Vector2Int position)
        {
            for (int index = 0; index < snapshot.Memory.Count; index++)
            {
                BotKnownEntityMemory memory = snapshot.Memory[index];
                if (!memory.WasConfirmedDestroyed)
                {
                    position = memory.LastKnownPosition;
                    return true;
                }
            }

            position = default;
            return false;
        }

        private static Vector2Int ResolveSearchAnchor(BotWorldSnapshot snapshot)
        {
            int direction = (int)(snapshot.GlobalTurn % 4L) switch
            {
                0 => 1,
                1 => -1,
                2 => 2,
                _ => -2,
            };
            return snapshot.StartPosition + new Vector2Int(direction * 6, direction * 3);
        }

        private static int ScoreTile(Vector2Int anchor, UnitMovementTileSnapshot tile)
        {
            int distance = Mathf.Abs(anchor.x - tile.Position.x) + Mathf.Abs(anchor.y - tile.Position.y);
            return 500 - distance * 10 - Mathf.RoundToInt(tile.Cost * 4f);
        }

        private static bool IsLikelyCastle(string id)
            => !string.IsNullOrWhiteSpace(id)
                && id.IndexOf("castle", StringComparison.OrdinalIgnoreCase) >= 0;

        private static int CompareCandidate(BotActionCandidate left, BotActionCandidate right)
        {
            int score = right.Score.Total.CompareTo(left.Score.Total);
            return score != 0 ? score : string.CompareOrdinal(left.CandidateId, right.CandidateId);
        }
    }
}
