using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.WorldCreation.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        private bool IsBlockedByFog(Vector2Int position)
        {
            try
            {
                if (_placementRulesProvider != null
                    && (!_placementRulesProvider.EnableFogRules || !_placementRulesProvider.RequireVisibleFogTile))
                {
                    if (VerboseLogs)
                        Debug.Log($"[Construction] IsBlockedByFog({position}): profile disabled fog rule");
                    return false;
                }

                if (_fogOfWarService == null)
                {
                    if (VerboseLogs)
                        Debug.Log($"[Construction] IsBlockedByFog({position}): _fogOfWarService == null, fog-перевірка відключена");
                    return false;
                }

                var fogState = _fogOfWarService.GetFogState(position);
                bool isBlocked = fogState != FogStateType.Visible;

                if (VerboseLogs && isBlocked)
                    Debug.Log($"[Construction] IsBlockedByFog({position}): BLOCKED (fogState={fogState})");

                return isBlocked;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Construction] ПОМИЛКА в IsBlockedByFog({position}): {ex.GetType().Name} - {ex.Message}");
                return false;
            }
        }

        private void ApplyBuildingFogReveal(string buildingId, Vector2Int position)
        {
            if (_fogOfWarService == null || _buildingRegistry == null)
                return;

            var definition = _buildingRegistry.GetById(buildingId);
            if (!BuildingDefinitionCapabilities.TryGetFogReveal(definition, out var fogReveal))
                return;

            int radius = Mathf.Max(0, fogReveal.RevealRadius);
            if (radius <= 0)
            {
                _fogOfWarService.UnregisterUnit(GetBuildingFogVisionAreaId(position));
                return;
            }

            string areaId = GetBuildingFogVisionAreaId(position);
            if (fogReveal.RevealWhileActive)
            {
                _fogOfWarService.RegisterFixedVisionArea(areaId, position, radius, fogReveal.Shape);
                return;
            }

            _fogOfWarService.UnregisterUnit(areaId);

            if (fogReveal.RevealOnBuilt)
                _fogOfWarService.RevealArea(position, radius, fogReveal.Shape, keepVisible: false, areaId);
        }

        private static string GetBuildingFogVisionAreaId(Vector2Int position)
            => $"building:{position.x}:{position.y}";

        private bool IsBlockedByTerrain(Vector2Int position, out string reason)
        {
            reason = null;

            if (_placementRulesProvider != null && !_placementRulesProvider.EnableTerrainRules)
                return false;

            if (_generatedTerrainLevelQuery != null
                && _generatedTerrainLevelQuery.TryGetTerrainLevel(position, out int terrainLevel)
                && terrainLevel > 0)
            {
                if ((_placementRulesProvider?.AllowBuildingOnHills ?? true) == false)
                {
                    reason = $"hill building disabled at level {terrainLevel}";
                    return true;
                }

                var profileBlockedTerrainRanges = _placementRulesProvider?.BlockedTerrainLevelRanges;
                if (profileBlockedTerrainRanges != null && profileBlockedTerrainRanges.Length > 0
                    && IsTerrainLevelBlocked(profileBlockedTerrainRanges, terrainLevel))
                {
                    reason = $"blocked profile terrain level {terrainLevel}";
                    return true;
                }

                if (HasCustomBuildingHillRestrictions())
                {
                    if (IsTerrainLevelBlocked(_worldDefaults.BlockedBuildingHillLevelRanges, terrainLevel))
                    {
                        reason = $"blocked hill level {terrainLevel}";
                        return true;
                    }
                }
            }

            if (_gridService == null)
                return false;

            if (!_gridService.TryGetTileData(position, out var tileTypeId))
            {
                reason = "outside generated grid";
                return true;
            }

            if (IsBlockedBuildingTile(tileTypeId))
            {
                reason = $"blocked tile '{tileTypeId}'";
                return true;
            }

            if (_tileSettings != null && _tileSettings.IsBuildBlocked(tileTypeId))
            {
                reason = $"build-blocked layer '{tileTypeId}'";
                return true;
            }

            return false;
        }

        private bool HasCustomBuildingHillRestrictions()
        {
            return _worldDefaults != null
                && _worldDefaults.BlockedBuildingHillLevelRanges != null
                && _worldDefaults.BlockedBuildingHillLevelRanges.Count > 0;
        }

        private bool IsBlockedBuildingTile(string tileTypeId)
        {
            if (string.IsNullOrWhiteSpace(tileTypeId))
                return false;

            var profileBlockedTileIds = _placementRulesProvider?.BlockedTileIds;
            if (profileBlockedTileIds != null && profileBlockedTileIds.Length > 0)
            {
                for (int i = 0; i < profileBlockedTileIds.Length; i++)
                {
                    string blockedId = profileBlockedTileIds[i];
                    if (string.IsNullOrWhiteSpace(blockedId))
                        continue;

                    if (string.Equals(blockedId.Trim(), tileTypeId, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            var blockedTileIds = _worldDefaults?.BlockedBuildingTileIds;
            if (blockedTileIds != null && blockedTileIds.Count > 0)
            {
                for (int i = 0; i < blockedTileIds.Count; i++)
                {
                    string blockedId = blockedTileIds[i];
                    if (string.IsNullOrWhiteSpace(blockedId))
                        continue;

                    if (string.Equals(blockedId.Trim(), tileTypeId, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
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
