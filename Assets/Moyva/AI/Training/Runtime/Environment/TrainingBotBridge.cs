using Kruty1918.Moyva.AI.Bot;
namespace Kruty1918.Moyva.AI.Training
{
    public interface ITrainingBotRuntimeSource
    {
        IBotTurnGateway TurnGateway { get; }
        BotCapabilityRegistry Capabilities { get; }
        IBotPerceptionSource Perception { get; }
    }
    public interface IGameplayTrainingReset
    {
        bool Reset(TrainingResetContext context);
    }
    public sealed class GameplayTrainingSimulationAdapter : ITrainingSimulation, ITrainingBotRuntimeSource
    {
        private readonly IGameplayTrainingReset _reset;
        private readonly System.IDisposable _ownedScope;
        public Kruty1918.Moyva.Turns.API.ITurnService Turns { get; }
        public string PlayerId { get; }
        public bool IsReady { get; private set; }
        public string Limitation => _reset == null ? "Missing scoped gameplay episode reset." : null;
        public IBotTurnGateway TurnGateway { get; }
        public BotCapabilityRegistry Capabilities { get; }
        public IBotPerceptionSource Perception { get; }
        public GameplayTrainingSimulationAdapter(string playerId, Kruty1918.Moyva.Turns.API.ITurnService turns,
            IBotTurnGateway turnGateway, BotCapabilityRegistry capabilities, IBotPerceptionSource perception,
            IGameplayTrainingReset reset, System.IDisposable ownedScope = null)
        {
            PlayerId = playerId; Turns = turns; TurnGateway = turnGateway; Capabilities = capabilities;
            Perception = perception; _reset = reset; _ownedScope = ownedScope;
        }
        public bool Reset(TrainingResetContext context) => IsReady = _reset != null && _reset.Reset(context);
        public bool CanEndTurn() => IsReady && TurnGateway.CanEndTurn(PlayerId, out _);
        public void Dispose() { IsReady = false; _ownedScope?.Dispose(); }
    }
    public sealed class TrainingBotBridge
    {
        private readonly ITrainingSimulation _simulation;
        private ManualBotPolicyDriver _policy;
        public BotDecisionOrchestrator Orchestrator { get; private set; }
        public BotTelemetryHub Telemetry { get; }
        public BotDecisionFrame Frame => Orchestrator?.Frame;
        public bool CanRequestDecision => Orchestrator?.Session?.State == BotOrchestratorState.AwaitingPolicy;
        public TrainingBotBridge(ITrainingSimulation simulation, int environmentId = -1, int telemetryCapacity = 128)
        { _simulation = simulation; Telemetry = new BotTelemetryHub(telemetryCapacity) { EnvironmentId = environmentId }; }
        public void Reset(int stage, System.Func<TrainingScenarioProgressTracker> progress = null,
            int capabilityMask = -1, System.Func<bool> suppressHints = null)
        {
            Orchestrator?.Dispose();
            var source = _simulation as ITrainingBotRuntimeSource;
            IBotTurnGateway turns = source?.TurnGateway ?? new SimulationTurns(_simulation);
            var registry = source?.Capabilities ?? BotRuntimeInstaller.CreateRegistry(turns);
            if (registry.Get(BotCapabilityId.Movement) == null)
                registry.Register(new UnavailableBotCapability(BotCapabilityId.Movement, "Training simulation has no shared movement gateway."));
            _policy = new ManualBotPolicyDriver();
            IBotPerceptionSource perception = source?.Perception ?? new EmptyBotPerceptionSource();
            if (progress != null) perception = new ScenarioAwarePerceptionSource(perception, progress, suppressHints);
            Orchestrator = new BotDecisionOrchestrator(turns, registry, perception,
                _policy, new BotRuntimeConfig { curriculumStage = stage, visibleDelay = 0, capabilityMask = capabilityMask }, Telemetry);
            Orchestrator.SetPolicy(_policy, "Training");
        }
        public void Tick(float seconds)
        {
            if (Orchestrator == null) return;
            Orchestrator.BeginTurn(_simulation.PlayerId);
            Orchestrator.Tick(seconds);
        }
        public bool Submit(int slot)
        {
            if (!CanRequestDecision) return false;
            _policy.Submit(slot);
            Orchestrator.Tick(0);
            return true;
        }
        private sealed class SimulationTurns : IBotTurnGateway
        {
            private readonly ITrainingSimulation _simulation;
            public SimulationTurns(ITrainingSimulation simulation) { _simulation = simulation; }
            public BotGameStamp Read(string player) => _simulation.Turns != null
                ? new MoyvaBotTurnAdapter(_simulation.Turns).Read(player)
                : new BotGameStamp(player, 0, 2, _simulation.IsReady);
            public bool CanEndTurn(string player, out string reason)
            {
                reason = "Training simulation has no legal EndTurn.";
                return _simulation.IsReady && _simulation.Turns != null
                    && _simulation.Turns.CanOwnerAct(player, out reason) && _simulation.CanEndTurn();
            }
            public bool EndTurn(string player, out string reason)
            {
                if (!CanEndTurn(player, out reason)) return false;
                return _simulation.Turns.TryEndTurn(player, out reason);
            }
        }
    }
}
