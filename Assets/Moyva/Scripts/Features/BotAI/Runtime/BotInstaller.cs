using Kruty1918.Moyva.BotAI.API;
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

            if (!Container.HasBinding<IBotTurnExecutor>())
            {
                Container.Bind<IBotTurnExecutor>()
                    .To<BotTurnExecutor>()
                    .AsSingle();
            }

            // P10 legacy wall-clock BotTickScheduler deliberately remains unbound.
            Container.BindInterfacesTo<BotFogInitializer>()
                .AsSingle()
                .NonLazy();
        }
    }
}
