using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotTurnPlanner :
        IBotTurnPlanner
    {
        private readonly IBotDeploymentPlanner _deployment;
        private readonly IBotCombatPlanner _combat;
        private readonly IBotTacticalSequencer _tactical;
        private readonly IBotDefensePlanner _defense;
        private readonly IBotRecruitmentPlanner _recruitment;
        private readonly IBotObjectivePlanner _objective;
        private readonly IBotScoutingPlanner _scouting;
        private readonly IBotConstructionPlanner _construction;
        private readonly IBotMovementPlanner _movement;
        private readonly IBotGoalStore _goals;
        private readonly IBotDecisionTrace _trace;
        private readonly IBotStallTracker _stall;

        [Inject]
        public BotTurnPlanner(
            [InjectOptional] IBotDeploymentPlanner deployment = null,
            [InjectOptional] IBotCombatPlanner combat = null,
            [InjectOptional] IBotObjectivePlanner objective = null,
            [InjectOptional] IBotConstructionPlanner construction = null,
            [InjectOptional] IBotMovementPlanner movement = null,
            [InjectOptional] IBotDefensePlanner defense = null,
            [InjectOptional] IBotTacticalSequencer tactical = null,
            [InjectOptional] IBotRecruitmentPlanner recruitment = null,
            [InjectOptional] IBotScoutingPlanner scouting = null,
            [InjectOptional] IBotGoalStore goals = null,
            [InjectOptional] IBotDecisionTrace trace = null,
            [InjectOptional] IBotStallTracker stall = null)
        {
            _deployment = deployment;
            _combat = combat;
            _objective = objective;
            _construction = construction;
            _movement = movement;
            _defense = defense;
            _tactical = tactical;
            _recruitment = recruitment;
            _scouting = scouting;
            _goals = goals;
            _trace = trace;
            _stall = stall;
        }

        public IReadOnlyList<BotActionCandidate>
            GenerateCandidates(
                BotWorldSnapshot snapshot,
                BotStrategicContext strategy)
        {
            if (snapshot == null)
                return Array.Empty<BotActionCandidate>();

            _goals?.Observe(
                snapshot,
                strategy);

            HashSet<string> protectedHomeGuards =
                BuildProtectedHomeGuardSet(
                    snapshot,
                    strategy);

            BotStallStatus stall =
                _stall?.GetStatus(snapshot.OwnerId) ??
                default;

            var candidates =
                new List<BotActionCandidate>();

            Add(
                candidates,
                _combat?.Generate(snapshot, strategy),
                snapshot,
                strategy,
                stall,
                protectedHomeGuards,
                defenseSource: false,
                tacticalSource: false);

            Add(
                candidates,
                _tactical?.Generate(snapshot, strategy),
                snapshot,
                strategy,
                stall,
                protectedHomeGuards,
                defenseSource: false,
                tacticalSource: true);

            Add(
                candidates,
                _deployment?.Generate(snapshot, strategy),
                snapshot,
                strategy,
                stall,
                protectedHomeGuards,
                defenseSource: false,
                tacticalSource: false);

            Add(
                candidates,
                _defense?.Generate(snapshot, strategy),
                snapshot,
                strategy,
                stall,
                protectedHomeGuards,
                defenseSource: true,
                tacticalSource: false);

            Add(
                candidates,
                _recruitment?.Generate(snapshot, strategy),
                snapshot,
                strategy,
                stall,
                protectedHomeGuards,
                defenseSource: false,
                tacticalSource: false);

            Add(
                candidates,
                _objective?.Generate(snapshot, strategy),
                snapshot,
                strategy,
                stall,
                protectedHomeGuards,
                defenseSource: false,
                tacticalSource: false);

            Add(
                candidates,
                _scouting?.Generate(snapshot, strategy),
                snapshot,
                strategy,
                stall,
                protectedHomeGuards,
                defenseSource: false,
                tacticalSource: false);

            Add(
                candidates,
                _construction?.Generate(snapshot, strategy),
                snapshot,
                strategy,
                stall,
                protectedHomeGuards,
                defenseSource: false,
                tacticalSource: false);

            Add(
                candidates,
                _movement?.Generate(snapshot, strategy),
                snapshot,
                strategy,
                stall,
                protectedHomeGuards,
                defenseSource: false,
                tacticalSource: false);

            candidates.RemoveAll(
                candidate =>
                    candidate.Kind ==
                    BotActionKind.None);

            candidates.Sort(CompareCandidate);

            _trace?.Record(
                snapshot.OwnerId,
                snapshot.GlobalTurn,
                strategy,
                candidates);

            return candidates;
        }

        private HashSet<string>
            BuildProtectedHomeGuardSet(
                BotWorldSnapshot snapshot,
                BotStrategicContext strategy)
        {
            var result =
                new HashSet<string>(
                    StringComparer.Ordinal);

            if (_defense == null ||
                strategy.Posture ==
                BotStrategicPosture.EmergencyDefense)
            {
                return result;
            }

            IReadOnlyList<string> ids =
                _defense.GetProtectedHomeGuardUnitIds(
                    snapshot,
                    strategy);

            if (ids == null)
                return result;

            for (int i = 0; i < ids.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(ids[i]))
                    result.Add(ids[i].Trim());
            }

            return result;
        }

        private void Add(
            List<BotActionCandidate> target,
            IReadOnlyList<BotActionCandidate> source,
            BotWorldSnapshot snapshot,
            BotStrategicContext strategy,
            BotStallStatus stall,
            HashSet<string> protectedHomeGuards,
            bool defenseSource,
            bool tacticalSource)
        {
            if (source == null)
                return;

            for (int i = 0; i < source.Count; i++)
            {
                BotActionCandidate candidate =
                    source[i];

                if (candidate.Kind ==
                    BotActionKind.None)
                {
                    continue;
                }

                bool ordinaryMovement =
                    candidate.Kind ==
                        BotActionKind.Move ||
                    candidate.Kind ==
                        BotActionKind.ScoutMove;

                bool retreat =
                    tacticalSource &&
                    string.Equals(
                        candidate.Reason,
                        "tactical-retreat",
                        StringComparison.Ordinal);

                if (!defenseSource &&
                    !retreat &&
                    ordinaryMovement &&
                    !string.IsNullOrWhiteSpace(
                        candidate.ActorId) &&
                    protectedHomeGuards.Contains(
                        candidate.ActorId))
                {
                    continue;
                }

                target.Add(
                    ApplyUtilityPolicy(
                        candidate,
                        snapshot,
                        strategy,
                        stall,
                        defenseSource,
                        tacticalSource));
            }
        }

        private static BotActionCandidate
            ApplyUtilityPolicy(
                BotActionCandidate candidate,
                BotWorldSnapshot snapshot,
                BotStrategicContext strategy,
                BotStallStatus stall,
                bool defenseSource,
                bool tacticalSource)
        {
            int adjustment = 0;
            var explanations =
                new List<string>();

            switch (strategy.Posture)
            {
                case BotStrategicPosture.EmergencyDefense:
                    adjustment +=
                        candidate.Kind switch
                        {
                            BotActionKind.Attack => 1200,
                            BotActionKind.DeployReadyUnit => 750,
                            BotActionKind.Move
                                when defenseSource => 950,
                            BotActionKind.Move
                                when tacticalSource &&
                                     candidate.Reason ==
                                     "tactical-retreat" => 550,
                            BotActionKind.Move
                                when tacticalSource => 250,
                            BotActionKind.ScoutMove => -1400,
                            BotActionKind.Move => -900,
                            BotActionKind.Build => -700,
                            BotActionKind.Recruit => 150,
                            _ => 0,
                        };

                    explanations.Add(
                        "EmergencyDefense utility");
                    break;

                case BotStrategicPosture.Opening:
                    if (candidate.Kind ==
                        BotActionKind.Build)
                    {
                        adjustment += 650;
                    }
                    else if (candidate.Kind ==
                             BotActionKind.Recruit)
                    {
                        adjustment -= 180;
                    }

                    explanations.Add(
                        "Opening prioritizes capital/infrastructure");
                    break;

                case BotStrategicPosture.Economy:
                    if (candidate.Kind ==
                        BotActionKind.Build)
                    {
                        adjustment += 400;
                    }
                    else if (candidate.Kind ==
                             BotActionKind.ScoutMove)
                    {
                        adjustment -= 80;
                    }

                    explanations.Add(
                        "Economy/resource-stage utility");
                    break;

                case BotStrategicPosture.ArmyBuildUp:
                    if (candidate.Kind ==
                        BotActionKind.Recruit)
                    {
                        adjustment += 500;
                    }
                    else if (candidate.Kind ==
                             BotActionKind.DeployReadyUnit)
                    {
                        adjustment += 650;
                    }
                    else if (candidate.Kind ==
                             BotActionKind.Build)
                    {
                        adjustment += 120;
                    }

                    explanations.Add(
                        "ArmyBuildUp utility");
                    break;

                case BotStrategicPosture.Search:
                    if (candidate.Kind ==
                        BotActionKind.ScoutMove)
                    {
                        adjustment += 550;
                    }

                    explanations.Add(
                        "Search utility");
                    break;

                case BotStrategicPosture.Recovery:
                    adjustment +=
                        candidate.Kind switch
                        {
                            BotActionKind.Build => 350,
                            BotActionKind.Recruit => 300,
                            BotActionKind.DeployReadyUnit => 320,
                            BotActionKind.ScoutMove => 140,
                            _ => 0,
                        };

                    explanations.Add(
                        "Recovery utility");
                    break;
            }

            if (stall.IsStalled)
            {
                if (!string.IsNullOrWhiteSpace(
                        stall.LastCandidateId) &&
                    string.Equals(
                        candidate.CandidateId,
                        stall.LastCandidateId,
                        StringComparison.Ordinal))
                {
                    // Cross-turn quarantine: do not blindly repeat the last
                    // zero-mutation candidate after anti-stall triggers.
                    adjustment -= 1800;
                    explanations.Add(
                        "anti-stall repeated-candidate penalty");
                }
                else
                {
                    adjustment += 180;
                    explanations.Add(
                        "anti-stall alternative-action bonus");
                }
            }

            if (adjustment == 0)
                return candidate;

            long adjusted =
                (long)candidate.Score.Total +
                adjustment;

            int score =
                adjusted > int.MaxValue
                    ? int.MaxValue
                    : adjusted < int.MinValue
                        ? int.MinValue
                        : (int)adjusted;

            string explanation =
                candidate.Score.Explanation ??
                string.Empty;

            if (explanations.Count > 0)
            {
                explanation =
                    explanation +
                    (explanation.Length > 0
                        ? " "
                        : string.Empty) +
                    string.Join(
                        "; ",
                        explanations) +
                    $"; adjustment={adjustment}.";
            }

            return new BotActionCandidate(
                candidate.CandidateId,
                candidate.Kind,
                candidate.Posture,
                new BotActionScore(
                    score,
                    explanation),
                candidate.ActorId,
                candidate.TargetId,
                candidate.TargetCell,
                candidate.DefinitionId,
                candidate.Reason);
        }

        private static int CompareCandidate(
            BotActionCandidate left,
            BotActionCandidate right)
        {
            int score =
                right.Score.Total.CompareTo(
                    left.Score.Total);

            if (score != 0)
                return score;

            int kind =
                ((int)left.Kind).CompareTo(
                    (int)right.Kind);

            return kind != 0
                ? kind
                : string.CompareOrdinal(
                    left.CandidateId,
                    right.CandidateId);
        }
    }
}
