using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.AI.Bot
{
    public enum BotOrchestratorState { Idle, BuildingFrame, AwaitingPolicy, Executing, Waiting, Cancelled }
    public sealed class BotTurnSession
    {
        public string Id { get; } = Guid.NewGuid().ToString("N");
        public string Owner { get; }
        public long Turn { get; }
        public int Decisions, Invalid, Stale, Executed;
        public DateTime StartedUtc { get; } = DateTime.UtcNow;
        public BotOrchestratorState State;
        public BotTurnSession(string owner, long turn) { Owner = owner; Turn = turn; }
    }
    public interface IBotDecisionOrchestrator
    {
        BotTurnSession Session { get; }
        BotDecisionFrame Frame { get; }
        BotTelemetryHub Telemetry { get; }
        void BeginTurn(string owner);
        void Tick(float seconds);
        void Cancel();
        void SetPolicy(IBotPolicyDriver policy, string profileName, string fallbackReason = null);
    }
    public sealed class BotDecisionOrchestrator : IBotDecisionOrchestrator, IDisposable
    {
        private readonly IBotTurnGateway _turns;
        private readonly BotCapabilityRegistry _registry;
        private readonly BotDecisionFrameBuilder _builder;
        private readonly BotRuntimeConfig _config;
        private IBotPolicyDriver _policy;
        private CancellationTokenSource _cancellation;
        private Task<BotPolicyDecision> _decision;
        private Task<BotExecutionResult> _execution;
        private BotCandidateAction _selected;
        private int _slot = -1;
        private float _elapsed;
        private long _haltedTurn = -1;
        private string _haltedOwner;
        private bool _fallback;
        private BotPolicyMode _decisionMode;
        public BotTurnSession Session { get; private set; }
        public BotDecisionFrame Frame { get; private set; }
        public BotTelemetryHub Telemetry { get; }
        public event Action<BotDecisionTrace> DecisionFinished;

        public BotDecisionOrchestrator(IBotTurnGateway turns, BotCapabilityRegistry registry,
            IBotPerceptionSource perception, IBotPolicyDriver policy, BotRuntimeConfig config, BotTelemetryHub telemetry)
        {
            _turns = turns; _registry = registry; _config = config.Snapshot(); Telemetry = telemetry;
            _builder = new BotDecisionFrameBuilder(registry, perception, turns, telemetry, _config);
            SetPolicy(policy, policy?.Mode == BotPolicyMode.Heuristic ? "Heuristic" : config.modelProfile.modelName, telemetry.FallbackReason);
        }
        public void SetPolicy(IBotPolicyDriver policy, string profileName, string fallbackReason = null)
        {
            if (Session != null && Session.State != BotOrchestratorState.Idle && Session.State != BotOrchestratorState.Cancelled)
                throw new InvalidOperationException("Swap policies between turns.");
            var previous = _policy;
            _policy = policy ?? new HeuristicBotPolicyDriver();
            if (!ReferenceEquals(previous, _policy)) (previous as IDisposable)?.Dispose();
            Telemetry.PolicyMode = _policy.Mode; Telemetry.ProfileName = profileName;
            Telemetry.FallbackReason = fallbackReason;
        }
        public void BeginTurn(string owner)
        {
            var stamp = _turns.Read(owner);
            if (!stamp.CanAct || (stamp.Turn == _haltedTurn && owner == _haltedOwner)) return;
            if (Session != null && Session.Owner == owner && Session.Turn == stamp.Turn
                && Session.State != BotOrchestratorState.Cancelled && Session.State != BotOrchestratorState.Idle) return;
            if (_execution != null && !_execution.IsCompleted) return;
            Cancel();
            _cancellation?.Dispose();
            _cancellation = new CancellationTokenSource();
            _decision = null; _execution = null; _fallback = false; _elapsed = 0;
            Session = new BotTurnSession(owner, stamp.Turn) { State = BotOrchestratorState.BuildingFrame };
        }
        public void Tick(float seconds)
        {
            if (Session == null || Session.State == BotOrchestratorState.Idle || Session.State == BotOrchestratorState.Cancelled) return;
            _elapsed += Math.Max(0, seconds);
            try
            {
                if (Session.State == BotOrchestratorState.Executing && _execution.IsCompleted)
                {
                    FinishExecution();
                    return;
                }
                var current = _turns.Read(Session.Owner);
                if (current.Owner != Session.Owner || current.Turn != Session.Turn) { Cancel(); return; }
                if (Session.State == BotOrchestratorState.Executing)
                {
                    if (_elapsed > _config.executionTimeout) Halt("Execution timed out; cancellation requested.");
                    return;
                }
                if (!current.CanAct)
                {
                    if (_elapsed > _config.decisionTimeout) Halt("No authoritative input phase.");
                    return;
                }
                if (Session.State == BotOrchestratorState.AwaitingPolicy)
                {
                    if (!_decision.IsCompleted)
                    {
                        if (_elapsed <= _config.decisionTimeout) return;
                        Telemetry.FallbackReason = "Policy timed out.";
                        _cancellation.Cancel(); _cancellation.Dispose(); _cancellation = new CancellationTokenSource();
                        _fallback = true; Session.State = BotOrchestratorState.BuildingFrame;
                    }
                    else
                    {
                        var decision = _decision.GetAwaiter().GetResult();
                        _slot = decision.Slot; _decisionMode = decision.Mode;
                        _selected = Frame.Candidates[_slot];
                        Session.Decisions++;
                        if (decision.Sequence != Frame.Sequence || !Frame.Stamp.Matches(current))
                        { Reject(BotDecisionFailure.StaleState, "Decision frame is stale."); return; }
                        if (_selected == null)
                        { Reject(BotDecisionFailure.ModelInvalid, "Selected slot was masked."); return; }
                        if (_selected.Intent == BotIntentType.Wait)
                        {
                            Trace(new BotExecutionResult(BotExecutionStatus.Pending, _selected, "Waiting for legal game action."), BotDecisionFailure.None);
                            Session.State = BotOrchestratorState.Waiting; _elapsed = 0; return;
                        }
                        var capability = _registry.Get(_selected.Capability);
                        if (capability == null || !capability.Validate(Session.Owner, _selected, out _))
                        { Reject(BotDecisionFailure.StaleState, "Candidate is no longer legal."); return; }
                        Session.State = BotOrchestratorState.Executing; _elapsed = 0;
                        _execution = capability.Execute(Session.Owner, _selected, _cancellation.Token);
                        if (_execution.IsCompleted) FinishExecution();
                        return;
                    }
                }
                if (Session.State == BotOrchestratorState.Waiting)
                {
                    if (_elapsed > _config.decisionTimeout) { Halt("No legal action before timeout."); return; }
                    if (!_turns.CanEndTurn(Session.Owner, out _)) return;
                    Session.State = BotOrchestratorState.BuildingFrame;
                }
                if (Session.State == BotOrchestratorState.BuildingFrame)
                {
                    _fallback |= Session.Decisions >= _config.maxDecisionsPerTurn
                        || Session.Invalid >= _config.maxInvalidDecisions || Session.Stale >= _config.maxStaleDecisions;
                    if (_fallback && !_config.autoEndTurn) { Halt("Decision budget exhausted."); return; }
                    Frame = _builder.Build(Session.Owner);
                    BotArchitectureValidator.ValidateFrame(Frame, _registry);
                    _decisionMode = _fallback ? BotPolicyMode.Heuristic : _policy.Mode;
                    var policy = _fallback ? new HeuristicBotPolicyDriver() : _policy;
                    if (_fallback) Telemetry.FallbackReason = "Decision budget exhausted; safe EndTurn.";
                    Session.State = BotOrchestratorState.AwaitingPolicy; _elapsed = 0;
                    _decision = policy.Decide(Frame, _cancellation.Token);
                }
            }
            catch (Exception exception)
            {
                if (Session.State == BotOrchestratorState.AwaitingPolicy && !_fallback)
                {
                    Telemetry.FallbackReason = exception.Message;
                    _fallback = true;
                    _cancellation.Cancel(); _cancellation.Dispose(); _cancellation = new CancellationTokenSource();
                    Session.State = BotOrchestratorState.BuildingFrame;
                }
                else Halt(exception.Message);
            }
        }
        private void Reject(BotDecisionFailure failure, string reason)
        {
            if (failure == BotDecisionFailure.StaleState) Session.Stale++; else Session.Invalid++;
            Trace(new BotExecutionResult(BotExecutionStatus.Rejected, _selected, reason), failure);
            if (_fallback) { Halt(reason); return; }
            Session.State = BotOrchestratorState.BuildingFrame; _elapsed = 0;
        }
        private void FinishExecution()
        {
            var result = _execution.GetAwaiter().GetResult();
            if (result.Status == BotExecutionStatus.Pending || result.Status == BotExecutionStatus.Accepted)
            { Halt("Capability returned pending without a completion task."); return; }
            if (result.Status == BotExecutionStatus.Completed) Session.Executed++;
            else Session.Invalid++;
            Session.State = _selected.Intent == BotIntentType.EndTurn && result.Status == BotExecutionStatus.Completed
                ? BotOrchestratorState.Idle : BotOrchestratorState.BuildingFrame;
            Trace(result, result.Status == BotExecutionStatus.Completed ? BotDecisionFailure.None : BotDecisionFailure.CommandRejected);
            _elapsed = 0;
            if (_fallback && result.Status != BotExecutionStatus.Completed) Halt(result.Reason);
        }
        private void Trace(BotExecutionResult result, BotDecisionFailure failure)
        {
            var trace = new BotDecisionTrace(Session, Frame, _slot, _decisionMode, result, failure, _elapsed);
            Telemetry.Record(trace);
            if (DecisionFinished != null)
                foreach (Action<BotDecisionTrace> observer in DecisionFinished.GetInvocationList())
                    try { observer(trace); }
                    catch (Exception exception) { Telemetry.LastError = "Observer failed: " + exception.Message; }
        }
        private void Halt(string reason)
        {
            Telemetry.LastError = reason; _haltedTurn = Session.Turn; _haltedOwner = Session.Owner;
            Cancel();
        }
        public void Cancel()
        {
            _cancellation?.Cancel();
            if (Session != null) Session.State = BotOrchestratorState.Cancelled;
        }
        public void Dispose()
        {
            Cancel();
            _cancellation?.Dispose();
            _cancellation = null;
            (_policy as IDisposable)?.Dispose();
            _policy = null;
        }
    }
}
