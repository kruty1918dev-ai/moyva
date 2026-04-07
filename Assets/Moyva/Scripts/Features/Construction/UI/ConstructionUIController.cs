using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.UI
{
    /// <summary>
    /// Main adapter/presenter between Unity UI and <see cref="IConstructionService"/>.
    /// Add this MonoBehaviour to a scene GameObject and wire sub-panels via the Inspector.
    ///
    /// HOW TO WIRE IN UNITY:
    /// 1. Add this component to a UI panel GameObject.
    /// 2. Drag <see cref="BuildingSelectionPanelUI"/>, <see cref="ConstructionActionBarUI"/>,
    ///    and <see cref="ConstructionStatusUI"/> into the corresponding fields.
    /// 3. Add <see cref="ConstructionUIInstaller"/> to the SceneContext and assign this component.
    /// 4. Optionally wire buttons directly to the public action methods via the Inspector
    ///    (OnConfirmClicked, OnCancelClicked, OnUndoClicked, OnRedoClicked, OnBuildingSelected).
    /// </summary>
    public class ConstructionUIController : MonoBehaviour, IInitializable, IDisposable
    {
        [Header("Sub-panels (drag in Inspector)")]
        [Tooltip("Panel that lists all available buildings for selection.")]
        [SerializeField] private BuildingSelectionPanelUI selectionPanel;

        [Tooltip("Panel with Confirm / Cancel / Undo / Redo buttons.")]
        [SerializeField] private ConstructionActionBarUI actionBar;

        [Tooltip("Panel that shows the current placement/preview status.")]
        [SerializeField] private ConstructionStatusUI statusDisplay;

        // --- Injected by Zenject ---
        private IConstructionService _constructionService;
        private IBuildingRegistry _buildingRegistry;
        private SignalBus _signalBus;

        // --- Internal state ---
        private string _selectedBuildingId;
        private BuildingPreviewState _lastPreviewState;
        private Vector2Int _lastPreviewPosition;

        /// <summary>Zenject injection point. Do not call manually.</summary>
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

        /// <summary>Called by Zenject after injection. Subscribes to signals and populates UI.</summary>
        public void Initialize()
        {
            _signalBus.Subscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.Subscribe<BuildingCancelledSignal>(OnBuildingCancelled);
            _signalBus.Subscribe<BuildingPreviewChangedSignal>(OnBuildingPreviewChanged);

            if (selectionPanel != null)
                selectionPanel.OnBuildingClicked += OnBuildingSelected;

            if (actionBar != null)
            {
                actionBar.OnConfirmClicked += OnConfirmClicked;
                actionBar.OnCancelClicked += OnCancelClicked;
                actionBar.OnUndoClicked += OnUndoClicked;
                actionBar.OnRedoClicked += OnRedoClicked;
            }

            PopulateBuildingList();
            RefreshUI();
        }

        /// <summary>Called by Zenject on destroy. Unsubscribes from signals.</summary>
        public void Dispose()
        {
            _signalBus.Unsubscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.Unsubscribe<BuildingCancelledSignal>(OnBuildingCancelled);
            _signalBus.Unsubscribe<BuildingPreviewChangedSignal>(OnBuildingPreviewChanged);

            if (selectionPanel != null)
                selectionPanel.OnBuildingClicked -= OnBuildingSelected;

            if (actionBar != null)
            {
                actionBar.OnConfirmClicked -= OnConfirmClicked;
                actionBar.OnCancelClicked -= OnCancelClicked;
                actionBar.OnUndoClicked -= OnUndoClicked;
                actionBar.OnRedoClicked -= OnRedoClicked;
            }
        }

        // -----------------------------------------------------------------------
        // Public action methods — wire to Button.onClick via Inspector or code
        // -----------------------------------------------------------------------

        /// <summary>
        /// Confirm all pending placements.
        /// Wire: Confirm button → OnClick → this method.
        /// </summary>
        public void OnConfirmClicked() => _constructionService.Confirm();

        /// <summary>
        /// Cancel the current build session.
        /// Wire: Cancel button → OnClick → this method.
        /// </summary>
        public void OnCancelClicked() => _constructionService.Cancel();

        /// <summary>
        /// Undo the last placement.
        /// Wire: Undo button → OnClick → this method.
        /// </summary>
        public void OnUndoClicked() => _constructionService.UndoLast();

        /// <summary>
        /// Redo the last undone placement.
        /// Wire: Redo button → OnClick → this method.
        /// </summary>
        public void OnRedoClicked() => _constructionService.RedoLast();

        /// <summary>
        /// Select a building for placement.
        /// Called automatically by <see cref="BuildingSelectionPanelUI"/>.
        /// Can also be called directly with a building ID string.
        /// </summary>
        public void OnBuildingSelected(string buildingId)
        {
            _selectedBuildingId = buildingId;
            _constructionService.SelectBuilding(buildingId);
            RefreshUI();
        }

        /// <summary>
        /// Forward a tile selection to the construction service.
        /// Call this when the player clicks a tile on the map.
        /// Example: tileClickHandler calls uiController.OnTileSelected(gridPos).
        /// </summary>
        public void OnTileSelected(Vector2Int position) =>
            _constructionService.TryPreviewAt(position);

        // -----------------------------------------------------------------------
        // Signal handlers
        // -----------------------------------------------------------------------

        private void OnBuildingPlaced(BuildingPlacedSignal signal)
        {
            RefreshUI();
        }

        private void OnBuildingCancelled(BuildingCancelledSignal signal)
        {
            _selectedBuildingId = null;
            _lastPreviewState = BuildingPreviewState.None;
            RefreshUI();
        }

        private void OnBuildingPreviewChanged(BuildingPreviewChangedSignal signal)
        {
            _lastPreviewState = signal.PreviewState;
            _lastPreviewPosition = signal.Position;
            RefreshUI();
        }

        // -----------------------------------------------------------------------
        // Internal helpers
        // -----------------------------------------------------------------------

        private void PopulateBuildingList()
        {
            if (selectionPanel == null || _buildingRegistry == null)
                return;

            var buildings = _buildingRegistry.GetAll();
            var items = new List<BuildingListItemData>(buildings.Length);
            foreach (var b in buildings)
                items.Add(new BuildingListItemData(b.Id, b.DisplayName, b.Category));

            selectionPanel.Populate(items);
        }

        private void RefreshUI()
        {
            var state = new ConstructionUIState(
                _constructionService.State,
                _selectedBuildingId,
                _lastPreviewState,
                _lastPreviewPosition);

            if (actionBar != null)
                actionBar.SetState(state);

            if (statusDisplay != null)
                statusDisplay.UpdateState(state);
        }
    }
}
