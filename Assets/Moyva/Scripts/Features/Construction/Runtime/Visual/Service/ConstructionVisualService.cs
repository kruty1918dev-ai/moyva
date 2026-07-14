using System;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionVisualService : IInitializable, IDisposable, ITickable
    {
        private readonly SignalBus _signalBus;
        private readonly IConstructionVisualRootService _rootService;
        private readonly IConstructionPreviewVisualSignalHandler _previewSignals;
        private readonly IConstructionPlacedVisualSignalHandler _placedSignals;
        private readonly IConstructionInfluenceRadiusVisualService _radiusVisuals;
        private readonly IConstructionBuildGridOverlayService _buildGridOverlay;
        private readonly IConstructionBlockedFlashService _blockedFlashService;
        private readonly IConstructionPreviewVisualService _previewVisuals;
        private readonly IConstructionPlacedVisualService _placedVisuals;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IConstructionPlacementRulesProvider _placementRules;
        private int _gridInvalidationRadius;
        private int _localGridInvalidationRadius;
        private string _selectedBuildingId;

        [Inject]
        public ConstructionVisualService(
            SignalBus signalBus,
            IConstructionVisualRootService rootService,
            IConstructionPreviewVisualSignalHandler previewSignals,
            IConstructionPlacedVisualSignalHandler placedSignals,
            IConstructionInfluenceRadiusVisualService radiusVisuals,
            IConstructionBuildGridOverlayService buildGridOverlay,
            IConstructionBlockedFlashService blockedFlashService,
            IConstructionPreviewVisualService previewVisuals,
            IConstructionPlacedVisualService placedVisuals,
            [InjectOptional] IBuildingRegistry buildingRegistry = null,
            [InjectOptional] IConstructionPlacementRulesProvider placementRules = null)
        {
            _signalBus = signalBus;
            _rootService = rootService;
            _previewSignals = previewSignals;
            _placedSignals = placedSignals;
            _radiusVisuals = radiusVisuals;
            _buildGridOverlay = buildGridOverlay;
            _blockedFlashService = blockedFlashService;
            _previewVisuals = previewVisuals;
            _placedVisuals = placedVisuals;
            _buildingRegistry = buildingRegistry;
            _placementRules = placementRules;
        }

        public void Initialize()
        {
            Debug.Log("[ConstructionVisual] Initializing visual stack...");
            _rootService.EnsureRoots();
            ResolveGridInvalidationRadii(out _localGridInvalidationRadius, out _gridInvalidationRadius);
            _radiusVisuals.Initialize();
            _buildGridOverlay.Initialize();
            SubscribeSignals();
            Debug.Log("[ConstructionVisual] Initialized. Roots, overlays and signal subscriptions are ready.");
        }

        public void Dispose()
        {
            UnsubscribeSignals();
            _blockedFlashService?.Clear();
            _previewVisuals.Dispose();
            _placedVisuals.ClearDemolitionPreviewStyles();
            _placedVisuals.Clear();
            _radiusVisuals.Dispose();
            _buildGridOverlay.Dispose();
        }

        public void Tick()
        {
            _blockedFlashService?.Tick();
            _buildGridOverlay.Tick();
            _radiusVisuals.Tick();
        }

        private void SubscribeSignals()
        {
            _signalBus.Subscribe<BuildingPreviewChangedSignal>(_previewSignals.Handle);
            _signalBus.Subscribe<BuildingSelectionChangedSignal>(HandleBuildGridStateChanged);
            _signalBus.Subscribe<BuildingPreviewMovedSignal>(_previewSignals.Handle);
            _signalBus.Subscribe<BuildingPreviewDragVisualSignal>(_previewSignals.Handle);
            _signalBus.Subscribe<BuildGridHoverChangedSignal>(_previewSignals.Handle);
            _signalBus.Subscribe<BuildingCancelledSignal>(_previewSignals.Handle);
            _signalBus.Subscribe<BuildingCancelledSignal>(HandleBuildGridStateChanged);
            _signalBus.Subscribe<OnObjectsMapChangedSignal>(HandleBuildGridStateChanged);
            _signalBus.Subscribe<GridTileChangedSignal>(HandleBuildGridStateChanged);
            _signalBus.Subscribe<FogStateChangedSignal>(HandleBuildGridStateChanged);
            _signalBus.Subscribe<SettlementResourceChangedSignal>(HandleBuildGridStateChanged);
            _signalBus.Subscribe<BuildingPlacedSignal>(_placedSignals.Handle);
            _signalBus.Subscribe<BuildingPlacedSignal>(HandleBuildGridStateChanged);
            _signalBus.Subscribe<BuildingDemolishedSignal>(_placedSignals.Handle);
            _signalBus.Subscribe<BuildingDemolishedSignal>(HandleBuildGridStateChanged);
            _signalBus.Subscribe<WorldInfoSelectionChangedSignal>(_placedSignals.Handle);
            _signalBus.Subscribe<GameModeChangedSignal>(_placedSignals.Handle);
            _signalBus.Subscribe<WorldGeneratedDataSignal>(HandleWorldGenerated);
        }

        private void UnsubscribeSignals()
        {
            _signalBus.TryUnsubscribe<BuildingPreviewChangedSignal>(_previewSignals.Handle);
            _signalBus.TryUnsubscribe<BuildingSelectionChangedSignal>(HandleBuildGridStateChanged);
            _signalBus.TryUnsubscribe<BuildingPreviewMovedSignal>(_previewSignals.Handle);
            _signalBus.TryUnsubscribe<BuildingPreviewDragVisualSignal>(_previewSignals.Handle);
            _signalBus.TryUnsubscribe<BuildGridHoverChangedSignal>(_previewSignals.Handle);
            _signalBus.TryUnsubscribe<BuildingCancelledSignal>(_previewSignals.Handle);
            _signalBus.TryUnsubscribe<BuildingCancelledSignal>(HandleBuildGridStateChanged);
            _signalBus.TryUnsubscribe<OnObjectsMapChangedSignal>(HandleBuildGridStateChanged);
            _signalBus.TryUnsubscribe<GridTileChangedSignal>(HandleBuildGridStateChanged);
            _signalBus.TryUnsubscribe<FogStateChangedSignal>(HandleBuildGridStateChanged);
            _signalBus.TryUnsubscribe<SettlementResourceChangedSignal>(HandleBuildGridStateChanged);
            _signalBus.TryUnsubscribe<BuildingPlacedSignal>(_placedSignals.Handle);
            _signalBus.TryUnsubscribe<BuildingPlacedSignal>(HandleBuildGridStateChanged);
            _signalBus.TryUnsubscribe<BuildingDemolishedSignal>(_placedSignals.Handle);
            _signalBus.TryUnsubscribe<BuildingDemolishedSignal>(HandleBuildGridStateChanged);
            _signalBus.TryUnsubscribe<WorldInfoSelectionChangedSignal>(_placedSignals.Handle);
            _signalBus.TryUnsubscribe<GameModeChangedSignal>(_placedSignals.Handle);
            _signalBus.TryUnsubscribe<WorldGeneratedDataSignal>(HandleWorldGenerated);
        }

        private void HandleWorldGenerated(WorldGeneratedDataSignal _)
        {
            _buildGridOverlay.ResetWorld();
        }

        private void HandleBuildGridStateChanged(BuildingCancelledSignal _)
        {
            _previewVisuals.ClearGridHover();
        }

        private void HandleBuildGridStateChanged(BuildingSelectionChangedSignal signal)
        {
            _selectedBuildingId = string.IsNullOrWhiteSpace(signal.BuildingId)
                ? null
                : signal.BuildingId.Trim();
            _previewVisuals.ClearGridHover();
            _buildGridOverlay.SetSelectedBuilding(signal.BuildingId, signal.IsDemolishMode);
        }

        private void HandleBuildGridStateChanged(OnObjectsMapChangedSignal signal)
        {
            _buildGridOverlay.MarkDirty(signal.Position, ResolveGridInvalidationRadius(signal.OccupantId));
        }

        private void HandleBuildGridStateChanged(BuildingPlacedSignal signal)
        {
            int radius = ResolveGridInvalidationRadius(signal.BuildingId);
            _buildGridOverlay.MarkDirty(signal.Position, radius);
            if (signal.HasRelocationSource && signal.RelocationSourcePosition != signal.Position)
                _buildGridOverlay.MarkDirty(signal.RelocationSourcePosition, radius);
        }

        private void HandleBuildGridStateChanged(BuildingDemolishedSignal signal)
        {
            _buildGridOverlay.MarkDirty(
                signal.Position,
                ResolveGridInvalidationRadius(signal.BuildingId));
        }

        private void HandleBuildGridStateChanged(GridTileChangedSignal signal)
        {
            _buildGridOverlay.MarkDirty(signal.Position, _gridInvalidationRadius);
        }

        private void HandleBuildGridStateChanged(FogStateChangedSignal _)
        {
            _buildGridOverlay.MarkDirty();
        }

        private void HandleBuildGridStateChanged(SettlementResourceChangedSignal _)
        {
            _buildGridOverlay.MarkDirty();
        }

        private int ResolveGridInvalidationRadius(string changedBuildingId)
        {
            BuildingDefinition definition = string.IsNullOrWhiteSpace(changedBuildingId)
                ? null
                : _buildingRegistry?.GetById(changedBuildingId);
            return definition != null
                   && BuildingPlacementEvaluator.IsInfluenceCenter(definition)
                   && SelectedBuildingDependsOnInfluence()
                ? _gridInvalidationRadius
                : _localGridInvalidationRadius;
        }

        private bool SelectedBuildingDependsOnInfluence()
        {
            BuildingDefinition selected = string.IsNullOrWhiteSpace(_selectedBuildingId)
                ? null
                : _buildingRegistry?.GetById(_selectedBuildingId);
            if (selected == null)
                return false;

            if (selected.UseCustomTownHallRules)
                return selected.RequireTownHallInRange || selected.BlockIfTownHallAlreadyInRange;

            // Legacy definitions implicitly require a center for ordinary buildings
            // and prevent influence-center overlap for centers.
            return true;
        }

        private void ResolveGridInvalidationRadii(out int localRadius, out int influenceRadius)
        {
            int maxRadius = Mathf.Max(
                _placementRules?.MinSpacing ?? 0,
                _placementRules?.TownHallBuildRadius ?? 0);
            int maxFootprintExtent = 0;
            BuildingDefinition[] definitions = _buildingRegistry?.GetAll() ?? Array.Empty<BuildingDefinition>();
            for (int i = 0; i < definitions.Length; i++)
            {
                BuildingDefinition definition = definitions[i];
                int footprintCellCount = BuildingFootprintUtility.GetOccupiedCellCount(definition);
                for (int cellIndex = 0; cellIndex < footprintCellCount; cellIndex++)
                {
                    Vector2Int offset = BuildingFootprintUtility.GetOccupiedCellOffset(definition, cellIndex);
                    maxFootprintExtent = Mathf.Max(
                        maxFootprintExtent,
                        Mathf.Max(Mathf.Abs(offset.x), Mathf.Abs(offset.y)));
                }

                if (!BuildingPlacementEvaluator.IsInfluenceCenter(definition))
                    continue;

                maxRadius = Mathf.Max(
                    maxRadius,
                    BuildingPlacementEvaluator.ResolveInfluenceRadius(
                        definition,
                        _placementRules?.TownHallBuildRadius ?? 0));
            }

            localRadius = Mathf.Max(
                0,
                (_placementRules?.MinSpacing ?? 0) + maxFootprintExtent * 2);

            // Two influence radii cover both required-area and center-overlap rules.
            influenceRadius = Mathf.Max(
                maxRadius * 2,
                localRadius);
        }
    }
}
