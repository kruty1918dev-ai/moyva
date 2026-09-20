using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.Grid.API;
using Zenject;
using System;
using System.Reflection;

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
            container.Bind<BotDebugOverlay>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
        }

        private static void ApplyLaunchDifficulty(BotRuntimeConfig config)
        {
            if (config == null)
                return;

            var registry = BotDifficultyRegistry.Load(config.difficultyRegistryResourceId);
            var selected = registry.Select(ReadLaunchBotDifficultyId());
            config.selectedDifficultyId = selected.id;
            config.selectedDifficultyName = selected.displayName;
            config.policyMode = selected.policyMode;
            config.modelProfile = selected.modelProfile ?? new BotModelProfile();
            config.curriculumStage = config.modelProfile.curriculumStage;
            if (selected.visibleDelay >= 0f)
                config.visibleDelay = selected.visibleDelay;
            if (selected.explorationRate >= 0f)
                config.explorationRate = selected.explorationRate;
        }

        private static string ReadLaunchBotDifficultyId()
        {
            var type = Type.GetType("Kruty1918.Moyva.SaveSystem.GameLaunchContext, Kruty1918.Moyva.SaveSystem");
            var property = type?.GetProperty("BotDifficultyId", BindingFlags.Public | BindingFlags.Static);
            return property?.GetValue(null) as string;
        }
        private static IBotDecisionOrchestrator Create(DiContainer container)
        {
            var turns = container.Resolve<ITurnService>();
            var gateway = new MoyvaBotTurnAdapter(turns, container.TryResolve<ITurnAuthorityPolicy>());
            var units = container.TryResolve<IUnitService>();
            var owners = container.TryResolve<IUnitOwnershipQuery>();
            var fog = container.TryResolve<IFogOwnerStateReader>();
            var profiles = container.TryResolve<IUnitGameplayProfileService>();
            var terrain = container.TryResolve<IGeneratedTerrainLevelQuery>();
            var registry = CreateGameplayRegistry(container, gateway);
            // Difficulty resolves lazily: on save-load the bot identity arrives
            // after container construction via the bot-opponent save module.
            var config = container.Resolve<BotRuntimeConfig>().Snapshot();
            ApplyLaunchDifficulty(config);
            config.Validate();
            var telemetry = container.Resolve<BotTelemetryHub>();
            var factory = container.TryResolve<IBotPolicyDriverFactory>();
            var policy = factory?.Create(config, telemetry) ?? new HeuristicBotPolicyDriver();
            var economyApi = container.TryResolve<IEconomyRuntimeApi>();
            var result = new BotDecisionOrchestrator(gateway, registry,
                new MoyvaBotPerceptionSource(turns, units, owners, fog,
                    profiles: profiles,
                    terrain: terrain,
                    economy: container.TryResolve<IEconomyInfoMediator>(),
                    intel: container.TryResolve<IFogIntelReader>(),
                    grid: container.TryResolve<IGridService>(),
                    placements: container.TryResolve<IConstructionSaveSnapshotSource>(),
                    buildingDefs: container.TryResolve<IBuildingRegistry>(),
                    economyApi: economyApi,
                    recruitment: container.TryResolve<IUnitRecruitmentQuery>(),
                    productionPerTurn: p => economyApi?.GetOwnerProductionSnapshot(p)?.ProductionPerTurn),
                policy, config, telemetry);
            if (config.policyMode != BotPolicyMode.Heuristic && factory == null)
                telemetry.FallbackReason = "No ML policy binding installed; using Heuristic.";
            return result;
        }
        public static BotCapabilityRegistry CreateGameplayRegistry(DiContainer container, IBotTurnGateway gateway)
        {
            var units = container.TryResolve<IUnitService>();
            var owners = container.TryResolve<IUnitOwnershipQuery>();
            var fog = container.TryResolve<IFogOwnerStateReader>();
            var profiles = container.TryResolve<IUnitGameplayProfileService>();
            var terrain = container.TryResolve<IGeneratedTerrainLevelQuery>();
            var registry = CreateRegistry(gateway,
                new CombatBotCapability(gateway, units, owners, container.TryResolve<IUnitCombatQuery>(),
                    container.TryResolve<ICombatCommandService>(), fog, container.TryResolve<IConstructionBuildingCombatTargetQuery>(),
                    container.TryResolve<IHealthRegistry>(), profiles, terrain),
                new RecruitmentBotCapability(gateway, container.TryResolve<IUnitRecruitmentQuery>(),
                    container.TryResolve<IUnitRecruitmentService>(), fog),
                new ConstructionBotCapability(gateway, container.TryResolve<IBuildingRegistry>(),
                    container.TryResolve<IConstructionPlacementQuery>(), container.TryResolve<IAuthoritativeConstructionPlacementExecutor>(),
                    container.TryResolve<IConstructionSaveSnapshotSource>(), units, owners, fog,
                    container.TryResolve<IBotOpeningPlacementAnchorSource>()),
                new CaptureBotCapability(gateway, units, owners, fog,
                    container.TryResolve<IConstructionSaveSnapshotSource>(), container.TryResolve<ISettlementCaptureQuery>(),
                    container.TryResolve<ISettlementCaptureService>()));
            registry.Register(new MovementBotCapability(gateway, units, owners,
                container.TryResolve<IUnitMovementQuery>(), container.TryResolve<IUnitMovementService>(), fog, profiles, terrain));
            return registry;
        }
        public static BotCapabilityRegistry CreateRegistry(IBotTurnGateway turns, IBotCapabilityProvider combat = null,
            IBotCapabilityProvider recruitment = null, IBotCapabilityProvider construction = null, IBotCapabilityProvider capture = null)
        {
            var registry = new BotCapabilityRegistry();
            registry.Register(new EndTurnBotCapability(turns));
            registry.Register(combat ?? new UnavailableBotCapability(BotCapabilityId.Combat,
                "Combat query/command and player-visible entity gateway are not connected."));
            registry.Register(recruitment ?? new UnavailableBotCapability(BotCapabilityId.Recruitment,
                "Recruitment query/command services are not installed."));
            registry.Register(construction ?? new UnavailableBotCapability(BotCapabilityId.Construction,
                "IConstructionPlacementQuery exists; a player-scoped build catalog and bounded candidate-location gateway are not connected."));
            registry.Register(capture ?? new UnavailableBotCapability(BotCapabilityId.Capture,
                "Authoritative capture query/command and player visibility are not connected."));
            return registry;
        }
    }
}
