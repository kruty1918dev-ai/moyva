using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.SaveSystem;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    /// <summary>
    /// Canonical BotAI DI composition root.
    ///
    /// Gameplay scenes are not allowed to depend on a scene-authored BotInstaller
    /// being present. Bootstrap can safely call this composition root, while the
    /// optional BotInstaller delegates to the same method. Every binding is guarded,
    /// so invoking the installer more than once in the same container is idempotent.
    /// </summary>
    public static class BotRuntimeBindings
    {
        public static void Install(DiContainer container)
        {
            if (container == null)
                return;

            BindDifficulty(container);
            BindPlanningProfile(container);

            BindIfMissing<IBotMemoryStore, BotMemoryStore>(container);

            if (!container.HasBinding<IBotTerrainKnowledge>())
            {
                container.BindInterfacesAndSelfTo<BotTerrainKnowledgeService>()
                    .AsSingle()
                    .NonLazy();
            }

            BindIfMissing<IBotPerceptionService, BotPerceptionService>(container);
            BindIfMissing<IBotCastleSiteEvaluator, BotCastleSiteEvaluator>(container);
            BindIfMissing<IBotReasoningTrace, BotReasoningTraceStore>(container);

            BindIfMissing<IBotWorldSnapshotBuilder, BotWorldSnapshotBuilder>(container);

            if (!container.HasBinding<IBotStrategicPlanner>() ||
                !container.HasBinding<IBotStrategicStateStore>())
            {
                container.BindInterfacesAndSelfTo<BotStrategicPlanner>()
                    .AsSingle();
            }

            BindSaveModule<BotAISaveModule>(container);

            BindIfMissing<IBotConstructionPlanner, BotConstructionPlanner>(container);
            BindIfMissing<IBotDeploymentPlanner, BotDeploymentPlanner>(container);
            BindIfMissing<IBotCombatPlanner, BotCombatPlanner>(container);
            BindIfMissing<IBotDefensePlanner, BotDefensePlanner>(container);
            BindIfMissing<IBotUnitRoleResolver, BotUnitRoleResolver>(container);
            BindIfMissing<IBotInfluenceMapService, BotInfluenceMapService>(container);
            BindIfMissing<IBotTacticalSequencer, BotTacticalSequencer>(container);
            BindIfMissing<IBotRecruitmentPlanner, BotRecruitmentPlanner>(container);
            BindIfMissing<IBotScoutingPlanner, BotScoutingPlanner>(container);
            BindIfMissing<IBotGoalStore, BotGoalStore>(container);
            BindIfMissing<IBotDecisionTrace, BotDecisionTraceStore>(container);

            BindSaveModule<BotGoalSaveModule>(container);

            BindIfMissing<IBotObjectivePlanner, BotObjectivePlanner>(container);
            BindIfMissing<IBotMovementPlanner, BotMovementPlanner>(container);
            BindIfMissing<IBotTurnPlanner, BotTurnPlanner>(container);
            BindIfMissing<IBotActionExecutor, BotActionExecutor>(container);

            if (!container.HasBinding<IBotTurnExecutor>())
            {
                container.BindInterfacesAndSelfTo<BotTurnExecutor>()
                    .AsSingle();
            }

            // Legacy wall-clock BotTickScheduler deliberately remains unbound.
            //
            // BotFogInitializer is intentionally NOT bound from the canonical bootstrap
            // composition root. It is an old global-fog compatibility bridge whose
            // Faction/Fog dependencies are optional scene features. Modern BotAI uses
            // IBotPerceptionService as the authoritative per-bot knowledge path.
        }

        public static bool IsCoreReady(DiContainer container)
        {
            return container != null
                && container.HasBinding<IBotTurnExecutor>()
                && container.HasBinding<IBotWorldSnapshotBuilder>()
                && container.HasBinding<IBotTurnPlanner>()
                && container.HasBinding<IBotActionExecutor>()
                && container.HasBinding<IBotStrategicPlanner>()
                && container.HasBinding<IBotGoalStore>()
                && container.HasBinding<IBotDecisionTrace>();
        }

        private static void BindDifficulty(DiContainer container)
        {
            if (container.HasBinding<IBotDifficultySettings>())
                return;

            container.Bind<IBotDifficultySettings>()
                .FromInstance(BotDifficultySettings.Normal())
                .AsSingle();
        }

        private static void BindPlanningProfile(DiContainer container)
        {
            if (container.HasBinding<BotPlanningProfile>())
                return;

            container.Bind<BotPlanningProfile>()
                .FromInstance(BotPlanningProfile.Normal())
                .AsSingle();
        }

        private static void BindIfMissing<TContract, TConcrete>(
            DiContainer container)
            where TConcrete : class, TContract
        {
            if (container.HasBinding<TContract>())
                return;

            container.Bind<TContract>()
                .To<TConcrete>()
                .AsSingle();
        }

        private static void BindSaveModule<TModule>(DiContainer container)
            where TModule : class, ISaveModule
        {
            if (container.HasBinding<TModule>())
                return;

            container.BindInterfacesAndSelfTo<TModule>()
                .AsSingle();

            container.BindInterfacesTo<SaveModuleRegistrar<TModule>>()
                .AsSingle()
                .NonLazy();
        }
    }
}
