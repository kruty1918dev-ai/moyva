using Kruty1918.Moyva.Signals;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    /// <summary>
    /// Показує чорний fullscreen overlay на старті сцени і плавно прибирає його,
    /// коли світ згенеровано/завантажено (WorldGeneratedDataSignal).
    /// Overlay не блокує рейкасти.
    /// </summary>
    internal sealed class WorldLoadRevealOverlayService : IInitializable, ITickable, System.IDisposable
    {
        private const float FallbackStartFadeTimeoutSeconds = 8f;

        private enum FadeState
        {
            Idle,
            WaitingSignal,
            Delay,
            Fading,
            Completed,
        }

        private readonly SignalBus _signalBus;
        private readonly BootstrapGameSettings _settings;

        private Canvas _canvas;
        private Image _image;
        private FadeState _state;
        private float _timer;
        private float _delay;
        private float _duration;
        private float _startAlpha;

        public WorldLoadRevealOverlayService(SignalBus signalBus, BootstrapGameSettings settings)
        {
            _signalBus = signalBus;
            _settings = settings;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
            _signalBus.Subscribe<WorldBuiltSignal>(OnWorldBuilt);

            var fadeSettings = _settings?.WorldRevealFade;
            if (fadeSettings == null || !fadeSettings.Enabled)
            {
                _state = FadeState.Idle;
                return;
            }

            _delay = Mathf.Max(0f, fadeSettings.StartDelaySeconds);
            _duration = Mathf.Max(0f, fadeSettings.DurationSeconds);
            _startAlpha = Mathf.Clamp01(fadeSettings.StartAlpha);

            EnsureOverlay();
            SetOverlayAlpha(_startAlpha);
            _state = FadeState.WaitingSignal;
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<WorldGeneratedDataSignal>(OnWorldGenerated);
            _signalBus.TryUnsubscribe<WorldBuiltSignal>(OnWorldBuilt);

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
                case FadeState.WaitingSignal:
                    _timer += Time.unscaledDeltaTime;
                    if (_timer >= FallbackStartFadeTimeoutSeconds)
                        BeginFade();
                    break;

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
                    // SmoothStep для мʼякого входу/виходу без ривків.
                    float eased = t * t * (3f - 2f * t);
                    SetOverlayAlpha(Mathf.Lerp(_startAlpha, 0f, eased));
                    if (t >= 1f)
                        CompleteFade();
                    break;
            }
        }

        private void OnWorldGenerated(WorldGeneratedDataSignal _)
        {
            BeginFade();
        }

        private void OnWorldBuilt(WorldBuiltSignal _)
        {
            BeginFade();
        }

        private void BeginFade()
        {
            if (_state != FadeState.WaitingSignal)
                return;

            _timer = 0f;
            _state = _delay > 0f ? FadeState.Delay : (_duration > 0f ? FadeState.Fading : FadeState.Completed);
            if (_state == FadeState.Completed)
                CompleteFade();
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

            var go = new GameObject("WorldLoadRevealOverlay");
            Object.DontDestroyOnLoad(go);

            _canvas = go.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = short.MaxValue;

            var scaler = go.AddComponent<CanvasScaler>();
            ConfigureCanvasScaler(scaler);
            go.AddComponent<GraphicRaycaster>();

            var imageGo = new GameObject("FadeImage");
            imageGo.transform.SetParent(go.transform, false);

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

        private static void ConfigureCanvasScaler(CanvasScaler scaler)
        {
            if (scaler == null)
                return;

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;
            scaler.referencePixelsPerUnit = 100f;
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
