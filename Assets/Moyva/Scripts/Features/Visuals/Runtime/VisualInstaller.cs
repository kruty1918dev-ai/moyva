using Kruty1918.Moyva.Calendar.Runtime;
using Kruty1918.Moyva.Jsonization;
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

            if (!Container.HasBinding<FarViewAtmosphereConfig>())
            {
                Container.Bind<FarViewAtmosphereConfig>()
                    .FromMethod(_ =>
                        MoyvaJsonRuntime.GetLegacyResource<FarViewAtmosphereConfig>(
                            nameof(FarViewAtmosphereConfig)))
                    .AsSingle()
                    .IfNotBound();
            }

            Container.BindInterfacesAndSelfTo<FarViewAtmosphereDriver>()
                .AsSingle()
                .NonLazy();
        }
    }
}
