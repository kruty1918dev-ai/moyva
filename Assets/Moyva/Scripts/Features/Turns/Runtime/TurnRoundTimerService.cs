using Zenject;

namespace Kruty1918.Moyva.Turns.Runtime
{
    /// <summary>
    /// Frame pump for the authoritative simultaneous-round timer. The service
    /// only forwards ticks into <see cref="TurnService.TickRoundTimer"/>, which
    /// itself gates on flow mode, phase, authority and configured duration.
    /// </summary>
    internal sealed class TurnRoundTimerService : ITickable
    {
        private readonly TurnService _turns;

        public TurnRoundTimerService(TurnService turns)
        {
            _turns = turns;
        }

        public void Tick()
            => _turns.TickRoundTimer();
    }
}
