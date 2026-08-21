using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.SaveSystem;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    public sealed class BotInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IBotDifficultySettings>()
                .FromInstance(BotDifficultySettings.Normal())
                .AsSingle();

            Container.Bind<BotPlanningProfile>()
                .FromInstance(BotPlanningProfile.Normal())
                .AsSingle();

            Container.BindInterfacesAndSelfTo<BotTerrainKnowledgeService>().AsSingle().NonLazy();
            Container.Bind<IBotPerceptionService>().To<BotPerceptionService>().AsSingle();
            Container.Bind<IBotCastleSiteEvaluator>().To<BotCastleSiteEvaluator>().AsSingle();
            Container.Bind<IBotReasoningTrace>().To<BotReasoningTraceStore>().AsSingle();

            Container.Bind<IBotMemoryStore>()
                .To<BotMemoryStore>()
                .AsSingle();

            Container.Bind<IBotWorldSnapshotBuilder>()
                .To<BotWorldSnapshotBuilder>()
                .AsSingle();

            Container.Bind<IBotStrategicPlanner>()
                .To<BotStrategicPlanner>()
                .AsSingle();

            Container.Bind<IBotStrategicStateStore>()
                .To<BotStrategicPlanner>()
                .FromResolve();

            Container.BindInterfacesAndSelfTo<BotAISaveModule>()
                .AsSingle();

            Container.BindInterfacesTo<SaveModuleRegistrar<BotAISaveModule>>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IBotConstructionPlanner>()
                .To<BotConstructionPlanner>()
                .AsSingle();

            Container.Bind<IBotDeploymentPlanner>()
                .To<BotDeploymentPlanner>()
                .AsSingle();

            Container.Bind<IBotCombatPlanner>()
                .To<BotCombatPlanner>()
                .AsSingle();

            Container.Bind<IBotDefensePlanner>()
                .To<BotDefensePlanner>()
                .AsSingle();

            Container.Bind<IBotUnitRoleResolver>()
                .To<BotUnitRoleResolver>()
                .AsSingle();

            Container.Bind<IBotInfluenceMapService>()
                .To<BotInfluenceMapService>()
                .AsSingle();

            Container.Bind<IBotTacticalSequencer>()
                .To<BotTacticalSequencer>()
                .AsSingle();

            Container.Bind<IBotRecruitmentPlanner>()
                .To<BotRecruitmentPlanner>()
                .AsSingle();

            Container.Bind<IBotScoutingPlanner>()
                .To<BotScoutingPlanner>()
                .AsSingle();

            Container.Bind<IBotGoalStore>()
                .To<BotGoalStore>()
                .AsSingle();

            Container.Bind<IBotDecisionTrace>()
                .To<BotDecisionTraceStore>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<BotGoalSaveModule>()
                .AsSingle();

            Container.BindInterfacesTo<SaveModuleRegistrar<BotGoalSaveModule>>()
                .AsSingle()
                .NonLazy();

            Container.Bind<IBotObjectivePlanner>()
                .To<BotObjectivePlanner>()
                .AsSingle();

            Container.Bind<IBotMovementPlanner>()
                .To<BotMovementPlanner>()
                .AsSingle();

            Container.Bind<IBotTurnPlanner>()
                .To<BotTurnPlanner>()
                .AsSingle();

            Container.Bind<IBotActionExecutor>()
                .To<BotActionExecutor>()
                .AsSingle();

            if (!Container.HasBinding<IBotTurnExecutor>())
            {
                Container.BindInterfacesAndSelfTo<BotTurnExecutor>()
                    .AsSingle();
            }

            // Legacy wall-clock BotTickScheduler deliberately remains unbound.
            Container.BindInterfacesTo<BotFogInitializer>()
                .AsSingle()
                .NonLazy();
        }
    }
}
