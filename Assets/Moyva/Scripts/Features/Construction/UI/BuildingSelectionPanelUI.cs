using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;

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

        /// <summary>
        /// Спрацьовує при натисканні кнопки будівлі.
        /// Аргумент — унікальний ідентифікатор будівлі (BuildingDefinition.Id).
        /// </summary>
        public Action<string> OnBuildingClicked;

        private readonly List<GameObject> _spawnedButtons = new List<GameObject>();
        private readonly List<BuildingButtonUI> _buttonComponents = new List<BuildingButtonUI>();
        private readonly List<BuildingListItemData> _allItems = new List<BuildingListItemData>();

        private BuildingButtonUI _selectedButton;
        private BuildingCategory? _activeCategory;

        private void Awake()
        {
            if (itemContainer == null)
                Debug.LogWarning($"[BuildingSelectionPanelUI] Поле 'itemContainer' не призначено на '{name}'. Кнопки не відображатимуться.", this);
            if (buttonPrefab == null)
                Debug.LogWarning($"[BuildingSelectionPanelUI] Поле 'buttonPrefab' не призначено на '{name}'. Кнопки не відображатимуться.", this);

            if (categoryTabs != null)
                categoryTabs.OnCategorySelected += SetCategoryFilter;
        }

        private void OnDestroy()
        {
            if (categoryTabs != null)
                categoryTabs.OnCategorySelected -= SetCategoryFilter;
        }

        /// <summary>
        /// Заповнює панель кнопками будівель.
        /// Попередні кнопки видаляються. Вкладка категорій ініціалізується автоматично.
        /// Викликається автоматично через <see cref="ConstructionUIController"/>.
        /// </summary>
        public void Populate(IEnumerable<BuildingListItemData> buildings)
        {
            ClearItems();
            _allItems.Clear();

            if (buildings != null)
                _allItems.AddRange(buildings);

            if (categoryTabs != null)
                categoryTabs.Initialize(_allItems);

            ApplyFilter();
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

        /// <summary>
        /// Встановлює виділення кнопки для будівлі з вказаним ID (збільшення при виборі).
        /// Знімає виділення з попередньо вибраної кнопки.
        /// </summary>
        public void SetSelectedBuilding(string buildingId)
        {
            if (_selectedButton != null)
                _selectedButton.SetSelected(false);

            _selectedButton = null;

            foreach (var btn in _buttonComponents)
            {
                if (!btn.gameObject.activeSelf) continue;

                if (btn.GetBuildingId() == buildingId)
                {
                    _selectedButton = btn;
                    btn.SetSelected(true);
                    break;
                }
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
            _selectedButton = null;
        }

        // -----------------------------------------------------------------------

        private void ApplyFilter()
        {
            if (itemContainer == null || buttonPrefab == null)
                return;

            ClearItems();

            foreach (var item in _allItems)
            {
                if (_activeCategory.HasValue && item.Category != _activeCategory.Value)
                    continue;

                var go = Instantiate(buttonPrefab, itemContainer);
                var btn = go.GetComponent<BuildingButtonUI>();
                if (btn != null)
                {
                    btn.Setup(item, HandleBuildingClicked);
                    _buttonComponents.Add(btn);
                }

                _spawnedButtons.Add(go);
            }
        }

        private void HandleBuildingClicked(string buildingId)
        {
            SetSelectedBuilding(buildingId);
            OnBuildingClicked?.Invoke(buildingId);
        }
    }
}
