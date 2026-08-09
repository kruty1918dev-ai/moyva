using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;

namespace Kruty1918.Moyva.Economy.Runtime
{
    /// <summary>
    /// Mutable runtime state of a single settlement during a game session.
    /// Holds population roster, resource pool, and building-worker assignments.
    /// </summary>
    [Serializable]
    public sealed class EconomySettlementState
    {
        public string SettlementId;
        public string SettlementName;
        public string OwnerId;
        public bool IsActive = true;
        public int CurrentTurn;

        // Population
        public List<EconomyResidentState> Residents = new List<EconomyResidentState>();

        // Resources — shared pool per settlement (keyed by resource id)
        public Dictionary<string, float> ResourcePool = new Dictionary<string, float>(StringComparer.Ordinal);

        // Resources per warehouse instance (key = warehouse key e.g. "x:y")
        public Dictionary<string, Dictionary<string, float>> WarehouseResourcePools =
            new Dictionary<string, Dictionary<string, float>>(StringComparer.Ordinal);

        public Dictionary<string, EconomyBuildingWarehousePolicy> WarehousePolicies =
            new Dictionary<string, EconomyBuildingWarehousePolicy>(StringComparer.Ordinal);

        [NonSerialized]
        private Dictionary<string, float> _warehouseReservationScratch;

        // Worker assignments — keyed by stable building instance key, not type ID.
        public Dictionary<string, int> WorkerAssignments =
            new Dictionary<string, int>(StringComparer.Ordinal);

        // Buildings with their active production state
        public List<EconomyBuildingState> Buildings = new List<EconomyBuildingState>();

        // Housing capacity sum from all housing buildings
        public int TotalHousingCapacity;

        public float GetResource(string resourceId)
        {
            return ResourcePool.TryGetValue(resourceId, out var amount) ? amount : 0f;
        }

        public void AddResource(string resourceId, float amount, string warehouseKey = null)
        {
            if (ResourcePool.ContainsKey(resourceId))
                ResourcePool[resourceId] += amount;
            else
                ResourcePool[resourceId] = amount;

            if (amount <= 0f)
                return;

            var targetWarehouse =
                ResolveWarehouseKey(
                    warehouseKey,
                    resourceId,
                    amount);
            if (string.IsNullOrEmpty(targetWarehouse))
                return;

            EnsureWarehousePool(targetWarehouse);
            var pool = WarehouseResourcePools[targetWarehouse];
            if (pool.ContainsKey(resourceId))
                pool[resourceId] += amount;
            else
                pool[resourceId] = amount;
        }

        public bool ConsumeResource(string resourceId, float amount)
        {
            if (!ResourcePool.TryGetValue(resourceId, out var current) || current < amount)
                return false;

            ResourcePool[resourceId] = current - amount;
            if (ResourcePool[resourceId] <= 0.0001f)
                ResourcePool.Remove(resourceId);

            float remaining = amount;
            foreach (var warehouse in WarehouseResourcePools)
            {
                if (warehouse.Value == null)
                    continue;

                if (!warehouse.Value.TryGetValue(resourceId, out var warehouseAmount) || warehouseAmount <= 0f)
                    continue;

                float consumed = Math.Min(warehouseAmount, remaining);
                warehouse.Value[resourceId] = warehouseAmount - consumed;
                if (warehouse.Value[resourceId] <= 0.0001f)
                    warehouse.Value.Remove(resourceId);

                remaining -= consumed;
                if (remaining <= 0.0001f)
                    break;
            }

            return true;
        }

        public void EnsureWarehousePool(string warehouseKey)
        {
            if (string.IsNullOrWhiteSpace(warehouseKey))
                return;

            if (!WarehouseResourcePools.ContainsKey(warehouseKey))
                WarehouseResourcePools[warehouseKey] = new Dictionary<string, float>(StringComparer.Ordinal);
        }

        public void RemoveWarehousePool(string warehouseKey)
        {
            if (string.IsNullOrWhiteSpace(warehouseKey))
                return;

            WarehouseResourcePools.Remove(warehouseKey);
            WarehousePolicies.Remove(warehouseKey);
        }

