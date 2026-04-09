using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
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

        // --- Інжектується Zenject ---
        private IConstructionService _constructionService;
        private IBuildingRegistry _buildingRegistry;
        private SignalBus _signalBus;

        // --- Внутрішній стан ---
        private string _selectedBuildingId;
        private BuildingPreviewState _lastPreviewState;
        private Vector2Int _lastPreviewPosition;
        private bool _isConstructionModeActive;
        private readonly BuildingMenuFactory _menuFactory = new BuildingMenuFactory();

        /// <summary>Точка ін'єкції Zenject. Не викликати вручну.</summary>
        [Inject]
        public void Construct(
            IConstructionService constructionService,
            IBuildingRegistry buildingRegistry,
            SignalBus signalBus)
        {
            _constructionService = constructionService;
            _buildingRegistry = buildingRegistry;
            _signalBus = signalBus;
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
            _signalBus.Subscribe<GameModeChangedSignal>(OnGameModeChanged);

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
                _signalBus.TryUnsubscribe<GameModeChangedSignal>(OnGameModeChanged);
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
        public void OnConfirmClicked() => _constructionService.Confirm();

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

            if (selectionPanel != null)
                selectionPanel.SetSelectedBuilding(buildingId);

            RefreshUI();
        }

        /// <summary>
        /// Передає вибір тайлу до сервісу будівництва.
        /// Викликай у TileClickHandler або InputHandler коли гравець клікає по карті.
        /// </summary>
        public void OnTileSelected(Vector2Int position)
        {
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
            RefreshUI();
        }

        private void OnBuildingCancelled(BuildingCancelledSignal signal)
        {
            _selectedBuildingId = null;
            _lastPreviewState = BuildingPreviewState.None;
            if (selectionPanel != null)
                selectionPanel.ClearSelection();
            RefreshUI();
        }

        private void OnBuildingPreviewChanged(BuildingPreviewChangedSignal signal)
        {
            _lastPreviewState = signal.PreviewState;
            _lastPreviewPosition = signal.Position;
            RefreshUI();
        }

        private void OnGameModeChanged(GameModeChangedSignal signal)
        {
            _isConstructionModeActive = signal.NewMode == GameModeType.Construction;
            SetConstructionUIVisible(_isConstructionModeActive);
            RefreshUI();
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
            var items = _menuFactory.BuildMenuItems(buildings, _buildingRegistry, this);

            Debug.Log($"[Construction UI] Ініціалізовано меню будівель. Знайдено елементів: {items.Count}.", this);

            selectionPanel.Populate(items);
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
        }
    }
}
