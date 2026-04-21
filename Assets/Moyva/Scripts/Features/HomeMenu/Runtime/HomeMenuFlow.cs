using System;
using System.Collections;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Центральний оркестратор HomeMenu: керує показом панелей, запускає прелоад
    /// при старті сцени, підписується на системні сигнали.
    ///
    /// Створюється через <see cref="HomeMenuInstaller"/> як <see cref="IHomeMenuFlow"/> +
    /// <see cref="IInitializable"/>.
    /// </summary>
    public sealed class HomeMenuFlow : MonoBehaviour, IHomeMenuFlow, IInitializable, IDisposable
    {
        [Tooltip("Кореневий обгортковий view з посиланнями на усі панелі.")]
        [SerializeField] private HomeMenuRootView rootView;

        private HomeMenuConfigSO _config;
        private IAudioSettingsService _audio;
        private ISceneLoadService _sceneLoader;
        private SignalBus _signalBus;

        private HomeMenuPanel _currentPanel = HomeMenuPanel.Loading;

        /// <inheritdoc/>
        public HomeMenuPanel CurrentPanel => _currentPanel;

        /// <inheritdoc/>
        public event Action<HomeMenuPanel> PanelChanged;

        [Inject]
        internal void Construct(
            HomeMenuConfigSO config,
            IAudioSettingsService audio,
            ISceneLoadService sceneLoader,
            SignalBus signalBus)
        {
            _config      = config      ?? throw new ArgumentNullException(nameof(config));
            _audio       = audio       ?? throw new ArgumentNullException(nameof(audio));
            _sceneLoader = sceneLoader ?? throw new ArgumentNullException(nameof(sceneLoader));
            _signalBus   = signalBus   ?? throw new ArgumentNullException(nameof(signalBus));
        }

        /// <summary>Викликається Zenject-ом після всіх ін'єкцій.</summary>
        public void Initialize()
        {
            if (rootView == null)
            {
                Debug.LogError($"[{nameof(HomeMenuFlow)}] rootView не призначено.", this);
                return;
            }

            // Крок 1. Завантажуємо аудіо-налаштування, щоб UI показував коректні значення.
            _audio.Load();

            // Крок 2. Панель завантаження одразу видима.
            SetPanel(HomeMenuPanel.Loading, forceRefresh: true);

            // Крок 3. Мінімальний прелоад, потім показуємо Main.
            StartCoroutine(PreloadThenShowMain());

            _signalBus.Subscribe<WorldCreationCancelledSignal>(OnWorldCreationCancelled);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _signalBus?.TryUnsubscribe<WorldCreationCancelledSignal>(OnWorldCreationCancelled);
        }

        // ── Public API ───────────────────────────────────────────────────

        /// <inheritdoc/>
        public void ShowMain()          => SetPanel(HomeMenuPanel.Main);
        /// <inheritdoc/>
        public void ShowWorldCreation() => SetPanel(HomeMenuPanel.WorldCreation);
        /// <inheritdoc/>
        public void ShowSettings()      => SetPanel(HomeMenuPanel.Settings);

        /// <inheritdoc/>
        public void ShowConfirm(string title, string message, Action onConfirm, Action onCancel = null)
        {
            if (rootView == null || rootView.ConfirmDialog == null)
            {
                Debug.LogError($"[{nameof(HomeMenuFlow)}] ConfirmDialogView не призначено.", this);
                onCancel?.Invoke();
                return;
            }
            rootView.ConfirmDialog.Show(title, message,
                onConfirm: () =>
                {
                    rootView.ConfirmDialog.Hide();
                    SetPanel(_currentPanel == HomeMenuPanel.Confirm ? HomeMenuPanel.Main : _currentPanel);
                    onConfirm?.Invoke();
                },
                onCancel: () =>
                {
                    rootView.ConfirmDialog.Hide();
                    SetPanel(_currentPanel == HomeMenuPanel.Confirm ? HomeMenuPanel.Main : _currentPanel);
                    onCancel?.Invoke();
                });
            _currentPanel = HomeMenuPanel.Confirm;
            PanelChanged?.Invoke(_currentPanel);
        }

        /// <inheritdoc/>
        public void RequestQuit()
        {
            ShowConfirm(
                title:   "Вийти з гри?",
                message: "Всі незбережені зміни буде втрачено.",
                onConfirm: () =>
                {
                    _signalBus.TryFire(new HomeMenuQuitRequestedSignal());
                    _sceneLoader.QuitApplication();
                });
        }

        // ── Internals ────────────────────────────────────────────────────

        private IEnumerator PreloadThenShowMain()
        {
            float elapsed = 0f;
            float min = Mathf.Max(0f, _config.MinPreloadSeconds);
            while (elapsed < min)
            {
                float t = min > 0f ? Mathf.Clamp01(elapsed / min) : 1f;
                rootView.LoadingOverlay?.SetProgress(t, "Завантаження…");
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            rootView.LoadingOverlay?.SetProgress(1f, "Готово");
            SetPanel(HomeMenuPanel.Main);
            _signalBus.TryFire(new HomeMenuReadySignal());
        }

        private void OnWorldCreationCancelled(WorldCreationCancelledSignal _)
        {
            ShowMain();
        }

        private void SetPanel(HomeMenuPanel panel, bool forceRefresh = false)
        {
            if (!forceRefresh && _currentPanel == panel) return;
            _currentPanel = panel;
            if (rootView != null)
                rootView.ApplyPanelState(panel);
            PanelChanged?.Invoke(panel);
        }

        /// <summary>
        /// Повертає посилання на loader для UI-кнопок, які ініціюють завантаження сцен
        /// (Start → Gameplay). Використовуйте через DI, не збирайте у код панелей вручну.
        /// </summary>
        public ISceneLoadService SceneLoader => _sceneLoader;

        /// <summary>
        /// Повертає root view (доступ з UI-контролерів без додаткових ін'єкцій).
        /// </summary>
        public HomeMenuRootView RootView => rootView;
    }
}
