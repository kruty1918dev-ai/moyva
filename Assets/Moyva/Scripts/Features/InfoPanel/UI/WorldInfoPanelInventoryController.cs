using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.InfoPanel.UI
{
    /// <summary>
    /// Structured warehouse resource inventory for WorldInfoPanel.
    /// Keeps the legacy info-panel signal contract intact while replacing the
    /// monolithic resource text with filterable, scrollable resource rows.
    /// </summary>
    public sealed class WorldInfoPanelInventoryController : IInitializable, IDisposable
    {
        private enum ResourceFilter
        {
            All = 0,
            Food = 1,
            Materials = 2,
        }

        private readonly SignalBus _signalBus;
        private readonly GameObject _panelRoot;
        private readonly TMP_Text _descriptionText;
        private readonly EconomyDatabaseSO _economyDatabase;
        private readonly EconomyManager _economyManager;

        private GameObject _inventoryRoot;
        private Transform _content;
        private TMP_Text _emptyState;
        private ScrollRect _scrollRect;
        private Button _allButton;
        private Button _foodButton;
        private Button _materialsButton;

        private ResourceFilter _filter = ResourceFilter.All;
        private WorldInfoSelectionKind _selectionKind;
        private string _selectedObjectId;
        private Vector2Int _selectedPosition;
        private bool _hasSelectedPosition;
        private bool _isWarehousePanel;

        [Inject]
        public WorldInfoPanelInventoryController(
            SignalBus signalBus,
            [Inject(Id = "BuildingInfoPanelRoot")] GameObject panelRoot,
            [Inject(Id = "BuildingInfoResourcesText")] TMP_Text descriptionText,
            [InjectOptional] EconomyDatabaseSO economyDatabase,
            [InjectOptional] EconomyManager economyManager)
        {
            _signalBus = signalBus;
            _panelRoot = panelRoot;
            _descriptionText = descriptionText;
            _economyDatabase = economyDatabase;
            _economyManager = economyManager;
        }

        public void Initialize()
        {
            ResolveUiReferences();

            if (_signalBus != null)
            {
                _signalBus.Subscribe<WorldInfoSelectionChangedSignal>(OnSelectionChanged);
                _signalBus.Subscribe<WorldInfoPanelRequestedSignal>(OnPanelRequested);
                _signalBus.Subscribe<WorldInfoPanelClosedSignal>(OnPanelClosed);
                _signalBus.Subscribe<SettlementResourceChangedSignal>(OnSettlementResourceChanged);
                _signalBus.Subscribe<EconomyTickCompletedSignal>(OnEconomyTickCompleted);
            }

            if (_allButton != null)
                _allButton.onClick.AddListener(() => SetFilter(ResourceFilter.All));
            if (_foodButton != null)
                _foodButton.onClick.AddListener(() => SetFilter(ResourceFilter.Food));
            if (_materialsButton != null)
                _materialsButton.onClick.AddListener(() => SetFilter(ResourceFilter.Materials));

            ApplyFilterVisuals();
        }

        public void Dispose()
        {
            if (_signalBus != null)
            {
                _signalBus.TryUnsubscribe<WorldInfoSelectionChangedSignal>(OnSelectionChanged);
                _signalBus.TryUnsubscribe<WorldInfoPanelRequestedSignal>(OnPanelRequested);
                _signalBus.TryUnsubscribe<WorldInfoPanelClosedSignal>(OnPanelClosed);
                _signalBus.TryUnsubscribe<SettlementResourceChangedSignal>(OnSettlementResourceChanged);
                _signalBus.TryUnsubscribe<EconomyTickCompletedSignal>(OnEconomyTickCompleted);
            }

            if (_allButton != null)
                _allButton.onClick.RemoveAllListeners();
            if (_foodButton != null)
                _foodButton.onClick.RemoveAllListeners();
            if (_materialsButton != null)
                _materialsButton.onClick.RemoveAllListeners();
        }

        private void ResolveUiReferences()
        {
            if (_panelRoot == null)
                return;

            Transform root = _panelRoot.transform.Find("ResourceInventory");
            _inventoryRoot = root != null ? root.gameObject : null;
            _allButton = root?.Find("FilterTabs/AllButton")?.GetComponent<Button>();
            _foodButton = root?.Find("FilterTabs/FoodButton")?.GetComponent<Button>();
            _materialsButton = root?.Find("FilterTabs/MaterialsButton")?.GetComponent<Button>();
            _scrollRect = root?.Find("ResourceScroll")?.GetComponent<ScrollRect>();
            _content = root?.Find("ResourceScroll/Viewport/Content");
            _emptyState = root?.Find("ResourceScroll/Viewport/EmptyState")?.GetComponent<TMP_Text>();
        }

        private void OnSelectionChanged(WorldInfoSelectionChangedSignal signal)
        {
            _selectionKind = signal.Kind;
            _selectedObjectId = signal.ObjectId;
            _selectedPosition = signal.Position;
            _hasSelectedPosition = signal.Kind != WorldInfoSelectionKind.None;
        }

        private void OnPanelRequested(WorldInfoPanelRequestedSignal signal)
        {
            if (_descriptionText != null)
                _descriptionText.text = StripConstructionCostBlock(signal.Content);

            _isWarehousePanel = IsWarehouseSelection(signal.Title, _selectedObjectId);

            if (_inventoryRoot != null)
                _inventoryRoot.SetActive(_isWarehousePanel);

            if (!_isWarehousePanel)
                return;

            _filter = ResourceFilter.All;
            ApplyFilterVisuals();
            RefreshInventory(resetScroll: true);
        }

        private void OnPanelClosed(WorldInfoPanelClosedSignal _)
        {
            _isWarehousePanel = false;
            if (_inventoryRoot != null)
                _inventoryRoot.SetActive(false);
        }

        private void OnSettlementResourceChanged(SettlementResourceChangedSignal _)
        {
            if (CanRefreshLive())
                RefreshInventory(resetScroll: false);
        }

        private void OnEconomyTickCompleted(EconomyTickCompletedSignal _)
        {
            if (CanRefreshLive())
                RefreshInventory(resetScroll: false);
        }

        private bool CanRefreshLive()
        {
            return _isWarehousePanel
                   && _panelRoot != null
                   && _panelRoot.activeInHierarchy
                   && _inventoryRoot != null
                   && _inventoryRoot.activeInHierarchy;
        }

        private void SetFilter(ResourceFilter filter)
        {
            if (_filter == filter)
                return;

            _filter = filter;
            ApplyFilterVisuals();
            RefreshInventory(resetScroll: true);
        }

        private void RefreshInventory(bool resetScroll)
        {
            if (_content == null)
                return;

            ClearRows();

            Dictionary<string, float> totals = ResolveCurrentWarehouseTotals(out string sourceDescription);
            var rows = new List<ResourceRowModel>();

            if (totals != null)
            {
                foreach (var pair in totals)
                {
                    if (string.IsNullOrWhiteSpace(pair.Key) || pair.Value <= 0.0001f)
                        continue;

                    ResolvePresentation(pair.Key, out string displayName, out Sprite icon, out EconomyResourceCategory category);
                    if (!MatchesFilter(category))
                        continue;

                    rows.Add(new ResourceRowModel
                    {
                        ResourceId = pair.Key,
                        DisplayName = displayName,
                        Amount = pair.Value,
                        Icon = icon,
                        Category = category,
                    });
                }
            }

            rows.Sort(CompareRows);

            for (int i = 0; i < rows.Count; i++)
                CreateResourceRow(rows[i], i);

            bool empty = rows.Count == 0;
            if (_emptyState != null)
            {
                _emptyState.gameObject.SetActive(empty);
                _emptyState.text = BuildEmptyState(sourceDescription);
            }

            if (_scrollRect != null && resetScroll)
                _scrollRect.verticalNormalizedPosition = 1f;

            Canvas.ForceUpdateCanvases();
        }

        private Dictionary<string, float> ResolveCurrentWarehouseTotals(out string sourceDescription)
        {
            sourceDescription = "warehouse";

            if (_economyManager == null)
            {
                sourceDescription = "economy-unavailable";
                return new Dictionary<string, float>(StringComparer.Ordinal);
            }

            string ownerId = EconomyManager.DefaultOwnerId;
            bool hasBuilding = false;

            if (_hasSelectedPosition)
            {
                hasBuilding = _economyManager.TryGetBuildingAtPosition(
                    _selectedPosition,
                    out _,
                    out string buildingOwnerId);

                if (!string.IsNullOrWhiteSpace(buildingOwnerId))
                    ownerId = buildingOwnerId.Trim();

                if (_economyManager.TryGetSettlementByPosition(_selectedPosition, out var settlement)
                    && settlement != null)
                {
                    sourceDescription = "warehouse";
                    return _economyManager.GetWarehouseResourceTotalsByPosition(_selectedPosition);
                }
            }

            // A warehouse can exist before a settlement/town hall is resolved.
            // In that state starter resources are still in the owner pool, so show
            // the real pre-settlement inventory instead of an empty fake warehouse.
            sourceDescription = hasBuilding ? "owner-pool-before-settlement" : "owner-pool-fallback";
            return _economyManager.GetOwnerPoolResourceTotals(ownerId);
        }

        private void ClearRows()
        {
            for (int i = _content.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(_content.GetChild(i).gameObject);
        }

        private void CreateResourceRow(ResourceRowModel model, int index)
        {
            var row = new GameObject($"ResourceRow_{index:00}_{SanitizeName(model.ResourceId)}", typeof(RectTransform));
            row.layer = _content.gameObject.layer;
            row.transform.SetParent(_content, false);

            var rowImage = row.AddComponent<Image>();
            rowImage.color = index % 2 == 0
                ? new Color(1f, 1f, 1f, 0.045f)
                : new Color(1f, 1f, 1f, 0.018f);
            rowImage.raycastTarget = false;

            var layoutElement = row.AddComponent<LayoutElement>();
            layoutElement.minHeight = 34f;
            layoutElement.preferredHeight = 34f;

            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(6, 8, 4, 4);
            layout.spacing = 7f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var iconObject = new GameObject("Icon", typeof(RectTransform));
            iconObject.layer = row.layer;
            iconObject.transform.SetParent(row.transform, false);
            var iconImage = iconObject.AddComponent<Image>();
            iconImage.sprite = model.Icon;
            iconImage.preserveAspect = true;
            iconImage.color = model.Icon != null ? Color.white : new Color(1f, 1f, 1f, 0.08f);
            iconImage.raycastTarget = false;
            var iconLayout = iconObject.AddComponent<LayoutElement>();
            iconLayout.minWidth = 24f;
            iconLayout.preferredWidth = 24f;
            iconLayout.minHeight = 24f;
            iconLayout.preferredHeight = 24f;

            var nameText = CreateRowText("Name", model.DisplayName, row.transform);
            nameText.alignment = TextAlignmentOptions.MidlineLeft;
            nameText.fontSize = 16f;
            var nameLayout = nameText.gameObject.AddComponent<LayoutElement>();
            nameLayout.flexibleWidth = 1f;
            nameLayout.minWidth = 80f;

            var amountText = CreateRowText("Amount", FormatAmount(model.Amount), row.transform);
            amountText.alignment = TextAlignmentOptions.MidlineRight;
            amountText.fontSize = 16f;
            amountText.fontStyle = FontStyles.Bold;
            var amountLayout = amountText.gameObject.AddComponent<LayoutElement>();
            amountLayout.minWidth = 64f;
            amountLayout.preferredWidth = 72f;
        }

        private TextMeshProUGUI CreateRowText(string name, string value, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);

            var text = go.AddComponent<TextMeshProUGUI>();
            text.text = value ?? string.Empty;
            text.color = Color.white;
            text.raycastTarget = false;
            text.overflowMode = TextOverflowModes.Ellipsis;

            if (_descriptionText != null && _descriptionText.font != null)
                text.font = _descriptionText.font;

            return text;
        }

        private void ResolvePresentation(
            string resourceId,
            out string displayName,
            out Sprite icon,
            out EconomyResourceCategory category)
        {
            // Technical ResourceId is never player-facing. If the database
            // cannot resolve presentation data, show a neutral label instead.
            displayName = "Ресурс";
            icon = null;
            category = EconomyResourceCategory.None;

            var resources = _economyDatabase?.Resources;
            if (resources == null)
                return;

            for (int i = 0; i < resources.Count; i++)
            {
                var resource = resources[i];
                if (resource == null || !string.Equals(resource.Id, resourceId, StringComparison.Ordinal))
                    continue;

                if (!string.IsNullOrWhiteSpace(resource.DisplayName))
                    displayName = resource.DisplayName.Trim();

                icon = resource.Icon;
                category = resource.Category;
                return;
            }
        }

        private bool MatchesFilter(EconomyResourceCategory category)
        {
            switch (_filter)
            {
                case ResourceFilter.Food:
                    return category == EconomyResourceCategory.Food;
                case ResourceFilter.Materials:
                    return category == EconomyResourceCategory.Materials;
                default:
                    return true;
            }
        }

        private void ApplyFilterVisuals()
        {
            SetButtonSelected(_allButton, _filter == ResourceFilter.All);
            SetButtonSelected(_foodButton, _filter == ResourceFilter.Food);
            SetButtonSelected(_materialsButton, _filter == ResourceFilter.Materials);
        }

        private static void SetButtonSelected(Button button, bool selected)
        {
            if (button == null || button.targetGraphic is not Image image)
                return;

            image.color = selected
                ? new Color(0.30f, 0.46f, 0.62f, 0.98f)
                : new Color(0.16f, 0.18f, 0.21f, 0.96f);
        }

        private static string StripConstructionCostBlock(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return string.Empty;

            string[] lines = content.Replace("\r", string.Empty).Split('\n');
            var kept = new List<string>(lines.Length);
            bool skippingCostBullets = false;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i] ?? string.Empty;
                string trimmed = line.Trim();

                if (trimmed.StartsWith("Вартість будівництва", StringComparison.OrdinalIgnoreCase)
                    || trimmed.StartsWith("Construction cost", StringComparison.OrdinalIgnoreCase)
                    || trimmed.StartsWith("Потрібно для будівництва", StringComparison.OrdinalIgnoreCase))
                {
                    skippingCostBullets = true;
                    continue;
                }

                if (skippingCostBullets)
                {
                    if (trimmed.StartsWith("-", StringComparison.Ordinal)
                        || trimmed.StartsWith("•", StringComparison.Ordinal)
                        || string.IsNullOrWhiteSpace(trimmed))
                    {
                        continue;
                    }

                    skippingCostBullets = false;
                }

                kept.Add(line);
            }

            while (kept.Count > 0 && string.IsNullOrWhiteSpace(kept[kept.Count - 1]))
                kept.RemoveAt(kept.Count - 1);

            return string.Join("\n", kept);
        }

        private static bool IsWarehouseSelection(string title, string objectId)
        {
            string combined = ((title ?? string.Empty) + " " + (objectId ?? string.Empty)).ToLowerInvariant();
            return combined.Contains("склад")
                   || combined.Contains("warehouse")
                   || combined.Contains("storage");
        }

        private string BuildEmptyState(string sourceDescription)
        {
            switch (_filter)
            {
                case ResourceFilter.Food:
                    return "Їжі в цій групі немає";
                case ResourceFilter.Materials:
                    return "Матеріалів у цій групі немає";
            }

            if (sourceDescription == "economy-unavailable")
                return "Економіка недоступна";

            return "Ресурсів немає";
        }

        private static int CompareRows(ResourceRowModel left, ResourceRowModel right)
        {
            int categoryComparison = CategoryOrder(left.Category).CompareTo(CategoryOrder(right.Category));
            if (categoryComparison != 0)
                return categoryComparison;

            return string.Compare(left.DisplayName, right.DisplayName, StringComparison.CurrentCultureIgnoreCase);
        }

        private static int CategoryOrder(EconomyResourceCategory category)
        {
            switch (category)
            {
                case EconomyResourceCategory.Food:
                    return 0;
                case EconomyResourceCategory.Materials:
                    return 1;
                case EconomyResourceCategory.Money:
                    return 2;
                default:
                    return 3;
            }
        }

        private static string FormatAmount(float amount)
        {
            return Mathf.Abs(amount - Mathf.Round(amount)) < 0.001f
                ? Mathf.RoundToInt(amount).ToString()
                : amount.ToString("0.#");
        }

        private static string SanitizeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "resource";

            char[] chars = value.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];
                if (!char.IsLetterOrDigit(c) && c != '_' && c != '-')
                    chars[i] = '_';
            }

            return new string(chars);
        }

        private sealed class ResourceRowModel
        {
            public string ResourceId;
            public string DisplayName;
            public float Amount;
            public Sprite Icon;
            public EconomyResourceCategory Category;
        }
    }
}
