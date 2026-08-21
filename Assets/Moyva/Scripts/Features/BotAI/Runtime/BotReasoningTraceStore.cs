using System;
using System.Collections.Generic;
using Kruty1918.Moyva.BotAI.API;
using UnityEngine;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    internal sealed class BotReasoningTraceStore : IBotReasoningTrace
    {
        private const int MaxEntriesPerOwner = 1024;

        private readonly Dictionary<string, List<BotReasoningEntry>> _entries =
            new(StringComparer.Ordinal);

        private long _sequence;

        public BotReasoningEntry Record(
            string ownerId,
            long globalTurn,
            BotReasoningStage stage,
            string headline,
            string narrative,
            int score = 0,
            Vector2Int? targetCell = null,
            string subjectId = null,
            IReadOnlyList<BotSiteScoreFactor> factors = null)
        {
            string owner = Normalize(ownerId);
            var entry = new BotReasoningEntry(
                ++_sequence,
                owner,
                globalTurn,
                stage,
                headline,
                narrative,
                score,
                targetCell,
                subjectId,
                factors);

            if (!_entries.TryGetValue(owner, out List<BotReasoningEntry> list))
            {
                list = new List<BotReasoningEntry>();
                _entries.Add(owner, list);
            }

            list.Add(entry);
            int excess = list.Count - MaxEntriesPerOwner;
            if (excess > 0)
                list.RemoveRange(0, excess);

            return entry;
        }

        public IReadOnlyList<BotReasoningEntry> GetEntries(string ownerId)
        {
            string owner = Normalize(ownerId);
            if (!_entries.TryGetValue(owner, out List<BotReasoningEntry> list))
                return Array.Empty<BotReasoningEntry>();

            return list.ToArray();
        }

        public IReadOnlyList<BotReasoningEntry> GetEntriesSince(
            string ownerId,
            long sequenceExclusive)
        {
            string owner = Normalize(ownerId);
            if (!_entries.TryGetValue(owner, out List<BotReasoningEntry> list))
                return Array.Empty<BotReasoningEntry>();

            var result = new List<BotReasoningEntry>();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Sequence > sequenceExclusive)
                    result.Add(list[i]);
            }
            return result;
        }

        public void Clear(string ownerId)
            => _entries.Remove(Normalize(ownerId));

        private static string Normalize(string value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }
}
