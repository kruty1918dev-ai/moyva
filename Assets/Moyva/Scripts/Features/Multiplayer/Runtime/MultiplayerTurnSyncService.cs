using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Calendar.Core;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Turns.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    internal sealed class MultiplayerTurnAuthorityPolicy : ITurnAuthorityPolicy
    {
        private readonly ILocalGameplayRoleResolver _roles;
        public MultiplayerTurnAuthorityPolicy(ILocalGameplayRoleResolver roles) => _roles = roles;
        public bool IsAuthoritative => _roles.Resolve().IsAuthoritative;
    }

    internal sealed partial class MultiplayerTurnSyncService :
        IInitializable, ITickable, IDisposable, ITurnRemoteCommandRequester
    {
        private readonly IGameCommandSyncService _sync;
        private readonly ISessionManager _session;
        private readonly ILocalGameplayRoleResolver _roles;
        private readonly ITurnService _turns;
        private readonly ITurnHistoryQuery _history;
        private readonly ITurnHistoryRestorer _historyRestorer;
        private readonly ITurnStateRestorer _turnRestorer;
        private readonly ICalendarService _calendar;
        private readonly ICalendarStateRestorer _calendarRestorer;
        private readonly IGameResultStateStore _result;
        private readonly string _hostEpoch = Guid.NewGuid().ToString("N");
        private readonly string _syncRequestId = Guid.NewGuid().ToString("N");
        private readonly Dictionary<string, TurnSyncPayload> _replies = new(StringComparer.Ordinal);
        private readonly Queue<string> _replyOrder = new();
        private string _epoch = string.Empty;
        private long _revision = 1;
        private long _lastAppliedRevision;
        private bool _dirty = true;
        private bool _disposed;
        private TurnSyncPayload _pendingEnd;
        private TurnSyncPayload _pendingState;
        private float _requestStarted;
        private float _nextSendAt;
        private float _nextSyncAt;
        private string _stateError = string.Empty;

        public MultiplayerTurnSyncService(IGameCommandSyncService sync, ISessionManager session,
            ILocalGameplayRoleResolver roles, ITurnService turns, ITurnHistoryQuery history,
            ITurnHistoryRestorer historyRestorer, ITurnStateRestorer turnRestorer,
            ICalendarService calendar, ICalendarStateRestorer calendarRestorer, IGameResultStateStore result)
        {
            _sync = sync; _session = session; _roles = roles; _turns = turns;
            _history = history; _historyRestorer = historyRestorer; _turnRestorer = turnRestorer;
            _calendar = calendar; _calendarRestorer = calendarRestorer; _result = result;
        }

        public event Action<bool, string> EndTurnResolved;
        public bool IsEndTurnPending => _pendingEnd != null;

        public void Initialize()
        {
            _sync.RegisterHandler(GameCommandType.EndTurn, OnMessage);
            _sync.RegisterHandler(GameCommandType.GameStateChange, OnMessage);
            _turns.StateChanged += MarkDirty;
        }

        public void Dispose()
        {
            _disposed = true;
            _turns.StateChanged -= MarkDirty;
            _sync.RegisterHandler(GameCommandType.EndTurn, null);
            _sync.RegisterHandler(GameCommandType.GameStateChange, null);
            _pendingEnd = null;
            _pendingState = null;
            _replies.Clear();
            _replyOrder.Clear();
        }

        private void MarkDirty()
        {
            _dirty = true;
            _revision++;
        }
        private bool HasStableState => _turns.Factions.Count > 0
            && (_turns.Phase == TurnPhase.AwaitingInput || _turns.Phase == TurnPhase.Completed);

        public void Tick()
        {
            var role = _roles.Resolve().Role;
            if (_disposed || role == LocalGameplayRole.Offline) return;
            if (role == LocalGameplayRole.Host)
            {
                if (_dirty && HasStableState) SendState(null, string.Empty);
                return;
            }
            TryApplyPendingState();
            float now = Time.unscaledTime;
            if (now >= _nextSyncAt)
            {
                SendToHost(new TurnSyncPayload { Kind = TurnSyncMessageKind.RequestState,
                    RequestId = _syncRequestId });
                _nextSyncAt = now + (_lastAppliedRevision == 0 ? 1f : 5f);
            }
            if (_pendingEnd == null) return;
            if (now - _requestStarted >= 6f)
            {
                _pendingEnd = null;
                EndTurnResolved?.Invoke(false, "No confirmation from the host. Check the connection and retry.");
            }
            else if (now >= _nextSendAt)
            {
                SendToHost(_pendingEnd);
                _nextSendAt = now + 1f;
            }
        }

        public bool TryRequestEndTurn(out string reason)
        {
            reason = null;
            if (_disposed || _roles.Resolve().Role != LocalGameplayRole.Client)
                reason = "Only a connected client sends turn requests.";
            else if (_epoch.Length == 0 || _lastAppliedRevision == 0 || _pendingState != null)
                reason = "Waiting for the host's turn state.";
            else if (_stateError.Length != 0)
                reason = _stateError;
            else if (_pendingEnd != null)
                reason = "Waiting for the host to confirm the turn.";
            else if (!_turns.CanOwnerAct(_turns.LocalOwnerId, out reason))
                return false;
            if (reason != null) return false;
            var message = new TurnSyncPayload { Kind = TurnSyncMessageKind.EndTurn,
                Epoch = _epoch, RequestId = Guid.NewGuid().ToString("N"),
                OwnerId = _turns.LocalOwnerId, GlobalTurn = _turns.GlobalTurn };
            _pendingEnd = message;
            _requestStarted = Time.unscaledTime;
            _nextSendAt = _requestStarted + 1f;
            if (SendToHost(message)) return true;
            _pendingEnd = null;
            reason = "The host is not connected.";
            return false;
        }

        private bool SendToHost(TurnSyncPayload message)
        {
            foreach (var participant in _session.Participants)
            {
                string peerId = participant?.Identity?.PlayerId;
                if (participant?.IsHost != true || string.IsNullOrWhiteSpace(peerId)) continue;
                _sync.SendCommandToPeer(peerId, message.Kind == TurnSyncMessageKind.EndTurn
                    ? GameCommandType.EndTurn : GameCommandType.GameStateChange, message.ToBytes());
                return true;
            }
            return false;
        }

        private void SendState(string peerId, string requestId)
        {
            if (!HasStableState) return;
            var message = new TurnSyncPayload { Kind = TurnSyncMessageKind.State,
                Epoch = _hostEpoch, Revision = _revision, RequestId = requestId,
                OwnerId = _turns.ActiveOwnerId, GlobalTurn = _turns.GlobalTurn,
                Round = _turns.Round, Actions = _turns.ActionsThisTurn,
                CalendarHours = _calendar.TotalHoursSinceEpoch, IsGameOver = _result.IsGameOver,
                WinnerId = _result.WinnerId, History = _history.GetParticipantHistory() };
            byte[] bytes = message.ToBytes();
            if (peerId == null)
            {
                _sync.SendCommand(GameCommandType.GameStateChange, bytes);
                _dirty = false;
            }
            else _sync.SendCommandToPeer(peerId, GameCommandType.GameStateChange, bytes);
        }
    }
}
