using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Construction.UI
{
    /// <summary>
    /// Панель вибору будівель. Відображає кнопку для кожної доступної будівлі
    /// та підтримує фільтрацію за категорією (<see cref="BuildingCategory"/>).
    /// Коли гравець натискає кнопку — спрацьовує <see cref="OnBuildingClicked"/>.
    ///
    /// ЯК ПІДКЛЮЧИТИ В UNITY:
    /// 1. Додай компонент до кореневого GameObject панелі вибору будівель.
    /// 2. Призначи <b>itemContainer</b> — Transform (наприклад VerticalLayoutGroup Content).
    /// 3. Призначи <b>buttonPrefab</b> — prefab з компонентом <see cref="BuildingButtonUI"/>.
    /// 4. Опціонально: підключи <see cref="BuildingCategoryTabsUI"/> через <b>categoryTabs</b>.
    /// 5. <see cref="ConstructionUIController"/> викликає <see cref="Populate"/> при старті.
    /// </summary>
    public class BuildingSelectionPanelUI : MonoBehaviour
    {
        [Header("Підключення")]
        [Tooltip("Батьківський Transform для кнопок будівель (наприклад Content у VerticalLayoutGroup).")]
        [SerializeField] private Transform itemContainer;

        [Tooltip("Prefab кнопки будівлі. Має містити компонент BuildingButtonUI.")]
        [SerializeField] private GameObject buttonPrefab;

        [Tooltip("Панель вкладок категорій. Необов'язкова.")]
        [SerializeField] private BuildingCategoryTabsUI categoryTabs;

        [Tooltip("Поле пошуку за назвою або ID. Ctrl+F переводить сюди фокус, коли каталог відкритий.")]
        [SerializeField] private TMP_InputField searchInput;

        [Header("Відображення кнопок")]
        [Tooltip("Єдиний розмір кнопок будівель у меню.")]
        [SerializeField] private Vector2 buttonSize = new Vector2(160f, 160f);

        /// <summary>
        /// Спрацьовує при натисканні кнопки будівлі.
        /// Аргумент — унікальний ідентифікатор будівлі (BuildingDefinition.Id).
        /// </summary>
        public Action<string> OnBuildingClicked;

        private readonly List<GameObject> _spawnedButtons = new List<GameObject>();
        private readonly List<BuildingButtonUI> _buttonComponents = new List<BuildingButtonUI>();
        private readonly Dictionary<string, BuildingButtonUI>
            _buttonsById =
                new Dictionary<string, BuildingButtonUI>();
        private readonly List<BuildingListItemData> _allItems =
            new List<BuildingListItemData>();
        private readonly Dictionary<string, BuildingListItemData>
            _itemsById =
                new Dictionary<string, BuildingListItemData>(
                    StringComparer.Ordinal);

        private BuildingButtonUI _selectedButton;
        private BuildingCategory? _activeCategory;
        private string _searchQuery = string.Empty;

        private void Awake()
        {
            if (itemContainer == null)
                Debug.LogWarning($"[BuildingSelectionPanelUI] Поле 'itemContainer' не призначено на '{name}'. Кнопки не відображатимуться.", this);
            if (buttonPrefab == null)
                Debug.LogWarning($"[BuildingSelectionPanelUI] Поле 'buttonPrefab' не призначено на '{name}'. Кнопки не відображатимуться.", this);

            if (categoryTabs != null)
                categoryTabs.OnCategorySelected += SetCategoryFilter;
            if (searchInput != null)
                searchInput.onValueChanged.AddListener(SetSearchQuery);
        }

        private void Update()
        {
            if (searchInput == null || !isActiveAndEnabled || !gameObject.activeInHierarchy)
                return;

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null
                || !keyboard.fKey.wasPressedThisFrame
                || !(keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed))
            {
                return;
            }

            searchInput.ActivateInputField();
            searchInput.Select();
        }

        private void OnDestroy()
        {
            if (categoryTabs != null)
                categoryTabs.OnCategorySelected -= SetCategoryFilter;
            if (searchInput != null)
                searchInput.onValueChanged.RemoveListener(SetSearchQuery);
        }

        /// <summary>
        /// Заповнює панель кнопками будівель.
        /// Попередні кнопки видаляються. Вкладка категорій ініціалізується автоматично.
        /// Викликається автоматично через <see cref="ConstructionUIController"/>.
        /// </summary>
        public void Populate(
            IEnumerable<BuildingListItemData> buildings)
        {
            string previouslySelectedId =
                _selectedButton != null
                    ? _selectedButton.GetBuildingId()
                    : null;

            _allItems.Clear();
            _itemsById.Clear();

            if (buildings != null)
            {
                foreach (BuildingListItemData item in buildings)
                {
                    if (item == null)
                        continue;

                    _allItems.Add(item);
                    if (!string.IsNullOrWhiteSpace(item.Id))
                        _itemsById[item.Id] = item;
                }
            }

            if (categoryTabs != null)
                categoryTabs.Initialize(_allItems);

            BuildCachedButtons();
            ApplyFilter();

            if (!string.IsNullOrWhiteSpace(
                    previouslySelectedId))
            {
                SetSelectedBuilding(previouslySelectedId);
            }
        }

        /// <summary>
        /// Встановлює фільтр категорії. null — показувати всі.
        /// Викликається автоматично <see cref="BuildingCategoryTabsUI"/>.
        /// </summary>
        public void SetCategoryFilter(BuildingCategory? category)
        {
            _activeCategory = category;
            ApplyFilter();
        }

        public void SetSearchQuery(string query)
        {
            string normalized = (query ?? string.Empty).Trim();
            if (string.Equals(_searchQuery, normalized, StringComparison.OrdinalIgnoreCase))
                return;

            _searchQuery = normalized;
            ApplyFilter();
        }

        /// <summary>
        /// Встановлює виділення кнопки для будівлі з вказаним ID (збільшення при виборі).
        /// Знімає виділення з попередньо вибраної кнопки.
        /// </summary>
        public void SetSelectedBuilding(string buildingId)
        {
            if (_selectedButton != null)
                _selectedButton.SetSelected(false);

            _selectedButton = null;

            if (string.IsNullOrWhiteSpace(buildingId))
                return;

            if (_buttonsById.TryGetValue(buildingId, out var selected))
            {
                _selectedButton = selected;
                _selectedButton.SetSelected(true);
            }
        }

        /// <summary>Знімає виділення з усіх кнопок.</summary>
        public void ClearSelection()
        {
            if (_selectedButton != null)
            {
                _selectedButton.SetSelected(false);
                _selectedButton = null;
            }
        }

        /// <summary>Видаляє всі кнопки будівель.</summary>
        public void ClearItems()
        {
            foreach (var go in _spawnedButtons)
            {
                if (go != null)
                    Destroy(go);
            }
            _spawnedButtons.Clear();
            _buttonComponents.Clear();
            _buttonsById.Clear();
            _itemsById.Clear();
            _selectedButton = null;
        }

        // -----------------------------------------------------------------------

        private void ApplyFilter()
        {
            if (itemContainer == null || buttonPrefab == null)
                return;

            foreach (var btn in _buttonComponents)
            {
                var buildingId = btn.GetBuildingId();
                if (!_buttonsById.TryGetValue(buildingId, out var button))
                    continue;

                var item = FindItemById(buildingId);
                bool visible = item != null
                               && (!_activeCategory.HasValue || item.Category == _activeCategory.Value)
                               && MatchesSearch(item, _searchQuery);
                button.gameObject.SetActive(visible);

                if (!visible && button == _selectedButton)
                {
                    button.SetSelected(false);
                    _selectedButton = null;
                }
            }
        }

        private void BuildCachedButtons()
        {
            if (itemContainer == null || buttonPrefab == null)
                return;

            _buttonComponents.Clear();
            _buttonsById.Clear();
            _selectedButton = null;

            int usedCount = 0;
            for (int itemIndex = 0;
                 itemIndex < _allItems.Count;
                 itemIndex++)
            {
                BuildingListItemData item =
                    _allItems[itemIndex];

                GameObject go = null;
                if (usedCount < _spawnedButtons.Count)
                {
                    go = _spawnedButtons[usedCount];
                    if (go == null)
                    {
                        go = Instantiate(
                            buttonPrefab,
                            itemContainer);
                        _spawnedButtons[usedCount] = go;
                        EnsureLayoutElement(
                            go.transform as RectTransform);
                    }
                }
                else
                {
                    go = Instantiate(
                        buttonPrefab,
                        itemContainer);
                    _spawnedButtons.Add(go);
                    EnsureLayoutElement(
                        go.transform as RectTransform);
                }

                if (go == null)
                    continue;

                go.SetActive(true);
                BuildingButtonUI btn =
                    go.GetComponent<BuildingButtonUI>();
                if (btn != null)
                {
                    btn.SetSelected(false);
                    btn.Setup(
                        item,
                        HandleBuildingClicked);
                    _buttonComponents.Add(btn);
                    if (!string.IsNullOrWhiteSpace(item.Id))
                        _buttonsById[item.Id] = btn;
                }
                else
                {
                    Debug.LogWarning(
                        $"[Construction UI] На префабі " +
                        $"'{buttonPrefab.name}' відсутній " +
                        $"BuildingButtonUI. Запис '{item.Id}' " +
                        "не буде керованим.",
                        this);
                }

                usedCount++;
            }

            for (int index = usedCount;
                 index < _spawnedButtons.Count;
                 index++)
            {
                GameObject unused =
                    _spawnedButtons[index];
                if (unused != null)
                    unused.SetActive(false);
            }
        }

        private void EnsureLayoutElement(RectTransform rectTransform)
        {
            if (rectTransform == null)
                return;

            var layoutElement = rectTransform.GetComponent<LayoutElement>();
            if (layoutElement == null)
                layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();

            layoutElement.minWidth = buttonSize.x;
            layoutElement.preferredWidth = buttonSize.x;
            layoutElement.minHeight = buttonSize.y;
            layoutElement.preferredHeight = buttonSize.y;
            layoutElement.flexibleWidth = 0f;
            layoutElement.flexibleHeight = 0f;
        }

        private BuildingListItemData FindItemById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            return _itemsById.TryGetValue(
                    id,
                    out BuildingListItemData item)
                ? item
                : null;
        }

        private static bool MatchesSearch(BuildingListItemData item, string query)
        {
            if (item == null)
                return false;
            if (string.IsNullOrWhiteSpace(query))
                return true;

            return (!string.IsNullOrWhiteSpace(item.DisplayName)
                    && item.DisplayName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                   || (!string.IsNullOrWhiteSpace(item.Id)
                       && item.Id.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void HandleBuildingClicked(string buildingId)
        {
            SetSelectedBuilding(buildingId);
            OnBuildingClicked?.Invoke(buildingId);
        }
    }
}
