using Kruty1918.Moyva.Signals;

namespace Kruty1918.Moyva.Economy.Runtime
{
    public sealed class EconomyStatisticsMediator : IEconomyStatisticsMediator
    {
        private readonly EconomyStatisticsService _statisticsService;

        public EconomyStatisticsMediator(EconomyStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        public bool TryGetSettlementStatistics(string settlementId, out SettlementStatisticsSnapshot snapshot)
        {
            snapshot = default;
            return _statisticsService != null
                && _statisticsService.TryGetSettlementStatistics(settlementId, out snapshot);
        }

        public KingdomStatisticsSnapshot GetKingdomStatistics(string ownerId)
        {
            return _statisticsService != null
                ? _statisticsService.GetKingdomStatistics(ownerId)
                : default;
        }
    }
}
