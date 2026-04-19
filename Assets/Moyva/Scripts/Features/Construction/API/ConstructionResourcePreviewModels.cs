using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public readonly struct ConstructionPendingPlacementStatus
    {
        public ConstructionPendingPlacementStatus(
            Vector2Int position,
            string buildingId,
            string settlementId,
            string settlementName,
            bool hasSettlement,
            bool isAffordable,
            string errorMessage)
        {
            Position = position;
            BuildingId = buildingId;
            SettlementId = settlementId;
            SettlementName = settlementName;
            HasSettlement = hasSettlement;
            IsAffordable = isAffordable;
            ErrorMessage = errorMessage;
        }

        public Vector2Int Position { get; }
        public string BuildingId { get; }
        public string SettlementId { get; }
        public string SettlementName { get; }
        public bool HasSettlement { get; }
        public bool IsAffordable { get; }
        public string ErrorMessage { get; }
    }

    public readonly struct ConstructionResourceBalance
    {
        public ConstructionResourceBalance(string resourceId, float available, float reserved)
        {
            ResourceId = resourceId;
            Available = available;
            Reserved = reserved;
            Remaining = available - reserved;
            IsDeficit = Remaining < -0.0001f;
        }

        public string ResourceId { get; }
        public float Available { get; }
        public float Reserved { get; }
        public float Remaining { get; }
        public bool IsDeficit { get; }
    }

    public sealed class ConstructionResourceProjection
    {
        public static ConstructionResourceProjection Empty { get; } =
            new ConstructionResourceProjection(null, null, null, false, false, string.Empty, new List<ConstructionResourceBalance>());

        public ConstructionResourceProjection(
            string ownerId,
            string settlementId,
            string settlementName,
            bool hasSettlement,
            bool hasDeficit,
            string message,
            IReadOnlyList<ConstructionResourceBalance> balances)
        {
            OwnerId = ownerId;
            SettlementId = settlementId;
            SettlementName = settlementName;
            HasSettlement = hasSettlement;
            HasDeficit = hasDeficit;
            Message = message;
            Balances = balances ?? new List<ConstructionResourceBalance>();
        }

        public string OwnerId { get; }
        public string SettlementId { get; }
        public string SettlementName { get; }
        public bool HasSettlement { get; }
        public bool HasDeficit { get; }
        public string Message { get; }
        public IReadOnlyList<ConstructionResourceBalance> Balances { get; }
    }
}
