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
                Debug.LogWarning(
                    $"[GameplayTurnHud] Recruitment recipes exceed authored scene slots. " +
                    $"Visible={_recipeButtons.Count}, skipped={skippedForCapacity}.");
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

        private void RefreshRecruitmentAuthority()
        {
            if (_recruitmentPanel == null || !_recruitmentPanel.activeSelf)
                return;

            bool ownsBuilding = IsSelectedBuildingOwnedByLocalPlayer();
            bool operational = IsSelectedBuildingOperational();
            bool queueFull = false;
            if (_selectedBuilding.HasValue && _selectedRecruitmentModule != null)
            {
                IReadOnlyList<UnitRecruitmentQueueItemSnapshot> queue =
                    _recruitment.GetQueue(_turns.LocalOwnerId, _selectedBuilding.Value);
                queueFull = (queue?.Count ?? 0)
                    >= Mathf.Max(1, _selectedRecruitmentModule.QueueCapacity);
            }

            bool canRecruit = _authority.CanIssueLocalCommands
                && ownsBuilding
                && operational
                && !_selectionFromQueue
                && _selectedRecruitmentRecipe != null
                && !queueFull;

            for (int index = 0; index < _recipeButtons.Count; index++)
            {
                Button button = _recipeButtons[index];
                if (button != null)
                {
                    // Recipe cards remain inspectable outside the local action phase.
                    button.interactable = ownsBuilding
                        && !string.IsNullOrWhiteSpace(_recipeUnitTypeIds[index]);
                }
            }

            if (_recruitmentView != null)
            {
                _recruitmentView.HireButton.interactable = canRecruit;
                _recruitmentView.HireButtonLabel.text = "Найняти";
                _recruitmentView.SetActionHint(ResolveRecruitmentActionHint(
                    ownsBuilding,
                    operational,
                    queueFull,
                    canRecruit));
            }
        }

        private void RefreshQueue()
        {
            if (_recruitmentView == null)
                return;

            if (!_selectedBuilding.HasValue
                || _recruitmentPanel == null
                || !_recruitmentPanel.activeSelf
                || !IsSelectedBuildingOwnedByLocalPlayer())
            {
                HideQueueRows();
                _recruitmentView.SetQueueSummary("Черга", "0/0", empty: true);
                ClearBuildingRecruitmentState();
                return;
            }

            int selectedCapacity = Mathf.Max(1, _selectedRecruitmentModule?.QueueCapacity ?? 1);

            if (_construction is IConstructionLifecycle lifecycle
                && lifecycle.TryGetProgress(
                    _selectedBuilding.Value,
                    out int completed,
                    out int required)
                && completed < required)
            {
                _queueText.text = $"Черга 0/{selectedCapacity}";
                _recruitmentView.SetQueueSummary("Черга", $"0/{selectedCapacity}", empty: true);
                HideQueueRows();
                SetBuildingRecruitmentState(
                    "Будівля будується",
                    $"{completed}/{required}",
                    required > 0 ? Mathf.Clamp01((float)completed / required) : 0f,
                    "БУДУЄТЬСЯ",
                    new Color32(142, 158, 174, 255));
                return;
            }

            // Some lifecycle implementations may not expose progress during every
            // construction phase. Operational state is authoritative for recruitment,
            // so never present a non-operational building as "free".
            if (!IsSelectedBuildingOperational())
            {
                _queueText.text = $"Черга 0/{selectedCapacity}";
                _recruitmentView.SetQueueSummary("Черга", $"0/{selectedCapacity}", empty: true);
                HideQueueRows();
                SetBuildingRecruitmentState(
                    "Будівля будується",
                    "Найм стане доступний одразу після завершення будівництва.",
                    0f,
                    "БУДУЄТЬСЯ",
                    new Color32(142, 158, 174, 255));
                return;
            }

            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> queue =
                _recruitment.GetQueue(
                    _turns.LocalOwnerId,
                    _selectedBuilding.Value);

            _visibleQueueItems.Clear();
            int capacity = selectedCapacity;
            int queueCount = queue?.Count ?? 0;
            int readyCount = 0;

            if (queue != null)
            {
                for (int index = 0; index < queue.Count; index++)
                {
                    if (queue[index].IsReady)
                        readyCount++;
                }
            }

            _queueText.text = queueCount == 0
                ? $"Черга 0/{capacity}"
                : $"Черга {queueCount}/{capacity}" + (readyCount > 0 ? $" · готово {readyCount}" : string.Empty);
            _recruitmentView.SetQueueSummary("Черга", $"{queueCount}/{capacity}", queueCount == 0);

            RefreshBuildingRecruitmentState(queue, capacity);

            for (int index = 0; index < _recruitmentView.QueueRowCount; index++)
            {
                Button button = _recruitmentView.GetQueueButton(index);
                Image icon = _recruitmentView.GetQueueIcon(index);
                TMP_Text label = _recruitmentView.GetQueueLabel(index);
                Image progressFill = _recruitmentView.GetQueueProgressFill(index);

                if (queue == null || index >= queue.Count)
                {
                    button.gameObject.SetActive(false);
                    icon.sprite = null;
                    icon.enabled = false;
                    label.text = string.Empty;
                    if (progressFill != null)
                        progressFill.fillAmount = 0f;
                    continue;
                }

                UnitRecruitmentQueueItemSnapshot job = queue[index];
                _visibleQueueItems.Add(job);
                UnitClassConfig config = _unitConfigs.GetConfig(job.UnitTypeId);
                icon.sprite = config?.ResolveCustomSprite();
                icon.enabled = icon.sprite != null;

                if (job.IsReady)
                {
                    label.text = $"{ResolveUnitName(job.UnitTypeId)}   Готовий";
                    if (progressFill != null)
                        progressFill.fillAmount = 1f;
                }
                else if (index == 0)
                {
                    label.text =
                        $"{ResolveUnitName(job.UnitTypeId)}   Навчання {job.CompletedTurns}/{job.TrainingTurns}";
                    if (progressFill != null)
                    {
                        progressFill.fillAmount = job.TrainingTurns > 0
                            ? Mathf.Clamp01((float)job.CompletedTurns / job.TrainingTurns)
                            : 0f;
                    }
                }
                else
                {
                    label.text = $"{ResolveUnitName(job.UnitTypeId)}   Очікує";
                    if (progressFill != null)
                        progressFill.fillAmount = 0f;
                }

                button.gameObject.SetActive(true);
                button.interactable = true;
            }

            if (_selectionFromQueue)
            {
                UnitRecruitmentQueueItemSnapshot? selected = FindVisibleQueueItem(_selectedQueueId);
                if (selected.HasValue)
                {
                    UnitRecruitmentRecipeDefinition recipe = ResolveRecipe(selected.Value.UnitTypeId);
                    RefreshSelectionDetails(selected.Value.UnitTypeId, recipe, selected.Value);
                }
                else
                {
                    SelectFirstRecipeIfAvailable();
                }
            }
        }

        private void RefreshBuildingRecruitmentState(
            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> queue,
            int capacity)
        {
            int count = queue?.Count ?? 0;
            if (count == 0)
            {
                SetBuildingRecruitmentState(
                    "Вільна",
                    $"Черга 0/{capacity}",
                    0f,
                    "ВІЛЬНА",
                    new Color32(92, 154, 98, 255));
                return;
            }

            UnitRecruitmentQueueItemSnapshot head = queue[0];
            int waiting = Mathf.Max(0, count - 1);

            if (head.IsReady)
            {
                SetBuildingRecruitmentState(
                    $"{ResolveUnitName(head.UnitTypeId)} готовий",
                    waiting > 0 ? $"Очікує розміщення · у черзі {waiting}" : "Очікує розміщення",
                    1f,
                    "ГОТОВО",
                    new Color32(91, 176, 96, 255));
                return;
            }

            float progress = head.TrainingTurns > 0
                ? Mathf.Clamp01((float)head.CompletedTurns / head.TrainingTurns)
                : 0f;
            string suffix = waiting > 0
                ? $" • після нього очікує: {waiting}"
                : string.Empty;

            SetBuildingRecruitmentState(
                $"Навчається: {ResolveUnitName(head.UnitTypeId)}",
                $"{head.CompletedTurns}/{head.TrainingTurns} раунди · " +
                $"залишилось {head.RemainingTurns}{suffix}",
                progress,
                "НАВЧАННЯ",
                new Color32(190, 132, 62, 255));
        }

        private void SetBuildingRecruitmentState(
            string title,
            string detail,
            float progress,
            string chipLabel,
            Color chipColor)
        {
            if (_recruitmentView == null
                || _recruitmentView.BuildingStatePanel == null)
            {
                return;
            }

            _recruitmentView.BuildingStatePanel.SetActive(true);
            _recruitmentView.BuildingStateTitle.text = title ?? string.Empty;
            _recruitmentView.BuildingStateDetail.text = detail ?? string.Empty;
            if (_recruitmentView.BuildingStateProgressFill != null)
                _recruitmentView.BuildingStateProgressFill.fillAmount =
                    Mathf.Clamp01(progress);
            _recruitmentView.SetStateChip(chipLabel, chipColor);
        }

        private void ClearBuildingRecruitmentState()
        {
            if (_recruitmentView == null
                || _recruitmentView.BuildingStatePanel == null)
            {
                return;
            }

            _recruitmentView.BuildingStatePanel.SetActive(false);
            if (_recruitmentView.BuildingStateTitle != null)
                _recruitmentView.BuildingStateTitle.text = string.Empty;
            if (_recruitmentView.BuildingStateDetail != null)
                _recruitmentView.BuildingStateDetail.text = string.Empty;
            if (_recruitmentView.BuildingStateProgressFill != null)
                _recruitmentView.BuildingStateProgressFill.fillAmount = 0f;
            _recruitmentView.SetStateChip(string.Empty, Color.clear);
        }

        private UnitRecruitmentQueueItemSnapshot? FindVisibleQueueItem(long queueId)
        {
            for (int index = 0; index < _visibleQueueItems.Count; index++)
            {
                if (_visibleQueueItems[index].QueueId == queueId)
                    return _visibleQueueItems[index];
            }

            return null;
        }

        private void HideQueueRows()
        {
            _visibleQueueItems.Clear();
            if (_recruitmentView == null)
                return;

            for (int index = 0; index < _recruitmentView.QueueRowCount; index++)
            {
                Button button = _recruitmentView.GetQueueButton(index);
                Image icon = _recruitmentView.GetQueueIcon(index);
                TMP_Text label = _recruitmentView.GetQueueLabel(index);
                Image progressFill = _recruitmentView.GetQueueProgressFill(index);
                button.gameObject.SetActive(false);
                icon.sprite = null;
                icon.enabled = false;
                label.text = string.Empty;
                if (progressFill != null)
                    progressFill.fillAmount = 0f;
            }
        }

        private void SelectFirstRecipeIfAvailable()
        {
            for (int index = 0; index < _recipeDefinitions.Count; index++)
            {
                if (_recipeDefinitions[index] == null)
                    continue;

                _selectedRecruitmentRecipe = _recipeDefinitions[index];
                _selectedRecipeIndex = index;
                _selectionFromQueue = false;
                _selectedQueueId = 0;
                _recruitmentView.SetSelectedRecipeIndex(index);
                RefreshSelectionDetails(
                    _selectedRecruitmentRecipe.UnitTypeId,
                    _selectedRecruitmentRecipe,
                    null);
                return;
            }

            _selectedRecruitmentRecipe = null;
            _selectedRecipeIndex = -1;
            _selectionFromQueue = false;
            _selectedQueueId = 0;
            _recruitmentView.SetSelectedRecipeIndex(-1);
            _recruitmentView.SelectionPanel.SetActive(false);
            _recruitmentView.ClearStatRows();
            _recruitmentView.SetActionHint(string.Empty);
            ClearCostRows();
        }

        private UnitRecruitmentRecipeDefinition ResolveRecipe(string unitTypeId)
        {
            if (_selectedRecruitmentModule?.Recipes == null || string.IsNullOrWhiteSpace(unitTypeId))
                return null;

            for (int index = 0; index < _selectedRecruitmentModule.Recipes.Count; index++)
            {
                UnitRecruitmentRecipeDefinition recipe = _selectedRecruitmentModule.Recipes[index];
                if (recipe != null
                    && string.Equals(recipe.UnitTypeId?.Trim(), unitTypeId.Trim(), StringComparison.Ordinal))
                {
                    return recipe;
                }
            }

            return null;
        }

        private int FindRecipeIndex(string unitTypeId)
        {
            if (string.IsNullOrWhiteSpace(unitTypeId))
                return -1;

            string normalized = unitTypeId.Trim();
            for (int index = 0; index < _recipeUnitTypeIds.Count; index++)
            {
                if (string.Equals(
                        _recipeUnitTypeIds[index]?.Trim(),
                        normalized,
                        StringComparison.Ordinal))
                {
                    return index;
                }
            }

            return -1;
        }

        private string ResolveRecruitmentActionHint(
            bool ownsBuilding,
            bool operational,
            bool queueFull,
            bool canRecruit)
        {
            if (_selectionFromQueue)
            {
                UnitRecruitmentQueueItemSnapshot? selected =
                    FindVisibleQueueItem(_selectedQueueId);
                if (selected.HasValue && selected.Value.IsReady)
                    return "Готовий юніт розміщується через індикатор біля будівлі.";
                return _selectedQueueId > 0 ? "Цей запис уже в черзі." : string.Empty;
            }

            if (!ownsBuilding)
                return "Найм доступний лише у власній будівлі.";
            if (!operational)
                return "Будівля будується.";
            if (!_authority.CanIssueLocalCommands)
                return "Зараз не ваш хід.";
            if (_selectedRecruitmentRecipe == null)
                return "Оберіть юніта.";
            if (queueFull)
                return "Черга заповнена.";

            return canRecruit ? string.Empty : "Найм зараз недоступний.";
        }
    }
}
