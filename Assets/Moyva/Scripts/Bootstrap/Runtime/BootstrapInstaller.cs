using Zenject;
using Kruty1918.Moyva.Bootstrap.Runtime;

namespace Kruty1918.Moyva.Bootstrap
{
    public class BootstrapInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // BindInterfacesTo прив'яже TestUnitSpawner до інтерфейсу IInitializable 
            Container.BindInterfacesTo<TestUnitSpawner>().AsSingle().NonLazy();
        }
    }
}
