using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
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
            // Навігація між меню
            Container.Bind<INavigation>()
                .To<HomeMenuNavigation>()
                .AsSingle();

            // Завантажувач оверлеїв (показує індикатор завантаження поверх меню)
            Container.Bind<IOverlayLoader>()
                .To<HomeMenuOverlayLoader>()
                .AsSingle();

            // UI-компоненти домашнього меню, що знаходяться в ієрархії сцени.
            Container.Bind<OverlayPanelLoader>()
                .FromComponentInHierarchy(includeInactive: true)
                .AsSingle();

            Container.Bind<IConfiremationPanel>()
            .To<ConfirmationPanel>()
            .FromComponentInHierarchy(includeInactive: true)
            .AsSingle();

            Container.BindInterfacesAndSelfTo<HomeMenuInitializer>()
                .AsSingle();

            // Множині компонети

            Container.BindInterfacesTo<NavigationPanel>()
                .FromComponentsInHierarchy(includeInactive: true)
                .AsCached();

            Container.Bind<NavigationButton>()
                .FromComponentsInHierarchy(includeInactive: true)
                .AsCached();

            Container.BindInterfacesAndSelfTo<IConfirmationButton>()
             .FromComponentsInHierarchy(includeInactive: true)  
                .AsCached();

            // Сервіси 
            Container.BindInterfacesAndSelfTo<ConformationService>()
            .AsSingle();

            // View controllers (from scene hierarchy)
            Container.BindInterfacesTo<BotViewController>()
                .FromComponentsInHierarchy(includeInactive: true)
                .AsCached();

            Container.BindInterfacesTo<ContinueViewController>()
                .FromComponentsInHierarchy(includeInactive: true)
                .AsCached();

            Container.BindInterfacesTo<CreateRoomViewController>()
                .FromComponentsInHierarchy(includeInactive: true)
                .AsCached();

            Container.BindInterfacesTo<JoinRoomViewController>()
                .FromComponentsInHierarchy(includeInactive: true)
                .AsCached();

            Container.BindInterfacesTo<MultiplayerViewController>()
                .FromComponentsInHierarchy(includeInactive: true)
                .AsCached();

            Container.BindInterfacesTo<SoloViewController>()
                .FromComponentsInHierarchy(includeInactive: true)
                .AsCached();

            Container.BindInterfacesTo<WorldSetupViewController>()
                .FromComponentsInHierarchy(includeInactive: true)
                .AsCached();

            // Panel services (logic layer)
            Container.BindInterfacesAndSelfTo<ContinuePanelService>()
                .AsSingle();

            // Default settings for bot panel (bound as instance so Zenject can inject struct)
            Container.BindInstance(new BotDefaultSettings
            {
                Difficulty = BotDifficulty.Medium,
                Strategy = BotStrategy.Random,
                BotCount = 1,
                AllowBotCheating = false
            }).AsSingle();

            Container.BindInterfacesAndSelfTo<BotPanelService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<SelectedGameModeService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<CreateRoomPanelService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<JoinRoomPanelService>()
                .AsSingle();
        }
    }
}