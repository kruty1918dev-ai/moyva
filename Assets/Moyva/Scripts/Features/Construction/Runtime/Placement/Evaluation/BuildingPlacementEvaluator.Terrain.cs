using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public static partial class BuildingPlacementEvaluator
    {
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
                    index,
                    request.Rotation);
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
                    index,
                    request.Rotation);
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
                Vector2Int position = BuildingFootprintUtility.GetOccupiedCell(
                    definition,
                    request.Position,
                    index,
                    request.Rotation);
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

    }
}
