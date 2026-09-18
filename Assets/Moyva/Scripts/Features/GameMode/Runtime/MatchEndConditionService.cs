using System;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Turns.API;
using Zenject;

namespace Kruty1918.Moyva.GameMode.Runtime
{
    internal sealed class MatchEndConditionService : IInitializable, IDisposable
    {
        private readonly ITurnService _turns;
        private readonly IGameStateService _gameState;
        private readonly ITurnHistoryQuery _history;
        private readonly ITurnAuthorityPolicy _authority;

        public MatchEndConditionService(
            ITurnService turns,
            IGameStateService gameState,
            ITurnHistoryQuery history,
            [InjectOptional] ITurnAuthorityPolicy authority = null)
        {
            _turns = turns;
            _gameState = gameState;
            _history = history;
            _authority = authority;
        }

        public void Initialize()
            => _turns.StateChanged += EvaluateResult;

        public void Dispose()
            => _turns.StateChanged -= EvaluateResult;

        private void EvaluateResult()
        {
            if ((_authority != null && !_authority.IsAuthoritative)
                || _gameState.CurrentState == GameStateType.GameOver
                || _turns.Phase != TurnPhase.AwaitingInput
                || _turns.Factions.Count <= 1)
            {
                return;
            }

            string winner = null;
            int activeOwners = 0;
            var history = _history.GetParticipantHistory();
            for (int index = 0; index < history.Count; index++)
            {
                string ownerId = history[index].OwnerId;
                if (history[index].IsEliminated || string.IsNullOrWhiteSpace(ownerId))
                {
                    continue;
                }

                activeOwners++;
                winner = ownerId;
                if (activeOwners > 1)
                    return;
            }

            if (activeOwners <= 1)
                _gameState.EndGame(winner);
        }
    }
}
