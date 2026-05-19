using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Signals;
using Zenject;

namespace Kruty1918.Moyva.Economy.Runtime
{
    /// <summary>
    /// Turn processor service: orchestrates economy tick for all settlements each turn.
    /// </summary>
    internal interface IEconomyTurnProcessor
    {
        /// <summary>Process one economy turn for all active settlements.</summary>
        void ProcessTurn(ISettlementRegistry registry, SignalBus signalBus, EconomyDatabaseSO database, float turnDurationSeconds);
    }
}
