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
        private void OnRecruitmentQueueChanged(UnitRecruitmentQueueChangedSignal signal)
        {
            if (IsSignalForSelectedBuilding(signal.OwnerId, signal.BuildingPosition))
            {
                RefreshQueue();
                RefreshTurnAuthority();
            }
        }

        private void OnRecruitmentReady(UnitRecruitmentReadySignal signal)
        {
            if (IsSignalForSelectedBuilding(signal.OwnerId, signal.BuildingPosition))
            {
                _statusOverride = $"{ResolveUnitName(signal.UnitTypeId)} готовий до розміщення.";
                RefreshQueue();
                RefreshTurnAuthority();
            }
        }

        private void OnRecruitmentDeployed(UnitRecruitmentDeployedSignal signal)
        {
            if (IsSignalForSelectedBuilding(signal.OwnerId, signal.BuildingPosition))
            {
                if (_selectionFromQueue && _selectedQueueId == signal.QueueId)
                    SelectFirstRecipeIfAvailable();

                _statusOverride = $"{ResolveUnitName(signal.UnitTypeId)} розміщено.";
                RefreshQueue();
                RefreshTurnAuthority();
            }
        }

        private UiActionResult HandleRecruitmentEnqueueAction()
        {
            if (_selectionFromQueue
                || _selectedRecruitmentRecipe == null
                || string.IsNullOrWhiteSpace(_selectedRecruitmentRecipe.UnitTypeId))
            {
                OnHireClicked();
                return UiActionResult.Rejected(UiActionReason.NoSelection);
            }

            OnHireClicked();
            return string.IsNullOrWhiteSpace(_statusOverride)
                || _statusOverride.Contains("додано до черги", StringComparison.Ordinal)
                    ? UiActionResult.Performed()
                    : UiActionResult.Rejected(UiActionReason.ActionUnavailable, _statusOverride);
        }

        private void CloseRecruitmentPanel()
        {
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
            HideQueueRows();
            ClearCostRows();
            _recruitmentView.ClearStatRows();
        }

        private void BuildRecipeButtons(string buildingId)
        {
            ClearRecipeSlots();
            _selectedRecruitmentModule = null;

            if (!IsSelectedBuildingOwnedByLocalPlayer())
            {
                _recruitmentPanel.SetActive(false);
                return;
            }

            BuildingDefinition definition = _buildings.GetById(buildingId);
            if (definition == null
                || !BuildingDefinitionCapabilities.TryGetEnabledModule(
                    definition,
                    out UnitRecruitmentBuildingModule module))
            {
                _recruitmentPanel.SetActive(false);
                return;
            }

            _selectedRecruitmentModule = module;
            _recruitmentPanel.SetActive(true);
            _recruitmentView.SetHeader(ResolveBuildingName(definition), "Найм військ");
            int slotIndex = 0;
            int skippedForCapacity = 0;

            if (module.Recipes != null)
            {
                foreach (UnitRecruitmentRecipeDefinition recipe in module.Recipes)
                {
                    if (recipe == null || string.IsNullOrWhiteSpace(recipe.UnitTypeId))
                        continue;

                    if (slotIndex >= _recipeButtons.Count)
                    {
                        skippedForCapacity++;
                        continue;
                    }

                    string unitTypeId = recipe.UnitTypeId.Trim();
                    Button button = _recipeButtons[slotIndex];
                    TMP_Text label = _view.GetRecipeLabel(slotIndex);
                    UnitClassConfig config = _unitConfigs.GetConfig(unitTypeId);
                    Image icon = _recruitmentView.GetRecipeIcon(slotIndex);

                    _recipeUnitTypeIds[slotIndex] = unitTypeId;
                    _recipeDefinitions[slotIndex] = recipe;
                    label.text = ResolveUnitName(unitTypeId);
                    _recruitmentView.GetRecipeNameText(slotIndex).text =
                        ResolveUnitName(unitTypeId);
                    _recruitmentView.SetRecipeTrainingText(
                        slotIndex,
                        $"{Mathf.Max(1, recipe.TrainingTurns)} р.");

                    icon.sprite = config?.ResolveCustomSprite();
                    icon.enabled = icon.sprite != null;
                    button.gameObject.SetActive(true);
                    slotIndex++;
                }
            }

            if (skippedForCapacity > 0)
            {
            }

            SelectFirstRecipeIfAvailable();
            RefreshRecruitmentAuthority();
            RefreshQueue();
        }

        private void ClearRecipeSlots()
        {
            for (int index = 0; index < _recipeButtons.Count; index++)
            {
                _recipeUnitTypeIds[index] = null;
                _recipeDefinitions[index] = null;
                Button button = _recipeButtons[index];
                if (button != null)
                    button.gameObject.SetActive(false);

                TMP_Text label = _view.GetRecipeLabel(index);
                if (label != null)
                    label.text = string.Empty;

                if (_recruitmentView != null && index < _recruitmentView.RecipeIconCount)
                {
                    Image icon = _recruitmentView.GetRecipeIcon(index);
                    icon.sprite = null;
                    icon.enabled = false;
                    _recruitmentView.GetRecipeNameText(index).text = string.Empty;
                    _recruitmentView.SetRecipeTrainingText(index, string.Empty);
                }
            }

            _selectedRecruitmentRecipe = null;
            _selectedRecipeIndex = -1;
            _selectionFromQueue = false;
            _selectedQueueId = 0;
            if (_recruitmentView != null)
            {
                _recruitmentView.SelectionPanel.SetActive(false);
                _recruitmentView.SetSelectedRecipeIndex(-1);
                _recruitmentView.ClearStatRows();
                _recruitmentView.SetActionHint(string.Empty);
            }
            ClearCostRows();
        }

        private void OnRecipeSlotClicked(int index)
        {
            if (index < 0 || index >= _recipeDefinitions.Count)
                return;

            UnitRecruitmentRecipeDefinition recipe = _recipeDefinitions[index];
            if (recipe == null || string.IsNullOrWhiteSpace(recipe.UnitTypeId))
                return;

            _selectedRecruitmentRecipe = recipe;
            _selectedRecipeIndex = index;
            _selectionFromQueue = false;
            _selectedQueueId = 0;
            _recruitmentView.SetSelectedRecipeIndex(index);
            RefreshSelectionDetails(recipe.UnitTypeId, recipe, null);
            RefreshRecruitmentAuthority();
        }

        private void OnQueueSlotClicked(int index)
        {
            if (index < 0 || index >= _visibleQueueItems.Count)
                return;

            UnitRecruitmentQueueItemSnapshot job = _visibleQueueItems[index];
            UnitRecruitmentRecipeDefinition recipe = ResolveRecipe(job.UnitTypeId);
            _selectedRecruitmentRecipe = recipe;
            _selectionFromQueue = true;
            _selectedQueueId = job.QueueId;
            _selectedRecipeIndex = FindRecipeIndex(job.UnitTypeId);
            _recruitmentView.SetSelectedRecipeIndex(_selectedRecipeIndex);
            RefreshSelectionDetails(job.UnitTypeId, recipe, job);
            RefreshRecruitmentAuthority();
        }

        private void OnHireClicked()
        {
            if (_selectionFromQueue
                || _selectedRecruitmentRecipe == null
                || string.IsNullOrWhiteSpace(_selectedRecruitmentRecipe.UnitTypeId))
            {
                return;
            }

            Enqueue(_selectedRecruitmentRecipe.UnitTypeId);
        }

        private void Enqueue(string unitTypeId)
        {
            RefreshTurnAuthority();
            if (!_selectedBuilding.HasValue)
                return;

            if (!_authority.CanIssueLocalCommands)
            {
                _statusOverride = _authority.StatusText;
                RefreshTurnAuthority();
                return;
            }

            if (!IsSelectedBuildingOwnedByLocalPlayer())
            {
                _statusOverride = "Найм доступний лише у власній будівлі.";
                RefreshTurnAuthority();
                return;
            }

            if (!IsSelectedBuildingOperational())
            {
                _statusOverride =
                    "Військова будівля ще будується. Найм стане доступний після завершення будівництва.";
                RefreshTurnAuthority();
                RefreshQueue();
                return;
            }

            if (!_recruitment.TryEnqueue(
                    _authority.LocalOwnerId,
                    _selectedBuilding.Value,
                    unitTypeId,
                    out string reason))
            {
                _statusOverride = PresentRecruitmentFailure(reason);
            }
            else
            {
                _statusOverride =
                    $"{ResolveUnitName(unitTypeId)} додано до черги. " +
                    "Стан навчання показано у верхньому блоці казарми.";
            }

            RefreshTurnAuthority();
            RefreshQueue();
        }

        private static string PresentRecruitmentFailure(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return "Найм не вдалося розпочати.";

            string value = reason.Trim();
            return value switch
            {
                "Recruiting building is still under construction." =>
                    "Військова будівля ще будується. Найм стане доступний після завершення будівництва.",
                "Recruiting building belongs to another owner." =>
                    "Найм доступний лише у власній будівлі.",
                "Recruiting building is not a committed construction placement." =>
                    "Будівля ще не готова до використання.",
                "Building has no enabled unit recruitment module." =>
                    "Ця будівля не підтримує найм юнітів.",
                "Turn authority is unavailable for recruitment." =>
                    "Найм тимчасово недоступний: немає активного ходу.",
                "Construction recruitment context is unavailable." =>
                    "Найм тимчасово недоступний через стан будівництва.",
                "Economy is unavailable for recruitment costs." =>
                    "Неможливо перевірити ресурси для найму.",
                "No owned settlement is available to fund recruitment at this building." =>
                    "Будівля не підключена до власного поселення, яке може оплатити найм.",
                _ when value.StartsWith("Building cannot recruit unit type '", StringComparison.Ordinal) =>
                    "Ця будівля не може наймати вибраний тип юніта.",
                _ when value.StartsWith("Unit type '", StringComparison.Ordinal)
                    && value.EndsWith("' is not registered.", StringComparison.Ordinal) =>
                    "Вибраний тип юніта не зареєстрований у грі.",
                _ => value,
            };
        }

    }
}
