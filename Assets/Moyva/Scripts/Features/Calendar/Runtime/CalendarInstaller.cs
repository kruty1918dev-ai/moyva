using Kruty1918.Moyva.Calendar.Config;
using Kruty1918.Moyva.Calendar.Core;
using Kruty1918.Moyva.Calendar.Multiplayer;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Calendar.Runtime
{
    /// <summary>
    /// Zenject MonoInstaller для модуля календаря.
    ///
    /// Реалізує принцип єдиного джерела правди (host-driven time):
    /// — хост: <see cref="GameCalendarService"/> + <see cref="CalendarSyncAdapter"/>;
    ///   після кожного ходу <see cref="ICalendarSyncAdapter.OnHostAdvanced"/> передає
    ///   <c>TotalHoursSinceEpoch</c> мережевому шару, який розсилає його клієнтам.
    /// — клієнт: <see cref="ClientCalendarProxy"/>; мережевий шар викликає
    ///   <see cref="ClientCalendarProxy.ApplySnapshot"/> з отриманим значенням.
    ///
    /// Усі ігрові компоненти залежать лише від <see cref="ICalendarService"/> і не знають,
    /// хто є хостом, а хто — клієнтом.
    ///
    /// Підключіть цей installer у сцені. Якщо <see cref="_sessionConfig"/> не призначено,
    /// буде використано <see cref="CalendarConfig.Default()"/> (рік = <see cref="CalendarConfig.PeakUkraineYear"/> — 1054 р.).
    /// </summary>
    public sealed class CalendarInstaller : MonoInstaller
    {
        [SerializeField]
        [Tooltip("true — цей гравець є хостом (авторитетна сторона); false — клієнт.")]
        private bool _isHost = true;

        [SerializeField]
        [Tooltip("Конфігурація початкової дати сесії. Якщо не задано — використовується Default (1054 р.).")]
        private CalendarSessionConfigSO _sessionConfig;

        public override void InstallBindings()
        {
            CalendarConfig config = _sessionConfig != null
                ? _sessionConfig.BuildConfig()
                : CalendarConfig.Default();

            if (_isHost)
                InstallHost(config);
            else
                InstallClient(config);
        }

        private void InstallHost(CalendarConfig config)
        {
            var service = new GameCalendarService(config);
            Container.BindInstance(service).AsSingle();
            Container.Bind<ICalendarService>().FromInstance(service).AsSingle();

            var adapter = new CalendarSyncAdapter(service);
            Container.BindInstance(adapter).AsSingle();
            Container.Bind<ICalendarSyncAdapter>().FromInstance(adapter).AsSingle();
        }

        private void InstallClient(CalendarConfig config)
        {
            var proxy = new ClientCalendarProxy(config);
            Container.BindInstance(proxy).AsSingle();
            Container.Bind<ICalendarService>().FromInstance(proxy).AsSingle();
        }
    }
}
