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
        // ── Cleanup & helpers ─────────────────────────────────────────────────────

        private async Task SafeCleanupAsync(CancellationToken ct = default)
        {
            CancelAllPendingDisconnects();
            try { await _network.LeaveSessionAsync(ct); } catch (Exception e) { _logger.Warn($"Net leave: {e.Message}"); }
            try { await _lobby.LeaveAsync(ct); } catch (Exception e) { _logger.Warn($"Lobby leave: {e.Message}"); }

            _participants.Clear();
            _currentSessionId = null;
            _currentLobbyId = null;
            _currentLobbyCode = null;
            _currentRules = null;
            _isHost = false;
        }

        private void SaveMigrationCheckpoint()
        {
            if (_hostMigrationCheckpoint == null)
                return;

            var snapshot = !string.IsNullOrWhiteSpace(_currentSessionId) && _snapshotStore.Exists(_currentSessionId)
                ? _snapshotStore.Load(_currentSessionId)
                : null;

            _hostMigrationCheckpoint.Save(new HostMigrationCheckpoint(
                _currentSessionId,
                _currentLobbyId,
                _isHost ? _localPlayerId : ResolveHostPlayerId(),
                new List<Participant>(_participants),
                snapshot,
                DateTime.UtcNow));
        }

        private string ResolveHostPlayerId()
        {
            foreach (var participant in _participants)
            {
                if (participant.IsHost)
                    return participant.Identity.PlayerId;
            }

            return string.Empty;
        }

        private void ScheduleDisconnectFinalization(string peerId)
        {
            if (!IsPeerPresentInCurrentLobby(peerId))
            {
                _logger.Warn($"Peer '{peerId}' disconnected and is absent from current lobby snapshot. Finalizing immediately.");
                FinalizePeerDisconnect(peerId);
                return;
            }

            CancelPendingDisconnect(peerId);
            var cts = new CancellationTokenSource();
            _pendingDisconnects[peerId] = cts;
            var delay = TimeSpan.FromSeconds(Math.Max(1f, _config?.GracefulReconnectWindowSeconds ?? 8f));
            _logger.Warn($"Peer '{peerId}' disconnected. Waiting {delay.TotalSeconds:0.#}s graceful reconnect window.");
            _ = FinalizeDisconnectAfterDelayAsync(peerId, cts.Token, delay);
        }

        private bool IsPeerPresentInCurrentLobby(string peerId)
        {
            var currentLobby = _lobby.Current;
            if (currentLobby?.Players == null || currentLobby.Players.Count == 0 || string.IsNullOrEmpty(peerId))
                return false;

            for (int index = 0; index < currentLobby.Players.Count; index++)
            {
                if (string.Equals(currentLobby.Players[index].PlayerId, peerId, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private async Task FinalizeDisconnectAfterDelayAsync(string peerId, CancellationToken ct, TimeSpan delay)
        {
            try
            {
                await Task.Delay(delay, ct);
                if (!ct.IsCancellationRequested)
                    FinalizePeerDisconnect(peerId);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                if (_pendingDisconnects.TryGetValue(peerId, out var cts) && cts.Token == ct)
                    _pendingDisconnects.Remove(peerId);
            }
        }

        private void CancelPendingDisconnect(string peerId)
        {
            if (!_pendingDisconnects.TryGetValue(peerId, out var cts))
                return;

            try { cts.Cancel(); } catch { }
            cts.Dispose();
            _pendingDisconnects.Remove(peerId);
        }

        private void CancelAllPendingDisconnects()
        {
            foreach (var pair in _pendingDisconnects)
            {
                try { pair.Value.Cancel(); } catch { }
                pair.Value.Dispose();
            }

            _pendingDisconnects.Clear();
        }

    }
}
