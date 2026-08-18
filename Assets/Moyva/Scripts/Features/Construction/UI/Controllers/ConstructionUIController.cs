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
        private const string ModuleLogTag =
            "[MoyvaConstructionModules]";
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
        private const float previewInfoHeaderFontSize = 24f; // MOYVA_GAMEPLAY_UI_PASS73
        private const float previewInfoBodyFontSize = 18f; // MOYVA_GAMEPLAY_UI_PASS73
        private const float previewInfoBodyLineSpacing = 4f; // MOYVA_GAMEPLAY_UI_PASS73

        // --- Інжектується Zenject ---
        private IConstructionService _constructionService;
        private IBuildingRegistry _buildingRegistry;
        private IEconomyInfoMediator _economyInfoMediator;
        private IConstructionPlacementQuery _placementQuery;
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
        private readonly Dictionary<string, string>
            _buildingUnavailableReasons =
                new Dictionary<string, string>(StringComparer.Ordinal);
        private bool _buildingListRefreshRequested;
        private readonly StringBuilder _previewInfoBuilder =
            new StringBuilder(512);
        private readonly List<KeyValuePair<string, float>>
            _previewResourceScratch =
                new List<KeyValuePair<string, float>>(16);

        /// <summary>Точка ін'єкції Zenject. Не викликати вручну.</summary>
        [Inject]
        public void Construct(
            IConstructionService constructionService,
            IBuildingRegistry buildingRegistry,
            SignalBus signalBus,
            [InjectOptional] IEconomyInfoMediator economyInfoMediator = null,
            [InjectOptional] IConstructionPlacementQuery placementQuery = null)
        {
            _constructionService = constructionService;
            _buildingRegistry = buildingRegistry;
            _signalBus = signalBus;
            _economyInfoMediator = economyInfoMediator;
            _placementQuery = placementQuery;
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
            _signalBus.Subscribe<BuildingDemolishedSignal>(
                OnBuildingDemolished);
            _signalBus.Subscribe<BuildingCancelledSignal>(OnBuildingCancelled);
            _signalBus.Subscribe<BuildingPreviewChangedSignal>(OnBuildingPreviewChanged);
            _signalBus.Subscribe<BuildingSelectionChangedSignal>(OnBuildingSelectionChanged);
            _signalBus.Subscribe<GameModeChangedSignal>(OnGameModeChanged);
            _signalBus.Subscribe<SettlementResourceChangedSignal>(OnSettlementResourceChanged);
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
                _signalBus.TryUnsubscribe<BuildingDemolishedSignal>(
                    OnBuildingDemolished);
                _signalBus.TryUnsubscribe<BuildingCancelledSignal>(OnBuildingCancelled);
                _signalBus.TryUnsubscribe<BuildingPreviewChangedSignal>(OnBuildingPreviewChanged);
                _signalBus.TryUnsubscribe<BuildingSelectionChangedSignal>(OnBuildingSelectionChanged);
                _signalBus.TryUnsubscribe<GameModeChangedSignal>(OnGameModeChanged);
                _signalBus.TryUnsubscribe<SettlementResourceChangedSignal>(OnSettlementResourceChanged);
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

        private void LateUpdate()
        {
            if (!_buildingListRefreshRequested)
                return;

            _buildingListRefreshRequested = false;
            PopulateBuildingList();
            SynchronizeCastleBootstrapUi();
            RefreshUI();
        }

        private void RequestBuildingListRefresh()
            => _buildingListRefreshRequested = true;

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
            if (TryGetSelectionAvailability(
                    buildingId,
                    out ConstructionSelectionAvailabilityResult availability)
                && !availability.CanSelect)
            {
                string reason = string.IsNullOrWhiteSpace(availability.Reason)
                    ? "Будівля зараз недоступна."
                    : availability.Reason;
                Debug.LogWarning(
                    $"[MoyvaConstructionAvailability] ui-selection-rejected " +
                    $"building='{buildingId}' code='{availability.ReasonCode ?? "unavailable"}' " +
                    $"reason='{reason}'",
                    this);
                RequestBuildingListRefresh();
                return;
            }

            _constructionService.SelectBuilding(buildingId);
            string acceptedBuildingId = _constructionService.GetSelectedBuildingId();
            if (!string.Equals(
                    acceptedBuildingId,
                    buildingId,
                    StringComparison.Ordinal))
            {
                Debug.LogWarning(
                    $"[MoyvaConstructionAvailability] ui-selection-service-rejected " +
                    $"building='{buildingId}' accepted='{acceptedBuildingId ?? "none"}'",
                    this);
                RequestBuildingListRefresh();
                return;
            }

            _selectedBuildingId = acceptedBuildingId;
            _isPreviewInfoPinned = false;
            _pinnedPreviewBuildingId = null;

            if (selectionPanel != null)
                selectionPanel.SetSelectedBuilding(_selectedBuildingId);

            if (_isConstructionModeActive && !string.IsNullOrWhiteSpace(_selectedBuildingId))
                ShowPreviewInfoPanel(_selectedBuildingId, _lastPreviewPosition, pinToPreviewObject: false);

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

        private void OnSettlementResourceChanged(
            SettlementResourceChangedSignal signal)
        {
            RequestBuildingListRefresh();
            Debug.Log(
                $"[MoyvaConstructionAvailability] resource-change " +
                $"owner='{signal.OwnerId}' resource='{signal.ResourceId}' " +
                $"new={signal.NewAmount:0.###} delta={signal.Delta:0.###}",
                this);
        }

        private static readonly ProfilerMarker BuildingPlacedUiMarker =
            new("Moyva.BuildCommit.Subscriber.ConstructionUI");

        private void OnBuildingPlaced(BuildingPlacedSignal signal)
        {
            using var marker = BuildingPlacedUiMarker.Auto();
            RequestBuildingListRefresh();
            RefreshUI();
        }

        private void OnBuildingDemolished(
            BuildingDemolishedSignal signal)
        {
            RequestBuildingListRefresh();
            RefreshUI();
        }

        private void OnBuildingCancelled(BuildingCancelledSignal signal)
        {
            _selectedBuildingId = null;
            _lastPreviewState = BuildingPreviewState.None;
            HidePreviewInfoPanel();
            if (selectionPanel != null)
                selectionPanel.ClearSelection();
            SynchronizeCastleBootstrapUi();
            RefreshUI();
        }

        private void OnBuildingPreviewChanged(BuildingPreviewChangedSignal signal)
        {
            // Pass 61: pending budget changes are reflected in toolbar.
            RequestBuildingListRefresh();
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
            else if (signal.PreviewState
                         == BuildingPreviewState.Unaffordable
                     && _constructionService.HasPendingPlacementAt(
                         signal.Position))
            {
                ShowPreviewInfoPanel(
                    signal.BuildingId,
                    signal.Position,
                    pinToPreviewObject: false);
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
            else
            {
                PopulateBuildingList();
                SynchronizeCastleBootstrapUi();

                if (!string.IsNullOrWhiteSpace(
                        _selectedBuildingId))
                {
                    ShowPreviewInfoPanel(
                        _selectedBuildingId,
                        _lastPreviewPosition,
                        pinToPreviewObject: false);
                }
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
            _buildingListRefreshRequested = false;

            if (selectionPanel == null || _buildingRegistry == null)
                return;

            var buildings = _buildingRegistry.GetAll();
            _buildingUnavailableReasons.Clear();
            var items = _menuFactory.BuildMenuItems(
                buildings,
                _buildingRegistry,
                this,
                IsBuildingAvailable,
                includeSelector:
                    ShouldIncludeBuildingInMenu,
                unavailableReasonSelector:
                    GetBuildingUnavailableReason);

            if (Debug.isDebugBuild)
            {
                Debug.Log(
                    $"{ModuleLogTag} menu rebuilt " +
                    $"items={items.Count} " +
                    $"owner={_constructionService?.GetActiveOwner()}",
                    this);
            }

            int disabledCount = items.Count(item => !item.IsInteractable);
            Debug.Log(
                $"[MoyvaConstructionAvailability] menu-summary " +
                $"items={items.Count} enabled={items.Count - disabledCount} " +
                $"disabled={disabledCount} owner='{_constructionService.GetActiveOwner()}' " +
                $"pending={_constructionService.GetPendingPlacements().Count}",
                this);

            selectionPanel.Populate(items);
        }

        private bool ShouldIncludeBuildingInMenu(
            BuildingDefinition definition)
        {
            if (definition == null
                || string.IsNullOrWhiteSpace(definition.Id))
            {
                return false;
            }

            if (_constructionService
                is not IConstructionBootstrapQuery bootstrap)
            {
                return true;
            }

            string ownerId =
                _constructionService.GetActiveOwner();
            if (bootstrap.RequiresInitialCastle(
                    ownerId,
                    out string requiredCastleId))
            {
                return string.Equals(
                    definition.Id,
                    requiredCastleId,
                    StringComparison.Ordinal);
            }

            return !bootstrap.IsCastleBuilding(
                definition.Id);
        }

        private void SynchronizeCastleBootstrapUi()
        {
            if (_constructionService
                is not IConstructionBootstrapQuery bootstrap)
            {
                return;
            }

            string ownerId =
                _constructionService.GetActiveOwner();
            if (bootstrap.RequiresInitialCastle(
                    ownerId,
                    out string castleBuildingId))
            {
                _selectedBuildingId =
                    _constructionService.GetSelectedBuildingId();

                if (string.IsNullOrWhiteSpace(_selectedBuildingId))
                {
                    _constructionService.SelectBuilding(castleBuildingId);
                    _selectedBuildingId =
                        _constructionService.GetSelectedBuildingId();
                }

                selectionPanel?.SetSelectedBuilding(
                    _selectedBuildingId);

                if (Debug.isDebugBuild)
                {
                    Debug.Log(
                        $"{ModuleLogTag} castle-bootstrap-ui " +
                        $"owner={ownerId} required=true " +
                        $"selected={_selectedBuildingId ?? "none"}",
                        this);
                }
                return;
            }

            if (!string.IsNullOrWhiteSpace(_selectedBuildingId)
                && bootstrap.IsCastleBuilding(_selectedBuildingId))
            {
                _selectedBuildingId = null;
                selectionPanel?.ClearSelection();
                HidePreviewInfoPanel();
            }
        }

        private bool IsBuildingAvailable(BuildingDefinition definition)
        {
            if (definition == null
                || string.IsNullOrWhiteSpace(definition.Id))
            {
                return false;
            }

            if (!TryGetSelectionAvailability(
                    definition.Id,
                    out ConstructionSelectionAvailabilityResult result))
            {
                // Compatibility fallback if an isolated test scene binds an
                // older construction service without the optional query.
                if (_placementQuery == null)
                    return true;

                ConstructionPlacementQueryResult legacy =
                    _placementQuery.EvaluatePlacement(
                        new ConstructionPlacementQueryRequest(
                            definition.Id,
                            _lastPreviewPosition,
                            includeResources: false,
                            ownerId: _constructionService?.GetActiveOwner(),
                            attemptSource:
                                ConstructionPlacementAttemptSource.Unknown,
                            allowUniquePreviewRelocation: false));
                _buildingUnavailableReasons[definition.Id] =
                    legacy.CanSelect ? null : legacy.Reason;
                return legacy.CanSelect;
            }

            _buildingUnavailableReasons[definition.Id] =
                result.CanSelect
                    ? null
                    : ConstructionPlacementReasonText.Resolve(
                        result.ReasonCode,
                        result.Reason);

            return result.CanSelect;
        }

        private bool TryGetSelectionAvailability(
            string buildingId,
            out ConstructionSelectionAvailabilityResult result)
        {
            if (_constructionService
                is IConstructionSelectionAvailabilityQuery availabilityQuery)
            {
                result = availabilityQuery.EvaluateSelectionAvailability(
                    buildingId,
                    _constructionService.GetActiveOwner(),
                    _lastPreviewPosition,
                    includePendingPlacements: true);
                return true;
            }

            result = default;
            return false;
        }

        private string GetBuildingUnavailableReason(
            BuildingDefinition definition)
        {
            if (definition == null
                || string.IsNullOrWhiteSpace(definition.Id))
            {
                return "Некоректна конфігурація будівлі.";
            }

            return _buildingUnavailableReasons.TryGetValue(
                definition.Id,
                out string reason)
                ? reason
                : null;
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
