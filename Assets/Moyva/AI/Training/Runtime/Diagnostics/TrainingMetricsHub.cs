using System;
using System.Collections.Generic;
using System.Diagnostics;
using Kruty1918.Moyva.AI.Bot;

namespace Kruty1918.Moyva.AI.Training
{
    public interface ITrainingMetricsSource
    {
        TrainingMetricsHistory History { get; }
        IReadOnlyList<TrainingEnvironment> Environments { get; }
        double DecisionsPerSecond { get; }
        double EpisodesPerMinute { get; }
        double AverageEpisodeDuration { get; }
    }
    public sealed class TrainingMetricsHub : ITrainingMetricsSource, IDisposable
    {
        private readonly Stopwatch _clock = Stopwatch.StartNew();
        private readonly List<TrainingEnvironment> _environments = new List<TrainingEnvironment>();
        private readonly List<Action<TrainingEpisodeResult>> _handlers = new List<Action<TrainingEpisodeResult>>();
        private long _completedDecisions, _completedEpisodes;
        private double _totalDuration;
        public TrainingMetricsHistory History { get; }
        public IReadOnlyList<TrainingEnvironment> Environments => _environments.AsReadOnly();
        public double DecisionsPerSecond
        {
            get
            {
                long decisions = _completedDecisions;
                foreach (var environment in _environments)
                    if (environment.Result == TrainingEpisodeResult.None) decisions += environment.Diagnostics.Decisions;
                return decisions / Math.Max(0.001, _clock.Elapsed.TotalSeconds);
            }
        }
        public double EpisodesPerMinute => 60 * _completedEpisodes / Math.Max(0.001, _clock.Elapsed.TotalSeconds);
        public double AverageEpisodeDuration => _totalDuration / Math.Max(1, _completedEpisodes);
        public TrainingMetricsHub(int capacity) { History = new TrainingMetricsHistory(capacity); }
        public void Attach(TrainingEnvironment environment)
        {
            Action<TrainingEpisodeResult> handler = _ => Record(environment);
            _environments.Add(environment); _handlers.Add(handler);
            environment.EpisodeEnded += handler;
        }
        private void Record(TrainingEnvironment environment)
        {
            var episode = new TrainingEpisodeSummary(environment, _clock.Elapsed.TotalSeconds);
            History.Record(episode); _completedEpisodes++; _completedDecisions += episode.Decisions; _totalDuration += episode.Duration;
        }
        // Thin conversion reuses Bot Core's health heuristic over the selected history.
        public static BotRunMetrics HealthMetrics(TrainingEpisodeSummary[] episodes)
        {
            var metrics = new BotRunMetrics();
            for (int index = Math.Max(0, episodes.Length - 50); index < episodes.Length; index++)
            {
                var e = episodes[index];
                metrics.Decisions += e.Decisions; metrics.Invalid += e.Invalid; metrics.Stale += e.Stale;
                for (int i = 0; i < metrics.IntentCounts.Length; i++) metrics.IntentCounts[i] += e.ActionCounts[i];
                metrics.RecordEpisode(new BotEpisodeMetrics(e.Episode, e.Reward, e.Shaping,
                    e.Result == TrainingEpisodeResult.Victory, e.Result == TrainingEpisodeResult.Defeat,
                    e.Result == TrainingEpisodeResult.Draw, e.Result == TrainingEpisodeResult.Timeout, e.Decisions, e.Turns));
            }
            return metrics;
        }
        public void Dispose()
        {
            for (int i = 0; i < _environments.Count; i++) _environments[i].EpisodeEnded -= _handlers[i];
            _clock.Stop();
        }
    }
}
