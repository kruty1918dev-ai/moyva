using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Multiplayer.Persistence;
using UnityEngine.SceneManagement;

namespace Kruty1918.Moyva.Multiplayer.Core
{
    public sealed partial class SessionManager
    {
        // ── Session API ───────────────────────────────────────────────────────────

        public async Task<bool> CreateOrJoinSessionAsync(SessionConnectOptions options, CancellationToken ct = default)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            _config = MultiplayerConfigLifecycle.LoadValidateFreeze(_configStore);
            var opts = NormalizeOptions(options);
            _localPlayerId = opts.LocalIdentity.PlayerId;
            if (_network is INetworkPeerIdentityConfigurator identityConfigurator)
                identityConfigurator.SetLocalPeerId(_localPlayerId);
            // A multiplayer join always requires an explicit room identifier.
            if (!options.CreateIfNotExists && string.IsNullOrWhiteSpace(options.RoomId))
            {
                _failurePolicy.HandleRecoverable(FailureCategory.ParticipantRejected, "Join requires a room id.");
                return false;
            }

            // Config consistency should be validated even for offline provider.
            if (_config.EnforceConfigConsistency)
            {
                uint localChecksum = ComputeConfigChecksum(_config);
                if (opts.ConfigChecksum != 0 && opts.ConfigChecksum != localChecksum)
                {
                    _failurePolicy.HandleRecoverable(FailureCategory.ConfigMismatch, "Config checksums differ.");
                    return false;
                }
            }

            // Explicit local provider path used by direct gameplay and save inspection.
            if (_config.ProviderType == NetworkProviderType.Offline)
            {
                return StartLocalSession(opts);
            }

            // Local participant policy
            WorldSnapshot snapshot = _snapshotStore.Exists(opts.RoomId) ? _snapshotStore.Load(opts.RoomId) : null;
            if (!_participantPolicy.CanJoin(opts.LocalIdentity, _participants, opts.Rules, snapshot))
            {
                _failurePolicy.HandleRecoverable(FailureCategory.ParticipantRejected, $"Participant {opts.LocalIdentity.PlayerId} rejected.");
                return false;
            }

            try
            {
                if (opts.CreateIfNotExists)
                    return await HostFlowAsync(opts, ct);

                return await JoinFlowAsync(opts, ct);
            }
            catch (Exception e)
            {
                _failurePolicy.HandleNonRecoverable(FailureCategory.NetworkDisconnect, e.Message);
                return await FailSessionAsync(ct);
            }
        }

        public async Task LeaveSessionAsync(CancellationToken ct = default)
        {
            // Voluntary leave must preserve the lobby if the provider rejects the request.
            await _lobby.LeaveAsync(ct);
            await SafeCleanupAsync(ct);
        }

        // ── Host / Join flows ─────────────────────────────────────────────────────

