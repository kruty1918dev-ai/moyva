using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public static partial class BuildingPlacementEvaluator
    {
        private static bool IsBlockedByFog(
            BuildingPlacementEvaluationRequest request,
            BuildingDefinition definition,
            BuildingPlacementEvaluationResult result)
        {
            if (BuildingDefinitionCapabilities.TryGetEnabledModule(
                    definition,
                    out FogPlacementRuleModule module))
            {
                if (module.MergeMode == PlacementRuleMergeMode.Disabled
                    || (module.MergeMode
                            == PlacementRuleMergeMode.Override
                        && module.Visibility
                            == FogPlacementVisibility.Any))
                {
                    return false;
                }

                if (module.MergeMode == PlacementRuleMergeMode.Override)
                {
                    return IsBlockedByExplicitFogRule(
                        request,
                        definition,
                        module.Visibility,
                        result);
                }
            }

            if (definition != null
                && (definition.CanPlaceInFog
                    || definition.PlacementRules?.CanPlaceInFog == true))
                return false;

            if (request.IsFogBlocked == null)
                return false;

            int count = BuildingFootprintUtility.GetOccupiedCellCount(definition);
            for (int index = 0; index < count; index++)
            {
                Vector2Int position = BuildingFootprintUtility.GetOccupiedCell(
                    definition,
                    request.Position,
                    index,
                    request.Rotation);
                if (!request.IsFogBlocked(position))
                    continue;

                result?.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.Fog,
                    Message = "Тайл не є видимим у Fog of War. Будівництво дозволене тільки на Visible.",
                    Position = position,
                });
                return true;
            }

            return false;
        }

        private static bool IsBlockedByExplicitFogRule(
            BuildingPlacementEvaluationRequest request,
            BuildingDefinition definition,
            FogPlacementVisibility visibility,
            BuildingPlacementEvaluationResult result)
        {
            int count = BuildingFootprintUtility.GetOccupiedCellCount(definition);
            for (int index = 0; index < count; index++)
            {
                Vector2Int position = BuildingFootprintUtility
                    .GetOccupiedCell(
                        definition,
                        request.Position,
                        index,
                        request.Rotation);
                FogStateType? state = request.GetFogState?.Invoke(position);
                bool allowed = !state.HasValue
                    || state.Value == FogStateType.Visible
                    || (visibility == FogPlacementVisibility.ExploredOrVisible
                        && state.Value == FogStateType.Explored);
                if (allowed)
                    continue;

                result?.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.Fog,
                    Message =
                        $"Fog state '{state.Value}' does not satisfy '{visibility}'.",
                    Position = position,
                    BuildingId = request.BuildingId,
                });
                return true;
            }

            // A legacy reader can still enforce Visible when the explicit state
            // reader is unavailable.
            return request.GetFogState == null
                   && visibility == FogPlacementVisibility.Visible
                ? IsBlockedByLegacyFogCallback(
                    request,
                    definition,
                    result)
                : false;
        }

        private static bool IsBlockedByLegacyFogCallback(
            BuildingPlacementEvaluationRequest request,
            BuildingDefinition definition,
            BuildingPlacementEvaluationResult result)
        {
            if (request.IsFogBlocked == null)
                return false;

            int count = BuildingFootprintUtility.GetOccupiedCellCount(definition);
            for (int index = 0; index < count; index++)
            {
                Vector2Int position = BuildingFootprintUtility
                    .GetOccupiedCell(
                        definition,
                        request.Position,
                        index,
                        request.Rotation);
                if (!request.IsFogBlocked(position))
                    continue;

                result?.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.Fog,
                    Message = "Tile does not satisfy the building fog rule.",
                    Position = position,
                    BuildingId = request.BuildingId,
                });
                return true;
            }

            return false;
        }

    }
}
