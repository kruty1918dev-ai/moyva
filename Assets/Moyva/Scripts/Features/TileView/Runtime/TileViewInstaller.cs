using Zenject;

namespace Kruty1918.Moyva.Tiles
{
    public class TileViewInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);
            Container.DeclareSignal<TileClickedSignal>();
        }
    }
}