using System;
using Kruty1918.Moyva.AI.Bot;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.FogOfWar.API;
using Zenject;

namespace Kruty1918.Moyva.AI.Training
{
    public sealed class TrainingGameplayScope : IDisposable
    {
        private readonly IDisposable _lifetime;
        private readonly TrainingGameplayEventBridge _events;
        public string PlayerId { get; }
        public ITurnService Turns { get; }
        public IBotTurnGateway TurnGateway { get; }
        public BotCapabilityRegistry Capabilities { get; }
        public IBotPerceptionSource Perception { get; }
        public IGameplayTrainingReset Reset { get; }
        public ITrainingEpisodeOutcomeSource Outcomes { get; }

        public TrainingGameplayScope(DiContainer gameplay, string playerId,
            IGameplayTrainingReset reset, IDisposable ownedLifetime)
        {
            if (string.IsNullOrWhiteSpace(playerId)) throw new ArgumentException("Training player identity is missing.");
            _lifetime = ownedLifetime ?? throw new ArgumentNullException(nameof(ownedLifetime));
            Reset = reset ?? throw new InvalidOperationException("GAMEPLAY_RESET_BLOCKED: no complete episode reset.");
            PlayerId = playerId;
            // Required dependencies: never replace these with null/empty observations.
            Turns = gameplay.Resolve<ITurnService>();
            if (!(Turns is ITurnEndQuery)) throw new InvalidOperationException("Real turns must implement ITurnEndQuery.");
            var authority = gameplay.Resolve<ITurnAuthorityPolicy>();
            if (!authority.IsAuthoritative) throw new InvalidOperationException("Training scope must own gameplay authority.");
            var units = gameplay.Resolve<IUnitService>();
            var owners = gameplay.Resolve<IUnitOwnershipQuery>();
            var fog = gameplay.Resolve<IFogOwnerStateReader>();
            TurnGateway = new MoyvaBotTurnAdapter(Turns, authority);
            Capabilities = BotRuntimeInstaller.CreateRegistry(TurnGateway);
            Capabilities.Register(new MovementBotCapability(TurnGateway, units, owners,
                gameplay.Resolve<IUnitMovementQuery>(), gameplay.Resolve<IUnitMovementService>(), fog));
            Perception = new MoyvaBotPerceptionSource(Turns, units, owners, fog);
            Outcomes = gameplay.TryResolve<ITrainingEpisodeOutcomeSource>();
            if (Outcomes == null && gameplay.HasBinding<SignalBus>() && gameplay.HasBinding<ITurnHistoryQuery>())
            {
                _events = new TrainingGameplayEventBridge(gameplay.Resolve<SignalBus>(), gameplay.Resolve<ITurnHistoryQuery>(), playerId);
                Outcomes = _events;
            }
        }

        public void Dispose() { _events?.Dispose(); _lifetime.Dispose(); }
    }
}
