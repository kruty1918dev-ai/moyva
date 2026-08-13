using Kruty1918.Moyva.BotAI.API;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    /// <summary>
    /// Zenject MonoInstaller for bot support services.
    ///
    /// P10 deliberately does not bind the legacy wall-clock BotTickScheduler.
    /// Authoritative bot actions are owned by the turn loop; P11 supplies the
    /// IBotTurnExecutor implementation consumed by that loop.
    /// </summary>
    public sealed class BotInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IBotDifficultySettings>()
                .FromInstance(BotDifficultySettings.Normal())
                .AsSingle();

            // Do not restore BotTickScheduler here. Its Time.deltaTime loop was a
            // second authority path that could mutate bot factions outside their turn.

            Container.BindInterfacesTo<BotFogInitializer>()
                .AsSingle()
                .NonLazy();
        }
    }
}
