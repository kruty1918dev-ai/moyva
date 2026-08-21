using Kruty1918.Moyva.Turns.API;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Diagnostics
{
    internal sealed class BotDiagnosticsContextProvider
    {
        private readonly ITurnService _turns;

        [Inject]
        public BotDiagnosticsContextProvider(
            [InjectOptional] ITurnService turns = null)
        {
            _turns = turns;
        }

        public string OwnerId
            => _turns?.ActiveOwnerId ?? string.Empty;

        public long GlobalTurn
            => _turns?.GlobalTurn ?? 0;

        public string Phase
            => _turns?.Phase.ToString() ?? "Unavailable";

        public bool IsActiveFactionBot
            => _turns?.IsActiveFactionBot ?? false;

        public int Round
            => _turns?.Round ?? 0;

        public int ActionsThisTurn
            => _turns?.ActionsThisTurn ?? 0;
    }
}
