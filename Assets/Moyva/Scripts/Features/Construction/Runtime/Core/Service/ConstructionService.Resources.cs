using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        public bool TryGetPendingPlacementStatus(Vector2Int position, out ConstructionPendingPlacementStatus status)
        {
            int index = FindPendingPlacementIndex(position);
            if (index < 0)
            {
                status = default;
                return false;
            }

            var placement = _pendingPlacements[index];
            if (IsRelocation(placement))
            {
                status = new ConstructionPendingPlacementStatus(
                    position: position,
                    buildingId: placement.BuildingId,
                    settlementId: "Relocation",
                    settlementName: "Relocation",
                    hasSettlement: true,
                    isAffordable: true,
                    errorMessage: string.Empty);
                return true;
            }

            var projection = BuildResourceProjectionForPlacement(
                placement.Position,
                placement.BuildingId,
                _activeOwnerId,
                ignoredPendingPosition: placement.Position);

            status = new ConstructionPendingPlacementStatus(
                position: position,
                buildingId: placement.BuildingId,
                settlementId: projection.SettlementId ?? "Unknown",
                settlementName: projection.SettlementName ?? "Unknown",
                hasSettlement: projection.HasSettlement,
                isAffordable: !projection.HasDeficit,
                errorMessage: projection.HasDeficit ? projection.Message : string.Empty
            );

            return true;
        }

        public ConstructionResourceProjection GetResourceProjection(Vector2Int position)
        {
            if (!HasPendingPlacementAt(position))
                return ConstructionResourceProjection.Empty;

            try
            {
                if (!TryGetPendingBuildingIdAt(position, out var buildingId))
                    return ConstructionResourceProjection.Empty;

                int pendingIndex = FindPendingPlacementIndex(position);
                if (pendingIndex >= 0 && IsRelocation(_pendingPlacements[pendingIndex]))
                    return ConstructionResourceProjection.Empty;

                return BuildResourceProjectionForPlacement(position, buildingId, _activeOwnerId, ignoredPendingPosition: position);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Construction] GetResourceProjection error: {ex.Message}");
                return ConstructionResourceProjection.Empty;
            }
        }

        public string GetLastActionMessage()
        {
            return _lastActionMessage;
        }

        private bool TryValidateConstructionResources(
            Vector2Int position,
            string buildingId,
            string ownerId,
            Vector2Int? ignoredPendingPosition,
            out string reason,
            bool includePendingPlacements = true)
        {
            var projection = BuildResourceProjectionForPlacement(
                position,
                buildingId,
                ownerId,
                ignoredPendingPosition,
                includePendingPlacements);
            if (!projection.HasDeficit)
            {
                reason = null;
                return true;
            }

            reason = projection.Message;
            return false;
        }

        private bool TryConsumeConstructionResources(
            Vector2Int position,
            string buildingId,
            string ownerId,
            out string reason)
        {
            reason = null;
            string normalizedOwnerId = NormalizeOwnerId(ownerId);

            var costs = BuildConstructionCostMap(buildingId);
            if (costs.Count == 0)
                return true;

            if (_economyInfoMediator == null)
            {
                reason = "Економіка не підключена: неможливо перевірити ресурси для будівництва.";
                return false;
            }

            if (ShouldUseOwnerPoolConstructionFunding(normalizedOwnerId))
            {
                if (!_economyInfoMediator.TryConsumeOwnerPoolResources(normalizedOwnerId, costs, out reason))
                    return false;

                InvalidatePlacementResourceValidationCache();
                reason = null;
                return true;
            }

            if (!_economyInfoMediator.TryResolveConstructionSettlement(position, normalizedOwnerId, out var settlement)
                || string.IsNullOrWhiteSpace(settlement.SettlementId))
            {
                reason = "Не знайдено поселення/замок для списання ресурсів у цій зоні будівництва.";
                return false;
            }

            if (!_economyInfoMediator.TryConsumeSettlementResources(settlement.SettlementId, costs, out reason))
                return false;

            InvalidatePlacementResourceValidationCache();
            reason = null;
            return true;
        }

        private ConstructionResourceProjection BuildResourceProjectionForPlacement(
            Vector2Int position,
            string buildingId,
            string ownerId,
            Vector2Int? ignoredPendingPosition,
            bool includePendingPlacements = true)
        {
            string normalizedOwnerId = NormalizeOwnerId(ownerId);
            var costs = BuildConstructionCostMap(buildingId);
            if (costs.Count == 0)
            {
                return new ConstructionResourceProjection(
                    normalizedOwnerId,
                    null,
                    null,
                    hasSettlement: false,
                    hasDeficit: false,
                    message: string.Empty,
                    balances: new List<ConstructionResourceBalance>());
            }

            if (_economyInfoMediator == null)
            {
                return new ConstructionResourceProjection(
                    normalizedOwnerId,
                    null,
                    null,
                    hasSettlement: false,
                    hasDeficit: true,
                    message: "Економіка не підключена: неможливо перевірити ресурси для будівництва.",
                    balances: new List<ConstructionResourceBalance>());
            }

            bool hasSettlement = _economyInfoMediator.TryResolveConstructionSettlement(position, normalizedOwnerId, out var settlement)
                && !string.IsNullOrWhiteSpace(settlement.SettlementId);

            if (ShouldUseOwnerPoolConstructionFunding(normalizedOwnerId))
            {
                var ownerPoolAvailable = _economyInfoMediator.GetOwnerPoolResourceTotals(normalizedOwnerId);
                var ownerPoolReserved = includePendingPlacements
                    ? BuildReservedOwnerPoolCosts(normalizedOwnerId, ignoredPendingPosition)
                    : new Dictionary<string, float>(StringComparer.Ordinal);
                AddCosts(ownerPoolReserved, costs);

                var ownerPoolBalances = new List<ConstructionResourceBalance>(ownerPoolReserved.Count);
                bool ownerPoolHasDeficit = false;
                string ownerPoolDeficitMessage = string.Empty;

                foreach (var pair in ownerPoolReserved)
                {
                    float availableAmount = ownerPoolAvailable != null && ownerPoolAvailable.TryGetValue(pair.Key, out var value)
                        ? value
                        : 0f;
                    var balance = new ConstructionResourceBalance(pair.Key, availableAmount, pair.Value);
                    ownerPoolBalances.Add(balance);
                    if (balance.IsDeficit && string.IsNullOrEmpty(ownerPoolDeficitMessage))
                    {
                        ownerPoolHasDeficit = true;
                        ownerPoolDeficitMessage = $"Недостатньо ресурсу '{ResolveResourceDisplayName(pair.Key)}' у стартовому запасі власника '{normalizedOwnerId}': потрібно {pair.Value:0.#}, доступно {availableAmount:0.#}.";
                    }
                }

                ownerPoolBalances.Sort((left, right) => string.CompareOrdinal(left.ResourceId, right.ResourceId));

                return new ConstructionResourceProjection(
                    normalizedOwnerId,
                    hasSettlement ? settlement.SettlementId : null,
                    hasSettlement ? settlement.SettlementName : null,
                    hasSettlement: hasSettlement,
                    hasDeficit: ownerPoolHasDeficit,
                    message: ownerPoolHasDeficit ? ownerPoolDeficitMessage : string.Empty,
                    balances: ownerPoolBalances);
            }

            if (!hasSettlement)
            {
                return new ConstructionResourceProjection(
                    normalizedOwnerId,
                    null,
                    null,
                    hasSettlement: false,
                    hasDeficit: true,
                    message: "Не знайдено поселення/замок для ресурсів у цій зоні будівництва.",
                    balances: new List<ConstructionResourceBalance>());
            }

            var available = _economyInfoMediator.GetSettlementResourceTotals(settlement.SettlementId);
            var reserved = includePendingPlacements
                ? BuildReservedConstructionCosts(settlement.SettlementId, ownerId, ignoredPendingPosition)
                : new Dictionary<string, float>(StringComparer.Ordinal);
            AddCosts(reserved, costs);

            var balances = new List<ConstructionResourceBalance>(reserved.Count);
            bool hasDeficit = false;
            string deficitMessage = string.Empty;

            foreach (var pair in reserved)
            {
                float availableAmount = available != null && available.TryGetValue(pair.Key, out var value)
                    ? value
                    : 0f;
                var balance = new ConstructionResourceBalance(pair.Key, availableAmount, pair.Value);
                balances.Add(balance);
                if (balance.IsDeficit && string.IsNullOrEmpty(deficitMessage))
                {
                    hasDeficit = true;
                    deficitMessage = $"Недостатньо ресурсу '{ResolveResourceDisplayName(pair.Key)}' у поселенні '{settlement.SettlementName}': потрібно {pair.Value:0.#}, доступно {availableAmount:0.#}.";
                }
            }

            balances.Sort((left, right) => string.CompareOrdinal(left.ResourceId, right.ResourceId));

            return new ConstructionResourceProjection(
                settlement.OwnerId,
                settlement.SettlementId,
                settlement.SettlementName,
                hasSettlement: true,
                hasDeficit: hasDeficit,
                message: hasDeficit ? deficitMessage : string.Empty,
                balances: balances);
        }

        private Dictionary<string, float> BuildReservedOwnerPoolCosts(
            string ownerId,
            Vector2Int? ignoredPendingPosition)
        {
            var reserved = new Dictionary<string, float>(StringComparer.Ordinal);
            if (!ShouldUseOwnerPoolConstructionFunding(ownerId))
                return reserved;

            for (int i = 0; i < _pendingPlacements.Count; i++)
            {
                var placement = _pendingPlacements[i];
                if (ignoredPendingPosition.HasValue && placement.Position == ignoredPendingPosition.Value)
                    continue;
                if (IsRelocation(placement))
                    continue;

                AddCosts(reserved, BuildConstructionCostMap(placement.BuildingId));
            }

            return reserved;
        }

        private Dictionary<string, float> BuildReservedConstructionCosts(
            string settlementId,
            string ownerId,
            Vector2Int? ignoredPendingPosition)
        {
            var reserved = new Dictionary<string, float>(StringComparer.Ordinal);
            if (string.IsNullOrWhiteSpace(settlementId) || _economyInfoMediator == null)
                return reserved;

            for (int i = 0; i < _pendingPlacements.Count; i++)
            {
                var placement = _pendingPlacements[i];
                if (ignoredPendingPosition.HasValue && placement.Position == ignoredPendingPosition.Value)
                    continue;
                if (IsRelocation(placement))
                    continue;

                if (!_economyInfoMediator.TryResolveConstructionSettlement(placement.Position, ownerId, out var pendingSettlement)
                    || !string.Equals(pendingSettlement.SettlementId, settlementId, StringComparison.Ordinal))
                {
                    continue;
                }

                AddCosts(reserved, BuildConstructionCostMap(placement.BuildingId));
            }

            return reserved;
        }

        private bool ShouldUseOwnerPoolConstructionFunding(string ownerId)
        {
            return _economyInfoMediator != null
                && !_economyInfoMediator.OwnerHasAnyWarehouse(NormalizeOwnerId(ownerId));
        }

        private static string NormalizeOwnerId(string ownerId)
        {
            return string.IsNullOrWhiteSpace(ownerId) ? DefaultOwnerId : ownerId.Trim();
        }

        private string ResolveResourceDisplayName(string resourceId)
            => _economyInfoMediator?.GetResourceDisplayName(resourceId)
               ?? (string.IsNullOrWhiteSpace(resourceId) ? string.Empty : resourceId.Trim());

        private Dictionary<string, float> BuildConstructionCostMap(string buildingId)
        {
            var result = new Dictionary<string, float>(StringComparer.Ordinal);
            var definition = string.IsNullOrWhiteSpace(buildingId)
                ? null
                : _buildingRegistry?.GetById(buildingId);

            var costs = BuildingDefinitionCapabilities.GetConstructionCost(definition);
            for (int i = 0; i < costs.Count; i++)
            {
                var entry = costs[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.ResourceId) || entry.Amount <= 0)
                    continue;

                AddCost(result, entry.ResourceId.Trim(), entry.Amount);
            }

            return result;
        }

        private static void AddCosts(Dictionary<string, float> target, IReadOnlyDictionary<string, float> source)
        {
            if (target == null || source == null)
                return;

            foreach (var pair in source)
                AddCost(target, pair.Key, pair.Value);
        }

        private static void AddCost(Dictionary<string, float> target, string resourceId, float amount)
        {
            if (target == null || string.IsNullOrWhiteSpace(resourceId) || amount <= 0f)
                return;

            string normalizedId = resourceId.Trim();
            if (target.ContainsKey(normalizedId))
                target[normalizedId] += amount;
            else
                target[normalizedId] = amount;
        }
    }
}
