using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Zenject-інсталер сцени HomeMenu. Реєструє всі сервіси HomeMenu та
    /// декларує додаткові сигнали.
    ///
    /// ЗАЛЕЖНОСТІ:
    ///   - SignalBusInstaller       — має бути в списку інсталерів.
    ///   - SaveSystemInstaller      — надає ISaveService та IConfigService.
    ///   - WorldCreationInstaller   — надає IWorldCreationService.
    ///   - WorldCreationUIInstaller — надає WorldCreationUIController (ініціалізує UI світу).
    /// </summary>
    public sealed class HomeMenuInstaller : MonoInstaller
    {
        [Header("Конфігурація")]
        [Tooltip("Головний SO з параметрами меню (сцени, прелоад).")]
        [SerializeField] private HomeMenuConfigSO config;

        [Tooltip("Список соціальних посилань для відображення у налаштуваннях.")]
        [SerializeField] private SocialLinksConfigSO socialLinks;

        [Tooltip("Прив'язки AudioMixer для керування звуком. Якщо null, зберігається лише у PlayerPrefs.")]
        [SerializeField] private AudioMixerBindingsSO audioBindings;

        [Header("Сцена")]
        [Tooltip("Кореневий view з панелями (Main/WorldCreation/Settings/Loading/Confirm).")]
        [SerializeField] private HomeMenuRootView rootView;

        [Tooltip("Компонент HomeMenuFlow (MonoBehaviour) на сцені.")]
        [SerializeField] private HomeMenuFlow flow;

        [Tooltip("Компонент SceneLoadService (MonoBehaviour) на сцені.")]
        [SerializeField] private SceneLoadService sceneLoader;

        public override void InstallBindings()
        {
            if (config == null || socialLinks == null || audioBindings == null ||
                rootView == null || flow == null || sceneLoader == null)
            {
                Debug.LogError($"[{nameof(HomeMenuInstaller)}] Не всі поля інсталера заповнені. " +
                               "Перевір інспектор.", this);
                return;
            }

            // SO-залежності як інстанси.
            Container.BindInstance(config);
            Container.BindInstance(socialLinks);
            Container.BindInstance(audioBindings);
            Container.BindInstance(rootView);

            // Сервіси.
            Container.Bind<IAudioSettingsService>()
                .To<AudioSettingsService>()
                .AsSingle();

            Container.Bind<IUserDataService>()
                .To<UserDataService>()
                .AsSingle();

            Container.Bind<ISocialLinksService>()
                .To<SocialLinksService>()
                .AsSingle();

            Container.Bind<ISceneLoadService>()
                .FromInstance(sceneLoader)
                .AsSingle();

            Container.BindInterfacesAndSelfTo<HomeMenuFlow>()
                .FromInstance(flow)
                .AsSingle();

            Container.BindInterfacesTo<WorldLaunchService>()
                .AsSingle()
                .NonLazy();

            // Власні сигнали HomeMenu.
            Container.DeclareSignal<HomeMenuReadySignal>().OptionalSubscriber();
            Container.DeclareSignal<HomeMenuStartRequestedSignal>().OptionalSubscriber();
            Container.DeclareSignal<HomeMenuQuitRequestedSignal>().OptionalSubscriber();
            Container.DeclareSignal<UserDataClearedSignal>().OptionalSubscriber();
        }
    }
}
