using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
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

    }
}
