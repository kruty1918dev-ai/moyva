using System;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime.Startup;
using Kruty1918.Moyva.Shared.Common;
using UnityEngine;
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

        private readonly IOverlayLoader _overlayLoader;
        private readonly IGameplayStartupPipeline _startupPipeline;

        private bool _isStarting;

        [Inject]
        public HomeMenuGameStarter(IOverlayLoader overlayLoader, IGameplayStartupPipeline startupPipeline)
        {
            _overlayLoader = Guard.NotNull(overlayLoader, nameof(overlayLoader));
            _startupPipeline = Guard.NotNull(startupPipeline, nameof(startupPipeline));
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
                ct.ThrowIfCancellationRequested();
                await RunAsync(CancellationToken.None);
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
            Debug.Log($"{Prefix} Startup pipeline begin.");
            await _startupPipeline.RunAsync(ct);
            Debug.Log($"{Prefix} Startup pipeline completed. Final phase={_startupPipeline.CurrentPhase}.");
        }
    }
}
