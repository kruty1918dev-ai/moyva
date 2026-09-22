using System;
using Kruty1918.Moyva.Signals;
using Kruty1918.Telemetry.Core;
using Zenject;

namespace Kruty1918.Moyva.Telemetry
{
    /// <summary>
    /// Boundary adapter: translates gameplay SignalBus traffic into generic
    /// telemetry events. Telemetry observes — it never mutates gameplay state.
    /// </summary>
    public sealed class MoyvaSignalTelemetryAdapter : IInitializable, IDisposable
    {
        private readonly SignalBus _bus;
        private readonly ITelemetrySink _sink;
        private long _matchStartUtcTicks;

        private Action<WorldInfoPanelRequestedSignal> _onWorldInfoOpen;
        private Action<WorldInfoPanelClosedSignal> _onWorldInfoClosed;
        private Action<BuildingInfoPanelRequestedSignal> _onBuildingInfoOpen;
        private Action<BuildingInfoPanelClosedSignal> _onBuildingInfoClosed;
        private Action<UnitInfoPanelRequestedSignal> _onUnitInfoOpen;
        private Action<MapObjectInfoPanelRequestedSignal> _onMapObjectInfoOpen;
        private Action<WorldInfoSelectionChangedSignal> _onSelectionChanged;

        public MoyvaSignalTelemetryAdapter(SignalBus bus, ITelemetrySink sink)
        {
            _bus = bus;
            _sink = sink;
        }

        public void Initialize()
        {
            // Session lifecycle
            _bus.Subscribe<GameStartedSignal>(OnGameStarted);
            _bus.Subscribe<GameEndedSignal>(OnGameEnded);
            _bus.Subscribe<GamePausedSignal>(OnGamePaused);
            _bus.Subscribe<GameModeChangedSignal>(OnGameModeChanged);

            // World
            _bus.Subscribe<WorldGeneratedDataSignal>(OnWorldGenerated);

            // Construction
            _bus.Subscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _bus.Subscribe<BuildingCancelledSignal>(OnBuildingCancelled);
            _bus.Subscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
            _bus.Subscribe<ConstructionPlacementRejectedSignal>(OnPlacementRejected);
            _bus.Subscribe<BuildingOwnershipTransferredSignal>(OnBuildingOwnershipTransferred);

            // Units
            _bus.Subscribe<UnitCreatedSignal>(OnUnitCreated);
            _bus.Subscribe<UnitMovedSignal>(OnUnitMoved);
            _bus.Subscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _bus.Subscribe<UnitMoveRejectedSignal>(OnUnitMoveRejected);

            // Recruitment
            _bus.Subscribe<UnitRecruitmentQueueChangedSignal>(OnRecruitmentQueueChanged);
            _bus.Subscribe<UnitRecruitmentReadySignal>(OnRecruitmentReady);
            _bus.Subscribe<UnitRecruitmentDeployedSignal>(OnRecruitmentDeployed);
            _bus.Subscribe<UnitRecruitmentCommandRejectedSignal>(OnRecruitmentRejected);

            // Economy
            _bus.Subscribe<EconomyTickCompletedSignal>(OnEconomyTick);
            _bus.Subscribe<SettlementCreatedSignal>(OnSettlementCreated);
            _bus.Subscribe<SettlementDeactivatedSignal>(OnSettlementDeactivated);
            _bus.Subscribe<SettlementCapturedSignal>(OnSettlementCaptured);
            _bus.Subscribe<ResourceDeficitSignal>(OnResourceDeficit);

            // Fog
            _bus.Subscribe<FogStateChangedSignal>(OnFogChanged);

            // Save
            _bus.Subscribe<SaveCompletedSignal>(OnSaveCompleted);
            _bus.Subscribe<LoadRequestedSignal>(OnLoadRequested);

            // UI panels / selection
            _onWorldInfoOpen = _ => TrackPanel("worldInfo", "opened", null, null);
            _onWorldInfoClosed = _ => TrackPanel("worldInfo", "closed", null, null);
            _onBuildingInfoOpen = s => TrackPanel("buildingInfo", "opened", s.BuildingId, null);
            _onBuildingInfoClosed = _ => TrackPanel("buildingInfo", "closed", null, null);
            _onUnitInfoOpen = s => TrackPanel("unitInfo", "opened", s.UnitId, null);
            _onMapObjectInfoOpen = s => TrackPanel("mapObjectInfo", "opened", s.MapObjectId, null);
            _onSelectionChanged = s => TrackPanel("selection", "changed", s.ObjectId, s.Kind.ToString());
            _bus.Subscribe(_onWorldInfoOpen);
            _bus.Subscribe(_onWorldInfoClosed);
            _bus.Subscribe(_onBuildingInfoOpen);
            _bus.Subscribe(_onBuildingInfoClosed);
            _bus.Subscribe(_onUnitInfoOpen);
            _bus.Subscribe(_onMapObjectInfoOpen);
            _bus.Subscribe(_onSelectionChanged);
        }

