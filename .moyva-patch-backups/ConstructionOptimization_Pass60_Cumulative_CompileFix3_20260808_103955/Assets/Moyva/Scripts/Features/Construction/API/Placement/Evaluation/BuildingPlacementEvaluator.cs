using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{

    public static class BuildingPlacementEvaluator
    {
        private static readonly Vector2Int[] CardinalDirections =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left,
        };

        public static BuildingPlacementEvaluationResult Evaluate(BuildingPlacementEvaluationRequest request)
        {
            var result = new BuildingPlacementEvaluationResult();
            EvaluateCore(request, result);
            return result;
        }

        public static bool CanPlace(BuildingPlacementEvaluationRequest request)
        {
            return EvaluateCore(request, result: null);
        }

        private static bool EvaluateCore(
            BuildingPlacementEvaluationRequest request,
            BuildingPlacementEvaluationResult result)
        {
            if (request == null)
            {
                if (result != null)
                    result.ConfigurationBlocked = true;
                result?.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.Configuration,
                    Message = "Placement request is null.",
                });
                return false;
            }

            BuildingDefinition definition = request.BuildingRegistry?.GetById(request.BuildingId);
            if (definition == null)
            {
                if (result != null)
                    result.ConfigurationBlocked = true;
                result?.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.Configuration,
                    Message = $"Building definition '{request.BuildingId}' is missing.",
                    Position = request.Position,
                    BuildingId = request.BuildingId,
                });
                return false;
            }

            if (!BuildingFootprintUtility.TryValidate(definition, out string footprintConfigurationReason))
            {
                if (result != null)
                    result.ConfigurationBlocked = true;
                result?.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.Configuration,
                    Message = footprintConfigurationReason,
                    Position = request.Position,
                    BuildingId = request.BuildingId,
                });
                return false;
            }

            int footprintCellCount = BuildingFootprintUtility.GetOccupiedCellCount(definition);
            if (result != null)
            {
                for (int index = 0; index < footprintCellCount; index++)
                    result.AddFootprintPosition(BuildingFootprintUtility.GetOccupiedCell(definition, request.Position, index));
            }

            bool footprintOutsideMap =
                IsFootprintOutsideMap(request, definition, result);
            if (result != null)
                result.TerrainBlocked = footprintOutsideMap;
            if (footprintOutsideMap)
                return false;

            bool tileOccupied = IsFootprintOccupied(request, definition, result);
            if (result != null)
                result.TileOccupied = tileOccupied;
            if (tileOccupied)
                return false;

            bool terrainBlocked = IsBlockedByResolvedTerrain(
                    request,
                    definition,
                    result)
                || IsBlockedByFootprintSlope(request, definition, result);
            if (result != null)
                result.TerrainBlocked = terrainBlocked;
            if (terrainBlocked)
                return false;

            bool adjacencyBlocked =
                IsBlockedByRequiredNeighborOffsets(
                    request,
                    definition,
                    result)
                || IsBlockedByTileRequirements(
                    request,
                    definition,
                    result);
            if (result != null)
                result.AdjacencyBlocked = adjacencyBlocked;
            if (adjacencyBlocked)
                return false;

            bool spacingBlocked = IsBlockedBySpacing(request, definition, result);
            if (result != null)
                result.SpacingBlocked = spacingBlocked;
            if (spacingBlocked)
                return false;

            bool fogBlocked = IsBlockedByFog(request, definition, result);
            if (result != null)
                result.FogBlocked = fogBlocked;
            if (fogBlocked)
                return false;

            bool influenceBlocked =
                IsBlockedByInfluenceZone(request, result);
            if (result != null)
                result.InfluenceZoneBlocked = influenceBlocked;
            if (influenceBlocked)
                return false;

            return !IsBlockedByRegisteredEvaluators(
                request,
                definition,
                result);
        }

        public static int ResolveInfluenceRadius(BuildingDefinition definition, int fallbackRadius)
        {
            return BuildingDefinitionCapabilities.GetInfluenceRadius(definition, fallbackRadius);
        }

        public static int GetChebyshevDistance(Vector2Int first, Vector2Int second)
        {
            return Mathf.Max(Mathf.Abs(first.x - second.x), Mathf.Abs(first.y - second.y));
        }

        public static bool IsInfluenceCenter(BuildingDefinition definition)
        {
            if (definition == null)
                return false;

            if (BuildingDefinitionCapabilities.HasEnabledModule<
                    SettlementCenterBuildingModule>(definition))
            {
                return true;
            }

            if (definition.PlacementRules != null)
                return definition.PlacementRules.CreatesSettlementInfluence;

            return false;
        }

        private static bool IsBlockedByRegisteredEvaluators(
            BuildingPlacementEvaluationRequest request,
            BuildingDefinition definition,
            BuildingPlacementEvaluationResult result)
        {
            IReadOnlyList<IBuildingPlacementRuleEvaluator> evaluators =
                request.RuleEvaluators;
            if (evaluators == null)
                return false;

            for (int index = 0; index < evaluators.Count; index++)
            {
                IBuildingPlacementRuleEvaluator evaluator =
                    evaluators[index];
                if (evaluator == null)
                    continue;

                BuildingPlacementBlocker blocker;
                try
                {
                    blocker = evaluator.Evaluate(
                        request,
                        definition);
                }
                catch (Exception exception)
                {
                    blocker = new BuildingPlacementBlocker
                    {
                        Kind = BuildingPlacementBlockerKind.Configuration,
                        Message =
                            $"Placement evaluator '{evaluator.GetType().Name}' failed: {exception.Message}",
                        Position = request.Position,
                        BuildingId = request.BuildingId,
                    };
                }

                if (blocker == null)
                    continue;

                MarkBlocked(result, blocker.Kind);
                result?.AddBlocker(blocker);
                return true;
            }

            return false;
        }

        private static void MarkBlocked(
            BuildingPlacementEvaluationResult result,
            BuildingPlacementBlockerKind kind)
        {
            if (result == null)
                return;

            switch (kind)
            {
                case BuildingPlacementBlockerKind.OccupiedTile:
                    result.TileOccupied = true;
                    break;
                case BuildingPlacementBlockerKind.Spacing:
                    result.SpacingBlocked = true;
                    break;
                case BuildingPlacementBlockerKind.Fog:
                    result.FogBlocked = true;
                    break;
                case BuildingPlacementBlockerKind.InfluenceRequired:
                case BuildingPlacementBlockerKind.InfluenceOverlap:
                    result.InfluenceZoneBlocked = true;
                    break;
                case BuildingPlacementBlockerKind.Terrain:
                    result.TerrainBlocked = true;
                    break;
                case BuildingPlacementBlockerKind.Adjacency:
                    result.AdjacencyBlocked = true;
                    break;
                case BuildingPlacementBlockerKind.Configuration:
                case BuildingPlacementBlockerKind.Prerequisite:
                default:
                    result.ConfigurationBlocked = true;
                    break;
            }
        }

        private static bool IsFootprintOutsideMap(
            BuildingPlacementEvaluationRequest request,
            BuildingDefinition definition,
            BuildingPlacementEvaluationResult result)
        {
            if (request.TileExists == null)
                return false;

            int count = BuildingFootprintUtility.GetOccupiedCellCount(definition);
            for (int index = 0; index < count; index++)
            {
                Vector2Int position = BuildingFootprintUtility.GetOccupiedCell(
                    definition,
                    request.Position,
                    index);
                if (request.TileExists(position))
                    continue;

                result?.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.Terrain,
                    Message = "Footprint extends outside the generated map.",
                    Position = position,
                    BuildingId = request.BuildingId,
                });
                return true;
            }

            return false;
        }

        private static bool IsBlockedByResolvedTerrain(
            BuildingPlacementEvaluationRequest request,
            BuildingDefinition definition,
            BuildingPlacementEvaluationResult result)
        {
            if (BuildingDefinitionCapabilities.TryGetEnabledModule(
                    definition,
                    out TerrainPlacementRuleModule module))
            {
                switch (module.MergeMode)
                {
                    case PlacementRuleMergeMode.Disabled:
                        return false;
                    case PlacementRuleMergeMode.Override:
                        return IsBlockedByTerrainModule(
                            request,
                            definition,
                            module,
                            result);
                }
            }

            return IsBlockedByTerrain(request, definition, result)
                || IsBlockedByRequiredTerrain(
                    request,
                    definition,
                    result);
        }

        private static bool IsBlockedByTerrainModule(
            BuildingPlacementEvaluationRequest request,
            BuildingDefinition definition,
            TerrainPlacementRuleModule module,
            BuildingPlacementEvaluationResult result)
        {
            int count = BuildingFootprintUtility.GetOccupiedCellCount(definition);
            for (int index = 0; index < count; index++)
            {
                Vector2Int position = BuildingFootprintUtility.GetOccupiedCell(
                    definition,
                    request.Position,
                    index);
                string tileId = request.GetTileId?.Invoke(position);
                int? level = request.GetTerrainLevel?.Invoke(position);

                if (ContainsTileId(module.BlockedTerrainIds, tileId)
                    || MatchesAnyTerrainTag(
                        request,
                        position,
                        module.BlockedTerrainTags))
                {
                    return AddTerrainModuleBlocker(
                        request,
                        result,
                        position,
                        $"Terrain '{tileId}' is explicitly blocked for this building.");
                }

                bool hasAllowedIds = HasConfiguredValues(
                    module.AllowedTerrainIds);
                bool hasAllowedTags = HasConfiguredValues(
                    module.AllowedTerrainTags);
                if ((hasAllowedIds || hasAllowedTags)
                    && !ContainsTileId(module.AllowedTerrainIds, tileId)
                    && !MatchesAnyTerrainTag(
                        request,
                        position,
                        module.AllowedTerrainTags))
                {
                    return AddTerrainModuleBlocker(
                        request,
                        result,
                        position,
                        $"Terrain '{tileId}' is not allowed for this building.");
                }

                if (!module.AllowHills && level.GetValueOrDefault() > 0)
                {
                    return AddTerrainModuleBlocker(
                        request,
                        result,
                        position,
                        $"Elevated terrain level {level.Value} is disabled for this building.");
                }

                if (level.HasValue
                    && ContainsLevel(
                        module.BlockedTerrainLevels,
                        level.Value))
                {
                    return AddTerrainModuleBlocker(
                        request,
                        result,
                        position,
                        $"Terrain level {level.Value} is explicitly blocked for this building.");
                }

                if (level.HasValue
                    && module.AllowedTerrainLevels != null
                    && module.AllowedTerrainLevels.Length > 0
                    && !ContainsLevel(
                        module.AllowedTerrainLevels,
                        level.Value))
                {
                    return AddTerrainModuleBlocker(
                        request,
                        result,
                        position,
                        $"Terrain level {level.Value} is not allowed for this building.");
                }

                if (module.BlockEdgeTerrainTiles
                    && IsTerrainEdge(request, position))
                {
                    return AddTerrainModuleBlocker(
                        request,
                        result,
                        position,
                        "Terrain height edge is blocked for this building.");
                }
            }

            return false;
        }

        private static bool AddTerrainModuleBlocker(
            BuildingPlacementEvaluationRequest request,
            BuildingPlacementEvaluationResult result,
            Vector2Int position,
            string message)
        {
            result?.AddBlocker(new BuildingPlacementBlocker
            {
                Kind = BuildingPlacementBlockerKind.Terrain,
                Message = message,
                Position = position,
                BuildingId = request.BuildingId,
            });
            return true;
        }

        private static bool IsTerrainEdge(
            BuildingPlacementEvaluationRequest request,
            Vector2Int position)
        {
            if (request.GetTerrainLevel == null)
                return false;

            int current = request.GetTerrainLevel(position).GetValueOrDefault();
            for (int index = 0; index < CardinalDirections.Length; index++)
            {
                Vector2Int neighbor =
                    position + CardinalDirections[index];
                if (request.TileExists != null
                    && !request.TileExists(neighbor))
                {
                    continue;
                }

                int neighborLevel =
                    request.GetTerrainLevel(neighbor).GetValueOrDefault();
                if (neighborLevel != current)
                    return true;
            }

            return false;
        }

        private static bool IsBlockedByRequiredTerrain(
            BuildingPlacementEvaluationRequest request,
            BuildingDefinition definition,
            BuildingPlacementEvaluationResult result)
        {
            IReadOnlyList<string> requiredTerrainIds = definition?.RequiredTerrainIds;
            if (requiredTerrainIds == null || requiredTerrainIds.Count == 0)
                return false;

            if (request.GetTileId == null)
            {
                result?.AddNote("GetTileId не заданий, terrain-вимогу будівлі пропущено.");
                return false;
            }

            int count = BuildingFootprintUtility.GetOccupiedCellCount(definition);
            for (int index = 0; index < count; index++)
            {
                Vector2Int position = BuildingFootprintUtility.GetOccupiedCell(definition, request.Position, index);
                string tileId = request.GetTileId(position);
                if (ContainsTileId(requiredTerrainIds, tileId))
                    continue;

                result?.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.Terrain,
                    Message = $"Будівлю '{request.BuildingId}' не можна ставити на tile '{tileId}'.",
                    Position = position,
                    BuildingId = request.BuildingId,
                });
                return true;
            }

            return false;
        }

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
                        cellIndex);
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

        private static bool IsBlockedByTerrain(
            BuildingPlacementEvaluationRequest request,
            BuildingDefinition definition,
            BuildingPlacementEvaluationResult result)
        {
            if (request.IsTerrainBlocked == null)
                return false;

            int count = BuildingFootprintUtility.GetOccupiedCellCount(definition);
            for (int index = 0; index < count; index++)
            {
                Vector2Int position = BuildingFootprintUtility.GetOccupiedCell(definition, request.Position, index);
                if (!request.IsTerrainBlocked(position))
                    continue;

                result?.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.Terrain,
                    Message = "Footprint contains a terrain cell blocked for construction.",
                    Position = position,
                    BuildingId = request.BuildingId,
                });
                return true;
            }

            return false;
        }

        private static bool IsBlockedByFootprintSlope(
            BuildingPlacementEvaluationRequest request,
            BuildingDefinition definition,
            BuildingPlacementEvaluationResult result)
        {
            bool requiresFlatGround =
                definition?.Footprint?.RequiresFlatGround == true;
            if (BuildingDefinitionCapabilities.TryGetEnabledModule(
                    definition,
                    out TerrainPlacementRuleModule terrainModule))
            {
                requiresFlatGround = terrainModule.MergeMode switch
                {
                    PlacementRuleMergeMode.Disabled => false,
                    PlacementRuleMergeMode.Override =>
                        terrainModule.RequiresFlatGround,
                    _ => requiresFlatGround,
                };
            }

            if (!requiresFlatGround
                || request.GetTerrainLevel == null)
            {
                return false;
            }

            int? expectedLevel = null;
            int count = BuildingFootprintUtility.GetOccupiedCellCount(definition);
            for (int index = 0; index < count; index++)
            {
                Vector2Int position = BuildingFootprintUtility.GetOccupiedCell(definition, request.Position, index);
                int? level = request.GetTerrainLevel(position);
                if (!level.HasValue)
                    continue;

                if (!expectedLevel.HasValue)
                {
                    expectedLevel = level;
                    continue;
                }

                if (expectedLevel.Value == level.Value)
                    continue;

                result?.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.Terrain,
                    Message = $"Footprint requires flat ground, but terrain levels differ ({expectedLevel.Value} and {level.Value}).",
                    Position = position,
                    BuildingId = request.BuildingId,
                });
                return true;
            }

            return false;
        }

        private static bool ContainsTileId(IReadOnlyList<string> tileIds, string tileId)
        {
            if (tileIds == null || tileIds.Count == 0 || string.IsNullOrWhiteSpace(tileId))
                return false;

            for (int i = 0; i < tileIds.Count; i++)
            {
                string candidate = tileIds[i];
                if (string.IsNullOrWhiteSpace(candidate))
                    continue;

                if (string.Equals(candidate.Trim(), tileId, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool ContainsLevel(
            IReadOnlyList<int> levels,
            int expected)
        {
            if (levels == null)
                return false;

            for (int index = 0; index < levels.Count; index++)
            {
                if (levels[index] == expected)
                    return true;
            }

            return false;
        }

        private static bool IsFootprintOccupied(
            BuildingPlacementEvaluationRequest request,
            BuildingDefinition definition,
            BuildingPlacementEvaluationResult result)
        {
            int count = BuildingFootprintUtility.GetOccupiedCellCount(definition);
            for (int index = 0; index < count; index++)
            {
                Vector2Int position = BuildingFootprintUtility.GetOccupiedCell(definition, request.Position, index);
                bool occupied = request.IsOccupied != null && request.IsOccupied(position);
                bool pending = HasPendingAt(request, position, request.IgnoredPendingPosition, out var pendingEntry);
                if (!occupied && !pending)
                    continue;

                result?.AddBlocker(new BuildingPlacementBlocker
                {
                    Kind = BuildingPlacementBlockerKind.OccupiedTile,
                    Message = "Footprint cell is already occupied by a building or pending preview.",
                    Position = position,
                    BuildingId = pending ? pendingEntry.BuildingId : GetOccupantId(request, position),
                });
                return true;
            }

            return false;
        }

        private static bool IsBlockedBySpacing(
            BuildingPlacementEvaluationRequest request,
            BuildingDefinition definition,
            BuildingPlacementEvaluationResult result)
        {
            int spacing = ResolveSpacing(request, definition);
            if (spacing <= 0)
                return false;

            int footprintCount = BuildingFootprintUtility.GetOccupiedCellCount(definition);
            for (int cellIndex = 0; cellIndex < footprintCount; cellIndex++)
            {
                Vector2Int footprintCell = BuildingFootprintUtility.GetOccupiedCell(definition, request.Position, cellIndex);
                for (int offsetX = -spacing; offsetX <= spacing; offsetX++)
                {
                    for (int offsetY = -spacing; offsetY <= spacing; offsetY++)
                    {
                        if (offsetX == 0 && offsetY == 0)
                            continue;

                        var neighbor = new Vector2Int(footprintCell.x + offsetX, footprintCell.y + offsetY);
                        if (BuildingFootprintUtility.Contains(definition, request.Position, neighbor))
                            continue;

                        bool occupied = request.IsOccupied != null && request.IsOccupied(neighbor);
                        bool pending = HasPendingAt(request, neighbor, request.IgnoredPendingPosition, out var pendingEntry);
                        if (!occupied && !pending)
                            continue;

                        result?.AddBlocker(new BuildingPlacementBlocker
                        {
                            Kind = BuildingPlacementBlockerKind.Spacing,
                            Message = $"Порушено мінімальний відступ {spacing}: поруч є {(pending ? "pending-preview" : "зайнятий тайл")}.",
                            Position = neighbor,
                            BuildingId = pending ? pendingEntry.BuildingId : GetOccupantId(request, neighbor),
                            Radius = spacing,
                        });
                        return true;
                    }
                }
            }

            return false;
        }

        private static int ResolveSpacing(
            BuildingPlacementEvaluationRequest request,
            BuildingDefinition definition)
        {
            if (!BuildingDefinitionCapabilities.TryGetEnabledModule(
                    definition,
                    out SpacingPlacementRuleModule module))
            {
                return Mathf.Max(0, request.MinSpacing);
            }

            return module.MergeMode switch
            {
                PlacementRuleMergeMode.Disabled => 0,
                PlacementRuleMergeMode.Override =>
                    Mathf.Max(0, module.MinimumSpacing),
                _ => Mathf.Max(0, request.MinSpacing),
            };
        }

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
                Vector2Int position = BuildingFootprintUtility.GetOccupiedCell(definition, request.Position, index);
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
                        index);
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
                        index);
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

        private static bool HasInfluenceCenterCoveringPosition(
            BuildingPlacementEvaluationRequest request,
            Vector2Int position,
            BuildingDefinition candidate,
            out BuildingPlacementSimulationEntry? coveringCenter)
        {
            coveringCenter = null;
            int candidateLimit = ResolveCandidateProximityLimit(candidate);

            IReadOnlyList<BuildingPlacementSimulationEntry> placedBuildings =
                request.PlacedBuildings;
            if (placedBuildings != null)
            {
                for (int index = 0; index < placedBuildings.Count; index++)
                {
                    BuildingPlacementSimulationEntry placed =
                        placedBuildings[index];
                    if (placed.Position == request.IgnoredOccupiedPosition)
                        continue;
                    if (!IsSameOwnerOrUnknown(
                            request.OwnerId,
                            placed.OwnerId))
                    {
                        continue;
                    }

                    BuildingDefinition definition =
                        request.BuildingRegistry.GetById(
                            placed.BuildingId);
                    if (!IsInfluenceCenter(definition))
                        continue;

                    int allowedRadius = ResolveCoverageRadius(
                        definition,
                        candidateLimit,
                        request.TownHallBuildRadius);
                    if (allowedRadius <= 0
                        || GetChebyshevDistance(
                            placed.Position,
                            position) > allowedRadius)
                    {
                        continue;
                    }

                    coveringCenter = placed;
                    return true;
                }
            }
            else
            {
                int searchRadius =
                    ResolvePlacedCenterSearchRadius(
                        request,
                        candidateLimit);
                for (int offsetX = -searchRadius;
                     offsetX <= searchRadius;
                     offsetX++)
                {
                    for (int offsetY = -searchRadius;
                         offsetY <= searchRadius;
                         offsetY++)
                    {
                        var centerPosition = new Vector2Int(
                            position.x + offsetX,
                            position.y + offsetY);
                        if (centerPosition
                            == request.IgnoredPendingPosition)
                        {
                            continue;
                        }

                        string occupantId =
                            GetOccupantId(
                                request,
                                centerPosition);
                        if (string.IsNullOrWhiteSpace(occupantId))
                            continue;

                        Vector2Int resolvedOrigin =
                            ResolveOccupantOrigin(
                                request,
                                centerPosition);
                        if (!IsOwnedByPlacementOwner(
                                request,
                                resolvedOrigin))
                        {
                            continue;
                        }

                        BuildingDefinition definition =
                            request.BuildingRegistry.GetById(
                                occupantId);
                        if (!IsInfluenceCenter(definition))
                            continue;

                        int allowedRadius =
                            ResolveCoverageRadius(
                                definition,
                                candidateLimit,
                                request.TownHallBuildRadius);
                        if (allowedRadius <= 0)
                            continue;

                        if (GetChebyshevDistance(
                                resolvedOrigin,
                                position) <= allowedRadius)
                        {
                            coveringCenter =
                                new BuildingPlacementSimulationEntry(
                                    resolvedOrigin,
                                    occupantId,
                                    request.GetOccupantOwnerId?.Invoke(
                                        resolvedOrigin));
                            return true;
                        }
                    }
                }
            }

            IReadOnlyList<BuildingPlacementSimulationEntry>
                pendingPlacements = request.PendingPlacements;
            if (pendingPlacements == null)
                return false;

            for (int index = 0;
                 index < pendingPlacements.Count;
                 index++)
            {
                BuildingPlacementSimulationEntry pending =
                    pendingPlacements[index];
                if (pending.Position == request.IgnoredPendingPosition)
                    continue;
                if (!IsSameOwnerOrUnknown(
                        request.OwnerId,
                        pending.OwnerId))
                {
                    continue;
                }

                BuildingDefinition pendingDefinition =
                    request.BuildingRegistry.GetById(
                        pending.BuildingId);
                if (!IsInfluenceCenter(pendingDefinition))
                    continue;

                int allowedRadius = ResolveCoverageRadius(
                    pendingDefinition,
                    candidateLimit,
                    request.TownHallBuildRadius);
                if (allowedRadius <= 0)
                    continue;

                if (GetChebyshevDistance(
                        pending.Position,
                        position) <= allowedRadius)
                {
                    coveringCenter = pending;
                    return true;
                }
            }

            return false;
        }

        private static bool HasInfluenceCenterTooClose(
            BuildingPlacementEvaluationRequest request,
            Vector2Int candidatePosition,
            BuildingDefinition candidate,
            out BuildingPlacementOverlap overlap)
        {
            overlap = default;
            int candidateMinimum =
                BuildingDefinitionCapabilities
                    .GetMinimumSettlementCenterDistance(candidate);

            IReadOnlyList<BuildingPlacementSimulationEntry> placed =
                request.PlacedBuildings;
            if (placed != null)
            {
                for (int index = 0; index < placed.Count; index++)
                {
                    BuildingPlacementSimulationEntry entry = placed[index];
                    if (entry.Position == request.IgnoredOccupiedPosition)
                        continue;

                    BuildingDefinition existing =
                        request.BuildingRegistry.GetById(entry.BuildingId);
                    if (!IsInfluenceCenter(existing))
                        continue;

                    int requiredDistance = Mathf.Max(
                        candidateMinimum,
                        BuildingDefinitionCapabilities
                            .GetMinimumSettlementCenterDistance(existing));
                    if (requiredDistance <= 0)
                        continue;

                    if (GetChebyshevDistance(
                            entry.Position,
                            candidatePosition)
                        < requiredDistance)
                    {
                        overlap = new BuildingPlacementOverlap(
                            entry.Position,
                            entry.BuildingId,
                            requiredDistance);
                        return true;
                    }
                }
            }
            else
            {
                // Compatibility fallback for tests/external callers that do not
                // provide the Pass-2 placed-building snapshot. Runtime service
                // does provide it, so gameplay avoids this radius^2 path.
                int searchRadius = candidateMinimum;
                BuildingDefinition[] definitions =
                    request.BuildingRegistry?.GetAll()
                    ?? Array.Empty<BuildingDefinition>();
                for (int index = 0; index < definitions.Length; index++)
                {
                    BuildingDefinition definition = definitions[index];
                    if (IsInfluenceCenter(definition))
                    {
                        searchRadius = Mathf.Max(
                            searchRadius,
                            BuildingDefinitionCapabilities
                                .GetMinimumSettlementCenterDistance(definition));
                    }
                }

                for (int offsetX = -searchRadius;
                     offsetX <= searchRadius;
                     offsetX++)
                {
                    for (int offsetY = -searchRadius;
                         offsetY <= searchRadius;
                         offsetY++)
                    {
                        Vector2Int probe = new Vector2Int(
                            candidatePosition.x + offsetX,
                            candidatePosition.y + offsetY);
                        string occupantId = GetOccupantId(request, probe);
                        if (string.IsNullOrWhiteSpace(occupantId))
                            continue;

                        Vector2Int origin = ResolveOccupantOrigin(request, probe);
                        if (origin == request.IgnoredOccupiedPosition)
                            continue;

                        BuildingDefinition existing =
                            request.BuildingRegistry.GetById(occupantId);
                        if (!IsInfluenceCenter(existing))
                            continue;

                        int requiredDistance = Mathf.Max(
                            candidateMinimum,
                            BuildingDefinitionCapabilities
                                .GetMinimumSettlementCenterDistance(existing));
                        if (requiredDistance > 0
                            && GetChebyshevDistance(origin, candidatePosition)
                                < requiredDistance)
                        {
                            overlap = new BuildingPlacementOverlap(
                                origin,
                                occupantId,
                                requiredDistance);
                            return true;
                        }
                    }
                }
            }

            IReadOnlyList<BuildingPlacementSimulationEntry> pending =
                request.PendingPlacements;
            if (pending == null)
                return false;

            for (int index = 0; index < pending.Count; index++)
            {
                BuildingPlacementSimulationEntry entry = pending[index];
                if (entry.Position == request.IgnoredPendingPosition)
                    continue;

                BuildingDefinition existing =
                    request.BuildingRegistry.GetById(entry.BuildingId);
                if (!IsInfluenceCenter(existing))
                    continue;

                int requiredDistance = Mathf.Max(
                    candidateMinimum,
                    BuildingDefinitionCapabilities
                        .GetMinimumSettlementCenterDistance(existing));
                if (requiredDistance <= 0)
                    continue;

                if (GetChebyshevDistance(
                        entry.Position,
                        candidatePosition)
                    < requiredDistance)
                {
                    overlap = new BuildingPlacementOverlap(
                        entry.Position,
                        entry.BuildingId,
                        requiredDistance);
                    return true;
                }
            }

            return false;
        }

        private static bool HasOverlappingInfluenceCenter(
            BuildingPlacementEvaluationRequest request,
            Vector2Int candidatePosition,
            int candidateRadius,
            out BuildingPlacementOverlap overlap)
        {
            overlap = default;
            if (candidateRadius <= 0)
                return false;

            IReadOnlyList<BuildingPlacementSimulationEntry> placedBuildings =
                request.PlacedBuildings;
            if (placedBuildings != null)
            {
                for (int index = 0; index < placedBuildings.Count; index++)
                {
                    BuildingPlacementSimulationEntry placed =
                        placedBuildings[index];
                    if (placed.Position == request.IgnoredOccupiedPosition)
                        continue;

                    BuildingDefinition definition =
                        request.BuildingRegistry.GetById(
                            placed.BuildingId);
                    if (!IsInfluenceCenter(definition))
                        continue;

                    int existingRadius = ResolveInfluenceRadius(
                        definition,
                        Mathf.Max(
                            0,
                            request.TownHallBuildRadius));
                    if (existingRadius <= 0
                        || GetChebyshevDistance(
                            placed.Position,
                            candidatePosition)
                            > candidateRadius + existingRadius)
                    {
                        continue;
                    }

                    overlap = new BuildingPlacementOverlap(
                        placed.Position,
                        placed.BuildingId,
                        existingRadius);
                    return true;
                }
            }
            else
            {
                int searchRadius =
                    candidateRadius
                    + ResolveMaxInfluenceRadius(request);
                for (int offsetX = -searchRadius;
                     offsetX <= searchRadius;
                     offsetX++)
                {
                    for (int offsetY = -searchRadius;
                         offsetY <= searchRadius;
                         offsetY++)
                    {
                        var centerPosition = new Vector2Int(
                            candidatePosition.x + offsetX,
                            candidatePosition.y + offsetY);
                        if (centerPosition
                            == request.IgnoredPendingPosition)
                        {
                            continue;
                        }

                        string occupantId =
                            GetOccupantId(
                                request,
                                centerPosition);
                        if (string.IsNullOrWhiteSpace(occupantId))
                            continue;

                        Vector2Int resolvedOrigin =
                            ResolveOccupantOrigin(
                                request,
                                centerPosition);
                        BuildingDefinition definition =
                            request.BuildingRegistry.GetById(
                                occupantId);
                        if (!IsInfluenceCenter(definition))
                            continue;

                        int existingRadius = ResolveInfluenceRadius(
                            definition,
                            Mathf.Max(
                                0,
                                request.TownHallBuildRadius));
                        if (existingRadius <= 0)
                            continue;

                        if (GetChebyshevDistance(
                                resolvedOrigin,
                                candidatePosition)
                            > candidateRadius + existingRadius)
                        {
                            continue;
                        }

                        overlap = new BuildingPlacementOverlap(
                            resolvedOrigin,
                            occupantId,
                            existingRadius);
                        return true;
                    }
                }
            }

            IReadOnlyList<BuildingPlacementSimulationEntry>
                pendingPlacements = request.PendingPlacements;
            if (pendingPlacements == null)
                return false;

            for (int index = 0;
                 index < pendingPlacements.Count;
                 index++)
            {
                BuildingPlacementSimulationEntry pending =
                    pendingPlacements[index];
                if (pending.Position == request.IgnoredPendingPosition)
                    continue;

                BuildingDefinition pendingDefinition =
                    request.BuildingRegistry.GetById(
                        pending.BuildingId);
                if (!IsInfluenceCenter(pendingDefinition))
                    continue;

                int existingRadius = ResolveInfluenceRadius(
                    pendingDefinition,
                    Mathf.Max(
                        0,
                        request.TownHallBuildRadius));
                if (existingRadius <= 0)
                    continue;

                if (GetChebyshevDistance(
                        pending.Position,
                        candidatePosition)
                    <= candidateRadius + existingRadius)
                {
                    overlap = new BuildingPlacementOverlap(
                        pending.Position,
                        pending.BuildingId,
                        existingRadius);
                    return true;
                }
            }

            return false;
        }

private static int ResolveCoverageRadius(BuildingDefinition centerDefinition, int candidateLimit, int fallbackRadius)
        {
            int sourceRadius = ResolveInfluenceRadius(centerDefinition, Mathf.Max(0, fallbackRadius));
            if (sourceRadius <= 0)
                return 0;

            return candidateLimit > 0
                ? Mathf.Min(sourceRadius, candidateLimit)
                : sourceRadius;
        }

        private static int ResolvePlacedCenterSearchRadius(BuildingPlacementEvaluationRequest request, int candidateLimit)
        {
            int maxRadius = ResolveMaxInfluenceRadius(request);
            return candidateLimit > 0
                ? Mathf.Min(maxRadius, candidateLimit)
                : maxRadius;
        }

        private static int ResolveMaxInfluenceRadius(BuildingPlacementEvaluationRequest request)
        {
            if (request.MaxInfluenceRadius >= 0)
                return request.MaxInfluenceRadius;

            int maxRadius = 0;
            var definitions = request.BuildingRegistry.GetAll() ?? Array.Empty<BuildingDefinition>();
            for (int index = 0; index < definitions.Length; index++)
            {
                var definition = definitions[index];
                if (!IsInfluenceCenter(definition))
                    continue;

                maxRadius = Mathf.Max(maxRadius, ResolveInfluenceRadius(definition, request.TownHallBuildRadius));
            }

            return Mathf.Max(0, maxRadius);
        }

        private static int ResolveCandidateProximityLimit(BuildingDefinition candidate)
        {
            if (BuildingDefinitionCapabilities.TryGetEnabledModule(
                    candidate,
                    out SettlementInfluenceRequirementBuildingModule module)
                && module.MergeMode == PlacementRuleMergeMode.Override
                && module.MaximumDistanceToCenter > 0)
            {
                return module.MaximumDistanceToCenter;
            }

            return candidate != null && candidate.TownHallProximityRadiusOverride > 0
                ? candidate.TownHallProximityRadiusOverride
                : 0;
        }

        private static bool HasPendingAt(
            BuildingPlacementEvaluationRequest request,
            Vector2Int position,
            Vector2Int? ignoredPendingPosition,
            out BuildingPlacementSimulationEntry pendingEntry)
        {
            pendingEntry = default;
            var pendingPlacements = request.PendingPlacements;
            if (pendingPlacements == null)
                return false;

            for (int index = 0; index < pendingPlacements.Count; index++)
            {
                var pending = pendingPlacements[index];
                if (pending.Position == ignoredPendingPosition)
                    continue;

                BuildingDefinition pendingDefinition = request.BuildingRegistry?.GetById(pending.BuildingId);
                if (!BuildingFootprintUtility.Contains(pendingDefinition, pending.Position, position))
                    continue;

                pendingEntry = pending;
                return true;
            }

            return false;
        }

        private static string GetOccupantId(BuildingPlacementEvaluationRequest request, Vector2Int position)
        {
            return request.GetOccupantId != null ? request.GetOccupantId(position) : null;
        }

        private static Vector2Int ResolveOccupantOrigin(BuildingPlacementEvaluationRequest request, Vector2Int position)
        {
            Vector2Int? origin = request.GetOccupantOrigin?.Invoke(position);
            return origin ?? position;
        }

        private static bool IsOwnedByPlacementOwner(
            BuildingPlacementEvaluationRequest request,
            Vector2Int origin)
        {
            if (request.GetOccupantOwnerId == null)
                return true;

            return IsSameOwnerOrUnknown(
                request.OwnerId,
                request.GetOccupantOwnerId(origin));
        }

        private static bool IsSameOwnerOrUnknown(
            string placementOwnerId,
            string existingOwnerId)
        {
            if (string.IsNullOrWhiteSpace(placementOwnerId)
                || string.IsNullOrWhiteSpace(existingOwnerId))
            {
                return true;
            }

            return string.Equals(
                placementOwnerId.Trim(),
                existingOwnerId.Trim(),
                StringComparison.Ordinal);
        }

        private readonly struct BuildingPlacementOverlap
        {
            public BuildingPlacementOverlap(Vector2Int position, string buildingId, int radius)
            {
                Position = position;
                BuildingId = buildingId;
                Radius = radius;
            }

            public Vector2Int Position { get; }
            public string BuildingId { get; }
            public int Radius { get; }
        }
    }
}
