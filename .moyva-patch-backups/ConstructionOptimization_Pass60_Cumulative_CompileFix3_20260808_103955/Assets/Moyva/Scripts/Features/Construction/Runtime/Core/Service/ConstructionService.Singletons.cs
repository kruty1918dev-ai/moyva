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
            if (!TryGetSelectedDefinition(
                    out BuildingDefinition definition))
            {
                return false;
            }

            if (BuildingDefinitionCapabilities
                    .IsStrictPerOwnerUnique(definition))
            {
                if (TryFindPendingPlacementByBuildingId(
                        _selectedBuildingId,
                        out int strictPendingIndex))
                {
                    Vector2Int pendingPosition =
                        _pendingPlacements[strictPendingIndex].Position;
                    placementSucceeded =
                        pendingPosition == targetPosition
                        || TryMovePendingPlacement(
                            pendingPosition,
                            targetPosition);
                    return true;
                }

                if (TryFindOwnedPlacedBuildingPosition(
                        _selectedBuildingId,
                        _activeOwnerId,
                        out Vector2Int existingCastlePosition))
                {
                    _lastActionMessage =
                        $"Замок уже побудований на {existingCastlePosition}. " +
                        "Другий замок для цього гравця заборонений.";
                    Debug.Log(
                        $"{ModuleLogTag} castle-placement blocked " +
                        $"owner={_activeOwnerId} " +
                        $"existing={existingCastlePosition}");
                    return true;
                }

                return false;
            }

            bool hasExplicitLimit =
                BuildingDefinitionCapabilities.TryGetEnabledModule(
                    definition,
                    out BuildingPerPlayerLimitModule limitModule)
                && Mathf.Max(0, limitModule.MaxBuildingsPerPlayer) > 0
                && limitModule.OverflowPolicy
                    != BuildingLimitOverflowPolicy.Legacy;
            if (hasExplicitLimit)
            {
                if (limitModule.OverflowPolicy
                        == BuildingLimitOverflowPolicy.Block
                    || !IsBuildingLimitAtCapacity(
                        _selectedBuildingId,
                        _activeOwnerId,
                        limitModule))
                {
                    return false;
                }

                if (TryFindPendingPlacementByBuildingId(
                        _selectedBuildingId,
                        out int overflowPendingIndex))
                {
                    Vector2Int pendingPosition =
                        _pendingPlacements[overflowPendingIndex].Position;
                    placementSucceeded = pendingPosition == targetPosition
                        || TryMovePendingPlacement(
                            pendingPosition,
                            targetPosition);
                    return true;
                }

                if (limitModule.OverflowPolicy
                    == BuildingLimitOverflowPolicy.MovePending)
                {
                    // An already committed item is not movable under MovePending.
                    // Continue through the normal query so the limit is reported
                    // as a global availability blocker.
                    return false;
                }

                if (!TryFindOwnedPlacedBuildingPosition(
                        _selectedBuildingId,
                        _activeOwnerId,
                        out Vector2Int ownedOriginalPosition))
                {
                    // A global limit may be consumed entirely by other owners.
                    // RelocateExisting never grants ownership of their buildings.
                    return false;
                }

                BuildingPlacementUniquenessScope explicitScope =
                    limitModule.LimitScope == BuildingLimitScope.Global
                        ? BuildingPlacementUniquenessScope.Global
                        : BuildingPlacementUniquenessScope.PerOwner;
                return TryStartPlacedBuildingRelocation(
                    targetPosition,
                    ownedOriginalPosition,
                    explicitScope,
                    out placementSucceeded);
            }

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

            return TryStartPlacedBuildingRelocation(
                targetPosition,
                originalPosition,
                scope,
                out placementSucceeded);
        }

        private bool TryStartPlacedBuildingRelocation(
            Vector2Int targetPosition,
            Vector2Int originalPosition,
            BuildingPlacementUniquenessScope scope,
            out bool placementSucceeded)
        {
            placementSucceeded = false;
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
            if (!relocationResult.CanPreview)
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

        private bool IsBuildingLimitAtCapacity(
            string buildingId,
            string ownerId,
            BuildingPerPlayerLimitModule limitModule)
        {
            int limit = Mathf.Max(
                0,
                limitModule?.MaxBuildingsPerPlayer ?? 0);
            if (limit <= 0 || string.IsNullOrWhiteSpace(buildingId))
                return false;

            string normalizedOwnerId = NormalizeOwnerId(ownerId);
            var request = new ConstructionPlacementQueryRequest(
                buildingId,
                default,
                ownerId: normalizedOwnerId,
                includePendingPlacements: true);
            int existingCount = limitModule.LimitScope
                == BuildingLimitScope.Global
                    ? CountPlacedBuildingsGlobally(buildingId, null)
                    : CountPlacedBuildingsForOwner(
                        buildingId,
                        normalizedOwnerId,
                        null);
            int pendingCount = limitModule.LimitScope
                == BuildingLimitScope.Global
                    ? CountPendingBuildings(request)
                    : CountPendingBuildingsForOwner(
                        request,
                        normalizedOwnerId);
            return existingCount + pendingCount >= limit;
        }

        private bool TryFindOwnedPlacedBuildingPosition(
            string buildingId,
            string ownerId,
            out Vector2Int position)
        {
            position = default;
            if (string.IsNullOrWhiteSpace(buildingId))
                return false;

            string normalizedOwnerId = NormalizeOwnerId(ownerId);
            foreach (var pair in _factionPlacedBuildings)
            {
                if (string.Equals(
                        pair.Value.BuildingId,
                        buildingId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        pair.Value.FactionId,
                        normalizedOwnerId,
                        StringComparison.Ordinal))
                {
                    position = pair.Key;
                    return true;
                }
            }

            if (!string.Equals(
                    normalizedOwnerId,
                    NormalizeOwnerId(_activeOwnerId),
                    StringComparison.Ordinal))
            {
                return false;
            }

            foreach (var pair in _playerPlacedBuildings)
            {
                if (!string.Equals(
                        pair.Value,
                        buildingId,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                position = pair.Key;
                return true;
            }

            return false;
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
