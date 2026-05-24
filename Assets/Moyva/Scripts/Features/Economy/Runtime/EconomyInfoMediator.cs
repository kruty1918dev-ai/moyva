using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Economy.Runtime
{
    public sealed class EconomyInfoMediator : IEconomyInfoMediator
    {
        private static readonly IReadOnlyDictionary<string, float> EmptyResources =
            new Dictionary<string, float>(StringComparer.Ordinal);

        private readonly EconomyManager _economyManager;
        private readonly EconomyDatabaseSO _database;

        public EconomyInfoMediator(
            EconomyManager economyManager,
            [InjectOptional] EconomyDatabaseSO database)
        {
            _economyManager = economyManager;
            _database = database;
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

        public bool TryResolveConstructionSettlement(Vector2Int position, string ownerId, out EconomySettlementContext context)
        {
            context = default;
            if (_economyManager == null)
                return false;

            if (!_economyManager.TryResolveConstructionSettlement(position, ownerId, out var state) || state == null)
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

        public bool TryConsumeSettlementResources(string settlementId, IReadOnlyDictionary<string, float> resourceCosts, out string errorMessage)
        {
            errorMessage = null;
            return _economyManager != null
                && _economyManager.TryConsumeSettlementResources(settlementId, resourceCosts, out errorMessage);
        }

        public bool TryConsumeOwnerPoolResources(string ownerId, IReadOnlyDictionary<string, float> resourceCosts, out string errorMessage)
        {
            errorMessage = null;
            return _economyManager != null
                && _economyManager.TryConsumeOwnerPoolResources(ownerId, resourceCosts, out errorMessage);
        }

        public bool OwnerHasAnyWarehouse(string ownerId)
            => _economyManager != null && _economyManager.OwnerHasAnyWarehouse(ownerId);

        public IReadOnlyDictionary<string, float> GetWarehouseResourceTotals(Vector2Int warehousePosition)
            => _economyManager?.GetWarehouseResourceTotalsByPosition(warehousePosition) ?? EmptyResources;

        public IReadOnlyDictionary<string, float> GetSettlementWarehousesTotal(string settlementId)
            => _economyManager?.GetSettlementWarehousesTotal(settlementId) ?? EmptyResources;

        public IReadOnlyDictionary<string, float> GetSettlementResourceTotals(string settlementId)
            => _economyManager?.GetSettlementResourceTotals(settlementId) ?? EmptyResources;

        public IReadOnlyDictionary<string, float> GetOwnerPoolResourceTotals(string ownerId)
            => _economyManager?.GetOwnerPoolResourceTotals(ownerId) ?? EmptyResources;

        public IReadOnlyDictionary<string, float> GetOwnerResourceTotals(string ownerId)
            => _economyManager?.GetOwnerResourceTotals(ownerId) ?? EmptyResources;

        public string GetResourceDisplayName(string resourceId)
        {
            string fallback = string.IsNullOrWhiteSpace(resourceId) ? string.Empty : resourceId.Trim();
            if (_database?.Resources == null || string.IsNullOrWhiteSpace(fallback))
                return fallback;

            for (int i = 0; i < _database.Resources.Count; i++)
            {
                var resource = _database.Resources[i];
                if (resource == null || !string.Equals(resource.Id, fallback, StringComparison.Ordinal))
                    continue;

                return string.IsNullOrWhiteSpace(resource.DisplayName)
                    ? fallback
                    : resource.DisplayName;
            }

            return fallback;
        }
    }
}