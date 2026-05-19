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

        /// <summary>Overlay, який показує користувачу прогрес старту гри.</summary>
        private readonly IOverlayLoader _overlayLoader;

        /// <summary>Startup-пайплайн, що готує й активує gameplay-сцену.</summary>
        private readonly IGameplayStartupPipeline _startupPipeline;

        /// <summary>Захист від повторного одночасного старту гри.</summary>
        private bool _isStarting;

        /// <summary>Створити сервіс запуску гри з меню.</summary>
        [Inject]
        public HomeMenuGameStarter(IOverlayLoader overlayLoader, IGameplayStartupPipeline startupPipeline)
        {
            _overlayLoader = Guard.NotNull(overlayLoader, nameof(overlayLoader));
            _startupPipeline = Guard.NotNull(startupPipeline, nameof(startupPipeline));
        }

        /// <summary>
        /// Запустити gameplay через startup pipeline з базовим захистом від повторного входу та помилок.
        /// </summary>
        public async Task StartGameAsync(CancellationToken ct = default)
        {
            // 1: Якщо старт уже триває, не запускаємо другий паралельний pipeline.
            if (_isStarting)
            {
                Debug.LogWarning($"{Prefix} StartGameAsync викликано повторно — ігнорування.");
                return;
            }

            // 2: Позначаємо сервіс як зайнятий, щоб заблокувати повторні запуски до завершення поточного.
            _isStarting = true;
            try
            {
                // 3: Даємо зовнішньому коду скасувати операцію ще до входу в pipeline.
                ct.ThrowIfCancellationRequested();

                // 4: Запускаємо внутрішній workflow старту гри.
                await RunAsync(CancellationToken.None);
            }
            catch (OperationCanceledException)
            {
                // 5: При скасуванні розблоковуємо overlay і прибираємо його негайно.
                _overlayLoader.UnlockOverlay();
                _overlayLoader.StopOverlay(forceImmediate: true);
                Debug.Log($"{Prefix} Запуск скасовано.");
                throw;
            }
            catch (Exception e)
            {
                // 6: На будь-якій помилці також відновлюємо UI в консистентний стан.
                _overlayLoader.UnlockOverlay();
                _overlayLoader.StopOverlay(forceImmediate: true);
                Debug.LogError($"{Prefix} Помилка запуску: {e}");
                throw;
            }
            finally
            {
                // 7: Незалежно від результату знімаємо прапорець in-progress.
                _isStarting = false;
            }
        }

        /// <summary>Виконати внутрішній startup workflow і залогувати його межі.</summary>
        private async Task RunAsync(CancellationToken ct)
        {
            // 1: Логуємо початок pipeline для діагностики запуску.
            Debug.Log($"{Prefix} Startup pipeline begin.");

            // 2: Передаємо керування startup-пайплайну, який виконає всі фази переходу в gameplay.
            await _startupPipeline.RunAsync(ct);

            // 3: Логуємо завершення та фінальну фазу для простішого troubleshooting.
            Debug.Log($"{Prefix} Startup pipeline completed. Final phase={_startupPipeline.CurrentPhase}.");
        }
    }
}
