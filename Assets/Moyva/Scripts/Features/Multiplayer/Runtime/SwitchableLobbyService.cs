using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Jsonization;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;

namespace Kruty1918.Moyva.Multiplayer.Lobbies
{
    /// <summary>
    /// Switchable ILobbyService wrapper. Keeps a stable DI entry point while allowing runtime switches
    /// between UGS lobby and LAN discovery.
    /// </summary>
    public sealed class SwitchableLobbyService : ILobbyService, ILobbyLocalIdentity, IDisposable
    {
        private readonly MultiplayerConfig _config;
        private readonly string _configFingerprint;

        private ILobbyService _inner;
        private NetworkProviderType _requestedProviderType;
        private NetworkProviderType _effectiveProviderType;
        private readonly SemaphoreSlim _switchLock = new SemaphoreSlim(1, 1);

        public event Action<LobbyRoom> LobbyUpdated;
        public event Action<string> KickedFromLobby;
        public event Action<LobbyState> StateChanged;

        public LobbyRoom Current => _inner?.Current;
        public string LocalPlayerId => (_inner as ILobbyLocalIdentity)?.LocalPlayerId ?? string.Empty;
        public LobbyState State => _inner?.State ?? LobbyState.Closed;

        public SwitchableLobbyService(MultiplayerConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _configFingerprint = MoyvaJsonRuntime.ConfigFingerprint;

            _requestedProviderType = _config.ProviderType;
            _inner = CreateByType(_requestedProviderType, out _effectiveProviderType);
            HookInner(_inner);
        }

        public NetworkProviderType RequestedProviderType => _requestedProviderType;

        /// <summary>
        /// Фактичний тип активної lobby-реалізації.
        /// </summary>
        public NetworkProviderType CurrentProviderType => _effectiveProviderType;

        private ILobbyService CreateByType(NetworkProviderType type, out NetworkProviderType effectiveType)
        {
            if (type == NetworkProviderType.Lan)
            {
                effectiveType = NetworkProviderType.Lan;
                return new LanLobbyService();
            }

            if (type == NetworkProviderType.Offline)
            {
                effectiveType = NetworkProviderType.Offline;
                return new OfflineLobbyService();
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
                            return new UgsLobbyService();
                        }
                    }
                }
                catch { }

            effectiveType = NetworkProviderType.Offline;
            return new OfflineLobbyService();
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
            => _inner.CreateRoomAsync(
                options?.WithConfigFingerprint(_configFingerprint),
                ct);

        public Task<LobbyRoom> JoinByCodeAsync(string lobbyCode, string displayName, CancellationToken ct = default)
            => JoinCompatibleAsync(
                _inner.JoinByCodeAsync(lobbyCode, displayName, ct),
                ct);

        public Task<LobbyRoom> JoinByIdAsync(string lobbyId, string displayName, CancellationToken ct = default)
            => JoinCompatibleAsync(
                _inner.JoinByIdAsync(lobbyId, displayName, ct),
                ct);

        public Task<LobbyRoom> JoinByCodeWithPasswordAsync(string lobbyCode, string displayName, string password, CancellationToken ct = default)
            => JoinCompatibleAsync(
                _inner.JoinByCodeWithPasswordAsync(lobbyCode, displayName, password, ct),
                ct);

        public Task<IReadOnlyList<LobbyRoom>> QueryRoomsAsync(CancellationToken ct = default)
            => _inner.QueryRoomsAsync(ct);

        public Task LeaveAsync(CancellationToken ct = default) => _inner.LeaveAsync(ct);

        public Task KickAsync(string playerId, CancellationToken ct = default) => _inner.KickAsync(playerId, ct);

        public Task SetRelayJoinCodeAsync(string relayJoinCode, CancellationToken ct = default) => _inner.SetRelayJoinCodeAsync(relayJoinCode, ct);

        public Task LockAsync(bool locked, byte[] startedWorldSettingsBytes = null, CancellationToken ct = default) => _inner.LockAsync(locked, startedWorldSettingsBytes, ct);

        private async Task<LobbyRoom> JoinCompatibleAsync(
            Task<LobbyRoom> joinOperation,
            CancellationToken ct)
        {
            LobbyRoom room = await joinOperation.ConfigureAwait(false);
            if (room == null)
                return null;

            if (!_config.EnforceConfigConsistency
                || string.Equals(
                    room.ConfigFingerprint,
                    _configFingerprint,
                    StringComparison.Ordinal))
            {
                return room;
            }

            UnityEngine.Debug.LogError(
                "[MultiplayerConfig] Refusing lobby join because gameplay JSON fingerprints " +
                $"do not match. Lobby={room.ConfigFingerprint}, Local={_configFingerprint}.");
            try
            {
                await _inner.LeaveAsync(ct).ConfigureAwait(false);
            }
            catch (Exception leaveException)
            {
                UnityEngine.Debug.LogWarning(
                    $"[MultiplayerConfig] Cleanup after rejected join failed: {leaveException.Message}");
            }

            throw new RoomConfigMismatchException(
                _configFingerprint,
                room.ConfigFingerprint);
        }

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
