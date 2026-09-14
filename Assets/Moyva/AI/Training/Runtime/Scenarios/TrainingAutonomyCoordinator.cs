using System;
using System.Collections.Generic;

namespace Kruty1918.Moyva.AI.Training
{
    public sealed class TrainingAutonomyCoordinator : IDisposable
    {
        private readonly TrainingCurriculumController _controller;
        private readonly List<TrainingEnvironment> _environments = new List<TrainingEnvironment>();
        private readonly Dictionary<TrainingEnvironment, Action<TrainingEpisodeResult>> _handlers =
            new Dictionary<TrainingEnvironment, Action<TrainingEpisodeResult>>();
        public TrainingCurriculumController Controller => _controller;

        public TrainingAutonomyCoordinator(TrainingConfig config)
        {
            if (config == null || config.curriculum == null || config.curriculum.autonomous == null)
                throw new ArgumentNullException(nameof(config));
            _controller = new TrainingCurriculumController(config.curriculum.autonomous, config.baseSeed);
        }

        public void Attach(TrainingEnvironment environment)
        {
            if (environment == null || _environments.Contains(environment)) return;
            _environments.Add(environment);
            environment.SetScenario(_controller.ChooseNext());
            Action<TrainingEpisodeResult> handler = result => OnEpisodeEnded(environment, result);
            _handlers.Add(environment, handler);
            environment.EpisodeEnded += handler;
        }

        private void OnEpisodeEnded(TrainingEnvironment environment, TrainingEpisodeResult result)
        {
            bool success = result == TrainingEpisodeResult.ScenarioSuccess
                || (environment.Scenario != null && environment.Scenario.fullGame && result == TrainingEpisodeResult.Victory);
            if (result != TrainingEpisodeResult.InvalidState && environment.Scenario != null)
                _controller.RecordTrainingEpisode(environment.Scenario.id, success, environment.Diagnostics.Decisions);

            // Frozen held-out evaluation remains a separate process. We persist the due flag here;
            // training episodes never masquerade as evaluation and never advance mastery by themselves.
            environment.SetScenario(_controller.ChooseNext());
        }

        public void Dispose()
        {
            foreach (var pair in _handlers) pair.Key.EpisodeEnded -= pair.Value;
            _handlers.Clear();
            _environments.Clear();
            _controller.Dispose();
        }
    }
}
