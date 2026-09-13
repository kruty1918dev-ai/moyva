using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Combat.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.AI.Bot
{
    public static class BotRuntimeInstaller
    {
        public static void Install(DiContainer container)
        {
            var asset = Resources.Load<TextAsset>("MoyvaBotRuntime");
            var config = asset != null ? JsonUtility.FromJson<BotRuntimeConfig>(asset.text) : new BotRuntimeConfig();
            var profile = Resources.Load<TextAsset>(config.modelProfileResourceId);
            if (profile != null) config.modelProfile = JsonUtility.FromJson<BotModelProfile>(profile.text);
            config.Validate();
            container.Bind<BotRuntimeConfig>().FromInstance(config.Snapshot()).AsSingle();
            container.Bind<BotTelemetryHub>().FromInstance(new BotTelemetryHub(config.telemetryCapacity, config.telemetryEnabled)).AsSingle();
            container.Bind<IBotDecisionOrchestrator>().FromMethod(context => Create(context.Container)).AsSingle();
            container.Bind<BotTelemetryView>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
        }
        private static IBotDecisionOrchestrator Create(DiContainer container)
        {
            var turns = container.Resolve<ITurnService>();
            var gateway = new MoyvaBotTurnAdapter(turns, container.TryResolve<ITurnAuthorityPolicy>());
            var units = container.TryResolve<IUnitService>();
            var owners = container.TryResolve<IUnitOwnershipQuery>();
            var fog = container.TryResolve<IFogOwnerStateReader>();
            var registry = CreateRegistry(gateway, new CombatBotCapability(gateway, units, owners,
                container.TryResolve<IUnitCombatQuery>(), container.TryResolve<ICombatCommandService>(), fog));
            registry.Register(new MovementBotCapability(gateway, units, owners,
                container.TryResolve<IUnitMovementQuery>(), container.TryResolve<IUnitMovementService>(), fog));
            var config = container.Resolve<BotRuntimeConfig>();
            var telemetry = container.Resolve<BotTelemetryHub>();
            var factory = container.TryResolve<IBotPolicyDriverFactory>();
            var policy = factory?.Create(config, telemetry) ?? new HeuristicBotPolicyDriver();
            var result = new BotDecisionOrchestrator(gateway, registry,
                new MoyvaBotPerceptionSource(turns, units, owners, fog), policy, config, telemetry);
            if (config.policyMode != BotPolicyMode.Heuristic && factory == null)
                telemetry.FallbackReason = "No ML policy binding installed; using Heuristic.";
            return result;
        }
        public static BotCapabilityRegistry CreateRegistry(IBotTurnGateway turns, IBotCapabilityProvider combat = null)
        {
            var registry = new BotCapabilityRegistry();
            registry.Register(new EndTurnBotCapability(turns));
            registry.Register(combat ?? new UnavailableBotCapability(BotCapabilityId.Combat,
                "Combat query/command and player-visible entity gateway are not connected."));
            registry.Register(new UnavailableBotCapability(BotCapabilityId.Recruitment,
                "IUnitRecruitmentService has no non-mutating enqueue eligibility/options query for a player and source."));
            registry.Register(new UnavailableBotCapability(BotCapabilityId.Construction,
                "IConstructionPlacementQuery exists; a player-scoped build catalog and bounded candidate-location gateway are not connected."));
            registry.Register(new UnavailableBotCapability(BotCapabilityId.Capture,
                "No public capture candidate/legality/command gateway identified."));
            return registry;
        }
    }
}
