using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Signals;

namespace Kruty1918.Moyva.BotAI.Editor.Analyzer
{
    internal sealed class BotAnalyzerEconomyCollector
    {
        private readonly IEconomyInfoMediator _economy;
        private string _cachedOwner = string.Empty;
        private double _nextSampleTime;
        private List<BotAnalyzerResourceState> _cached = new();

        public BotAnalyzerEconomyCollector(IEconomyInfoMediator economy)
        {
            _economy = economy;
        }

        public List<BotAnalyzerResourceState> Collect(
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
            _nextSampleTime = now + Math.Max(0.10f, settings?.EconomySampleInterval ?? 0.20f);

            if (_economy == null || string.IsNullOrWhiteSpace(ownerId))
            {
                _cached = new List<BotAnalyzerResourceState>();
                return Clone(_cached);
            }

            IReadOnlyDictionary<string, float> totals = null;
            try
            {
                totals = _economy.GetOwnerResourceTotals(ownerId);
            }
            catch
            {
                totals = null;
            }

            _cached = FromTotals(
                totals,
                id =>
                {
                    try { return _economy.GetResourceDisplayName(id); }
                    catch { return id; }
                });

            return Clone(_cached);
        }

        internal static List<BotAnalyzerResourceState> FromTotals(
            IReadOnlyDictionary<string, float> totals,
            Func<string, string> displayNameResolver = null)
        {
            var result = new List<BotAnalyzerResourceState>();
            if (totals == null)
                return result;

            var ids = new List<string>(totals.Keys);
            ids.Sort(StringComparer.Ordinal);

            foreach (string id in ids)
            {
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                string displayName = displayNameResolver?.Invoke(id);
                result.Add(new BotAnalyzerResourceState
                {
                    ResourceId = id,
                    DisplayName = string.IsNullOrWhiteSpace(displayName) ? id : displayName,
                    Amount = totals[id],
                });
            }

            return result;
        }

        private static List<BotAnalyzerResourceState> Clone(List<BotAnalyzerResourceState> source)
        {
            var result = new List<BotAnalyzerResourceState>();
            if (source == null)
                return result;

            foreach (BotAnalyzerResourceState item in source)
            {
                if (item == null)
                    continue;
                result.Add(new BotAnalyzerResourceState
                {
                    ResourceId = item.ResourceId,
                    DisplayName = item.DisplayName,
                    Amount = item.Amount,
                });
            }
            return result;
        }
    }
}
