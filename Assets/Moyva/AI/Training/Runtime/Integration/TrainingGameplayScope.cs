using System;
using Kruty1918.Moyva.AI.Bot;
using Kruty1918.Moyva.Turns.API;

namespace Kruty1918.Moyva.AI.Training
{
    // Stable training handle, fresh owned gameplay composition for every episode.
    public sealed class TrainingGameplayScope : IGameplayTrainingReset, ITrainingEpisodeOutcomeSource, IDisposable
    {
        private readonly TrainingConfig _config;
        private GameplayTrainingEpisode _episode;

        // A procedurally generated world may be valid in general but unsuitable
        // for a two-player training episode (too few legal spawns or the learner
        // spawn being disconnected from every legal opponent spawn).
        //
        // Retry only these generation failures. All other exceptions still fail
        // the training run so real gameplay/runtime bugs are never hidden.
        private const int MaxGenerationAttempts = 8;

        public const string LearnerId = "training_player";
        public const string OpponentId = "training_opponent";
        public string PlayerId => LearnerId;
        internal GameplayTrainingEpisode Episode => _episode;
        public ITurnService Turns => _episode?.Turns;
        public IBotTurnGateway TurnGateway => _episode?.Gateway;
        public BotCapabilityRegistry Capabilities => _episode?.Capabilities;
        public IBotPerceptionSource Perception => _episode?.Perception;
        public IGameplayTrainingReset Reset => this;
        public ITrainingEpisodeOutcomeSource Outcomes => this;
        public bool IsConnected => _episode?.Outcomes.IsConnected == true;
        public string Limitation => IsConnected ? null : "Episode has not been initialized.";
        public event Action<TrainingEpisodeResult> Completed;
        public event Action<TrainingRewardEvent> Reward;
        public TrainingGameplayScope(TrainingConfig config) { _config = config; }
        bool IGameplayTrainingReset.Reset(TrainingResetContext context)
        {
            DisposeEpisode();

            for (int attempt = 0; attempt < MaxGenerationAttempts; attempt++)
            {
                var attemptContext = CreateAttemptContext(context, attempt);

                try
                {
                    _episode = new GameplayTrainingEpisode(_config, attemptContext);
                    _episode.Outcomes.Completed += ForwardOutcome;
                    _episode.Outcomes.Reward += ForwardReward;
                    return Turns.CanOwnerAct(PlayerId, out _);
                }
                catch (InvalidOperationException exception)
                    when (IsRetryableGenerationFailure(exception)
                          && attempt + 1 < MaxGenerationAttempts)
                {
                    DisposeEpisode();

                    UnityEngine.Debug.LogWarning(
                        $"MOYVA_TRAINING_WORLD_RETRY " +
                        $"episode={context.EpisodeId} " +
                        $"attempt={attempt + 1}/{MaxGenerationAttempts} " +
                        $"seed={attemptContext.Seed} " +
                        $"reason={exception.Message}");
                }
                catch
                {
                    DisposeEpisode();
                    throw;
                }
            }

            throw new InvalidOperationException(
                "Training world generation retry budget exhausted.");
        }

        private static TrainingResetContext CreateAttemptContext(
            TrainingResetContext context,
            int attempt)
        {
            if (attempt == 0)
                return context;

            int retrySeed = TrainingResetContext.DeriveSeed(
                context.Seed,
                context.EnvironmentId,
                unchecked(context.EpisodeId + attempt));

            return new TrainingResetContext(
                context.EnvironmentId,
                context.EpisodeId,
                retrySeed,
                context.CurriculumStage,
                context.WorldSize,
                context.ScenarioId,
                context.LearnInitialCastle,
                context.Scenario);
        }

        private static bool IsRetryableGenerationFailure(
            InvalidOperationException exception)
        {
            string message = exception?.Message ?? string.Empty;

            return message.StartsWith(
                       "Generated world has fewer than two legal unit spawns.",
                       StringComparison.Ordinal)
                   || string.Equals(
                       message,
                       "Generated world has no connected legal opponent spawn.",
                       StringComparison.Ordinal)
                   || message.StartsWith(
                       TrainingScenarioScaffolder.PlacementFailurePrefix,
                       StringComparison.Ordinal);
        }

        public void Tick(float seconds) => _episode?.Tick(seconds);
        private void ForwardOutcome(TrainingEpisodeResult result) => Completed?.Invoke(result);
        private void ForwardReward(TrainingRewardEvent reward) => Reward?.Invoke(reward);
        private void DisposeEpisode()
        {
            if (_episode == null) return;
            _episode.Outcomes.Completed -= ForwardOutcome;
            _episode.Outcomes.Reward -= ForwardReward;
            _episode.Dispose();
            _episode = null;
        }
        public void Dispose() => DisposeEpisode();
    }
}
