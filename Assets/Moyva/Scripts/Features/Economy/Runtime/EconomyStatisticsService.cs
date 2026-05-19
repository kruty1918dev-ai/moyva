using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Signals;
using Zenject;

namespace Kruty1918.Moyva.Economy.Runtime
{
    public sealed class EconomyStatisticsService : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly EconomyManager _economyManager;

        private readonly Dictionary<string, RollingSettlementStats> _statsBySettlement =
            new Dictionary<string, RollingSettlementStats>(StringComparer.Ordinal);

        private const int RollingWindowTurns = 24;

        public EconomyStatisticsService(SignalBus signalBus, EconomyManager economyManager)
        {
            _signalBus = signalBus;
            _economyManager = economyManager;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<EconomyTickCompletedSignal>(OnEconomyTickCompleted);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<EconomyTickCompletedSignal>(OnEconomyTickCompleted);
        }

        public bool TryGetSettlementStatistics(string settlementId, out SettlementStatisticsSnapshot snapshot)
        {
            snapshot = default;

            if (string.IsNullOrWhiteSpace(settlementId) || _economyManager == null)
                return false;

            var state = _economyManager.GetSettlement(settlementId);
            if (state == null)
                return false;

            if (!_statsBySettlement.TryGetValue(settlementId, out var rolling))
                rolling = default;

            snapshot = new SettlementStatisticsSnapshot(
                settlementId,
                _economyManager.GetSettlementNameOrFallback(settlementId),
                state.OwnerId,
                state.Residents != null ? state.Residents.Count : 0,
                rolling.LastArrivals,
                rolling.LastDeaths,
                rolling.GetAverageArrivals(),
                rolling.GetAverageBirthRate(),
                rolling.GetAverageMortalityRate(),
                rolling.GetAverageMood(),
                rolling.SampleCount);

            return true;
        }

        public KingdomStatisticsSnapshot GetKingdomStatistics(string ownerId)
        {
            string normalizedOwner = string.IsNullOrWhiteSpace(ownerId)
                ? EconomyManager.DefaultOwnerId
                : ownerId.Trim();

            if (_economyManager == null)
                return default;

            int activeSettlements = 0;
            int totalPopulation = 0;
            int totalLastArrivals = 0;
            int totalLastDeaths = 0;

            float sumAvgArrivals = 0f;
            float sumAvgBirthRate = 0f;
            float sumAvgMortality = 0f;
            float weightedMoodSum = 0f;
            int weightedMoodPopulation = 0;
            int historyTurnsMin = int.MaxValue;

            foreach (var pair in _economyManager.Settlements)
            {
                var state = pair.Value;
                if (state == null || !state.IsActive)
                    continue;

                if (!string.Equals(state.OwnerId, normalizedOwner, StringComparison.Ordinal))
                    continue;

                activeSettlements++;
                int population = state.Residents != null ? state.Residents.Count : 0;
                totalPopulation += population;

                if (_statsBySettlement.TryGetValue(state.SettlementId, out var rolling))
                {
                    totalLastArrivals += rolling.LastArrivals;
                    totalLastDeaths += rolling.LastDeaths;

                    sumAvgArrivals += rolling.GetAverageArrivals();
                    sumAvgBirthRate += rolling.GetAverageBirthRate();
                    sumAvgMortality += rolling.GetAverageMortalityRate();

                    float mood = rolling.GetAverageMood();
                    weightedMoodSum += mood * Math.Max(1, population);
                    weightedMoodPopulation += Math.Max(1, population);

                    historyTurnsMin = Math.Min(historyTurnsMin, rolling.SampleCount);
                }
            }

            float divider = Math.Max(1, activeSettlements);
            float avgMood = weightedMoodPopulation > 0
                ? weightedMoodSum / weightedMoodPopulation
                : 0f;

            return new KingdomStatisticsSnapshot(
                normalizedOwner,
                activeSettlements,
                totalPopulation,
                totalLastArrivals,
                totalLastDeaths,
                sumAvgArrivals / divider,
                sumAvgBirthRate / divider,
                sumAvgMortality / divider,
                avgMood,
                historyTurnsMin == int.MaxValue ? 0 : historyTurnsMin);
        }

        private void OnEconomyTickCompleted(EconomyTickCompletedSignal signal)
        {
            if (string.IsNullOrWhiteSpace(signal.SettlementId) || _economyManager == null)
                return;

            var state = _economyManager.GetSettlement(signal.SettlementId);
            if (state == null)
                return;

            float mood = ComputeAverageMood(state);
            int population = state.Residents != null ? state.Residents.Count : 0;

            if (!_statsBySettlement.TryGetValue(signal.SettlementId, out var rolling))
                rolling = new RollingSettlementStats(RollingWindowTurns);

            rolling.Push(signal.Arrivals, signal.Deaths, population, mood);
            _statsBySettlement[signal.SettlementId] = rolling;
        }

        private static float ComputeAverageMood(EconomySettlementState state)
        {
            if (state == null || state.Residents == null || state.Residents.Count == 0)
                return 0f;

            float sum = 0f;
            for (int i = 0; i < state.Residents.Count; i++)
                sum += state.Residents[i].Comfort;

            return sum / state.Residents.Count;
        }

        private struct RollingSettlementStats
        {
            private readonly int _capacity;
            private readonly Queue<int> _arrivals;
            private readonly Queue<int> _deaths;
            private readonly Queue<float> _birthRates;
            private readonly Queue<float> _mortalityRates;
            private readonly Queue<float> _moods;

            public int LastArrivals { get; private set; }
            public int LastDeaths { get; private set; }
            public int SampleCount => _arrivals.Count;

            public RollingSettlementStats(int capacity)
            {
                _capacity = Math.Max(1, capacity);
                _arrivals = new Queue<int>(_capacity);
                _deaths = new Queue<int>(_capacity);
                _birthRates = new Queue<float>(_capacity);
                _mortalityRates = new Queue<float>(_capacity);
                _moods = new Queue<float>(_capacity);
                LastArrivals = 0;
                LastDeaths = 0;
            }

            public void Push(int arrivals, int deaths, int population, float mood)
            {
                LastArrivals = arrivals;
                LastDeaths = deaths;

                float safePopulation = Math.Max(1, population);
                float birthRate = arrivals / safePopulation;
                float mortalityRate = deaths / safePopulation;

                EnqueueWithCap(_arrivals, arrivals);
                EnqueueWithCap(_deaths, deaths);
                EnqueueWithCap(_birthRates, birthRate);
                EnqueueWithCap(_mortalityRates, mortalityRate);
                EnqueueWithCap(_moods, mood);
            }

            public float GetAverageArrivals() => Average(_arrivals);
            public float GetAverageBirthRate() => Average(_birthRates);
            public float GetAverageMortalityRate() => Average(_mortalityRates);
            public float GetAverageMood() => Average(_moods);

            private void EnqueueWithCap<T>(Queue<T> queue, T value)
            {
                queue.Enqueue(value);
                while (queue.Count > _capacity)
                    queue.Dequeue();
            }

            private static float Average(IEnumerable<int> values)
            {
                int count = 0;
                float sum = 0f;
                foreach (int value in values)
                {
                    count++;
                    sum += value;
                }

                return count == 0 ? 0f : sum / count;
            }

            private static float Average(IEnumerable<float> values)
            {
                int count = 0;
                float sum = 0f;
                foreach (float value in values)
                {
                    count++;
                    sum += value;
                }

                return count == 0 ? 0f : sum / count;
            }
        }
    }
}
