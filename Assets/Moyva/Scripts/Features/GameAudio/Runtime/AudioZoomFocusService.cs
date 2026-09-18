using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.GameAudio.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.GameAudio.Runtime
{
    /// <summary>
    /// Нормалізований стан зуму камери для аудіо-шарів.
    /// ZoomT: 0 = максимально близько, 1 = максимально далеко (згладжено).
    /// Використовує той самий діапазон, що й CameraZoom (CameraSettingsSO).
    /// </summary>
    public sealed class AudioZoomFocusService : IInitializable, ITickable
    {
        private readonly AudioAmbienceConfig _config;
        private readonly CameraSettingsSO _cameraSettings;
        private readonly UnityEngine.Camera _camera;

        private float _zoomT;
        private bool _hasCamera;

        public AudioZoomFocusService(
            [InjectOptional] AudioAmbienceConfig config,
            [InjectOptional] CameraSettingsSO cameraSettings,
            [InjectOptional] UnityEngine.Camera camera)
        {
            _config = config;
            _cameraSettings = cameraSettings;
            _camera = camera;
        }

        /// <summary>0 = близько, 1 = далеко.</summary>
        public float ZoomT => _zoomT;

        public void Initialize()
        {
            _hasCamera = _camera != null;
            _zoomT = EvaluateRawZoomT();
        }

        public void Tick()
        {
            float target = EvaluateRawZoomT();
            float smoothing = _config?.zoom != null ? Mathf.Max(0.1f, _config.zoom.smoothing) : 5f;
            _zoomT = Mathf.Lerp(_zoomT, target, 1f - Mathf.Exp(-smoothing * Time.unscaledDeltaTime));
        }

        /// <summary>Множник чутності позиційного еміера за поточним zoom (1..0).</summary>
        public float EvaluateEmitterFactor(float fadeStart, float fadeEnd)
        {
            if (_config?.zoom != null && !_config.zoom.enabled)
                return 1f;

            float start = fadeStart >= 0f ? fadeStart : (_config?.zoom?.emitterFadeStart ?? 0.45f);
            float end = fadeEnd >= 0f ? fadeEnd : (_config?.zoom?.emitterFadeEnd ?? 0.9f);
            if (end <= start)
                return _zoomT < end ? 1f : 0f;

            return Mathf.Clamp01(1f - Mathf.InverseLerp(start, end, _zoomT));
        }

        /// <summary>Lowpass-частота для ambient-шарів за поточним zoom.</summary>
        public float EvaluateBedCutoff()
        {
            if (_config?.zoom == null)
                return 22000f;

            float t = _config.zoom.enabled ? _zoomT : 0f;
            return Mathf.Lerp(_config.zoom.nearCutoff, _config.zoom.farCutoff, t);
        }

        private float EvaluateRawZoomT()
        {
            if (!_hasCamera || _camera == null)
                return 0f;

            float current = _camera.orthographic ? _camera.orthographicSize : _camera.fieldOfView;
            float min = _cameraSettings != null ? _cameraSettings.ResolveMinZoom() : 0.1f;
            float max = _cameraSettings != null ? _cameraSettings.ResolveMaxZoom() : Mathf.Max(min + 1f, current);
            if (max <= min)
                return 0f;

            return Mathf.Clamp01(Mathf.InverseLerp(min, max, current));
        }
    }
}
