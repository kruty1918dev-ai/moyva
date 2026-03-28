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
            Container.DeclareSignal<OnTileChanged>();
            Container.DeclareSignal<UnitCreatedSignal>();
            Container.DeclareSignal<UnitMovedSignal>();
            Container.DeclareSignal<UnitDestroyedSignal>();
        }
    }
}