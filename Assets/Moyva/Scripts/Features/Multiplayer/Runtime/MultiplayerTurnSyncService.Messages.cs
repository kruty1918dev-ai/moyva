using System;
using System.IO;
using Kruty1918.Moyva.Multiplayer.Core;
using UnityEngine;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    internal sealed partial class MultiplayerTurnSyncService
    {
        private void OnMessage(string senderId, byte[] bytes)
        {
            if (_disposed) return;
            var role = _roles.Resolve().Role;
            if (role == LocalGameplayRole.Offline) return;
            if (role == LocalGameplayRole.Client
                && !MultiplayerAuthorityService.IsAuthorizedHostSender(_session.Participants, senderId)) return;
            try
            {
                TurnSyncPayload message = TurnSyncPayload.FromBytes(bytes);
                if (role == LocalGameplayRole.Host)
                {
                    if (!MultiplayerAuthorityService.TryResolveAuthorizedRequestOwner(
                        _session.Participants, senderId, message.OwnerId, message.OwnerId, out _, out _)) return;
                    if (message.Kind == TurnSyncMessageKind.RequestState)
                        SendState(senderId, message.RequestId);
                    else if (message.Kind == TurnSyncMessageKind.EndTurn)
                        HandleEndTurn(senderId, message);
                    return;
                }
                if (message.Kind == TurnSyncMessageKind.State)
                {
                    if (_epoch.Length == 0 && message.RequestId != _syncRequestId) return;
                    if (_epoch.Length != 0 && message.Epoch != _epoch)
                    {
                        if (message.RequestId == _syncRequestId)
                            ReportStateError("The host's match has changed. Rejoin the match to continue.");
                        return;
                    }
                    if (message.Revision <= _lastAppliedRevision
                        || (_pendingState != null && message.Revision <= _pendingState.Revision)) return;
                    _epoch = message.Epoch;
                    _pendingState = message;
                    TryApplyPendingState();
                }
                else if ((message.Kind == TurnSyncMessageKind.Accepted || message.Kind == TurnSyncMessageKind.Rejected)
                    && message.Epoch == _epoch && _pendingEnd?.RequestId == message.RequestId
                    && message.OwnerId == _pendingEnd.OwnerId)
                {
                    bool accepted = message.Kind == TurnSyncMessageKind.Accepted;
                    _pendingEnd = null;
                    if (message.GlobalTurn != _turns.GlobalTurn) _nextSyncAt = 0f;
                    EndTurnResolved?.Invoke(accepted, accepted ? "Turn ended." : message.Reason);
                }
            }
            catch (Exception exception) when (exception is IOException || exception is ArgumentException)
            {
                Debug.LogWarning($"[MultiplayerTurns] Rejected invalid turn message: {exception.Message}");
            }
        }

        private void HandleEndTurn(string senderId, TurnSyncPayload request)
        {
            if (request.Epoch != _hostEpoch) return;
            string key = senderId + ":" + request.RequestId;
            if (!_replies.TryGetValue(key, out var reply))
            {
                string reason = "The turn has already changed.";
                bool accepted = request.GlobalTurn == _turns.GlobalTurn
                    && _turns.TryEndTurn(request.OwnerId, out reason);
                reply = new TurnSyncPayload { Kind = accepted ? TurnSyncMessageKind.Accepted : TurnSyncMessageKind.Rejected,
                    Epoch = _hostEpoch, RequestId = request.RequestId, OwnerId = request.OwnerId,
                    GlobalTurn = _turns.GlobalTurn, Reason = reason ?? string.Empty };
                _replies.Add(key, reply);
                _replyOrder.Enqueue(key);
                while (_replyOrder.Count > 128) _replies.Remove(_replyOrder.Dequeue());
            }
            SendState(senderId, string.Empty);
            _sync.SendCommandToPeer(senderId, GameCommandType.EndTurn, reply.ToBytes());
        }

        private void TryApplyPendingState()
        {
            var state = _pendingState;
            if (state == null || _turns.Factions.Count == 0) return;
            if (state.History.Count != _turns.Factions.Count)
            {
                ReportStateError("Waiting for matching participant lists from the host.");
                return;
            }
            for (int i = 0; i < state.History.Count; i++)
            {
                if (state.History[i].OwnerId == _turns.Factions[i].OwnerId) continue;
                ReportStateError("Host and client participant order differs. Turn commands are blocked.");
                return;
            }
            // Apply a public projection, never replay end/start-turn gameplay callbacks on clients.
            _historyRestorer.RestoreHistory(state.History);
            if (state.IsGameOver != _result.IsGameOver || state.WinnerId != _result.WinnerId)
                _result.RestoreResult(state.IsGameOver, state.WinnerId);
            _calendarRestorer.RestoreByTotalHours(state.CalendarHours);
            _turnRestorer.Restore(state.Round, state.GlobalTurn, state.OwnerId, state.Actions);
            _lastAppliedRevision = state.Revision;
            _pendingState = null;
            _stateError = string.Empty;
        }

        private void ReportStateError(string reason)
        {
            if (reason == _stateError) return;
            _stateError = reason;
            Debug.LogWarning($"[MultiplayerTurns] {reason}");
            EndTurnResolved?.Invoke(false, reason);
        }
    }
}
