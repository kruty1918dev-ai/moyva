using System;

namespace Kruty1918.Moyva.AI.Training
{
    public sealed class TrainingEnvironment : ITrainingEnvironment, IDisposable
    {
        private readonly TrainingConfig _config;
        private readonly ITrainingSimulation _simulation;
        private readonly TrainingActionExecutor _executor;
        private readonly System.Diagnostics.Stopwatch _timer = new System.Diagnostics.Stopwatch();
        private readonly int _seedBase;
        private bool _pendingReset;
        public event Action<TrainingEpisodeResult> EpisodeEnded;
        public int EnvironmentId { get; }
        public long EpisodeId { get; private set; }
        public bool IsReady { get; private set; }
        public TrainingEpisodeResult Result { get; private set; }
        public TrainingCurriculumStage Stage => _config.curriculum.stage;
        public string Limitation => _simulation.Limitation;
        public TrainingAction? PreviousAction { get; private set; }
        public TrainingRewardTracker Rewards { get; }
        public TrainingDiagnostics Diagnostics { get; } = new TrainingDiagnostics();
        public TrainingActionMaskProvider Actions { get; }
        public ITrainingObservationProvider Observations { get; }

        public TrainingEnvironment(int environmentId, TrainingConfig config, ITrainingSimulation simulation)
        {
            if (environmentId < 0) throw new ArgumentOutOfRangeException(nameof(environmentId));
            EnvironmentId = environmentId;
            _config = config.Snapshot();
            _simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
            _seedBase = _config.deterministicMode ? _config.baseSeed : Guid.NewGuid().GetHashCode();
            Rewards = new TrainingRewardTracker(_config.rewards, simulation.PlayerId);
            Rewards.EpisodeCompleted += EndEpisode;
            Actions = new TrainingActionMaskProvider(simulation);
            _executor = new TrainingActionExecutor(simulation, Actions);
            Observations = new TrainingObservationProvider(simulation.PlayerId, simulation as ITrainingVisibleObservationSource);
        }

        public void ResetEnvironment()
        {
            if (_pendingReset) return;
            IsReady = false;
            EpisodeId++;
            Result = TrainingEpisodeResult.None;
            PreviousAction = null;
            var context = new TrainingResetContext(EnvironmentId, EpisodeId,
                TrainingResetContext.DeriveSeed(_seedBase, EnvironmentId, EpisodeId), Stage);
            Rewards.Reset(EpisodeId);
            Diagnostics.Reset(context);
            _timer.Reset();
            _pendingReset = true;
            try
            {
                if (!_simulation.Reset(context) || !_simulation.IsReady)
                    Fail("Simulation reset adapter did not produce a ready environment.");
            }
            catch (Exception exception) { Fail(exception.Message); }
        }

        public void BeginEpisode()
        {
            if (IsReady) return;
            if (!_pendingReset) ResetEnvironment();
            _pendingReset = false;
            if (Result != TrainingEpisodeResult.None) return;
            IsReady = true;
            _timer.Start();
            Record(TrainingRewardEventType.EpisodeStarted, "start");
        }

        public bool Step(int actionIndex)
        {
            if (!IsReady) return false;
            Diagnostics.Decisions++;
            Record(TrainingRewardEventType.Decision, "decision");
            bool valid;
            string reason;
            try { valid = _executor.TryExecute(actionIndex, out reason); }
            catch (Exception exception) { Fail(exception.Message); return false; }
            if (valid)
            {
                PreviousAction = Actions.Decode(actionIndex);
                Diagnostics.ValidActions++;
                Record(TrainingRewardEventType.ValidAction, "valid");
                if (PreviousAction.Value.ActionType == TrainingActionType.EndTurn)
                {
                    Diagnostics.Turns++;
                    Record(TrainingRewardEventType.TurnCompleted, "turn");
                }
            }
            else
            {
                Diagnostics.InvalidActions++;
                Diagnostics.LastError = reason;
                Record(TrainingRewardEventType.InvalidAction, "invalid");
            }
            UpdateDiagnostics();
            if (Diagnostics.InvalidActions >= _config.rewards.invalidActionLimit)
                Fail("Invalid action limit reached.");
            else if (Diagnostics.Decisions >= _config.maxDecisionsPerEpisode
                || Diagnostics.Turns >= _config.maxTurnsPerEpisode)
                EndEpisode(TrainingEpisodeResult.Timeout);
            return valid;
        }

        public void EndEpisode(TrainingEpisodeResult result)
        {
            if (Result != TrainingEpisodeResult.None || result == TrainingEpisodeResult.None) return;
            IsReady = false;
            Result = result;
            _timer.Stop();
            Rewards.Complete(result);
            Diagnostics.EpisodeResult = result;
            UpdateDiagnostics();
            EpisodeEnded?.Invoke(result);
        }

        private void Fail(string reason)
        {
            Diagnostics.LastError = reason;
            EndEpisode(TrainingEpisodeResult.InvalidState);
        }

        private void Record(TrainingRewardEventType type, string prefix)
            => Rewards.Record(new TrainingRewardEvent(EpisodeId, prefix + ":" + Diagnostics.Decisions,
                type, validated: true));

        private void UpdateDiagnostics()
        {
            Diagnostics.TotalReward = Rewards.TotalReward;
            Diagnostics.ShapingReward = Rewards.ShapingReward;
            Diagnostics.ElapsedSeconds = _timer.Elapsed.TotalSeconds;
        }

        public void Dispose()
        {
            IsReady = false;
            _timer.Stop();
            Rewards.EpisodeCompleted -= EndEpisode;
            _simulation.Dispose();
        }
    }
}
