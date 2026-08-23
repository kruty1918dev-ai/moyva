// AI-context consolidation: related partials live together by responsibility.
// No gameplay behavior is intentionally changed by this file organization.
using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;

// ---- Consolidated from ConstructionService.Resources.cs ----
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
        {
            string displayName = _economyInfoMediator?.GetResourceDisplayName(resourceId);
            if (!string.IsNullOrWhiteSpace(displayName)
                && !string.Equals(
                    displayName.Trim(),
                    resourceId?.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            {
                return displayName.Trim();
            }

            return "Ресурс";
        }

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

// ---- Consolidated from ConstructionService.PerPlayerLimits.cs ----
namespace Kruty1918.Moyva.Construction.Runtime
{
    internal readonly struct BuildingPerPlayerLimitEvaluation
    {
        public BuildingPerPlayerLimitEvaluation(
            int limit,
            int existingCount,
            int pendingCount,
            string reason)
        {
            Limit = Mathf.Max(0, limit);
            ExistingCount = Mathf.Max(0, existingCount);
            PendingCount = Mathf.Max(0, pendingCount);
            Reason = reason;
        }

        public int Limit { get; }
        public int ExistingCount { get; }
        public int PendingCount { get; }
        public int TotalCount => ExistingCount + PendingCount;
        public string Reason { get; }
        public bool IsEnabled => Limit > 0;
        public bool IsValid => !IsEnabled || TotalCount < Limit;

        public static BuildingPerPlayerLimitEvaluation Disabled
            => new BuildingPerPlayerLimitEvaluation(0, 0, 0, null);
    }

    internal sealed partial class ConstructionService
    {
        private bool TryValidateBuildingPrerequisites(
            BuildingDefinition candidate,
            string ownerId,
            out string reason)
        {
            reason = null;
            if (!BuildingDefinitionCapabilities.TryGetEnabledModule(
                    candidate,
                    out BuildingPrerequisiteModule module))
            {
                return true;
            }

            if (module.MergeMode != PlacementRuleMergeMode.Override)
                return true;

            int minimum = Mathf.Max(1, module.MinimumCount);
            var criteria = new System.Collections.Generic.List<string>();
            if (module.BuildingIds != null)
            {
                for (int index = 0;
                     index < module.BuildingIds.Length;
                     index++)
                {
                    string id = module.BuildingIds[index]?.Trim();
                    if (!string.IsNullOrWhiteSpace(id))
                        criteria.Add($"id:{id}");
                }
            }

            if (module.BuildingTags != null)
            {
                for (int index = 0;
                     index < module.BuildingTags.Length;
                     index++)
                {
                    string tag = module.BuildingTags[index]?.Trim();
                    if (!string.IsNullOrWhiteSpace(tag))
                        criteria.Add($"tag:{tag}");
                }
            }

            if (criteria.Count == 0)
                return true;

            string normalizedOwnerId = NormalizeOwnerId(ownerId);
            bool anySatisfied = false;
            for (int index = 0; index < criteria.Count; index++)
            {
                int count = CountOwnedBuildingsMatchingCriterion(
                    normalizedOwnerId,
                    criteria[index]);
                bool satisfied = count >= minimum;
                anySatisfied |= satisfied;
                if (module.MatchMode
                        == BuildingPrerequisiteMatchMode.All
                    && !satisfied)
                {
                    reason =
                        $"Потрібна побудована споруда '{criteria[index]}' у кількості {minimum}; знайдено {count}.";
                    return false;
                }
            }

            if (module.MatchMode == BuildingPrerequisiteMatchMode.Any
                && !anySatisfied)
            {
                reason =
                    $"Не виконано жодної передумови будівлі (потрібно щонайменше {minimum}).";
                return false;
            }

            return true;
        }

        private int CountOwnedBuildingsMatchingCriterion(
            string ownerId,
            string criterion)
        {
            bool isTag = criterion.StartsWith(
                "tag:",
                StringComparison.Ordinal);
            string value = criterion.Substring(
                criterion.IndexOf(':') + 1);
            int count = 0;

            foreach (var pair in _factionPlacedBuildings)
            {
                if (!string.Equals(
                        pair.Value.FactionId,
                        ownerId,
                        StringComparison.Ordinal)
                    || !BuildingMatchesCriterion(
                        pair.Value.BuildingId,
                        value,
                        isTag))
                {
                    continue;
                }

                count++;
            }

            if (!string.Equals(
                    ownerId,
                    NormalizeOwnerId(_activeOwnerId),
                    StringComparison.Ordinal))
            {
                return count;
            }

            foreach (var pair in _playerPlacedBuildings)
            {
                if (BuildingMatchesCriterion(
                        pair.Value,
                        value,
                        isTag))
                {
                    count++;
                }
            }

            return count;
        }

        private bool BuildingMatchesCriterion(
            string buildingId,
            string value,
            bool isTag)
        {
            if (!isTag)
            {
                return string.Equals(
                    buildingId,
                    value,
                    StringComparison.Ordinal);
            }

            BuildingDefinition definition =
                _placementBuildingRegistry?.GetById(buildingId);
            return ContainsTag(definition?.Tags, value)
                || ContainsTag(definition?.RuntimeTags, value);
        }

        private static bool ContainsTag(
            System.Collections.Generic.IReadOnlyList<string> tags,
            string value)
        {
            if (tags == null || string.IsNullOrWhiteSpace(value))
                return false;

            for (int index = 0; index < tags.Count; index++)
            {
                if (string.Equals(
                        tags[index]?.Trim(),
                        value.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryValidateModuleSingletonScopes(
            BuildingDefinition candidate,
            ConstructionPlacementQueryRequest request,
            string ownerId,
            out string reason)
        {
            reason = null;
            if (candidate?.Modules == null)
                return true;

            string normalizedOwner = NormalizeOwnerId(ownerId);
            string candidateSettlementId = null;
            bool hasCandidateSettlement =
                TryResolveConstructionSettlementId(
                    request.Position,
                    normalizedOwner,
                    out candidateSettlementId);

            for (int moduleIndex = 0;
                 moduleIndex < candidate.Modules.Count;
                 moduleIndex++)
            {
                BuildingModuleDefinition module =
                    candidate.Modules[moduleIndex];
                if (module == null || !module.IsEnabled)
                    continue;

                BuildingModuleScope scope = module.SingletonScope;
                if (scope != BuildingModuleScope.PerSettlement
                    && scope != BuildingModuleScope.Global)
                {
                    continue;
                }

                if (scope == BuildingModuleScope.PerSettlement
                    && !hasCandidateSettlement)
                {
                    // Economy may be intentionally absent in isolated tests.
                    // Fail open instead of producing a false global blocker.
                    continue;
                }

                Type moduleType = module.GetType();
                if (HasPlacedModuleScopeConflict(
                        moduleType,
                        scope,
                        normalizedOwner,
                        candidateSettlementId,
                        request)
                    || HasPendingModuleScopeConflict(
                        moduleType,
                        scope,
                        normalizedOwner,
                        candidateSettlementId,
                        request))
                {
                    string scopeName =
                        scope == BuildingModuleScope.Global
                            ? "глобально"
                            : $"у поселенні '{candidateSettlementId}'";
                    reason =
                        $"Модуль '{moduleType.Name}' дозволений лише один раз {scopeName}.";
                    return false;
                }
            }

            return true;
        }

        private bool HasPlacedModuleScopeConflict(
            Type moduleType,
            BuildingModuleScope scope,
            string ownerId,
            string candidateSettlementId,
            ConstructionPlacementQueryRequest request)
        {
            Vector2Int? ignoredOrigin =
                ResolveIgnoredOccupiedPosition(request);

            foreach (var pair in _factionPlacedBuildings)
            {
                if (ignoredOrigin.HasValue
                    && pair.Key == ignoredOrigin.Value)
                {
                    continue;
                }

                if (scope == BuildingModuleScope.PerSettlement
                    && !IsPositionInSettlementScope(
                        pair.Key,
                        pair.Value.FactionId,
                        candidateSettlementId))
                {
                    continue;
                }

                BuildingDefinition placed =
                    _placementBuildingRegistry?.GetById(
                        pair.Value.BuildingId);
                if (HasEnabledModuleType(placed, moduleType))
                    return true;
            }

            foreach (var pair in _playerPlacedBuildings)
            {
                if (ignoredOrigin.HasValue
                    && pair.Key == ignoredOrigin.Value)
                {
                    continue;
                }

                if (scope == BuildingModuleScope.PerSettlement)
                {
                    if (!string.Equals(
                            NormalizeOwnerId(_activeOwnerId),
                            ownerId,
                            StringComparison.Ordinal)
                        || !IsPositionInSettlementScope(
                            pair.Key,
                            ownerId,
                            candidateSettlementId))
                    {
                        continue;
                    }
                }

                BuildingDefinition placed =
                    _placementBuildingRegistry?.GetById(pair.Value);
                if (HasEnabledModuleType(placed, moduleType))
                    return true;
            }

            return false;
        }

        private bool HasPendingModuleScopeConflict(
            Type moduleType,
            BuildingModuleScope scope,
            string ownerId,
            string candidateSettlementId,
            ConstructionPlacementQueryRequest request)
        {
            if (scope == BuildingModuleScope.PerSettlement
                && !string.Equals(
                    NormalizeOwnerId(_activeOwnerId),
                    ownerId,
                    StringComparison.Ordinal))
            {
                return false;
            }

            for (int index = 0;
                 index < _pendingPlacements.Count;
                 index++)
            {
                PendingPlacement pending = _pendingPlacements[index];
                if (request.IgnoredPendingPosition.HasValue
                    && pending.Position
                        == request.IgnoredPendingPosition.Value)
                {
                    continue;
                }

                if (scope == BuildingModuleScope.PerSettlement
                    && !IsPositionInSettlementScope(
                        pending.Position,
                        ownerId,
                        candidateSettlementId))
                {
                    continue;
                }

                BuildingDefinition pendingDefinition =
                    _placementBuildingRegistry?.GetById(
                        pending.BuildingId);
                if (HasEnabledModuleType(
                        pendingDefinition,
                        moduleType))
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsPositionInSettlementScope(
            Vector2Int position,
            string ownerId,
            string candidateSettlementId)
        {
            return !string.IsNullOrWhiteSpace(candidateSettlementId)
                && TryResolveConstructionSettlementId(
                    position,
                    ownerId,
                    out string settlementId)
                && string.Equals(
                    settlementId,
                    candidateSettlementId,
                    StringComparison.Ordinal);
        }

        private bool TryResolveConstructionSettlementId(
            Vector2Int position,
            string ownerId,
            out string settlementId)
        {
            settlementId = null;
            if (_economyInfoMediator == null
                || !_economyInfoMediator.TryResolveConstructionSettlement(
                    position,
                    NormalizeOwnerId(ownerId),
                    out var context)
                || string.IsNullOrWhiteSpace(context.SettlementId))
            {
                return false;
            }

            settlementId = context.SettlementId;
            return true;
        }

        private static bool HasEnabledModuleType(
            BuildingDefinition definition,
            Type moduleType)
        {
            if (definition?.Modules == null || moduleType == null)
                return false;

            for (int index = 0;
                 index < definition.Modules.Count;
                 index++)
            {
                BuildingModuleDefinition module =
                    definition.Modules[index];
                if (module != null
                    && module.IsEnabled
                    && module.GetType() == moduleType)
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryValidatePerPlayerBuildingLimit(
            ConstructionPlacementQueryRequest request,
            string ownerId,
            out BuildingPerPlayerLimitEvaluation evaluation)
        {
            BuildingDefinition definition =
                _placementBuildingRegistry?.GetById(request.BuildingId);
            int limit = BuildingDefinitionCapabilities.GetMaxBuildingsPerPlayer(definition);
            if (limit <= 0)
            {
                evaluation = BuildingPerPlayerLimitEvaluation.Disabled;
                return true;
            }

            string normalizedOwnerId = NormalizeOwnerId(ownerId);
            Vector2Int? ignoredPlacedOrigin = ResolveIgnoredOccupiedPosition(request);
            bool strictPerOwner =
                BuildingDefinitionCapabilities
                    .IsStrictPerOwnerUnique(definition);
            BuildingLimitScope scope =
                strictPerOwner
                    ? BuildingLimitScope.PerOwner
                    : BuildingDefinitionCapabilities
                        .TryGetEnabledModule(
                            definition,
                            out BuildingPerPlayerLimitModule limitModule)
                        ? limitModule.LimitScope
                        : BuildingLimitScope.PerOwner;
            int existingCount = scope == BuildingLimitScope.Global
                ? CountPlacedBuildingsGlobally(
                    request.BuildingId,
                    ignoredPlacedOrigin)
                : CountPlacedBuildingsForOwner(
                    request.BuildingId,
                    normalizedOwnerId,
                    ignoredPlacedOrigin);
            int pendingCount = request.IncludePendingPlacements
                ? scope == BuildingLimitScope.Global
                    ? CountPendingBuildings(request)
                    : CountPendingBuildingsForOwner(
                        request,
                        normalizedOwnerId)
                : 0;
            int total = existingCount + pendingCount;
            string reason = total < limit
                ? null
                : scope == BuildingLimitScope.Global
                    ? $"Досягнуто глобального ліміту будівель: {total}/{limit}."
                    : $"Досягнуто ліміту будівель для гравця: {total}/{limit}.";

            evaluation = new BuildingPerPlayerLimitEvaluation(
                limit,
                existingCount,
                pendingCount,
                reason);
            return evaluation.IsValid;
        }

        private int CountPlacedBuildingsForOwner(
            string buildingId,
            string ownerId,
            Vector2Int? ignoredOrigin)
        {
            int count = 0;
            foreach (var pair in _factionPlacedBuildings)
            {
                if (ignoredOrigin.HasValue && pair.Key == ignoredOrigin.Value)
                    continue;

                if (string.Equals(
                        pair.Value.BuildingId,
                        buildingId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        pair.Value.FactionId,
                        ownerId,
                        StringComparison.Ordinal))
                {
                    count++;
                }
            }

            // Legacy/local placements do not store an owner separately. They belong to the
            // currently active owner and are used only outside faction-authoritative placement.
            if (!string.Equals(
                    ownerId,
                    NormalizeOwnerId(_activeOwnerId),
                    StringComparison.Ordinal))
            {
                return count;
            }

            foreach (var pair in _playerPlacedBuildings)
            {
                if (ignoredOrigin.HasValue && pair.Key == ignoredOrigin.Value)
                    continue;

                if (string.Equals(pair.Value, buildingId, StringComparison.Ordinal))
                    count++;
            }

            return count;
        }

        private int CountPlacedBuildingsGlobally(
            string buildingId,
            Vector2Int? ignoredOrigin)
        {
            int count = 0;
            foreach (var pair in _factionPlacedBuildings)
            {
                if (ignoredOrigin.HasValue
                    && pair.Key == ignoredOrigin.Value)
                {
                    continue;
                }

                if (string.Equals(
                        pair.Value.BuildingId,
                        buildingId,
                        StringComparison.Ordinal))
                {
                    count++;
                }
            }

            foreach (var pair in _playerPlacedBuildings)
            {
                if (ignoredOrigin.HasValue
                    && pair.Key == ignoredOrigin.Value)
                {
                    continue;
                }

                if (string.Equals(
                        pair.Value,
                        buildingId,
                        StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }

        private int CountPendingBuildingsForOwner(
            ConstructionPlacementQueryRequest request,
            string ownerId)
        {
            // Pending placements are local to the active construction owner. Network commands
            // use TryDirectPlace and therefore have no local pending state to count.
            if (!string.Equals(
                    ownerId,
                    NormalizeOwnerId(_activeOwnerId),
                    StringComparison.Ordinal))
            {
                return 0;
            }

            int count = 0;
            for (int index = 0; index < _pendingPlacements.Count; index++)
            {
                PendingPlacement placement = _pendingPlacements[index];
                if (request.IgnoredPendingPosition.HasValue
                    && placement.Position == request.IgnoredPendingPosition.Value)
                {
                    continue;
                }

                if (string.Equals(
                        placement.BuildingId,
                        request.BuildingId,
                        StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }

        private int CountPendingBuildings(
            ConstructionPlacementQueryRequest request)
        {
            int count = 0;
            for (int index = 0; index < _pendingPlacements.Count; index++)
            {
                PendingPlacement placement = _pendingPlacements[index];
                if (request.IgnoredPendingPosition.HasValue
                    && placement.Position
                        == request.IgnoredPendingPosition.Value)
                {
                    continue;
                }

                if (string.Equals(
                        placement.BuildingId,
                        request.BuildingId,
                        StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }
    }
}

// ---- Consolidated from ConstructionService.TurnAuthority.cs ----
namespace Kruty1918.Moyva.Construction.Runtime
{
    internal readonly struct ConstructionTurnAuthoritySnapshot
    {
        public ConstructionTurnAuthoritySnapshot(
            bool hasTurnAuthority,
            string activeOwnerId,
            string localOwnerId,
            TurnPhase phase)
        {
            HasTurnAuthority = hasTurnAuthority;
            ActiveOwnerId = activeOwnerId ?? string.Empty;
            LocalOwnerId = localOwnerId ?? string.Empty;
            Phase = phase;
        }

        public bool HasTurnAuthority { get; }
        public string ActiveOwnerId { get; }
        public string LocalOwnerId { get; }
        public TurnPhase Phase { get; }
    }

    internal static class ConstructionTurnAuthorityPolicy
    {
        public static bool TryAuthorize(
            string requestedOwnerId,
            in ConstructionTurnAuthoritySnapshot snapshot,
            bool requireLocalOwner,
            out string normalizedOwnerId,
            out string reason)
        {
            normalizedOwnerId = requestedOwnerId?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedOwnerId))
            {
                normalizedOwnerId = string.Empty;
                reason = "Construction command owner is empty.";
                return false;
            }

            if (!snapshot.HasTurnAuthority)
            {
                reason = "Turn authority is unavailable.";
                return false;
            }

            if (snapshot.Phase != TurnPhase.AwaitingInput)
            {
                reason =
                    $"Turn phase is {snapshot.Phase}, expected {TurnPhase.AwaitingInput}.";
                return false;
            }

            string activeOwnerId = snapshot.ActiveOwnerId?.Trim();
            if (string.IsNullOrWhiteSpace(activeOwnerId))
            {
                reason = "Active turn owner is empty.";
                return false;
            }

            if (!string.Equals(
                    activeOwnerId,
                    normalizedOwnerId,
                    StringComparison.Ordinal))
            {
                reason =
                    $"Owner '{normalizedOwnerId}' is not the active turn owner '{activeOwnerId}'.";
                return false;
            }

            if (requireLocalOwner)
            {
                string localOwnerId = snapshot.LocalOwnerId?.Trim();
                if (string.IsNullOrWhiteSpace(localOwnerId))
                {
                    reason = "Local turn owner is empty.";
                    return false;
                }

                if (!string.Equals(
                        localOwnerId,
                        normalizedOwnerId,
                        StringComparison.Ordinal))
                {
                    reason =
                        $"Owner '{normalizedOwnerId}' is not the local turn owner '{localOwnerId}'.";
                    return false;
                }
            }

            reason = null;
            return true;
        }
    }

    internal sealed partial class ConstructionService :
        IConfirmedConstructionDemolitionApplier
    {
        private bool TryAuthorizeConstructionMutation(
            string ownerId,
            string action,
            bool requireLocalOwner,
            out string normalizedOwnerId,
            out string reason)
        {
            var snapshot = new ConstructionTurnAuthoritySnapshot(
                _turns != null,
                _turns?.ActiveOwnerId,
                _turns?.LocalOwnerId,
                _turns?.Phase ?? TurnPhase.Initializing);

            if (!ConstructionTurnAuthorityPolicy.TryAuthorize(
                    ownerId,
                    snapshot,
                    requireLocalOwner,
                    out normalizedOwnerId,
                    out reason))
            {
                reason = $"{action}: {reason}";
                return false;
            }

            if (!_turns.CanOwnerAct(normalizedOwnerId, out reason))
            {
                reason = $"{action}: {reason}";
                return false;
            }

            return true;
        }

        private bool CanActiveOwnerMutate(
            string action,
            out string reason)
            => TryAuthorizeConstructionMutation(
                _activeOwnerId,
                action,
                requireLocalOwner: true,
                out _,
                out reason);

        private bool IsLocalConstructionOwner(string ownerId)
        {
            if (_turns == null || string.IsNullOrWhiteSpace(ownerId))
                return false;

            return string.Equals(
                _turns.LocalOwnerId?.Trim(),
                ownerId.Trim(),
                StringComparison.Ordinal);
        }

        private void RecordConstructionAction(
            string ownerId,
            string actionId)
        {
            if (_turns == null)
                return;

            string normalizedOwner = ownerId?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedOwner)
                || string.IsNullOrWhiteSpace(actionId))
            {
                return;
            }

            if (!_turns.TryRecordAction(normalizedOwner, actionId))
            {
                Debug.LogWarning(
                    $"[ConstructionTurnAuthority] Failed to record action '{actionId}' " +
                    $"for owner '{normalizedOwner}'.");
            }
        }

        private bool TryResolveCommittedBuildingForOwner(
            Vector2Int position,
            string ownerId,
            out Vector2Int origin,
            out string buildingId,
            out string reason)
        {
            origin = ResolvePlacedOrigin(position);
            buildingId = null;
            string normalizedOwner = ownerId?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedOwner))
            {
                reason = "Building owner is empty.";
                return false;
            }

            if (_factionPlacedBuildings.TryGetValue(
                    origin,
                    out var factionEntry))
            {
                string entryOwner =
                    NormalizeOwnerId(factionEntry.FactionId);
                if (!string.Equals(
                        entryOwner,
                        normalizedOwner,
                        StringComparison.Ordinal))
                {
                    reason =
                        $"Building at {origin} belongs to '{entryOwner}', not '{normalizedOwner}'.";
                    return false;
                }

                buildingId = factionEntry.BuildingId;
                reason = null;
                return !string.IsNullOrWhiteSpace(buildingId);
            }

            // Compatibility only for sessions created before P06. New commits and
            // restores are indexed in _factionPlacedBuildings with a stable owner.
            if (_playerPlacedBuildings.TryGetValue(
                    origin,
                    out string legacyBuildingId))
            {
                string legacyOwner = NormalizeOwnerId(_activeOwnerId);
                if (!string.Equals(
                        legacyOwner,
                        normalizedOwner,
                        StringComparison.Ordinal))
                {
                    reason =
                        $"Legacy building at {origin} is not owned by '{normalizedOwner}'.";
                    return false;
                }

                buildingId = legacyBuildingId;
                reason = null;
                return !string.IsNullOrWhiteSpace(buildingId);
            }

            reason = $"No committed building exists at {origin}.";
            return false;
        }

        private bool TryApplyCommittedDemolition(
            Vector2Int position,
            string ownerId,
            bool recordTurnAction,
            bool idempotentWhenMissing,
            out string reason)
        {
            if (!TryResolveCommittedBuildingForOwner(
                    position,
                    ownerId,
                    out Vector2Int origin,
                    out string buildingId,
                    out reason))
            {
                if (idempotentWhenMissing
                    && reason != null
                    && reason.StartsWith(
                        "No committed building exists",
                        StringComparison.Ordinal))
                {
                    reason = null;
                    return true;
                }

                return false;
            }

            string normalizedOwner = ownerId.Trim();
            UnregisterBuildingFootprint(origin, buildingId);
            RemovePlacedRecordAt(origin);
            InvalidatePlacementAvailabilityCache();
            _fogOfWarService?.UnregisterUnit(
                GetBuildingFogVisionAreaId(origin));

            _signalBus.Fire(new BuildingDemolishedSignal
            {
                BuildingId = buildingId,
                Position = origin,
                OwnerId = normalizedOwner,
                SourceFactionId = normalizedOwner,
            });

            if (recordTurnAction)
                RecordConstructionAction(normalizedOwner, "building-demolish");

            reason = null;
            return true;
        }

        public bool TryApplyConfirmedDemolition(
            Vector2Int position,
            string ownerId)
        {
            if (string.IsNullOrWhiteSpace(ownerId))
                return false;

            // Host-confirmed replication is a state-convergence path, not a local
            // gameplay command. It intentionally bypasses local turn authority.
            return TryApplyCommittedDemolition(
                position,
                ownerId.Trim(),
                recordTurnAction: false,
                idempotentWhenMissing: true,
                out _);
        }
    }
}
