using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        public void SelectBuilding(string buildingId)
        {
            _lastActionMessage = string.Empty;
            if (!_isActive)
            {
                LogSelectionRejected(buildingId, "Construction mode is not active.");
                return;
            }

            if (!CanActiveOwnerMutate(
                    "select building",
                    out string turnReason))
            {
                _lastActionMessage = turnReason;
                LogSelectionRejected(buildingId, turnReason);
                return;
            }

            if (string.IsNullOrWhiteSpace(buildingId))
            {
                LogSelectionRejected(buildingId, "Building id is empty.");
                return;
            }

            BuildingDefinition selectedDefinition =
                _placementBuildingRegistry.GetById(buildingId);
            if (selectedDefinition == null)
            {
                LogSelectionRejected(
                    buildingId,
                    $"Building definition '{buildingId}' is missing.");
                return;
            }

            if (RequiresInitialCastle(
                    _activeOwnerId,
                    out string requiredCastleId)
                && !string.Equals(
                    buildingId,
                    requiredCastleId,
                    StringComparison.Ordinal))
            {
                _lastActionMessage =
                    "Спочатку потрібно побудувати замок.";
                LogSelectionRejected(buildingId, _lastActionMessage);
                return;
            }

            if (BuildingDefinitionCapabilities.IsCastle(selectedDefinition)
                && HasPlacedCastleForOwner(
                    NormalizeOwnerId(_activeOwnerId)))
            {
                _lastActionMessage =
                    "Замок цього гравця вже побудований.";
                LogSelectionRejected(buildingId, _lastActionMessage);
                return;
            }

            ConstructionSelectionAvailabilityResult selectionAvailability =
                EvaluateSelectionAvailability(
                    buildingId,
                    _activeOwnerId,
                    preferredFundingPosition: null,
                    includePendingPlacements: true);
            if (!selectionAvailability.CanSelect)
            {
                _lastActionMessage = string.IsNullOrWhiteSpace(
                    selectionAvailability.Reason)
                    ? "Будівля зараз недоступна."
                    : selectionAvailability.Reason;
                LogSelectionRejected(buildingId, _lastActionMessage);
                return;
            }

            try
            {
                ClearPendingDemolitionsPreview();
                IsDemolishMode = false;
                SetPlacementSelection(buildingId, BuildingPlacementState.Placing);
            }
            catch (Exception ex)
            {
                _lastActionMessage = ex.Message;
                Debug.LogError($"[Construction] ПОМИЛКА в SelectBuilding('{buildingId}'): {ex.GetType().Name} - {ex.Message}");
            }
        }

        private void LogSelectionRejected(string buildingId, string reason)
        {
            _lastActionMessage = reason;
            if (!Application.isEditor && !Debug.isDebugBuild)
                return;

            string message = string.IsNullOrWhiteSpace(reason)
                ? "Selection was rejected without a reason."
                : reason;
            Debug.LogWarning(
                $"[Construction] Selection rejected for '{buildingId}', owner='{_activeOwnerId}': {message}");
        }

        public string GetSelectedBuildingId()
        {
            return _selectedBuildingId;
        }

        public ConstructionRotation SelectedRotation =>
            _selectedRotation;

        public bool RotateSelectedClockwise()
        {
            _lastActionMessage = string.Empty;
            int targetIndex = -1;
            for (int index = _pendingPlacements.Count - 1; index >= 0; index--)
                if (string.IsNullOrWhiteSpace(_selectedBuildingId)
                    || string.Equals(_pendingPlacements[index].BuildingId, _selectedBuildingId, StringComparison.Ordinal))
                { targetIndex = index; break; }
            string targetBuildingId = targetIndex >= 0
                ? _pendingPlacements[targetIndex].BuildingId : _selectedBuildingId;
            if (!_isActive
                || (targetIndex < 0 && State != BuildingPlacementState.Placing)
                || string.IsNullOrWhiteSpace(targetBuildingId)
                || IsDemolishMode)
            {
                _lastActionMessage = "Select a building or pending placement to rotate.";
                return false;
            }

            if (!CanActiveOwnerMutate(
                    "rotate selected building",
                    out string turnReason))
            {
                _lastActionMessage = turnReason;
                return false;
            }

            if (_wallTopologyService?.IsWallOrGate(
                    targetBuildingId) == true)
            {
                _lastActionMessage =
                    "Напрямок стін і воріт визначається сусідніми сегментами.";
                return false;
            }

            ConstructionRotation next = ConstructionRotationUtility.NextClockwise(_selectedRotation);
            if (targetIndex >= 0)
            {
                PendingPlacement placement = _pendingPlacements[targetIndex];
                next = ConstructionRotationUtility.NextClockwise(placement.Rotation);
                var result = EvaluatePlacement(new ConstructionPlacementQueryRequest(
                    placement.BuildingId, placement.Position, placement.Position, placement.OriginalPosition,
                    includeResources: true, includeDetails: true, ownerId: _activeOwnerId,
                    allowUniquePreviewRelocation: false,
                    satisfiedReplacementBuildingId: placement.ReplacedPendingBuildingId, rotation: next));
                if (!result.CanPreview || !result.ResourcesValid)
                { _lastActionMessage = result.Reason; return false; }
                SaveSnapshotForUndo(clearRedoHistory: true);
                var rotated = new PendingPlacement(placement.Position, placement.BuildingId,
                    placement.OriginalPosition, placement.ReplacedPendingBuildingId, next);
                _pendingPlacements[targetIndex] = rotated;
                _pendingPlacementByPosition[placement.Position] = rotated;
                MarkPendingPlacementsChanged();
                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = placement.Position, BuildingId = placement.BuildingId,
                    RotationQuarterTurns = (int)next, PreviewState = BuildingPreviewState.Valid,
                });
            }
            _selectedRotation = next;
            _lastActionMessage = string.Empty;
            PublishSelectionChanged();
            return true;
        }

        public bool TryGetPendingRotation(
            Vector2Int position,
            out ConstructionRotation rotation)
        {
            if (_pendingPlacementByPosition.TryGetValue(
                    position,
                    out PendingPlacement placement))
            {
                rotation = placement.Rotation;
                return true;
            }

            rotation = ConstructionRotation.Degrees0;
            return false;
        }

        public void SetActiveOwner(string ownerId)
        {
            string nextOwnerId =
                string.IsNullOrWhiteSpace(ownerId)
                    ? DefaultOwnerId
                    : ownerId.Trim();

            if (string.Equals(
                    _activeOwnerId,
                    nextOwnerId,
                    StringComparison.Ordinal))
            {
                return;
            }

            // Pending previews, demolition marks and undo/redo history belong to
            // exactly one turn owner. Never carry them into the next faction turn.
            if (_isActive)
                ResetSession(clearRedoHistory: true);

            _activeOwnerId = nextOwnerId;

            if (_isActive)
                ApplyBootstrapCastleSelectionIfNeeded();
        }

        public bool TryGetPlacedBuildingOwner(
            Vector2Int position,
            out string ownerId)
        {
            ownerId = null;
            Vector2Int origin =
                _footprints.ResolveOrigin(position);

            if (_factionPlacedBuildings.TryGetValue(
                    origin,
                    out var factionPlacement))
            {
                ownerId = NormalizeOwnerId(
                    factionPlacement.FactionId);
                return true;
            }

            if (_playerPlacedBuildings.ContainsKey(origin))
            {
                ownerId = NormalizeOwnerId(_activeOwnerId);
                return true;
            }

            return false;
        }

        public bool IsCastleBuilding(string buildingId)
        {
            BuildingDefinition definition =
                string.IsNullOrWhiteSpace(buildingId)
                    ? null
                    : _placementBuildingRegistry?.GetById(buildingId);
            return BuildingDefinitionCapabilities.IsCastle(definition);
        }

        public bool RequiresInitialCastle(
            string ownerId,
            out string castleBuildingId)
        {
            castleBuildingId = null;
            if (!TryResolvePrimaryCastleBuildingId(
                    out string primaryCastleId))
            {
                return false;
            }

            castleBuildingId = primaryCastleId;
            return !HasPlacedCastleForOwner(
                NormalizeOwnerId(ownerId));
        }

        private bool TryResolvePrimaryCastleBuildingId(
            out string castleBuildingId)
        {
            castleBuildingId = null;
            BuildingDefinition[] definitions =
                _placementBuildingRegistry?.GetAll();
            if (definitions == null)
                return false;

            for (int index = 0;
                 index < definitions.Length;
                 index++)
            {
                BuildingDefinition definition = definitions[index];
                if (definition == null
                    || string.IsNullOrWhiteSpace(definition.Id)
                    || !BuildingDefinitionCapabilities.IsCastle(definition))
                {
                    continue;
                }

                castleBuildingId = definition.Id;
                return true;
            }

            return false;
        }

        private bool HasPlacedCastleForOwner(string ownerId)
        {
            string normalizedOwnerId =
                NormalizeOwnerId(ownerId);

            foreach (var pair in _factionPlacedBuildings)
            {
                if (!string.Equals(
                        NormalizeOwnerId(pair.Value.FactionId),
                        normalizedOwnerId,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                if (IsCastleBuilding(pair.Value.BuildingId))
                    return true;
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
                if (IsCastleBuilding(pair.Value))
                    return true;
            }

            return false;
        }

        private void ApplyBootstrapCastleSelectionIfNeeded()
        {
            if (_turns != null
                && !IsLocalConstructionOwner(_activeOwnerId))
            {
                return;
            }

            if (!RequiresInitialCastle(
                    _activeOwnerId,
                    out string castleBuildingId))
            {
                return;
            }

            SelectBuilding(castleBuildingId);
        }

        public string GetActiveOwner()
        {
            return _activeOwnerId;
        }

        public void ToggleDemolishMode()
        {
            if (!_isActive)
            {
                return;
            }

            if (!CanActiveOwnerMutate(
                    "toggle demolition mode",
                    out string turnReason))
            {
                _lastActionMessage = turnReason;
                return;
            }

            IsDemolishMode = !IsDemolishMode;

            if (!IsDemolishMode)
                ClearPendingDemolitionsPreview();

            PublishSelectionChanged();
        }

        private bool RevalidateActiveSelectionAvailability(
            string trigger,
            Vector2Int? preferredFundingPosition = null)
        {
            if (!_isActive
                || IsDemolishMode
                || State != BuildingPlacementState.Placing
                || string.IsNullOrWhiteSpace(_selectedBuildingId))
            {
                return true;
            }

            string selectedBuildingId = _selectedBuildingId;
            ConstructionSelectionAvailabilityResult availability =
                EvaluateSelectionAvailability(
                    selectedBuildingId,
                    _activeOwnerId,
                    preferredFundingPosition,
                    includePendingPlacements: true);

            if (availability.CanSelect
                || CanContinueSelectionAtCapacity(
                    selectedBuildingId,
                    availability))
            {
                return true;
            }

            _lastActionMessage =
                string.IsNullOrWhiteSpace(availability.Reason)
                    ? "Будівля більше недоступна для нового розміщення."
                    : availability.Reason;

            SetPlacementSelection(
                null,
                BuildingPlacementState.Placing);
            return false;
        }

        private bool CanContinueSelectionAtCapacity(
            string buildingId,
            ConstructionSelectionAvailabilityResult availability)
        {
            if (!string.Equals(
                    availability.ReasonCode,
                    "per-player-limit",
                    StringComparison.Ordinal))
            {
                return false;
            }

            BuildingDefinition definition =
                _placementBuildingRegistry?.GetById(buildingId);
            if (!BuildingDefinitionCapabilities.TryGetEnabledModule(
                    definition,
                    out BuildingPerPlayerLimitModule limitModule))
            {
                return false;
            }

            switch (limitModule.OverflowPolicy)
            {
                case BuildingLimitOverflowPolicy.MovePending:
                    return TryFindPendingPlacementByBuildingId(
                        buildingId,
                        out _);

                case BuildingLimitOverflowPolicy.RelocateExisting:
                    return TryFindPendingPlacementByBuildingId(
                               buildingId,
                               out _)
                           || TryFindOwnedPlacedBuildingPosition(
                               buildingId,
                               _activeOwnerId,
                               out _);

                default:
                    return false;
            }
        }

        private void PublishSelectionChanged()
        {
            _signalBus?.Fire(new BuildingSelectionChangedSignal
            {
                BuildingId = _selectedBuildingId,
                IsDemolishMode = IsDemolishMode,
                RotationQuarterTurns = (int)_selectedRotation,
            });
        }

        private bool SetPlacementSelection(string buildingId, BuildingPlacementState state)
        {
            string normalizedId = string.IsNullOrWhiteSpace(buildingId) ? null : buildingId.Trim();
            bool changed = State != state
                || !string.Equals(_selectedBuildingId, normalizedId, StringComparison.Ordinal);

            if (!string.Equals(
                    _selectedBuildingId,
                    normalizedId,
                    StringComparison.Ordinal))
            {
                _selectedRotation = ConstructionRotation.Degrees0;
            }

            State = state;
            _selectedBuildingId = normalizedId;
            if (!changed)
                return false;

            PublishSelectionChanged();
            return true;
        }
    }
}
