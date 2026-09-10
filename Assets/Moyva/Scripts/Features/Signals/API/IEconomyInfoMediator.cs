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

    public readonly struct RecruitmentPopulationSnapshot
    {
        public RecruitmentPopulationSnapshot(int total, int available, int training, int military, float constructionSpeed)
        {
            Total = total; Available = available; Training = training; Military = military;
            ConstructionSpeed = constructionSpeed;
        }
        public int Total { get; }
        public int Available { get; }
        public int Training { get; }
        public int Military { get; }
        public float ConstructionSpeed { get; }
    }

    public struct CaravanDeliveryCompletedSignal
    {
        public string OwnerId;
        public string UnitId;
        public string SettlementId;
        public Vector2Int WarehousePosition;
        public IReadOnlyDictionary<string, float> Resources;
    }

    public struct SettlementPopulationChangedSignal
    {
        public string OwnerId;
        public string SettlementId;
    }

    public interface IEconomyInfoMediator
    {
        RecruitmentPopulationSnapshot GetRecruitmentPopulation(string ownerId, Vector2Int position);
        bool TryReserveRecruitmentPopulation(string ownerId, Vector2Int position, long queueId, int count, out string reason);
        void ReleaseRecruitmentPopulation(string ownerId, long queueId);
        void DeployRecruitmentPopulation(string ownerId, long queueId, string unitId);
        bool TryGetSettlementContext(Vector2Int position, out EconomySettlementContext context);
        bool TryResolveConstructionSettlement(Vector2Int position, string ownerId, out EconomySettlementContext context);
        bool TryGetBuildingContext(Vector2Int position, out string buildingId, out string ownerId);
        bool TryConsumeSettlementResources(string settlementId, IReadOnlyDictionary<string, float> resourceCosts, out string errorMessage);
        bool TryConsumeOwnerPoolResources(string ownerId, IReadOnlyDictionary<string, float> resourceCosts, out string errorMessage);
        void RefundOwnerPoolResources(string ownerId, IReadOnlyDictionary<string, float> resources);
        void RefundRecruitmentResources(string ownerId, string settlementId,
            IReadOnlyDictionary<string, float> resources);
        bool OwnerHasAnyWarehouse(string ownerId);
        IReadOnlyDictionary<string, float> GetWarehouseResourceTotals(Vector2Int warehousePosition);
        IReadOnlyDictionary<string, float> GetSettlementWarehousesTotal(string settlementId);
        IReadOnlyDictionary<string, float> GetSettlementResourceTotals(string settlementId);
        IReadOnlyDictionary<string, float> GetOwnerPoolResourceTotals(string ownerId);
        IReadOnlyDictionary<string, float> GetOwnerResourceTotals(string ownerId);
        string GetResourceDisplayName(string resourceId);
    }
}
