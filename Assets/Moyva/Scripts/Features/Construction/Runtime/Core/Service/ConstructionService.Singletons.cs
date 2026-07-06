using System;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        private bool TryHandleSingletonPreview(Vector2Int targetPosition)
        {
            if (!TryGetSelectedDefinition(out BuildingDefinition definition)
                || !BuildingDefinitionCapabilities.IsGlobalSingleton(definition))
            {
                return false;
            }

            if (TryFindPendingPlacementByBuildingId(_selectedBuildingId, out int pendingIndex))
            {
                Vector2Int currentPosition = _pendingPlacements[pendingIndex].Position;
                if (VerboseLogs)
                    Debug.Log($"[Construction] Global singleton '{_selectedBuildingId}' already has pending preview at {currentPosition}. Redirecting move to {targetPosition}.");
                return currentPosition == targetPosition || TryMovePendingPlacement(currentPosition, targetPosition);
            }

            if (!TryFindPlacedBuildingPosition(_selectedBuildingId, out Vector2Int originalPosition))
                return false;

            if (originalPosition == targetPosition)
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] Global singleton '{_selectedBuildingId}' already placed at {targetPosition}. Waiting for a different relocation target.");
                return false;
            }

            if (!CanPlaceAt(targetPosition, null, _selectedBuildingId, out var tileOccupied, out var spacingBlocked, out var fogBlocked, out var influenceZoneBlocked, out var terrainBlocked, ignoredOccupiedPosition: originalPosition))
            {
                if (VerboseLogs)
                {
                    Debug.Log(
                        $"[Construction] Singleton relocation '{_selectedBuildingId}' {originalPosition} -> {targetPosition} blocked. " +
                        $"occupied={tileOccupied}, spacing={spacingBlocked}, fog={fogBlocked}, influence={influenceZoneBlocked}, terrain={terrainBlocked}");
                }
                return false;
            }

            if (!AddPendingPlacement(targetPosition, _selectedBuildingId, clearRedoHistory: true, originalPosition))
                return false;

            if (VerboseLogs)
                Debug.Log($"[Construction] Global singleton '{_selectedBuildingId}' entered relocation preview {originalPosition} -> {targetPosition}.");
            return true;
        }

        private bool TryGetSelectedDefinition(out BuildingDefinition definition)
        {
            definition = string.IsNullOrWhiteSpace(_selectedBuildingId) ? null : _buildingRegistry?.GetById(_selectedBuildingId);
            return definition != null;
        }

        private bool TryFindPlacedBuildingPosition(string buildingId, out Vector2Int position)
        {
            position = default;
            if (string.IsNullOrWhiteSpace(buildingId))
                return false;

            foreach (var pair in _factionPlacedBuildings)
            {
                if (string.Equals(pair.Value.BuildingId, buildingId, StringComparison.Ordinal))
                {
                    position = pair.Key;
                    return true;
                }
            }

            foreach (var pair in _playerPlacedBuildings)
            {
                if (string.Equals(pair.Value, buildingId, StringComparison.Ordinal))
                {
                    position = pair.Key;
                    return true;
                }
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
                if (!string.Equals(_pendingPlacements[i].BuildingId, buildingId, StringComparison.Ordinal))
                    continue;

                index = i;
                return true;
            }

            return false;
        }

        private static bool IsRelocation(in PendingPlacement placement)
            => placement.OriginalPosition.HasValue && placement.OriginalPosition.Value != placement.Position;

        private void RemovePlacedRecordAt(Vector2Int position)
        {
            _playerPlacedBuildings.Remove(position);
            _factionPlacedBuildings.Remove(position);
        }
    }
}
