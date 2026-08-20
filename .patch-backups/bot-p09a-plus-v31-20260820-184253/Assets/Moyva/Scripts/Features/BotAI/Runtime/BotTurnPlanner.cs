using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotTurnPlanner : IBotTurnPlanner
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
            [InjectOptional] IBotDecisionTrace trace = null)
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
        }

        public IReadOnlyList<BotActionCandidate> GenerateCandidates(BotWorldSnapshot snapshot, BotStrategicContext strategy)
        {
            if (snapshot == null) return Array.Empty<BotActionCandidate>();
            _goals?.Observe(snapshot, strategy);

            HashSet<string> protectedHomeGuards = BuildProtectedHomeGuardSet(snapshot, strategy);
            var candidates = new List<BotActionCandidate>();

            Add(candidates, _combat?.Generate(snapshot, strategy), strategy, protectedHomeGuards, defenseSource:false, tacticalSource:false);
            Add(candidates, _tactical?.Generate(snapshot, strategy), strategy, protectedHomeGuards, defenseSource:false, tacticalSource:true);
            Add(candidates, _deployment?.Generate(snapshot, strategy), strategy, protectedHomeGuards, defenseSource:false, tacticalSource:false);
            Add(candidates, _defense?.Generate(snapshot, strategy), strategy, protectedHomeGuards, defenseSource:true, tacticalSource:false);
            Add(candidates, _recruitment?.Generate(snapshot, strategy), strategy, protectedHomeGuards, defenseSource:false, tacticalSource:false);
            Add(candidates, _objective?.Generate(snapshot, strategy), strategy, protectedHomeGuards, defenseSource:false, tacticalSource:false);
            Add(candidates, _scouting?.Generate(snapshot, strategy), strategy, protectedHomeGuards, defenseSource:false, tacticalSource:false);
            Add(candidates, _construction?.Generate(snapshot, strategy), strategy, protectedHomeGuards, defenseSource:false, tacticalSource:false);
            Add(candidates, _movement?.Generate(snapshot, strategy), strategy, protectedHomeGuards, defenseSource:false, tacticalSource:false);

            candidates.RemoveAll(c => c.Kind == BotActionKind.None);
            candidates.Sort(CompareCandidate);
            _trace?.Record(snapshot.OwnerId, snapshot.GlobalTurn, strategy, candidates);
            return candidates;
        }

        private HashSet<string> BuildProtectedHomeGuardSet(BotWorldSnapshot snapshot, BotStrategicContext strategy)
        {
            var result = new HashSet<string>(StringComparer.Ordinal);
            if (_defense == null || strategy.Posture == BotStrategicPosture.EmergencyDefense) return result;
            IReadOnlyList<string> ids = _defense.GetProtectedHomeGuardUnitIds(snapshot, strategy);
            if (ids == null) return result;
            for (int i=0;i<ids.Count;i++) if (!string.IsNullOrWhiteSpace(ids[i])) result.Add(ids[i].Trim());
            return result;
        }

        private static void Add(
            List<BotActionCandidate> target,
            IReadOnlyList<BotActionCandidate> source,
            BotStrategicContext strategy,
            HashSet<string> protectedHomeGuards,
            bool defenseSource,
            bool tacticalSource)
        {
            if (source == null) return;
            for (int i=0;i<source.Count;i++)
            {
                BotActionCandidate candidate = source[i];
                if (candidate.Kind == BotActionKind.None) continue;
                bool ordinaryMovement = candidate.Kind == BotActionKind.Move || candidate.Kind == BotActionKind.ScoutMove;
                bool retreat = tacticalSource && string.Equals(candidate.Reason, "tactical-retreat", StringComparison.Ordinal);
                if (!defenseSource && !retreat && ordinaryMovement && !string.IsNullOrWhiteSpace(candidate.ActorId) && protectedHomeGuards.Contains(candidate.ActorId))
                    continue;
                target.Add(ApplyStrategicPolicy(candidate, strategy, defenseSource, tacticalSource));
            }
        }

        private static BotActionCandidate ApplyStrategicPolicy(
            BotActionCandidate candidate,
            BotStrategicContext strategy,
            bool defenseSource,
            bool tacticalSource)
        {
            int adjustment = 0;
            if (strategy.Posture == BotStrategicPosture.EmergencyDefense)
            {
                adjustment = candidate.Kind switch
                {
                    BotActionKind.Attack => 1200,
                    BotActionKind.DeployReadyUnit => 750,
                    BotActionKind.Move when defenseSource => 950,
                    BotActionKind.Move when tacticalSource && candidate.Reason == "tactical-retreat" => 550,
                    BotActionKind.Move when tacticalSource => 250,
                    BotActionKind.ScoutMove => -1400,
                    BotActionKind.Move => -900,
                    BotActionKind.Build => -700,
                    BotActionKind.Recruit => 150,
                    _ => 0,
                };
            }
            else if (strategy.Posture == BotStrategicPosture.Search && candidate.Kind == BotActionKind.ScoutMove)
                adjustment = 500;
            else if (strategy.Posture == BotStrategicPosture.ArmyBuildUp && candidate.Kind == BotActionKind.Recruit)
                adjustment = 450;

            if (adjustment == 0) return candidate;
            long adjusted=(long)candidate.Score.Total+adjustment;
            int score=adjusted>int.MaxValue?int.MaxValue:adjusted<int.MinValue?int.MinValue:(int)adjusted;
            return new BotActionCandidate(candidate.CandidateId,candidate.Kind,candidate.Posture,
                new BotActionScore(score,(candidate.Score.Explanation??string.Empty)+" Strategic policy adjustment."),
                candidate.ActorId,candidate.TargetId,candidate.TargetCell,candidate.DefinitionId,candidate.Reason);
        }

        private static int CompareCandidate(BotActionCandidate left, BotActionCandidate right)
        {
            int score=right.Score.Total.CompareTo(left.Score.Total);
            if(score!=0)return score;
            int kind=((int)left.Kind).CompareTo((int)right.Kind);
            return kind!=0?kind:string.CompareOrdinal(left.CandidateId,right.CandidateId);
        }
    }
}
