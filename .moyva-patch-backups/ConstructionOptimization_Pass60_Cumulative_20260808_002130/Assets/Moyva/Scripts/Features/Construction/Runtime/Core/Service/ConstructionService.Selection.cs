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

            if (_placementBuildingRegistry.GetById(buildingId) == null)
            {
                Debug.LogWarning($"[MoyvaBuildGridDiag] selection-rejected building='{buildingId}' reason='definition-missing'");
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
            _activeOwnerId = string.IsNullOrWhiteSpace(ownerId) ? DefaultOwnerId : ownerId.Trim();
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
