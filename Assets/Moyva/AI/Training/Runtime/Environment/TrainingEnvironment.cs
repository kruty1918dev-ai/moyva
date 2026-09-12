using System;
using Kruty1918.Moyva.AI.Bot;

namespace Kruty1918.Moyva.AI.Training
{
    public sealed class TrainingEnvironment : ITrainingEnvironment, IDisposable
    {
        private readonly TrainingConfig _config;
        private readonly ITrainingSimulation _simulation;
        private readonly System.Diagnostics.Stopwatch _timer = new System.Diagnostics.Stopwatch();
        private readonly int _seedBase;
        private bool _pendingReset;
        private TrainingEpisodeResult _pendingOutcome;
        private readonly ITrainingEpisodeOutcomeSource _outcomes;
        public bool IsRealGameplay => _simulation is GameplayTrainingSimulation;
        public double ElapsedSeconds => _timer.Elapsed.TotalSeconds;
        public BotCandidateAction LastCandidate { get; private set; }
        public int[] ActionCounts { get; } = new int[12];
        public TrainingReadinessReport Readiness { get; private set; } = new TrainingReadinessReport();
        private ITrainingSimulationFactory _factory;
        public event Action<TrainingEpisodeResult> EpisodeEnded;
        public int EnvironmentId { get; }
        public long EpisodeId { get; private set; }
        public bool IsReady { get; private set; }
        public bool CanRequestDecision => IsReady && Bridge.CanRequestDecision;
        public TrainingEpisodeResult Result { get; private set; }
        public TrainingCurriculumStage Stage => _config.curriculum.stage;
        public string Limitation => _simulation.Limitation;
        public TrainingAction? PreviousAction { get; private set; }
        public TrainingRewardTracker Rewards { get; }
        public TrainingDiagnostics Diagnostics { get; } = new TrainingDiagnostics();
        public TrainingActionMaskProvider Actions { get; }
        public ITrainingObservationProvider Observations { get; }
        public TrainingBotBridge Bridge { get; }

        public TrainingEnvironment(int environmentId, TrainingConfig config, ITrainingSimulation simulation)
        {
            if (environmentId < 0) throw new ArgumentOutOfRangeException(nameof(environmentId));
            EnvironmentId = environmentId;
            _config = config.Snapshot();
            _simulation = simulation ?? throw new ArgumentNullException(nameof(simulation));
            _seedBase = _config.deterministicMode ? _config.baseSeed : Guid.NewGuid().GetHashCode();
            Rewards = new TrainingRewardTracker(_config.rewards, simulation.PlayerId);
            Rewards.EpisodeCompleted += EndEpisode;
            Bridge = new TrainingBotBridge(simulation, environmentId,
                _config.presentationMode == TrainingPresentationMode.HeadlessFast ? 4 : 128);
            Actions = new TrainingActionMaskProvider(Bridge);
            Observations = new TrainingObservationProvider(Bridge);
            _outcomes = (simulation as GameplayTrainingSimulation)?.Outcomes;
            if (_outcomes != null) _outcomes.Completed += OnOutcome;
        }

        public TrainingReadinessReport CheckReadiness(ITrainingSimulationFactory factory)
        {
            _factory = factory;
            return Readiness = TrainingReadinessValidator.Validate(factory, _simulation, this);
        }

        private void OnOutcome(TrainingEpisodeResult result)
        {
            if (!IsReady) return;
            // The signal can fire inside EndTurn. Let its decision trace finish before
            // completing the episode, so metrics include the actual terminal action.
            _pendingOutcome = result;
        }

        private void CompletePendingOutcome()
        {
            var result = _pendingOutcome;
            _pendingOutcome = TrainingEpisodeResult.None;
            if (!IsReady || result == TrainingEpisodeResult.None) return;
            if (result == TrainingEpisodeResult.InvalidState) { Fail("Match cancelled or terminal result is ambiguous."); return; }
            var type = result == TrainingEpisodeResult.Victory ? TrainingRewardEventType.EpisodeWon
                : result == TrainingEpisodeResult.Defeat ? TrainingRewardEventType.EpisodeLost : TrainingRewardEventType.EpisodeDraw;
            Record(type, "game-result");
        }

