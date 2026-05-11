using System;
using System.Collections.Generic;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Signals;
using Zenject;

namespace Kruty1918.Moyva.GameMode.Runtime
{
    /// <summary>
    /// Центральний контролер UI-панелей на основі ігрового режиму.
    ///
    /// Підписується на <see cref="GameModeChangedSignal"/> та показує панель, чий
    /// <see cref="IGameModePanel.TargetMode"/> збігається з новим режимом, а всі
    /// інші приховує.
    ///
    /// Щоб додати нову панель — реалізуйте <see cref="IGameModePanel"/> і зареєструйте
    /// реалізацію через Zenject (наприклад,
    /// <c>Container.BindInterfacesTo&lt;MyPanel&gt;().AsSingle()</c>).
    /// Жодних змін у цьому класі не потрібно.
    /// </summary>
    internal sealed class GameModePanelController : IInitializable, IDisposable
    {
        private readonly IReadOnlyList<IGameModePanel> _panels;
        private readonly SignalBus _signalBus;

        [Inject]
        public GameModePanelController(List<IGameModePanel> panels, SignalBus signalBus)
        {
            _panels = panels;
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<GameModeChangedSignal>(OnGameModeChanged);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<GameModeChangedSignal>(OnGameModeChanged);
        }

        private void OnGameModeChanged(GameModeChangedSignal signal)
        {
            foreach (var panel in _panels)
            {
                if (panel.TargetMode == signal.NewMode)
                    panel.Show();
                else
                    panel.Hide();
            }
        }
    }
}
