using Kruty1918.Moyva.Calendar.Runtime;
using Zenject;

namespace Kruty1918.Moyva.Visuals
{
    public class VisualInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            CalendarInstaller.InstallDefaultIfMissing(Container);

            Container.BindInterfacesAndSelfTo<DayNightShaderController>()
                .AsSingle()
                .NonLazy();
        }
    }
}
