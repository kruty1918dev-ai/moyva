using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime.Services;
using Kruty1918.Moyva.HomeMenu.Runtime.Startup;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Jsonization;
using Kruty1918.Moyva.Multiplayer.Runtime;
using Kruty1918.Moyva.WorldCreation.API;
using UnityEngine;
using UnityHTML.Runtime;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
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
        [SerializeField] private HomeMenuRevealFadeSettings _menuRevealFade = new();

        public override void InstallBindings()
        {
            var config = _config != null ? _config : MoyvaJsonObjectFactory.Create<HomeMenuConfigSO>();
            var useMoyvaUi = config != null && config.useUnityHtmlShell;

            if (!useMoyvaUi)
                HomeMenuRuntimeUiFactory.EnsureRequiredPanels(_infoPanelName);

            MenuWorldPreviewKingdomPlacementFeatureBindings.Install(Container);
            MenuWorldPreviewTextureBuilderFeatureBindings.Install(Container);

            Container.BindInstance(_menuRevealFade ?? new HomeMenuRevealFadeSettings()).AsSingle();
            Container.BindInstance(config).AsSingle().IfNotBound();

            Container.BindInterfacesTo<HomeMenuRevealOverlayService>().AsSingle().NonLazy();
            Container.Bind<INavigation>().To<HomeMenuNavigation>().AsSingle();
            Container.BindInterfacesAndSelfTo<HomeMenuInitializer>().AsSingle();

            BindSharedSceneUi();
            BindCoreServices();
            BindUiLayer(useMoyvaUi);
            BindPanelServices();
            BindPanelNames();

            Container.BindInterfacesAndSelfTo<ConnectivityWatchdogService>().AsSingle();
            Container.BindInterfacesAndSelfTo<WorldCreationPanelService>().AsSingle();
        }

        private void BindSharedSceneUi()
        {
            Container.BindInterfacesTo<PlayerNameTextComponent>()
                .FromComponentsInHierarchy(includeInactive: true)
                .AsCached();

            Container.BindInterfacesTo<ConfirmButton>()
                .FromComponentsInHierarchy(includeInactive: true)
                .AsCached();
        }

        private void BindCoreServices()
        {
            Container.BindInterfacesAndSelfTo<ConformationService>().AsSingle();
            Container.BindInterfacesAndSelfTo<LocalGameSettingsService>().AsSingle();
            Container.BindInterfacesAndSelfTo<AudioSettingsRuntimeSyncService>().AsSingle();

            if (_audioMixerBindings != null)
                Container.BindInstance(_audioMixerBindings).AsSingle();

            if (_worldCreationDefaults != null)
                Container.BindInstance(_worldCreationDefaults).AsSingle();

            Container.Bind<IUnityHtmlHost>().To<UnityHtmlHost>().AsSingle();
        }

        private void BindUiLayer(bool useMoyvaUi)
        {
            if (useMoyvaUi)
            {
                Container.Bind<HomeMenuMoyvaUiState>().AsSingle();

                Container.BindInterfacesAndSelfTo<HomeMenuMoyvaUiViewController>()
                    .AsSingle();

                BindMoyvaUiPanel("PlayModePanel");
                BindMoyvaUiPanel("ContinuePanel");
                BindMoyvaUiPanel(_multiplayerTypePanelName);
                BindMoyvaUiPanel("MultiplayerPanel");
                BindMoyvaUiPanel("SettingsPanel");
                BindMoyvaUiPanel(_createRoomPanelName);
                BindMoyvaUiPanel(_joinRoomPanelName);
                BindMoyvaUiPanel(_worldSetupPanelName);
                BindMoyvaUiPanel(_lobbyPanelName);
                BindMoyvaUiPanel(_kickPlayerPanelName);
                BindMoyvaUiPanel(_infoPanelName);

                Container.Bind<HomeMenuMoyvaUiAnchor>()
                    .FromComponentsInHierarchy(includeInactive: true)
                    .AsCached();

                Container.BindInterfacesAndSelfTo<HomeMenuMoyvaUiPresenter>()
                    .AsSingle()
                    .NonLazy();
                return;
            }

            Container.Bind<IOverlayLoader>().To<HomeMenuOverlayLoader>().AsSingle();
            Container.Bind<OverlayPanelLoader>().FromComponentInHierarchy(includeInactive: true).AsSingle();
            Container.Bind<IConfiremationPanel>().To<ConfirmationPanel>().FromComponentInHierarchy(includeInactive: true).AsSingle();

            Container.BindInterfacesTo<NavigationPanel>().FromComponentsInHierarchy(includeInactive: true).AsCached();
            Container.Bind<NavigationButton>().FromComponentsInHierarchy(includeInactive: true).AsCached();
            Container.Bind<JoinRoomOpenButton>().FromComponentsInHierarchy(includeInactive: true).AsCached();

            Container.Bind<HomeMenuHtmlShellAnchor>()
                .FromComponentsInHierarchy(includeInactive: true)
                .AsCached();

            Container.BindInterfacesAndSelfTo<HomeMenuHtmlShellPresenter>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesTo<ContinueViewController>().FromComponentsInHierarchy(includeInactive: true).AsCached();
            Container.BindInterfacesTo<CreateRoomViewController>().FromComponentsInHierarchy(includeInactive: true).AsCached();
            Container.BindInterfacesTo<LobbyPanelViewController>().FromComponentInHierarchy(includeInactive: true).AsCached();
            Container.BindInterfacesTo<KickPlayerPanelViewController>().FromComponentInHierarchy(includeInactive: true).AsCached();
            Container.BindInterfacesTo<JoinRoomViewController>().FromComponentsInHierarchy(includeInactive: true).AsCached();
            Container.BindInterfacesTo<InfoPanelViewController>().FromComponentInHierarchy(includeInactive: true).AsSingle();
            Container.BindInterfacesTo<PasswordPanelViewController>().FromComponentInHierarchy(includeInactive: true).AsSingle();
            Container.BindInterfacesTo<GameSettingsViewController>().FromComponentsInHierarchy(includeInactive: true).AsCached();
            Container.BindInterfacesTo<MultiplayerViewController>().FromComponentsInHierarchy(includeInactive: true).AsCached();
            Container.BindInterfacesTo<MultiplayerModeViewController>().FromComponentsInHierarchy(includeInactive: true).AsCached();
            Container.BindInterfacesTo<WorldSetupViewController>().FromComponentsInHierarchy(includeInactive: true).AsCached();
        }

        private void BindPanelServices()
        {
            Container.BindInterfacesAndSelfTo<JoinRoomUiGateway>().AsSingle();
            Container.BindInterfacesAndSelfTo<ContinuePanelService>().AsSingle();
            Container.BindInterfacesAndSelfTo<MultiplayerPanelService>().AsSingle();
            Container.BindInterfacesAndSelfTo<LobbyFlowContext>().AsSingle();
            Container.BindInterfacesAndSelfTo<CreateRoomPanelService>().AsSingle();
            Container.BindInterfacesAndSelfTo<JoinRoomPanelService>().AsSingle();
            Container.BindInterfacesAndSelfTo<MultiplayerModePanelService>().AsSingle();
            Container.BindInterfacesAndSelfTo<MultiplayerMenuModeService>().AsSingle();
            Container.BindInterfacesAndSelfTo<LobbyPanelService>().AsSingle();
            Container.BindInterfacesAndSelfTo<KickPlayerPanelService>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameplaySession>().AsSingle();
            Container.BindInterfacesTo<MenuApi>().AsSingle();
            Container.BindInterfacesAndSelfTo<HomeMenuGameStarter>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameplayStartupPipeline>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameStartListenerService>().AsSingle();
            Container.BindInterfacesAndSelfTo<InfoPanelService>().AsSingle();
            Container.BindInterfacesAndSelfTo<HostDisconnectNoticePresenter>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PasswordPanelService>().AsSingle();
            Container.BindInterfacesAndSelfTo<GameSettingsPanelService>().AsSingle();
        }

        private void BindPanelNames()
        {
            Container.BindInstance(_lobbyPanelName).WithId("LobbyPanelName");
            Container.BindInstance(_worldSetupPanelName).WithId("WorldSetupPanelName");
            Container.BindInstance(_createRoomPanelName).WithId("CreateRoomPanelName");
            Container.BindInstance(_joinRoomPanelName).WithId("JoinRoomPanelName");
            Container.BindInstance(_kickPlayerPanelName).WithId("KickPlayerPanelName");
            Container.BindInstance(_infoPanelName).WithId("InfoPanelName");
            Container.BindInstance(_multiplayerTypePanelName).WithId("MultiplayerTypePanelName");
            Container.BindInstance(_confirmOnBackMenuNames).AsSingle();
        }

        private void BindMoyvaUiPanel(string panelName)
        {
            if (string.IsNullOrWhiteSpace(panelName))
                return;

            Container.Bind<INavigationPanel>()
                .To<HomeMenuMoyvaUiNavigationPanel>()
                .AsCached()
                .WithArguments(panelName.Trim());
        }
    }
}
