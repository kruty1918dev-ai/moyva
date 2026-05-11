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
    internal sealed class GameplayStartupPipeline : IGameplayStartupPipeline
    {
        private const string Prefix = "[GameplayStartupPipeline]";

        private readonly HomeMenuConfigSO _config;
        private readonly IOverlayLoader _overlayLoader;
        private readonly IGameplaySession _session;
        private readonly IGraphicsSettingsService _graphicsSettingsService;
        private readonly IStartupPrewarmService _startupPrewarmService;
        private readonly IScenePreActivationInitializer[] _preActivationInitializers;
        private readonly ServiceModeProfile _gameplayProfile;

        private AsyncOperation _loadOp;
        private float _overlayStartTime;
        private float _displayProgress;
        private string _sceneName;

        public GameplayStartupPhase CurrentPhase { get; private set; } = GameplayStartupPhase.None;

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

        public async Task RunAsync(CancellationToken ct = default)
        {
            ApplyGameplayGraphicsPolicy();

            _sceneName = _config.gameplaySceneName;
            _displayProgress = 0f;

            EnterPhase(GameplayStartupPhase.Preload);
            await ExecutePreloadAsync(ct);

            EnterPhase(GameplayStartupPhase.Bind);
            await ExecuteBindAsync(ct);

            EnterPhase(GameplayStartupPhase.Warmup);
            await ExecuteWarmupAsync(ct);

            EnterPhase(GameplayStartupPhase.SceneActivate);
            await ExecuteSceneActivateAsync(ct);

            EnterPhase(GameplayStartupPhase.Completed);
            Debug.Log($"{Prefix} Scene '{_sceneName}' loaded successfully.");
        }

        private void EnterPhase(GameplayStartupPhase phase)
        {
            CurrentPhase = phase;
            if (_gameplayProfile.IsVerboseLogging || phase == GameplayStartupPhase.Completed)
                Debug.Log($"{Prefix} Phase => {phase}");
        }

        private async Task ExecutePreloadAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            _overlayLoader.LockOverlay();
            _overlayLoader.LoadOverlay(0f, 100f, "%");

            await PreloadStartupResourcesAsync(ct);
            _overlayLoader.UpdateOverlay(8f, 100f, "%");

            if (_startupPrewarmService != null)
                await _startupPrewarmService.PrewarmAsync(ct);
            _overlayLoader.UpdateOverlay(15f, 100f, "%");

            _loadOp = SceneManager.LoadSceneAsync(_sceneName, LoadSceneMode.Single);
            if (_loadOp == null)
            {
                throw new InvalidOperationException(
                    $"{Prefix} SceneManager returned null for '{_sceneName}'. Ensure the scene is in Build Settings.");
            }

            _loadOp.allowSceneActivation = false;
            _overlayStartTime = Time.realtimeSinceStartup;

            await WaitForScenePreloadAsync(ct);
        }

        private async Task WaitForScenePreloadAsync(CancellationToken ct)
        {
            while (_loadOp.progress < 0.9f)
            {
                ct.ThrowIfCancellationRequested();
                float target = _loadOp.progress / 0.9f;
                _displayProgress = Mathf.Lerp(_displayProgress, target, 0.15f);
                _overlayLoader.UpdateOverlay(Mathf.Round(_displayProgress * 100f), 100f, "%");
                await Task.Yield();
            }

            float elapsed = Time.realtimeSinceStartup - _overlayStartTime;
            if (elapsed < _config.minPreloadSeconds)
            {
                int remainMs = Mathf.RoundToInt((_config.minPreloadSeconds - elapsed) * 1000f);
                await Task.Delay(remainMs, ct);
            }
        }

        private async Task ExecuteBindAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            if (GameLaunchContext.Mode != GameLaunchMode.Unknown &&
                GameLaunchContext.Mode != GameLaunchMode.DirectGameplayTest)
            {
                return;
            }

            if (_session.IsHost && _session.Mode == NetworkProviderType.Offline)
            {
                ConfigureOfflineNewGameFallback();
            }
            else
            {
                ConfigureMultiplayerFallback();
            }

            await Task.CompletedTask;
        }

        private async Task ExecuteWarmupAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            for (int i = 0; i < _preActivationInitializers.Length; i++)
            {
                ct.ThrowIfCancellationRequested();
                if (_preActivationInitializers[i] != null)
                    await _preActivationInitializers[i].InitializeBeforeActivationAsync(ct);
            }

            _overlayLoader.UpdateOverlay(100f, 100f, "%");

            if (_config.sceneActivationDelay > 0f)
            {
                await Task.Delay(Mathf.RoundToInt(_config.sceneActivationDelay * 1000f), ct);
            }
        }

        private async Task ExecuteSceneActivateAsync(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            _loadOp.allowSceneActivation = true;

            while (!_loadOp.isDone)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Yield();
            }
        }

        private static async Task PreloadStartupResourcesAsync(CancellationToken ct)
        {
            ResourceRequest graphicsRequest = Resources.LoadAsync<GraphicsStartupSettingsSO>(GraphicsStartupSettingsSO.DefaultResourcePath);
            ResourceRequest audioRequest = Resources.LoadAsync<AudioRegistrySO>("MoyvaAudioRegistry");

            while (!graphicsRequest.isDone || !audioRequest.isDone)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Yield();
            }

            _ = graphicsRequest.asset;
            _ = audioRequest.asset;
        }

        private void ApplyGameplayGraphicsPolicy()
        {
            if (_graphicsSettingsService == null || !_gameplayProfile.ApplyGraphicsProfile)
                return;

            var current = _graphicsSettingsService.Settings.Profile;
            if (_gameplayProfile.RespectCustomGraphicsProfile && current == GraphicsQualityProfile.Custom)
                return;

            if (current != _gameplayProfile.GraphicsProfile)
                _graphicsSettingsService.SetProfile(_gameplayProfile.GraphicsProfile);
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
