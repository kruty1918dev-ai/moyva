using UnityEngine;
using Zenject;
using Kruty1918.Moyva.Signals.DomainEvents;

namespace Kruty1918.Moyva.Signals
{
    public class SignalBusInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Zenject.SignalBusInstaller.Install(Container);

            DeclareLegacySignals();
            DeclareDomainEventSignals();
            BindDomainEventBridge();
        }

        private void DeclareLegacySignals()
        {
            // Input and world interaction
            Container.DeclareSignal<TileClickedSignal>();
            Container.DeclareSignal<UnitCreatedSignal>();
            Container.DeclareSignal<UnitMovedSignal>();
            Container.DeclareSignal<UnitDestroyedSignal>();
            Container.DeclareSignal<InterruptMovementSignal>();
            Container.DeclareSignal<OnMapObjectSpawnedSignal>();
            Container.DeclareSignal<OnObjectsMapChangedSignal>().OptionalSubscriber();
            Container.DeclareSignal<WorldBuiltSignal>();
            Container.DeclareSignal<WorldGeneratedDataSignal>().OptionalSubscriber();
            Container.DeclareSignal<WorldSpawnPositionsSignal>().OptionalSubscriber();

            // GameMode (UI/request + legacy changed signal)
            Container.DeclareSignal<GameModeChangedSignal>();
            Container.DeclareSignal<GameModeChangeRequestedSignal>();

            // Construction (legacy + UI preview/handles)
            Container.DeclareSignal<BuildingPlacedSignal>();
            Container.DeclareSignal<BuildingCancelledSignal>();
            Container.DeclareSignal<BuildingPreviewChangedSignal>();
            Container.DeclareSignal<BuildingDemolishedSignal>().OptionalSubscriber();
            Container.DeclareSignal<ShowWallHandlesSignal>();
            Container.DeclareSignal<PlaceBuildingConfirmRequestSignal>().OptionalSubscriber();
            Container.DeclareSignal<MoveUnitRequestSignal>().OptionalSubscriber();

            // Building Info Panel
            Container.DeclareSignal<WorldInfoPanelRequestedSignal>().OptionalSubscriber();
            Container.DeclareSignal<WorldInfoPanelClosedSignal>().OptionalSubscriber();
            Container.DeclareSignal<BuildingInfoPanelRequestedSignal>().OptionalSubscriber();
            Container.DeclareSignal<BuildingInfoPanelClosedSignal>().OptionalSubscriber();
            Container.DeclareSignal<UnitInfoPanelRequestedSignal>().OptionalSubscriber();
            Container.DeclareSignal<MapObjectInfoPanelRequestedSignal>().OptionalSubscriber();
            Container.DeclareSignal<WorldInfoSelectionChangedSignal>().OptionalSubscriber();

            // FogOfWar
            Container.DeclareSignal<FogStateChangedSignal>();

            // Economy
            Container.DeclareSignal<EconomyTickCompletedSignal>().OptionalSubscriber();
            Container.DeclareSignal<SettlementCreatedSignal>().OptionalSubscriber();
            Container.DeclareSignal<SettlementDeactivatedSignal>().OptionalSubscriber();
            Container.DeclareSignal<SettlementResourceChangedSignal>().OptionalSubscriber();
            Container.DeclareSignal<ResourceDeficitSignal>().OptionalSubscriber();
            Container.DeclareSignal<GrantStarterPackResourcesSignal>().OptionalSubscriber();

            // SaveSystem
            Container.DeclareSignal<SaveRequestedSignal>();
            Container.DeclareSignal<LoadRequestedSignal>();
            Container.DeclareSignal<SaveCompletedSignal>().OptionalSubscriber();

            // GameState
            Container.DeclareSignal<GameStartedSignal>().OptionalSubscriber();
            Container.DeclareSignal<GameEndedSignal>().OptionalSubscriber();
            Container.DeclareSignal<GamePausedSignal>().OptionalSubscriber();

            // Faction
            Container.DeclareSignal<FactionEliminatedSignal>().OptionalSubscriber();

            // WorldCreation
            Container.DeclareSignal<WorldCreationConfirmedSignal>().OptionalSubscriber();
            Container.DeclareSignal<WorldCreationCancelledSignal>().OptionalSubscriber();
        }

        private void DeclareDomainEventSignals()
        {
            // Domain events layer (gameplay state transitions)
            Container.DeclareSignal<UnitCreatedDomainEvent>().OptionalSubscriber();
            Container.DeclareSignal<UnitMovedDomainEvent>().OptionalSubscriber();
            Container.DeclareSignal<UnitDestroyedDomainEvent>().OptionalSubscriber();
            Container.DeclareSignal<WorldBuiltDomainEvent>().OptionalSubscriber();
            Container.DeclareSignal<GameModeChangedDomainEvent>().OptionalSubscriber();
            Container.DeclareSignal<BuildingPlacedDomainEvent>().OptionalSubscriber();
            Container.DeclareSignal<BuildingDemolishedDomainEvent>().OptionalSubscriber();
            Container.DeclareSignal<EconomyTickCompletedDomainEvent>().OptionalSubscriber();
            Container.DeclareSignal<SettlementCreatedDomainEvent>().OptionalSubscriber();
            Container.DeclareSignal<SettlementDeactivatedDomainEvent>().OptionalSubscriber();
            Container.DeclareSignal<SettlementResourceChangedDomainEvent>().OptionalSubscriber();
            Container.DeclareSignal<ResourceDeficitDomainEvent>().OptionalSubscriber();
            Container.DeclareSignal<GameStartedDomainEvent>().OptionalSubscriber();
            Container.DeclareSignal<GameEndedDomainEvent>().OptionalSubscriber();
            Container.DeclareSignal<GamePausedDomainEvent>().OptionalSubscriber();
        }

        private void BindDomainEventBridge()
        {
            Container.BindInterfacesAndSelfTo<SignalDomainEventBridge>().AsSingle().NonLazy();
            Container.Bind<IWorldGenerationSignalState>().To<WorldGenerationSignalState>().AsSingle();
        }
    }
}
