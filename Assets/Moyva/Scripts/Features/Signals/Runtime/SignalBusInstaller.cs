using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Signals
{
    public class SignalBusInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Zenject.SignalBusInstaller.Install(Container);
            Container.DeclareSignal<TileClickedSignal>();
            Container.DeclareSignal<UnitCreatedSignal>();
            Container.DeclareSignal<UnitMovedSignal>();
            Container.DeclareSignal<UnitDestroyedSignal>();
            Container.DeclareSignal<InterruptMovementSignal>();
            Container.DeclareSignal<OnMapObjectSpawnedSignal>();
            Container.DeclareSignal<OnObjectsMapChangedSignal>().OptionalSubscriber();
            Container.DeclareSignal<WorldBuiltSignal>();
            Container.DeclareSignal<WorldGeneratedDataSignal>().OptionalSubscriber();

            // GameMode
            Container.DeclareSignal<GameModeChangedSignal>();
            Container.DeclareSignal<GameModeChangeRequestedSignal>();

            // Construction
            Container.DeclareSignal<BuildingPlacedSignal>();
            Container.DeclareSignal<BuildingCancelledSignal>();
            Container.DeclareSignal<BuildingPreviewChangedSignal>();
            Container.DeclareSignal<BuildingDemolishedSignal>().OptionalSubscriber();
            Container.DeclareSignal<ShowWallHandlesSignal>();

            // Building Info Panel
            Container.DeclareSignal<WorldInfoPanelRequestedSignal>().OptionalSubscriber();
            Container.DeclareSignal<WorldInfoPanelClosedSignal>().OptionalSubscriber();
            Container.DeclareSignal<BuildingInfoPanelRequestedSignal>().OptionalSubscriber();
            Container.DeclareSignal<BuildingInfoPanelClosedSignal>().OptionalSubscriber();
            Container.DeclareSignal<UnitInfoPanelRequestedSignal>().OptionalSubscriber();
            Container.DeclareSignal<WorldInfoSelectionChangedSignal>().OptionalSubscriber();

            // FogOfWar
            Container.DeclareSignal<FogStateChangedSignal>();

            // Economy
            Container.DeclareSignal<EconomyTickCompletedSignal>().OptionalSubscriber();
            Container.DeclareSignal<SettlementCreatedSignal>().OptionalSubscriber();
            Container.DeclareSignal<SettlementDeactivatedSignal>().OptionalSubscriber();
            Container.DeclareSignal<SettlementResourceChangedSignal>().OptionalSubscriber();
            Container.DeclareSignal<ResourceDeficitSignal>().OptionalSubscriber();

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
    }
}