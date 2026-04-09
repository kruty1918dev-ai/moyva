using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kruty1918.Moyva.Construction.UI
{
    /// <summary>
    /// Панель вкладок категорій будівель (Військові / Цивільні / Індустріальні).
    /// Генерує одну кнопку-вкладку на кожну категорію, що є у реєстрі.
    /// При натисканні на вкладку — спрацьовує <see cref="OnCategorySelected"/>.
    ///
    /// ЯК ПІДКЛЮЧИТИ В UNITY:
    /// 1. Додай компонент до панелі вкладок (наприклад HorizontalLayoutGroup).
    /// 2. Призначи <b>tabContainer</b> — Transform для кнопок вкладок.
    /// 3. Призначи <b>tabButtonPrefab</b> — prefab кнопки (Button + TextMeshProUGUI).
    /// 4. Підключи цей компонент у поле <b>categoryTabs</b> у <see cref="BuildingSelectionPanelUI"/>.
    ///    Ініціалізація відбувається автоматично через <see cref="Initialize"/>.
    /// </summary>
    public class BuildingCategoryTabsUI : MonoBehaviour
    {
        [Header("Підключення")]
        [Tooltip("Батьківський Transform для кнопок вкладок категорій.")]
        [SerializeField] private Transform tabContainer;

        [Tooltip("Prefab кнопки вкладки. Має містити Button та TextMeshProUGUI.")]
        [SerializeField] private GameObject tabButtonPrefab;

        [Tooltip("Колір активної вкладки.")]
        [SerializeField] private Color activeColor = new Color(0.2f, 0.6f, 1f);

        [Tooltip("Колір неактивної вкладки.")]
        [SerializeField] private Color inactiveColor = Color.white;

        [Tooltip("Єдиний розмір кнопки вкладки класу будівель.")]
        [SerializeField] private Vector2 categoryButtonSize = new Vector2(180f, 64f);

        [Header("Анімація натиску (DOTween)")]
        [Tooltip("Множник масштабу для анімації натиску вкладки.")]
        [SerializeField] private float pressScaleMultiplier = 1.06f;

        [Tooltip("Тривалість анімації натиску вкладки у секундах.")]
        [SerializeField] private float pressDuration = 0.14f;

        [Tooltip("Інтенсивність поштовху (кількість коливань).")]
        [SerializeField] private int pressVibrato = 7;

        [Tooltip("Пружність анімації натиску (0..1).")]
        [SerializeField] private float pressElasticity = 0.75f;

        /// <summary>
        /// Спрацьовує при виборі категорії. null означає «показати всі».
        /// </summary>
        public Action<BuildingCategory?> OnCategorySelected;

        private readonly List<GameObject> _tabs = new List<GameObject>();
        private readonly List<Button> _tabButtons = new List<Button>();
        private readonly List<BuildingCategory> _categories = new List<BuildingCategory>();
        private int _activeIndex = -1; // -1 = «всі»

        private void Awake()
        {
            if (tabContainer == null)
                Debug.LogWarning($"[BuildingCategoryTabsUI] Поле 'tabContainer' не призначено на '{name}'.", this);
            if (tabButtonPrefab == null)
                Debug.LogWarning($"[BuildingCategoryTabsUI] Поле 'tabButtonPrefab' не призначено на '{name}'.", this);
        }

        /// <summary>
        /// Ініціалізує вкладки на основі унікальних категорій зі списку будівель.
        /// Викликається автоматично через <see cref="BuildingSelectionPanelUI.Populate"/>.
        /// </summary>
        public void Initialize(IEnumerable<BuildingListItemData> buildings)
        {
            ClearTabs();

            if (tabContainer == null || tabButtonPrefab == null)
                return;

            // Вимога: щоразу на ініціалізації читаємо enum категорій заново.
            _categories.AddRange((BuildingCategory[])Enum.GetValues(typeof(BuildingCategory)));

            // Кнопка «Всі»
            CreateTabButton("Всі", -1);

            // Кнопки для кожної категорії
            for (int i = 0; i < _categories.Count; i++)
                CreateTabButton(CategoryLabel(_categories[i]), i);

            // За замовчуванням вибираємо «Всі»
            SetActiveTab(-1);
        }

        private void CreateTabButton(string labelText, int index)
        {
            if (tabContainer == null || tabButtonPrefab == null) return;

            var go = Instantiate(tabButtonPrefab, tabContainer);
            var btn = go.GetComponent<Button>();
            var lbl = go.GetComponentInChildren<TextMeshProUGUI>();

            if (btn == null && lbl != null)
            {
                btn = go.AddComponent<Button>();
                Debug.LogWarning($"[Construction UI] На вкладці '{labelText}' не було Button. Компонент додано автоматично.", this);
            }

            EnsureLayoutElement(go.transform as RectTransform);

            if (lbl != null) lbl.text = labelText;

            int capturedIndex = index;
            if (btn != null)
            {
                btn.onClick.AddListener(() =>
                {
                    ConstructionButtonPressAnimator.AnimatePress(btn.transform, pressScaleMultiplier, pressDuration, pressVibrato, pressElasticity);
                    OnTabClicked(capturedIndex);
                });
            }

            _tabs.Add(go);
            _tabButtons.Add(btn);
        }

        private void OnTabClicked(int index)
        {
            SetActiveTab(index);
            OnCategorySelected?.Invoke(index < 0 ? (BuildingCategory?)null : _categories[index]);
        }

        private void SetActiveTab(int index)
        {
            _activeIndex = index;
            // Перша кнопка — «Всі» (offset 0), решта — категорії (offset 1+)
            for (int i = 0; i < _tabButtons.Count; i++)
            {
                if (_tabButtons[i] == null) continue;
                var colors = _tabButtons[i].colors;
                int tabIndex = i == 0 ? -1 : i - 1;
                colors.normalColor = (tabIndex == index) ? activeColor : inactiveColor;
                _tabButtons[i].colors = colors;
            }
        }

        private void ClearTabs()
        {
            foreach (var go in _tabs)
                if (go != null) Destroy(go);
            _tabs.Clear();
            _tabButtons.Clear();
            _categories.Clear();
            _activeIndex = -1;
        }

        private void EnsureLayoutElement(RectTransform rectTransform)
        {
            if (rectTransform == null)
                return;

            var layoutElement = rectTransform.GetComponent<LayoutElement>();
            if (layoutElement == null)
                layoutElement = rectTransform.gameObject.AddComponent<LayoutElement>();

            layoutElement.minWidth = categoryButtonSize.x;
            layoutElement.preferredWidth = categoryButtonSize.x;
            layoutElement.minHeight = categoryButtonSize.y;
            layoutElement.preferredHeight = categoryButtonSize.y;
            layoutElement.flexibleWidth = 0f;
            layoutElement.flexibleHeight = 0f;
        }

        private void OnDestroy() => ClearTabs();

        private static string CategoryLabel(BuildingCategory category) => category switch
        {
            BuildingCategory.Military   => "Військові",
            BuildingCategory.Civilian   => "Цивільні",
            BuildingCategory.Industrial => "Індустріальні",
            BuildingCategory.Walls      => "Стіни",
            _                           => category.ToString()
        };
    }
}