        public void ResetEnvironment()
        {
            if (_pendingReset) return;
            IsReady = false;
            Bridge.Orchestrator?.Dispose();
            EpisodeId++;
            Result = TrainingEpisodeResult.None;
            PreviousAction = null;
            _pendingOutcome = TrainingEpisodeResult.None;
            LastCandidate = null;
            Array.Clear(ActionCounts, 0, ActionCounts.Length);
            var context = new TrainingResetContext(EnvironmentId, EpisodeId,
                TrainingResetContext.DeriveSeed(_seedBase, EnvironmentId, EpisodeId), Stage);
            Rewards.Reset(EpisodeId);
            Diagnostics.Reset(context);
            _timer.Reset();
            _pendingReset = true;
            try
            {
                if (!_config.allowScaffoldSimulation)
                {
                    var source = _simulation as ITrainingBotRuntimeSource;
                    if (!IsRealGameplay || source?.Perception == null || source.Perception is EmptyBotPerceptionSource)
                        throw new InvalidOperationException("REAL_SIMULATION/PERCEPTION_BLOCKED: scaffold and fallback perception are forbidden.");
                }
                if (!_simulation.Reset(context) || !_simulation.IsReady)
                { Fail("Simulation reset adapter did not produce a ready environment."); return; }
                Bridge.Reset((int)Stage);
                Bridge.Orchestrator.DecisionFinished += OnDecisionFinished;
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
            Bridge.Tick(0);
            if (_factory != null && !_config.allowScaffoldSimulation && !CheckReadiness(_factory).IsReady)
                Fail(Readiness.ToString());
        }

        public void Tick(float seconds)
        {
            CompletePendingOutcome();
            if (!IsReady) return;
            Bridge.Tick(seconds);
            CompletePendingOutcome();
            if (Bridge.Orchestrator.Session?.State == BotOrchestratorState.Cancelled)
                Fail(Bridge.Telemetry.LastError ?? "Bot session cancelled.");
        }

        public bool Step(int actionIndex)
        {
            if (!CanRequestDecision) return false;
            Diagnostics.Decisions++;
            Record(TrainingRewardEventType.Decision, "decision");
            return Bridge.Submit(actionIndex);
        }

        private void OnDecisionFinished(BotDecisionTrace trace)
        {
            if (!IsReady) return;
            var candidate = Bridge.Frame?.Candidates[trace.Slot];
            LastCandidate = candidate;
            if (candidate != null) PreviousAction = TrainingActionMaskProvider.FromCandidate(candidate);
            if (trace.Failure == BotDecisionFailure.ModelInvalid)
            {
                Diagnostics.InvalidActions++;
                Record(TrainingRewardEventType.InvalidAction, "invalid");
            }
            else if (trace.Failure == BotDecisionFailure.StaleState) Diagnostics.StaleActions++;
            else if (trace.Result == BotExecutionStatus.Completed)
            {
                ActionCounts[(int)trace.Intent]++;
                Diagnostics.ValidActions++;
                Record(TrainingRewardEventType.ValidAction, "valid");
                if (trace.Intent == BotIntentType.EndTurn)
                {
                    Diagnostics.Turns++;
                    Record(TrainingRewardEventType.TurnCompleted, "turn");
                }
            }
            Diagnostics.LastError = trace.Reason;
            UpdateDiagnostics();
            if (_pendingOutcome != TrainingEpisodeResult.None)
            {
                CompletePendingOutcome();
                return;
            }
            if (Diagnostics.InvalidActions >= _config.rewards.invalidActionLimit)
                Fail("Invalid action limit reached.");
            else if (trace.Intent == BotIntentType.Wait || Diagnostics.Decisions >= _config.maxDecisionsPerEpisode
                || Diagnostics.Turns >= _config.maxTurnsPerEpisode)
                EndEpisode(TrainingEpisodeResult.Timeout);
        }

        public void EndEpisode(TrainingEpisodeResult result)
        {
            if (Result != TrainingEpisodeResult.None || result == TrainingEpisodeResult.None) return;
            IsReady = false;
            Result = result;
            _timer.Stop();
            Bridge.Orchestrator?.Cancel();
            Rewards.Complete(result);
            Diagnostics.EpisodeResult = result;
            UpdateDiagnostics();
            Bridge.Telemetry.Metrics.RecordEpisode(new BotEpisodeMetrics(EpisodeId, Rewards.TotalReward, Rewards.ShapingReward,
                result == TrainingEpisodeResult.Victory, result == TrainingEpisodeResult.Defeat,
                result == TrainingEpisodeResult.Draw, result == TrainingEpisodeResult.Timeout, Diagnostics.Decisions, Diagnostics.Turns));
            EpisodeEnded?.Invoke(result);
        }

        private void Fail(string reason)
        {
            Readiness.Block(reason);
            Diagnostics.LastError = reason;
            EndEpisode(TrainingEpisodeResult.InvalidState);
        }
        private void Record(TrainingRewardEventType type, string prefix)
            => Rewards.Record(new TrainingRewardEvent(EpisodeId, prefix + ":" + Diagnostics.Decisions, type, validated: true));
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
            Bridge.Orchestrator?.Dispose();
            Rewards.EpisodeCompleted -= EndEpisode;
            if (_outcomes != null) _outcomes.Completed -= OnOutcome;
            _simulation.Dispose();
        }
    }
}
