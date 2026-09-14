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
        private TrainingScenarioDefinition _scenario;
        private TrainingScenarioProgressTracker _scenarioProgress;
        private TrainingDecisionJournal _decisionJournal;
        private string _observerSessionId;
        private float _lastJournalReward;
        private int _scenarioGameplayRewardEventsThisTurn;
        private readonly ITrainingEpisodeOutcomeSource _outcomes;
        public bool IsRealGameplay => _simulation is GameplayTrainingSimulation;
        internal GameplayTrainingEpisode GameplayEpisode => (_simulation as GameplayTrainingSimulation)?.Episode;
        public double ElapsedSeconds => _timer.Elapsed.TotalSeconds;
        public BotCandidateAction LastCandidate { get; private set; }
        public int[] ActionCounts { get; } = new int[12];
        public TrainingReadinessReport Readiness { get; private set; } = new TrainingReadinessReport();
        private ITrainingSimulationFactory _factory;
        public event Action<TrainingEpisodeResult> EpisodeEnded;
        public event Action<int, long> EpisodeReset;
        public int EnvironmentId { get; }
        public long EpisodeId { get; private set; }
        public bool IsReady { get; private set; }
        public bool CanRequestDecision => IsReady && Bridge.CanRequestDecision;
        public TrainingEpisodeResult Result { get; private set; }
        public TrainingScenarioDefinition Scenario => _scenario;
        public TrainingScenarioProgressTracker ScenarioProgress => _scenarioProgress;
        public TrainingCurriculumStage Stage => _scenario?.legacyStage ?? _config.curriculum.stage;
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
            if (simulation is GameplayTrainingSimulation gameplay) gameplay.GameplayReward += OnGameplayReward;
        }

        public void SetScenario(TrainingScenarioDefinition scenario)
        {
            if (IsReady) throw new InvalidOperationException("Cannot change training scenario while an episode is running.");
            _scenario = scenario;
            _scenarioProgress = scenario == null ? null : new TrainingScenarioProgressTracker(scenario);
        }

        public void SetDecisionJournal(TrainingDecisionJournal journal)
        {
            if (IsReady) throw new InvalidOperationException("Cannot change decision journal while an episode is running.");
            _decisionJournal = journal;
        }

        public void SetObserverSessionId(string sessionId)
        {
            if (IsReady) throw new InvalidOperationException("Cannot change observer session while an episode is running.");
            _observerSessionId = sessionId;
        }

        internal ArenaSnapshot CaptureObserverSnapshot(string sessionId, long sequence)
        {
            return new ArenaSnapshot
            {
                sessionId = sessionId ?? _observerSessionId ?? string.Empty,
                arenaId = EnvironmentId,
                episodeId = EpisodeId,
                sequence = sequence,
                width = _config.worldSize,
                height = _config.worldSize,
                scenarioId = _scenario?.id,
                scenarioStep = _scenarioProgress?.StepIndex ?? -1,
                isComplete = false,
                status = IsRealGameplay
                    ? "partial: training source exposes arena metadata, but generator-owned visual layers are not exported by this patch"
                    : "partial: scaffold/non-gameplay training source has no authoritative visual map layers"
            };
        }

        private TrainingScenarioFacts CaptureScenarioFacts()
            => GameplayEpisode?.CaptureTrainingFacts() ?? default;

        public TrainingReadinessReport CheckReadiness(ITrainingSimulationFactory factory)
        {
            _factory = factory;
            return Readiness = TrainingReadinessValidator.Validate(factory, _simulation, this);
        }

        private void OnOutcome(TrainingEpisodeResult result)
        {
            if (!IsReady) return;
            _scenarioProgress?.ObserveMatch(result);
            _pendingOutcome = result;
        }
        private void OnGameplayReward(TrainingRewardEvent reward)
        {
            if (!IsReady) return;
            var facts = CaptureScenarioFacts();
            if (facts?.IsSetup == true) return;

            var rules = _scenario?.rewardRules;
            if (rules != null)
            {
                if (!rules.Allows(reward)) return;
                if (rules.maxGameplayRewardEventsPerTurn > 0
                    && _scenarioGameplayRewardEventsThisTurn >= rules.maxGameplayRewardEventsPerTurn)
                    return;
            }

            _scenarioGameplayRewardEventsThisTurn++;
            Rewards.Record(reward);
            _scenarioProgress?.ObserveReward(reward, facts);
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
                TrainingResetContext.DeriveSeed(_seedBase, EnvironmentId, EpisodeId), Stage,
                _scenario?.id,
                _scenario?.startingConditions?.learnerMustPlaceCastle
                    ?? _scenario?.learnerBuildsInitialCastle
                    ?? _config.learnInitialCastle);
            Rewards.Reset(EpisodeId);
            _lastJournalReward = 0;
            _scenarioGameplayRewardEventsThisTurn = 0;
            Diagnostics.Reset(context);
            _timer.Reset();
            _pendingReset = true;
            try
            {
                if (!_simulation.Reset(context) || !_simulation.IsReady)
                { Fail("Simulation reset adapter did not produce a ready environment."); return; }
                if (!_config.allowScaffoldSimulation)
                {
                    var source = _simulation as ITrainingBotRuntimeSource;
                    if (!IsRealGameplay || source?.Perception == null || source.Perception is EmptyBotPerceptionSource)
                        throw new InvalidOperationException("REAL_SIMULATION/PERCEPTION_BLOCKED: scaffold and fallback perception are forbidden.");
                }
                _scenarioProgress?.Begin(EpisodeId, CaptureScenarioFacts());
                Bridge.Reset((int)Stage, () => _scenarioProgress);
                Bridge.Orchestrator.DecisionFinished += OnDecisionFinished;
                EpisodeReset?.Invoke(EnvironmentId, EpisodeId);
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
            (_simulation as GameplayTrainingSimulation)?.Tick(seconds);
            Bridge.Tick(seconds);
            CompletePendingOutcome();
            if (IsReady && Bridge.Orchestrator.Session?.State == BotOrchestratorState.Cancelled
                && !string.IsNullOrEmpty(Bridge.Telemetry.LastError))
                Fail(Bridge.Telemetry.LastError ?? "Bot session cancelled.");
        }

        public bool Step(int actionIndex)
        {
            if (!CanRequestDecision) return false;
            Diagnostics.Decisions++;
            Record(TrainingRewardEventType.Decision, "decision");
            if (!Actions.IsLegal(actionIndex))
            {
                Diagnostics.InvalidActions++;
                Record(TrainingRewardEventType.InvalidAction, "invalid");
                Diagnostics.LastError = "Selected slot was masked.";
                UpdateDiagnostics();
                if (Diagnostics.InvalidActions >= _config.rewards.invalidActionLimit)
                    Fail("Invalid action limit reached.");
                return false;
            }

            var scenarioCandidate = Bridge.Frame?.Candidates != null
                && actionIndex >= 0 && actionIndex < Bridge.Frame.Candidates.Count
                    ? Bridge.Frame.Candidates[actionIndex] : null;
            if (_scenario != null && scenarioCandidate != null && !_scenario.AllowsIntent(scenarioCandidate.Intent))
            {
                Diagnostics.InvalidActions++;
                Record(TrainingRewardEventType.InvalidAction, "scenario-capability");
                Diagnostics.LastError = "Action is outside this scenario's availableCapabilities.";
                UpdateDiagnostics();
                if (Diagnostics.InvalidActions >= _config.rewards.invalidActionLimit)
                    Fail("Invalid action limit reached.");
                return false;
            }
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
                    _scenarioGameplayRewardEventsThisTurn = 0;
                }
            }
            _scenarioProgress?.ObserveAction(trace.Intent, trace.Result, CaptureScenarioFacts());
            Diagnostics.LastError = trace.Reason;
            UpdateDiagnostics();
            AppendDecisionEvent(trace);
            if (_scenario != null && !_scenario.fullGame && _scenarioProgress?.IsComplete == true)
            {
                EndEpisode(TrainingEpisodeResult.ScenarioSuccess);
                return;
            }
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
        private void AppendDecisionEvent(BotDecisionTrace trace)
        {
            if (_decisionJournal == null) return;
            var frame = Bridge.Frame;
            var candidate = frame?.Candidates[trace.Slot];
            var candidates = frame?.Candidates;
            string[] available = Array.Empty<string>();
            if (candidates != null)
            {
                available = new string[candidates.Count];
                for (int i = 0; i < candidates.Count; i++)
                    available[i] = candidates[i]?.Id ?? string.Empty;
            }
            float rewardDelta = Rewards.TotalReward - _lastJournalReward;
            _lastJournalReward = Rewards.TotalReward;
            _decisionJournal.Append(new AgentDecisionEvent
            {
                sessionId = string.IsNullOrEmpty(_observerSessionId) ? trace.SessionId : _observerSessionId,
                arenaId = EnvironmentId,
                episodeId = EpisodeId,
                agentId = trace.Player,
                scenarioId = _scenario?.id,
                scenarioStep = _scenarioProgress?.StepIndex ?? -1,
                availableActions = available,
                actionId = candidate?.Id,
                targetId = candidate?.TargetKey,
                result = trace.Result.ToString(),
                rejectionReason = trace.Failure == BotDecisionFailure.None ? trace.Reason : trace.Failure + ": " + trace.Reason,
                rewardDelta = rewardDelta
            });
        }
        public void Dispose()
        {
            IsReady = false;
            _timer.Stop();
            Bridge.Orchestrator?.Dispose();
            Rewards.EpisodeCompleted -= EndEpisode;
            if (_outcomes != null) _outcomes.Completed -= OnOutcome;
            if (_simulation is GameplayTrainingSimulation gameplay) gameplay.GameplayReward -= OnGameplayReward;
            _simulation.Dispose();
        }
    }
}
