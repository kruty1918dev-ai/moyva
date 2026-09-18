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
        private int _stagnantDecisions;
        private TrainingScenarioFacts _lastDevelopmentFacts;
        // Set immediately before an engine-forced single-candidate submission;
        // consumed by the synchronous DecisionFinished callback.
        private bool _pendingForcedSubmission;
        private bool _suppressScenarioHints;
        // Watchdog window state (submissions = forced + trainable).
        private int _wdSubmissions;
        private int _wdForced;
        private float _wdProgressMark;
        private int _wdStagnant;
        // Candidate diversity/intent accumulation is tracked per scenario step so
        // multi-step scenarios are not aborted for intents that only become legal
        // in a later step.
        private sealed class StepWatchdogWindow
        {
            public int Submissions;
            public long CandidateSum;
            public long IntentMask;
        }
        private readonly System.Collections.Generic.Dictionary<int, StepWatchdogWindow> _wdSteps
            = new System.Collections.Generic.Dictionary<int, StepWatchdogWindow>();
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
        // True only when the pending frame holds >=2 real legal candidates — an actual
        // policy decision. A single real candidate is engine-forced and must never
        // reach Agent.RequestDecision; zero real candidates is a dead end, not a step.
        public bool HasTrainableDecision => CanRequestDecision && (Bridge.Frame?.RealCandidateCount ?? 0) >= 2;
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
            var generated = GameplayEpisode?.GeneratedWorld;
            return new ArenaSnapshot
            {
                sessionId = sessionId ?? _observerSessionId ?? string.Empty,
                arenaId = EnvironmentId,
                episodeId = EpisodeId,
                sequence = sequence,
                width = generated?.Width ?? _config.worldSize,
                height = generated?.Height ?? _config.worldSize,
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

        private int ResolveEpisodeWorldSize(int seed)
        {
            if (!_config.randomizeWorldSize) return _config.worldSize;
            int min = Math.Max(12, _config.minWorldSize);
            int max = Math.Min(128, _config.maxWorldSize);
            if (max < min) return _config.worldSize;
            unchecked
            {
                int mixed = seed ^ (EnvironmentId * 73856093) ^ ((int)EpisodeId * 19349663);
                var random = new Random(mixed);
                return random.Next(min, max + 1);
            }
        }

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

        public void ResetEnvironment() => ResetEnvironmentCore(true, null);

        internal void ResetForInspectorReplay(int seed)
        {
            if (!_config.inspectorMode)
                throw new InvalidOperationException("Inspector replay reset is available only in Model Inspector mode.");
            _pendingReset = false;
            ResetEnvironmentCore(false, seed);
        }

        private void ResetEnvironmentCore(bool advanceEpisode, int? seedOverride)
        {
            if (_pendingReset) return;
            IsReady = false;
            Bridge.Orchestrator?.Dispose();
            if (advanceEpisode || EpisodeId == 0) EpisodeId++;
            Result = TrainingEpisodeResult.None;
            PreviousAction = null;
            _pendingOutcome = TrainingEpisodeResult.None;
            LastCandidate = null;
            Array.Clear(ActionCounts, 0, ActionCounts.Length);
            int resolvedSeed = seedOverride
                ?? TrainingResetContext.DeriveSeed(_seedBase, EnvironmentId, EpisodeId);
            int resolvedWorldSize = ResolveEpisodeWorldSize(resolvedSeed);
            var context = new TrainingResetContext(EnvironmentId, EpisodeId,
                resolvedSeed, Stage,
                resolvedWorldSize,
                _scenario?.id,
                _scenario?.startingConditions?.learnerMustPlaceCastle
                    ?? _scenario?.learnerBuildsInitialCastle
                    ?? _config.learnInitialCastle,
                _scenario);
            Rewards.Reset(EpisodeId);
            _lastJournalReward = 0;
            _scenarioGameplayRewardEventsThisTurn = 0;
            _stagnantDecisions = 0;
            _lastDevelopmentFacts = null;
            _pendingForcedSubmission = false;
            _wdSubmissions = _wdForced = _wdStagnant = 0;
            _wdProgressMark = -1f;
            _wdSteps.Clear();
            // Frozen evaluation never sees training-only opaque scenario hints
            // (scenario id hash / step / progress); training drops them for a
            // fraction of episodes so the policy cannot key on the scenario hash.
            _suppressScenarioHints = _scenario != null && !_scenario.fullGame
                && (_config.evaluationMode
                    || (!_config.inspectorMode && _config.scenarioHintDropout > 0f
                        && new Random(resolvedSeed ^ 0x51D10F).NextDouble() < _config.scenarioHintDropout));
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
                var baselineFacts = CaptureScenarioFacts();
                _scenarioProgress?.Begin(EpisodeId, baselineFacts);
                _lastDevelopmentFacts = baselineFacts;
                // Scenarios gate via their capability mask, so the legacy stage
                // gate stays at FullGame whenever a scenario is active.
                int bridgeStage = _scenario == null ? (int)Stage : (int)TrainingCurriculumStage.FullGame;
                Bridge.Reset(bridgeStage, () => _scenarioProgress,
                    ScenarioCapabilityMask(), () => _suppressScenarioHints);
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
            RefreshCandidateDiagnostics();
            // Zero real legal candidates at episode start is a broken scenario, not
            // a forced "wait": fail before the frame can reach forced-action logic.
            // Scaffold simulations are exempt so env-mechanics tests keep working.
            if (!_config.allowScaffoldSimulation && Bridge.Frame?.HasRealLegalCandidates != true)
            {
                Fail("TRAINING_ABORTED reason=no_legal_candidates scenario=" + (_scenario?.id ?? "<none>")
                    + " episode=" + EpisodeId + " phase=episode-start");
                return;
            }
            if (!ValidateCurrentScenarioCandidates()) return;
            if (_factory != null && !_config.allowScaffoldSimulation && !CheckReadiness(_factory).IsReady)
                Fail(Readiness.ToString());
        }

        public void Tick(float seconds)
        {
            CompletePendingOutcome();
            if (!IsReady) return;
            (_simulation as GameplayTrainingSimulation)?.Tick(seconds);
            Bridge.Tick(seconds);
            TrySubmitForcedDecision();
            // A freshly built frame awaiting a policy decision gets its real
            // candidate diagnostics refreshed immediately, so Step charges the
            // frame actually being decided and reachability checks see it.
            if (IsReady && CanRequestDecision)
            {
                RefreshCandidateDiagnostics();
                ValidateCurrentScenarioCandidates();
            }
            CompletePendingOutcome();
            if (IsReady && Bridge.Orchestrator.Session?.State == BotOrchestratorState.Cancelled
                && !string.IsNullOrEmpty(Bridge.Telemetry.LastError))
                Fail(Bridge.Telemetry.LastError ?? "Bot session cancelled.");
        }

        // A frame with exactly one real legal candidate is not a decision: the
        // engine executes it without RequestDecision, a training step or policy
        // gradient. Zero real candidates means the scenario hit a dead end — the
        // synthetic "wait" placeholder must never be submitted as a forced action.
        private void TrySubmitForcedDecision()
        {
            if (!CanRequestDecision) return;
            var candidates = Bridge.Frame?.Candidates;
            int real = candidates?.RealCount ?? 0;
            if (real <= 0)
            {
                Fail("TRAINING_ABORTED reason=no_legal_candidates scenario=" + (_scenario?.id ?? "<none>")
                    + " episode=" + EpisodeId + " slotCount=" + (candidates?.Count ?? 0));
                return;
            }
            if (real != 1) return;
            int slot = -1;
            for (int i = 0; i < candidates.Count; i++)
                if (candidates[i] != null && !candidates[i].Synthetic) { slot = i; break; }
            if (slot < 0) return;
            _pendingForcedSubmission = true;
            Diagnostics.ForcedActions++;
            Bridge.Submit(slot);
        }

        private int ScenarioCapabilityMask()
        {
            var caps = _scenario?.availableCapabilities;
            return caps == null || caps.Length == 0 ? -1 : BotCapabilityMap.Mask(caps);
        }

        public bool Step(int actionIndex)
        {
            if (!CanRequestDecision) return false;
            Diagnostics.Decisions++;
            // Charge the real candidate count of the frame actually being decided —
            // frame diagnostics refresh on Tick/decision completion, so read the
            // pending frame directly rather than a possibly stale counter.
            Diagnostics.CandidateSum += Bridge.Frame?.RealCandidateCount ?? 0;
            Record(TrainingRewardEventType.Decision, "decision");
            if (!Actions.IsLegal(actionIndex))
            {
                Diagnostics.InvalidActions++;
                Record(TrainingRewardEventType.InvalidAction, "invalid");
                Diagnostics.LastError = "Selected slot was masked.";
                UpdateDiagnostics();
                AppendRejectedDecisionEvent(actionIndex, Diagnostics.LastError);
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
                AppendRejectedDecisionEvent(actionIndex, Diagnostics.LastError);
                if (Diagnostics.InvalidActions >= _config.rewards.invalidActionLimit)
                    Fail("Invalid action limit reached.");
                return false;
            }
            return Bridge.Submit(actionIndex);
        }

        private void OnDecisionFinished(BotDecisionTrace trace)
        {
            bool forced = _pendingForcedSubmission;
            _pendingForcedSubmission = false;
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
            var facts = CaptureScenarioFacts();
            // The step this decision was made under — captured before the action
            // can advance progress, so per-step candidate contracts attribute to
            // the frame's step, not the step the action unlocked.
            int decisionStepIndex = _scenarioProgress?.StepIndex ?? -1;
            _scenarioProgress?.ObserveAction(trace.Intent, trace.Result, facts);
            ObserveDevelopmentShaping(trace, facts);
            Diagnostics.LastError = trace.Reason;
            UpdateDiagnostics();
            AppendDecisionEvent(trace, forced);
            RunWatchdogChecks(forced, trace, decisionStepIndex);
            if (!IsReady) return;
            RefreshCandidateDiagnostics();
            if (!ValidateCurrentScenarioCandidates()) return;
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

        private void ObserveDevelopmentShaping(BotDecisionTrace trace, TrainingScenarioFacts facts)
        {
            if (facts == null || facts.IsSetup || trace.Result != BotExecutionStatus.Completed) return;
            if (_lastDevelopmentFacts == null)
            {
                _lastDevelopmentFacts = facts;
                return;
            }

            bool developed = facts.OwnedSettlements > _lastDevelopmentFacts.OwnedSettlements
                || facts.OperationalCastles > _lastDevelopmentFacts.OperationalCastles
                || facts.OwnedUnits > _lastDevelopmentFacts.OwnedUnits
                || facts.ExploredCells > _lastDevelopmentFacts.ExploredCells
                || facts.LearnerResourcePotential > _lastDevelopmentFacts.LearnerResourcePotential
                || TotalProduction(facts) > TotalProduction(_lastDevelopmentFacts)
                || TotalStock(facts) > TotalStock(_lastDevelopmentFacts) + 0.1f;

            _stagnantDecisions = developed ? 0 : _stagnantDecisions + 1;
            if (!developed && _stagnantDecisions >= 3)
                Rewards.Record(new TrainingRewardEvent(EpisodeId, "stagnant:" + Diagnostics.Decisions,
                    TrainingRewardEventType.StagnantDecision, validated: true, meaningful: true));

            if (facts.OwnedSettlements > 0
                && facts.TotalLandCells > 0
                && facts.ReachableLandCellsFromLearner < Math.Min(16, facts.TotalLandCells)
                && facts.LearnerReachableLandRatio < 0.12f)
                Rewards.Record(new TrainingRewardEvent(EpisodeId, "isolated:" + Diagnostics.Decisions,
                    TrainingRewardEventType.IsolatedSettlement, validated: true, meaningful: true));

            if (facts.OwnedSettlements > 0 && facts.LearnerResourcePotential > 0)
                Rewards.Record(new TrainingRewardEvent(EpisodeId, "resource-potential:" + facts.LearnerResourcePotential,
                    TrainingRewardEventType.ResourcePotential,
                    subjectId: "resource-potential:" + facts.LearnerResourcePotential,
                    actorOwnerId: _simulation.PlayerId,
                    validated: true,
                    meaningful: true));

            _lastDevelopmentFacts = facts;
        }

        private static float TotalStock(TrainingScenarioFacts facts)
        {
            float total = 0;
            foreach (var pair in facts.ResourceStock) total += pair.Value;
            return total;
        }

        private static float TotalProduction(TrainingScenarioFacts facts)
        {
            float total = 0;
            foreach (var pair in facts.ProductionPerTurn) total += pair.Value;
            return total;
        }

        private void UpdateDiagnostics()
        {
            Diagnostics.TotalReward = Rewards.TotalReward;
            Diagnostics.ShapingReward = Rewards.ShapingReward;
            Diagnostics.ElapsedSeconds = _timer.Elapsed.TotalSeconds;
        }

        // Candidate diagnostics always describe real legal candidates; synthetic
        // fallbacks (the "wait" placeholder) are excluded from counts and intents.
        private void RefreshCandidateDiagnostics()
        {
            Array.Clear(Diagnostics.CandidateCountsByIntent, 0, Diagnostics.CandidateCountsByIntent.Length);
            var candidates = Bridge.Frame?.Candidates;
            Diagnostics.CandidateCount = candidates?.RealCount ?? 0;
            if (candidates == null) return;
            for (int i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (candidate == null || candidate.Synthetic) continue;
                int index = (int)candidate.Intent;
                if (index >= 0 && index < Diagnostics.CandidateCountsByIntent.Length)
                    Diagnostics.CandidateCountsByIntent[index]++;
            }
        }

        private bool ValidateCurrentScenarioCandidates()
        {
            if (!IsReady || _scenarioProgress == null || _scenarioProgress.IsComplete) return true;
            var required = RequiredIntentsFor(_scenarioProgress.CurrentStep?.EffectiveCriterion);
            if (required.Length == 0) return true;
            for (int i = 0; i < required.Length; i++)
            {
                int index = (int)required[i];
                int count = index >= 0 && index < Diagnostics.CandidateCountsByIntent.Length
                    ? Diagnostics.CandidateCountsByIntent[index]
                    : 0;
                if (count > 0) return true;
            }
            string reason = "SCENARIO_UNREACHABLE: scenario=" + (_scenario?.id ?? "<none>")
                + " step=" + (_scenarioProgress.CurrentStep?.id ?? "<none>")
                + " requiredIntent=" + string.Join("|", required)
                + " candidateCount=" + Diagnostics.CandidateCount
                + " counts=" + FormatCandidateCounts()
                + " unavailable=" + FormatUnavailable();
            Fail(reason);
            return false;
        }

        private static BotIntentType[] RequiredIntentsFor(TrainingScenarioCriterionKind? criterion)
            => TrainingScenarioContract.RequiredIntentsFor(criterion);

        private string FormatCandidateCounts()
        {
            var parts = new System.Collections.Generic.List<string>();
            for (int i = 0; i < Diagnostics.CandidateCountsByIntent.Length; i++)
            {
                int count = Diagnostics.CandidateCountsByIntent[i];
                if (count <= 0) continue;
                parts.Add(((BotIntentType)i) + "=" + count);
            }
            return parts.Count == 0 ? "<empty>" : string.Join(",", parts);
        }

        private string FormatUnavailable()
        {
            var unavailable = Bridge.Frame?.Unavailable;
            if (unavailable == null || unavailable.Count == 0) return "<none>";
            var parts = new System.Collections.Generic.List<string>();
            foreach (var pair in unavailable)
                parts.Add(pair.Key + ":" + pair.Value);
            return string.Join("|", parts);
        }
        // Journal candidate fields describe real legal candidates only; synthetic
        // fallbacks are surfaced via syntheticFallback, never as phantom choices.
        private static void JournalCandidates(BotCandidateSet candidates,
            out string[] available, out string[] intents)
        {
            available = Array.Empty<string>();
            intents = Array.Empty<string>();
            if (candidates == null) return;
            int real = candidates.RealCount;
            available = new string[real];
            intents = new string[real];
            int slot = 0;
            for (int i = 0; i < candidates.Count && slot < real; i++)
            {
                var candidate = candidates[i];
                if (candidate == null || candidate.Synthetic) continue;
                available[slot] = candidate.Id ?? string.Empty;
                intents[slot] = candidate.Intent.ToString();
                slot++;
            }
        }

        private void AppendRejectedDecisionEvent(int actionIndex, string reason)
        {
            if (_decisionJournal == null) return;
            var frame = Bridge.Frame;
            var candidates = frame?.Candidates;
            var candidate = candidates != null && actionIndex >= 0 && actionIndex < candidates.Count
                ? candidates[actionIndex] : null;
            JournalCandidates(candidates, out string[] available, out string[] intents);
            float rewardDelta = Rewards.TotalReward - _lastJournalReward;
            _lastJournalReward = Rewards.TotalReward;
            _decisionJournal.Append(new AgentDecisionEvent
            {
                sessionId = _observerSessionId ?? string.Empty,
                arenaId = EnvironmentId,
                episodeId = EpisodeId,
                agentId = _simulation.PlayerId.ToString(),
                scenarioId = _scenario?.id,
                scenarioStep = _scenarioProgress?.StepIndex ?? -1,
                scenarioProgress = _scenarioProgress?.Progress ?? 0f,
                candidateCount = frame?.RealCandidateCount ?? 0,
                syntheticFallback = frame?.UsedNoLegalActionFallback == true,
                availableIntents = intents,
                availableActions = available,
                chosenSlot = actionIndex,
                chosenIntent = candidate?.Intent.ToString(),
                actorId = candidate?.ActorKey,
                actionId = candidate?.Id,
                targetId = candidate?.TargetKey,
                targetX = candidate?.X ?? 0,
                targetY = candidate?.Y ?? 0,
                result = "Rejected",
                rejectionReason = reason,
                rewardDelta = rewardDelta
            });
        }

        // Sliding-window learnability gates. Forced ratio is evaluated over all
        // submissions in a window; candidate diversity and required intents are
        // evaluated per scenario step so a later step's intents (e.g. Attack in a
        // movement→combat combo) cannot false-abort an earlier step where they are
        // legitimately unavailable. All candidate counts are real legal candidates.
        private void RunWatchdogChecks(bool forced, BotDecisionTrace trace, int stepIndex)
        {
            if (!_config.watchdogEnabled || _scenario == null) return;
            _wdSubmissions++;
            if (forced) _wdForced++;

            float progress = _scenarioProgress?.Progress ?? 0f;
            if (_wdProgressMark < 0f) _wdProgressMark = progress;
            if (progress > _wdProgressMark + 0.0001f) { _wdProgressMark = progress; _wdStagnant = 0; }
            else _wdStagnant++;
            // FullGame progress is sparse by design; its episodes end on the
            // match outcome, not on step completion.
            if (!_scenario.fullGame && _scenarioProgress?.IsComplete != true
                && _wdStagnant >= _config.watchdogMaxStagnantSubmissions)
            {
                Fail("TRAINING_ABORTED reason=no_scenario_progress scenario=" + _scenario.id
                    + " stagnantSubmissions=" + _wdStagnant + " progress=" + progress.ToString("0.###"));
                return;
            }

            if (_scenarioProgress != null && stepIndex >= 0 && _scenario.steps != null
                && stepIndex < _scenario.steps.Length && !_scenarioProgress.IsComplete)
            {
                if (!_wdSteps.TryGetValue(stepIndex, out var window))
                    _wdSteps[stepIndex] = window = new StepWatchdogWindow();
                window.Submissions++;
                // The trace carries the decided frame's real candidate count;
                // diagnostics refresh after this point, so bind to the trace.
                window.CandidateSum += trace.RealCandidateCount;
                var candidates = Bridge.Frame?.Candidates;
                if (candidates != null)
                    for (int i = 0; i < candidates.Count; i++)
                        if (candidates[i] != null && !candidates[i].Synthetic)
                            window.IntentMask |= 1L << (int)candidates[i].Intent;
                if (window.Submissions >= _config.watchdogWindow)
                {
                    _wdSteps.Remove(stepIndex);
                    EvaluateStepContract(_scenario.steps[stepIndex], stepIndex, window);
                    if (!IsReady) return;
                }
            }

            if (_wdSubmissions < _config.watchdogWindow) return;
            EvaluateWatchdogWindow();
            _wdSubmissions = _wdForced = 0;
        }

        private void EvaluateWatchdogWindow()
        {
            float forcedRatio = _wdForced / (float)Math.Max(1, _wdSubmissions);
            float cap = Math.Min(_config.watchdogMaxForcedRatio,
                _scenario.maxForcedActionRatio >= 0f ? _scenario.maxForcedActionRatio : 1f);
            if (forcedRatio > cap)
                Fail("TRAINING_ABORTED reason=forced_action_ratio_too_high scenario=" + _scenario.id
                    + " window=" + _wdSubmissions + " forcedRatio=" + forcedRatio.ToString("0.###")
                    + " cap=" + cap.ToString("0.###"));
        }

        private void EvaluateStepContract(TrainingScenarioStepDefinition step, int stepIndex, StepWatchdogWindow window)
        {
            string stepId = step?.id ?? stepIndex.ToString();
            int minimum = step != null && step.minMeaningfulCandidates > 0
                ? step.minMeaningfulCandidates : _scenario.minMeaningfulCandidates;
            float meanCandidates = window.CandidateSum / (float)Math.Max(1, window.Submissions);
            if (minimum > 0 && meanCandidates < minimum)
            {
                Fail("TRAINING_ABORTED reason=candidate_diversity_collapse scenario=" + _scenario.id
                    + " step=" + stepId + " window=" + window.Submissions
                    + " meanCandidates=" + meanCandidates.ToString("0.###")
                    + " required=" + minimum);
                return;
            }
            var required = step?.requiredIntents != null && step.requiredIntents.Length > 0
                ? step.requiredIntents : _scenario.requiredIntents;
            if (required == null) return;
            foreach (var name in required)
            {
                if (!Enum.TryParse(name, true, out BotIntentType intent) || intent == BotIntentType.None) continue;
                int bit = (int)intent;
                if (bit < 0 || bit >= 64 || (window.IntentMask & (1L << bit)) != 0) continue;
                Fail("TRAINING_ABORTED reason=required_intent_missing scenario=" + _scenario.id
                    + " step=" + stepId + " window=" + window.Submissions
                    + " missingIntent=" + intent);
                return;
            }
        }

        private void AppendDecisionEvent(BotDecisionTrace trace, bool forced = false)
        {
            if (_decisionJournal == null) return;
            var frame = Bridge.Frame;
            var candidate = frame?.Candidates[trace.Slot];
            JournalCandidates(frame?.Candidates, out string[] available, out string[] intents);
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
                scenarioProgress = _scenarioProgress?.Progress ?? 0f,
                candidateCount = frame?.RealCandidateCount ?? 0,
                syntheticFallback = frame?.UsedNoLegalActionFallback == true,
                forcedAction = forced,
                availableIntents = intents,
                availableActions = available,
                chosenSlot = trace.Slot,
                chosenIntent = trace.Intent.ToString(),
                actorId = candidate?.ActorKey,
                actionId = candidate?.Id,
                targetId = candidate?.TargetKey,
                targetX = candidate?.X ?? 0,
                targetY = candidate?.Y ?? 0,
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
