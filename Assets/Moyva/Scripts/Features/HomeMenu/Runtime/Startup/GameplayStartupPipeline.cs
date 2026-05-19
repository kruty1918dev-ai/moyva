using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Audio.Runtime;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Shared.Common;
using Kruty1918.Moyva.Shared.Graphics;
using Kruty1918.Moyva.Shared.Performance;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime.Startup
{
    /// <summary>
    /// Пайплайн запуску gameplay-сцени з HomeMenu.
    /// Залежності:
    /// - читає конфігурацію сцен із <see cref="HomeMenuConfigSO"/>;
    /// - показує прогрес через <see cref="IOverlayLoader"/>;
    /// - враховує мережевий і світовий контекст з <see cref="IGameplaySession"/>;
    /// - застосовує графічний профіль і prewarm/initializer залежності перед активацією сцени.
    /// </summary>
    internal sealed class GameplayStartupPipeline : IGameplayStartupPipeline
    {
        private const string Prefix = "[GameplayStartupPipeline]";

        /// <summary>Конфігурація сцен і часових затримок запуску.</summary>
        private readonly HomeMenuConfigSO _config;

        /// <summary>Завантажувальний overlay для показу прогресу користувачу.</summary>
        private readonly IOverlayLoader _overlayLoader;

        /// <summary>Підготовлена gameplay-сесія з world/network параметрами.</summary>
        private readonly IGameplaySession _session;

        /// <summary>Сервіс застосування графічного профілю перед стартом сцени.</summary>
        private readonly IGraphicsSettingsService _graphicsSettingsService;

        /// <summary>Опційний prewarm сервіс для прогріву систем.</summary>
        private readonly IStartupPrewarmService _startupPrewarmService;

        /// <summary>Ініціалізатори, які мають відпрацювати до активації сцени.</summary>
        private readonly IScenePreActivationInitializer[] _preActivationInitializers;

        /// <summary>Профіль режиму runtime для gameplay.</summary>
        private readonly ServiceModeProfile _gameplayProfile;

        /// <summary>AsyncOperation завантаження цільової сцени.</summary>
        private AsyncOperation _loadOp;

        /// <summary>Час старту overlay для контролю мінімальної тривалості preload-фази.</summary>
        private float _overlayStartTime;

        /// <summary>Поточний інтерпольований прогрес, що показується користувачу.</summary>
        private float _displayProgress;

        /// <summary>Назва сцени, яку зараз запускає pipeline.</summary>
        private string _sceneName;

        /// <summary>Поточна фаза виконання startup-пайплайна.</summary>
        public GameplayStartupPhase CurrentPhase { get; private set; } = GameplayStartupPhase.None;

        /// <summary>Створити startup-пайплайн для переходу з HomeMenu у gameplay.</summary>
        [Inject]
        public GameplayStartupPipeline(
            HomeMenuConfigSO config,
            IOverlayLoader overlayLoader,
            IGameplaySession session,
            [InjectOptional] IGraphicsSettingsService graphicsSettingsService = null,
            [InjectOptional] IStartupPrewarmService startupPrewarmService = null,
            [InjectOptional] List<IScenePreActivationInitializer> preActivationInitializers = null,
            [InjectOptional] IServiceModeProfileProvider serviceModeProfileProvider = null)
        {
            _config = Guard.NotNull(config, nameof(config));
            _overlayLoader = Guard.NotNull(overlayLoader, nameof(overlayLoader));
            _session = Guard.NotNull(session, nameof(session));
            _graphicsSettingsService = graphicsSettingsService;
            _startupPrewarmService = startupPrewarmService;
            _preActivationInitializers = preActivationInitializers != null ? preActivationInitializers.ToArray() : Array.Empty<IScenePreActivationInitializer>();
            _gameplayProfile = serviceModeProfileProvider?.Get(ServiceRuntimeMode.Gameplay) ?? ServiceModeProfileDefaults.Gameplay;
        }

        /// <summary>
        /// Запустити всі фази переходу в gameplay у фіксованій послідовності.
        /// </summary>
        public async Task RunAsync(CancellationToken ct = default)
        {
            // 1: Перед стартом сцени вирівнюємо графічний профіль під gameplay-режим.
            ApplyGameplayGraphicsPolicy();

            // 2: Фіксуємо назву цільової сцени і скидаємо видимий прогрес overlay.
            _sceneName = _config.gameplaySceneName;
            _displayProgress = 0f;

            // 3: Завантажуємо ресурси та саму сцену до pre-activation стану.
            EnterPhase(GameplayStartupPhase.Preload);
            await ExecutePreloadAsync(ct);

            // 4: Налаштовуємо fallback launch context залежно від режиму гри.
            EnterPhase(GameplayStartupPhase.Bind);
            await ExecuteBindAsync(ct);

            // 5: Виконуємо прогрів і pre-activation ініціалізатори перед фінальним стартом сцени.
            EnterPhase(GameplayStartupPhase.Warmup);
            await ExecuteWarmupAsync(ct);

            // 6: Дозволяємо активацію сцени й чекаємо фактичного завершення завантаження.
            EnterPhase(GameplayStartupPhase.SceneActivate);
            await ExecuteSceneActivateAsync(ct);

            // 7: Позначаємо pipeline завершеним і пишемо фінальний лог успішного запуску.
            EnterPhase(GameplayStartupPhase.Completed);
            Debug.Log($"{Prefix} Scene '{_sceneName}' loaded successfully.");
        }

        /// <summary>Перевести pipeline у нову фазу і, за потреби, залогувати її.</summary>
        private void EnterPhase(GameplayStartupPhase phase)
        {
            // 1: Оновлюємо публічний стан пайплайна.
            CurrentPhase = phase;

            // 2: Логуємо фазу або в verbose-режимі, або при фінальному завершенні процесу.
            if (_gameplayProfile.IsVerboseLogging || phase == GameplayStartupPhase.Completed)
                Debug.Log($"{Prefix} Phase => {phase}");
        }

        /// <summary>Виконати preload-фазу: overlay, ресурси, prewarm і асинхронне завантаження сцени.</summary>
        private async Task ExecutePreloadAsync(CancellationToken ct)
        {
            // 1: Не починаємо роботу, якщо старт уже скасований зовнішнім кодом.
            ct.ThrowIfCancellationRequested();

            // 2: Блокуємо overlay від передчасного закриття і відкриваємо його на нульовому прогресі.
            _overlayLoader.LockOverlay();
            _overlayLoader.LoadOverlay(0f, 100f, "%");

            // 3: Попередньо підвантажуємо базові startup-ресурси, від яких залежить сцена.
            await PreloadStartupResourcesAsync(ct);
            _overlayLoader.UpdateOverlay(8f, 100f, "%");

            // 4: За наявності окремого prewarm сервісу даємо йому прогріти системи до активації сцени.
            if (_startupPrewarmService != null)
                await _startupPrewarmService.PrewarmAsync(ct);
            _overlayLoader.UpdateOverlay(15f, 100f, "%");

            // 5: Починаємо асинхронне завантаження gameplay-сцени в Single-режимі.
            _loadOp = SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Single);
            if (_loadOp == null)
            {
                throw new InvalidOperationException(
                    $"{Prefix} SceneManager returned null for '{_sceneName}'. Ensure the scene is in Build Settings.");
            }

            // 6: Зупиняємо автоматичну активацію, щоб встигнути виконати bind/warmup фази.
            _loadOp.allowSceneActivation = false;

            // 7: Запам'ятовуємо час старту preload-фази для контролю мінімального UX-інтервалу.
            _overlayStartTime = Time.realtimeSinceStartup;

            // 8: Чекаємо, доки Unity довантажить сцену до порогу pre-activation.
            await WaitForScenePreloadAsync(ct);
        }

        /// <summary>Дочекатися завантаження сцени до межі 0.9 і синхронізувати з цим overlay.</summary>
        private async Task WaitForScenePreloadAsync(CancellationToken ct)
        {
            // 1: Unity тримає прогрес на 0.9, поки не дозволена активація сцени.
            while (_loadOp.progress < 0.9f)
            {
                // 2: Даємо можливість скасувати завантаження між ітераціями циклу.
                ct.ThrowIfCancellationRequested();

                // 3: Нормалізуємо unity-progress у діапазон 0..1 для відображення користувачу.
                float target = _loadOp.progress / 0.9f;

                // 4: Згладжуємо прогрес, щоб overlay не рухався ривками.
                _displayProgress = Mathf.Lerp(_displayProgress, target, 0.15f);

                // 5: Оновлюємо числове значення overlay у відсотках.
                _overlayLoader.UpdateOverlay(Mathf.Round(_displayProgress * 100f), 100f, "%");

                // 6: Віддаємо керування Unity/main loop до наступного кадру.
                await Task.Yield();
            }

            // 7: Вимірюємо фактичний час preload, щоб не закрити фазу надто швидко для користувача.
            float elapsed = Time.realtimeSinceStartup - _overlayStartTime;
            if (elapsed < _config.minPreloadSeconds)
            {
                // 8: Дотримуємо мінімальної тривалості preload-фази, заданої конфігурацією.
                int remainMs = Mathf.RoundToInt((_config.minPreloadSeconds - elapsed) * 1000f);
                await Task.Delay(remainMs, ct);
            }
        }

        /// <summary>Налаштувати GameLaunchContext перед фактичною активацією сцени.</summary>
        private async Task ExecuteBindAsync(CancellationToken ct)
        {
            // 1: Поважаємо скасування перед модифікацією глобального launch-контексту.
            ct.ThrowIfCancellationRequested();

            // 2: Якщо контекст уже налаштовано іншим шляхом, не перетираємо його fallback-логікою меню.
            if (GameLaunchContext.Mode != GameLaunchMode.Unknown &&
                GameLaunchContext.Mode != GameLaunchMode.DirectGameplayTest)
            {
                return;
            }

            // 3: Окремо готуємо офлайн-нову гру для локального хоста.
            if (_session.IsHost && _session.Mode == NetworkProviderType.Offline)
            {
                ConfigureOfflineNewGameFallback();
            }
            else
            {
                // 4: В усіх інших випадках стартуємо як multiplayer-derived launch context.
                ConfigureMultiplayerFallback();
            }

            // 5: Метод зберігає асинхронний контракт, хоча робота тут фактично синхронна.
            await Task.CompletedTask;
        }

        /// <summary>Виконати прогрів pre-activation систем і затримку перед активацією сцени.</summary>
        private async Task ExecuteWarmupAsync(CancellationToken ct)
        {
            // 1: Перевіряємо скасування перед обходом усіх pre-activation ініціалізаторів.
            ct.ThrowIfCancellationRequested();

            // 2: Послідовно запускаємо кожен ініціалізатор, щоб забезпечити передбачуваний порядок підготовки.
            for (int i = 0; i < _preActivationInitializers.Length; i++)
            {
                ct.ThrowIfCancellationRequested();

                // 3: Пропускаємо null-елементи списку, щоб DI-конфігурація залишалась толерантною.
                if (_preActivationInitializers[i] != null)
                    await _preActivationInitializers[i].InitializeBeforeActivationAsync(ct);
            }

            // 4: Після warmup показуємо на overlay повну готовність до активації сцени.
            _overlayLoader.UpdateOverlay(100f, 100f, "%");

            // 5: Опційно даємо невелику паузу перед активацією, щоб згладити UX-перехід.
            if (_config.sceneActivationDelay > 0f)
            {
                await Task.Delay(Mathf.RoundToInt(_config.sceneActivationDelay * 1000f), ct);
            }
        }

        /// <summary>Дозволити активацію сцени й дочекатися її фінального завершення.</summary>
        private async Task ExecuteSceneActivateAsync(CancellationToken ct)
        {
            // 1: Поважаємо скасування до моменту фактичного показу сцени.
            ct.ThrowIfCancellationRequested();

            // 2: Даємо Unity дозвіл активувати вже попередньо завантажену сцену.
            _loadOp.allowSceneActivation = true;

            // 3: Чекаємо, доки Unity повністю закінчить асинхронну операцію.
            while (!_loadOp.isDone)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Yield();
            }
        }

        /// <summary>Попередньо завантажити ресурси, потрібні на ранній фазі старту gameplay.</summary>
        private static async Task PreloadStartupResourcesAsync(CancellationToken ct)
        {
            // 1: Стартуємо асинхронне завантаження графічних startup-налаштувань.
            ResourceRequest graphicsRequest = Resources.LoadAsync<GraphicsStartupSettingsSO>(GraphicsStartupSettingsSO.DefaultResourcePath);

            // 2: Паралельно підвантажуємо audio registry, щоб уникнути пізніх затримок після активації сцени.
            ResourceRequest audioRequest = Resources.LoadAsync<AudioRegistrySO>("MoyvaAudioRegistry");

            // 3: Чекаємо, доки обидва ресурси будуть повністю готові.
            while (!graphicsRequest.isDone || !audioRequest.isDone)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Yield();
            }

            // 4: Доторкаємося до asset'ів, щоб зафіксувати факт завершеного завантаження та уникнути попереджень про невикористання.
            _ = graphicsRequest.asset;
            _ = audioRequest.asset;
        }

        /// <summary>За потреби застосувати gameplay-графічний профіль перед стартом сцени.</summary>
        private void ApplyGameplayGraphicsPolicy()
        {
            // 1: Якщо графічний сервіс відсутній або профіль вимкнений, нічого не змінюємо.
            if (_graphicsSettingsService == null || !_gameplayProfile.ApplyGraphicsProfile)
                return;

            // 2: Зчитуємо поточний профіль користувача.
            var current = _graphicsSettingsService.Settings.Profile;

            // 3: Поважаємо custom-профіль, якщо policy вимагає не перезаписувати його автоматично.
            if (_gameplayProfile.RespectCustomGraphicsProfile && current == GraphicsQualityProfile.Custom)
                return;

            // 4: Перемикаємо профіль лише тоді, коли він відрізняється від цільового gameplay-профілю.
            if (current != _gameplayProfile.GraphicsProfile)
                _graphicsSettingsService.SetProfile(_gameplayProfile.GraphicsProfile);
        }

        /// <summary>Налаштувати fallback launch context для офлайн нової гри.</summary>
        private void ConfigureOfflineNewGameFallback()
        {
            // 1: Беремо актуальні world settings із підготовленої gameplay-сесії.
            var settings = _session.WorldSettings;

            // 2: Записуємо всі ключові параметри нової гри до глобального launch context.
            GameLaunchContext.ConfigureMenuNewGame(
                0,
                settings.WorldName,
                settings.Seed,
                settings.Size,
                (int)settings.MapType,
                (int)settings.Difficulty,
                settings.MaxPlayers,
                settings.IsPrivate,
                settings.Width,
                settings.Height);
        }

        /// <summary>Налаштувати fallback launch context для multiplayer-старту.</summary>
        private void ConfigureMultiplayerFallback()
        {
            // 1: Беремо актуальні world settings із gameplay-сесії, синхронізованої між учасниками.
            var settings = _session.WorldSettings;

            // 2: Записуємо multiplayer-параметри в launch context для подальшого підняття gameplay-сцени.
            GameLaunchContext.ConfigureMenuMultiplayerGame(
                settings.WorldName,
                settings.Seed,
                settings.Size,
                (int)settings.MapType,
                (int)settings.Difficulty,
                settings.MaxPlayers,
                settings.IsPrivate,
                settings.Width,
                settings.Height);
        }
    }
}
