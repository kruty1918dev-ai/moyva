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
    /// Installer завжди піднімає локальний авторитетний календар.
    /// Рішення про host/client не приймається тут і має походити з відповідальних
    /// session/network сервісів. Для соло-гри це означає, що календар завжди доступний.
    ///
    /// Підключіть цей installer у сцені. Якщо <see cref="_sessionConfig"/> не призначено,
    /// буде використано <see cref="CalendarConfig.Default()"/> (рік = <see cref="CalendarConfig.PeakUkraineYear"/> — 1054 р.).
    /// </summary>
    public sealed class CalendarInstaller : MonoInstaller
    {
        [SerializeField]
        [Tooltip("Конфігурація початкової дати сесії. Якщо не задано — використовується Default (1054 р.).")]
        private CalendarSessionConfigSO _sessionConfig;

        public override void InstallBindings()
        {
            CalendarConfig config = _sessionConfig != null
                ? _sessionConfig.BuildConfig()
                : CalendarConfig.Default();

            var service = new GameCalendarService(config);
            Container.BindInstance(service).AsSingle();
            Container.Bind<ICalendarService>().FromInstance(service).AsSingle();

            var adapter = new CalendarSyncAdapter(service);
            Container.BindInstance(adapter).AsSingle();
            Container.Bind<ICalendarSyncAdapter>().FromInstance(adapter).AsSingle();
        }
    }
}
