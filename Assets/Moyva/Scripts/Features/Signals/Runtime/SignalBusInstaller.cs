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
            Container.DeclareSignal<OnObjectsMapChangedSignal>();

            // Construction signals
            Container.DeclareSignal<BuildingModeStartedSignal>();
            Container.DeclareSignal<BuildingPreviewMovedSignal>();
            Container.DeclareSignal<BuildingPlacedSignal>();
            Container.DeclareSignal<BuildingUndoneSignal>();
            Container.DeclareSignal<BuildingRedoneSignal>();
            Container.DeclareSignal<BuildingCancelledSignal>();
            Container.DeclareSignal<BuildingConfirmedSignal>();
            Container.DeclareSignal<WallConnectionPointsShownSignal>();
            Container.DeclareSignal<WallConnectionPointsHiddenSignal>();
        }
    }
}