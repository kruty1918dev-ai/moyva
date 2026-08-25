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
    /// Coordinates Construction UI state and delegates gameplay mutations.
    /// </summary>
    public partial class ConstructionUIController : MonoBehaviour, IInitializable, IDisposable
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
        private IConstructionSessionCommands _constructionService;
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
            IConstructionSessionCommands constructionService,
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

    }
}
