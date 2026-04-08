using System;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Signals;
using Zenject;

namespace Kruty1918.Moyva.GameMode.Runtime
{
    /// <summary>
    /// Маршрутизує зовнішні запити на зміну режиму гри до IGameModeService.
    /// Це прибирає пряме керування режимом з UI-модулів.
    /// </summary>
    public sealed class GameModeChangeRequestRouter : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IGameModeService _gameModeService;

        [Inject]
        public GameModeChangeRequestRouter(SignalBus signalBus, IGameModeService gameModeService)
        {
            _signalBus = signalBus;
            _gameModeService = gameModeService;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<GameModeChangeRequestedSignal>(OnModeChangeRequested);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<GameModeChangeRequestedSignal>(OnModeChangeRequested);
        }

        private void OnModeChangeRequested(GameModeChangeRequestedSignal signal)
        {
            _gameModeService.SetMode(signal.RequestedMode);
        }
    }
}
