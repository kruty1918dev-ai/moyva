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
        private readonly IBotObjectivePlanner _objective;
        private readonly IBotConstructionPlanner _construction;
        private readonly IBotMovementPlanner _movement;

        [Inject]
        public BotTurnPlanner(
            [InjectOptional] IBotDeploymentPlanner deployment = null,
            [InjectOptional] IBotCombatPlanner combat = null,
            [InjectOptional] IBotObjectivePlanner objective = null,
            [InjectOptional] IBotConstructionPlanner construction = null,
            [InjectOptional] IBotMovementPlanner movement = null)
        {
            _deployment = deployment;
            _combat = combat;
            _objective = objective;
            _construction = construction;
            _movement = movement;
        }

        public IReadOnlyList<BotActionCandidate> GenerateCandidates(
            BotWorldSnapshot snapshot,
            BotStrategicContext strategy)
        {
            if (snapshot == null)
                return Array.Empty<BotActionCandidate>();

            var candidates = new List<BotActionCandidate>();
            Add(candidates, _deployment?.Generate(snapshot, strategy));
            Add(candidates, _combat?.Generate(snapshot, strategy));
            Add(candidates, _objective?.Generate(snapshot, strategy));
            Add(candidates, _construction?.Generate(snapshot, strategy));
            Add(candidates, _movement?.Generate(snapshot, strategy));

            candidates.RemoveAll(candidate => candidate.Kind == BotActionKind.None);
            candidates.Sort(CompareCandidate);
            return candidates;
        }

        private static void Add(
            List<BotActionCandidate> target,
            IReadOnlyList<BotActionCandidate> source)
        {
            if (source == null)
                return;

            for (int index = 0; index < source.Count; index++)
                target.Add(source[index]);
        }

        private static int CompareCandidate(BotActionCandidate left, BotActionCandidate right)
        {
            int score = right.Score.Total.CompareTo(left.Score.Total);
            if (score != 0)
                return score;

            int kind = ((int)left.Kind).CompareTo((int)right.Kind);
            return kind != 0 ? kind : string.CompareOrdinal(left.CandidateId, right.CandidateId);
        }
    }
}
