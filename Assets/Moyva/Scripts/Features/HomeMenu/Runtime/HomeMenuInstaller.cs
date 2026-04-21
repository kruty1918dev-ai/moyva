using System.Collections.Generic;
using System.Text;
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

        [Tooltip("Контролер Create/Join/List для мультиплеєрної частини меню.")]
        [SerializeField] private MultiplayerMenuPanelController multiplayerMenuController;

        public override void InstallBindings()
        {
            var errors = new List<string>();

            // ── Конфігураційні SO ─────────────────────────────────────────
            if (config == null)        errors.Add("Поле 'config' (HomeMenuConfigSO) не призначено.");
            if (socialLinks == null)   errors.Add("Поле 'socialLinks' (SocialLinksConfigSO) не призначено.");
            if (audioBindings == null) errors.Add("Поле 'audioBindings' (AudioMixerBindingsSO) не призначено.");

            // ── Сценові компоненти (авто-пошук + створення де можливо) ───
            EnsureSceneReferences(errors);

            if (errors.Count > 0)
            {
                var sb = new StringBuilder();
                sb.Append('[').Append(nameof(HomeMenuInstaller)).Append("] Сцену HomeMenu налаштовано некоректно. ")
                  .Append("Список проблем (").Append(errors.Count).AppendLine("):");
                for (int i = 0; i < errors.Count; i++)
                    sb.Append("  ").Append(i + 1).Append(". ").AppendLine(errors[i]);
                Debug.LogError(sb.ToString(), this);
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

        private void EnsureSceneReferences(List<string> errors)
        {
            // HomeMenuRootView: критично для WorldLaunchService. НЕ створюється авто —
            // вимагає серіалізованих посилань на панелі/оверлеї з ієрархії сцени.
            if (rootView == null)
                rootView = Object.FindFirstObjectByType<HomeMenuRootView>();
            if (rootView == null)
                errors.Add("На сцені відсутній компонент HomeMenuRootView. " +
                           "Додай його до кореневого GameObject UI меню та признач посилання на панелі (Main/WorldCreation/Settings) і оверлеї (Loading/Confirm).");

            // SceneLoadService: MonoBehaviour без серіалізованих UI-полів — безпечно створити.
            if (sceneLoader == null)
            {
                sceneLoader = Object.FindFirstObjectByType<SceneLoadService>();
                if (sceneLoader == null)
                {
                    var loaderGo = new GameObject("SceneLoadService");
                    sceneLoader = loaderGo.AddComponent<SceneLoadService>();
                }
            }

            // HomeMenuFlow: MonoBehaviour без обов'язкових серіалізованих полів — безпечно створити.
            if (flow == null)
            {
                flow = Object.FindFirstObjectByType<HomeMenuFlow>();
                if (flow == null)
                {
                    var flowGo = new GameObject("HomeMenuFlow");
                    flow = flowGo.AddComponent<HomeMenuFlow>();
                }
            }

            // MultiplayerMenuPanelController: auto-find + fallback create.
            if (multiplayerMenuController == null)
            {
                multiplayerMenuController = Object.FindFirstObjectByType<MultiplayerMenuPanelController>();
                if (multiplayerMenuController == null)
                {
                    var host = Object.FindFirstObjectByType<HomeMenuNavigationController>();
                    if (host != null)
                        multiplayerMenuController = host.gameObject.AddComponent<MultiplayerMenuPanelController>();
                    else
                    {
                        var controllerGo = new GameObject("MultiplayerMenuPanelController");
                        multiplayerMenuController = controllerGo.AddComponent<MultiplayerMenuPanelController>();
                    }
                }
            }
        }
    }
}
