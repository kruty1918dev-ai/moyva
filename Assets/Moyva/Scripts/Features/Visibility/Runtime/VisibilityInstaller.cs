using Kruty1918.Moyva.Visibility.API;
using Zenject;

namespace Kruty1918.Moyva.Visibility.Runtime
{
    public class VisibilityInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<VisibilityService>()
                .AsSingle()
                .NonLazy();
        }
    }
}
