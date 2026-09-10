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
        private readonly SemaphoreSlim _switchLock = new SemaphoreSlim(1, 1);
        private int _hasExplicitSelection;

        public event Action<NetworkProviderType> OnModeChanged;

        public NetworkProviderType CurrentMode { get; private set; }
        public NetworkProviderType EffectiveMode => _lobbyService.CurrentProviderType;

        public MultiplayerModeSelector(
            SwitchableNetworkProvider networkProvider,
            SwitchableLobbyService lobbyService,
            MultiplayerConfig config)
        {
            _networkProvider = networkProvider ?? throw new ArgumentNullException(nameof(networkProvider));
            _lobbyService = lobbyService ?? throw new ArgumentNullException(nameof(lobbyService));
            _config = config;

            var initialMode = config != null ? config.ProviderType : _lobbyService.RequestedProviderType;
            CurrentMode = initialMode;
        }

        public async Task SetModeAsync(NetworkProviderType mode, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            Interlocked.Exchange(ref _hasExplicitSelection, 1);
            await _switchLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                await ApplyModeAsync(mode, NormalizeNetworkMode(mode), ct).ConfigureAwait(false);
            }
            finally
            {
                _switchLock.Release();
            }
        }

        // Connectivity initialization can finish after the user has opened or created a LAN room.
        // Bootstrap may only choose a default while the session is still unclaimed.
        internal async Task ApplyBootstrapModeAsync(
            NetworkProviderType mode,
            NetworkProviderType networkMode,
            CancellationToken ct = default)
        {
            await _switchLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (Volatile.Read(ref _hasExplicitSelection) != 0 || _lobbyService.Current != null)
                    return;

                await ApplyModeAsync(mode, networkMode, ct).ConfigureAwait(false);
            }
            finally
            {
                _switchLock.Release();
            }
        }

        private async Task ApplyModeAsync(
            NetworkProviderType mode,
            NetworkProviderType networkMode,
            CancellationToken ct)
        {
            var changed = CurrentMode != mode;

            if (_networkProvider.CurrentType != networkMode)
                await _networkProvider.SwitchToAsync(networkMode, ct).ConfigureAwait(false);

            if (_lobbyService.RequestedProviderType != mode)
                await _lobbyService.SwitchToAsync(mode, ct).ConfigureAwait(false);

            CurrentMode = mode;
            if (changed)
                OnModeChanged?.Invoke(mode);
        }

        private NetworkProviderType NormalizeNetworkMode(NetworkProviderType requestedMode)
        {
            if (requestedMode != NetworkProviderType.Relay || RelayNetworkProvider.IsRuntimeAvailable)
                return requestedMode;

            var fallbackMode = _config != null ? _config.FallbackProviderType : NetworkProviderType.Offline;
            if (fallbackMode == NetworkProviderType.Relay)
                fallbackMode = NetworkProviderType.Offline;
            return fallbackMode;
        }

        public void Dispose()
        {
            _switchLock.Dispose();
        }
    }
}
