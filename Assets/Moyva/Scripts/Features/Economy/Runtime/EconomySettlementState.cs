using System;
using System.Collections.Generic;

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

        // Worker assignments — keyed by building id, value = assigned workers count
        public Dictionary<string, int> WorkerAssignments = new Dictionary<string, int>(StringComparer.Ordinal);

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

            var targetWarehouse = ResolveWarehouseKey(warehouseKey);
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

            string defaultWarehouse = ResolveWarehouseKey(null);
            if (string.IsNullOrEmpty(defaultWarehouse))
                return;

            EnsureWarehousePool(defaultWarehouse);
            var defaultPool = WarehouseResourcePools[defaultWarehouse];

            foreach (var resource in ResourcePool)
            {
                float inWarehouses = 0f;
                foreach (var warehouse in WarehouseResourcePools)
                {
                    if (warehouse.Value != null && warehouse.Value.TryGetValue(resource.Key, out var amount))
                        inWarehouses += amount;
                }

                float delta = resource.Value - inWarehouses;
                if (Math.Abs(delta) <= 0.0001f)
                    continue;

                if (defaultPool.ContainsKey(resource.Key))
                    defaultPool[resource.Key] += delta;
                else
                    defaultPool[resource.Key] = delta;

                if (defaultPool[resource.Key] < 0f)
                    defaultPool[resource.Key] = 0f;
            }
        }

        private string ResolveWarehouseKey(string preferred)
        {
            if (!string.IsNullOrWhiteSpace(preferred) && WarehouseResourcePools.ContainsKey(preferred))
                return preferred;

            if (!string.IsNullOrWhiteSpace(preferred) && !WarehouseResourcePools.ContainsKey(preferred))
                return preferred;

            string first = null;
            foreach (var key in WarehouseResourcePools.Keys)
            {
                if (first == null || string.CompareOrdinal(key, first) < 0)
                    first = key;
            }

            return first;
        }
    }

    [Serializable]
    public sealed class EconomyBuildingState
    {
        public string BuildingId;
        public string ProductionProfileId;
        public int RequiredWorkers;
        public int AssignedWorkers;
        public int EconomyPriority;
        public bool IsActive = true;
        public float ProductionProgress;

        public bool IsFullyStaffed => AssignedWorkers >= RequiredWorkers;
    }
}
