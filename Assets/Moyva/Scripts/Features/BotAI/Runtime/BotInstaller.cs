using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    /// <summary>
    /// Zenject MonoInstaller для AI-ботів.
    /// Підключіть у сцені після FactionInstaller.
    /// </summary>
    public sealed class BotInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<BotTickScheduler>()
                .AsSingle()
                .NonLazy();
        }
    }
}
