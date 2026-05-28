using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Economy.Runtime
{
    internal sealed class EconomyOwnerResourcePoolService : IEconomyOwnerResourcePoolService
    {
        private const string DefaultOwnerId = EconomyManager.DefaultOwnerId;

        // Завдання: owner-pool є навмисним сховищем стартової економіки до появи першого складу.
        // Не переносити це в settlement-only storage, бо новий світ має ресурси ще до будівництва складу.
        private readonly Dictionary<string, Dictionary<string, float>> _ownerResourcePools =
            new Dictionary<string, Dictionary<string, float>>(StringComparer.Ordinal);

        public void AddOwnerResource(
            string ownerId,
            string resourceId,
            float amount,
            SignalBus signalBus)
        {
            if (string.IsNullOrWhiteSpace(resourceId) || amount <= 0f)
                return;

            string normalizedOwnerId = NormalizeOwnerId(ownerId);
            if (!_ownerResourcePools.TryGetValue(normalizedOwnerId, out var pool) || pool == null)
            {
                pool = new Dictionary<string, float>(StringComparer.Ordinal);
                _ownerResourcePools[normalizedOwnerId] = pool;
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

        public void TransferOwnerResourcesToWarehouse(
            string ownerId,
            EconomySettlementState state,
            string warehouseKey,
            SignalBus signalBus)
        {
            if (state == null || string.IsNullOrWhiteSpace(warehouseKey))
                return;

            string normalizedOwnerId = NormalizeOwnerId(ownerId);
            if (!_ownerResourcePools.TryGetValue(normalizedOwnerId, out var pool) || pool == null || pool.Count == 0)
                return;

            var transferred = new List<KeyValuePair<string, float>>(pool);
            _ownerResourcePools.Remove(normalizedOwnerId);

            for (int index = 0; index < transferred.Count; index++)
            {
                var entry = transferred[index];
                if (string.IsNullOrWhiteSpace(entry.Key) || entry.Value <= 0f)
                    continue;

                state.AddResource(entry.Key, entry.Value, warehouseKey);
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

        public bool TransferOwnerResourcesToFirstWarehouse(
            string ownerId,
            IReadOnlyDictionary<string, EconomySettlementState> settlements,
            SignalBus signalBus,
            string logTag)
        {
            if (_ownerResourcePools.Count == 0 || settlements == null || settlements.Count == 0)
                return false;

            if (!TryGetFirstWarehouseTarget(ownerId, settlements, out var settlement, out var warehouseKey))
                return false;

            string normalizedOwnerId = NormalizeOwnerId(ownerId);
            var pendingTransfer = GetOwnerPoolResourceTotals(normalizedOwnerId);
            if (pendingTransfer.Count == 0)
                return false;

            // Gameplay contract: starter resources live at owner level until the first warehouse exists,
            // then the remaining owner-pool is moved into that warehouse so normal settlement storage takes over.
            Debug.Log($"{logTag} Moving owner-pool resources into first warehouse: owner='{normalizedOwnerId}', settlement='{settlement.SettlementId}', warehouse='{warehouseKey}', entries=[{DescribeResourceEntries(pendingTransfer)}].");
            TransferOwnerResourcesToWarehouse(normalizedOwnerId, settlement, warehouseKey, signalBus);
            return true;
        }

        public void TransferOwnerResourcesToExistingWarehouses(
            IReadOnlyDictionary<string, EconomySettlementState> settlements,
            SignalBus signalBus,
            string logTag)
        {
            if (_ownerResourcePools.Count == 0)
                return;

            var owners = new List<string>(_ownerResourcePools.Keys);
            for (int ownerIndex = 0; ownerIndex < owners.Count; ownerIndex++)
                TransferOwnerResourcesToFirstWarehouse(owners[ownerIndex], settlements, signalBus, logTag);
        }

        public bool TryConsumeOwnerPoolResources(
            string ownerId,
            IReadOnlyDictionary<string, float> resourceCosts,
            Func<string, string> resolveResourceDisplayName,
            SignalBus signalBus,
            out string errorMessage)
        {
            // Завдання: construction може витрачати стартову економіку до першого складу.
            // Після появи складу ця гілка вимикається через OwnerHasAnyWarehouse і витрати йдуть зі settlement storage.
            errorMessage = null;

            if (resourceCosts == null || resourceCosts.Count == 0)
                return true;

            string normalizedOwnerId = NormalizeOwnerId(ownerId);
            if (!_ownerResourcePools.TryGetValue(normalizedOwnerId, out var pool) || pool == null || pool.Count == 0)
            {
                errorMessage = $"У власника '{normalizedOwnerId}' немає стартового запасу для списання ресурсів.";
                return false;
            }

            foreach (var pair in resourceCosts)
            {
                if (string.IsNullOrWhiteSpace(pair.Key))
                {
                    errorMessage = "Спроба списати ресурс з порожнім ID.";
                    return false;
                }

                if (pair.Value <= 0f)
                    continue;

                float currentAmount = pool.TryGetValue(pair.Key, out var current) ? current : 0f;
                if (currentAmount + 0.0001f < pair.Value)
                {
                    string displayName = resolveResourceDisplayName != null ? resolveResourceDisplayName(pair.Key) : pair.Key;
                    errorMessage = $"Недостатньо ресурсу '{displayName}' у стартовому запасі власника '{normalizedOwnerId}': потрібно {pair.Value:0.#}, зараз {currentAmount:0.#}.";
                    return false;
                }
            }

            foreach (var pair in resourceCosts)
            {
                if (pair.Value <= 0f)
                    continue;

                float before = pool.TryGetValue(pair.Key, out var current) ? current : 0f;
                float after = before - pair.Value;
                if (after <= 0.0001f)
                    pool.Remove(pair.Key);
                else
                    pool[pair.Key] = after;

                signalBus?.Fire(new SettlementResourceChangedSignal
                {
                    SettlementId = string.Empty,
                    OwnerId = normalizedOwnerId,
                    ResourceId = pair.Key,
                    NewAmount = after <= 0.0001f ? 0f : after,
                    Delta = -pair.Value,
                });
            }

            if (pool.Count == 0)
                _ownerResourcePools.Remove(normalizedOwnerId);

            return true;
        }

        public bool OwnerHasAnyWarehouse(
            string ownerId,
            IReadOnlyDictionary<string, EconomySettlementState> settlements)
        {
            if (settlements == null || settlements.Count == 0)
                return false;

            string normalizedOwnerId = NormalizeOwnerId(ownerId);
            foreach (var settlement in settlements.Values)
            {
                if (settlement == null || !settlement.IsActive)
                    continue;

                if (!string.Equals(NormalizeOwnerId(settlement.OwnerId), normalizedOwnerId, StringComparison.Ordinal))
                    continue;

                if (settlement.WarehouseResourcePools != null && settlement.WarehouseResourcePools.Count > 0)
                    return true;
            }

            return false;
        }

        public Dictionary<string, float> GetOwnerPoolResourceTotals(string ownerId)
        {
            string normalizedOwnerId = NormalizeOwnerId(ownerId);
            if (!_ownerResourcePools.TryGetValue(normalizedOwnerId, out var pool) || pool == null)
                return new Dictionary<string, float>(StringComparer.Ordinal);

            return new Dictionary<string, float>(pool, StringComparer.Ordinal);
        }

        public Dictionary<string, float> GetOwnerResourceTotals(
            IReadOnlyDictionary<string, EconomySettlementState> settlements,
            string ownerId)
        {
            var normalized = NormalizeOwnerId(ownerId);
            var totals = new Dictionary<string, float>(StringComparer.Ordinal);

            if (_ownerResourcePools.TryGetValue(normalized, out var ownerPool) && ownerPool != null)
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

        public Dictionary<string, Dictionary<string, float>> GetOwnerResourceTotalsSnapshot(
            IReadOnlyDictionary<string, EconomySettlementState> settlements)
        {
            var ownerIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (var ownerPair in _ownerResourcePools)
                ownerIds.Add(NormalizeOwnerId(ownerPair.Key));

            if (settlements != null)
            {
                foreach (var settlement in settlements.Values)
                {
                    if (settlement == null)
                        continue;

                    ownerIds.Add(NormalizeOwnerId(settlement.OwnerId));
                }
            }

            var snapshot = new Dictionary<string, Dictionary<string, float>>(StringComparer.Ordinal);
            foreach (string ownerId in ownerIds)
            {
                var totals = GetOwnerResourceTotals(settlements, ownerId);
                if (totals.Count > 0)
                    snapshot[ownerId] = totals;
            }

            return snapshot;
        }

        public Dictionary<string, Dictionary<string, float>> GetOwnerResourcePoolsSnapshot()
        {
            var snapshot = new Dictionary<string, Dictionary<string, float>>(StringComparer.Ordinal);

            foreach (var ownerPair in _ownerResourcePools)
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
            Dictionary<string, Dictionary<string, float>> snapshot,
            SignalBus signalBus)
        {
            _ownerResourcePools.Clear();

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
                    _ownerResourcePools[ownerId] = ownerPool;
            }
        }

        private bool TryGetFirstWarehouseTarget(
            string ownerId,
            IReadOnlyDictionary<string, EconomySettlementState> settlements,
            out EconomySettlementState settlement,
            out string warehouseKey)
        {
            settlement = null;
            warehouseKey = null;

            string normalizedOwnerId = NormalizeOwnerId(ownerId);
            string selectedSettlementId = null;
            string selectedWarehouseKey = null;

            foreach (var candidate in settlements.Values)
            {
                if (candidate == null || !candidate.IsActive)
                    continue;

                if (!string.Equals(NormalizeOwnerId(candidate.OwnerId), normalizedOwnerId, StringComparison.Ordinal))
                    continue;

                string candidateWarehouseKey = GetFirstWarehouseKey(candidate);
                if (string.IsNullOrWhiteSpace(candidateWarehouseKey))
                    continue;

                bool isEarlierSettlement = selectedSettlementId == null
                    || string.CompareOrdinal(candidate.SettlementId, selectedSettlementId) < 0;
                bool isEarlierWarehouseInSameSettlement = string.Equals(candidate.SettlementId, selectedSettlementId, StringComparison.Ordinal)
                    && (selectedWarehouseKey == null || string.CompareOrdinal(candidateWarehouseKey, selectedWarehouseKey) < 0);

                if (!isEarlierSettlement && !isEarlierWarehouseInSameSettlement)
                    continue;

                settlement = candidate;
                warehouseKey = candidateWarehouseKey;
                selectedSettlementId = candidate.SettlementId;
                selectedWarehouseKey = candidateWarehouseKey;
            }

            return settlement != null && !string.IsNullOrWhiteSpace(warehouseKey);
        }

        private static string GetFirstWarehouseKey(EconomySettlementState state)
        {
            if (state?.WarehouseResourcePools == null || state.WarehouseResourcePools.Count == 0)
                return null;

            string first = null;
            foreach (var key in state.WarehouseResourcePools.Keys)
            {
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                if (first == null || string.CompareOrdinal(key, first) < 0)
                    first = key;
            }

            return first;
        }

        private static string DescribeResourceEntries(IReadOnlyDictionary<string, float> entries)
        {
            if (entries == null || entries.Count == 0)
                return "none";

            var parts = new List<string>(entries.Count);
            foreach (var entry in entries)
            {
                if (string.IsNullOrWhiteSpace(entry.Key) || entry.Value <= 0f)
                    continue;

                parts.Add($"{entry.Key.Trim()}={entry.Value:0.##}");
            }

            return parts.Count == 0 ? "none" : string.Join(", ", parts);
        }

        private static string NormalizeOwnerId(string ownerId)
        {
            return string.IsNullOrWhiteSpace(ownerId) ? DefaultOwnerId : ownerId.Trim();
        }
    }
}