        public void ConfigureWarehousePolicy(
            string warehouseKey,
            int capacity,
            IReadOnlyList<string> acceptedResourceIds)
        {
            if (string.IsNullOrWhiteSpace(warehouseKey))
                return;

            EnsureWarehousePool(warehouseKey);
            var normalized = new List<string>();
            if (acceptedResourceIds != null)
            {
                for (int index = 0;
                     index < acceptedResourceIds.Count;
                     index++)
                {
                    string resourceId =
                        acceptedResourceIds[index]?.Trim();
                    if (!string.IsNullOrWhiteSpace(resourceId)
                        && !normalized.Contains(resourceId))
                    {
                        normalized.Add(resourceId);
                    }
                }
            }

            WarehousePolicies[warehouseKey] =
                new EconomyBuildingWarehousePolicy
                {
                    Capacity = capacity < -1 ? -1 : capacity,
                    AcceptedResourceIds = normalized.ToArray(),
                };
        }

        public void MoveWarehousePolicy(
            string previousWarehouseKey,
            string nextWarehouseKey)
        {
            if (string.IsNullOrWhiteSpace(previousWarehouseKey)
                || string.IsNullOrWhiteSpace(nextWarehouseKey)
                || string.Equals(
                    previousWarehouseKey,
                    nextWarehouseKey,
                    StringComparison.Ordinal))
            {
                return;
            }

            if (WarehousePolicies.TryGetValue(
                    previousWarehouseKey,
                    out EconomyBuildingWarehousePolicy policy))
            {
                WarehousePolicies.Remove(previousWarehouseKey);
                WarehousePolicies[nextWarehouseKey] = policy;
            }
        }

        public bool CanStoreResources(
            IReadOnlyList<BuildingResourceAmount> outputs)
        {
            if (outputs == null || outputs.Count == 0)
                return true;

            Dictionary<string, float> reservedByWarehouse =
                GetWarehouseReservationScratch();

            for (int index = 0; index < outputs.Count; index++)
            {
                BuildingResourceAmount output = outputs[index];
                if (output == null
                    || string.IsNullOrWhiteSpace(output.ResourceId)
                    || output.Amount <= 0)
                {
                    continue;
                }

                if (!TryReserveWarehouseCapacity(
                        output.ResourceId,
                        output.Amount,
                        reservedByWarehouse))
                {
                    return false;
                }
            }

            return true;
        }

        public bool CanStoreResource(
            string resourceId,
            float amount)
        {
            if (string.IsNullOrWhiteSpace(resourceId)
                || amount <= 0f)
            {
                return true;
            }

            Dictionary<string, float> reservedByWarehouse =
                GetWarehouseReservationScratch();
            return TryReserveWarehouseCapacity(
                resourceId,
                amount,
                reservedByWarehouse);
        }

        private Dictionary<string, float>
            GetWarehouseReservationScratch()
        {
            _warehouseReservationScratch ??=
                new Dictionary<string, float>(StringComparer.Ordinal);
            _warehouseReservationScratch.Clear();
            return _warehouseReservationScratch;
        }

        private bool TryReserveWarehouseCapacity(
            string resourceId,
            float amount,
            Dictionary<string, float> reservedByWarehouse)
        {
            foreach (string warehouseKey
                     in WarehouseResourcePools.Keys)
            {
                float alreadyReserved =
                    reservedByWarehouse.TryGetValue(
                        warehouseKey,
                        out float reserved)
                        ? reserved
                        : 0f;

                if (!CanWarehouseAccept(
                        warehouseKey,
                        resourceId,
                        amount,
                        alreadyReserved))
                {
                    continue;
                }

                reservedByWarehouse[warehouseKey] =
                    alreadyReserved + amount;
                return true;
            }

            return false;
        }

        public Dictionary<string, float> GetWarehouseSnapshot(string warehouseKey)
        {
            EnsureWarehouseConsistency();

            if (!WarehouseResourcePools.TryGetValue(warehouseKey, out var pool) || pool == null)
                return new Dictionary<string, float>(StringComparer.Ordinal);

            return new Dictionary<string, float>(pool, StringComparer.Ordinal);
        }

        public Dictionary<string, float> GetAllWarehousesTotalSnapshot()
        {
            EnsureWarehouseConsistency();

            var result = new Dictionary<string, float>(StringComparer.Ordinal);
            foreach (var warehouse in WarehouseResourcePools)
            {
                if (warehouse.Value == null)
                    continue;

                foreach (var resource in warehouse.Value)
                {
                    if (result.ContainsKey(resource.Key))
                        result[resource.Key] += resource.Value;
                    else
                        result[resource.Key] = resource.Value;
                }
            }

            return result;
        }

