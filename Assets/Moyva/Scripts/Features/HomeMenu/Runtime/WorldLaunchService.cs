using System;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.WorldCreation.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Слухає <see cref="WorldCreationConfirmedSignal"/> та запускає завантаження
    /// ігрової сцени через <see cref="ISceneLoadService"/>. Параметри світу
    /// вже попередньо записані в <see cref="IWorldCreationService.CurrentConfig"/>
    /// самим <c>WorldCreationUIController</c>, а також публікуються в сигналі
    /// для підписників (наприклад Bootstrap у ігровій сцені).
    /// </summary>
    internal sealed class WorldLaunchService : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly ISceneLoadService _sceneLoader;
        private readonly HomeMenuConfigSO _config;
        private readonly IWorldCreationService _worldService;
        private readonly HomeMenuRootView _rootView;

        public WorldLaunchService(
            SignalBus signalBus,
            ISceneLoadService sceneLoader,
            HomeMenuConfigSO config,
            IWorldCreationService worldService,
            HomeMenuRootView rootView)
        {
            _signalBus    = signalBus    ?? throw new ArgumentNullException(nameof(signalBus));
            _sceneLoader  = sceneLoader  ?? throw new ArgumentNullException(nameof(sceneLoader));
            _config       = config       ?? throw new ArgumentNullException(nameof(config));
            _worldService = worldService ?? throw new ArgumentNullException(nameof(worldService));
            _rootView     = rootView; // може бути null у тестах
        }

        public void Initialize()
        {
            _signalBus.Subscribe<WorldCreationConfirmedSignal>(OnWorldCreationConfirmed);
        }

        public void Dispose()
        {
            _signalBus?.TryUnsubscribe<WorldCreationConfirmedSignal>(OnWorldCreationConfirmed);
        }

        private void OnWorldCreationConfirmed(WorldCreationConfirmedSignal _)
        {
            // Фінальна валідація перед завантаженням.
            var cfg = _worldService.CurrentConfig;
            if (!_worldService.ValidateConfig(cfg, out string error))
            {
                Debug.LogError($"[WorldLaunchService] Завантаження скасовано: конфіг світу невалідний — {error}");
                return;
            }

            if (string.IsNullOrWhiteSpace(_config.GameplaySceneName))
            {
                Debug.LogError("[WorldLaunchService] HomeMenuConfigSO.GameplaySceneName не задано.");
                return;
            }

            // Показуємо оверлей завантаження.
            _rootView?.ApplyPanelState(HomeMenuPanel.Loading);
            _rootView?.LoadingOverlay?.SetProgress(0f, "Підготовка світу…");

            _sceneLoader.LoadSceneAsync(
                _config.GameplaySceneName,
                progress: p => _rootView?.LoadingOverlay?.SetProgress(p, FormatLoadingText(p)));
        }

        private static string FormatLoadingText(float p)
        {
            int percent = Mathf.RoundToInt(Mathf.Clamp01(p) * 100f);
            return $"Завантаження світу… {percent}%";
        }
    }
}
