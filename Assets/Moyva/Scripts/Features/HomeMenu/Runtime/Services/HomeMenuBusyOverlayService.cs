using System;
using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.API;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>HomeMenuBusyOverlayService — class: головного меню зайнятості накладання сервісу.</summary>
    internal sealed class HomeMenuBusyOverlayService : IOverlayLoader, IDisposable
    {
        private readonly HomeMenuMoyvaUiViewController _view;

        private TaskCompletionSource<bool> _completionSource;
        private OverlayLoaderResult _result;
        private int _lockCount;
        private string _suffix = "%";

        /// <summary>Виконує HomeMenuBusyOverlayService.</summary>
        public HomeMenuBusyOverlayService(HomeMenuMoyvaUiViewController view)
        {
            _view = view;
            OverlayLoaderResult.CurrentChanged += HandleCurrentChanged;
        }

        /// <summary>Звільняє ресурси та відписує від подій.</summary>
        public void Dispose()
        {
            OverlayLoaderResult.CurrentChanged -= HandleCurrentChanged;
            StopOverlay(forceImmediate: true);
        }

        /// <summary>Завантажує накладання.</summary>
        public OverlayLoaderResult LoadOverlay(float value, float maxValue = 100, string sufix = "%")
        {
            _result?.SetLoading(false, _result.Progress);
            _completionSource?.TrySetResult(true);
            _completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _suffix = string.IsNullOrEmpty(sufix) ? "%" : sufix;
            var progress = maxValue <= 0f ? 0f : Mathf.Clamp01(value / maxValue) * 100f;
            _result = OverlayLoaderResult.Start(
                async () => await _completionSource.Task.ConfigureAwait(false),
                null,
                _ => StopOverlay(true));
            _result.SetLoading(true, progress);
            Push();
            return _result;
        }

        /// <summary>Оновлює накладання.</summary>
        public void UpdateOverlay(float value, float maxValue = 100, string sufix = "%")
        {
            if (_result == null || !_result.IsLoading)
                return;

            _suffix = string.IsNullOrEmpty(sufix) ? "%" : sufix;
            var progress = maxValue <= 0f ? 0f : Mathf.Clamp01(value / maxValue) * 100f;
            _result.SetLoading(true, progress);
            Push();
        }

        /// <summary>Встановлює вже локалізований статус-рядок під прогресом оверлею.</summary>
        public void SetOverlayStatus(string status)
        {
            _result?.SetStatus(status ?? string.Empty);
            Push();
        }

        /// <summary>Зупиняє накладання.</summary>
        public void StopOverlay(bool forceImmediate = false)
        {
            if (_lockCount > 0 && !forceImmediate)
                return;

            _completionSource?.TrySetResult(true);
            _completionSource = null;
            _result?.SetLoading(false, _result.Progress);
            _result = null;
            Push();
        }

        /// <summary>Блокує накладання.</summary>
        public void LockOverlay() => _lockCount++;

        /// <summary>Розблоковує накладання.</summary>
        public void UnlockOverlay() => _lockCount = Math.Max(0, _lockCount - 1);

        // CurrentChanged fires under the result's internal lock on the mutating
        // thread; Push only copies plain fields into the view read-model, which is
        // safe to do inline (same thread guarantees as the previous implementation).
        private void HandleCurrentChanged(OverlayLoaderResult _) => Push();

        private void Push()
        {
            var loading = _result != null && _result.IsLoading;
            _view.ApplyOverlayPresentation(
                loading,
                _result?.Progress ?? 0f,
                _suffix,
                _result?.Status ?? string.Empty);
        }
    }
}
