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

            // GameMode
            Container.DeclareSignal<GameModeChangedSignal>();

            // Construction
            Container.DeclareSignal<BuildingPlacedSignal>();
            Container.DeclareSignal<BuildingCancelledSignal>();
            Container.DeclareSignal<BuildingPreviewChangedSignal>();
            Container.DeclareSignal<ShowWallHandlesSignal>();
        }
    }
}