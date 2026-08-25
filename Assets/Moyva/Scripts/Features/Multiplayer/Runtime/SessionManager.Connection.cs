using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Diagnostics.API;
using Kruty1918.Moyva.Diagnostics.Runtime.Flows;
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

            var flow = _diagnostics?.StartFlow(
                options.RoomId,
                new DiagnosticContext()
                    .Add("roomId", options.RoomId)
                    .Add("createIfNotExists", options.CreateIfNotExists));
            _diagnostics?.CompleteStep(flow, MultiplayerSessionDiagnosticSteps.ConnectRequested, $"roomId={options.RoomId}, create={options.CreateIfNotExists}");

            _config = MultiplayerConfigLifecycle.LoadValidateFreeze(_configStore, _logger);
            var opts = NormalizeOptions(options);
            _localPlayerId = opts.LocalIdentity.PlayerId;
            if (_network is INetworkPeerIdentityConfigurator identityConfigurator)
                identityConfigurator.SetLocalPeerId(_localPlayerId);
            _diagnostics?.CompleteStep(flow, MultiplayerSessionDiagnosticSteps.TransportSelected, $"provider={_config.ProviderType}, player={_localPlayerId}");

            // Missing join target: fall back to local solo instead of attempting online join.
            if (!options.CreateIfNotExists && string.IsNullOrWhiteSpace(options.RoomId))
            {
                _logger.Warn("Join requested without room id. Falling back to local single-player session.");
                _diagnostics?.SkipStep(flow, MultiplayerSessionDiagnosticSteps.LobbyResolved, "missing-room-id");
                _diagnostics?.CompleteStep(flow, MultiplayerSessionDiagnosticSteps.GameplayReady, "fallback=offline-solo");
                _diagnostics?.Report(flow);
                return StartOfflineSolo(opts);
            }

            // Config consistency should be validated even for offline provider.
            if (_config.EnforceConfigConsistency)
            {
                uint localChecksum = ComputeConfigChecksum(_config);
                if (opts.ConfigChecksum != 0 && opts.ConfigChecksum != localChecksum)
                {
                    _logger.Warn($"Config checksum mismatch: local={localChecksum:X8}, remote={opts.ConfigChecksum:X8}");
                    _failurePolicy.HandleRecoverable(FailureCategory.ConfigMismatch, "Config checksums differ.");
                    _diagnostics?.FailStep(flow, MultiplayerSessionDiagnosticSteps.LobbyResolved, "config-mismatch");
                    _diagnostics?.Report(flow);
                    return false;
                }
            }

            // Pure-offline / solo path
            if (_config.ProviderType == NetworkProviderType.Offline)
            {
                _diagnostics?.SkipStep(flow, MultiplayerSessionDiagnosticSteps.LobbyResolved, "offline-provider");
                _diagnostics?.CompleteStep(flow, MultiplayerSessionDiagnosticSteps.GameplayReady, "provider=offline");
                _diagnostics?.Report(flow);
                return StartOfflineSolo(opts);
            }

            // Local participant policy
            WorldSnapshot snapshot = _snapshotStore.Exists(opts.RoomId) ? _snapshotStore.Load(opts.RoomId) : null;
            if (!_participantPolicy.CanJoin(opts.LocalIdentity, _participants, opts.Rules, snapshot))
            {
                _failurePolicy.HandleRecoverable(FailureCategory.ParticipantRejected, $"Participant {opts.LocalIdentity.PlayerId} rejected.");
                _diagnostics?.FailStep(flow, MultiplayerSessionDiagnosticSteps.LocalPlayerRegistered, "participant-rejected", opts.LocalIdentity.PlayerId);
                _diagnostics?.Report(flow);
                return false;
            }

            try
            {
                if (opts.CreateIfNotExists)
                    return await HostFlowAsync(opts, flow, ct);

                return await JoinFlowAsync(opts, flow, ct);
            }
            catch (Exception e)
            {
                _logger.Error($"CreateOrJoinSession failed: {e.Message}");
                _failurePolicy.HandleNonRecoverable(FailureCategory.NetworkDisconnect, e.Message);
                _diagnostics?.FailStep(flow, MultiplayerSessionDiagnosticSteps.SessionStarted, "exception", e.Message);
                _diagnostics?.Report(flow);
                return await FallbackToOfflineSoloAsync(opts, "Unhandled exception in session flow.", ct);
            }
        }

        public async Task LeaveSessionAsync(CancellationToken ct = default)
        {
            _logger.Info($"Leaving session: {_currentSessionId}");
            await SafeCleanupAsync(ct);
        }

        // ── Host / Join flows ─────────────────────────────────────────────────────

        private async Task<bool> HostFlowAsync(SessionConnectOptions opts, Kruty1918.Moyva.Diagnostics.API.IDiagnosticFlow flow, CancellationToken ct)
        {
            _logger.Info($"Host flow: room='{opts.RoomId}' max={opts.Rules.MaxParticipants}");

            var hostSessionId = BuildTransportHostSessionId(_config.ProviderType, opts.RoomId);
            var hostResult = await _network.HostSessionAsync(hostSessionId, ct);
            if (hostResult == null || !hostResult.Success)
            {
                var error = hostResult?.ErrorMessage ?? "Failed to host network session.";
                _failurePolicy.HandleNonRecoverable(FailureCategory.NetworkDisconnect, error);
                _diagnostics?.FailStep(flow, MultiplayerSessionDiagnosticSteps.SessionStarted, "host-session-failed", error);
                _diagnostics?.Report(flow);
                return await FallbackToOfflineSoloAsync(opts, error, ct);
            }
            _diagnostics?.CompleteStep(flow, MultiplayerSessionDiagnosticSteps.SessionStarted, $"hostSessionId={hostResult.SessionId}");

            var transportJoinCode = hostResult.SessionId?.Trim() ?? string.Empty;
            if (_config.ProviderType == NetworkProviderType.Relay && !RelayJoinCodeUtility.IsValid(transportJoinCode))
            {
                var error = $"Relay host returned invalid join code '{transportJoinCode}'.";
                _failurePolicy.HandleNonRecoverable(FailureCategory.NetworkDisconnect, error);
                return await FallbackToOfflineSoloAsync(opts, error, ct);
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
                _diagnostics?.FailStep(flow, MultiplayerSessionDiagnosticSteps.LobbyResolved, "lobby-create-exception", e.Message);
                try { await _network.LeaveSessionAsync(ct); } catch (Exception leaveError) { _logger.Warn($"Leave after failed lobby create failed: {leaveError.Message}"); }
                _diagnostics?.Report(flow);
                return await FallbackToOfflineSoloAsync(opts, e.Message, ct);
            }

            if (lobby == null)
            {
                _failurePolicy.HandleNonRecoverable(FailureCategory.NetworkDisconnect, "Failed to create lobby.");
                _diagnostics?.FailStep(flow, MultiplayerSessionDiagnosticSteps.LobbyResolved, "lobby-create-null");
                try { await _network.LeaveSessionAsync(ct); } catch (Exception leaveError) { _logger.Warn($"Leave after failed lobby create failed: {leaveError.Message}"); }
                _diagnostics?.Report(flow);
                return await FallbackToOfflineSoloAsync(opts, "Failed to create lobby.", ct);
            }
            _diagnostics?.CompleteStep(flow, MultiplayerSessionDiagnosticSteps.LobbyResolved, $"lobbyCode={lobby.LobbyCode}, host=true");

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
                _diagnostics?.FailStep(flow, MultiplayerSessionDiagnosticSteps.LobbyResolved, "relay-code-publish-failed", e.Message);
                try { await _lobby.LeaveAsync(ct); } catch (Exception leaveError) { _logger.Warn($"Lobby leave after Relay code publish failed: {leaveError.Message}"); }
                try { await _network.LeaveSessionAsync(ct); } catch (Exception leaveError) { _logger.Warn($"Network leave after Relay code publish failed: {leaveError.Message}"); }
                _diagnostics?.Report(flow);
                return await FallbackToOfflineSoloAsync(opts, e.Message, ct);
            }

            _currentSessionId = transportJoinCode;
            _currentRules = opts.Rules;

            UpsertLocalParticipant(opts.LocalIdentity, isHost: true);
            _diagnostics?.CompleteStep(flow, MultiplayerSessionDiagnosticSteps.LocalPlayerRegistered, $"playerId={opts.LocalIdentity.PlayerId}, host=true");
            CleanupHostAliasParticipants();
            SaveMigrationCheckpoint();
            _diagnostics?.CompleteStep(flow, MultiplayerSessionDiagnosticSteps.ParticipantsSynced, $"count={_participants.Count}, host=true");
            _diagnostics?.CompleteStep(flow, MultiplayerSessionDiagnosticSteps.GameplayReady, $"lobbyCode={_currentLobbyCode}");
            _diagnostics?.Report(flow);
            _logger.Info($"Session hosted. LobbyCode={_currentLobbyCode} RelayCode={_currentSessionId}");
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

        private async Task<bool> JoinFlowAsync(SessionConnectOptions opts, Kruty1918.Moyva.Diagnostics.API.IDiagnosticFlow flow, CancellationToken ct)
        {
            _logger.Info($"Join flow: lobbyCode='{opts.RoomId}'");

            var lobby = await _lobby.JoinByCodeAsync(opts.RoomId, opts.LocalIdentity.Nickname, ct);
            if (lobby == null)
            {
                _failurePolicy.HandleNonRecoverable(FailureCategory.NetworkDisconnect, "Failed to join lobby.");
                _diagnostics?.FailStep(flow, MultiplayerSessionDiagnosticSteps.LobbyResolved, "join-lobby-null");
                _diagnostics?.Report(flow);
                return await FallbackToOfflineSoloAsync(opts, "Failed to join lobby.", ct);
            }
            _diagnostics?.CompleteStep(flow, MultiplayerSessionDiagnosticSteps.LobbyResolved, $"lobbyCode={lobby.LobbyCode}, host=false");

            _currentLobbyId = lobby.LobbyId;
            _currentLobbyCode = lobby.LobbyCode;
            _isHost = false;

            // Wait for relay code to become available (host publishes it asynchronously).
            string relayCode = await WaitForRelayCodeAsync(TimeSpan.FromSeconds(15), ct);
            if (string.IsNullOrEmpty(relayCode))
            {
                _failurePolicy.HandleNonRecoverable(FailureCategory.NetworkDisconnect, "Relay code not published by host.");
                _diagnostics?.FailStep(flow, MultiplayerSessionDiagnosticSteps.SessionStarted, "relay-code-missing");
                _diagnostics?.Report(flow);
                return await FallbackToOfflineSoloAsync(opts, "Relay code not published by host.", ct);
            }

            var joinResult = await _network.JoinSessionAsync(relayCode, ct);
            if (!joinResult.Success)
            {
                _failurePolicy.HandleNonRecoverable(FailureCategory.NetworkDisconnect, joinResult.ErrorMessage);
                _diagnostics?.FailStep(flow, MultiplayerSessionDiagnosticSteps.SessionStarted, "join-session-failed", joinResult.ErrorMessage);
                _diagnostics?.Report(flow);
                return await FallbackToOfflineSoloAsync(opts, joinResult.ErrorMessage, ct);
            }
            _diagnostics?.CompleteStep(flow, MultiplayerSessionDiagnosticSteps.SessionStarted, $"sessionId={joinResult.SessionId}");

            _currentSessionId = joinResult.SessionId;
            _currentRules = opts.Rules;

            UpsertLocalParticipant(opts.LocalIdentity, isHost: false);
            _diagnostics?.CompleteStep(flow, MultiplayerSessionDiagnosticSteps.LocalPlayerRegistered, $"playerId={opts.LocalIdentity.PlayerId}, host=false");
            SaveMigrationCheckpoint();
            _diagnostics?.CompleteStep(flow, MultiplayerSessionDiagnosticSteps.ParticipantsSynced, $"count={_participants.Count}, host=false");
            _diagnostics?.CompleteStep(flow, MultiplayerSessionDiagnosticSteps.GameplayReady, $"lobbyCode={_currentLobbyCode}");
            _diagnostics?.Report(flow);
            _logger.Info($"Session joined. Lobby={_currentLobbyCode}");
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
