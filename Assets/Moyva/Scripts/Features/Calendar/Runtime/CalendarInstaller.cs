using Kruty1918.Moyva.Calendar.Config;
using Kruty1918.Moyva.Calendar.Core;
using Kruty1918.Moyva.Calendar.Multiplayer;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Calendar.Runtime
{
    public sealed class CalendarInstaller : MonoInstaller
    {
        [SerializeField]
        [Tooltip("Конфігурація початкової дати сесії. Якщо не задано — використовується Default (1054 р.).")]
        private CalendarSessionConfigSO _sessionConfig;

        public override void InstallBindings()
        {
            CalendarConfig rawConfig = _sessionConfig != null
                ? _sessionConfig.BuildConfig()
                : CalendarConfig.Default();

            CalendarConfig config = CalendarConfigLifecycle.ValidateAndFreeze(rawConfig);

            InstallIfMissing(Container, config);
        }

        public static void InstallDefaultIfMissing(DiContainer container)
        {
            InstallIfMissing(
                container,
                CalendarConfigLifecycle.ValidateAndFreeze(CalendarConfig.Default()));
        }

        private static void InstallIfMissing(
            DiContainer container,
            CalendarConfig config)
        {
            if (container == null
                || container.HasBinding<ICalendarService>())
            {
                return;
            }

            var service = new GameCalendarService(config);
            container.BindInstance(service).AsSingle();
            container.Bind<ICalendarService>()
                .FromInstance(service)
                .AsSingle();
            container.Bind<ICalendarStateRestorer>()
                .FromInstance(service)
                .AsSingle();

            var adapter = new CalendarSyncAdapter(service);
            container.BindInstance(adapter).AsSingle();
            container.Bind<ICalendarSyncAdapter>()
                .FromInstance(adapter)
                .AsSingle();
        }
    }
}
