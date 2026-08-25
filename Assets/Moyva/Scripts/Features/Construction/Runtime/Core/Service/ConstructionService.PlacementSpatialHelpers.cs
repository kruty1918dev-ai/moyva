using Kruty1918.Moyva.Construction.API;
using System;
using System.Collections.Generic;
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
            Vector2Int? ignoredOccupiedPosition = null,
            string satisfiedReplacementBuildingId = null,
            ConstructionRotation rotation =
                ConstructionRotation.Degrees0)
        {
            var fastQuery = new ConstructionPlacementQueryRequest(
                buildingId,
                position,
                ignoredPendingPosition,
                ignoredOccupiedPosition,
                includeResources: false,
                includeDetails: false,
                ownerId: _activeOwnerId,
                attemptSource:
                    ConstructionPlacementAttemptSource.Confirm,
                allowUniquePreviewRelocation: false,
                satisfiedReplacementBuildingId:
                    satisfiedReplacementBuildingId,
                rotation: rotation);

            ConstructionPlacementQueryResult fastResult =
                EvaluatePlacement(fastQuery);

            bool fastAccepted =
                fastResult.AvailabilityValid
                && fastResult.SpatialValid
                && fastResult.AuthorityValid;

            if (fastAccepted)
            {
                tileOccupied = false;
                spacingBlocked = false;
                fogBlocked = false;
                influenceZoneBlocked = false;
                terrainBlocked = false;

                LogPlacementAttempt(
                    fastResult,
                    emitRejectedAction: false);
                return true;
            }

            var detailedQuery = new ConstructionPlacementQueryRequest(
                buildingId,
                position,
                ignoredPendingPosition,
                ignoredOccupiedPosition,
                includeResources: false,
                includeDetails: true,
                ownerId: _activeOwnerId,
                attemptSource:
                    ConstructionPlacementAttemptSource.Confirm,
                allowUniquePreviewRelocation: false,
                satisfiedReplacementBuildingId:
                    satisfiedReplacementBuildingId,
                rotation: rotation);

            ConstructionPlacementQueryResult result =
                EvaluatePlacement(detailedQuery);

            BuildingPlacementEvaluationResult evaluation =
                result.EvaluationResult;

            tileOccupied = evaluation?.TileOccupied
                ?? (!result.IsSpatiallyValid
                    && _objectsMapService.IsOccupied(position));
            spacingBlocked =
                evaluation?.SpacingBlocked ?? false;
            fogBlocked =
                evaluation?.FogBlocked ?? false;
            influenceZoneBlocked =
                evaluation?.InfluenceZoneBlocked ?? false;
            terrainBlocked = evaluation?.TerrainBlocked
                ?? (!result.IsSpatiallyValid
                    && _placementEnvironmentRules.IsBlockedByTerrain(position, out _));

            LogPlacementAttempt(
                result,
                emitRejectedAction: !result.CanCommit);

            return result.AvailabilityValid
                && result.SpatialValid
                && result.AuthorityValid;
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
                _placementSimulationSnapshot.Add(
                    new BuildingPlacementSimulationEntry(
                        placement.Position,
                        placement.BuildingId,
                        NormalizeOwnerId(_activeOwnerId),
                        placement.Rotation));
            }

            return _placementSimulationSnapshot;
        }

        private IReadOnlyList<BuildingPlacementSimulationEntry>
            BuildPlacedBuildingSimulationEntries()
        {
            _placedBuildingSimulationSnapshot.Clear();

            foreach (var pair in _factionPlacedBuildings)
            {
                _placedBuildingSimulationSnapshot.Add(
                    new BuildingPlacementSimulationEntry(
                        pair.Key,
                        pair.Value.BuildingId,
                        NormalizeOwnerId(pair.Value.FactionId),
                        ResolvePlacedRotation(pair.Key)));
            }

            foreach (var pair in _playerPlacedBuildings)
            {
                _placedBuildingSimulationSnapshot.Add(
                    new BuildingPlacementSimulationEntry(
                        pair.Key,
                        pair.Value,
                        NormalizeOwnerId(_activeOwnerId),
                        ResolvePlacedRotation(pair.Key)));
            }

            return _placedBuildingSimulationSnapshot;
        }
    }
}
