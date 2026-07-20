using System;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        private bool TryHandleUniqueBuildingPreview(
            Vector2Int targetPosition,
            out bool placementSucceeded)
        {
            placementSucceeded = false;
            if (!TryGetSelectedDefinition(out BuildingDefinition definition))
                return false;

            BuildingPlacementUniquenessScope scope =
                BuildingDefinitionCapabilities.GetPlacementUniquenessScope(definition);
            if (scope == BuildingPlacementUniquenessScope.None)
                return false;

            if (TryFindPendingPlacementByBuildingId(_selectedBuildingId, out int pendingIndex))
            {
                Vector2Int currentPosition = _pendingPlacements[pendingIndex].Position;
                if (VerboseLogs)
                {
                    Debug.Log(
                        $"[Construction] Unique placement '{_selectedBuildingId}' scope={scope} already has " +
                        $"pending preview at {currentPosition}. Redirecting move to {targetPosition}.");
                }

                placementSucceeded = currentPosition == targetPosition
                    || TryMovePendingPlacement(currentPosition, targetPosition);
                return true;
            }

            if (!TryFindPlacedBuildingPosition(
                    _selectedBuildingId,
                    _activeOwnerId,
                    scope,
                    out Vector2Int originalPosition))
            {
                return false;
            }

            if (originalPosition == targetPosition)
            {
                _lastActionMessage =
                    $"Будівля '{_selectedBuildingId}' вже розміщена на клітинці {targetPosition}.";
                LogSyntheticPlacementRejection(
                    ConstructionPlacementAttemptSource.PointerClick,
                    _selectedBuildingId,
                    targetPosition,
                    _activeOwnerId,
                    "unique-building-already-at-target",
                    _lastActionMessage);
                return true;
            }

            ConstructionPlacementQueryResult relocationResult = EvaluatePlacement(
                new ConstructionPlacementQueryRequest(
                    _selectedBuildingId,
                    targetPosition,
                    ignoredOccupiedPosition: originalPosition,
                    includeResources: false,
                    includeDetails: true,
                    ownerId: _activeOwnerId,
                    attemptSource: ConstructionPlacementAttemptSource.PointerClick,
                    allowUniquePreviewRelocation: false));
            if (!relocationResult.IsValid)
            {
                _lastActionMessage = relocationResult.Reason;
                LogPlacementAttempt(relocationResult, emitRejectedAction: true);
                return true;
            }

            if (!AddPendingPlacement(
                    targetPosition,
                    _selectedBuildingId,
                    clearRedoHistory: true,
                    originalPosition))
            {
                return true;
            }

            placementSucceeded = true;
            LogPlacementAttempt(relocationResult, emitRejectedAction: false);
            if (VerboseLogs)
            {
                Debug.Log(
                    $"[Construction] Unique placement '{_selectedBuildingId}' scope={scope} entered " +
                    $"relocation preview {originalPosition} -> {targetPosition}.");
            }

            return true;
        }

        private bool TryGetSelectedDefinition(out BuildingDefinition definition)
        {
            definition = string.IsNullOrWhiteSpace(_selectedBuildingId)
                ? null
                : _buildingRegistry?.GetById(_selectedBuildingId);
            return definition != null;
        }

        private bool TryFindPlacedBuildingPosition(
            string buildingId,
            string ownerId,
            BuildingPlacementUniquenessScope scope,
            out Vector2Int position)
        {
            position = default;
            if (string.IsNullOrWhiteSpace(buildingId)
                || scope == BuildingPlacementUniquenessScope.None)
            {
                return false;
            }

            string normalizedOwnerId = NormalizeOwnerId(ownerId);
            foreach (var pair in _factionPlacedBuildings)
            {
                if (!string.Equals(pair.Value.BuildingId, buildingId, StringComparison.Ordinal))
                    continue;

                if (scope == BuildingPlacementUniquenessScope.PerOwner
                    && !string.Equals(
                        pair.Value.FactionId,
                        normalizedOwnerId,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                position = pair.Key;
                return true;
            }

            if (scope == BuildingPlacementUniquenessScope.PerOwner
                && !string.Equals(
                    normalizedOwnerId,
                    NormalizeOwnerId(_activeOwnerId),
                    StringComparison.Ordinal))
            {
                return false;
            }

            foreach (var pair in _playerPlacedBuildings)
            {
                if (!string.Equals(pair.Value, buildingId, StringComparison.Ordinal))
                    continue;

                position = pair.Key;
                return true;
            }

            return false;
        }

        private bool TryFindPendingPlacementByBuildingId(string buildingId, out int index)
        {
            index = -1;
            if (string.IsNullOrWhiteSpace(buildingId))
                return false;

            for (int i = 0; i < _pendingPlacements.Count; i++)
            {
                if (!string.Equals(
                        _pendingPlacements[i].BuildingId,
                        buildingId,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                index = i;
                return true;
            }

            return false;
        }

        private static bool IsRelocation(in PendingPlacement placement)
            => placement.OriginalPosition.HasValue
                && placement.OriginalPosition.Value != placement.Position;

        private void RemovePlacedRecordAt(Vector2Int position)
        {
            _playerPlacedBuildings.Remove(position);
            _factionPlacedBuildings.Remove(position);
        }
    }
}
