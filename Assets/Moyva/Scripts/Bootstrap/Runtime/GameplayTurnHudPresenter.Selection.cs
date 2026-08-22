using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.UIActions.API;
using Kruty1918.Moyva.Units.API;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed partial class GameplayTurnHudPresenter
    {
        private bool IsSignalForSelectedBuilding(string ownerId, Vector2Int position)
        {
            return _selectedBuilding.HasValue
                && _selectedBuilding.Value == position
                && string.Equals(
                    ownerId?.Trim(),
                    _turns.LocalOwnerId?.Trim(),
                    StringComparison.Ordinal);
        }

        private void OnUnitSelected(UnitInfoPanelRequestedSignal signal)
        {
            _selectedUnitId = signal.UnitId;
            _selectedBuilding = null;
            _selectedBuildingId = null;
            _selectedRecruitmentModule = null;
            _selectedRecruitmentRecipe = null;
            _selectedRecipeIndex = -1;
            _selectionFromQueue = false;
            _selectedQueueId = 0;
            _statusOverride = null;
            _recruitmentPanel.SetActive(false);
            _recruitmentView.SetSelectedRecipeIndex(-1);
            _recruitmentView.SetHeader(string.Empty, string.Empty);
        }

        private void OnBuildingSelected(BuildingInfoPanelRequestedSignal signal)
        {
            _selectedUnitId = null;
            _selectedBuilding = signal.Position;
            _selectedBuildingId = signal.BuildingId;
            _statusOverride = null;
            BuildRecipeButtons(signal.BuildingId);
        }

        private void OnSelectionClosed(WorldInfoPanelClosedSignal _)
        {
            _selectedUnitId = null;
            _selectedBuilding = null;
            _selectedBuildingId = null;
            _selectedRecruitmentModule = null;
            _selectedRecruitmentRecipe = null;
            _selectedRecipeIndex = -1;
            _selectionFromQueue = false;
            _selectedQueueId = 0;
            _statusOverride = null;
            _recruitmentPanel.SetActive(false);
            _recruitmentView.SetSelectedRecipeIndex(-1);
            _recruitmentView.SetHeader(string.Empty, string.Empty);
            ClearBuildingRecruitmentState();
        }

        private bool IsSelectedBuildingOwnedByLocalPlayer()
        {
            if (!_selectedBuilding.HasValue
                || string.IsNullOrWhiteSpace(_turns.LocalOwnerId)
                || _construction is not IConstructionBuildingOwnershipQuery ownership)
            {
                return false;
            }

            return ownership.TryGetPlacedBuildingOwner(
                       _selectedBuilding.Value,
                       out string ownerId)
                && string.Equals(
                    ownerId?.Trim(),
                    _turns.LocalOwnerId.Trim(),
                    StringComparison.Ordinal);
        }

        private bool IsSelectedBuildingOperational()
        {
            if (!_selectedBuilding.HasValue)
                return false;

            return _construction is not IConstructionLifecycle lifecycle
                || lifecycle.IsOperational(_selectedBuilding.Value);
        }

        private void RefreshUnit()
        {
            if (string.IsNullOrWhiteSpace(_selectedUnitId))
            {
                _unitText.text = string.Empty;
                _unitText.transform.parent.gameObject.SetActive(false);
                return;
            }

            _unitText.transform.parent.gameObject.SetActive(true);
            string typeId = _units.GetUnitTypeId(_selectedUnitId);
            UnitClassConfig config = _unitConfigs.GetConfig(typeId);
            _unitText.text =
                $"{ResolveUnitName(typeId)}    Stamina " +
                $"{_units.GetStamina(_selectedUnitId):0.#}/{config?.BaseStamina ?? 0f:0.#}";
        }

        private void RefreshSelectionDetails(
            string unitTypeId,
            UnitRecruitmentRecipeDefinition recipe,
            UnitRecruitmentQueueItemSnapshot? queueItem)
        {
            if (_recruitmentView == null || string.IsNullOrWhiteSpace(unitTypeId))
                return;

            UnitClassConfig config = _unitConfigs.GetConfig(unitTypeId);
            _recruitmentView.SetSelectedRecipeIndex(
                queueItem.HasValue ? FindRecipeIndex(unitTypeId) : _selectedRecipeIndex);
            _recruitmentView.SelectionPanel.SetActive(true);
            _recruitmentView.SelectionName.text = ResolveUnitName(unitTypeId);
            _recruitmentView.SelectionClass.text =
                config != null ? LocalizeCombatType(config.CombatType) : string.Empty;
            _recruitmentView.SelectionIcon.sprite = config?.ResolveCustomSprite();
            _recruitmentView.SelectionIcon.enabled = _recruitmentView.SelectionIcon.sprite != null;
            if (_recruitmentView.SelectionStats != null)
                _recruitmentView.SelectionStats.text = string.Empty;

            int trainingTurns = Mathf.Max(1, recipe?.TrainingTurns ?? queueItem?.TrainingTurns ?? 1);
            string progress = queueItem.HasValue
                ? queueItem.Value.IsReady
                    ? "Готовий до розміщення"
                    : queueItem.Value.QueueId == (_visibleQueueItems.Count > 0 ? _visibleQueueItems[0].QueueId : -1)
                        ? $"Навчається {queueItem.Value.CompletedTurns}/{queueItem.Value.TrainingTurns}"
                        : $"Очікує {queueItem.Value.CompletedTurns}/{queueItem.Value.TrainingTurns}"
                : $"Навчання: {trainingTurns} {RoundWord(trainingTurns)}";

            PopulateStatRows(config, trainingTurns, progress);

            RefreshCostRows(recipe);
            _recruitmentView.HireButtonLabel.text = "Найняти";
            if (queueItem.HasValue)
                _recruitmentView.SetActionHint(queueItem.Value.IsReady
                    ? "Готовий юніт розміщується через індикатор біля будівлі."
                    : "Цей запис уже в черзі.");
        }

        private string ResolveUnitName(string typeId)
        {
            UnitClassConfig config = _unitConfigs.GetConfig(typeId);
            return config != null && !string.IsNullOrWhiteSpace(config.DisplayName)
                ? config.DisplayName.Trim()
                : "Юніт";
        }

        private string ResolveBuildingName(BuildingDefinition definition)
        {
            if (!string.IsNullOrWhiteSpace(definition?.DisplayName))
                return definition.DisplayName.Trim();

            return string.IsNullOrWhiteSpace(_selectedBuildingId)
                ? "Будівля"
                : "Будівля";
        }
    }
}
