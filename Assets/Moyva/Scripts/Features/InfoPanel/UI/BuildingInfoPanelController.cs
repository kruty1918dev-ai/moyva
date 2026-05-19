using System;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.InfoPanel.UI
{
    public sealed class WorldInfoPanelController : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;

        private readonly GameObject _panelRoot;
        private readonly TMP_Text _titleText;
        private readonly TMP_Text _subtitleText;
        private readonly TMP_Text _resourcesText;
        private readonly Button _closeButton;
        private readonly Transform _constructionCostContainer;
        private readonly EconomyDatabaseSO _economyDatabase;

        public WorldInfoPanelController(
            SignalBus signalBus,
            [Inject(Id = "BuildingInfoPanelRoot")] GameObject panelRoot,
            [Inject(Id = "BuildingInfoTitleText")] TMP_Text titleText,
            [Inject(Id = "BuildingInfoSubtitleText")] TMP_Text subtitleText,
            [Inject(Id = "BuildingInfoResourcesText")] TMP_Text resourcesText,
            [Inject(Id = "BuildingInfoCloseButton")] Button closeButton,
            [Inject(Id = "ConstructionCostContainer", Optional = true)] Transform constructionCostContainer,
            [InjectOptional] EconomyDatabaseSO economyDatabase)
        {
            _signalBus = signalBus;
            _panelRoot = panelRoot;
            _titleText = titleText;
            _subtitleText = subtitleText;
            _resourcesText = resourcesText;
            _closeButton = closeButton;
            _constructionCostContainer = constructionCostContainer;
            _economyDatabase = economyDatabase;
        }

        public void Initialize()
        {
            try
            {
                if (_signalBus == null)
                {
                    Debug.LogError("[WorldInfoPanel] Initialize: _signalBus == null");
                    return;
                }

                _signalBus.Subscribe<WorldInfoPanelRequestedSignal>(OnInfoRequested);
                _signalBus.Subscribe<WorldInfoPanelClosedSignal>(OnPanelClosed);

                if (_closeButton != null)
                    _closeButton.onClick.AddListener(ClosePanel);

                SetVisible(false);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WorldInfoPanel] ПОМИЛКА в Initialize(): {ex.GetType().Name} - {ex.Message}");
            }
        }

        public void Dispose()
        {
            try
            {
                if (_signalBus == null)
                    return;

                _signalBus.TryUnsubscribe<WorldInfoPanelRequestedSignal>(OnInfoRequested);
                _signalBus.TryUnsubscribe<WorldInfoPanelClosedSignal>(OnPanelClosed);

                if (_closeButton != null)
                    _closeButton.onClick.RemoveListener(ClosePanel);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WorldInfoPanel] ПОМИЛКА в Dispose(): {ex.GetType().Name} - {ex.Message}");
            }
        }

        private void OnInfoRequested(WorldInfoPanelRequestedSignal signal)
        {
            try
            {
                _titleText.text = signal.Title ?? string.Empty;
                _subtitleText.text = signal.Subtitle ?? string.Empty;
                _resourcesText.text = signal.Content ?? string.Empty;

                RefreshConstructionCostItems(signal.ConstructionCostItems);

                SetVisible(true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WorldInfoPanel] ПОМИЛКА в OnInfoRequested(): {ex.GetType().Name} - {ex.Message}");
                SetVisible(false);
            }
        }

        private void RefreshConstructionCostItems(BuildingConstructionCostItemData[] items)
        {
            if (_constructionCostContainer == null)
                return;

            // Очищаємо попередні дочірні об'єкти
            for (int i = _constructionCostContainer.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(_constructionCostContainer.GetChild(i).gameObject);

            bool hasItems = items != null && items.Length > 0;
            _constructionCostContainer.gameObject.SetActive(true);

            // Заголовок секції
            CreateLabelRow("\u0412\u0430\u0440\u0442\u0456\u0441\u0442\u044c \u0431\u0443\u0434\u0456\u0432\u043d\u0438\u0446\u0442\u0432\u0430:", bold: true);

            if (!hasItems)
            {
                CreateLabelRow("Безкоштовно", bold: false);
                return;
            }

            foreach (var item in items)
                CreateCostItemRow(item);
        }

        private void CreateLabelRow(string text, bool bold)
        {
            var go = new GameObject("CostHeader", typeof(RectTransform));
            go.transform.SetParent(_constructionCostContainer, false);

            var tmp = go.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 14f;
            tmp.color = new Color(0.9f, 0.9f, 0.9f);
            if (bold)
                tmp.fontStyle = TMPro.FontStyles.Bold;

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 20f;
        }

        private void CreateCostItemRow(BuildingConstructionCostItemData item)
        {
            ResolveResourcePresentation(item, out var displayName, out var icon);

            var row = new GameObject("CostRow", typeof(RectTransform));
            row.transform.SetParent(_constructionCostContainer, false);

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlHeight = true;
            hlg.childControlWidth = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            var rowCsf = row.AddComponent<ContentSizeFitter>();
            rowCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Іконка ресурсу (якщо є)
            if (icon != null)
            {
                var iconGO = new GameObject("Icon", typeof(RectTransform));
                iconGO.transform.SetParent(row.transform, false);

                var img = iconGO.AddComponent<Image>();
                img.sprite = icon;
                img.preserveAspect = true;

                var iconLe = iconGO.AddComponent<LayoutElement>();
                iconLe.minWidth = 24f;
                iconLe.minHeight = 24f;
                iconLe.preferredWidth = 24f;
                iconLe.preferredHeight = 24f;
            }

            // Назва ресурсу + кількість
            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(row.transform, false);

            var tmp = labelGO.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.text = $"{displayName}: <b>{item.Amount}</b>";
            tmp.fontSize = 14f;
            tmp.color = Color.white;
            tmp.alignment = TMPro.TextAlignmentOptions.MidlineLeft;

            var labelLe = labelGO.AddComponent<LayoutElement>();
            labelLe.preferredHeight = 24f;
        }

        private void ResolveResourcePresentation(BuildingConstructionCostItemData item, out string displayName, out Sprite icon)
        {
            displayName = string.IsNullOrWhiteSpace(item.DisplayName) ? item.ResourceId : item.DisplayName;
            icon = item.Icon;

            if (_economyDatabase == null || string.IsNullOrWhiteSpace(item.ResourceId))
                return;

            var resources = _economyDatabase.Resources;
            for (int i = 0; i < resources.Count; i++)
            {
                var res = resources[i];
                if (res == null || !string.Equals(res.Id, item.ResourceId, StringComparison.Ordinal))
                    continue;

                if (string.IsNullOrWhiteSpace(item.DisplayName) && !string.IsNullOrWhiteSpace(res.DisplayName))
                    displayName = res.DisplayName;

                if (icon == null)
                    icon = res.Icon;

                return;
            }
        }

        private void OnPanelClosed(WorldInfoPanelClosedSignal _)
        {
            SetVisible(false);
        }

        private void ClosePanel()
        {
            SetVisible(false);
            _signalBus.Fire<WorldInfoPanelClosedSignal>();
        }

        private void SetVisible(bool isVisible)
        {
            if (_panelRoot != null)
                _panelRoot.SetActive(isVisible);
        }
    }
}
