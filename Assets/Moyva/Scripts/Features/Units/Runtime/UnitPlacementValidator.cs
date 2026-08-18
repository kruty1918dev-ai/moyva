using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.WorldCreation.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
    /// <summary>
    /// Canonical tile validator for placing a newly recruited unit.
    /// Mirrors the movement terrain restrictions so a green deployment tile
    /// cannot immediately be rejected by movement terrain rules.
    /// </summary>
    internal sealed class UnitPlacementValidator : IUnitPlacementValidator
    {
        private readonly IGridService _grid;
        private readonly IObjectsMapService _objectsMap;
        private readonly IGeneratedTerrainLevelQuery _terrainLevelQuery;
        private readonly WorldCreationDefaultsSO _worldDefaults;

        public UnitPlacementValidator(
            IGridService grid,
            IObjectsMapService objectsMap,
            [InjectOptional] IGeneratedTerrainLevelQuery terrainLevelQuery = null,
            [InjectOptional] WorldCreationDefaultsSO worldDefaults = null)
        {
            _grid = grid;
            _objectsMap = objectsMap;
            _terrainLevelQuery = terrainLevelQuery;
            _worldDefaults = worldDefaults;
        }

        public bool IsTerrainAllowed(Vector2Int position, out string reason)
        {
            reason = null;

            if (_grid == null || !_grid.ContainsCell(position))
            {
                reason = "Тайл знаходиться за межами карти.";
                return false;
            }

            if (!_grid.TryGetTileTypeId(position, out string tileTypeId)
                || string.IsNullOrWhiteSpace(tileTypeId))
            {
                reason = "На клітинці немає валідного типу тайла.";
                return false;
            }

            if (IsBlockedUnitTile(tileTypeId))
            {
                reason = $"Тип тайла '{tileTypeId}' заборонений для юнітів.";
                return false;
            }

            if (_terrainLevelQuery != null
                && _terrainLevelQuery.TryGetTerrainLevel(position, out int terrainLevel)
                && terrainLevel > 0
                && IsTerrainLevelBlocked(
                    _worldDefaults?.BlockedUnitHillLevelRanges,
                    terrainLevel))
            {
                reason = $"Рівень висоти {terrainLevel} заборонений для юнітів.";
                return false;
            }

            return true;
        }

        public bool CanDeployUnit(
            string unitTypeId,
            Vector2Int position,
            out string reason)
        {
            if (string.IsNullOrWhiteSpace(unitTypeId))
            {
                reason = "Тип юніта не визначено.";
                return false;
            }

            if (!IsTerrainAllowed(position, out reason))
                return false;

            if (_objectsMap == null)
            {
                reason = "Карта зайнятості об'єктів недоступна.";
                return false;
            }

            if (_objectsMap.IsOccupied(position))
            {
                _objectsMap.TryGetOccupant(position, out string occupantId);
                reason = string.IsNullOrWhiteSpace(occupantId)
                    ? "Клітинка вже зайнята."
                    : $"Клітинка зайнята об'єктом '{occupantId}'.";
                return false;
            }

            reason = null;
            return true;
        }

        private bool IsBlockedUnitTile(string tileTypeId)
        {
            if (string.IsNullOrWhiteSpace(tileTypeId))
                return false;

            IReadOnlyList<string> blockedTileIds =
                _worldDefaults?.BlockedUnitTileIds;

            if (blockedTileIds == null || blockedTileIds.Count == 0)
                return false;

            for (int i = 0; i < blockedTileIds.Count; i++)
            {
                string blockedId = blockedTileIds[i];
                if (string.IsNullOrWhiteSpace(blockedId))
                    continue;

                if (string.Equals(
                    blockedId.Trim(),
                    tileTypeId,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsTerrainLevelBlocked(
            IReadOnlyList<TerrainLevelRestrictionRange> ranges,
            int terrainLevel)
        {
            if (ranges == null || ranges.Count == 0)
                return false;

            for (int i = 0; i < ranges.Count; i++)
            {
                TerrainLevelRestrictionRange range = ranges[i];
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