        private async Task<bool> HostFlowAsync(SessionConnectOptions opts, CancellationToken ct)
        {
            var hostSessionId = BuildTransportHostSessionId(_config.ProviderType, opts.RoomId);
            var hostResult = await _network.HostSessionAsync(hostSessionId, ct);
            if (hostResult == null || !hostResult.Success)
            {
                var error = hostResult?.ErrorMessage ?? "Failed to host network session.";
                _failurePolicy.HandleNonRecoverable(FailureCategory.NetworkDisconnect, error);
                return await FailSessionAsync(ct);
            }

            var transportJoinCode = hostResult.SessionId?.Trim() ?? string.Empty;
            if (_config.ProviderType == NetworkProviderType.Relay && !RelayJoinCodeUtility.IsValid(transportJoinCode))
            {
                var error = $"Relay host returned invalid join code '{transportJoinCode}'.";
                _failurePolicy.HandleNonRecoverable(FailureCategory.NetworkDisconnect, error);
                return await FailSessionAsync(ct);
            }

            var createOpts = new CreateRoomOptions(opts.RoomId, opts.Rules.MaxParticipants, isPrivate: false,
                displayName: opts.LocalIdentity.Nickname, relayJoinCode: transportJoinCode);

            LobbyRoom lobby;
            try
            {
                lobby = await _lobby.CreateRoomAsync(createOpts, ct);
            }
            catch (Exception e)
            {
                _failurePolicy.HandleNonRecoverable(FailureCategory.NetworkDisconnect, e.Message);
                try { await _network.LeaveSessionAsync(ct); } catch { }
                return await FailSessionAsync(ct);
            }

            if (lobby == null)
            {
                _failurePolicy.HandleNonRecoverable(FailureCategory.NetworkDisconnect, "Failed to create lobby.");
                try { await _network.LeaveSessionAsync(ct); } catch { }
                return await FailSessionAsync(ct);
            }

            _currentLobbyId = lobby.LobbyId;
            _currentLobbyCode = lobby.LobbyCode;
            _isHost = true;

            // Relay join code → lobby data so clients can discover it.
            try
            {
                await _lobby.SetRelayJoinCodeAsync(transportJoinCode, ct);
            }
            catch (Exception e)
            {
                _failurePolicy.HandleNonRecoverable(FailureCategory.NetworkDisconnect, e.Message);
                try { await _lobby.LeaveAsync(ct); } catch { }
                try { await _network.LeaveSessionAsync(ct); } catch { }
                return await FailSessionAsync(ct);
            }

            _currentSessionId = transportJoinCode;
            _currentRules = opts.Rules;

            UpsertLocalParticipant(opts.LocalIdentity, isHost: true);
            CleanupHostAliasParticipants();
            SaveMigrationCheckpoint();
            return true;
        }

        private static string BuildTransportHostSessionId(NetworkProviderType providerType, string roomId)
        {
            if (providerType == NetworkProviderType.Relay)
                return string.Empty;

            return string.IsNullOrWhiteSpace(roomId)
                ? Guid.NewGuid().ToString("N")
                : roomId.Trim();
        }

        private async Task<bool> JoinFlowAsync(SessionConnectOptions opts, CancellationToken ct)
        {
            var lobby = await _lobby.JoinByCodeAsync(opts.RoomId, opts.LocalIdentity.Nickname, ct);
            if (lobby == null)
            {
                _failurePolicy.HandleNonRecoverable(FailureCategory.NetworkDisconnect, "Failed to join lobby.");
                return await FailSessionAsync(ct);
            }

            _currentLobbyId = lobby.LobbyId;
            _currentLobbyCode = lobby.LobbyCode;
            _isHost = false;

            // Wait for relay code to become available (host publishes it asynchronously).
            string relayCode = await WaitForRelayCodeAsync(TimeSpan.FromSeconds(15), ct);
            if (string.IsNullOrEmpty(relayCode))
            {
                _failurePolicy.HandleNonRecoverable(FailureCategory.NetworkDisconnect, "Relay code not published by host.");
                return await FailSessionAsync(ct);
            }

            var joinResult = await _network.JoinSessionAsync(relayCode, ct);
            if (!joinResult.Success)
            {
                _failurePolicy.HandleNonRecoverable(FailureCategory.NetworkDisconnect, joinResult.ErrorMessage);
                return await FailSessionAsync(ct);
            }

            _currentSessionId = joinResult.SessionId;
            _currentRules = opts.Rules;

            UpsertLocalParticipant(opts.LocalIdentity, isHost: false);
            SaveMigrationCheckpoint();
            return true;
        }

        private async Task<string> WaitForRelayCodeAsync(TimeSpan timeout, CancellationToken ct)
        {
            var deadline = DateTime.UtcNow.Add(timeout);
            while (DateTime.UtcNow < deadline)
            {
                if (ct.IsCancellationRequested) return null;

                var snap = _lobby.Current;
                if (snap != null && !string.IsNullOrEmpty(snap.RelayJoinCode))
                    return snap.RelayJoinCode;

                try { await Task.Delay(500, ct); }
                catch (OperationCanceledException) { return null; }
            }
            return null;
        }

    }
}
