using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Signals
{
    public class SignalBusInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Zenject.SignalBusInstaller.Install(Container);

            DeclareLegacySignals();
            BindWorldGenerationSignalState();
        }

        private void DeclareLegacySignals()
        {
            // Input and world interaction
            Container.DeclareSignal<TileClickedSignal>();
            Container.DeclareSignal<UnitCreatedSignal>();
            Container.DeclareSignal<UnitMovedSignal>();
            Container.DeclareSignal<UnitDestroyedSignal>();
            Container.DeclareSignal<UnitGarrisonStateChangedSignal>()
                .OptionalSubscriber();
            Container.DeclareSignal<InterruptMovementSignal>();
            Container.DeclareSignal<LocalUnitSelectionChangedSignal>().OptionalSubscriber();
            Container.DeclareSignal<OnMapObjectSpawnedSignal>();
            Container.DeclareSignal<OnObjectsMapChangedSignal>().OptionalSubscriber();
            Container.DeclareSignal<GridTileChangedSignal>().OptionalSubscriber();
            Container.DeclareSignal<WorldBuiltSignal>().OptionalSubscriber();
            Container.DeclareSignal<WorldGeneratedDataSignal>().OptionalSubscriber();
            Container.DeclareSignal<WorldSpawnPositionsSignal>().OptionalSubscriber();

            // GameMode (UI/request + legacy changed signal)
            Container.DeclareSignal<GameModeChangedSignal>();
            Container.DeclareSignal<GameModeChangeRequestedSignal>();

            // Construction (legacy + UI preview/handles)
            Container.DeclareSignal<BuildingPlacedSignal>();
            Container.DeclareSignal<BuildingCancelledSignal>();
            Container.DeclareSignal<BuildingPreviewChangedSignal>();
            Container.DeclareSignal<BuildingSelectionChangedSignal>().OptionalSubscriber();
            Container.DeclareSignal<BuildingPreviewMovedSignal>().OptionalSubscriber();
            Container.DeclareSignal<BuildingPreviewDragVisualSignal>().OptionalSubscriber();
            Container.DeclareSignal<BuildGridHoverChangedSignal>().OptionalSubscriber();
            Container.DeclareSignal<BuildingDemolishedSignal>().OptionalSubscriber();
            Container.DeclareSignal<BuildingOwnershipTransferredSignal>().OptionalSubscriber();
            Container.DeclareSignal<BuildingOperationalSignal>().OptionalSubscriber();
            Container.DeclareSignal<ShowWallHandlesSignal>();
            Container.DeclareSignal<PlaceBuildingConfirmRequestSignal>().OptionalSubscriber();
            Container.DeclareSignal<MoveUnitRequestSignal>().OptionalSubscriber();
            Container.DeclareSignal<MoveGroupRequestSignal>().OptionalSubscriber();
            Container.DeclareSignal<UnitMoveRejectedSignal>().OptionalSubscriber();
            Container.DeclareSignal<UnitGroupChangedSignal>().OptionalSubscriber();
            Container.DeclareSignal<UnitGroupCommandRejectedSignal>().OptionalSubscriber();
            Container.DeclareSignal<UnitRecruitmentCommandRejectedSignal>().OptionalSubscriber();

            // Unit recruitment
            Container.DeclareSignal<UnitRecruitmentQueueChangedSignal>().OptionalSubscriber();
            Container.DeclareSignal<UnitRecruitmentReadySignal>().OptionalSubscriber();
            Container.DeclareSignal<UnitRecruitmentDeployedSignal>().OptionalSubscriber();
            Container.DeclareSignal<UnitRecruitmentReadyIndicatorClickedSignal>().OptionalSubscriber();

            // Building Info Panel
            Container.DeclareSignal<WorldInfoPanelRequestedSignal>().OptionalSubscriber();
            Container.DeclareSignal<WorldInfoPanelClosedSignal>().OptionalSubscriber();
            Container.DeclareSignal<BuildingInfoPanelRequestedSignal>().OptionalSubscriber();
            Container.DeclareSignal<BuildingInfoPanelClosedSignal>().OptionalSubscriber();
            Container.DeclareSignal<UnitInfoPanelRequestedSignal>().OptionalSubscriber();
            Container.DeclareSignal<MapObjectInfoPanelRequestedSignal>().OptionalSubscriber();
            Container.DeclareSignal<WorldInfoSelectionChangedSignal>().OptionalSubscriber();
            Container.DeclareSignal<WorldFocusPingRequestedSignal>().OptionalSubscriber();

            // FogOfWar
            Container.DeclareSignal<FogStateChangedSignal>();

            // Economy
            Container.DeclareSignal<CaravanDeliveryCompletedSignal>().OptionalSubscriber();
            Container.DeclareSignal<ConstructionSupplyReadySignal>().OptionalSubscriber();
            Container.DeclareSignal<ConstructionSupplyOrderClosedSignal>().OptionalSubscriber();
            Container.DeclareSignal<SettlementPopulationChangedSignal>().OptionalSubscriber();
            Container.DeclareSignal<EconomyTickCompletedSignal>().OptionalSubscriber();
            Container.DeclareSignal<SettlementCreatedSignal>().OptionalSubscriber();
            Container.DeclareSignal<SettlementDeactivatedSignal>().OptionalSubscriber();
            Container.DeclareSignal<SettlementCapturedSignal>().OptionalSubscriber();
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

        private void BindWorldGenerationSignalState()
        {
            Container.Bind<IWorldGenerationSignalState>().To<WorldGenerationSignalState>().AsSingle();
        }
    }
}
