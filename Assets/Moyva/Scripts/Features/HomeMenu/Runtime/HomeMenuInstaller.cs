using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Multiplayer.Runtime;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Zenject MonoInstaller для домашнього меню.
    /// Підключіть у сцені з домашнім меню для реєстрації всіх його сервісів.
    /// </summary>
    public sealed class HomeMenuInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Install(Container);
        }

        public static void Install(DiContainer container)
        {
            // // Навігація між меню
            // container.Bind<INavigation>()
            //     .To<HomeMenuNavigation>()
            //     .AsSingle();

            // // Завантажувач оверлеїв (показує індикатор завантаження поверх меню)
            // container.Bind<IOverlayLoader>()
            //     .To<HomeMenuOverlayLoader>()
            //     .AsSingle();
        }
    }

    internal class HomeMenuInitializer : IInitializable
    {
        private readonly IMultiplayerState _multiplayerState;

        internal HomeMenuInitializer(IMultiplayerState multiplayerState)
        {
            _multiplayerState = multiplayerState;
        }   

        public void Initialize()
        {
            
        }
    }
}