using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;

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
        private readonly SemaphoreSlim _switchLock = new SemaphoreSlim(1, 1);

        public event Action<LobbyRoom> LobbyUpdated;
        public event Action<string> KickedFromLobby;

        public LobbyRoom Current => _inner?.Current;

        public SwitchableLobbyService(MultiplayerConfig config, IMultiplayerLogger logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _inner = CreateByType(_config.ProviderType);
            HookInner(_inner);
        }

        private ILobbyService CreateByType(Networking.NetworkProviderType type)
        {
            return type switch
            {
                Networking.NetworkProviderType.Lan => new LanLobbyService(_logger),
                Networking.NetworkProviderType.Offline => new OfflineLobbyService(_logger),
                _ => new UgsLobbyService(_logger)
            };
        }

        private void HookInner(ILobbyService service)
        {
            service.LobbyUpdated += OnLobbyUpdatedInternal;
            service.KickedFromLobby += OnKickedFromLobbyInternal;
        }

        private void UnhookInner(ILobbyService service)
        {
            try { service.LobbyUpdated -= OnLobbyUpdatedInternal; } catch { }
            try { service.KickedFromLobby -= OnKickedFromLobbyInternal; } catch { }
        }

        private void OnLobbyUpdatedInternal(LobbyRoom r) => LobbyUpdated?.Invoke(r);
        private void OnKickedFromLobbyInternal(string s) => KickedFromLobby?.Invoke(s);

        public async Task SwitchToAsync(Networking.NetworkProviderType type, CancellationToken ct = default)
        {
            await _switchLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if ((_inner is LanLobbyService && type == Networking.NetworkProviderType.Lan) ||
                    (_inner is UgsLobbyService && type != Networking.NetworkProviderType.Lan))
                {
                    return;
                }

                try { await _inner.LeaveAsync(ct).ConfigureAwait(false); } catch { }
                UnhookInner(_inner);
                _inner = CreateByType(type);
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

        public Task<IReadOnlyList<LobbyRoom>> QueryRoomsAsync(CancellationToken ct = default)
            => _inner.QueryRoomsAsync(ct);

        public Task LeaveAsync(CancellationToken ct = default) => _inner.LeaveAsync(ct);

        public Task KickAsync(string playerId, CancellationToken ct = default) => _inner.KickAsync(playerId, ct);

        public Task SetRelayJoinCodeAsync(string relayJoinCode, CancellationToken ct = default) => _inner.SetRelayJoinCodeAsync(relayJoinCode, ct);

        public Task LockAsync(bool locked, CancellationToken ct = default) => _inner.LockAsync(locked, ct);

        public void Dispose()
        {
            try { _inner?.Dispose(); } catch { }
        }
    }
}
