using System;

namespace Kruty1918.Moyva.Economy.Runtime
{
    /// <summary>
    /// Result of a single economy tick — returned by <see cref="EconomyTickOrchestrator"/>.
    /// </summary>
    [Serializable]
    public sealed class EconomyTickResult
    {
        public int Turn;
        public int Arrivals;
        public int Deaths;
        public int TotalPopulation;
        public int AvailableWorkers;
        public int AssignedWorkers;
        public int ProductionCyclesCompleted;
        public float TotalFoodConsumed;
        public float TotalWaterConsumed;
    }
}
