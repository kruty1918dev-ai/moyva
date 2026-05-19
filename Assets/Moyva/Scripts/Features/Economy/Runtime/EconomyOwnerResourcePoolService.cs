using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Signals;
using Zenject;

namespace Kruty1918.Moyva.Economy.Runtime
{
    internal sealed class EconomyOwnerResourcePoolService : IEconomyOwnerResourcePoolService
    {
        private const string DefaultOwnerId = EconomyManager.DefaultOwnerId;

        public void AddOwnerResource(
            Dictionary<string, Dictionary<string, float>> pools,
            string ownerId,
            string resourceId,
            float amount,
            SignalBus signalBus)
        {
            if (pools == null || string.IsNullOrWhiteSpace(resourceId) || amount <= 0f)
                return;

            string normalizedOwnerId = NormalizeOwnerId(ownerId);
            if (!pools.TryGetValue(normalizedOwnerId, out var pool) || pool == null)
            {
                pool = new Dictionary<string, float>(StringComparer.Ordinal);
                pools[normalizedOwnerId] = pool;
            }

            float before = pool.TryGetValue(resourceId, out var current) ? current : 0f;
            pool[resourceId] = before + amount;

            signalBus?.Fire(new SettlementResourceChangedSignal
            {
                SettlementId = string.Empty,
                OwnerId = normalizedOwnerId,
                ResourceId = resourceId,
                NewAmount = pool[resourceId],
                Delta = amount,
            });
        }

        public void TransferOwnerResourcesToSettlement(
            Dictionary<string, Dictionary<string, float>> pools,
            string ownerId,
            EconomySettlementState state,
            SignalBus signalBus)
        {
            if (pools == null || state == null)
                return;

            string normalizedOwnerId = NormalizeOwnerId(ownerId);
            if (!pools.TryGetValue(normalizedOwnerId, out var pool) || pool == null || pool.Count == 0)
                return;

            var transferred = new List<KeyValuePair<string, float>>(pool);
            pools.Remove(normalizedOwnerId);

            for (int index = 0; index < transferred.Count; index++)
            {
                var entry = transferred[index];
                if (string.IsNullOrWhiteSpace(entry.Key) || entry.Value <= 0f)
                    continue;

                state.AddResource(entry.Key, entry.Value);
                signalBus?.Fire(new SettlementResourceChangedSignal
                {
                    SettlementId = state.SettlementId,
                    OwnerId = normalizedOwnerId,
                    ResourceId = entry.Key,
                    NewAmount = state.GetResource(entry.Key),
                    Delta = entry.Value,
                });
            }
        }

        public Dictionary<string, float> GetOwnerResourceTotals(
            Dictionary<string, Dictionary<string, float>> pools,
            IReadOnlyDictionary<string, EconomySettlementState> settlements,
            string ownerId)
        {
            var normalized = NormalizeOwnerId(ownerId);
            var totals = new Dictionary<string, float>(StringComparer.Ordinal);

            if (pools != null && pools.TryGetValue(normalized, out var ownerPool) && ownerPool != null)
            {
                foreach (var resource in ownerPool)
                {
                    if (totals.ContainsKey(resource.Key))
                        totals[resource.Key] += resource.Value;
                    else
                        totals[resource.Key] = resource.Value;
                }
            }

            if (settlements == null)
                return totals;

            foreach (var settlement in settlements.Values)
            {
                if (settlement == null)
                    continue;

                if (!string.Equals(NormalizeOwnerId(settlement.OwnerId), normalized, StringComparison.Ordinal))
                    continue;

                foreach (var resource in settlement.ResourcePool)
                {
                    if (totals.ContainsKey(resource.Key))
                        totals[resource.Key] += resource.Value;
                    else
                        totals[resource.Key] = resource.Value;
                }
            }

            return totals;
        }

        public Dictionary<string, Dictionary<string, float>> GetOwnerResourcePoolsSnapshot(
            Dictionary<string, Dictionary<string, float>> pools)
        {
            var snapshot = new Dictionary<string, Dictionary<string, float>>(StringComparer.Ordinal);
            if (pools == null)
                return snapshot;

            foreach (var ownerPair in pools)
            {
                if (string.IsNullOrWhiteSpace(ownerPair.Key) || ownerPair.Value == null || ownerPair.Value.Count == 0)
                    continue;

                var ownerPool = new Dictionary<string, float>(StringComparer.Ordinal);
                foreach (var resourcePair in ownerPair.Value)
                {
                    if (string.IsNullOrWhiteSpace(resourcePair.Key) || resourcePair.Value <= 0f)
                        continue;

                    ownerPool[resourcePair.Key] = resourcePair.Value;
                }

                if (ownerPool.Count > 0)
                    snapshot[NormalizeOwnerId(ownerPair.Key)] = ownerPool;
            }

            return snapshot;
        }

        public void RestoreOwnerResourcePools(
            Dictionary<string, Dictionary<string, float>> pools,
            Dictionary<string, Dictionary<string, float>> snapshot,
            SignalBus signalBus)
        {
            if (pools == null)
                return;

            pools.Clear();

            if (snapshot == null || snapshot.Count == 0)
                return;

            foreach (var ownerPair in snapshot)
            {
                string ownerId = NormalizeOwnerId(ownerPair.Key);
                if (ownerPair.Value == null || ownerPair.Value.Count == 0)
                    continue;

                var ownerPool = new Dictionary<string, float>(StringComparer.Ordinal);
                foreach (var resourcePair in ownerPair.Value)
                {
                    if (string.IsNullOrWhiteSpace(resourcePair.Key) || resourcePair.Value <= 0f)
                        continue;

                    string resourceId = resourcePair.Key.Trim();
                    ownerPool[resourceId] = resourcePair.Value;

                    signalBus?.Fire(new SettlementResourceChangedSignal
                    {
                        SettlementId = string.Empty,
                        OwnerId = ownerId,
                        ResourceId = resourceId,
                        NewAmount = resourcePair.Value,
                        Delta = resourcePair.Value,
                    });
                }

                if (ownerPool.Count > 0)
                    pools[ownerId] = ownerPool;
            }
        }

        public void TransferOwnerResourcesToExistingSettlements(
            Dictionary<string, Dictionary<string, float>> pools,
            IReadOnlyDictionary<string, EconomySettlementState> settlements,
            SignalBus signalBus)
        {
            if (pools == null || settlements == null || pools.Count == 0 || settlements.Count == 0)
                return;

            var owners = new List<string>(pools.Keys);
            for (int ownerIndex = 0; ownerIndex < owners.Count; ownerIndex++)
            {
                string normalizedOwnerId = NormalizeOwnerId(owners[ownerIndex]);
                foreach (var settlement in settlements.Values)
                {
                    if (settlement == null || !settlement.IsActive)
                        continue;

                    if (!string.Equals(NormalizeOwnerId(settlement.OwnerId), normalizedOwnerId, StringComparison.Ordinal))
                        continue;

                    TransferOwnerResourcesToSettlement(pools, normalizedOwnerId, settlement, signalBus);
                    break;
                }
            }
        }

        private static string NormalizeOwnerId(string ownerId)
        {
            return string.IsNullOrWhiteSpace(ownerId) ? DefaultOwnerId : ownerId.Trim();
        }
    }
}