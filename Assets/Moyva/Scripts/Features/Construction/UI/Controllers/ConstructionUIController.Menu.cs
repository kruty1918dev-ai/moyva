using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using TMPro;
using UnityEngine;
using Unity.Profiling;
using Zenject;

namespace Kruty1918.Moyva.Construction.UI
{
    public partial class ConstructionUIController
    {
        private void PopulateBuildingList()
        {
            _buildingListRefreshRequested = false;

            if (selectionPanel == null || _buildingRegistry == null)
                return;

            var buildings = _buildingRegistry.GetAll();
            _buildingUnavailableReasons.Clear();
            var items = _menuFactory.BuildMenuItems(
                buildings,
                _buildingRegistry,
                this,
                IsBuildingAvailable,
                includeSelector:
                    ShouldIncludeBuildingInMenu,
                unavailableReasonSelector:
                    GetBuildingUnavailableReason);

            if (Debug.isDebugBuild)
            {
                Debug.Log(
                    $"{ModuleLogTag} menu rebuilt " +
                    $"items={items.Count} " +
                    $"owner={_constructionService?.GetActiveOwner()}",
                    this);
            }

            int disabledCount = items.Count(item => !item.IsInteractable);
            Debug.Log(
                $"[MoyvaConstructionAvailability] menu-summary " +
                $"items={items.Count} enabled={items.Count - disabledCount} " +
                $"disabled={disabledCount} owner='{_constructionService.GetActiveOwner()}' " +
                $"pending={_constructionService.GetPendingPlacements().Count}",
                this);

            selectionPanel.Populate(items);
        }

        private bool ShouldIncludeBuildingInMenu(
            BuildingDefinition definition)
        {
            if (definition == null
                || string.IsNullOrWhiteSpace(definition.Id))
            {
                return false;
            }

            if (_constructionService
                is not IConstructionBootstrapQuery bootstrap)
            {
                return true;
            }

            string ownerId =
                _constructionService.GetActiveOwner();
            if (bootstrap.RequiresInitialCastle(
                    ownerId,
                    out string requiredCastleId))
            {
                return string.Equals(
                    definition.Id,
                    requiredCastleId,
                    StringComparison.Ordinal);
            }

            return !bootstrap.IsCastleBuilding(
                definition.Id);
        }

        private void SynchronizeCastleBootstrapUi()
        {
            if (_constructionService
                is not IConstructionBootstrapQuery bootstrap)
            {
                return;
            }

            string ownerId =
                _constructionService.GetActiveOwner();
            if (bootstrap.RequiresInitialCastle(
                    ownerId,
                    out string castleBuildingId))
            {
                _selectedBuildingId =
                    _constructionService.GetSelectedBuildingId();

                if (string.IsNullOrWhiteSpace(_selectedBuildingId))
                {
                    _constructionService.SelectBuilding(castleBuildingId);
                    _selectedBuildingId =
                        _constructionService.GetSelectedBuildingId();
                }

                selectionPanel?.SetSelectedBuilding(
                    _selectedBuildingId);

                if (Debug.isDebugBuild)
                {
                    Debug.Log(
                        $"{ModuleLogTag} castle-bootstrap-ui " +
                        $"owner={ownerId} required=true " +
                        $"selected={_selectedBuildingId ?? "none"}",
                        this);
                }
                return;
            }

            if (!string.IsNullOrWhiteSpace(_selectedBuildingId)
                && bootstrap.IsCastleBuilding(_selectedBuildingId))
            {
                _selectedBuildingId = null;
                selectionPanel?.ClearSelection();
                HidePreviewInfoPanel();
            }
        }

        private bool IsBuildingAvailable(BuildingDefinition definition)
        {
            if (definition == null
                || string.IsNullOrWhiteSpace(definition.Id))
            {
                return false;
            }

            if (!TryGetSelectionAvailability(
                    definition.Id,
                    out ConstructionSelectionAvailabilityResult result))
            {
                // Compatibility fallback if an isolated test scene binds an
                // older construction service without the optional query.
                if (_placementQuery == null)
                    return true;

                ConstructionPlacementQueryResult legacy =
                    _placementQuery.EvaluatePlacement(
                        new ConstructionPlacementQueryRequest(
                            definition.Id,
                            _lastPreviewPosition,
                            includeResources: false,
                            ownerId: _constructionService?.GetActiveOwner(),
                            attemptSource:
                                ConstructionPlacementAttemptSource.Unknown,
                            allowUniquePreviewRelocation: false));
                _buildingUnavailableReasons[definition.Id] =
                    legacy.CanSelect ? null : legacy.Reason;
                return legacy.CanSelect;
            }

            _buildingUnavailableReasons[definition.Id] =
                result.CanSelect
                    ? null
                    : ConstructionPlacementReasonText.Resolve(
                        result.ReasonCode,
                        result.Reason);

            return result.CanSelect;
        }

        private bool TryGetSelectionAvailability(
            string buildingId,
            out ConstructionSelectionAvailabilityResult result)
        {
            if (_constructionService
                is IConstructionSelectionAvailabilityQuery availabilityQuery)
            {
                result = availabilityQuery.EvaluateSelectionAvailability(
                    buildingId,
                    _constructionService.GetActiveOwner(),
                    _lastPreviewPosition,
                    includePendingPlacements: true);
                return true;
            }

            result = default;
            return false;
        }

        private string GetBuildingUnavailableReason(
            BuildingDefinition definition)
        {
            if (definition == null
                || string.IsNullOrWhiteSpace(definition.Id))
            {
                return "Некоректна конфігурація будівлі.";
            }

            return _buildingUnavailableReasons.TryGetValue(
                definition.Id,
                out string reason)
                ? reason
                : null;
        }

        private void RefreshUI()
        {
            var state = new ConstructionUIState(
                _constructionService.State,
                _selectedBuildingId,
                _lastPreviewState,
                _lastPreviewPosition,
                _constructionService.IsDemolishMode,
                _isConstructionModeActive);

            if (actionBar != null)
                actionBar.SetState(state);

            if (statusDisplay != null)
                statusDisplay.UpdateState(state);

            if (_isConstructionModeActive && string.IsNullOrWhiteSpace(_selectedBuildingId) && !_isPreviewInfoPinned)
                HidePreviewInfoPanel();
        }

    }
}
