using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Economy.API
{
    public enum ConstructionSupplyOrderStatus { Active = 0, Ready = 1, Cancelled = 2 }

    public readonly struct ConstructionSupplyResourceLine
    {
        public ConstructionSupplyResourceLine(string resourceId, float required,
            float localAvailable, float delivered, float deficit)
        {
            ResourceId = resourceId; Required = required; LocalAvailable = localAvailable;
            Delivered = delivered; Deficit = deficit;
        }

        public string ResourceId { get; }
        public float Required { get; }
        public float LocalAvailable { get; }
        public float Delivered { get; }
        public float Deficit { get; }
    }

    public readonly struct ConstructionSupplySourceSnapshot
    {
        public ConstructionSupplySourceSnapshot(string settlementId, string settlementName,
            string warehouseKey, IReadOnlyDictionary<string, float> available)
        {
            SettlementId = settlementId; SettlementName = settlementName;
            WarehouseKey = warehouseKey; Available = available;
        }

        public string SettlementId { get; }
        public string SettlementName { get; }
        public string WarehouseKey { get; }
        public IReadOnlyDictionary<string, float> Available { get; }
    }

    public readonly struct ConstructionSupplyWagonSnapshot
    {
        public ConstructionSupplyWagonSnapshot(string unitId, Vector2Int position,
            float capacity, float cargoUsed, bool busy, string status)
        {
            UnitId = unitId; Position = position; Capacity = capacity;
            CargoUsed = cargoUsed; Busy = busy; Status = status;
        }

        public string UnitId { get; }
        public Vector2Int Position { get; }
        public float Capacity { get; }
        public float CargoUsed { get; }
        public float FreeCapacity => Math.Max(0f, Capacity - CargoUsed);
        public bool Busy { get; }
        public string Status { get; }
    }

    public readonly struct ConstructionSupplyEvaluation
    {
        public ConstructionSupplyEvaluation(bool resolved, string settlementId,
            string settlementName, Vector2Int position, string reason,
            IReadOnlyList<ConstructionSupplyResourceLine> resources,
            IReadOnlyList<ConstructionSupplySourceSnapshot> sources,
            IReadOnlyList<ConstructionSupplyWagonSnapshot> wagons)
        {
            Resolved = resolved; SettlementId = settlementId; SettlementName = settlementName;
            Position = position; Reason = reason;
            Resources = resources; Sources = sources; Wagons = wagons;
        }

        public bool Resolved { get; }
        public string SettlementId { get; }
        public string SettlementName { get; }
        public Vector2Int Position { get; }
        public string Reason { get; }
        public IReadOnlyList<ConstructionSupplyResourceLine> Resources { get; }
        public IReadOnlyList<ConstructionSupplySourceSnapshot> Sources { get; }
        public IReadOnlyList<ConstructionSupplyWagonSnapshot> Wagons { get; }
        public bool HasDeficit { get { if (Resources == null) return false; for (int i = 0; i < Resources.Count; i++) if (Resources[i].Deficit > 0.0001f) return true; return false; } }
    }

    public readonly struct ConstructionSupplyDispatchRequest
    {
        public ConstructionSupplyDispatchRequest(string ownerId, string buildingId,
            Vector2Int position, string sourceSettlementId, string sourceWarehouseKey, string unitId)
        {
            OwnerId = ownerId; BuildingId = buildingId; Position = position;
            SourceSettlementId = sourceSettlementId; SourceWarehouseKey = sourceWarehouseKey;
            UnitId = unitId;
        }

        public string OwnerId { get; }
        public string BuildingId { get; }
        public Vector2Int Position { get; }
        public string SourceSettlementId { get; }
        public string SourceWarehouseKey { get; }
        public string UnitId { get; }
    }

    public readonly struct ConstructionSupplyOrderSnapshot
    {
        public ConstructionSupplyOrderSnapshot(string orderId, string ownerId, string buildingId,
            Vector2Int position, string settlementId, string settlementName,
            ConstructionSupplyOrderStatus status,
            IReadOnlyDictionary<string, float> required,
            IReadOnlyDictionary<string, float> delivered,
            IReadOnlyDictionary<string, float> remaining,
            IReadOnlyList<string> wagonIds)
        {
            OrderId = orderId; OwnerId = ownerId; BuildingId = buildingId; Position = position;
            SettlementId = settlementId; SettlementName = settlementName; Status = status;
            Required = required; Delivered = delivered; Remaining = remaining; WagonIds = wagonIds;
        }

        public string OrderId { get; }
        public string OwnerId { get; }
        public string BuildingId { get; }
        public Vector2Int Position { get; }
        public string SettlementId { get; }
        public string SettlementName { get; }
        public ConstructionSupplyOrderStatus Status { get; }
        public IReadOnlyDictionary<string, float> Required { get; }
        public IReadOnlyDictionary<string, float> Delivered { get; }
        public IReadOnlyDictionary<string, float> Remaining { get; }
        public IReadOnlyList<string> WagonIds { get; }
    }

    /// <summary>
    /// Settlement-local construction supply. Tracks deficits for pending
    /// placements, dispatches real wagon routes toward them and reserves
    /// delivered stock so construction cannot double-spend it.
    /// </summary>
    public interface IConstructionSupplyService
    {
        event Action Changed;

        ConstructionSupplyEvaluation Evaluate(string ownerId, string buildingId,
            Vector2Int position, IReadOnlyDictionary<string, float> requiredCosts);

        CaravanTransferResult DispatchSupply(ConstructionSupplyDispatchRequest request,
            IReadOnlyDictionary<string, float> requiredCosts);

        IReadOnlyList<ConstructionSupplyOrderSnapshot> GetOrders(string ownerId);

        bool TryGetOrderAt(Vector2Int position, out ConstructionSupplyOrderSnapshot snapshot);

        /// <summary>Resources spendable by the placement at this position
        /// (pool − other reservations + this order's delivered stock).</summary>
        IReadOnlyDictionary<string, float> GetResourcesForPlacement(
            string settlementId, Vector2Int position);

        void CancelOrderAt(Vector2Int position);

        /// <summary>Applies a host-confirmed supply dispatch on a non-authoritative peer.</summary>
        CaravanTransferResult ApplyConfirmedDispatch(ConstructionSupplyDispatchRequest request,
            IReadOnlyDictionary<string, float> requiredCosts);

        /// <summary>Applies a host-confirmed supply order close on a non-authoritative peer.</summary>
        void ApplyConfirmedCancelOrder(string ownerId, string unitId, Vector2Int position);
    }
}
