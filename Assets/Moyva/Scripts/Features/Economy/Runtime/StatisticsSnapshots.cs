using System;

namespace Kruty1918.Moyva.Economy.Runtime
{
    public readonly struct SettlementStatisticsSnapshot
    {
        public string SettlementId { get; }
        public string SettlementName { get; }
        public string OwnerId { get; }
        public int Population { get; }
        public int LastArrivals { get; }
        public int LastDeaths { get; }
        public float AverageArrivals { get; }
        public float AverageBirthRate { get; }
        public float AverageMortalityRate { get; }
        public float AverageMood { get; }
        public int SampleCount { get; }

        public SettlementStatisticsSnapshot(
            string settlementId,
            string settlementName,
            string ownerId,
            int population,
            int lastArrivals,
            int lastDeaths,
            float averageArrivals,
            float averageBirthRate,
            float averageMortalityRate,
            float averageMood,
            int sampleCount)
        {
            SettlementId = settlementId ?? string.Empty;
            SettlementName = settlementName ?? string.Empty;
            OwnerId = ownerId ?? string.Empty;
            Population = population;
            LastArrivals = lastArrivals;
            LastDeaths = lastDeaths;
            AverageArrivals = averageArrivals;
            AverageBirthRate = averageBirthRate;
            AverageMortalityRate = averageMortalityRate;
            AverageMood = averageMood;
            SampleCount = sampleCount;
        }
    }

    public readonly struct KingdomStatisticsSnapshot
    {
        public string OwnerId { get; }
        public int ActiveSettlements { get; }
        public int TotalPopulation { get; }
        public int TotalLastArrivals { get; }
        public int TotalLastDeaths { get; }
        public float AverageArrivals { get; }
        public float AverageBirthRate { get; }
        public float AverageMortalityRate { get; }
        public float AverageMood { get; }
        public int HistorySampleCount { get; }

        public KingdomStatisticsSnapshot(
            string ownerId,
            int activeSettlements,
            int totalPopulation,
            int totalLastArrivals,
            int totalLastDeaths,
            float averageArrivals,
            float averageBirthRate,
            float averageMortalityRate,
            float averageMood,
            int historySampleCount)
        {
            OwnerId = ownerId ?? string.Empty;
            ActiveSettlements = activeSettlements;
            TotalPopulation = totalPopulation;
            TotalLastArrivals = totalLastArrivals;
            TotalLastDeaths = totalLastDeaths;
            AverageArrivals = averageArrivals;
            AverageBirthRate = averageBirthRate;
            AverageMortalityRate = averageMortalityRate;
            AverageMood = averageMood;
            HistorySampleCount = historySampleCount;
        }
    }
}
