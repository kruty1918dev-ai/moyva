using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public static partial class BuildingPlacementEvaluator
    {
        private static bool IsBlockedByRequiredNeighborOffsets(
            BuildingPlacementEvaluationRequest request,
            BuildingDefinition definition,
            BuildingPlacementEvaluationResult result)
        {
            Vector2Int[] offsets;
            if (BuildingDefinitionCapabilities.TryGetEnabledModule(
                    definition,
                    out TerrainPlacementRuleModule module))
            {
                offsets = module.MergeMode switch
                {
                    PlacementRuleMergeMode.Disabled =>
                        Array.Empty<Vector2Int>(),
                    PlacementRuleMergeMode.Override =>
                        module.RequiredNeighborOffsets,
                    _ => definition?.PlacementRules
                        ?.RequiredNeighborOffsets,
                };
            }
            else
            {
                offsets = definition?.PlacementRules
                    ?.RequiredNeighborOffsets;
            }
            if (offsets == null || offsets.Length == 0)
                return false;

            for (int index = 0; index < offsets.Length; index++)
            {
                Vector2Int requiredPosition =
                    request.Position + offsets[index];
                bool exists = request.TileExists != null
                    ? request.TileExists(requiredPosition)
                    : !string.IsNullOrWhiteSpace(
                        request.GetTileId?.Invoke(requiredPosition));
                if (exists)
                    continue;

                result?.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.Adjacency,
                    Message = $"Required neighboring cell at offset {offsets[index]} does not exist.",
                    Position = requiredPosition,
                    BuildingId = request.BuildingId,
                });
                return true;
            }

            return false;
        }

        private static bool IsBlockedByTileRequirements(
            BuildingPlacementEvaluationRequest request,
            BuildingDefinition definition,
            BuildingPlacementEvaluationResult result)
        {
            TileRequirementDefinition[] requirements;
            bool hasModule = BuildingDefinitionCapabilities
                .TryGetEnabledModule(
                    definition,
                    out TileRequirementBuildingModule module);
            bool moduleOverrides = hasModule
                && module.MergeMode == PlacementRuleMergeMode.Override;
            if (hasModule
                && module.MergeMode == PlacementRuleMergeMode.Disabled)
            {
                return false;
            }

            if (moduleOverrides)
            {
                requirements = module.Requirements
                    ?? Array.Empty<TileRequirementDefinition>();
            }
            else
            {
                requirements = definition?.PlacementRules
                                   ?.NearbyTileRequirements
                    ?? Array.Empty<TileRequirementDefinition>();
            }

            for (int index = 0; index < requirements.Length; index++)
            {
                TileRequirementDefinition requirement = requirements[index];
                if (requirement == null)
                    continue;

                if (CountMatchingDistinctTiles(
                        request,
                        definition,
                        requirement)
                    >= Mathf.Max(1, requirement.MinimumTileCount))
                {
                    continue;
                }

                string identity = !string.IsNullOrWhiteSpace(
                    requirement.TerrainTag)
                    ? $"tag '{requirement.TerrainTag}'"
                    : $"tile '{requirement.TileId}'";
                result?.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.Adjacency,
                    Message =
                        $"Requires at least {Mathf.Max(1, requirement.MinimumTileCount)} distinct cells matching {identity} within radius {Mathf.Max(0, requirement.Radius)}.",
                    Position = request.Position,
                    BuildingId = request.BuildingId,
                    Radius = Mathf.Max(0, requirement.Radius),
                });
                return true;
            }

            if (moduleOverrides)
                return false;

            BuildingPlacementRules placement = definition?.PlacementRules;
            return IsBlockedBySemanticRequirement(
                       request,
                       definition,
                       placement?.RequiresWaterNearby == true,
                       "water",
                       result)
                || IsBlockedBySemanticRequirement(
                    request,
                    definition,
                    placement?.RequiresForestNearby == true,
                    "forest",
                    result)
                || IsBlockedBySemanticRequirement(
                    request,
                    definition,
                    placement?.RequiresMountainNearby == true,
                    "mountain",
                    result)
                || IsBlockedBySemanticRequirement(
                    request,
                    definition,
                    placement?.RequiresRoadNearby == true,
                    "road",
                    result);
        }

        private static bool IsBlockedBySemanticRequirement(
            BuildingPlacementEvaluationRequest request,
            BuildingDefinition definition,
            bool enabled,
            string tag,
            BuildingPlacementEvaluationResult result)
        {
            if (!enabled)
                return false;

            var requirement = new TileRequirementDefinition
            {
                TerrainTag = tag,
                Radius = 1,
                MinimumTileCount = 1,
            };
            if (CountMatchingDistinctTiles(request, definition, requirement)
                > 0)
            {
                return false;
            }

            result?.AddBlocker(new BuildingPlacementBlocker
            {
                Kind = BuildingPlacementBlockerKind.Adjacency,
                Message =
                    $"Requires terrain tag '{tag}' next to the footprint.",
                Position = request.Position,
                BuildingId = request.BuildingId,
                Radius = 1,
            });
            return true;
        }

        private static int CountMatchingDistinctTiles(
            BuildingPlacementEvaluationRequest request,
            BuildingDefinition definition,
            TileRequirementDefinition requirement)
        {
            int radius = Mathf.Max(0, requirement.Radius);
            HashSet<Vector2Int> matches =
                request.TileMatchWorkspace
                ?? new HashSet<Vector2Int>();
            matches.Clear();
            int footprintCount =
                BuildingFootprintUtility.GetOccupiedCellCount(definition);
            for (int cellIndex = 0;
                 cellIndex < footprintCount;
                 cellIndex++)
            {
                Vector2Int center = BuildingFootprintUtility
                    .GetOccupiedCell(
                        definition,
                        request.Position,
                        cellIndex,
                        request.Rotation);
                for (int offsetX = -radius;
                     offsetX <= radius;
                     offsetX++)
                {
                    for (int offsetY = -radius;
                         offsetY <= radius;
                         offsetY++)
                    {
                        Vector2Int position = center
                            + new Vector2Int(offsetX, offsetY);
                        if (matches.Contains(position)
                            || (request.TileExists != null
                                && !request.TileExists(position)))
                        {
                            continue;
                        }

                        if (MatchesTileRequirement(
                                request,
                                position,
                                requirement))
                        {
                            matches.Add(position);
                        }
                    }
                }
            }

            int matchCount = matches.Count;
            matches.Clear();
            return matchCount;
        }

        private static bool MatchesTileRequirement(
            BuildingPlacementEvaluationRequest request,
            Vector2Int position,
            TileRequirementDefinition requirement)
        {
            if (!string.IsNullOrWhiteSpace(requirement.TerrainTag))
            {
                if (request.HasTerrainTag != null
                    && request.HasTerrainTag(
                        position,
                        requirement.TerrainTag))
                {
                    return true;
                }

                return string.Equals(
                    request.GetTileId?.Invoke(position)?.Trim(),
                    requirement.TerrainTag.Trim(),
                    StringComparison.OrdinalIgnoreCase);
            }

            return !string.IsNullOrWhiteSpace(requirement.TileId)
                && string.Equals(
                    request.GetTileId?.Invoke(position)?.Trim(),
                    requirement.TileId.Trim(),
                    StringComparison.OrdinalIgnoreCase);
        }

        private static bool MatchesAnyTerrainTag(
            BuildingPlacementEvaluationRequest request,
            Vector2Int position,
            IReadOnlyList<string> tags)
        {
            if (tags == null || request.HasTerrainTag == null)
                return false;

            for (int index = 0; index < tags.Count; index++)
            {
                string tag = tags[index];
                if (!string.IsNullOrWhiteSpace(tag)
                    && request.HasTerrainTag(position, tag))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasConfiguredValues(
            IReadOnlyList<string> values)
        {
            if (values == null)
                return false;

            for (int index = 0; index < values.Count; index++)
            {
                if (!string.IsNullOrWhiteSpace(values[index]))
                    return true;
            }

            return false;
        }

    }
}
