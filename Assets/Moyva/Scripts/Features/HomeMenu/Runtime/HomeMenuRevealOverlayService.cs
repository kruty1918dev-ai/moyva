using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Показує чорний fullscreen overlay на вході в HomeMenu і плавно прибирає його.
    /// Overlay не блокує рейкасти й повністю налаштовується з HomeMenuInstaller.
    /// </summary>
    internal sealed class HomeMenuRevealOverlayService : IInitializable, ITickable, System.IDisposable
    {
        private enum FadeState
        {
            Idle,
            Delay,
            Fading,
            Completed,
        }

        private readonly HomeMenuRevealFadeSettings _settings;

        private Canvas _canvas;
        private Image _image;
        private FadeState _state;
        private float _timer;
        private float _delay;
        private float _duration;
        private float _startAlpha;

        public HomeMenuRevealOverlayService(HomeMenuRevealFadeSettings settings)
        {
            _settings = settings;
        }

        public void Initialize()
        {
            if (_settings == null || !_settings.Enabled)
            {
                _state = FadeState.Idle;
                return;
            }

            _delay = Mathf.Max(0f, _settings.StartDelaySeconds);
            _duration = Mathf.Max(0f, _settings.DurationSeconds);
            _startAlpha = Mathf.Clamp01(_settings.StartAlpha);

            EnsureOverlay();
            SetOverlayAlpha(_startAlpha);

            if (_canvas != null)
                _canvas.gameObject.SetActive(true);

            _state = _delay > 0f
                ? FadeState.Delay
                : (_duration > 0f ? FadeState.Fading : FadeState.Completed);

            if (_state == FadeState.Completed)
                CompleteFade();
        }

        public void Dispose()
        {
            if (_canvas != null)
                Object.Destroy(_canvas.gameObject);

            _canvas = null;
            _image = null;
            _state = FadeState.Idle;
        }

        public void Tick()
        {
            if (_image == null)
                return;

            switch (_state)
            {
                case FadeState.Delay:
                    _timer += Time.unscaledDeltaTime;
                    if (_timer >= _delay)
                    {
                        _timer = 0f;
                        _state = _duration <= 0f ? FadeState.Completed : FadeState.Fading;
                        if (_state == FadeState.Completed)
                            CompleteFade();
                    }
                    break;

                case FadeState.Fading:
                    _timer += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(_timer / Mathf.Max(0.0001f, _duration));
                    float eased = t * t * (3f - 2f * t);
                    SetOverlayAlpha(Mathf.Lerp(_startAlpha, 0f, eased));
                    if (t >= 1f)
                        CompleteFade();
                    break;
            }
        }

        private void CompleteFade()
        {
            SetOverlayAlpha(0f);
            if (_canvas != null)
                _canvas.gameObject.SetActive(false);
            _state = FadeState.Completed;
        }

        private void EnsureOverlay()
        {
            if (_canvas != null && _image != null)
                return;

            var root = new GameObject("HomeMenuRevealOverlay");

            _canvas = root.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = short.MaxValue - 8;

            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            root.AddComponent<GraphicRaycaster>();

            var imageGo = new GameObject("FadeImage");
            imageGo.transform.SetParent(root.transform, false);

            var rect = imageGo.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            _image = imageGo.AddComponent<Image>();
            _image.color = Color.black;
            _image.raycastTarget = false;

            var group = imageGo.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        private void SetOverlayAlpha(float alpha)
        {
            if (_image == null)
                return;

            Color color = _image.color;
            color.a = Mathf.Clamp01(alpha);
            _image.color = color;
        }
    }
}