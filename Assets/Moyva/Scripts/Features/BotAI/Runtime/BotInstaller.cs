using Kruty1918.Moyva.BotAI.API;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    /// <summary>
    /// Zenject MonoInstaller для AI-ботів.
    /// Підключіть у сцені після FactionInstaller та FogOfWarInstaller.
    /// </summary>
    public sealed class BotInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<IBotDifficultySettings>()
                .FromInstance(BotDifficultySettings.Normal())
                .AsSingle();

            Container.BindInterfacesAndSelfTo<BotTickScheduler>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesTo<BotFogInitializer>()
                .AsSingle()
                .NonLazy();
        }
    }
}
