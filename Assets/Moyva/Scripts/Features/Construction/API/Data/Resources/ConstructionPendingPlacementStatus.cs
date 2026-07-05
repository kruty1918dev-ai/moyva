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
}
