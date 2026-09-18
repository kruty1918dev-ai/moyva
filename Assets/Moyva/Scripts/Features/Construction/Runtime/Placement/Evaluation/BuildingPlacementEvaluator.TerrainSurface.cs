using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public static partial class BuildingPlacementEvaluator
    {
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
                Vector2Int position = BuildingFootprintUtility.GetOccupiedCell(
                    definition,
                    request.Position,
                    index,
                    request.Rotation);
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
                Vector2Int position = BuildingFootprintUtility.GetOccupiedCell(
                    definition,
                    request.Position,
                    index,
                    request.Rotation);
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
    }
}
