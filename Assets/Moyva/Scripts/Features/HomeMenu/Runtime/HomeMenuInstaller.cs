using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime.Services;
using Kruty1918.Moyva.HomeMenu.Runtime.Startup;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Multiplayer.Runtime;
using Kruty1918.Moyva.WorldCreation.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Zenject MonoInstaller для домашнього меню.
    /// Підключіть у сцені з домашнім меню для реєстрації всіх його сервісів.
    /// </summary>
    public sealed class HomeMenuInstaller : MonoInstaller
    {
        [SerializeField] private string _lobbyPanelName = "LobbyPanel";
        [SerializeField] private string _worldSetupPanelName = "WorldSetupPanel";
        [SerializeField] private string _createRoomPanelName = "CreateRoomPanel";
        [SerializeField] private string _joinRoomPanelName = "JoinRoomPanel";
        [SerializeField] private string _kickPlayerPanelName = "KickPlayerPanel";
        [SerializeField] private string _infoPanelName = "InfoPanel";
        [SerializeField] private string _multiplayerTypePanelName = "SelectMultiplayerType";
        [SerializeField] private AudioMixerBindingsSO _audioMixerBindings;
        [SerializeField] private HomeMenuConfigSO _config;
        [SerializeField] private WorldCreationDefaultsSO _worldCreationDefaults;
        [Header("Navigation")]
        [Tooltip("Names of panels that require confirmation when the player navigates back from them.")]
        [SerializeField] private string[] _confirmOnBackMenuNames = new string[0];
        [Header("Menu Reveal")]
        [Tooltip("Плавне проявлення головного меню при вході в сцену.")]
        [SerializeField] private HomeMenuRevealFadeSettings _menuRevealFade = new();

        public override void InstallBindings()
        {
            HomeMenuRuntimeUiFactory.EnsureRequiredPanels(_infoPanelName);

            var menuRevealFade = _menuRevealFade ?? new HomeMenuRevealFadeSettings();
            Container.BindInstance(menuRevealFade).AsSingle();

            Container.BindInterfacesTo<HomeMenuRevealOverlayService>()
                .AsSingle()
                .NonLazy();

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

            Container.Bind<JoinRoomOpenButton>()
                .FromComponentsInHierarchy(includeInactive: true)
                .AsCached();

            Container.BindInterfacesTo<PlayerNameTextComponent>()
                .FromComponentsInHierarchy(includeInactive: true)
                .AsCached();

            Container.BindInterfacesAndSelfTo<IConfirmationButton>()
             .FromComponentsInHierarchy(includeInactive: true)
                .AsCached();

            // Сервіси 
            Container.BindInterfacesAndSelfTo<ConformationService>()
            .AsSingle();

            Container.BindInterfacesAndSelfTo<LocalGameSettingsService>()
                .AsSingle();

            if (_audioMixerBindings != null)
                Container.BindInstance(_audioMixerBindings).AsSingle();

            if (_worldCreationDefaults != null)
                Container.BindInstance(_worldCreationDefaults).AsSingle();

            var config = _config != null ? _config : ScriptableObject.CreateInstance<HomeMenuConfigSO>();
            Container.BindInstance(config).AsSingle().IfNotBound();

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

            Container.BindInterfacesTo<LobbyPanelViewController>()
                .FromComponentInHierarchy(includeInactive: true)
                .AsCached();

            Container.BindInterfacesTo<KickPlayerPanelViewController>()
                .FromComponentInHierarchy(includeInactive: true)
                .AsCached();

            Container.BindInterfacesTo<JoinRoomViewController>()
                .FromComponentsInHierarchy(includeInactive: true)
                .AsCached();

            // ── Інформаційна панель (модальне OK-повідомлення)
            Container.BindInterfacesTo<InfoPanelViewController>()
                .FromComponentInHierarchy(includeInactive: true)
                .AsSingle();

            // ── Панель введення пароля для приватних кімнат
            Container.BindInterfacesTo<PasswordPanelViewController>()
                .FromComponentInHierarchy(includeInactive: true)
                .AsSingle();

            Container.BindInterfacesTo<GameSettingsViewController>()
                .FromComponentsInHierarchy(includeInactive: true)
                .AsCached();

            Container.BindInterfacesTo<MultiplayerViewController>()
                .FromComponentsInHierarchy(includeInactive: true)
                .AsCached();

            Container.BindInterfacesTo<MultiplayerModeViewController>()
                .FromComponentsInHierarchy(includeInactive: true)
                .AsCached();

            Container.BindInterfacesTo<SoloViewController>()
                .FromComponentsInHierarchy(includeInactive: true)
                .AsCached();

            Container.BindInterfacesTo<WorldSetupViewController>()
                .FromComponentsInHierarchy(includeInactive: true)
                .AsCached();

            // Panel services (logic layer)
            Container.BindInterfacesAndSelfTo<JoinRoomUiGateway>()
                .AsSingle();

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

            Container.BindInterfacesAndSelfTo<SoloPanelService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<MultiplayerPanelService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<CreateRoomPanelService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<JoinRoomPanelService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<MultiplayerModePanelService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<MultiplayerMenuModeService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<LobbyPanelService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<KickPlayerPanelService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<GameplaySession>()
                .AsSingle();

            Container.BindInterfacesTo<MenuApi>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<HomeMenuGameStarter>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<GameplayStartupPipeline>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<GameStartListenerService>()
                .AsSingle();

            // Сервіс інформаційної панелі (черга OK-повідомлень).
            Container.BindInterfacesAndSelfTo<InfoPanelService>()
                .AsSingle();

            // Дренує HostDisconnectNotice при завантаженні HomeMenu сцени.
            Container.BindInterfacesAndSelfTo<HostDisconnectNoticePresenter>()
                .AsSingle()
                .NonLazy();

            // Сервіс запиту пароля для приватних кімнат.
            Container.BindInterfacesAndSelfTo<PasswordPanelService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<GameSettingsPanelService>()
                .AsSingle();

            Container.BindInstance(_lobbyPanelName)
                .WithId("LobbyPanelName");

            Container.BindInstance(_worldSetupPanelName)
                .WithId("WorldSetupPanelName");

            Container.BindInstance(_createRoomPanelName)
                .WithId("CreateRoomPanelName");

            Container.BindInstance(_joinRoomPanelName)
                .WithId("JoinRoomPanelName");

            Container.BindInstance(_kickPlayerPanelName)
                .WithId("KickPlayerPanelName");

            Container.BindInstance(_infoPanelName)
                .WithId("InfoPanelName");

            Container.BindInstance(_multiplayerTypePanelName)
                .WithId("MultiplayerTypePanelName");

            // Сторожовий сервіс інтернет-з'єднання — викидає на fallback-панель при втраті мережі в onlineRelay/WS режимах.
            Container.BindInterfacesAndSelfTo<ConnectivityWatchdogService>()
                .AsSingle();

            // Bind confirm-on-back menu names so HomeMenuNavigation can ask for confirmation.
            Container.BindInstance(_confirmOnBackMenuNames).AsSingle();

            Container.BindInterfacesAndSelfTo<WorldCreationPanelService>()
                .AsSingle();
        }
    }
}