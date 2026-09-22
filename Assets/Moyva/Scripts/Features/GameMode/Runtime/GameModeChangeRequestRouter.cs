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
        private readonly System.Collections.Generic.IReadOnlyList<IGameModeTransitionPolicy> _transitionPolicies;
        private bool _disposed;

        [Inject]
        public GameModeChangeRequestRouter(
            SignalBus signalBus,
            IGameModeService gameModeService,
            [InjectOptional] System.Collections.Generic.List<IGameModeTransitionPolicy> transitionPolicies = null)
        {
            _signalBus = signalBus;
            _gameModeService = gameModeService;
            _transitionPolicies = transitionPolicies;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<GameModeChangeRequestedSignal>(OnModeChangeRequested);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _signalBus.TryUnsubscribe<GameModeChangeRequestedSignal>(OnModeChangeRequested);
        }

        private void OnModeChangeRequested(GameModeChangeRequestedSignal signal)
        {
            if (GameModeTransitionPolicyGuard.TryGetBlockReason(
                    _transitionPolicies, _gameModeService.CurrentMode, signal.RequestedMode, out _))
                return;

            _gameModeService.SetMode(signal.RequestedMode);
        }
    }
}
