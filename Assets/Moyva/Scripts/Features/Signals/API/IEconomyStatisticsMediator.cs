namespace Kruty1918.Moyva.Signals
{
    public readonly struct SettlementStatisticsSnapshot
    {
        public SettlementStatisticsSnapshot(
            string settlementId,
            string settlementName,
            string ownerId,
            int population,
            int lastArrivals,
            int lastDeaths,
            float avgArrivalsPerTurn,
            float avgBirthRatePerTurn,
            float avgMortalityPerTurn,
            float avgMood,
            int historyTurns)
        {
            SettlementId = settlementId;
            SettlementName = settlementName;
            OwnerId = ownerId;
            Population = population;
            LastArrivals = lastArrivals;
            LastDeaths = lastDeaths;
            AvgArrivalsPerTurn = avgArrivalsPerTurn;
            AvgBirthRatePerTurn = avgBirthRatePerTurn;
            AvgMortalityPerTurn = avgMortalityPerTurn;
            AvgMood = avgMood;
            HistoryTurns = historyTurns;
        }

        public string SettlementId { get; }
        public string SettlementName { get; }
        public string OwnerId { get; }
        public int Population { get; }
        public int LastArrivals { get; }
        public int LastDeaths { get; }
        public float AvgArrivalsPerTurn { get; }
        public float AvgBirthRatePerTurn { get; }
        public float AvgMortalityPerTurn { get; }
        public float AvgMood { get; }
        public int HistoryTurns { get; }
    }

    public readonly struct KingdomStatisticsSnapshot
    {
        public KingdomStatisticsSnapshot(
            string ownerId,
            int activeSettlements,
            int totalPopulation,
            int lastArrivals,
            int lastDeaths,
            float avgArrivalsPerTurn,
            float avgBirthRatePerTurn,
            float avgMortalityPerTurn,
            float avgMood,
            int historyTurns)
        {
            OwnerId = ownerId;
            ActiveSettlements = activeSettlements;
            TotalPopulation = totalPopulation;
            LastArrivals = lastArrivals;
            LastDeaths = lastDeaths;
            AvgArrivalsPerTurn = avgArrivalsPerTurn;
            AvgBirthRatePerTurn = avgBirthRatePerTurn;
            AvgMortalityPerTurn = avgMortalityPerTurn;
            AvgMood = avgMood;
            HistoryTurns = historyTurns;
        }

        public string OwnerId { get; }
        public int ActiveSettlements { get; }
        public int TotalPopulation { get; }
        public int LastArrivals { get; }
        public int LastDeaths { get; }
        public float AvgArrivalsPerTurn { get; }
        public float AvgBirthRatePerTurn { get; }
        public float AvgMortalityPerTurn { get; }
        public float AvgMood { get; }
        public int HistoryTurns { get; }
    }

    public interface IEconomyStatisticsMediator
    {
        bool TryGetSettlementStatistics(string settlementId, out SettlementStatisticsSnapshot snapshot);
        KingdomStatisticsSnapshot GetKingdomStatistics(string ownerId);
    }
}
