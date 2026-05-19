using System.Collections;
using UnityEngine;
using Kruty1918.Moyva.HomeMenu.API;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Віджет loading-overlay для HomeMenu з анімацією прогресу і watchdog-закриттям.
    /// Підписується на <see cref="OverlayLoaderResult.CurrentChanged"/> та синхронізує видимість оверлею.
    /// </summary>
    public class OverlayPanelLoader : MonoBehaviour
    {
        [SerializeField] private GameObject _overlayRoot;
        [SerializeField] private float _animationDuration = 0.25f;
        [SerializeField] private float _closeDelay = 0.15f;

        private float _displayedProgress;
        private Coroutine _progressCoroutine;
        private Coroutine _hideWatchdogCoroutine;
        private static int _instanceCount;
        private static readonly bool VerboseLogging = false;

        private void Awake()
        {
            _instanceCount++;
            LogWithSufix($"Awake. Instances: {_instanceCount}. GameObject: {gameObject.name}");
            if (_overlayRoot == null)
                _overlayRoot = gameObject;

            _overlayRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            _instanceCount--;
            LogWithSufix($"OnDestroy. Instances: {_instanceCount}. GameObject: {gameObject.name}");
        }

        private void OnEnable()
        {
            OverlayLoaderResult.CurrentChanged += OnOverlayChanged;
            if (OverlayLoaderResult.Current != null)
                OnOverlayChanged(OverlayLoaderResult.Current);
        }

        private void OnDisable()
        {
            OverlayLoaderResult.CurrentChanged -= OnOverlayChanged;
        }

        private void OnOverlayChanged(OverlayLoaderResult result)
        {
            if (_overlayRoot == null)
                return;

            var isLoading = result?.IsLoading ?? false;
            var progress = result?.Progress ?? 0f;
            SetLoading(isLoading, progress);
            LogWithSufix($"Overlay was changed. Is loading? - {isLoading}, Progress: {progress:0.##}%.");
        }

        public void SetLoading(bool isLoading, float progress)
        {
            if (_overlayRoot == null)
                _overlayRoot = gameObject;

            // 1: Скасовуємо поточну анімацію прогресу перед новою операцією.
            if (_progressCoroutine != null)
            {
                StopCoroutine(_progressCoroutine);
                _progressCoroutine = null;
                LogWithSufix("Corutine was cancled. Progress corutine is null");
            }

            // 2: При старті loading скасовуємо відкладений hide-watchdog.
            // cancel any pending watchdog when opening or updating
            if (isLoading && _hideWatchdogCoroutine != null)
            {
                StopCoroutine(_hideWatchdogCoroutine);
                _hideWatchdogCoroutine = null;
                LogWithSufix("Operation was canceled by new loading request. Watchdog canceled.");
            }

            // 3: Або відкриваємо overlay з анімацією до target progress, або закриваємо з graceful finish.
            if (isLoading)
            {
                _overlayRoot.SetActive(true);
                _progressCoroutine = StartCoroutine(AnimateProgress(progress));
                LogWithSufix($"Overlay opened. Progress: {progress:0.##}%.");
            }
            else
            {
                _progressCoroutine = StartCoroutine(CloseAndHide());
                LogWithSufix($"Overlay closing. Progress: {progress:0.##}%.");
            }
        }

        private IEnumerator AnimateProgress(float target)
        {
            float start = _displayedProgress;
            float t = 0f;
            while (t < _animationDuration)
            {
                t += Time.deltaTime;
                _displayedProgress = Mathf.Lerp(start, target, Mathf.Clamp01(t / _animationDuration));
                yield return null;
            }
            _displayedProgress = target;
            LogWithSufix($"Animate Process completed. Progress: {_displayedProgress}");
            _progressCoroutine = null;
        }

        private IEnumerator CloseAndHide()
        {
LogWithSufix("Start the close process");

            // Animate up to 100% for smooth close
            if (_displayedProgress < 100f)
            {
                yield return StartCoroutine(AnimateProgress(100f));
            }

            yield return new WaitForSeconds(_closeDelay);
            _overlayRoot.SetActive(false);
            LogWithSufix("Overlay hidden.");
            _displayedProgress = 0f;

            if (_hideWatchdogCoroutine != null)
            {
                StopCoroutine(_hideWatchdogCoroutine);
                _hideWatchdogCoroutine = null;
            }

            LogWithSufix("Was closed and hided");
        }

        private IEnumerator HideWatchdog(float timeout)
        {
            yield return new WaitForSeconds(timeout);
            if (_overlayRoot != null && _overlayRoot.activeSelf)
            {
                LogWithSufix("HideWatchdog triggered ForceHide.");
                ForceHide();
            }
            _hideWatchdogCoroutine = null;
            LogWithSufix("Watchdog was hidedd");
        }

        public void EnsureHiddenAfterDelay(float timeoutSeconds)
        {
            if (_hideWatchdogCoroutine != null)
            {
                StopCoroutine(_hideWatchdogCoroutine);
                _hideWatchdogCoroutine = null;
            }
            _hideWatchdogCoroutine = StartCoroutine(HideWatchdog(timeoutSeconds));
        }

        public void ForceHide()
        {
            if (_overlayRoot == null)
                _overlayRoot = gameObject;

            if (_progressCoroutine != null)
            {
                StopCoroutine(_progressCoroutine);
                _progressCoroutine = null;
            }

            if (_hideWatchdogCoroutine != null)
            {
                StopCoroutine(_hideWatchdogCoroutine);
                _hideWatchdogCoroutine = null;
            }

            _overlayRoot.SetActive(false);
            _displayedProgress = 0f;
            LogWithSufix("ForceHide executed.");
        }

        private void LogWithSufix(string msg)
        {
            if (!VerboseLogging)
                return;

            Debug.Log($"[OverlayPanelLoader] {msg}");
        }
    }
}