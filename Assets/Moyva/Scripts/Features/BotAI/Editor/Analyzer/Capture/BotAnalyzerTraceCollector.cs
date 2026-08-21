using System;
using System.Collections.Generic;
using System.Text;
using Kruty1918.Moyva.BotAI.API;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer
{
    internal sealed class BotAnalyzerTraceCollector
    {
        private readonly IBotDecisionTrace _trace;
        private string _cachedOwner = string.Empty;
        private double _nextSampleTime;
        private List<BotAnalyzerCandidateState> _cached = new();

        public BotAnalyzerTraceCollector(IBotDecisionTrace trace)
        {
            _trace = trace;
        }

        public List<BotAnalyzerCandidateState> Collect(
            string ownerId,
            double now,
            BotAnalyzerSettings settings,
            bool force = false)
        {
            if (!force &&
                string.Equals(_cachedOwner, ownerId, StringComparison.Ordinal) &&
                now < _nextSampleTime)
            {
                return Clone(_cached);
            }

            _cachedOwner = ownerId ?? string.Empty;
            _nextSampleTime = now + Math.Max(0.05f, settings?.TraceSampleInterval ?? 0.10f);

            IReadOnlyList<BotDecisionTraceEntry> entries = null;
            if (_trace != null && !string.IsNullOrWhiteSpace(ownerId))
            {
                try
                {
                    entries = _trace.GetLast(ownerId);
                }
                catch
                {
                    entries = null;
                }
            }

            _cached = Map(entries);
            return Clone(_cached);
        }

        internal static List<BotAnalyzerCandidateState> Map(IReadOnlyList<BotDecisionTraceEntry> entries)
        {
            var result = new List<BotAnalyzerCandidateState>();
            if (entries == null)
                return result;

            for (int i = 0; i < entries.Count; i++)
            {
                BotDecisionTraceEntry entry = entries[i];
                result.Add(new BotAnalyzerCandidateState
                {
                    Rank = i + 1,
                    GlobalTurn = entry.GlobalTurn,
                    Posture = entry.Posture.ToString(),
                    CandidateId = entry.CandidateId,
                    Kind = entry.Kind.ToString(),
                    Score = entry.Score,
                    Explanation = entry.Explanation,
                });
            }

            return result;
        }

        internal static string Fingerprint(IReadOnlyList<BotAnalyzerCandidateState> candidates)
        {
            if (candidates == null || candidates.Count == 0)
                return string.Empty;

            var builder = new StringBuilder(candidates.Count * 48);
            for (int i = 0; i < candidates.Count; i++)
            {
                BotAnalyzerCandidateState item = candidates[i];
                if (item == null)
                    continue;

                builder
                    .Append(item.GlobalTurn).Append('|')
                    .Append(item.CandidateId).Append('|')
                    .Append(item.Kind).Append('|')
                    .Append(item.Score).Append(';');
            }
            return builder.ToString();
        }

        private static List<BotAnalyzerCandidateState> Clone(List<BotAnalyzerCandidateState> source)
        {
            var result = new List<BotAnalyzerCandidateState>();
            if (source == null)
                return result;

            foreach (BotAnalyzerCandidateState item in source)
            {
                if (item == null)
                    continue;

                result.Add(new BotAnalyzerCandidateState
                {
                    Rank = item.Rank,
                    GlobalTurn = item.GlobalTurn,
                    Posture = item.Posture,
                    CandidateId = item.CandidateId,
                    Kind = item.Kind,
                    Score = item.Score,
                    Explanation = item.Explanation,
                    ActorId = item.ActorId,
                    TargetId = item.TargetId,
                    HasTargetCell = item.HasTargetCell,
                    TargetCell = item.TargetCell,
                    DefinitionId = item.DefinitionId,
                    Reason = item.Reason,
                });
            }
            return result;
        }
    }
}
