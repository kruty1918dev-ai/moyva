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

    }
}