        public void EnsureWarehouseConsistency()
        {
            if (WarehouseResourcePools.Count == 0)
                return;

            foreach (var resource in ResourcePool)
            {
                float inWarehouses = 0f;
                foreach (var warehouse in WarehouseResourcePools)
                {
                    if (warehouse.Value != null
                        && warehouse.Value.TryGetValue(
                            resource.Key,
                            out float amount))
                    {
                        inWarehouses += amount;
                    }
                }

                float delta = resource.Value - inWarehouses;
                if (Math.Abs(delta) <= 0.0001f)
                    continue;

                if (delta > 0f)
                {
                    string targetWarehouse = ResolveWarehouseKey(
                        null,
                        resource.Key,
                        delta);
                    if (string.IsNullOrWhiteSpace(targetWarehouse))
                        continue;

                    EnsureWarehousePool(targetWarehouse);
                    Dictionary<string, float> targetPool =
                        WarehouseResourcePools[targetWarehouse];
                    if (targetPool.ContainsKey(resource.Key))
                        targetPool[resource.Key] += delta;
                    else
                        targetPool[resource.Key] = delta;
                    continue;
                }

                float excess = -delta;
                foreach (var warehouse in WarehouseResourcePools)
                {
                    if (excess <= 0.0001f
                        || warehouse.Value == null
                        || !warehouse.Value.TryGetValue(
                            resource.Key,
                            out float stored)
                        || stored <= 0f)
                    {
                        continue;
                    }

                    float remove = Math.Min(stored, excess);
                    float next = stored - remove;
                    if (next <= 0.0001f)
                        warehouse.Value.Remove(resource.Key);
                    else
                        warehouse.Value[resource.Key] = next;
                    excess -= remove;
                }
            }
        }

        private string ResolveWarehouseKey(
            string preferred,
            string resourceId = null,
            float amount = 0f)
        {
            if (!string.IsNullOrWhiteSpace(preferred))
            {
                if (!WarehouseResourcePools.ContainsKey(preferred)
                    || CanWarehouseAccept(
                        preferred,
                        resourceId,
                        amount))
                {
                    return preferred;
                }
            }

            string first = null;
            foreach (string key in WarehouseResourcePools.Keys)
            {
                if (!CanWarehouseAccept(
                        key,
                        resourceId,
                        amount))
                {
                    continue;
                }

                if (first == null
                    || string.CompareOrdinal(key, first) < 0)
                {
                    first = key;
                }
            }

            return first;
        }

        private bool CanWarehouseAccept(
            string warehouseKey,
            string resourceId,
            float amount,
            float reservedCapacity = 0f)
        {
            if (!WarehouseResourcePools.TryGetValue(
                    warehouseKey,
                    out Dictionary<string, float> pool)
                || pool == null)
            {
                return false;
            }

            if (!WarehousePolicies.TryGetValue(
                    warehouseKey,
                    out EconomyBuildingWarehousePolicy policy)
                || policy == null)
            {
                return true;
            }

            if (policy.AcceptedResourceIds != null
                && policy.AcceptedResourceIds.Length > 0
                && !ContainsResourceId(
                    policy.AcceptedResourceIds,
                    resourceId))
            {
                return false;
            }

            if (policy.Capacity < 0)
                return true;

            float stored = 0f;
            foreach (var pair in pool)
                stored += Math.Max(0f, pair.Value);

            return stored
                + Math.Max(0f, reservedCapacity)
                + Math.Max(0f, amount)
                <= policy.Capacity + 0.0001f;
        }

        private static bool ContainsResourceId(
            IReadOnlyList<string> values,
            string resourceId)
        {
            if (values == null
                || string.IsNullOrWhiteSpace(resourceId))
            {
                return false;
            }

            for (int index = 0; index < values.Count; index++)
            {
                if (string.Equals(
                        values[index]?.Trim(),
                        resourceId.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }

    [Serializable]
    public sealed class EconomyBuildingWarehousePolicy
    {
        public int Capacity = -1;
        public string[] AcceptedResourceIds =
            Array.Empty<string>();
    }

    [Serializable]
    public sealed class EconomyBuildingState
    {
        public string InstanceKey;
        public UnityEngine.Vector2Int GridPosition;
        public string BuildingId;
        public string ProductionProfileId;
        public string WorkerTypeId;
        public List<ProductionRecipeDefinition> ProductionRecipes =
            new List<ProductionRecipeDefinition>();
        public Dictionary<string, float> RecipeProgress =
            new Dictionary<string, float>(StringComparer.Ordinal);
        public int RequiredWorkers;
        public int AssignedWorkers;
        public int EconomyPriority;
        public bool IsActive = true;
        public float ProductionProgress;

        public bool IsFullyStaffed => AssignedWorkers >= RequiredWorkers;
    }
}
