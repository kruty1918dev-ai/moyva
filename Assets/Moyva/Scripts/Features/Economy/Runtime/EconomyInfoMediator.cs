using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Economy.Runtime
{
    public sealed class EconomyInfoMediator : IEconomyInfoMediator
    {
        private static readonly IReadOnlyDictionary<string, float> EmptyResources =
            new Dictionary<string, float>(StringComparer.Ordinal);

        private readonly EconomyManager _economyManager;

        public EconomyInfoMediator(EconomyManager economyManager)
        {
            _economyManager = economyManager;
        }

        public bool TryGetSettlementContext(Vector2Int position, out EconomySettlementContext context)
        {
            context = default;
            if (_economyManager == null)
                return false;

            if (!_economyManager.TryGetSettlementByPosition(position, out var state) || state == null)
                return false;

            context = new EconomySettlementContext(
                state.SettlementId,
                _economyManager.GetSettlementNameOrFallback(state.SettlementId),
                state.OwnerId);
            return true;
        }

        public bool TryGetBuildingContext(Vector2Int position, out string buildingId, out string ownerId)
        {
            buildingId = null;
            ownerId = null;

            if (_economyManager == null)
                return false;

            return _economyManager.TryGetBuildingAtPosition(position, out buildingId, out ownerId);
        }

        public IReadOnlyDictionary<string, float> GetWarehouseResourceTotals(Vector2Int warehousePosition)
            => _economyManager?.GetWarehouseResourceTotalsByPosition(warehousePosition) ?? EmptyResources;

        public IReadOnlyDictionary<string, float> GetSettlementWarehousesTotal(string settlementId)
            => _economyManager?.GetSettlementWarehousesTotal(settlementId) ?? EmptyResources;

        public IReadOnlyDictionary<string, float> GetSettlementResourceTotals(string settlementId)
            => _economyManager?.GetSettlementResourceTotals(settlementId) ?? EmptyResources;

        public IReadOnlyDictionary<string, float> GetOwnerResourceTotals(string ownerId)
            => _economyManager?.GetOwnerResourceTotals(ownerId) ?? EmptyResources;
    }
}