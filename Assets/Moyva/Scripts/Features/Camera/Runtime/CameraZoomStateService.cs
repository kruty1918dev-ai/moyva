using Kruty1918.Moyva.Camera.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Camera.Runtime
{
    /// <summary>
    /// Single authoritative publisher of normalized camera-distance state.
    /// Reads the live camera once per frame (LateTick, after CameraZoom applied
    /// the smoothed zoom) and exposes raw/smoothed normalized zoom plus the
    /// shared FarViewWeight for visuals and audio.
    /// </summary>
    internal sealed class CameraZoomStateService
        : ICameraZoomState, IInitializable, ILateTickable
    {
        private readonly UnityEngine.Camera _camera;
        private readonly CameraSettingsSO _settings;

        private float _currentZoom;
        private float _minZoom;
        private float _maxZoom;
        private float _normalizedZoom;
        private float _smoothedNormalizedZoom;
        private float _farViewWeight;
        private bool _isPerspective;

        public CameraZoomStateService(
            UnityEngine.Camera camera,
            CameraSettingsSO settings)
        {
            _camera = camera;
            _settings = settings;
        }

        public float CurrentZoom => _currentZoom;
        public float MinZoom => _minZoom;
        public float MaxZoom => _maxZoom;
        public float NormalizedZoom => _normalizedZoom;
        public float SmoothedNormalizedZoom => _smoothedNormalizedZoom;
        public float FarViewWeight => _farViewWeight;
        public bool IsPerspective => _isPerspective;

        public void Initialize()
        {
            SampleRaw();
            // Start fully converged — no smoothing ramp on scene load.
            _smoothedNormalizedZoom = _normalizedZoom;
            _farViewWeight = EvaluateWeight(_smoothedNormalizedZoom);
        }

        public void LateTick()
        {
            SampleRaw();

            float smoothing = ResolveFarViewSmoothing();
            _smoothedNormalizedZoom = Mathf.Lerp(
                _smoothedNormalizedZoom,
                _normalizedZoom,
                CameraZoomMath.SmoothingFactor(smoothing, Time.unscaledDeltaTime));

            _farViewWeight = EvaluateWeight(_smoothedNormalizedZoom);
        }

        /// <summary>
        /// Test hook: advance one frame of state without Zenject.
        /// </summary>
        internal void TickForTest(float unscaledDeltaTime)
        {
            SampleRaw();
            float smoothing = ResolveFarViewSmoothing();
            _smoothedNormalizedZoom = Mathf.Lerp(
                _smoothedNormalizedZoom,
                _normalizedZoom,
                CameraZoomMath.SmoothingFactor(smoothing, unscaledDeltaTime));
            _farViewWeight = EvaluateWeight(_smoothedNormalizedZoom);
        }

        private void SampleRaw()
        {
            _isPerspective = _camera != null && !_camera.orthographic;
            _currentZoom = _camera == null
                ? 0f
                : (_camera.orthographic ? _camera.orthographicSize : _camera.fieldOfView);

            _minZoom = _settings != null ? _settings.ResolveMinZoom() : 0.1f;
            _maxZoom = _settings != null
                ? Mathf.Max(_minZoom + 0.1f, _settings.ResolveMaxZoom())
                : Mathf.Max(_minZoom + 1f, _currentZoom);

            _normalizedZoom = CameraZoomMath.NormalizeZoom(
                _currentZoom, _minZoom, _maxZoom);
        }

        private float EvaluateWeight(float smoothedNormalized)
        {
            if (_settings == null)
                return smoothedNormalized;

            return CameraZoomMath.EvaluateFarViewWeight(
                smoothedNormalized,
                _settings.ResolveFarViewStart(),
                _settings.ResolveFarViewFull(),
                _settings.ResolveFarViewShape());
        }

        private float ResolveFarViewSmoothing()
            => _settings != null ? _settings.ResolveFarViewSmoothing() : 4f;
    }
}
