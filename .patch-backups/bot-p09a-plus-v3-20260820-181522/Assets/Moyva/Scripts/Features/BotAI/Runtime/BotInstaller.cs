using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.SaveSystem;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    /// <summary>
    /// Zenject MonoInstaller for bot support services.
    /// Runtime bot mutations are turn-scoped through IBotTurnExecutor.
    /// </summary>
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

            // P10 legacy wall-clock BotTickScheduler deliberately remains unbound.
            Container.BindInterfacesTo<BotFogInitializer>()
                .AsSingle()
                .NonLazy();
        }
    }
}
