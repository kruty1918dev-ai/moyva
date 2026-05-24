using System.Collections.Generic;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal interface IBootstrapStarterPackGrantService
    {
        bool HasStarterPackEntries();
        string DescribeConfiguredEntries();
        bool TryGrant(string settlementId, string ownerId);
    }

    internal sealed class BootstrapStarterPackGrantService : IBootstrapStarterPackGrantService
    {
        private const string StarterPackLogTag = "[Bootstrap][StarterPack]";

        private readonly BootstrapGameSettings _settings;
        private readonly SignalBus _signalBus;

        public BootstrapStarterPackGrantService(BootstrapGameSettings settings, SignalBus signalBus)
        {
            _settings = settings;
            _signalBus = signalBus;
        }

        public bool HasStarterPackEntries()
            => BootstrapStarterPackResourceUtility.HasEntries(_settings.InitialResources);

        public string DescribeConfiguredEntries()
            => BootstrapStarterPackResourceUtility.DescribeEntries(_settings.InitialResources);

        public bool TryGrant(string settlementId, string ownerId)
        {
            var entries = _settings.InitialResources;
            if (entries == null || entries.Count == 0)
            {
                Debug.Log($"{StarterPackLogTag} Skip grant: no starter-pack entries configured for owner '{ownerId}'.");
                return true;
            }

            var payload = new List<StarterPackResourceEntrySignal>();
            bool grantedAny = false;
            for (int index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                if (entry == null || string.IsNullOrWhiteSpace(entry.ResourceId) || entry.Amount <= 0f)
                    continue;

                payload.Add(new StarterPackResourceEntrySignal
                {
                    ResourceId = entry.ResourceId.Trim(),
                    Amount = entry.Amount,
                });
                grantedAny = true;
            }

            if (!grantedAny)
            {
                Debug.LogWarning($"{StarterPackLogTag} Skip grant: configured starter-pack entries for owner '{ownerId}' are empty after validation.");
                return true;
            }

            _signalBus.Fire(new GrantStarterPackResourcesSignal
            {
                SettlementId = settlementId,
                OwnerId = ownerId,
                Entries = payload.ToArray(),
            });

            string target = string.IsNullOrWhiteSpace(settlementId)
                ? "owner pool"
                : $"settlement='{settlementId}'";
            Debug.Log($"{StarterPackLogTag} Grant fired: owner='{ownerId}', target={target}, entries=[{BootstrapStarterPackResourceUtility.DescribeEntries(payload)}].");
            Debug.Log($"[Bootstrap] Видано стартовий пакет owner='{ownerId}' для {target}.");

            return true;
        }
    }

    internal static class BootstrapStarterPackResourceUtility
    {
        public static bool HasEntries(IReadOnlyList<InitialResourceEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                return false;

            for (int index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                if (entry != null && !string.IsNullOrWhiteSpace(entry.ResourceId) && entry.Amount > 0f)
                    return true;
            }

            return false;
        }

        public static string DescribeEntries(IReadOnlyList<InitialResourceEntry> entries)
        {
            if (entries == null || entries.Count == 0)
                return "none";

            var parts = new List<string>(entries.Count);
            for (int index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                if (entry == null || string.IsNullOrWhiteSpace(entry.ResourceId) || entry.Amount <= 0f)
                    continue;

                parts.Add($"{entry.ResourceId.Trim()}={entry.Amount:0.##}");
            }

            return parts.Count == 0 ? "none" : string.Join(", ", parts);
        }

        public static string DescribeEntries(IReadOnlyList<StarterPackResourceEntrySignal> entries)
        {
            if (entries == null || entries.Count == 0)
                return "none";

            var parts = new List<string>(entries.Count);
            for (int index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                if (string.IsNullOrWhiteSpace(entry.ResourceId) || entry.Amount <= 0f)
                    continue;

                parts.Add($"{entry.ResourceId.Trim()}={entry.Amount:0.##}");
            }

            return parts.Count == 0 ? "none" : string.Join(", ", parts);
        }
    }
}