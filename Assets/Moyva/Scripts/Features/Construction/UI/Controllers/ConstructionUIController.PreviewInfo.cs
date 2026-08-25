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
        private void EnsurePreviewInfoPanel()
        {
            if (_previewInfoPanelInstance != null)
                return;

            if (previewInfoPanelPrefab == null)
                return;

            if (previewInfoPanelPrefab.scene.IsValid())
            {
                _previewInfoPanelInstance = previewInfoPanelPrefab;
            }
            else
            {
                _previewInfoPanelInstance = Instantiate(previewInfoPanelPrefab, transform);
                _previewInfoPanelInstance.name = previewInfoPanelPrefab.name;
            }

            CachePreviewInfoPanelTexts();
        }

        private void CachePreviewInfoPanelTexts()
        {
            if (_previewInfoPanelInstance == null)
                return;

            var texts = _previewInfoPanelInstance.GetComponentsInChildren<TMP_Text>(true);
            _previewInfoPanelLabel = FindTextByKeyword(texts, previewInfoPanelTextLabelKeyWord);
            _previewInfoPanelInfo = FindTextByKeyword(texts, previewInfoPanelTextInfoKeyWord);

            if (_previewInfoPanelLabel == null && texts.Length > 0)
                _previewInfoPanelLabel = texts[0];

            if (_previewInfoPanelInfo == null && texts.Length > 1)
                _previewInfoPanelInfo = texts[1];

            if (_previewInfoPanelInfo == null)
                _previewInfoPanelInfo = _previewInfoPanelLabel;

            ApplyPreviewTextStyle(_previewInfoPanelLabel, isHeader: true);
            ApplyPreviewTextStyle(_previewInfoPanelInfo, isHeader: false);
        }

        private static void ApplyPreviewTextStyle(TMP_Text text, bool isHeader)
        {
            if (text == null)
                return;

            // Фіксуємо вигляд у коді, щоб не залежати від довільних налаштувань prefab/scene.
            text.enableAutoSizing = false;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.overflowMode = TextOverflowModes.Overflow;
            text.alignment = isHeader
                ? TextAlignmentOptions.TopLeft
                : TextAlignmentOptions.TopLeft;

            if (isHeader)
            {
                text.fontStyle = FontStyles.Bold;
                text.fontSize = previewInfoHeaderFontSize;
                text.lineSpacing = 0f;
            }
            else
            {
                text.fontStyle = FontStyles.Normal;
                text.fontSize = previewInfoBodyFontSize;
                text.lineSpacing = previewInfoBodyLineSpacing;
            }
        }

        private static TMP_Text FindTextByKeyword(IEnumerable<TMP_Text> texts, string keyword)
        {
            if (texts == null || string.IsNullOrWhiteSpace(keyword))
                return null;

            foreach (var text in texts)
            {
                if (text == null || string.IsNullOrWhiteSpace(text.name))
                    continue;

                if (text.name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                    return text;
            }

            return null;
        }

        private bool TryTogglePreviewInfoByPosition(Vector2Int position)
        {
            if (_constructionService == null)
                return false;

            if (_constructionService.HasPendingPlacementAt(position)
                && _constructionService.TryGetPendingBuildingIdAt(position, out var previewBuildingId)
                && !string.IsNullOrWhiteSpace(previewBuildingId))
            {
                bool isSamePinnedPreview = _isPreviewInfoVisible
                                           && _isPreviewInfoPinned
                                           && _pinnedPreviewPosition == position;

                if (isSamePinnedPreview)
                {
                    HidePreviewInfoPanel();
                    return true;
                }

                ShowPreviewInfoPanel(previewBuildingId, position, pinToPreviewObject: true);
                return true;
            }

            if (string.IsNullOrWhiteSpace(_selectedBuildingId))
                HidePreviewInfoPanel();

            return false;
        }

        private void ShowPreviewInfoPanel(string buildingId, Vector2Int position, bool pinToPreviewObject)
        {
            EnsurePreviewInfoPanel();
            if (_previewInfoPanelInstance == null)
                return;

            var definition = _buildingRegistry?.GetById(buildingId);
            if (definition == null)
                return;

            if (_previewInfoPanelLabel != null)
                _previewInfoPanelLabel.text = "Параметри розміщення";

            if (_previewInfoPanelInfo != null)
                _previewInfoPanelInfo.text = BuildPreviewInfoText(definition, position);

            _previewInfoPanelInstance.SetActive(true);
            _isPreviewInfoVisible = true;

            _isPreviewInfoPinned = pinToPreviewObject;
            _pinnedPreviewBuildingId = buildingId;
            if (pinToPreviewObject)
                _pinnedPreviewPosition = position;
        }

        private void HidePreviewInfoPanel()
        {
            if (_previewInfoPanelInstance != null)
                _previewInfoPanelInstance.SetActive(false);

            _isPreviewInfoVisible = false;
            _isPreviewInfoPinned = false;
            _pinnedPreviewBuildingId = null;
        }

        private string BuildPreviewInfoText(
            BuildingDefinition definition,
            Vector2Int position)
        {
            StringBuilder sb = _previewInfoBuilder;
            sb.Clear();

            var displayName = string.IsNullOrWhiteSpace(definition.DisplayName)
                ? "Будівля"
                : definition.DisplayName.Trim();

            sb.AppendLine($"Обрано: {displayName}");
            if (_isPreviewInfoPinned)
                sb.AppendLine($"Тайл: {position.x}, {position.y}");

            int beforeFacts = sb.Length;
            if (BuildingDefaultInfoExtractor.AppendMeaningfulFacts(definition, sb, ResolveResourceDisplayName))
            {
                if (beforeFacts > 0)
                    sb.Insert(beforeFacts, Environment.NewLine);
            }

            AppendConstructionCostBlock(sb, definition);
            AppendPendingDeficitBlock(sb, position);
            AppendOwnerResourcesBlock(sb);

            while (sb.Length > 0
                   && (sb[sb.Length - 1] == '\n'
                       || sb[sb.Length - 1] == '\r'))
            {
                sb.Length--;
            }

            return sb.ToString();
        }

        private void AppendPendingDeficitBlock(
            StringBuilder sb,
            Vector2Int position)
        {
            if (_constructionService == null
                || !_constructionService.TryGetPendingPlacementStatus(
                    position,
                    out ConstructionPendingPlacementStatus status)
                || status.IsAffordable
                || string.IsNullOrWhiteSpace(status.ErrorMessage))
            {
                return;
            }

            sb.AppendLine();
            sb.AppendLine("Не можна підтвердити:");
            sb.AppendLine($"• {status.ErrorMessage}");
        }

        private void AppendConstructionCostBlock(StringBuilder sb, BuildingDefinition definition)
        {
            sb.AppendLine();
            sb.AppendLine("Потрібно для будівництва:");

            if (definition == null || definition.ConstructionCost == null || definition.ConstructionCost.Count == 0)
            {
                sb.AppendLine("• Безкоштовно");
                return;
            }

            bool hasValidEntries = false;
            for (int i = 0; i < definition.ConstructionCost.Count; i++)
            {
                var entry = definition.ConstructionCost[i];
                if (entry == null || entry.Amount <= 0)
                    continue;

                hasValidEntries = true;
                string resourceId = string.IsNullOrWhiteSpace(entry.ResourceId)
                    ? "<невідомий ресурс>"
                    : entry.ResourceId.Trim();

                sb.AppendLine($"• {ResolveResourceDisplayName(resourceId)}: {entry.Amount}");
            }

            if (!hasValidEntries)
                sb.AppendLine("• Безкоштовно");
        }

        private void AppendOwnerResourcesBlock(StringBuilder sb)
        {
            sb.AppendLine();
            sb.AppendLine("Ресурси власника зараз:");

            if (_economyInfoMediator == null || _constructionService == null)
            {
                sb.AppendLine("• Дані економіки недоступні");
                return;
            }

            var ownerId = _constructionService.GetActiveOwner();
            var totals = _economyInfoMediator.GetOwnerResourceTotals(ownerId);
            if (totals == null || totals.Count == 0)
            {
                sb.AppendLine("• Немає ресурсів");
                return;
            }

            _previewResourceScratch.Clear();
            foreach (KeyValuePair<string, float> pair in totals)
                _previewResourceScratch.Add(pair);

            _previewResourceScratch.Sort(
                ComparePreviewResources);

            int visibleCount = Mathf.Min(
                previewInfoMaxResourcesLines,
                _previewResourceScratch.Count);
            for (int index = 0;
                 index < visibleCount;
                 index++)
            {
                KeyValuePair<string, float> resource =
                    _previewResourceScratch[index];
                sb.AppendLine(
                    $"• {ResolveResourceDisplayName(resource.Key)}: " +
                    $"{resource.Value:0.#}");
            }

            int hiddenCount =
                totals.Count - visibleCount;
            if (hiddenCount > 0)
                sb.AppendLine($"• + ще {hiddenCount}");
        }

        private static int ComparePreviewResources(
            KeyValuePair<string, float> left,
            KeyValuePair<string, float> right)
        {
            int byValue =
                right.Value.CompareTo(left.Value);
            return byValue != 0
                ? byValue
                : StringComparer.Ordinal.Compare(
                    left.Key,
                    right.Key);
        }

        private string ResolveResourceDisplayName(string resourceId)
        {
            string displayName = _economyInfoMediator?.GetResourceDisplayName(resourceId);
            if (!string.IsNullOrWhiteSpace(displayName)
                && !string.Equals(
                    displayName.Trim(),
                    resourceId?.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            {
                return displayName.Trim();
            }

            return "Ресурс";
        }
    }
}
