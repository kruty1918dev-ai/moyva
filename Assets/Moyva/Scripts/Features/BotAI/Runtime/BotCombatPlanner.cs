using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Combat.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotCombatPlanner : IBotCombatPlanner
    {
        internal const int MaxAttackCandidatesPerUnit = 64;

        private readonly ICombatCommandService _combat;

        [Inject]
        public BotCombatPlanner([InjectOptional] ICombatCommandService combat = null)
        {
            _combat = combat;
        }

        public IReadOnlyList<BotActionCandidate> Generate(
            BotWorldSnapshot snapshot,
            BotStrategicContext strategy)
        {
            if (snapshot == null || _combat == null || snapshot.OwnUnits.Count == 0 || snapshot.VisibleEnemyUnits.Count == 0)
                return Array.Empty<BotActionCandidate>();

            var candidates = new List<BotActionCandidate>();
            for (int unitIndex = 0; unitIndex < snapshot.OwnUnits.Count; unitIndex++)
            {
                BotUnitSnapshot attacker = snapshot.OwnUnits[unitIndex];
                if (string.IsNullOrWhiteSpace(attacker.UnitId))
                    continue;

                int evaluated = 0;
                for (int targetIndex = 0; targetIndex < snapshot.VisibleEnemyUnits.Count
                    && evaluated < MaxAttackCandidatesPerUnit; targetIndex++)
                {
                    BotUnitSnapshot target = snapshot.VisibleEnemyUnits[targetIndex];
                    if (string.IsNullOrWhiteSpace(target.UnitId))
                        continue;

                    evaluated++;
                    if (!_combat.TryPreview(attacker.UnitId, target.UnitId, out CombatCommandPreview preview, out _))
                        continue;

                    int score = ScoreAttack(snapshot, strategy, attacker, target, preview);
                    candidates.Add(new BotActionCandidate(
                        $"attack:{attacker.UnitId}:{target.UnitId}",
                        BotActionKind.Attack,
                        strategy.Posture,
                        new BotActionScore(score, preview.TargetWouldDie ? "Legal lethal attack." : "Legal immediate attack."),
                        actorId: attacker.UnitId,
                        targetId: target.UnitId,
                        targetCell: target.Position,
                        reason: "Attack through shared combat command after revalidation."));
                }
            }

            candidates.Sort(CompareCandidate);
            return candidates;
        }

        private static int ScoreAttack(
            BotWorldSnapshot snapshot,
            BotStrategicContext strategy,
            BotUnitSnapshot attacker,
            BotUnitSnapshot target,
            CombatCommandPreview preview)
        {
            int ownBaseDistance = GridDistance(snapshot.StartPosition, target.Position);
            int pressure = Mathf.Max(0, 240 - ownBaseDistance * 20);
            int postureBonus = strategy.Posture switch
            {
                BotStrategicPosture.EmergencyDefense => 500,
                BotStrategicPosture.Pressure => 220,
                BotStrategicPosture.ArmyBuildUp => 120,
                _ => 80,
            };
            int lethalBonus = preview.TargetWouldDie ? 700 : 0;
            int damageValue = Mathf.Max(0, preview.ExpectedDamage) * 12;
            int tieBreaker = 50 - Mathf.Min(50, GridDistance(attacker.Position, target.Position));
            return 1000 + postureBonus + lethalBonus + pressure + damageValue + tieBreaker;
        }

        private static int GridDistance(Vector2Int a, Vector2Int b)
            => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

        private static int CompareCandidate(BotActionCandidate left, BotActionCandidate right)
        {
            int score = right.Score.Total.CompareTo(left.Score.Total);
            return score != 0 ? score : string.CompareOrdinal(left.CandidateId, right.CandidateId);
        }
    }
}