        public void Dispose()
        {
            _bus.TryUnsubscribe<GameStartedSignal>(OnGameStarted);
            _bus.TryUnsubscribe<GameEndedSignal>(OnGameEnded);
            _bus.TryUnsubscribe<GamePausedSignal>(OnGamePaused);
            _bus.TryUnsubscribe<GameModeChangedSignal>(OnGameModeChanged);
            _bus.TryUnsubscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
            _bus.TryUnsubscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _bus.TryUnsubscribe<BuildingCancelledSignal>(OnBuildingCancelled);
            _bus.TryUnsubscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
            _bus.TryUnsubscribe<ConstructionPlacementRejectedSignal>(OnPlacementRejected);
            _bus.TryUnsubscribe<BuildingOwnershipTransferredSignal>(OnBuildingOwnershipTransferred);
            _bus.TryUnsubscribe<UnitCreatedSignal>(OnUnitCreated);
            _bus.TryUnsubscribe<UnitMovedSignal>(OnUnitMoved);
            _bus.TryUnsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _bus.TryUnsubscribe<UnitMoveRejectedSignal>(OnUnitMoveRejected);
            _bus.TryUnsubscribe<UnitRecruitmentQueueChangedSignal>(OnRecruitmentQueueChanged);
            _bus.TryUnsubscribe<UnitRecruitmentReadySignal>(OnRecruitmentReady);
            _bus.TryUnsubscribe<UnitRecruitmentDeployedSignal>(OnRecruitmentDeployed);
            _bus.TryUnsubscribe<UnitRecruitmentCommandRejectedSignal>(OnRecruitmentRejected);
            _bus.TryUnsubscribe<EconomyTickCompletedSignal>(OnEconomyTick);
            _bus.TryUnsubscribe<SettlementCreatedSignal>(OnSettlementCreated);
            _bus.TryUnsubscribe<SettlementDeactivatedSignal>(OnSettlementDeactivated);
            _bus.TryUnsubscribe<SettlementCapturedSignal>(OnSettlementCaptured);
            _bus.TryUnsubscribe<ResourceDeficitSignal>(OnResourceDeficit);
            _bus.TryUnsubscribe<FogStateChangedSignal>(OnFogChanged);
            _bus.TryUnsubscribe<SaveCompletedSignal>(OnSaveCompleted);
            _bus.TryUnsubscribe<LoadRequestedSignal>(OnLoadRequested);
            if (_onWorldInfoOpen != null) _bus.TryUnsubscribe(_onWorldInfoOpen);
            if (_onWorldInfoClosed != null) _bus.TryUnsubscribe(_onWorldInfoClosed);
            if (_onBuildingInfoOpen != null) _bus.TryUnsubscribe(_onBuildingInfoOpen);
            if (_onBuildingInfoClosed != null) _bus.TryUnsubscribe(_onBuildingInfoClosed);
            if (_onUnitInfoOpen != null) _bus.TryUnsubscribe(_onUnitInfoOpen);
            if (_onMapObjectInfoOpen != null) _bus.TryUnsubscribe(_onMapObjectInfoOpen);
            if (_onSelectionChanged != null) _bus.TryUnsubscribe(_onSelectionChanged);
        }

