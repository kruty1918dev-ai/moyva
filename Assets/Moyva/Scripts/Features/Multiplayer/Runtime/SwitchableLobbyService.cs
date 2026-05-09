using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;

namespace Kruty1918.Moyva.Multiplayer.Lobbies
{
    /// <summary>
    /// Switchable ILobbyService wrapper. Keeps a stable DI entry point while allowing runtime switches
    /// between UGS lobby and LAN discovery.
    /// </summary>
    public sealed class SwitchableLobbyService : ILobbyService, IDisposable
    {
        private readonly MultiplayerConfig _config;
        private readonly IMultiplayerLogger _logger;

        private ILobbyService _inner;
        private NetworkProviderType _requestedProviderType;
        private NetworkProviderType _effectiveProviderType;
        private readonly SemaphoreSlim _switchLock = new SemaphoreSlim(1, 1);

        public event Action<LobbyRoom> LobbyUpdated;
        public event Action<string> KickedFromLobby;
        public event Action<LobbyState> StateChanged;

        public LobbyRoom Current => _inner?.Current;
        public LobbyState State => _inner?.State ?? LobbyState.Closed;

        public SwitchableLobbyService(MultiplayerConfig config, IMultiplayerLogger logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _requestedProviderType = _config.ProviderType;
            _inner = CreateByType(_requestedProviderType, out _effectiveProviderType);
            HookInner(_inner);
        }

        public NetworkProviderType RequestedProviderType => _requestedProviderType;

        /// <summary>
        /// Current active provider type represented by the inner implementation.
        /// Useful for diagnostics and logging.
        /// </summary>
        public NetworkProviderType CurrentProviderType => _effectiveProviderType;

        private ILobbyService CreateByType(NetworkProviderType type, out NetworkProviderType effectiveType)
        {
            if (type == NetworkProviderType.Lan)
            {
                effectiveType = NetworkProviderType.Lan;
                return new LanLobbyService(_logger);
            }

            if (type == NetworkProviderType.Offline)
            {
                effectiveType = NetworkProviderType.Offline;
                return new OfflineLobbyService(_logger);
            }

            // For Relay (UGS) provider: if the Unity Lobbies package isn't installed in the project,
            // creating a real UgsLobbyService would be a no-op stub (returns null). Detect presence
            // of the Unity Lobbies assembly at runtime and fall back to OfflineLobbyService when
            // the package is not present so CreateRoomAsync doesn't silently return null.
                try
                {
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (var asm in assemblies)
                    {
                        var name = asm.GetName().Name;
                        if (string.Equals(name, "Unity.Services.Lobbies", StringComparison.OrdinalIgnoreCase) ||
                            name.StartsWith("Unity.Services.Lobbies", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(name, "Unity.Services.Multiplayer", StringComparison.OrdinalIgnoreCase) ||
                            name.StartsWith("Unity.Services.Multiplayer", StringComparison.OrdinalIgnoreCase))
                        {
                            effectiveType = NetworkProviderType.Relay;
                            return new UgsLobbyService(_logger);
                        }
                    }
                }
                catch { }

            effectiveType = NetworkProviderType.Offline;
            _logger?.Warn($"[SwitchableLobbyService] Lobby provider '{type}' is unavailable - falling back to OfflineLobbyService.");
            return new OfflineLobbyService(_logger);
        }

        private void HookInner(ILobbyService service)
        {
            service.LobbyUpdated += OnLobbyUpdatedInternal;
            service.KickedFromLobby += OnKickedFromLobbyInternal;
            service.StateChanged += OnStateChangedInternal;
        }

        private void UnhookInner(ILobbyService service)
        {
            try { service.LobbyUpdated -= OnLobbyUpdatedInternal; } catch { }
            try { service.KickedFromLobby -= OnKickedFromLobbyInternal; } catch { }
            try { service.StateChanged -= OnStateChangedInternal; } catch { }
        }

        private void OnLobbyUpdatedInternal(LobbyRoom r) => LobbyUpdated?.Invoke(r);
        private void OnKickedFromLobbyInternal(string s) => KickedFromLobby?.Invoke(s);
        private void OnStateChangedInternal(LobbyState s) => StateChanged?.Invoke(s);

        private static void DisposeInner(ILobbyService service)
        {
            try
            {
                if (service is IDisposable disposable)
                    disposable.Dispose();
            }
            catch { }
        }

        public async Task SwitchToAsync(NetworkProviderType type, CancellationToken ct = default)
        {
            await _switchLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (_requestedProviderType == type)
                {
                    return;
                }

                try { await _inner.LeaveAsync(ct).ConfigureAwait(false); } catch { }
                UnhookInner(_inner);
                DisposeInner(_inner);

                _requestedProviderType = type;
                _inner = CreateByType(type, out _effectiveProviderType);
                HookInner(_inner);
            }
            finally { _switchLock.Release(); }
        }

        // Delegates
        public Task<LobbyRoom> CreateRoomAsync(CreateRoomOptions options, CancellationToken ct = default)
            => _inner.CreateRoomAsync(options, ct);

        public Task<LobbyRoom> JoinByCodeAsync(string lobbyCode, string displayName, CancellationToken ct = default)
            => _inner.JoinByCodeAsync(lobbyCode, displayName, ct);

        public Task<LobbyRoom> JoinByIdAsync(string lobbyId, string displayName, CancellationToken ct = default)
            => _inner.JoinByIdAsync(lobbyId, displayName, ct);

        public Task<LobbyRoom> JoinByCodeWithPasswordAsync(string lobbyCode, string displayName, string password, CancellationToken ct = default)
            => _inner.JoinByCodeWithPasswordAsync(lobbyCode, displayName, password, ct);

        public Task<IReadOnlyList<LobbyRoom>> QueryRoomsAsync(CancellationToken ct = default)
            => _inner.QueryRoomsAsync(ct);

        public Task LeaveAsync(CancellationToken ct = default) => _inner.LeaveAsync(ct);

        public Task KickAsync(string playerId, CancellationToken ct = default) => _inner.KickAsync(playerId, ct);

        public Task SetRelayJoinCodeAsync(string relayJoinCode, CancellationToken ct = default) => _inner.SetRelayJoinCodeAsync(relayJoinCode, ct);

        public Task LockAsync(bool locked, byte[] startedWorldSettingsBytes = null, CancellationToken ct = default) => _inner.LockAsync(locked, startedWorldSettingsBytes, ct);

        public void Dispose()
        {
            try
            {
                if (_inner is IDisposable d) d.Dispose();
            }
            catch { }
            _switchLock.Dispose();
        }
    }
}
