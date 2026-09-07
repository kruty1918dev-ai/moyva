using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Turns.API;

namespace Kruty1918.Moyva.Turns.Runtime
{
    internal sealed partial class TurnService
    {
        private readonly Dictionary<string, long> _completedTurns = new(StringComparer.Ordinal);
        private bool _deriveLegacyHistory;
        private bool _historyPrepared;
        private IReadOnlyList<TurnParticipantHistorySnapshot> _historyCache;
        private long _historyGlobalTurn;
        private TurnPhase _historyPhase;

        public IReadOnlyList<TurnParticipantHistorySnapshot> GetParticipantHistory()
        {
            if (_historyCache != null && _historyGlobalTurn == GlobalTurn && _historyPhase == Phase)
                return _historyCache;

            var result = new List<TurnParticipantHistorySnapshot>(_factions.Count);
            for (int index = 0; index < _factions.Count; index++)
            {
                string ownerId = _factions[index].OwnerId;
                _completedTurns.TryGetValue(ownerId, out long completed);
                bool eliminated = _eliminatedOwners.Contains(ownerId);
                result.Add(new TurnParticipantHistorySnapshot(
                    ownerId, completed,
                    !eliminated && !_matchEnded && index == _activeFactionIndex,
                    string.Equals(ownerId, LocalOwnerId, StringComparison.Ordinal),
                    eliminated));
            }

            _historyGlobalTurn = GlobalTurn;
            _historyPhase = Phase;
            _historyCache = result.AsReadOnly();
            return _historyCache;
        }

        public void RestoreHistory(IReadOnlyList<TurnParticipantHistorySnapshot> history)
        {
            // Validate the whole roster before replacing live state.
            if (history != null)
            {
                var seen = new HashSet<string>(StringComparer.Ordinal);
                for (int index = 0; index < history.Count; index++)
                {
                    var item = history[index];
                    if (string.IsNullOrWhiteSpace(item.OwnerId) || item.OwnerId != item.OwnerId.Trim()
                        || item.CompletedTurns < 0 || !seen.Add(item.OwnerId))
                        throw new ArgumentException("Invalid turn participant history.", nameof(history));
                }
            }

            _historyCache = null;
            _historyPrepared = true;
            _completedTurns.Clear();
            _eliminatedOwners.Clear();
            _deriveLegacyHistory = history == null;
            if (history == null)
                return;

            for (int index = 0; index < history.Count; index++)
            {
                var item = history[index];
                _completedTurns.Add(item.OwnerId, item.CompletedTurns);
                if (item.IsEliminated)
                    _eliminatedOwners.Add(item.OwnerId);
            }
        }

        private void RestoreLegacyHistory()
        {
            long completed = Math.Max(0L, GlobalTurn - 1);
            for (int index = 0; index < _factions.Count; index++)
                _completedTurns[_factions[index].OwnerId] = completed / _factions.Count
                    + (index < completed % _factions.Count ? 1L : 0L);
            _deriveLegacyHistory = false;
            _historyCache = null;
        }

        private void RecordCompletedTurn(string ownerId)
        {
            _completedTurns.TryGetValue(ownerId, out long completed);
            _completedTurns[ownerId] = checked(completed + 1);
            _historyCache = null;
        }
    }
}
