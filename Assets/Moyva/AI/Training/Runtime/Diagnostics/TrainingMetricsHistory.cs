using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.AI.Training
{
    public sealed class TrainingEpisodeSummary
    {
        public readonly int EnvironmentId, Seed, Turns, Decisions, Invalid, Stale;
        public readonly long Episode;
        public readonly TrainingEpisodeResult Result;
        public readonly float Reward, Shaping;
        public readonly double Duration, TrainingSeconds;
        public IReadOnlyList<int> ActionCounts { get; }
        public TrainingEpisodeSummary(TrainingEnvironment environment, double seconds)
        {
            var d = environment.Diagnostics;
            EnvironmentId = environment.EnvironmentId; Episode = environment.EpisodeId; Seed = d.LastSeed;
            Result = environment.Result; Turns = d.Turns; Decisions = d.Decisions; Invalid = d.InvalidActions; Stale = d.StaleActions;
            Reward = d.TotalReward; Shaping = d.ShapingReward; Duration = environment.ElapsedSeconds; TrainingSeconds = seconds;
            ActionCounts = Array.AsReadOnly((int[])environment.ActionCounts.Clone());
        }
    }
    public sealed class TrainingMetricsHistory
    {
        private readonly Queue<TrainingEpisodeSummary> _episodes = new Queue<TrainingEpisodeSummary>();
        public int Capacity { get; }
        public int Count => _episodes.Count;
        public TrainingMetricsHistory(int capacity = 500) { Capacity = Math.Clamp(capacity, 1, 5000); }
        public void Record(TrainingEpisodeSummary episode)
        {
            if (_episodes.Count == Capacity) _episodes.Dequeue();
            _episodes.Enqueue(episode);
        }
        public TrainingEpisodeSummary[] Snapshot() => _episodes.ToArray();
        public void Clear() => _episodes.Clear();
    }
}
