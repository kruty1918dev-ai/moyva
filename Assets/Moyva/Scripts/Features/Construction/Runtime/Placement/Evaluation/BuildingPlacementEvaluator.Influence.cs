using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public static partial class BuildingPlacementEvaluator
    {
        private static bool IsBlockedByInfluenceZone(BuildingPlacementEvaluationRequest request, BuildingPlacementEvaluationResult result)
        {
            if (string.IsNullOrWhiteSpace(request.BuildingId))
            {
                result?.AddNote("BuildingId порожній, influence-перевірку пропущено.");
                return false;
            }

            if (request.BuildingRegistry == null)
            {
                result?.AddNote("BuildingRegistry не заданий, influence-перевірку пропущено.");
                return false;
            }

            var candidate = request.BuildingRegistry.GetById(request.BuildingId);
            if (candidate == null)
            {
                result?.AddNote($"Будівлю '{request.BuildingId}' не знайдено у реєстрі, influence-перевірку пропущено.");
                return false;
            }

            bool hasInfluenceModule =
                BuildingDefinitionCapabilities.TryGetEnabledModule(
                    candidate,
                    out SettlementInfluenceRequirementBuildingModule
                        influenceModule);
            if (hasInfluenceModule
                && influenceModule.MergeMode
                    == PlacementRuleMergeMode.Disabled)
            {
                return false;
            }
            if (request.SkipInfluenceRules
                && (!hasInfluenceModule
                    || influenceModule.MergeMode
                        == PlacementRuleMergeMode.Inherit))
            {
                return false;
            }

            ResolveInfluencePolicy(
                candidate,
                hasInfluenceModule ? influenceModule : null,
                out bool requireInfluenceCenterInRange,
                out bool blockWhenInfluenceCenterExists);
            if (!requireInfluenceCenterInRange
                && !blockWhenInfluenceCenterExists)
            {
                return false;
            }

            bool hasAuthoritativeInfluenceRule =
                hasInfluenceModule
                && influenceModule.MergeMode
                    == PlacementRuleMergeMode.Override;
            if (!AnyInfluenceCenterDefined(request))
            {
                if (hasAuthoritativeInfluenceRule
                    && requireInfluenceCenterInRange)
                {
                    result?.AddBlocker(new BuildingPlacementBlocker
                    {
                        Kind = BuildingPlacementBlockerKind.InfluenceRequired,
                        Message = "Будівля потребує зони поселення, але у реєстрі немає жодного SettlementCenterBuildingModule.",
                        Position = request.Position,
                    });
                    return true;
                }

                result?.AddNote("У реєстрі немає центру поселення з SettlementCenterBuildingModule, тому legacy influence-правило вимкнене.");
                return false;
            }

            int ruleRadius = IsInfluenceCenter(candidate)
                ? ResolveInfluenceRadius(candidate, Mathf.Max(0, request.TownHallBuildRadius))
                : ResolveMaxInfluenceRadius(request);
            if (ruleRadius <= 0)
            {
                if (hasAuthoritativeInfluenceRule
                    && requireInfluenceCenterInRange)
                {
                    result?.AddBlocker(new BuildingPlacementBlocker
                    {
                        Kind = BuildingPlacementBlockerKind.InfluenceRequired,
                        Message = "Будівля потребує зони поселення, але жоден SettlementCenterBuildingModule не має додатного радіуса influence.",
                        Position = request.Position,
                    });
                    return true;
                }

                result?.AddNote("Радіус influence-правила дорівнює 0, перевірку пропущено.");
                return false;
            }

            bool hasInfluenceCenterInRange = HasInfluenceCenterCoveringPosition(request, request.Position, candidate, out var coveringCenter);
            if (requireInfluenceCenterInRange && !hasInfluenceCenterInRange)
            {
                result?.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.InfluenceRequired,
                    Message = $"Потрібен центр поселення у радіусі {ruleRadius}.",
                    Position = request.Position,
                    Radius = ruleRadius,
                });
                return true;
            }

            if (coveringCenter.HasValue)
            {
                result?.AddNote($"Позицію покриває центр '{coveringCenter.Value.BuildingId}' на {coveringCenter.Value.Position}.");
            }

            int candidateRadius = ResolveInfluenceRadius(
                candidate,
                Mathf.Max(0, request.TownHallBuildRadius));

            if (IsInfluenceCenter(candidate)
                && HasInfluenceCenterTooClose(
                    request,
                    request.Position,
                    candidate,
                    out var tooClose))
            {
                int minimumDistance =
                    BuildingDefinitionCapabilities
                        .GetMinimumSettlementCenterDistance(candidate);
                result?.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.InfluenceOverlap,
                    Message =
                        $"Центр поселення надто близько до " +
                        $"'{tooClose.BuildingId}' на {tooClose.Position}. " +
                        $"Мінімальна відстань: {minimumDistance}.",
                    Position = tooClose.Position,
                    BuildingId = tooClose.BuildingId,
                    Radius = minimumDistance,
                });
                return true;
            }

            if (blockWhenInfluenceCenterExists
                && HasOverlappingInfluenceCenter(request, request.Position, candidateRadius, out var overlap))
            {
                result?.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.InfluenceOverlap,
                    Message = $"Зона центру перетинається з '{overlap.BuildingId}' на {overlap.Position}.",
                    Position = overlap.Position,
                    BuildingId = overlap.BuildingId,
                    Radius = overlap.Radius,
                });
                return true;
            }

            return false;
        }

        private static void ResolveInfluencePolicy(
            BuildingDefinition candidate,
            SettlementInfluenceRequirementBuildingModule module,
            out bool requireInfluence,
            out bool blockOverlap)
        {
            if (module != null
                && module.MergeMode == PlacementRuleMergeMode.Override)
            {
                requireInfluence = module.RequiresInfluence;
                blockOverlap = module.BlockOverlappingCenters;
                return;
            }

            if (candidate?.PlacementRules != null)
            {
                requireInfluence = candidate.PlacementRules
                    .RequiresSettlementInfluence;
                blockOverlap = candidate.PlacementRules
                    .BlockIfSettlementCenterInRange;
                return;
            }

            if (candidate?.UseCustomTownHallRules == true)
            {
                requireInfluence = candidate.RequireTownHallInRange;
                blockOverlap =
                    candidate.BlockIfTownHallAlreadyInRange;
                return;
            }

            // Compatibility adapter for old runtime-only definitions. A legacy
            // center whose RequireTownHallInRange still has the field default
            // (true) was never authored with a usable center policy: requiring
            // another center would make the first one impossible to place.
            // Preserve the old center defaults in that sentinel case, while a
            // definition that explicitly turned the requirement off keeps its
            // authored overlap flag. New assets and explicit influence modules
            // returned above, so this branch cannot override authoritative data.
            bool candidateIsCenter = IsInfluenceCenter(candidate);
            if (candidateIsCenter)
            {
                requireInfluence = false;
                blockOverlap =
                    candidate?.RequireTownHallInRange == true
                    || candidate?.BlockIfTownHallAlreadyInRange == true;
                return;
            }

            requireInfluence = candidate?.RequireTownHallInRange == true;
            blockOverlap =
                candidate?.BlockIfTownHallAlreadyInRange == true;
        }

        private static bool AnyInfluenceCenterDefined(BuildingPlacementEvaluationRequest request)
        {
            if (request.HasInfluenceCenterDefinitions.HasValue)
                return request.HasInfluenceCenterDefinitions.Value;

            var definitions = request.BuildingRegistry.GetAll() ?? Array.Empty<BuildingDefinition>();
            for (int index = 0; index < definitions.Length; index++)
            {
                if (IsInfluenceCenter(definitions[index]))
                    return true;
            }

            return false;
        }

    }
}
