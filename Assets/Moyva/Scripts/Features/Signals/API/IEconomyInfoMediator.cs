using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Signals
{
    public readonly struct EconomySettlementContext
    {
        public EconomySettlementContext(string settlementId, string settlementName, string ownerId)
        {
            SettlementId = settlementId;
            SettlementName = settlementName;
            OwnerId = ownerId;
        }

        public string SettlementId { get; }
        public string SettlementName { get; }
        public string OwnerId { get; }
    }

    public interface IEconomyInfoMediator
    {
        bool TryGetSettlementContext(Vector2Int position, out EconomySettlementContext context);
        bool TryGetBuildingContext(Vector2Int position, out string buildingId, out string ownerId);
        IReadOnlyDictionary<string, float> GetWarehouseResourceTotals(Vector2Int warehousePosition);
        IReadOnlyDictionary<string, float> GetSettlementWarehousesTotal(string settlementId);
        IReadOnlyDictionary<string, float> GetSettlementResourceTotals(string settlementId);
        IReadOnlyDictionary<string, float> GetOwnerResourceTotals(string ownerId);
    }
}