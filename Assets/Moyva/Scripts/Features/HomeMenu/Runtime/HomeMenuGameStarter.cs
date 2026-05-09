using System;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Реалізація <see cref="IHomeMenuGameStarter"/> — завантажує ігрову сцену з меню.
    /// Налаштовує <see cref="GameLaunchContext"/> на основі <see cref="IGameplaySession.IsHost"/>
    /// і відображає прогрес через <see cref="IOverlayLoader"/>.
    /// </summary>
    internal sealed class HomeMenuGameStarter : IHomeMenuGameStarter
    {
        private const string Prefix = "[HomeMenuGameStarter]";

        private readonly HomeMenuConfigSO _config;
        private readonly IOverlayLoader _overlayLoader;
        private readonly IGameplaySession _session;

        private bool _isStarting;

        [Inject]
        public HomeMenuGameStarter(HomeMenuConfigSO config, IOverlayLoader overlayLoader, IGameplaySession session)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _overlayLoader = overlayLoader ?? throw new ArgumentNullException(nameof(overlayLoader));
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task StartGameAsync(CancellationToken ct = default)
        {
            if (_isStarting)
            {
                Debug.LogWarning($"{Prefix} StartGameAsync викликано повторно — ігнорування.");
                return;
            }

            _isStarting = true;
            try
            {
                await RunAsync(ct);
            }
            catch (OperationCanceledException)
            {
                _overlayLoader.UnlockOverlay();
                _overlayLoader.StopOverlay(forceImmediate: true);
                Debug.Log($"{Prefix} Запуск скасовано.");
                throw;
            }
            catch (Exception e)
            {
                _overlayLoader.UnlockOverlay();
                _overlayLoader.StopOverlay(forceImmediate: true);
                Debug.LogError($"{Prefix} Помилка запуску: {e}");
                throw;
            }
            finally
            {
                _isStarting = false;
            }
        }

        private async Task RunAsync(CancellationToken ct)
        {
            string sceneName = _config.gameplaySceneName;
            Debug.Log($"{Prefix} Завантаження сцени '{sceneName}'.");

            // Lock the overlay so that other services (e.g. JoinRoomPanelService)
            // cannot prematurely stop it during scene loading.
            _overlayLoader.LockOverlay();
            _overlayLoader.LoadOverlay(0f, 100f, "%");

            var loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (loadOp == null)
                throw new InvalidOperationException($"{Prefix} SceneManager повернув null для '{sceneName}'. Переконайтесь, що сцена додана до Build Settings.");

            loadOp.allowSceneActivation = false;

            float startTime = Time.realtimeSinceStartup;
            float displayProgress = 0f;

            // Чекаємо попереднього завантаження (до 90 %)
            while (loadOp.progress < 0.9f)
            {
                ct.ThrowIfCancellationRequested();
                float target = loadOp.progress / 0.9f;
                displayProgress = Mathf.Lerp(displayProgress, target, 0.15f);
                _overlayLoader.UpdateOverlay(Mathf.Round(displayProgress * 100f), 100f, "%");
                await Task.Yield();
            }

            // Гарантуємо мінімальний час показу оверлею
            float elapsed = Time.realtimeSinceStartup - startTime;
            if (elapsed < _config.minPreloadSeconds)
            {
                int remainMs = Mathf.RoundToInt((_config.minPreloadSeconds - elapsed) * 1000f);
                await Task.Delay(remainMs, ct);
            }

            ct.ThrowIfCancellationRequested();

            // Встановлюємо контекст запуску ДО активації сцени
            if (GameLaunchContext.Mode == GameLaunchMode.Unknown || GameLaunchContext.Mode == GameLaunchMode.DirectGameplayTest)
            {
                if (_session.IsHost && _session.Mode == NetworkProviderType.Offline)
                    ConfigureOfflineNewGameFallback();
                else if (_session.IsHost)
                    ConfigureMultiplayerFallback();
                else
                    ConfigureMultiplayerFallback();
            }

            Debug.Log($"{Prefix} GameLaunchContext.Mode = {GameLaunchContext.Mode}");

            _overlayLoader.UpdateOverlay(100f, 100f, "%");

            // Невелика затримка перед активацією для плавного відображення 100 %
            if (_config.sceneActivationDelay > 0f)
                await Task.Delay(Mathf.RoundToInt(_config.sceneActivationDelay * 1000f), ct);

            ct.ThrowIfCancellationRequested();
            loadOp.allowSceneActivation = true;

            // Чекаємо повного завантаження.
            // Оверлей природньо зникне разом з вивантаженням HomeMenu сцени —
            // не потрібно явно викликати StopOverlay тут.
            while (!loadOp.isDone)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Yield();
            }

            Debug.Log($"{Prefix} Сцена '{sceneName}' завантажена.");
        }

        private void ConfigureOfflineNewGameFallback()
        {
            var settings = _session.WorldSettings;
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

        private void ConfigureMultiplayerFallback()
        {
            var settings = _session.WorldSettings;
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
