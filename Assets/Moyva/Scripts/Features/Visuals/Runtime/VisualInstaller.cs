using Kruty1918.Moyva.Calendar.Config;
using Kruty1918.Moyva.Calendar.Core;
using Kruty1918.Moyva.Calendar.Multiplayer;
using Kruty1918.Moyva.Calendar.Runtime;
using Zenject;

namespace Kruty1918.Moyva.Visuals
{
    public class VisualInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            var calendarService = new GameCalendarService(CalendarConfig.Default());
            Container.BindInstance(calendarService)
                .IfNotBound();
            Container.Bind<ICalendarService>()
                .FromInstance(calendarService)
                .AsSingle()
                .IfNotBound();

            var calendarSyncAdapter = new CalendarSyncAdapter(calendarService);
            Container.BindInstance(calendarSyncAdapter)
                .IfNotBound();
            Container.Bind<ICalendarSyncAdapter>()
                .FromInstance(calendarSyncAdapter)
                .AsSingle()
                .IfNotBound();

            Container.BindInterfacesAndSelfTo<DayNightShaderController>()
                .AsSingle()
                .NonLazy();
        }
    }
}