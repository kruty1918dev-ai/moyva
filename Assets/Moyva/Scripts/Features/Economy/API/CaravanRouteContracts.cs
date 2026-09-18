using System.Collections.Generic;

namespace Kruty1918.Moyva.Economy.API
{
    public enum CaravanRoutePhase { ToSource, ToDestination, Completed }

    public readonly struct CaravanRouteRequest
    {
        public CaravanRouteRequest(string ownerId, string unitId, string sourceSettlementId, string sourceWarehouseKey,
            string targetSettlementId, string targetWarehouseKey, IReadOnlyDictionary<string, float> resources, bool repeat)
        {
            OwnerId = ownerId; UnitId = unitId;
            SourceSettlementId = sourceSettlementId; SourceWarehouseKey = sourceWarehouseKey;
            TargetSettlementId = targetSettlementId; TargetWarehouseKey = targetWarehouseKey;
            Resources = resources; Repeat = repeat;
        }

        public string OwnerId { get; }
        public string UnitId { get; }
        public string SourceSettlementId { get; }
        public string SourceWarehouseKey { get; }
        public string TargetSettlementId { get; }
        public string TargetWarehouseKey { get; }
        public IReadOnlyDictionary<string, float> Resources { get; }
        public bool Repeat { get; }
    }

    public readonly struct CaravanRouteSnapshot
    {
        public CaravanRouteSnapshot(CaravanRouteRequest request, CaravanRoutePhase phase, string status, bool moving)
        { Request = request; Phase = phase; Status = status; Moving = moving; }

        public CaravanRouteRequest Request { get; }
        public CaravanRoutePhase Phase { get; }
        public string Status { get; }
        public bool Moving { get; }
    }
}
