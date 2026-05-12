using Kruty1918.Moyva.Signals;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using Zenject;

namespace Kruty1918.Moyva.InfoPanel.UI
{
    /// <summary>
    /// Контролер для детальної панелі замку з інтерактивним списком будівель.
    /// Керує відображенням статистики поселення та кнопками для переміщення до кожної будівлі.
    /// </summary>
    public sealed class CastleDetailedInfoPanelController : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly CastleDetailedInfoPresenter _castlePresenter;
        
        private readonly GameObject _detailedPanelRoot;
        private readonly TMP_Text _castleNameText;
        private readonly TMP_Text _statisticsText;
        private readonly Transform _buildingsListContainer;
        private readonly Button _closeButton;
        private readonly GameObject _buildingItemPrefab;

        private readonly List<GameObject> _spawnedItems = new();

        public CastleDetailedInfoPanelController(
            SignalBus signalBus,
            [InjectOptional] CastleDetailedInfoPresenter castlePresenter,
            [Inject(Id = "CastleDetailedPanelRoot")] GameObject detailedPanelRoot,
            [Inject(Id = "CastleNameText")] TMP_Text castleNameText,
            [Inject(Id = "CastleStatisticsText")] TMP_Text statisticsText,
            [Inject(Id = "CastleBuildingsListContainer")] Transform buildingsListContainer,
            [Inject(Id = "CastleDetailedCloseButton")] Button closeButton,
            [Inject(Id = "CastleBuildingItemPrefab")] GameObject buildingItemPrefab)
        {
            _signalBus = signalBus;
            _castlePresenter = castlePresenter;
            _detailedPanelRoot = detailedPanelRoot;
            _castleNameText = castleNameText;
            _statisticsText = statisticsText;
            _buildingsListContainer = buildingsListContainer;
            _closeButton = closeButton;
            _buildingItemPrefab = buildingItemPrefab;
        }

        public void Initialize()
        {
            try
            {
                _signalBus.Subscribe<WorldInfoPanelRequestedSignal>(OnWorldInfoRequested);
                _signalBus.Subscribe<WorldInfoPanelClosedSignal>(OnPanelClosed);

                if (_closeButton != null)
                    _closeButton.onClick.AddListener(ClosePanel);

                SetVisible(false);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CastleDetailedInfoPanel] ПОМИЛКА в Initialize(): {ex.Message}");
            }
        }

        public void Dispose()
        {
            try
            {
                _signalBus.TryUnsubscribe<WorldInfoPanelRequestedSignal>(OnWorldInfoRequested);
                _signalBus.TryUnsubscribe<WorldInfoPanelClosedSignal>(OnPanelClosed);

                if (_closeButton != null)
                    _closeButton.onClick.RemoveListener(ClosePanel);

                ClearBuildingsList();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CastleDetailedInfoPanel] ПОМИЛКА в Dispose(): {ex.Message}");
            }
        }

        private void OnWorldInfoRequested(WorldInfoPanelRequestedSignal signal)
        {
            // Перевірити чи це замок за наявністю "Капітал" в сабтайтлі
            if (signal.Subtitle != null && signal.Subtitle.Contains("Капітал"))
            {
                try
                {
                    _castleNameText.text = signal.Title ?? "Замок";
                    _statisticsText.text = signal.Content ?? string.Empty;

                    // Спарсити список будівель з контенту і створити кнопки
                    PopulateBuildingsList(signal.Content);

                    SetVisible(true);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[CastleDetailedInfoPanel] ПОМИЛКА в OnWorldInfoRequested(): {ex.Message}");
                }
            }
            else
            {
                SetVisible(false);
            }
        }

        private void OnPanelClosed(WorldInfoPanelClosedSignal _)
        {
            SetVisible(false);
            ClearBuildingsList();
        }

        private void PopulateBuildingsList(string content)
        {
            ClearBuildingsList();

            if (string.IsNullOrEmpty(content) || _buildingsListContainer == null || _buildingItemPrefab == null)
                return;

            // Парсити список зі строк що мають формат "icon BuildingName (x, y)"
            var lines = content.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            bool inBuildingsSection = false;

            foreach (var line in lines)
            {
                if (line.Contains("БУДІВЛІ"))
                {
                    inBuildingsSection = true;
                    continue;
                }

                if (!inBuildingsSection)
                    continue;

                if (line.StartsWith("═") || line.StartsWith("─") || string.IsNullOrWhiteSpace(line))
                    break;

                // Спробуємо спарсити позицію зі строки
                // Формат: "icon BuildingName (x, y)"
                if (TryParseBuilding(line, out var buildingName, out var position))
                {
                    CreateBuildingItemButton(buildingName, position);
                }
            }
        }

        private bool TryParseBuilding(string line, out string buildingName, out Vector2Int position)
        {
            buildingName = null;
            position = Vector2Int.zero;

            var trimmed = line.Trim();
            if (trimmed.Length < 2)
                return false;

            // Пропустити іконку (перший символ)
            var content = trimmed.Substring(1).Trim();

            // Знайти останні дужки для позиції
            int lastOpenParen = content.LastIndexOf('(');
            if (lastOpenParen < 0)
                return false;

            buildingName = content.Substring(0, lastOpenParen).Trim();

            // Спарсити позицію "x, y)"
            var positionStr = content.Substring(lastOpenParen + 1).TrimEnd(')').Trim();
            var coords = positionStr.Split(',');

            if (coords.Length != 2 || !int.TryParse(coords[0].Trim(), out int x) || !int.TryParse(coords[1].Trim(), out int y))
                return false;

            position = new Vector2Int(x, y);
            return true;
        }

        private void CreateBuildingItemButton(string buildingName, Vector2Int position)
        {
            if (_buildingsListContainer == null)
                return;

            // Створити простий Button програмно
            var buttonGO = new GameObject($"BuildingButton_{position.x}_{position.y}");
            buttonGO.transform.SetParent(_buildingsListContainer, false);

            var rectTransform = buttonGO.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(300, 40);

            var button = buttonGO.AddComponent<Button>();
            var image = buttonGO.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.3f, 0.8f);

            // Додати Text для назви будівлі
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);

            var textComponent = textGO.AddComponent<TMP_Text>();
            textComponent.text = $"📍 {buildingName} ({position.x}, {position.y})";
            textComponent.fontSize = 28;
            textComponent.alignment = TextAlignmentOptions.MidlineLeft;
            textComponent.color = Color.white;

            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 0);
            textRect.offsetMax = new Vector2(-10, 0);

            // Додати LayoutElement для фіксованої висоти
            var layoutElement = buttonGO.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 40;
            layoutElement.preferredWidth = 300;

            button.onClick.AddListener(() =>
            {
                OnBuildingClicked(position, buildingName);
            });

            _spawnedItems.Add(buttonGO);
        }

        private void OnBuildingClicked(Vector2Int position, string buildingName)
        {
            if (_castlePresenter != null)
            {
                _castlePresenter.NavigateToBuildingAt(position, buildingName);
            }
        }

        private void ClearBuildingsList()
        {
            foreach (var item in _spawnedItems)
            {
                if (item != null)
                    Object.Destroy(item);
            }
            _spawnedItems.Clear();
        }

        private void ClosePanel()
        {
            _signalBus.Fire(new WorldInfoPanelClosedSignal());
        }

        private void SetVisible(bool visible)
        {
            if (_detailedPanelRoot != null)
                _detailedPanelRoot.SetActive(visible);
        }
    }
}
