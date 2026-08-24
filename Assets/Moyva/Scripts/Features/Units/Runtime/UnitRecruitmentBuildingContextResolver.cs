using System;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;

namespace Kruty1918.Moyva.Units.Runtime
{
    /// <summary>
    /// Resolves construction/building context required by recruitment.
    /// This component validates identity/configuration only and never mutates
    /// recruitment queues, economy state or units.
    /// </summary>
    internal sealed class UnitRecruitmentBuildingContextResolver
    {
        private readonly IUnitClassConfig _unitClassConfig;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IConstructionSaveSnapshotSource _constructionSnapshot;
        private readonly IConstructionLifecycle _constructionLifecycle;

        public UnitRecruitmentBuildingContextResolver(
            IUnitClassConfig unitClassConfig,
            IBuildingRegistry buildingRegistry,
            IConstructionSaveSnapshotSource constructionSnapshot,
            IConstructionLifecycle constructionLifecycle)
        {
            _unitClassConfig = unitClassConfig;
            _buildingRegistry = buildingRegistry;
            _constructionSnapshot = constructionSnapshot;
            _constructionLifecycle = constructionLifecycle;
        }

        public bool TryResolveEnqueue(
            Vector2Int position,
            string ownerId,
            string unitTypeId,
            out string buildingId,
            out UnitRecruitmentBuildingModule module,
            out UnitRecruitmentRecipeDefinition recipe,
            out string reason)
        {
            buildingId = null;
            module = null;
            recipe = null;
            reason = null;

            if (_constructionSnapshot == null
                || _buildingRegistry == null
                || _constructionLifecycle == null)
            {
                reason = "Construction recruitment context is unavailable.";
                return false;
            }

            if (!TryFindPlacement(
                    position,
                    out ConstructionSavedPlacement placement))
            {
                reason =
                    "Recruiting building is not a committed construction placement.";
                return false;
            }

            if (!string.Equals(
                    NormalizeRequiredId(placement.OwnerId),
                    ownerId,
                    StringComparison.Ordinal))
            {
                reason = "Recruiting building belongs to another owner.";
                return false;
            }

            if (!_constructionLifecycle.IsOperational(position))
            {
                reason = "Recruiting building is still under construction.";
                return false;
            }

            BuildingDefinition definition =
                _buildingRegistry.GetById(placement.BuildingId);

            if (definition == null
                || !BuildingDefinitionCapabilities.TryGetEnabledModule(
                    definition,
                    out module))
            {
                reason = "Building has no enabled unit recruitment module.";
                return false;
            }

            if (!TryFindRecipe(module, unitTypeId, out recipe))
            {
                reason =
                    $"Building cannot recruit unit type '{unitTypeId}'.";
                return false;
            }

            if (_unitClassConfig?.GetConfig(unitTypeId) == null)
            {
                reason =
                    $"Unit type '{unitTypeId}' is not registered.";
                return false;
            }

            buildingId = placement.BuildingId;
            return true;
        }

        public bool TryResolveDeployment(
            UnitRecruitmentQueueItemSnapshot ready,
            out UnitRecruitmentBuildingModule module,
            out string reason)
        {
            module = null;
            reason = null;

            if (_constructionSnapshot == null
                || _buildingRegistry == null
                || _constructionLifecycle == null)
            {
                reason = "construction context is unavailable";
                return false;
            }

            if (!TryFindPlacement(
                    ready.RecruitingBuildingPosition,
                    out ConstructionSavedPlacement placement))
            {
                reason = "recruiting building no longer exists";
                return false;
            }

            if (!string.Equals(
                    NormalizeRequiredId(placement.OwnerId),
                    ready.OwnerId,
                    StringComparison.Ordinal))
            {
                reason = "recruiting building owner changed";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(ready.RecruitingBuildingId)
                && !string.Equals(
                    placement.BuildingId,
                    ready.RecruitingBuildingId,
                    StringComparison.Ordinal))
            {
                reason = "recruiting building identity changed";
                return false;
            }

            if (!_constructionLifecycle.IsOperational(
                    ready.RecruitingBuildingPosition))
            {
                reason = "recruiting building is not operational";
                return false;
            }

            BuildingDefinition definition =
                _buildingRegistry.GetById(placement.BuildingId);

            if (definition == null
                || !BuildingDefinitionCapabilities.TryGetEnabledModule(
                    definition,
                    out module))
            {
                reason = "recruiting module is unavailable";
                return false;
            }

            if (_unitClassConfig?.GetConfig(ready.UnitTypeId) == null)
            {
                reason = "unit type is no longer registered";
                return false;
            }

            return true;
        }

        private bool TryFindPlacement(
            Vector2Int position,
            out ConstructionSavedPlacement placement)
        {
            var placements = _constructionSnapshot?.GetSavedPlacements();
            if (placements != null)
            {
                for (int index = 0; index < placements.Count; index++)
                {
                    if (placements[index].Position != position)
                        continue;

                    placement = placements[index];
                    return true;
                }
            }

            placement = default;
            return false;
        }

        private static bool TryFindRecipe(
            UnitRecruitmentBuildingModule module,
            string unitTypeId,
            out UnitRecruitmentRecipeDefinition recipe)
        {
            if (module?.Recipes != null)
            {
                for (int index = 0; index < module.Recipes.Count; index++)
                {
                    UnitRecruitmentRecipeDefinition candidate =
                        module.Recipes[index];

                    if (candidate == null
                        || !string.Equals(
                            NormalizeRequiredId(candidate.UnitTypeId),
                            unitTypeId,
                            StringComparison.Ordinal))
                    {
                        continue;
                    }

                    recipe = candidate;
                    return true;
                }
            }

            recipe = null;
            return false;
        }

        private static string NormalizeRequiredId(string value)
            => string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
    }
}
