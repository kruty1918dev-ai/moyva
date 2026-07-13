using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using TMPro;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.UI
{
    /// <summary>
    /// Адаптер між Construction UI панелями та <see cref="IConstructionService"/>.
    /// Керує ТІЛЬКИ своїми панелями (показ/сховання), реагуючи на сигнали та дії гравця.
    /// Перемикання режимів гри керує окремо — через GameMode систему.
    ///
    /// ФУНКЦІОНАЛЬНІСТЬ:
    /// — Показання/сховання Construction UI панелей при зміні режиму гри.
    /// — Вибір будівлі → виділення іконки у меню → preview на тайлі.
    /// — Підтвердження, скасування, Undo/Redo (тільки будівництво).
    /// — Режим знесення (тільки будівлі, поставлені гравцем).
    ///
    /// ЯК ПІДКЛЮЧИТИ В UNITY:
    /// 1. Додай компонент до GameObject U корені Construction UI (або якому-то його дочірньому елементу).
    /// 2. Перетягни <see cref="BuildingSelectionPanelUI"/>, <see cref="ConstructionActionBarUI"/>,
    ///    <see cref="ConstructionStatusUI"/> у відповідні поля.
    /// 3. Призначи <b>constructionUIRoot</b> — кореневий GameObject UI будівництва (якщо null — використається gameObject).
    /// 4. Додай <see cref="ConstructionUIInstaller"/> до SceneContext.
    ///
    /// ПРИМІТКА: Управління ігровим режимом (входу/виходу з режиму будівництва) повинно бути
    /// пов'язано з кнопками на ігровому UI (не тут!). Див. GameModeChangeRequestRouter.
    /// </summary>
    public class ConstructionUIController : MonoBehaviour, IInitializable, IDisposable
    {
        [Header("Підпанелі (перетягни в Inspector)")]
        [Tooltip("Панель вибору будівель.")]
        [SerializeField] private BuildingSelectionPanelUI selectionPanel;

        [Tooltip("Панель кнопок Confirm / Cancel / Undo / Redo / Знести.")]
        [SerializeField] private ConstructionActionBarUI actionBar;

        [Tooltip("Панель статусу розміщення/preview.")]
        [SerializeField] private ConstructionStatusUI statusDisplay;

        [Header("Construction UI (перетягни в Inspector)")]
        [Tooltip("Кореневий GameObject UI будівництва. Якщо null — використовується gameObject цього компонента.")]
        [SerializeField] private GameObject constructionUIRoot;

        [Header("Налаштування превью більд панелі (опціонально)")]
        [SerializeField] private GameObject previewInfoPanelPrefab;
        private const string previewInfoPanelTextLabelKeyWord = "Label";
        private const string previewInfoPanelTextInfoKeyWord = "Info";
        private const int previewInfoMaxResourcesLines = 6;
        private const float previewInfoHeaderFontSize = 20f;
        private const float previewInfoBodyFontSize = 14f;
        private const float previewInfoBodyLineSpacing = 8f;

        // --- Інжектується Zenject ---
        private IConstructionService _constructionService;
        private IBuildingRegistry _buildingRegistry;
        private IEconomyInfoMediator _economyInfoMediator;
        private SignalBus _signalBus;

        // --- Внутрішній стан ---
        private string _selectedBuildingId;
        private BuildingPreviewState _lastPreviewState;
        private Vector2Int _lastPreviewPosition;
        private bool _isConstructionModeActive;
        private readonly BuildingMenuFactory _menuFactory = new BuildingMenuFactory();
        private GameObject _previewInfoPanelInstance;
        private TMP_Text _previewInfoPanelLabel;
        private TMP_Text _previewInfoPanelInfo;
        private bool _isPreviewInfoVisible;
        private bool _isPreviewInfoPinned;
        private Vector2Int _pinnedPreviewPosition;
        private string _pinnedPreviewBuildingId;

        /// <summary>Точка ін'єкції Zenject. Не викликати вручну.</summary>
        [Inject]
        public void Construct(
            IConstructionService constructionService,
            IBuildingRegistry buildingRegistry,
            SignalBus signalBus,
            [InjectOptional] IEconomyInfoMediator economyInfoMediator = null)
        {
            _constructionService = constructionService;
            _buildingRegistry = buildingRegistry;
            _signalBus = signalBus;
            _economyInfoMediator = economyInfoMediator;
        }

        /// <summary>Викликається Zenject після ін'єкції. Підписується на сигнали та заповнює UI.</summary>
        public void Initialize()
        {
            if (_signalBus == null || _constructionService == null || _buildingRegistry == null)
            {
                Debug.LogError("[ConstructionUIController] Zenject не інʼєктував усі залежності. Перевір SceneContext installers для Construction та Signals.", this);
                return;
            }

            _signalBus.Subscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.Subscribe<BuildingCancelledSignal>(OnBuildingCancelled);
            _signalBus.Subscribe<BuildingPreviewChangedSignal>(OnBuildingPreviewChanged);
            _signalBus.Subscribe<BuildingSelectionChangedSignal>(OnBuildingSelectionChanged);
            _signalBus.Subscribe<GameModeChangedSignal>(OnGameModeChanged);
            _signalBus.Subscribe<TileClickedSignal>(OnTileClicked);

            if (selectionPanel != null)
                selectionPanel.OnBuildingClicked += OnBuildingSelected;
            else
                Debug.LogWarning("[ConstructionUIController] Поле 'selectionPanel' не призначено. Меню будівель не відображатиметься.", this);

            if (actionBar != null)
            {
                actionBar.OnConfirmClicked  += OnConfirmClicked;
                actionBar.OnCancelClicked   += OnCancelClicked;
                actionBar.OnUndoClicked     += OnUndoClicked;
                actionBar.OnRedoClicked     += OnRedoClicked;
                actionBar.OnDemolishToggled += OnDemolishToggled;
            }
            else
            {
                Debug.LogWarning("[ConstructionUIController] Поле 'actionBar' не призначено. Кнопки дій не будуть підключені.", this);
            }



            // Ховаємо UI будівництва при старті
            SetConstructionUIVisible(false);
            EnsurePreviewInfoPanel();
            HidePreviewInfoPanel();

            PopulateBuildingList();
            RefreshUI();
        }

        /// <summary>Викликається Zenject при знищенні. Відписується від сигналів.</summary>
        public void Dispose()
        {
            if (_signalBus != null)
            {
                _signalBus.TryUnsubscribe<BuildingPlacedSignal>(OnBuildingPlaced);
                _signalBus.TryUnsubscribe<BuildingCancelledSignal>(OnBuildingCancelled);
                _signalBus.TryUnsubscribe<BuildingPreviewChangedSignal>(OnBuildingPreviewChanged);
                _signalBus.TryUnsubscribe<BuildingSelectionChangedSignal>(OnBuildingSelectionChanged);
                _signalBus.TryUnsubscribe<GameModeChangedSignal>(OnGameModeChanged);
                _signalBus.TryUnsubscribe<TileClickedSignal>(OnTileClicked);
            }

            if (selectionPanel != null)
                selectionPanel.OnBuildingClicked -= OnBuildingSelected;

            if (actionBar != null)
            {
                actionBar.OnConfirmClicked  -= OnConfirmClicked;
                actionBar.OnCancelClicked   -= OnCancelClicked;
                actionBar.OnUndoClicked     -= OnUndoClicked;
                actionBar.OnRedoClicked     -= OnRedoClicked;
                actionBar.OnDemolishToggled -= OnDemolishToggled;
            }

            if (_previewInfoPanelInstance != null
                && previewInfoPanelPrefab != null
                && !previewInfoPanelPrefab.scene.IsValid())
            {
                Destroy(_previewInfoPanelInstance);
            }
        }

        // -----------------------------------------------------------------------
        // Публічні методи дій — підключи до Button.onClick через Inspector або код
        // -----------------------------------------------------------------------

        /// <summary>
        /// Запросити вхід в режим будівництва.</summary>
        /// <remarks>
        /// Підключи до кнопки «Будівництво» основного UI.
        /// Це надсилає запит GameModeChangeRequestRouter, який вирішує чи дозволити.
        /// </remarks>
        public void EnterConstructionMode()
        {
            RequestEnterConstructionMode();
        }

        /// <summary>
        /// Підтвердити всі pending-розміщення.
        /// Підключи: Confirm button → OnClick → цей метод.
        /// </summary>
        public void OnConfirmClicked()
        {
            _signalBus.Fire(new PlaceBuildingConfirmRequestSignal());

            if (_constructionService.State == BuildingPlacementState.Placing || _constructionService.IsDemolishMode)
                _constructionService.Confirm();
        }

        /// <summary>
        /// Скасувати поточну сесію будівництва.</summary>
        /// <remarks>
        /// Підключи: Cancel button → OnClick → цей метод.
        /// Це ТІЛЬКИ скасовує будівництво. Вихід з режиму (якщо потрібен) керується окремо.
        /// </remarks>
        public void OnCancelClicked()
        {
            _constructionService.Cancel();
        }

        /// <summary>
        /// Відмінити останнє розміщення.
        /// Підключи: Undo button → OnClick → цей метод.
        /// </summary>
        public void OnUndoClicked() => _constructionService.UndoLast();

        /// <summary>
        /// Повернути скасоване розміщення.
        /// Підключи: Redo button → OnClick → цей метод.
        /// </summary>
        public void OnRedoClicked() => _constructionService.RedoLast();

        /// <summary>
        /// Перемикач режиму знесення.
        /// Підключи: Demolish button → OnClick → цей метод.
        /// </summary>
        public void OnDemolishToggled() => _constructionService.ToggleDemolishMode();

        /// <summary>
        /// Вибрати будівлю для розміщення.
        /// Викликається автоматично через <see cref="BuildingSelectionPanelUI"/>.
        /// </summary>
        public void OnBuildingSelected(string buildingId)
        {
            _selectedBuildingId = buildingId;
            _constructionService.SelectBuilding(buildingId);
            _isPreviewInfoPinned = false;
            _pinnedPreviewBuildingId = null;

            if (selectionPanel != null)
                selectionPanel.SetSelectedBuilding(buildingId);

            if (_isConstructionModeActive && !string.IsNullOrWhiteSpace(buildingId))
                ShowPreviewInfoPanel(buildingId, _lastPreviewPosition, pinToPreviewObject: false);

            RefreshUI();
        }

        /// <summary>
        /// Передає вибір тайлу до сервісу будівництва.
        /// Викликай у TileClickHandler або InputHandler коли гравець клікає по карті.
        /// </summary>
        public void OnTileSelected(Vector2Int position)
        {
            if (!_constructionService.IsDemolishMode && TryTogglePreviewInfoByPosition(position))
                return;

            if (_constructionService.IsDemolishMode)
                _constructionService.TryDemolishAt(position);
            else
                _constructionService.TryPreviewAt(position);
        }

        // -----------------------------------------------------------------------
        // Обробники сигналів
        // -----------------------------------------------------------------------

        private void OnBuildingPlaced(BuildingPlacedSignal signal)
        {
            PopulateBuildingList();
            RefreshUI();
        }

        private void OnBuildingCancelled(BuildingCancelledSignal signal)
        {
            _selectedBuildingId = null;
            _lastPreviewState = BuildingPreviewState.None;
            HidePreviewInfoPanel();
            if (selectionPanel != null)
                selectionPanel.ClearSelection();
            RefreshUI();
        }

        private void OnBuildingPreviewChanged(BuildingPreviewChangedSignal signal)
        {
            _lastPreviewState = signal.PreviewState;
            _lastPreviewPosition = signal.Position;

            if (_isPreviewInfoPinned
                && !string.IsNullOrWhiteSpace(_pinnedPreviewBuildingId)
                && signal.PreviewState != BuildingPreviewState.None
                && string.Equals(signal.BuildingId, _pinnedPreviewBuildingId, StringComparison.Ordinal)
                && _constructionService.HasPendingPlacementAt(signal.Position))
            {
                _pinnedPreviewPosition = signal.Position;
                ShowPreviewInfoPanel(signal.BuildingId, signal.Position, pinToPreviewObject: true);
            }

            RefreshUI();
        }

        private void OnBuildingSelectionChanged(BuildingSelectionChangedSignal signal)
        {
            _selectedBuildingId = signal.IsDemolishMode ? null : signal.BuildingId;
            _isPreviewInfoPinned = false;
            _pinnedPreviewBuildingId = null;

            if (selectionPanel != null)
                selectionPanel.SetSelectedBuilding(_selectedBuildingId);

            if (string.IsNullOrWhiteSpace(_selectedBuildingId))
                HidePreviewInfoPanel();

            RefreshUI();
        }

        private void OnGameModeChanged(GameModeChangedSignal signal)
        {
            _isConstructionModeActive = signal.NewMode == GameModeType.Construction;
            SetConstructionUIVisible(_isConstructionModeActive);

            if (!_isConstructionModeActive)
            {
                HidePreviewInfoPanel();
            }
            else if (!string.IsNullOrWhiteSpace(_selectedBuildingId))
            {
                ShowPreviewInfoPanel(_selectedBuildingId, _lastPreviewPosition, pinToPreviewObject: false);
            }

            RefreshUI();
        }

        private void OnTileClicked(TileClickedSignal signal)
        {
            if (!_isConstructionModeActive)
                return;

            TryTogglePreviewInfoByPosition(signal.Position);
        }

        // -----------------------------------------------------------------------
        // Допоміжні методи
        // -----------------------------------------------------------------------

        private void SetConstructionUIVisible(bool visible)
        {
            // Якщо constructionUIRoot не задано — не ховаємо gameObject контролера
            // (він має залишатись активним для обробки сигналів).
            if (constructionUIRoot != null)
                constructionUIRoot.SetActive(visible);
        }

        private void RequestEnterConstructionMode()
        {
            _signalBus.Fire(new GameModeChangeRequestedSignal { RequestedMode = GameModeType.Construction });
        }

        private void PopulateBuildingList()
        {
            if (selectionPanel == null || _buildingRegistry == null)
                return;

            var buildings = _buildingRegistry.GetAll();
            bool ownerHasCastle = OwnerHasAnyCastle();
            var items = _menuFactory.BuildMenuItems(
                buildings,
                _buildingRegistry,
                this,
                definition => ownerHasCastle || (definition != null && BuildingDefinitionCapabilities.IsCastle(definition)),
                definition => !ownerHasCastle || definition == null || !BuildingDefinitionCapabilities.IsCastle(definition));

            Debug.Log($"[Construction UI] Ініціалізовано меню будівель. Знайдено елементів: {items.Count}.", this);

            selectionPanel.Populate(items);
        }

        private bool OwnerHasAnyCastle()
        {
            if (_constructionService == null || _buildingRegistry == null)
                return false;

            string ownerId = _constructionService.GetActiveOwner();
            var all = _buildingRegistry.GetAll();
            if (all == null || all.Length == 0)
                return false;

            for (int i = 0; i < all.Length; i++)
            {
                var def = all[i];
                if (def == null || string.IsNullOrWhiteSpace(def.Id))
                    continue;

                if (!BuildingDefinitionCapabilities.IsCastle(def))
                    continue;

                if (_constructionService.HasPlacedBuilding(def.Id, ownerId))
                    return true;
            }

            return false;
        }

        private void RefreshUI()
        {
            var state = new ConstructionUIState(
                _constructionService.State,
                _selectedBuildingId,
                _lastPreviewState,
                _lastPreviewPosition,
                _constructionService.IsDemolishMode,
                _isConstructionModeActive);

            if (actionBar != null)
                actionBar.SetState(state);

            if (statusDisplay != null)
                statusDisplay.UpdateState(state);

            if (_isConstructionModeActive && string.IsNullOrWhiteSpace(_selectedBuildingId) && !_isPreviewInfoPinned)
                HidePreviewInfoPanel();
        }

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

        private string BuildPreviewInfoText(BuildingDefinition definition, Vector2Int position)
        {
            var sb = new StringBuilder();

            var displayName = string.IsNullOrWhiteSpace(definition.DisplayName)
                ? definition.Id
                : definition.DisplayName;

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
            AppendOwnerResourcesBlock(sb);
            return sb.ToString().TrimEnd();
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

            var topResources = totals
                .OrderByDescending(x => x.Value)
                .ThenBy(x => x.Key, StringComparer.Ordinal)
                .Take(previewInfoMaxResourcesLines)
                .ToList();

            foreach (var resource in topResources)
                sb.AppendLine($"• {ResolveResourceDisplayName(resource.Key)}: {resource.Value:0.#}");

            int hiddenCount = totals.Count - topResources.Count;
            if (hiddenCount > 0)
                sb.AppendLine($"• + ще {hiddenCount}");
        }

        private string ResolveResourceDisplayName(string resourceId)
            => _economyInfoMediator?.GetResourceDisplayName(resourceId)
               ?? (string.IsNullOrWhiteSpace(resourceId) ? "<невідомий ресурс>" : resourceId.Trim());
    }
}
