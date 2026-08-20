using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotDecisionTraceStore : IBotDecisionTrace
    {
        private const int MaxEntries = 8;
        private readonly Dictionary<string, IReadOnlyList<BotDecisionTraceEntry>> _last = new(StringComparer.Ordinal);

        [Inject]
        public BotDecisionTraceStore() { }

        public void Record(string ownerId, long globalTurn, BotStrategicContext strategy, IReadOnlyList<BotActionCandidate> candidates)
        {
            if (string.IsNullOrWhiteSpace(ownerId)) return;
            int count = Math.Min(MaxEntries, candidates?.Count ?? 0);
            var trace = new BotDecisionTraceEntry[count];
            for (int i = 0; i < count; i++)
            {
                BotActionCandidate c = candidates[i];
                trace[i] = new BotDecisionTraceEntry(globalTurn, strategy.Posture, c.CandidateId, c.Kind, c.Score.Total,
                    string.IsNullOrWhiteSpace(c.Score.Explanation) ? c.Reason : c.Score.Explanation);
            }
            _last[ownerId.Trim()] = trace;
        }

        public IReadOnlyList<BotDecisionTraceEntry> GetLast(string ownerId)
            => !string.IsNullOrWhiteSpace(ownerId) && _last.TryGetValue(ownerId.Trim(), out IReadOnlyList<BotDecisionTraceEntry> trace)
                ? trace : Array.Empty<BotDecisionTraceEntry>();
    }
}
