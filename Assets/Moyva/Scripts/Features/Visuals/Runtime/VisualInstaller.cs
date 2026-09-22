using Kruty1918.Calendar.Runtime;
using Kruty1918.JsonConfig;
using Zenject;
using Kruty1918.Moyva.Calendar.Runtime;

namespace Kruty1918.Moyva.Visuals
{
    /// <summary>Zenject-інсталер візуального модуля.</summary>
    public class VisualInstaller : MonoInstaller
    {
        /// <summary>Реєструє біндінги візуального модуля.</summary>
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
                        JsonConfigRuntime.GetLegacyResource<FarViewAtmosphereConfig>(
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
