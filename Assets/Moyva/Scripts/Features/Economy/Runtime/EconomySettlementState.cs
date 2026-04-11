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
        public bool IsActive = true;
        public int CurrentTurn;

        // Population
        public List<EconomyResidentState> Residents = new List<EconomyResidentState>();

        // Resources — shared pool per settlement (keyed by resource id)
        public Dictionary<string, float> ResourcePool = new Dictionary<string, float>(StringComparer.Ordinal);

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

        public void AddResource(string resourceId, float amount)
        {
            if (ResourcePool.ContainsKey(resourceId))
                ResourcePool[resourceId] += amount;
            else
                ResourcePool[resourceId] = amount;
        }

        public bool ConsumeResource(string resourceId, float amount)
        {
            if (!ResourcePool.TryGetValue(resourceId, out var current) || current < amount)
                return false;

            ResourcePool[resourceId] = current - amount;
            return true;
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
