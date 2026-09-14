using System;
using System.Collections.Generic;
using System.Linq;

namespace Kruty1918.Moyva.AI.Training
{
    public sealed class TrainingDecisionJournal
    {
        private readonly int _capacity;
        private readonly Queue<AgentDecisionEvent> _events;
        private long _sequence;
        public TrainingDecisionJournal(int capacity = 4096)
        { _capacity = Math.Max(64, capacity); _events = new Queue<AgentDecisionEvent>(_capacity); }

        public long NextSequence() => ++_sequence;
        public void Append(AgentDecisionEvent entry)
        {
            if (entry == null) return;
            if (entry.sequence <= 0) entry.sequence = NextSequence();
            else _sequence = Math.Max(_sequence, entry.sequence);
            while (_events.Count >= _capacity) _events.Dequeue();
            _events.Enqueue(entry);
        }

        public AgentDecisionEvent[] Query(int? arena = null, string agent = null, string action = null, bool errorsOnly = false, long afterSequence = 0)
            => _events.Where(e => e.sequence > afterSequence
                && (!arena.HasValue || e.arenaId == arena.Value)
                && (string.IsNullOrWhiteSpace(agent) || string.Equals(e.agentId, agent, StringComparison.Ordinal))
                && (string.IsNullOrWhiteSpace(action) || string.Equals(e.actionId, action, StringComparison.Ordinal))
                && (!errorsOnly || !string.IsNullOrWhiteSpace(e.rejectionReason)))
                .ToArray();

        public void ResetSequence(long value = 0) => _sequence = Math.Max(0, value);
    }
}
