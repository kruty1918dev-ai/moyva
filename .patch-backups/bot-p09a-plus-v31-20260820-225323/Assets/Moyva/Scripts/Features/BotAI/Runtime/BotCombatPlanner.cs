using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Units.API;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotCombatPlanner : IBotCombatPlanner
    {
        internal const int MaxAttackCandidatesPerUnit = 64;

        private readonly IUnitCombatService _combat;
        private readonly IBuildingRegistry _buildings;
        private readonly IUnitClassConfig _unitConfigs;
        private readonly BotPlanningProfile _profile;

        [Inject]
        public BotCombatPlanner(
            [InjectOptional] IUnitCombatService combat = null,
            [InjectOptional] IBuildingRegistry buildings = null,
            [InjectOptional] IUnitClassConfig unitConfigs = null,
            [InjectOptional] BotPlanningProfile profile = null)
        {
            _combat = combat;
            _buildings = buildings;
            _unitConfigs = unitConfigs;
            _profile = profile ?? BotPlanningProfile.Normal();
        }

        public IReadOnlyList<BotActionCandidate> Generate(
            BotWorldSnapshot snapshot,
            BotStrategicContext strategy)
        {
            if (snapshot == null
                || _combat == null
                || snapshot.OwnUnits.Count == 0
                || snapshot.VisibleEnemyUnits.Count == 0)
            {
                return Array.Empty<BotActionCandidate>();
            }

            BotDefenseContext defense =
                BotTacticalAnalysis.BuildDefenseContext(
                    snapshot,
                    _buildings,
                    _unitConfigs,
                    _profile);
            Dictionary<string, BotThreatSnapshot> threatById =
                BuildThreatLookup(defense);

            var scored = new List<ScoredAttack>();
            List<BotUnitSnapshot> attackers =
                StableUnits(snapshot.OwnUnits);
            List<BotUnitSnapshot> targets =
                StableUnits(snapshot.VisibleEnemyUnits);

            for (int unitIndex = 0;
                 unitIndex < attackers.Count;
                 unitIndex++)
            {
                BotUnitSnapshot attacker = attackers[unitIndex];
                if (string.IsNullOrWhiteSpace(attacker.UnitId)
                    || !string.Equals(
                        attacker.OwnerId,
                        snapshot.OwnerId,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                int evaluated = 0;
                for (int targetIndex = 0;
                     targetIndex < targets.Count
                     && evaluated < MaxAttackCandidatesPerUnit;
                     targetIndex++)
                {
                    BotUnitSnapshot target = targets[targetIndex];
                    if (string.IsNullOrWhiteSpace(target.UnitId)
                        || string.Equals(
                            target.OwnerId,
                            snapshot.OwnerId,
                            StringComparison.Ordinal))
                    {
                        continue;
                    }

                    // Discovery comes only from snapshot.VisibleEnemyUnits.
                    // CanAttack only revalidates legality for that visible target.
                    evaluated++;
                    if (!_combat.CanAttack(
                            attacker.UnitId,
                            target.UnitId,
                            out UnitAttackRejectReason rejectReason))
                    {
                        continue;
                    }

                    if (!_combat.TryPreviewAttack(
                            attacker.UnitId,
                            target.UnitId,
                            out UnitCombatBreakdown preview))
                    {
                        continue;
                    }

                    _combat.TryGetHealth(
                        target.UnitId,
                        out UnitHealthSnapshot targetHealth);

                    bool targetHasHealth =
                        targetHealth.MaxHp > 0
                        && !string.IsNullOrEmpty(targetHealth.UnitId);
                    int hp = targetHasHealth
                        ? targetHealth.CurrentHp
                        : Math.Max(1, preview.DefenderHitPoints);
                    bool lethal = preview.TotalDamage >= hp;

                    bool hasThreat = threatById.TryGetValue(
                        target.UnitId,
                        out BotThreatSnapshot threat);

                    int score = ScoreAttack(
                        strategy,
                        attacker,
                        target,
                        preview,
                        hp,
                        lethal,
                        threat,
                        hasThreat);

                    if (_combat.TryPreviewDuel(
                            attacker.UnitId,
                            target.UnitId,
                            out UnitCombatDuel duel))
                    {
                        score += duel.Outcome switch
                        {
                            UnitCombatOutcome.AttackerAdvantage => 120,
                            UnitCombatOutcome.DefenderAdvantage => -120,
                            _ => 0,
                        };
                    }

                    string explanation = lethal
                        ? "Legal lethal attack."
                        : "Legal immediate attack.";
                    var candidate = new BotActionCandidate(
                        $"attack:{attacker.UnitId}:{target.UnitId}",
                        BotActionKind.Attack,
                        strategy.Posture,
                        new BotActionScore(score, explanation),
                        actorId: attacker.UnitId,
                        targetId: target.UnitId,
                        targetCell: target.Position,
                        reason:
                            $"Canonical combat revalidation succeeded ({rejectReason}).");

                    int threatScore = hasThreat
                        ? threat.ThreatScore
                        : 0;
                    scored.Add(new ScoredAttack(
                        candidate,
                        threatScore,
                        hp));
                }
            }

            scored.Sort(CompareScoredAttack);
            if (scored.Count == 0)
                return Array.Empty<BotActionCandidate>();

            var result = new BotActionCandidate[scored.Count];
            for (int index = 0; index < scored.Count; index++)
                result[index] = scored[index].Candidate;
            return result;
        }

        private int ScoreAttack(
            BotStrategicContext strategy,
            BotUnitSnapshot attacker,
            BotUnitSnapshot target,
            UnitCombatBreakdown preview,
            int targetHp,
            bool lethal,
            BotThreatSnapshot threat,
            bool hasThreat)
        {
            long score = 1600;
            score += (long)Math.Max(0, preview.TotalDamage) * 12L;

            if (lethal)
                score += 1300 + _profile.FocusFireLethalWeight;

            if (targetHp > 0 && targetHp <= Math.Max(1, preview.TotalDamage * 2))
                score += 240;

            int overkill =
                Math.Max(
                    0,
                    preview.TotalDamage - Math.Max(0, targetHp));
            score -= (long)overkill * 2L;

            score += strategy.Posture switch
            {
                BotStrategicPosture.EmergencyDefense => 650,
                BotStrategicPosture.Pressure => 260,
                BotStrategicPosture.Siege => 240,
                BotStrategicPosture.ArmyBuildUp => 120,
                _ => 80,
            };

            if (hasThreat)
            {
                score += Math.Min(
                    3000,
                    threat.ThreatScore / 20);

                if (threat.CanAttackCastleAreaNow)
                    score += 1200;
                else if (threat.CanThreatenCastleNextTurn)
                    score += 550;
            }

            int distance =
                BotTacticalAnalysis.Chebyshev(
                    attacker.Position,
                    target.Position);
            score += Math.Max(0, 64 - distance);

            if (score > int.MaxValue)
                return int.MaxValue;
            if (score < int.MinValue)
                return int.MinValue;
            return (int)score;
        }

        private static Dictionary<string, BotThreatSnapshot>
            BuildThreatLookup(BotDefenseContext context)
        {
            var result =
                new Dictionary<string, BotThreatSnapshot>(
                    StringComparer.Ordinal);
            if (context?.Threats == null)
                return result;

            for (int index = 0;
                 index < context.Threats.Count;
                 index++)
            {
                BotThreatSnapshot threat =
                    context.Threats[index];
                if (!string.IsNullOrWhiteSpace(
                        threat.EnemyUnitId))
                {
                    result[threat.EnemyUnitId] = threat;
                }
            }

            return result;
        }

        private static List<BotUnitSnapshot> StableUnits(
            IReadOnlyList<BotUnitSnapshot> source)
        {
            var result =
                new List<BotUnitSnapshot>(
                    source?.Count ?? 0);

            if (source != null)
            {
                for (int index = 0;
                     index < source.Count;
                     index++)
                {
                    result.Add(source[index]);
                }
            }

            result.Sort(
                (left, right) =>
                    string.CompareOrdinal(
                        left.UnitId,
                        right.UnitId));
            return result;
        }

        private static int CompareScoredAttack(
            ScoredAttack left,
            ScoredAttack right)
        {
            int score =
                right.Candidate.Score.Total
                .CompareTo(left.Candidate.Score.Total);
            if (score != 0)
                return score;

            int threat =
                right.TargetThreatScore
                .CompareTo(left.TargetThreatScore);
            if (threat != 0)
                return threat;

            int hp =
                left.TargetHp.CompareTo(right.TargetHp);
            if (hp != 0)
                return hp;

            int attacker =
                string.CompareOrdinal(
                    left.Candidate.ActorId,
                    right.Candidate.ActorId);
            return attacker != 0
                ? attacker
                : string.CompareOrdinal(
                    left.Candidate.TargetId,
                    right.Candidate.TargetId);
        }

        private readonly struct ScoredAttack
        {
            public ScoredAttack(
                BotActionCandidate candidate,
                int targetThreatScore,
                int targetHp)
            {
                Candidate = candidate;
                TargetThreatScore = targetThreatScore;
                TargetHp = targetHp;
            }

            public BotActionCandidate Candidate { get; }
            public int TargetThreatScore { get; }
            public int TargetHp { get; }
        }
    }
}