        private void TrackPanel(string kind, string action, string objectId, string selectionKind)
            => _sink.Track(new MoyvaUiPanelEvent
            { Kind = kind, Action = action, ObjectId = objectId, SelectionKind = selectionKind });

        private void OnGameStarted(GameStartedSignal _)
        {
            _matchStartUtcTicks = DateTime.UtcNow.Ticks;
            _sink.Track(new MoyvaMatchLifecycleEvent { EventType = "moyva.match.started" });
        }

        private void OnGameEnded(GameEndedSignal s)
            => _sink.Track(new MoyvaMatchLifecycleEvent
            {
                EventType = "moyva.match.ended",
                WinnerId = s.WinnerId,
                DurationMs = _matchStartUtcTicks > 0
                    ? (DateTime.UtcNow.Ticks - _matchStartUtcTicks) / TimeSpan.TicksPerMillisecond
                    : 0,
            });

        private void OnGamePaused(GamePausedSignal s)
            => _sink.Track(new MoyvaMatchLifecycleEvent
            { EventType = "moyva.match.paused", IsPaused = s.IsPaused });

        private void OnGameModeChanged(GameModeChangedSignal s)
            => _sink.Track(new MoyvaMatchLifecycleEvent
            { EventType = "moyva.match.modeChanged", NewMode = s.NewMode.ToString() });

        private void OnWorldGenerated(WorldGeneratedDataSignal s)
            => _sink.Track(new MoyvaWorldGeneratedEvent
            {
                Source = s.Source.ToString(),
                Width = s.Width,
                Height = s.Height,
                CellSize = s.CellSize,
                StartupSessionId = s.StartupSessionId,
                SnapshotRevision = s.SnapshotRevision,
            });

        private void OnBuildingPlaced(BuildingPlacedSignal s)
            => _sink.Track(new MoyvaConstructionEvent
            {
                EventType = "moyva.construction.placed",
                BuildingId = s.BuildingId,
                X = s.Position.x, Y = s.Position.y,
                OwnerId = s.OwnerId,
                Rotation = s.RotationQuarterTurns,
                Relocated = s.HasRelocationSource,
            });

        private void OnBuildingCancelled(BuildingCancelledSignal _)
            => _sink.Track(new MoyvaConstructionEvent
            { EventType = "moyva.construction.cancelled" });

        private void OnBuildingDemolished(BuildingDemolishedSignal s)
            => _sink.Track(new MoyvaConstructionEvent
            {
                EventType = "moyva.construction.demolished",
                BuildingId = s.BuildingId,
                X = s.Position.x, Y = s.Position.y,
                OwnerId = s.OwnerId,
            });

        private void OnPlacementRejected(ConstructionPlacementRejectedSignal s)
            => _sink.Track(new MoyvaConstructionEvent
            {
                EventType = "moyva.construction.rejected",
                BuildingId = s.BuildingId,
                X = s.Position.x, Y = s.Position.y,
                Reason = s.Reason,
            });

        private void OnBuildingOwnershipTransferred(BuildingOwnershipTransferredSignal s)
            => _sink.Track(new MoyvaConstructionEvent
            {
                EventType = "moyva.construction.transferred",
                BuildingId = s.BuildingId,
                X = s.Position.x, Y = s.Position.y,
                PreviousOwnerId = s.PreviousOwnerId,
                NewOwnerId = s.NewOwnerId,
            });

        private void OnUnitCreated(UnitCreatedSignal s)
            => _sink.Track(new MoyvaUnitEvent
            {
                EventType = "moyva.units.created",
                UnitId = s.UnitId,
                UnitTypeId = s.UnitTypeId,
                X = s.Position.x, Y = s.Position.y,
                OwnerId = s.OwnerId,
            });

        private void OnUnitMoved(UnitMovedSignal s)
            => _sink.Track(new MoyvaUnitEvent
            {
                EventType = "moyva.units.moved",
                UnitId = s.UnitId,
                X = s.NewPosition.x, Y = s.NewPosition.y,
                Cost = s.Cost,
                OwnerId = s.SourceFactionId,
            });

        private void OnUnitDestroyed(UnitDestroyedSignal s)
            => _sink.Track(new MoyvaUnitEvent
            { EventType = "moyva.units.destroyed", UnitId = s.UnitId });

