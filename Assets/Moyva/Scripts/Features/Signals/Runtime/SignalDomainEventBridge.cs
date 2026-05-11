using System;
using Kruty1918.Moyva.Signals.DomainEvents;
using Zenject;

namespace Kruty1918.Moyva.Signals
{
    /// <summary>
    /// Transitional bridge that mirrors legacy gameplay signals into the domain events layer.
    /// Keeps backward compatibility while modules migrate to DomainEvents namespace.
    /// </summary>
    public sealed class SignalDomainEventBridge : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;

        public SignalDomainEventBridge(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.Subscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.Subscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus.Subscribe<WorldBuiltSignal>(OnWorldBuilt);
            _signalBus.Subscribe<GameModeChangedSignal>(OnGameModeChanged);
            _signalBus.Subscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.Subscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
            _signalBus.Subscribe<EconomyTickCompletedSignal>(OnEconomyTickCompleted);
            _signalBus.Subscribe<SettlementCreatedSignal>(OnSettlementCreated);
            _signalBus.Subscribe<SettlementDeactivatedSignal>(OnSettlementDeactivated);
            _signalBus.Subscribe<SettlementResourceChangedSignal>(OnSettlementResourceChanged);
            _signalBus.Subscribe<ResourceDeficitSignal>(OnResourceDeficit);
            _signalBus.Subscribe<GameStartedSignal>(OnGameStarted);
            _signalBus.Subscribe<GameEndedSignal>(OnGameEnded);
            _signalBus.Subscribe<GamePausedSignal>(OnGamePaused);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<UnitCreatedSignal>(OnUnitCreated);
            _signalBus.TryUnsubscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.TryUnsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus.TryUnsubscribe<WorldBuiltSignal>(OnWorldBuilt);
            _signalBus.TryUnsubscribe<GameModeChangedSignal>(OnGameModeChanged);
            _signalBus.TryUnsubscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.TryUnsubscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
            _signalBus.TryUnsubscribe<EconomyTickCompletedSignal>(OnEconomyTickCompleted);
            _signalBus.TryUnsubscribe<SettlementCreatedSignal>(OnSettlementCreated);
            _signalBus.TryUnsubscribe<SettlementDeactivatedSignal>(OnSettlementDeactivated);
            _signalBus.TryUnsubscribe<SettlementResourceChangedSignal>(OnSettlementResourceChanged);
            _signalBus.TryUnsubscribe<ResourceDeficitSignal>(OnResourceDeficit);
            _signalBus.TryUnsubscribe<GameStartedSignal>(OnGameStarted);
            _signalBus.TryUnsubscribe<GameEndedSignal>(OnGameEnded);
            _signalBus.TryUnsubscribe<GamePausedSignal>(OnGamePaused);
        }

        private void OnUnitCreated(UnitCreatedSignal signal)
        {
            _signalBus.Fire(new UnitCreatedDomainEvent
            {
                UnitId = signal.UnitId,
                UnitTypeId = signal.UnitTypeId,
                Position = signal.Position,
                VisionRange = signal.VisionRange,
                UnitObject = signal.UnitObject,
                OwnerId = signal.OwnerId
            });
        }

        private void OnUnitMoved(UnitMovedSignal signal)
        {
            _signalBus.Fire(new UnitMovedDomainEvent
            {
                UnitId = signal.UnitId,
                NewPosition = signal.NewPosition,
                Cost = signal.Cost,
                SourceFactionId = signal.SourceFactionId
            });
        }

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
        {
            _signalBus.Fire(new UnitDestroyedDomainEvent { UnitId = signal.UnitId });
        }

        private void OnWorldBuilt()
        {
            _signalBus.Fire(new WorldBuiltDomainEvent());
        }

        private void OnGameModeChanged(GameModeChangedSignal signal)
        {
            _signalBus.Fire(new GameModeChangedDomainEvent { NewMode = signal.NewMode });
        }

        private void OnBuildingPlaced(BuildingPlacedSignal signal)
        {
            _signalBus.Fire(new BuildingPlacedDomainEvent
            {
                BuildingId = signal.BuildingId,
                Position = signal.Position,
                OwnerId = signal.OwnerId,
                SourceFactionId = signal.SourceFactionId
            });
        }

        private void OnBuildingDemolished(BuildingDemolishedSignal signal)
        {
            _signalBus.Fire(new BuildingDemolishedDomainEvent
            {
                BuildingId = signal.BuildingId,
                Position = signal.Position,
                OwnerId = signal.OwnerId,
                SourceFactionId = signal.SourceFactionId
            });
        }

        private void OnEconomyTickCompleted(EconomyTickCompletedSignal signal)
        {
            _signalBus.Fire(new EconomyTickCompletedDomainEvent
            {
                SettlementId = signal.SettlementId,
                OwnerId = signal.OwnerId,
                Turn = signal.Turn,
                TotalPopulation = signal.TotalPopulation,
                Arrivals = signal.Arrivals,
                Deaths = signal.Deaths,
                ProductionCyclesCompleted = signal.ProductionCyclesCompleted
            });
        }

        private void OnSettlementCreated(SettlementCreatedSignal signal)
        {
            _signalBus.Fire(new SettlementCreatedDomainEvent
            {
                SettlementId = signal.SettlementId,
                OwnerId = signal.OwnerId,
                TownHallPosition = signal.TownHallPosition
            });
        }

        private void OnSettlementDeactivated(SettlementDeactivatedSignal signal)
        {
            _signalBus.Fire(new SettlementDeactivatedDomainEvent
            {
                SettlementId = signal.SettlementId,
                OwnerId = signal.OwnerId,
                Reason = signal.Reason
            });
        }

        private void OnSettlementResourceChanged(SettlementResourceChangedSignal signal)
        {
            _signalBus.Fire(new SettlementResourceChangedDomainEvent
            {
                SettlementId = signal.SettlementId,
                OwnerId = signal.OwnerId,
                ResourceId = signal.ResourceId,
                NewAmount = signal.NewAmount,
                Delta = signal.Delta
            });
        }

        private void OnResourceDeficit(ResourceDeficitSignal signal)
        {
            _signalBus.Fire(new ResourceDeficitDomainEvent
            {
                SettlementId = signal.SettlementId,
                OwnerId = signal.OwnerId,
                ResourceId = signal.ResourceId
            });
        }

        private void OnGameStarted()
        {
            _signalBus.Fire(new GameStartedDomainEvent());
        }

        private void OnGameEnded(GameEndedSignal signal)
        {
            _signalBus.Fire(new GameEndedDomainEvent { WinnerId = signal.WinnerId });
        }

        private void OnGamePaused(GamePausedSignal signal)
        {
            _signalBus.Fire(new GamePausedDomainEvent { IsPaused = signal.IsPaused });
        }
    }
}
