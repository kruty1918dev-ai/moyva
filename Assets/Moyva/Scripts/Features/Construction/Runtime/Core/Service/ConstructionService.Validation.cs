using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        private bool IsBlockedBySpacing(Vector2Int position, Vector2Int? ignoredPendingPosition)
        {
            try
            {
                if (_minSpacing <= 0)
                {
                    if (VerboseLogs)
                        Debug.Log($"[Construction] IsBlockedBySpacing({position}): _minSpacing <= 0, spacing-перевірка відключена");
                    return false;
                }

                if (_objectsMapService == null)
                {
                    Debug.LogError("[Construction] IsBlockedBySpacing: _objectsMapService == null");
                    return false;
                }

                if (_pendingPositions == null)
                {
                    Debug.LogError("[Construction] IsBlockedBySpacing: _pendingPositions == null");
                    return false;
                }

                for (int dx = -_minSpacing; dx <= _minSpacing; dx++)
                {
                    for (int dy = -_minSpacing; dy <= _minSpacing; dy++)
                    {
                        if (dx == 0 && dy == 0)
                            continue;

                        var neighbor = new Vector2Int(position.x + dx, position.y + dy);
                        bool blockedByPending = _pendingPositions.Contains(neighbor) && neighbor != ignoredPendingPosition;
                        bool isOccupied = _objectsMapService.IsOccupied(neighbor);
                        if (isOccupied || blockedByPending)
                        {
                            if (VerboseLogs)
                                Debug.Log($"[Construction] IsBlockedBySpacing({position}): BLOCKED біля {neighbor} (occupied={isOccupied}, pending={blockedByPending})");
                            return true;
                        }
                    }
                }

                if (VerboseLogs)
                    Debug.Log($"[Construction] IsBlockedBySpacing({position}): OK (spacing={_minSpacing})");

                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Construction] ПОМИЛКА в IsBlockedBySpacing({position}): {ex.GetType().Name} - {ex.Message}");
                return false;
            }
        }

        private bool CanPlaceAt(
            Vector2Int position,
            Vector2Int? ignoredPendingPosition,
            string buildingId,
            out bool tileOccupied,
            out bool spacingBlocked,
            out bool fogBlocked,
            out bool influenceZoneBlocked,
            out bool terrainBlocked,
            Vector2Int? ignoredOccupiedPosition = null)
        {
            if (VerboseLogs)
                Debug.Log($"[Construction] CanPlaceAt({position}, buildingId={buildingId}) проверка ПОЧАЛАСЬ");

            try
            {
                var result = BuildingPlacementEvaluator.Evaluate(new BuildingPlacementEvaluationRequest
                {
                    BuildingRegistry = _buildingRegistry,
                    BuildingId = buildingId,
                    Position = position,
                    IgnoredPendingPosition = ignoredPendingPosition,
                    IgnoredOccupiedPosition = ignoredOccupiedPosition,
                    MinSpacing = _minSpacing,
                    TownHallBuildRadius = _townHallBuildRadius,
                    IsOccupied = tilePosition => !ignoredOccupiedPosition.HasValue || tilePosition != ignoredOccupiedPosition.Value
                        ? _objectsMapService.IsOccupied(tilePosition)
                        : false,
                    GetOccupantId = tilePosition => GetObjectOccupantId(tilePosition, ignoredOccupiedPosition),
                    IsFogBlocked = IsBlockedByFog,
                    PendingPlacements = BuildPlacementSimulationEntries(),
                });

                tileOccupied = result.TileOccupied;
                if (VerboseLogs)
                    Debug.Log($"[Construction] CanPlaceAt({position}): tileOccupied={tileOccupied}");

                spacingBlocked = result.SpacingBlocked;
                if (VerboseLogs)
                    Debug.Log($"[Construction] CanPlaceAt({position}): spacingBlocked={spacingBlocked}");

                fogBlocked = result.FogBlocked;
                if (VerboseLogs)
                    Debug.Log($"[Construction] CanPlaceAt({position}): fogBlocked={fogBlocked}");

                influenceZoneBlocked = result.InfluenceZoneBlocked;
                if (VerboseLogs)
                    Debug.Log($"[Construction] CanPlaceAt({position}): influenceZoneBlocked={influenceZoneBlocked}");

                terrainBlocked = IsBlockedByTerrain(position, out var terrainReason);
                if (VerboseLogs)
                    Debug.Log($"[Construction] CanPlaceAt({position}): terrainBlocked={terrainBlocked}, terrainReason={terrainReason}");

                bool allowed = result.IsValid && !terrainBlocked;
                if (VerboseLogs)
                    Debug.Log($"[Construction] CanPlaceAt({position}) результат: {(allowed ? "✓ VALID" : "❌ BLOCKED")}");

                return allowed;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Construction] ПОМИЛКА в CanPlaceAt({position}, {buildingId}): {ex.GetType().Name} - {ex.Message}");
                tileOccupied = false;
                spacingBlocked = false;
                fogBlocked = false;
                influenceZoneBlocked = false;
                terrainBlocked = false;
                return false;
            }
        }

        private string GetObjectOccupantId(Vector2Int position, Vector2Int? ignoredOccupiedPosition = null)
        {
            if (ignoredOccupiedPosition.HasValue && position == ignoredOccupiedPosition.Value)
                return null;

            return _objectsMapService.TryGetOccupant(position, out var occupantId)
                ? occupantId
                : null;
        }

        private List<BuildingPlacementSimulationEntry> BuildPlacementSimulationEntries()
        {
            var entries = new List<BuildingPlacementSimulationEntry>(_pendingPlacements.Count);
            for (int index = 0; index < _pendingPlacements.Count; index++)
            {
                var placement = _pendingPlacements[index];
                entries.Add(new BuildingPlacementSimulationEntry(placement.Position, placement.BuildingId));
            }

            return entries;
        }
    }
}
