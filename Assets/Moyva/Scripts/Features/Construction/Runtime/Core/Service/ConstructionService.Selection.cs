using System;
using Kruty1918.Moyva.Construction.API;
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

            try
            {
                ClearPendingDemolitionsPreview();
                IsDemolishMode = false;
                _selectedBuildingId = buildingId;
                State = BuildingPlacementState.Placing;

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

            if (VerboseLogs)
                Debug.Log($"[Construction] ToggleDemolishMode -> {IsDemolishMode}");
        }
    }
}
