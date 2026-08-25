using Kruty1918.Moyva.Construction.API;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        private bool TryResolveSelectionFundingPosition(
            string ownerId,
            Vector2Int? preferredFundingPosition,
            out Vector2Int resolvedPosition)
        {
            if (ShouldUseOwnerPoolConstructionFunding(ownerId))
            {
                resolvedPosition = preferredFundingPosition.GetValueOrDefault();
                return true;
            }

            if (preferredFundingPosition.HasValue
                && !string.Equals(
                    ResolveResourceFundingContext(
                        preferredFundingPosition.Value,
                        ownerId),
                    "no-settlement",
                    StringComparison.Ordinal))
            {
                resolvedPosition = preferredFundingPosition.Value;
                return true;
            }

            for (int i = 0; i < _pendingPlacements.Count; i++)
            {
                Vector2Int candidate = _pendingPlacements[i].Position;
                if (!string.Equals(
                        ResolveResourceFundingContext(candidate, ownerId),
                        "no-settlement",
                        StringComparison.Ordinal))
                {
                    resolvedPosition = candidate;
                    return true;
                }
            }

            foreach (var pair in _factionPlacedBuildings)
            {
                if (!string.Equals(
                        NormalizeOwnerId(pair.Value.FactionId),
                        ownerId,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                if (!string.Equals(
                        ResolveResourceFundingContext(pair.Key, ownerId),
                        "no-settlement",
                        StringComparison.Ordinal))
                {
                    resolvedPosition = pair.Key;
                    return true;
                }
            }

            if (string.Equals(
                    NormalizeOwnerId(_activeOwnerId),
                    ownerId,
                    StringComparison.Ordinal))
            {
                foreach (var pair in _playerPlacedBuildings)
                {
                    if (!string.Equals(
                            ResolveResourceFundingContext(pair.Key, ownerId),
                            "no-settlement",
                            StringComparison.Ordinal))
                    {
                        resolvedPosition = pair.Key;
                        return true;
                    }
                }
            }

            resolvedPosition = default;
            return false;
        }

        private PlacementAvailabilityCacheValue
            ResolvePlacementAvailabilityCached(
                ConstructionPlacementQueryRequest request,
                string ownerId)
        {
            /*
             * Relocation queries can ignore an existing or pending origin,
             * which changes the per-owner limit result. Keep those queries
             * on the authoritative uncached path.
             */
            bool cacheable =
                Application.isPlaying
                && !request.IgnoredPendingPosition.HasValue
                && !request.IgnoredOccupiedPosition.HasValue;

            if (!cacheable)
            {
                return EvaluatePlacementAvailability(
                    request,
                    ownerId);
            }

            int frame =
                Time.frameCount;

            if (_placementAvailabilityCacheFrame
                != frame)
            {
                _placementAvailabilityCacheFrame =
                    frame;

                _placementAvailabilityCache.Clear();
            }

            string normalizedOwnerId =
                NormalizeOwnerId(ownerId);

            var key =
                new PlacementAvailabilityCacheKey(
                    request.BuildingId,
                    normalizedOwnerId,
                    request.IncludePendingPlacements,
                    request.IncludePendingPlacements
                        ? _pendingPlacementsVersion
                        : -1);

            if (_placementAvailabilityCache.TryGetValue(
                    key,
                    out PlacementAvailabilityCacheValue cached))
            {
                return cached;
            }

            PlacementAvailabilityCacheValue evaluated =
                EvaluatePlacementAvailability(
                    request,
                    normalizedOwnerId);

            _placementAvailabilityCache[key] =
                evaluated;

            return evaluated;
        }

        private PlacementAvailabilityCacheValue
            EvaluatePlacementAvailability(
                ConstructionPlacementQueryRequest request,
                string ownerId)
        {
            BuildingDefinition requestedDefinition =
                _placementBuildingRegistry?.GetById(
                    request.BuildingId);

            if (requestedDefinition == null)
            {
                return PlacementAvailabilityCacheValue.Invalid(
                    $"Building definition '{request.BuildingId}' is missing.",
                    "building-definition-missing",
                    BuildingPlacementBlockerKind.Configuration,
                    BuildingPerPlayerLimitEvaluation.Disabled);
            }

            if (RequiresInitialCastle(
                    ownerId,
                    out string requiredCastleId)
                && !BuildingDefinitionCapabilities.IsCastle(
                    requestedDefinition))
            {
                return PlacementAvailabilityCacheValue.Invalid(
                    $"Спочатку потрібно побудувати замок '{requiredCastleId}'.",
                    "initial-castle-required",
                    BuildingPlacementBlockerKind.Prerequisite,
                    BuildingPerPlayerLimitEvaluation.Disabled);
            }

            IReadOnlyList<BuildingValidationIssue> moduleIssues =
                GetModuleValidationIssuesCached(requestedDefinition);
            if (BuildingModuleValidation.HasErrors(moduleIssues))
            {
                string validationReason =
                    "Конфігурація модулів будівлі містить runtime-помилки.";
                for (int issueIndex = 0;
                     issueIndex < moduleIssues.Count;
                     issueIndex++)
                {
                    BuildingValidationIssue issue = moduleIssues[issueIndex];
                    if (issue != null
                        && issue.Severity == BuildingValidationSeverity.Error)
                    {
                        validationReason = issue.Message;
                        break;
                    }
                }

                return PlacementAvailabilityCacheValue.Invalid(
                    validationReason,
                    "module-validation-error",
                    BuildingPlacementBlockerKind.Configuration,
                    BuildingPerPlayerLimitEvaluation.Disabled);
            }

            if (!TryValidateBuildingPrerequisites(
                    requestedDefinition,
                    ownerId,
                    out string prerequisiteReason))
            {
                return PlacementAvailabilityCacheValue.Invalid(
                    prerequisiteReason,
                    "building-prerequisite",
                    BuildingPlacementBlockerKind.Prerequisite,
                    BuildingPerPlayerLimitEvaluation.Disabled);
            }

            if (!TryValidateModuleSingletonScopes(
                    requestedDefinition,
                    request,
                    ownerId,
                    out string singletonReason))
            {
                return PlacementAvailabilityCacheValue.Invalid(
                    singletonReason,
                    "module-singleton-scope",
                    BuildingPlacementBlockerKind.Configuration,
                    BuildingPerPlayerLimitEvaluation.Disabled);
            }

            if (!TryValidatePerPlayerBuildingLimit(
                    request,
                    ownerId,
                    out BuildingPerPlayerLimitEvaluation limitEvaluation))
            {
                return PlacementAvailabilityCacheValue.Invalid(
                    limitEvaluation.Reason,
                    "per-player-limit",
                    BuildingPlacementBlockerKind.Configuration,
                    limitEvaluation);
            }

            return PlacementAvailabilityCacheValue.Valid(
                limitEvaluation);
        }

    }
}
