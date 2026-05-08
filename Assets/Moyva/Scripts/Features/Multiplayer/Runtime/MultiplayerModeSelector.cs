using System;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    public sealed class MultiplayerModeSelector : IMultiplayerModeSelector, IDisposable
    {
        private readonly SwitchableNetworkProvider _networkProvider;
        private readonly SwitchableLobbyService _lobbyService;
        private readonly MultiplayerConfig _config;
        private readonly IMultiplayerLogger _logger;
        private readonly SemaphoreSlim _switchLock = new SemaphoreSlim(1, 1);

        public event Action<NetworkProviderType> OnModeChanged;

        public NetworkProviderType CurrentMode { get; private set; }
        public NetworkProviderType EffectiveMode => _lobbyService.CurrentProviderType;

        public MultiplayerModeSelector(
            SwitchableNetworkProvider networkProvider,
            SwitchableLobbyService lobbyService,
            MultiplayerConfig config,
            IMultiplayerLogger logger)
        {
            _networkProvider = networkProvider ?? throw new ArgumentNullException(nameof(networkProvider));
            _lobbyService = lobbyService ?? throw new ArgumentNullException(nameof(lobbyService));
            _config = config;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var initialMode = config != null ? config.ProviderType : _lobbyService.RequestedProviderType;
            CurrentMode = initialMode;
        }

        public async Task SetModeAsync(NetworkProviderType mode, CancellationToken ct = default)
        {
            await _switchLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                var resolvedNetworkMode = NormalizeNetworkMode(mode);
                var changed = CurrentMode != mode;

                if (_networkProvider.CurrentType != resolvedNetworkMode)
                    await _networkProvider.SwitchToAsync(resolvedNetworkMode, ct).ConfigureAwait(false);

                if (_lobbyService.RequestedProviderType != mode)
                    await _lobbyService.SwitchToAsync(mode, ct).ConfigureAwait(false);

                CurrentMode = mode;

                if (changed)
                {
                    _logger.Info($"[MultiplayerModeSelector] Mode switched to {mode}; effective lobby provider is {EffectiveMode}, network provider is {resolvedNetworkMode}.");
                    OnModeChanged?.Invoke(mode);
                }
            }
            finally
            {
                _switchLock.Release();
            }
        }

        private NetworkProviderType NormalizeNetworkMode(NetworkProviderType requestedMode)
        {
            if (requestedMode != NetworkProviderType.Relay || RelayNetworkProvider.IsRuntimeAvailable)
                return requestedMode;

            var fallbackMode = _config != null ? _config.FallbackProviderType : NetworkProviderType.Offline;
            if (fallbackMode == NetworkProviderType.Relay)
                fallbackMode = NetworkProviderType.Offline;

            _logger.Warn($"[MultiplayerModeSelector] Relay mode requested, but Relay runtime is unavailable. Falling back to {fallbackMode}.");
            return fallbackMode;
        }

        public void Dispose()
        {
            _switchLock.Dispose();
        }
    }
}