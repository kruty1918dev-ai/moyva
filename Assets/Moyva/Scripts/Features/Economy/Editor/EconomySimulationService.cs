using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Economy.API;
using UnityEngine;

namespace Kruty1918.Moyva.Economy.Editor
{
    public sealed class EconomySimulationInput
    {
        public EconomySettlementDefinition Settlement;
        public IReadOnlyList<EconomyProductionProfile> ProductionProfiles;
        public float DurationMinutes;
    }

    public sealed class EconomySimulationResult
    {
        public EconomySettlementDefinition Settlement;
        public float DurationMinutes;
        public IReadOnlyDictionary<string, int> ResourceTotals;
        public IReadOnlyList<string> Log;
    }

    public sealed class EconomySimulationService
    {
        public EconomySimulationResult Simulate(EconomySimulationInput input)
        {
            var durationMinutes = Mathf.Max(0f, input?.DurationMinutes ?? 0f);
            var durationSeconds = durationMinutes * 60f;

            var totals = new SortedDictionary<string, int>(StringComparer.Ordinal);
            var log = new List<string>();

            var profiles = (input?.ProductionProfiles ?? Array.Empty<EconomyProductionProfile>())
                .Where(profile => profile != null)
                .OrderBy(profile => profile.BuildingId ?? string.Empty, StringComparer.Ordinal)
                .ThenBy(profile => profile.RecipeId ?? string.Empty, StringComparer.Ordinal)
                .ToList();

            foreach (var profile in profiles)
            {
                if (profile.CycleDurationSeconds <= 0f || profile.OutputAmountPerCycle <= 0)
                    continue;

                var cycleCount = Mathf.FloorToInt(durationSeconds / profile.CycleDurationSeconds);
                if (cycleCount <= 0)
                    continue;

                var resourceId = (profile.RecipeId ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(resourceId))
                    continue;

                var delta = cycleCount * profile.OutputAmountPerCycle;
                totals[resourceId] = totals.TryGetValue(resourceId, out var current)
                    ? current + delta
                    : delta;

                log.Add($"{profile.BuildingId} -> {resourceId}: +{delta} ({cycleCount} cycles)");
            }

            return new EconomySimulationResult
            {
                Settlement = input?.Settlement,
                DurationMinutes = durationMinutes,
                ResourceTotals = totals,
                Log = log,
            };
        }
    }
}
