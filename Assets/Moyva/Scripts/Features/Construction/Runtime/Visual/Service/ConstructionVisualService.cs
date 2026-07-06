using System;
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
            IConstructionPlacedVisualService placedVisuals)
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
        }

        public void Initialize()
        {
            Debug.Log("[ConstructionVisual] Initializing visual stack...");
            _rootService.EnsureRoots();
            _radiusVisuals.Initialize();
            _buildGridOverlay.Initialize();
            SubscribeSignals();
            Debug.Log("[ConstructionVisual] Initialized. Roots, overlays and signal subscriptions are ready.");
        }

        public void Dispose()
        {
            UnsubscribeSignals();
            _blockedFlashService?.Clear();
            _previewVisuals.Clear();
            _placedVisuals.ClearDemolitionPreviewStyles();
            _placedVisuals.Clear();
            _radiusVisuals.Dispose();
            _buildGridOverlay.Hide();
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
            _signalBus.Subscribe<BuildingCancelledSignal>(_previewSignals.Handle);
            _signalBus.Subscribe<BuildingPlacedSignal>(_placedSignals.Handle);
            _signalBus.Subscribe<BuildingDemolishedSignal>(_placedSignals.Handle);
            _signalBus.Subscribe<WorldInfoSelectionChangedSignal>(_placedSignals.Handle);
            _signalBus.Subscribe<GameModeChangedSignal>(_placedSignals.Handle);
            _signalBus.Subscribe<WorldGeneratedDataSignal>(HandleWorldGenerated);
        }

        private void UnsubscribeSignals()
        {
            _signalBus.TryUnsubscribe<BuildingPreviewChangedSignal>(_previewSignals.Handle);
            _signalBus.TryUnsubscribe<BuildingCancelledSignal>(_previewSignals.Handle);
            _signalBus.TryUnsubscribe<BuildingPlacedSignal>(_placedSignals.Handle);
            _signalBus.TryUnsubscribe<BuildingDemolishedSignal>(_placedSignals.Handle);
            _signalBus.TryUnsubscribe<WorldInfoSelectionChangedSignal>(_placedSignals.Handle);
            _signalBus.TryUnsubscribe<GameModeChangedSignal>(_placedSignals.Handle);
            _signalBus.TryUnsubscribe<WorldGeneratedDataSignal>(HandleWorldGenerated);
        }

        private void HandleWorldGenerated(WorldGeneratedDataSignal _)
        {
            _buildGridOverlay.MarkDirty();
        }
    }
}