        private void OnUnitMoveRejected(UnitMoveRejectedSignal s)
            => _sink.Track(new MoyvaUnitEvent
            {
                EventType = "moyva.units.moveRejected",
                UnitId = s.UnitId,
                X = s.TargetPosition.x, Y = s.TargetPosition.y,
                Reason = s.Reason,
            });

        private void OnRecruitmentQueueChanged(UnitRecruitmentQueueChangedSignal s)
            => _sink.Track(new MoyvaRecruitmentEvent
            {
                EventType = "moyva.recruitment.queueChanged",
                OwnerId = s.OwnerId,
                UnitTypeId = s.UnitTypeId,
                QueueId = s.QueueId,
                BuildingX = s.BuildingPosition.x, BuildingY = s.BuildingPosition.y,
            });

        private void OnRecruitmentReady(UnitRecruitmentReadySignal s)
            => _sink.Track(new MoyvaRecruitmentEvent
            {
                EventType = "moyva.recruitment.ready",
                OwnerId = s.OwnerId,
                UnitTypeId = s.UnitTypeId,
                QueueId = s.QueueId,
                BuildingX = s.BuildingPosition.x, BuildingY = s.BuildingPosition.y,
            });

        private void OnRecruitmentDeployed(UnitRecruitmentDeployedSignal s)
            => _sink.Track(new MoyvaRecruitmentEvent
            {
                EventType = "moyva.recruitment.deployed",
                OwnerId = s.OwnerId,
                UnitTypeId = s.UnitTypeId,
                QueueId = s.QueueId,
                BuildingX = s.BuildingPosition.x, BuildingY = s.BuildingPosition.y,
                UnitId = s.UnitId,
            });

        private void OnRecruitmentRejected(UnitRecruitmentCommandRejectedSignal s)
            => _sink.Track(new MoyvaRecruitmentEvent
            { EventType = "moyva.recruitment.rejected", Reason = s.Reason ?? "unknown" });

        private void OnEconomyTick(EconomyTickCompletedSignal s)
            => _sink.Track(new MoyvaEconomyTickEvent
            {
                SettlementId = s.SettlementId,
                OwnerId = s.OwnerId,
                Turn = s.Turn,
                Population = s.TotalPopulation,
                Arrivals = s.Arrivals,
                Deaths = s.Deaths,
                ProductionCycles = s.ProductionCyclesCompleted,
            });

        private void OnSettlementCreated(SettlementCreatedSignal s)
            => _sink.Track(new MoyvaSettlementEvent
            { Kind = "created", SettlementId = s.SettlementId, OwnerId = s.OwnerId });

        private void OnSettlementDeactivated(SettlementDeactivatedSignal s)
            => _sink.Track(new MoyvaSettlementEvent
            {
                Kind = "deactivated",
                SettlementId = s.SettlementId,
                OwnerId = s.OwnerId,
                Reason = s.Reason,
            });

        private void OnSettlementCaptured(SettlementCapturedSignal s)
            => _sink.Track(new MoyvaSettlementEvent
            {
                Kind = "captured",
                SettlementId = s.SettlementId,
                PreviousOwnerId = s.PreviousOwnerId,
                NewOwnerId = s.NewOwnerId,
                Reason = s.Reason,
            });

        private void OnResourceDeficit(ResourceDeficitSignal s)
            => _sink.Track(new MoyvaResourceEvent
            { Kind = "deficit", SettlementId = s.SettlementId, ResourceId = s.ResourceId });

        private void OnFogChanged(FogStateChangedSignal s)
            => _sink.Track(new MoyvaFogChangedEvent { ChangedTiles = s.ChangedTilesCount });

        private void OnSaveCompleted(SaveCompletedSignal s)
            => _sink.Track(new MoyvaSaveEvent
            { EventType = "moyva.save.saved", Slot = s.Slot, Success = s.Success });

        private void OnLoadRequested(LoadRequestedSignal s)
            => _sink.Track(new MoyvaSaveEvent
            { EventType = "moyva.save.loadRequested", Slot = s.Slot });
    }
}
