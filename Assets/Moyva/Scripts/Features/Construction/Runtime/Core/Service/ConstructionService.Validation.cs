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
            var query = new ConstructionPlacementQueryRequest(
                buildingId,
                position,
                ignoredPendingPosition,
                ignoredOccupiedPosition,
                includeResources: false,
                includeDetails: true,
                ownerId: _activeOwnerId,
                attemptSource:
                    ConstructionPlacementAttemptSource.Confirm,
                allowUniquePreviewRelocation: false);
            ConstructionPlacementQueryResult result = EvaluatePlacement(query);

            BuildingPlacementEvaluationResult evaluation = result.EvaluationResult;
            tileOccupied = evaluation?.TileOccupied
                ?? (!result.IsSpatiallyValid && _objectsMapService.IsOccupied(position));
            spacingBlocked = evaluation?.SpacingBlocked ?? false;
            fogBlocked = evaluation?.FogBlocked ?? false;
            influenceZoneBlocked = evaluation?.InfluenceZoneBlocked ?? false;
            terrainBlocked = evaluation?.TerrainBlocked
                ?? (!result.IsSpatiallyValid && IsBlockedByTerrain(position, out _));

            LogPlacementAttempt(
                result,
                emitRejectedAction: !result.IsSpatiallyValid);
            return result.IsSpatiallyValid;
        }

        private string GetObjectOccupantId(Vector2Int position, Vector2Int? ignoredOccupiedPosition = null)
        {
            if (ignoredOccupiedPosition.HasValue && position == ignoredOccupiedPosition.Value)
                return null;

            return _objectsMapService.TryGetOccupant(position, out var occupantId)
                ? occupantId
                : null;
        }

        private IReadOnlyList<BuildingPlacementSimulationEntry> BuildPlacementSimulationEntries()
        {
            if (_placementSimulationSnapshotVersion == _pendingPlacementsVersion)
                return _placementSimulationSnapshot;

            _placementSimulationSnapshotVersion = _pendingPlacementsVersion;
            _placementSimulationSnapshot.Clear();
            for (int index = 0; index < _pendingPlacements.Count; index++)
            {
                var placement = _pendingPlacements[index];
                _placementSimulationSnapshot.Add(new BuildingPlacementSimulationEntry(placement.Position, placement.BuildingId));
            }

            return _placementSimulationSnapshot;
        }
    }
}
