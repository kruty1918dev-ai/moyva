using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using Zenject;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal class HomeMenuOverlayLoader : IOverlayLoader
    {
        private float _currentValue;
        private float _currentMax = 100f;
        private string _currentSuffix = "%";
        private TaskCompletionSource<bool> _overlayCompletionSource;
        private OverlayLoaderResult _currentResult;
        private readonly OverlayPanelLoader _panel;

        [Inject]
        internal HomeMenuOverlayLoader([InjectOptional] OverlayPanelLoader panel = null)
        {
            _panel = panel;
        }

        public OverlayLoaderResult LoadOverlay(float value, float maxValue = 100, string sufix = "%")
        {
            if (_currentResult != null)
            {
                _currentResult.SetLoading(false, 0f);
            }

            _overlayCompletionSource?.TrySetResult(true);
            _overlayCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            _currentValue = value;
            _currentMax = maxValue > 0 ? maxValue : 100f;
            _currentSuffix = string.IsNullOrEmpty(sufix) ? "%" : sufix;

            float progressValue = _currentMax <= 0
                ? 0f
                : Mathf.Clamp01(_currentValue / _currentMax) * 100f;

            Debug.Log($"[HomeMenuOverlayLoader] Show overlay {progressValue:0.##}{_currentSuffix} ({_currentValue}/{_currentMax})");

            OverlayLoaderResult result = null;
            // Use an async waiter instead of blocking GetResult() to avoid thread-pool blocking.
            result = OverlayLoaderResult.Start(
                async () => await _overlayCompletionSource.Task.ConfigureAwait(false),
                null,
                ex => { StopLoading(result); Debug.LogError($"[HomeMenuOverlayLoader] Overlay failed: {ex.Message}"); }
            );

            _currentResult = result;
            result.SetLoading(true, progressValue);
            _panel?.SetLoading(true, progressValue);
            return result;
        }

        public void UpdateOverlay(float value, float maxValue = 100, string sufix = "%")
        {
            if (_currentResult == null || !_currentResult.IsLoading)
                return;

            _currentValue = value;
            _currentMax = maxValue > 0 ? maxValue : 100f;
            _currentSuffix = string.IsNullOrEmpty(sufix) ? "%" : sufix;

            float progressValue = _currentMax <= 0
                ? 0f
                : Mathf.Clamp01(_currentValue / _currentMax) * 100f;

            _currentResult.SetLoading(true, progressValue);
            _panel?.SetLoading(true, progressValue);
        }

        public void StopOverlay(bool forceImmediate = false)
        {
            Debug.Log("[HomeMenuOverlayLoader] StopOverlay called.");
            _overlayCompletionSource?.TrySetResult(true);
            StopLoading(_currentResult, forceImmediate);
            _currentResult = null;
            _overlayCompletionSource = null;
        }

        private void StopLoading(OverlayLoaderResult result, bool forceImmediate = false)
        {
            if (result == null || !result.IsLoading)
                return;

            var lastProgress = result.Progress;
            result.SetLoading(false, lastProgress);
            _panel?.SetLoading(false, lastProgress);
            if (forceImmediate)
            {
                _panel?.ForceHide();
                Debug.Log("[HomeMenuOverlayLoader] Overlay ready. Forced panel hide.");
            }
            else
            {
                // ensure panel actually hides after a short timeout as a fallback
                _panel?.EnsureHiddenAfterDelay(2f);
                Debug.Log("[HomeMenuOverlayLoader] Overlay ready. Requested panel hide.");
            }
        }
    }
}