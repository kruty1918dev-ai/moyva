using System;
using System.Collections.Generic;
using System.Linq;

namespace Kruty1918.Moyva.AI.Bot
{
    public sealed class BotCandidateReducer
    {
        public IReadOnlyList<BotCandidateAction> Reduce(IEnumerable<BotCandidateAction> source)
        {
            var ordered = source.OrderBy(x => x.Intent).ThenBy(x => x.Capability)
                .ThenBy(x => x.ActorKey, StringComparer.Ordinal).ThenBy(x => x.TargetKey, StringComparer.Ordinal)
                .ThenBy(x => x.X).ThenBy(x => x.Y).ThenBy(x => x.Id, StringComparer.Ordinal)
                .GroupBy(x => (x.Capability, x.Id)).Select(x => x.First()).ToList();
            var selected = ordered.Where(x => x.Intent == BotIntentType.EndTurn).Take(1).ToList();
            foreach (var group in ordered.GroupBy(x => x.Intent))
            {
                var first = group.OrderByDescending(x => x.Critical).First();
                if (selected.Count < 128 && !selected.Contains(first)) selected.Add(first);
            }
            foreach (var candidate in ordered.OrderByDescending(x => x.Critical))
                if (selected.Count < 128 && !selected.Contains(candidate)) selected.Add(candidate);
            return ordered.Where(selected.Contains).ToArray();
        }
    }
    public sealed class BotDecisionFrameBuilder
    {
        private readonly BotCapabilityRegistry _registry;
        private readonly IBotPerceptionSource _perception;
        private readonly IBotTurnGateway _turns;
        private readonly BotTelemetryHub _telemetry;
        private readonly BotRuntimeConfig _config;
        private long _sequence;
        public BotDecisionFrameBuilder(BotCapabilityRegistry registry, IBotPerceptionSource perception,
            IBotTurnGateway turns, BotTelemetryHub telemetry, BotRuntimeConfig config)
        { _registry = registry; _perception = perception; _turns = turns; _telemetry = telemetry; _config = config; }
        public BotDecisionFrame Build(string player)
        {
            var stamp = _turns.Read(player);
            var snapshot = _perception.Capture(player);
            var all = new List<BotCandidateAction>();
            var unavailable = new Dictionary<BotCapabilityId, string>();
            foreach (var provider in _registry.Providers)
            {
                snapshot.Global[BotObservationSchema.Capabilities + (int)provider.Id] = 0;
                int stage = provider.Id switch { BotCapabilityId.Construction => 5, BotCapabilityId.Capture => 6, _ => (int)provider.Id };
                string reason = stage > _config.curriculumStage ? "Disabled by curriculum." : provider.UnavailableReason(player);
                if (reason != null) { unavailable[provider.Id] = reason; continue; }
                snapshot.Global[BotObservationSchema.Capabilities + (int)provider.Id] = 1;
                foreach (var candidate in provider.Enumerate(player))
                    if (candidate.Capability == provider.Id && provider.Validate(player, candidate, out _)) all.Add(candidate);
            }
            if (all.Count == 0)
            {
                all.Add(new BotCandidateAction("wait", BotCapabilityId.Turn, BotIntentType.Wait));
                _telemetry.NoLegalActionFallbackCount++;
            }
            var set = new BotCandidateSet(new BotCandidateReducer().Reduce(all));
            return new BotDecisionFrame(player, ++_sequence, stamp, set,
                BotObservationEncoder.Encode(snapshot, set, _telemetry), unavailable);
        }
    }
}
