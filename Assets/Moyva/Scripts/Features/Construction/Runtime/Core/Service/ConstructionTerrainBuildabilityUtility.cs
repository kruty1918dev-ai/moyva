using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.WorldCreation.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal static class ConstructionTerrainBuildabilityUtility
    {
        private static readonly Vector2Int[] CardinalDirections =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left,
        };

        public static bool IsTerrainBlocked(
            Vector2Int position,
            IGridService gridService,
            IGeneratedTerrainLevelQuery generatedTerrainLevelQuery,
            ITileSettingsService tileSettings,
            IConstructionPlacementRulesProvider placementRulesProvider,
            WorldCreationDefaultsSO worldDefaults,
            out string reason)
        {
            reason = null;

            if (placementRulesProvider != null && !placementRulesProvider.EnableTerrainRules)
                return false;

            if (generatedTerrainLevelQuery != null
                && generatedTerrainLevelQuery.TryGetTerrainLevel(position, out int terrainLevel)
                && terrainLevel > 0)
            {
                if ((placementRulesProvider?.AllowBuildingOnHills ?? true) == false)
                {
                    reason = $"hill building disabled at level {terrainLevel}";
                    return true;
                }

                var profileBlockedTerrainRanges = placementRulesProvider?.BlockedTerrainLevelRanges;
                if (profileBlockedTerrainRanges != null
                    && profileBlockedTerrainRanges.Length > 0
                    && IsTerrainLevelBlocked(profileBlockedTerrainRanges, terrainLevel))
                {
                    reason = $"blocked profile terrain level {terrainLevel}";
                    return true;
                }

                if (worldDefaults?.BlockedBuildingHillLevelRanges != null
                    && worldDefaults.BlockedBuildingHillLevelRanges.Count > 0
                    && IsTerrainLevelBlocked(worldDefaults.BlockedBuildingHillLevelRanges, terrainLevel))
                {
                    reason = $"blocked hill level {terrainLevel}";
                    return true;
                }
            }

            if (gridService == null)
                return false;

            if (!gridService.TryGetTileData(position, out var tileTypeId))
            {
                reason = "outside generated grid";
                return true;
            }

            if (IsBlockedBuildingTile(tileTypeId, placementRulesProvider?.BlockedTileIds, worldDefaults?.BlockedBuildingTileIds))
            {
                reason = $"blocked tile '{tileTypeId}'";
                return true;
            }

            if (tileSettings != null && tileSettings.IsBuildBlocked(tileTypeId))
            {
                reason = $"build-blocked layer '{tileTypeId}'";
                return true;
            }

            if (placementRulesProvider?.BlockEdgeTerrainTiles ?? true)
            {
                if (IsEdgeTerrainTile(position, gridService, generatedTerrainLevelQuery))
                {
                    reason = "edge terrain tile";
                    return true;
                }
            }

            return false;
        }

        public static bool IsEdgeTerrainTile(
            Vector2Int position,
            IGridService gridService,
            IGeneratedTerrainLevelQuery generatedTerrainLevelQuery)
        {
            if (gridService == null || generatedTerrainLevelQuery == null)
                return false;

            if (!gridService.TryGetTileData(position, out _))
                return true;

            int currentLevel = ResolveTerrainLevel(position, generatedTerrainLevelQuery);
            for (int i = 0; i < CardinalDirections.Length; i++)
            {
                Vector2Int neighbor = position + CardinalDirections[i];
                if (!gridService.TryGetTileData(neighbor, out _))
                    continue;

                int neighborLevel = ResolveTerrainLevel(neighbor, generatedTerrainLevelQuery);
                if (neighborLevel != currentLevel)
                    return true;
            }

            return false;
        }

        private static int ResolveTerrainLevel(Vector2Int position, IGeneratedTerrainLevelQuery generatedTerrainLevelQuery)
        {
            return generatedTerrainLevelQuery != null
                && generatedTerrainLevelQuery.TryGetTerrainLevel(position, out int level)
                ? level
                : 0;
        }

        private static bool IsBlockedBuildingTile(
            string tileTypeId,
            string[] profileBlockedTileIds,
            IReadOnlyList<string> worldBlockedTileIds)
        {
            if (string.IsNullOrWhiteSpace(tileTypeId))
                return false;

            if (ContainsBlockedTileId(profileBlockedTileIds, tileTypeId))
                return true;

            return ContainsBlockedTileId(worldBlockedTileIds, tileTypeId);
        }

        private static bool ContainsBlockedTileId(IReadOnlyList<string> blockedTileIds, string tileTypeId)
        {
            if (blockedTileIds == null || blockedTileIds.Count == 0)
                return false;

            for (int i = 0; i < blockedTileIds.Count; i++)
            {
                string blockedId = blockedTileIds[i];
                if (string.IsNullOrWhiteSpace(blockedId))
                    continue;

                if (string.Equals(blockedId.Trim(), tileTypeId, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool IsTerrainLevelBlocked(IReadOnlyList<TerrainLevelRestrictionRange> ranges, int terrainLevel)
        {
            if (ranges == null || ranges.Count == 0)
                return false;

            for (int i = 0; i < ranges.Count; i++)
            {
                var range = ranges[i];
                if (range == null)
                    continue;

                int min = Mathf.Max(1, range.MinLevel);
                int max = Mathf.Max(1, range.MaxLevel);
                if (max < min)
                {
                    int swap = min;
                    min = max;
                    max = swap;
                }

                if (terrainLevel >= min && terrainLevel <= max)
                    return true;
            }

            return false;
        }
    }
}
