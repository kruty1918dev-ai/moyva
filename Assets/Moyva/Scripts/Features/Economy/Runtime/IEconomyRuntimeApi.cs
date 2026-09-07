using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Economy.Runtime
{
    public readonly struct EconomyFormattedCategoryTotals
    {
        public EconomyFormattedCategoryTotals(string foodText, string materialsText, string moneyText)
        {
            FoodText = foodText;
            MaterialsText = materialsText;
            MoneyText = moneyText;
        }

        public string FoodText { get; }
        public string MaterialsText { get; }
        public string MoneyText { get; }
    }

    public readonly struct EconomyCategoryTotals
    {
        public EconomyCategoryTotals(float foodTotal, float materialsTotal, float moneyTotal)
        {
            FoodTotal = foodTotal;
            MaterialsTotal = materialsTotal;
            MoneyTotal = moneyTotal;
        }

        public float FoodTotal { get; }
        public float MaterialsTotal { get; }
        public float MoneyTotal { get; }
    }

    public readonly struct EconomyWarehouseSnapshot
    {
        public EconomyWarehouseSnapshot(
            string warehouseKey,
            string buildingId,
            string settlementId,
            string settlementName,
            Vector2Int gridPosition,
            float usedCapacity,
            int capacity,
            IReadOnlyDictionary<string, float> resources)
        {
            WarehouseKey = warehouseKey ?? string.Empty;
            BuildingId = buildingId ?? string.Empty;
            SettlementId = settlementId ?? string.Empty;
            SettlementName = settlementName ?? string.Empty;
            GridPosition = gridPosition;
            UsedCapacity = usedCapacity;
            Capacity = capacity;
            Resources = resources ?? new Dictionary<string, float>();
        }

        public string WarehouseKey { get; }
        public string BuildingId { get; }
        public string SettlementId { get; }
        public string SettlementName { get; }
        public Vector2Int GridPosition { get; }
        public float UsedCapacity { get; }
        public int Capacity { get; }
        public IReadOnlyDictionary<string, float> Resources { get; }
    }

    public readonly struct EconomySettlementSnapshot
    {
        public EconomySettlementSnapshot(
            string settlementId,
            string name,
            int population,
            int buildingCount,
            IReadOnlyDictionary<string, float> resources)
        {
            SettlementId = settlementId ?? string.Empty;
            Name = name ?? string.Empty;
            Population = population;
            BuildingCount = buildingCount;
            Resources = resources ?? new Dictionary<string, float>();
        }

        public string SettlementId { get; }
        public string Name { get; }
        public int Population { get; }
        public int BuildingCount { get; }
        public IReadOnlyDictionary<string, float> Resources { get; }
    }

    public readonly struct SettlementCaptureResult
    {
        public SettlementCaptureResult(
            bool succeeded,
            string settlementId,
            string previousOwnerId,
            string newOwnerId,
            string reason)
        {
            Succeeded = succeeded;
            SettlementId = settlementId ?? string.Empty;
            PreviousOwnerId = previousOwnerId ?? string.Empty;
            NewOwnerId = newOwnerId ?? string.Empty;
            Reason = reason ?? string.Empty;
        }

        public bool Succeeded { get; }
        public string SettlementId { get; }
        public string PreviousOwnerId { get; }
        public string NewOwnerId { get; }
        public string Reason { get; }

        public static SettlementCaptureResult Rejected(
            string settlementId,
            string reason)
            => new(false, settlementId, string.Empty, string.Empty, reason);
    }

    public interface ISettlementCaptureService
    {
        SettlementCaptureResult CaptureSettlement(
            string settlementId,
            string previousOwnerId,
            string newOwnerId,
            string reason = null);

        SettlementCaptureResult CaptureSettlementAtPosition(
            Vector2Int centerOrBuildingPosition,
            string previousOwnerId,
            string newOwnerId,
            string reason = null);
    }

    public interface ISettlementCaptureRemoteCommandRequester
    {
        event Action<SettlementCaptureRemoteResult> CaptureRejected;

        bool TryRequestCapture(
            string requesterOwnerId,
            string unitId,
            string targetEntityId,
            Vector2Int targetPosition,
            out string reason);
    }

    public readonly struct SettlementCaptureRemoteResult
    {
        public SettlementCaptureRemoteResult(
            string requesterOwnerId,
            string unitId,
            string targetEntityId,
            Vector2Int targetPosition,
            string requestId,
            string reason)
        {
            RequesterOwnerId = requesterOwnerId ?? string.Empty;
            UnitId = unitId ?? string.Empty;
            TargetEntityId = targetEntityId ?? string.Empty;
            TargetPosition = targetPosition;
            RequestId = requestId ?? string.Empty;
            Reason = reason ?? string.Empty;
        }

        public string RequesterOwnerId { get; }
        public string UnitId { get; }
        public string TargetEntityId { get; }
        public Vector2Int TargetPosition { get; }
        public string RequestId { get; }
        public string Reason { get; }
    }

    public interface IEconomyRuntimeApi
    {
        IReadOnlyList<string> GetSettlementIdsForOwner(string ownerId);
        EconomyCategoryTotals GetOwnerCategoryTotals(string ownerId);
        EconomyFormattedCategoryTotals GetFormattedOwnerCategoryTotals(string ownerId);
        Dictionary<string, float> GetOwnerResourceTotals(string ownerId);
        EconomyCategoryTotals GetSettlementCategoryTotals(string settlementId);
        EconomyFormattedCategoryTotals GetFormattedSettlementCategoryTotals(string settlementId);
        Dictionary<string, float> GetSettlementResourceTotals(string settlementId);
        IReadOnlyList<EconomyWarehouseSnapshot> GetOwnerWarehouseSnapshots(string ownerId);
        IReadOnlyList<EconomySettlementSnapshot> GetOwnerSettlementSnapshots(string ownerId);
    }
}
