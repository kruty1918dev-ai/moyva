using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotMovementPlanner : IBotMovementPlanner
    {
        internal const int MaxUnitsPlanned = 4;
        internal const int MaxTilesPerUnit = 64;

        private readonly IUnitMovementQuery _movementQuery;

        [Inject]
        public BotMovementPlanner([InjectOptional] IUnitMovementQuery movementQuery = null)
        {
            _movementQuery = movementQuery;
        }

        public IReadOnlyList<BotActionCandidate> Generate(
            BotWorldSnapshot snapshot,
            BotStrategicContext strategy)
        {
            if (snapshot == null || _movementQuery == null || snapshot.OwnUnits.Count == 0)
                return Array.Empty<BotActionCandidate>();

            var candidates = new List<BotActionCandidate>();
            int plannedUnits = 0;
            for (int index = 0; index < snapshot.OwnUnits.Count && plannedUnits < MaxUnitsPlanned; index++)
            {
                BotUnitSnapshot unit = snapshot.OwnUnits[index];
                if (string.IsNullOrWhiteSpace(unit.UnitId))
                    continue;

                if (!TryChooseTarget(snapshot, strategy, unit, out Vector2Int target, out int score))
                    continue;

                BotActionKind kind = strategy.Posture == BotStrategicPosture.Search
                    ? BotActionKind.ScoutMove
                    : BotActionKind.Move;
                candidates.Add(new BotActionCandidate(
                    $"{kind}:{unit.UnitId}:{target.x},{target.y}",
                    kind,
                    strategy.Posture,
                    new BotActionScore(score, "Movement target selected from canonical reachable tiles."),
                    actorId: unit.UnitId,
                    targetCell: target,
                    reason: "Move through IUnitMovementService after revalidating reachability."));
                plannedUnits++;
            }

            candidates.Sort(CompareCandidate);
            return candidates;
        }

        private bool TryChooseTarget(
            BotWorldSnapshot snapshot,
            BotStrategicContext strategy,
            BotUnitSnapshot unit,
            out Vector2Int target,
            out int score)
        {
            target = default;
            score = int.MinValue;
            Vector2Int anchor = ResolveMovementAnchor(snapshot, strategy, unit.Position);
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

        private static Vector2Int ResolveMovementAnchor(
            BotWorldSnapshot snapshot,
            BotStrategicContext strategy,
            Vector2Int current)
        {
            if (snapshot.VisibleEnemyUnits.Count > 0)
                return snapshot.VisibleEnemyUnits[0].Position;

            if (snapshot.Memory.Count > 0)
                return snapshot.Memory[0].LastKnownPosition;

            return strategy.Posture == BotStrategicPosture.Search
                ? current + new Vector2Int(3, 2)
                : snapshot.StartPosition;
        }

        private static int ScoreTile(Vector2Int anchor, UnitMovementTileSnapshot tile)
        {
            int distance = Mathf.Abs(anchor.x - tile.Position.x) + Mathf.Abs(anchor.y - tile.Position.y);
            return 500 - (distance * 10) - Mathf.RoundToInt(tile.Cost * 4f);
        }

        private static int CompareCandidate(BotActionCandidate left, BotActionCandidate right)
        {
            int score = right.Score.Total.CompareTo(left.Score.Total);
            return score != 0 ? score : string.CompareOrdinal(left.CandidateId, right.CandidateId);
        }
    }
}
