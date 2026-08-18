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

            CalendarConfig config = CalendarConfigLifecycle.ValidateAndFreeze(
                rawConfig,
                message => Debug.LogWarning($"[CalendarInstaller] {message}"));

            var service = new GameCalendarService(config);
            Container.BindInstance(service).AsSingle();
            Container.Bind<ICalendarService>().FromInstance(service).AsSingle();
            Container.Bind<ICalendarStateRestorer>().FromInstance(service).AsSingle();

            var adapter = new CalendarSyncAdapter(service);
            Container.BindInstance(adapter).AsSingle();
            Container.Bind<ICalendarSyncAdapter>().FromInstance(adapter).AsSingle();
        }
    }
}
