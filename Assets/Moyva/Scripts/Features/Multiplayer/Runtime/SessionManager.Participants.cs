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
        // ── Participant sync ──────────────────────────────────────────────────────

        private void UpsertLocalParticipant(ParticipantIdentity identity, bool isHost)
        {
            var existing = _participants.Find(p => p.Identity.PlayerId == identity.PlayerId);
            if (existing == null)
                _participants.Add(new Participant(identity, isHost));
            else if (existing.IsHost != isHost)
                _participants[_participants.IndexOf(existing)] = new Participant(existing.Identity, isHost);
        }

        private void OnLobbyUpdated(LobbyRoom snapshot)
        {
            if (snapshot == null) return;
            var previousHostId = ResolveHostPlayerId();
            var previousSessionId = _currentSessionId;

            if (_lobby is ILobbyLocalIdentity lobbyIdentity
                && !string.IsNullOrWhiteSpace(lobbyIdentity.LocalPlayerId))
            {
                _localPlayerId = lobbyIdentity.LocalPlayerId;
                _isHost = string.Equals(snapshot.HostPlayerId, _localPlayerId, StringComparison.Ordinal);
            }

            _currentLobbyId = string.IsNullOrWhiteSpace(snapshot.LobbyId) ? _currentLobbyId : snapshot.LobbyId;
            _currentLobbyCode = string.IsNullOrWhiteSpace(snapshot.LobbyCode) ? _currentLobbyCode : snapshot.LobbyCode;

            // Add missing remote participants from the lobby's player list.
            foreach (var p in snapshot.Players)
            {
                if (string.IsNullOrEmpty(p.PlayerId)) continue;
                var existingIndex = _participants.FindIndex(x => x.Identity.PlayerId == p.PlayerId);
                if (existingIndex >= 0)
                {
                    var existing = _participants[existingIndex];
                    _participants[existingIndex] = new Participant(
                        new ParticipantIdentity(p.PlayerId, string.IsNullOrWhiteSpace(p.DisplayName) ? existing.Identity.Nickname : p.DisplayName),
                        isHost: p.IsHost);
                    continue;
                }

                // In tests/stubs the lobby host id may not match the local identity.
                // When we're the authoritative host, ignore that foreign host alias.
                if (_isHost && p.IsHost && !string.IsNullOrEmpty(_localPlayerId) &&
                    !string.Equals(p.PlayerId, _localPlayerId, StringComparison.Ordinal))
                    continue;

                var identity = new ParticipantIdentity(p.PlayerId, p.DisplayName);
                _participants.Add(new Participant(identity, isHost: p.IsHost));
            }

            // Remove participants no longer in the lobby.
            for (int i = _participants.Count - 1; i >= 0; i--)
            {
                var p = _participants[i];
                bool stillInLobby = false;
                for (int j = 0; j < snapshot.Players.Count; j++)
                {
                    if (snapshot.Players[j].PlayerId == p.Identity.PlayerId) { stillInLobby = true; break; }
                }
                if (!stillInLobby)
                {
                    if (ShouldKeepLocalParticipantMissingFromLobby(p))
                        continue;

                    _participants.RemoveAt(i);
                }
            }

            string nextHostId = snapshot.HostPlayerId ?? string.Empty;
            bool hostChanged = !string.IsNullOrWhiteSpace(previousHostId) &&
                               !string.Equals(previousHostId, nextHostId, StringComparison.Ordinal);

            if (hostChanged)
                SaveMigrationCheckpoint();

            if (!_hostTransportMigrationInProgress &&
                !string.IsNullOrWhiteSpace(_localPlayerId) &&
                string.Equals(nextHostId, _localPlayerId, StringComparison.Ordinal) &&
                hostChanged)
            {
                _ = PromoteLocalHostTransportAsync(snapshot);
            }
            else if (!_hostTransportMigrationInProgress &&
                     !string.IsNullOrWhiteSpace(snapshot.RelayJoinCode) &&
                     !string.IsNullOrWhiteSpace(previousSessionId) &&
                     !string.Equals(snapshot.RelayJoinCode, previousSessionId, StringComparison.Ordinal))
            {
                _ = JoinMigratedHostTransportAsync(snapshot);
            }
        }

        private async Task PromoteLocalHostTransportAsync(LobbyRoom snapshot)
        {
            if (_config == null || _config.ProviderType == NetworkProviderType.Offline)
                return;

            _hostTransportMigrationInProgress = true;
            try
            {
                try { await _network.LeaveSessionAsync(); } catch { }

                var result = await _network.HostSessionAsync(
                    BuildTransportHostSessionId(_config.ProviderType, snapshot?.LobbyId));
                if (result == null || !result.Success)
                {
                    _failurePolicy.HandleNonRecoverable(FailureCategory.HostMigrationFailed, result?.ErrorMessage ?? "Failed to host migrated transport.");
                    return;
                }

                string joinCode = result.SessionId?.Trim() ?? string.Empty;
                _currentSessionId = joinCode;
                _isHost = true;
                if (_lobby is ILobbyHostMigrationService migrationLobby)
                    await migrationLobby.TryTransferHostAsync(_localPlayerId, joinCode);
                else
                    await _lobby.SetRelayJoinCodeAsync(joinCode);

                SaveMigrationCheckpoint();
            }
            catch (Exception e)
            {
                _failurePolicy.HandleNonRecoverable(FailureCategory.HostMigrationFailed, e.Message);
            }
            finally
            {
                _hostTransportMigrationInProgress = false;
            }
        }

        private async Task JoinMigratedHostTransportAsync(LobbyRoom snapshot)
        {
            if (_config == null || _config.ProviderType == NetworkProviderType.Offline)
                return;

            _hostTransportMigrationInProgress = true;
            try
            {
                try { await _network.LeaveSessionAsync(); } catch { }
                var result = await _network.JoinSessionAsync(snapshot.RelayJoinCode);
                if (result == null || !result.Success)
                {
                    _failurePolicy.HandleRecoverable(FailureCategory.HostMigrationFailed, result?.ErrorMessage ?? "Failed to join migrated transport.");
                    return;
                }

                _currentSessionId = result.SessionId;
                _isHost = false;
                SaveMigrationCheckpoint();
            }
            catch (Exception e)
            {
                _failurePolicy.HandleRecoverable(FailureCategory.HostMigrationFailed, e.Message);
            }
            finally
            {
                _hostTransportMigrationInProgress = false;
            }
        }

        private bool ShouldKeepLocalParticipantMissingFromLobby(
            Participant participant)
        {
            if (participant == null
                || string.IsNullOrWhiteSpace(_localPlayerId)
                || string.IsNullOrWhiteSpace(participant.Identity?.PlayerId))
            {
                return false;
            }

            return _isHost
                   && string.Equals(
                       participant.Identity.PlayerId,
                       _localPlayerId,
                       StringComparison.Ordinal);
        }

        private void OnKickedFromLobby(string reason)
        {

            // If the local player has already moved to the gameplay scene
            // (i.e. active scene name is not the HomeMenu scene), surface a
            // host-disconnected notice and bounce back to the main menu so the
            // user is not left in a broken multiplayer match.
            try
            {
                var active = SceneManager.GetActiveScene().name;
                if (!string.IsNullOrEmpty(active) &&
                    !active.Equals("HomeMenu", StringComparison.OrdinalIgnoreCase))
                {
                    HostDisconnectNotice.Set(reason);
                    SceneManager.LoadScene("HomeMenu", LoadSceneMode.Single);
                }
            }
            catch (Exception)
            {
            }

            _ = SafeCleanupAsync();
        }

        private void OnPeerConnected(string peerId)
        {
            if (string.IsNullOrEmpty(peerId) || peerId == _localPlayerId) return;
            CancelPendingDisconnect(peerId);
            // Lobby update will fill identity; nothing to add here unless missing.
        }

        private void OnPeerDisconnected(string peerId)
        {
            if (string.IsNullOrEmpty(peerId)) return;

            if (!_participants.Exists(p => p.Identity.PlayerId == peerId))
            {
                return;
            }

            ScheduleDisconnectFinalization(peerId);
        }

        private void FinalizePeerDisconnect(string peerId)
        {
            if (string.IsNullOrEmpty(peerId))
                return;

            var leaving = _participants.Find(p => p.Identity.PlayerId == peerId);
            if (leaving == null)
            {
                return;
            }

            _participants.Remove(leaving);

            if (leaving.IsHost && _participants.Count > 0)
            {
                if (_config != null && !_config.EnableHostMigration)
                {
                    _failurePolicy.HandleNonRecoverable(FailureCategory.HostMigrationFailed, "Host migration is disabled by feature toggle.");
                    _ = SafeCleanupAsync();
                    return;
                }

                var migrated = _hostMigration.ChooseNewHost(_participants);
                if (migrated == null)
                {
                    _failurePolicy.HandleNonRecoverable(FailureCategory.NetworkDisconnect, "Host disconnected.");
                    _ = SafeCleanupAsync();
                    return;
                }

                int index = _participants.FindIndex(p => p.Identity.PlayerId == migrated.Identity.PlayerId);
                if (index >= 0)
                    _participants[index] = migrated;

                _isHost = string.Equals(migrated.Identity.PlayerId, _localPlayerId, StringComparison.Ordinal);
                SaveMigrationCheckpoint();
                return;
            }

            SaveMigrationCheckpoint();
        }

    }
}
