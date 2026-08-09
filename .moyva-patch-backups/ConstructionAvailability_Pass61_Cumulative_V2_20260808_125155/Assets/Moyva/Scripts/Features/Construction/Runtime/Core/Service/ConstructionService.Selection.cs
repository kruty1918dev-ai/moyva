using System;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        public void SelectBuilding(string buildingId)
        {
            Debug.Log($"[Construction] SelectBuilding('{buildingId}') викликана. active={_isActive}");

            if (!_isActive)
            {
                Debug.LogWarning("[Construction] SelectBuilding: Construction mode ВИМКНЕНА");
                return;
            }

            if (string.IsNullOrWhiteSpace(buildingId))
            {
                Debug.LogWarning("[Construction] SelectBuilding: buildingId порожня");
                return;
            }

            BuildingDefinition selectedDefinition =
                _placementBuildingRegistry.GetById(buildingId);
            if (selectedDefinition == null)
            {
                Debug.LogWarning(
                    $"[MoyvaBuildGridDiag] selection-rejected " +
                    $"building='{buildingId}' reason='definition-missing'");
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
                Debug.Log(
                    $"{ModuleLogTag} selection blocked " +
                    $"owner={_activeOwnerId} requested={buildingId} " +
                    $"requiredCastle={requiredCastleId}");
                return;
            }

            if (BuildingDefinitionCapabilities.IsCastle(selectedDefinition)
                && HasPlacedCastleForOwner(
                    NormalizeOwnerId(_activeOwnerId)))
            {
                _lastActionMessage =
                    "Замок цього гравця вже побудований.";
                Debug.Log(
                    $"{ModuleLogTag} castle selection blocked " +
                    $"owner={_activeOwnerId} building={buildingId}");
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
                Debug.LogWarning(
                    $"[MoyvaConstructionAvailability] selection-rejected " +
                    $"building='{buildingId}' owner='{NormalizeOwnerId(_activeOwnerId)}' " +
                    $"code='{selectionAvailability.ReasonCode ?? "unavailable"}' " +
                    $"reason='{_lastActionMessage}'");
                return;
            }

            try
            {
                ClearPendingDemolitionsPreview();
                IsDemolishMode = false;
                SetPlacementSelection(buildingId, BuildingPlacementState.Placing);

                Debug.Log($"[Construction] ✓ SelectBuilding -> id='{_selectedBuildingId}', state={State}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Construction] ПОМИЛКА в SelectBuilding('{buildingId}'): {ex.GetType().Name} - {ex.Message}");
            }
        }

        public string GetSelectedBuildingId()
        {
            return _selectedBuildingId;
        }

        public void SetActiveOwner(string ownerId)
        {
            _activeOwnerId =
                string.IsNullOrWhiteSpace(ownerId)
                    ? DefaultOwnerId
                    : ownerId.Trim();

            if (_isActive)
                ApplyBootstrapCastleSelectionIfNeeded();
        }

        public bool TryGetPlacedBuildingOwner(
            Vector2Int position,
            out string ownerId)
        {
            ownerId = null;
            Vector2Int origin =
                _placedOriginByOccupiedTile.TryGetValue(
                    position,
                    out Vector2Int resolvedOrigin)
                    ? resolvedOrigin
                    : position;

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
            if (!RequiresInitialCastle(
                    _activeOwnerId,
                    out string castleBuildingId))
            {
                return;
            }

            IsDemolishMode = false;
            SetPlacementSelection(
                castleBuildingId,
                BuildingPlacementState.Placing);

            Debug.Log(
                $"{ModuleLogTag} castle-bootstrap " +
                $"owner={_activeOwnerId} required=true " +
                $"castle={castleBuildingId}");
        }

        public string GetActiveOwner()
        {
            return _activeOwnerId;
        }

        public void ToggleDemolishMode()
        {
            if (!_isActive)
            {
                Debug.LogWarning("[Construction] ToggleDemolishMode called outside Construction mode.");
                return;
            }

            IsDemolishMode = !IsDemolishMode;

            if (!IsDemolishMode)
                ClearPendingDemolitionsPreview();

            PublishSelectionChanged();

            if (VerboseLogs)
                Debug.Log($"[Construction] ToggleDemolishMode -> {IsDemolishMode}");
        }

        private void PublishSelectionChanged()
        {
            _signalBus?.Fire(new BuildingSelectionChangedSignal
            {
                BuildingId = _selectedBuildingId,
                IsDemolishMode = IsDemolishMode
            });
        }

        private bool SetPlacementSelection(string buildingId, BuildingPlacementState state)
        {
            string normalizedId = string.IsNullOrWhiteSpace(buildingId) ? null : buildingId.Trim();
            bool changed = State != state
                || !string.Equals(_selectedBuildingId, normalizedId, StringComparison.Ordinal);

            State = state;
            _selectedBuildingId = normalizedId;
            if (!changed)
                return false;

            PublishSelectionChanged();
            if (VerboseLogs)
                Debug.Log($"[MoyvaBuildGridDiag] selection state={State} building='{_selectedBuildingId ?? "none"}' demolish={IsDemolishMode}");
            return true;
        }
    }
}
