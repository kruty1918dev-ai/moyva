using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotDeploymentPlanner : IBotDeploymentPlanner
    {
        private readonly IUnitRecruitmentService _recruitment;

        [Inject]
        public BotDeploymentPlanner([InjectOptional] IUnitRecruitmentService recruitment = null)
        {
            _recruitment = recruitment;
        }

        public IReadOnlyList<BotActionCandidate> Generate(
            BotWorldSnapshot snapshot,
            BotStrategicContext strategy)
        {
            if (snapshot == null || _recruitment == null || snapshot.ReadyRecruitmentItems.Count == 0)
                return Array.Empty<BotActionCandidate>();

            var candidates = new List<BotActionCandidate>();
            for (int index = 0; index < snapshot.ReadyRecruitmentItems.Count; index++)
            {
                UnitRecruitmentQueueItemSnapshot item = snapshot.ReadyRecruitmentItems[index];
                if (!item.IsReady)
                    continue;

                if (!TryChooseDeploymentTile(snapshot, item, out Vector2Int tile, out int score))
                    continue;

                candidates.Add(new BotActionCandidate(
                    $"deploy:{item.QueueId}:{tile.x},{tile.y}",
                    BotActionKind.DeployReadyUnit,
                    strategy.Posture,
                    new BotActionScore(score, "Ready recruitment head should deploy before enqueueing."),
                    actorId: item.RecruitingBuildingId,
                    targetCell: tile,
                    definitionId: item.UnitTypeId,
                    reason: "Deploy ready unit through IUnitRecruitmentService.TryDeployReady."));
            }

            candidates.Sort(CompareCandidate);
            return candidates;
        }

        private bool TryChooseDeploymentTile(
            BotWorldSnapshot snapshot,
            UnitRecruitmentQueueItemSnapshot item,
            out Vector2Int tile,
            out int score)
        {
            tile = default;
            score = int.MinValue;

            IReadOnlyList<UnitRecruitmentDeploymentTileSnapshot> tiles =
                _recruitment.GetDeploymentTiles(
                    snapshot.OwnerId,
                    item.RecruitingBuildingPosition,
                    item.QueueId);
            if (tiles == null || tiles.Count == 0)
                return false;

            for (int index = 0; index < tiles.Count; index++)
            {
                UnitRecruitmentDeploymentTileSnapshot candidate = tiles[index];
                if (!candidate.IsValid)
                    continue;

                int candidateScore = ScoreTile(snapshot, candidate.Position);
                if (candidateScore > score
                    || (candidateScore == score && BotTurnExecutor.ComparePosition(candidate.Position, tile) < 0))
                {
                    tile = candidate.Position;
                    score = candidateScore;
                }
            }

            return score > int.MinValue;
        }

        private static int ScoreTile(BotWorldSnapshot snapshot, Vector2Int tile)
        {
            int homeDistance = Mathf.Abs(snapshot.StartPosition.x - tile.x)
                + Mathf.Abs(snapshot.StartPosition.y - tile.y);
            int nearestThreatPenalty = 0;

            for (int index = 0; index < snapshot.VisibleEnemyUnits.Count; index++)
            {
                BotUnitSnapshot enemy = snapshot.VisibleEnemyUnits[index];
                int distance = Mathf.Abs(enemy.Position.x - tile.x) + Mathf.Abs(enemy.Position.y - tile.y);
                if (distance <= 2)
                    nearestThreatPenalty += 150;
            }

            return 700 - (homeDistance * 8) - nearestThreatPenalty;
        }

        private static int CompareCandidate(BotActionCandidate left, BotActionCandidate right)
        {
            int score = right.Score.Total.CompareTo(left.Score.Total);
            return score != 0 ? score : string.CompareOrdinal(left.CandidateId, right.CandidateId);
        }
    }
}
