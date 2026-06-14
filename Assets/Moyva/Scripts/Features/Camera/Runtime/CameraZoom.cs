using Kruty1918.Moyva.Camera.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Camera.Runtime
{
    internal sealed class CameraZoom : ICameraZoom, IInitializable, ILateTickable
    {
        private const string GlobalMipBiasProperty = "_MoyvaTexLodBias";
        private const float MaxAutoMipBiasFallback = 0.75f;
        private const float MobileCloseZoomMipBias = 0.75f;
        private const float MobileCloseZoomBiasEnd = 0.22f;
        private const float ZoomEpsilon = 0.0005f;
        private const float MouseWheelStepDelta = 120f;
        private const float RawMouseWheelThreshold = 10f;
        private const float MaxPinchTargetStepFraction = 0.1f;
        private const float MinimumPerspectiveFieldOfView = 1f;
        private const float MaximumPerspectiveFieldOfView = 179f;
        private static readonly int GlobalMipBiasId = Shader.PropertyToID(GlobalMipBiasProperty);

        private readonly UnityEngine.Camera _camera;
        private readonly CameraSettingsSO _settings;

        private float _targetZoom;
        private float _lastPushedMipBias = float.NaN;

        private float _forceBlockTimer;
        private const float ForceBlockDuration = 1.5f; // Час затримки після форсованого зуму

        public CameraZoom(
            UnityEngine.Camera camera,
            CameraSettingsSO settings)
        {
            _camera = camera;
            _settings = settings;
        }

        public void Initialize()
        {
            // На старті синхронізуємо цільовий зум з поточним Orthographic Size або Field Of View.
            _targetZoom = GetCurrentZoom();

            UpdateGlobalMipBias();
        }

        public void ZoomCamera(float delta)
            => ZoomCamera(delta, ResolveScreenCenter());

        public void ZoomCamera(float delta, Vector2 screenFocalPoint)
        {
            // Якщо діє блокування від форсованого зуму — ігноруємо інпут гравця
            if (_forceBlockTimer > 0f) return;

            float normalizedDelta = NormalizeWheelDelta(delta);
            if (Mathf.Abs(normalizedDelta) <= ZoomEpsilon)
                return;

            // Віднімаємо дельту: скрол вперед наближає, назад віддаляє.
            float zoomStep = Mathf.Max(0.01f, _settings.ResolveZoomSpeed());
            ResolveZoomRange(out float minZoom, out float maxZoom);
            float previousTargetZoom = _targetZoom;
            _targetZoom = Mathf.Clamp(_targetZoom - normalizedDelta * zoomStep, minZoom, maxZoom);

            if (Mathf.Abs(_targetZoom - previousTargetZoom) <= ZoomEpsilon)
                return;
        }

        public void ZoomCameraByScale(float scaleFactor, bool immediate)
            => ZoomCameraByScale(scaleFactor, immediate, ResolveScreenCenter());

        public void ZoomCameraByScale(float scaleFactor, bool immediate, Vector2 screenFocalPoint)
        {
            if (_forceBlockTimer > 0f) return;
            if (scaleFactor <= 0f || float.IsNaN(scaleFactor) || float.IsInfinity(scaleFactor)) return;

            float sensitivity = Mathf.Max(0.01f, _settings.ResolveTouchPinchZoomSensitivity());
            ResolveZoomRange(out float minZoom, out float maxZoom);
            float adjustedScale = Mathf.Pow(scaleFactor, sensitivity);
            float nextTarget = Mathf.Clamp(_targetZoom * adjustedScale, minZoom, maxZoom);
            float previousTargetZoom = _targetZoom;
            float maxPinchStep = Mathf.Max(0.1f, (maxZoom - minZoom) * MaxPinchTargetStepFraction);
            _targetZoom = Mathf.MoveTowards(_targetZoom, nextTarget, maxPinchStep);

            if (Mathf.Abs(_targetZoom - previousTargetZoom) <= ZoomEpsilon)
                return;

            if (!immediate)
                return;

            ApplyZoomValue(_targetZoom);
            UpdateGlobalMipBias();
        }

        private Vector2 ResolveScreenCenter()
        {
            float width = Screen.width > 0 ? Screen.width : (_camera != null ? Mathf.Max(1, _camera.pixelWidth) : 1f);
            float height = Screen.height > 0 ? Screen.height : (_camera != null ? Mathf.Max(1, _camera.pixelHeight) : 1f);
            return new Vector2(width * 0.5f, height * 0.5f);
        }

        public void ForceZoomCamera(float zoomLevel)
        {
            ResolveZoomRange(out float minZoom, out float maxZoom);
            _targetZoom = Mathf.Clamp(zoomLevel, minZoom, maxZoom);

            // Блокуємо ручне керування на заданий час
            _forceBlockTimer = ForceBlockDuration;
        }

        public void LateTick()
        {
            // Оновлюємо таймер блокування
            if (_forceBlockTimer > 0f)
            {
                _forceBlockTimer -= Time.deltaTime;
            }

            ResolveZoomRange(out float minZoom, out float maxZoom);
            _targetZoom = Mathf.Clamp(_targetZoom, minZoom, maxZoom);

            float currentZoom = GetCurrentZoom();
            float interpolation = ResolveInterpolationFactor(_settings.ResolveSmoothTime());
            float nextZoom = Mathf.Lerp(currentZoom, _targetZoom, interpolation);
            if (Mathf.Abs(nextZoom - _targetZoom) <= ZoomEpsilon)
                nextZoom = _targetZoom;

            ApplyZoomValue(nextZoom);

            UpdateGlobalMipBias();
        }

        private void ApplyZoomValue(float zoomValue)
        {
            if (_camera.orthographic)
            {
                _camera.orthographicSize = zoomValue;
                return;
            }

            _camera.fieldOfView = zoomValue;
        }

        private float GetCurrentZoom()
        {
            if (_camera.orthographic)
                return _camera.orthographicSize;

            return _camera.fieldOfView;
        }

        private void ResolveZoomRange(out float minZoom, out float maxZoom)
        {
            minZoom = _settings.ResolveMinZoom();
            maxZoom = Mathf.Max(minZoom + 0.1f, _settings.ResolveMaxZoom());

            if (_camera != null && !_camera.orthographic)
            {
                minZoom = Mathf.Clamp(minZoom, MinimumPerspectiveFieldOfView, MaximumPerspectiveFieldOfView - 0.1f);
                float configuredDefaultFov = _settings.ResolveDefault3DFieldOfView();
                maxZoom = Mathf.Clamp(Mathf.Max(maxZoom, configuredDefaultFov), minZoom + 0.1f, MaximumPerspectiveFieldOfView);
            }
        }

        private static float ResolveInterpolationFactor(float smoothTime)
        {
            if (smoothTime <= 0.0001f)
                return 1f;

            return 1f - Mathf.Exp(-Time.deltaTime / smoothTime);
        }

        private static float NormalizeWheelDelta(float delta)
        {
            if (Mathf.Abs(delta) <= ZoomEpsilon)
                return 0f;

            float normalized = Mathf.Abs(delta) >= RawMouseWheelThreshold
                ? delta / MouseWheelStepDelta
                : delta;

            return Mathf.Clamp(normalized, -3f, 3f);
        }

        private void UpdateGlobalMipBias()
        {
            if (!_settings.ResolveEnableAutomaticMipBias())
            {
                if (Mathf.Abs(_lastPushedMipBias) > ZoomEpsilon)
                {
                    Shader.SetGlobalFloat(GlobalMipBiasId, 0f);
                    _lastPushedMipBias = 0f;
                }
                return;
            }

            float currentZoom = GetCurrentZoom();
            ResolveZoomRange(out float minZoom, out float maxZoom);

            float normalized = maxZoom > minZoom
                ? Mathf.InverseLerp(minZoom, maxZoom, currentZoom)
                : 0f;

            float maxAutoMipBias = Mathf.Max(0f, _settings.ResolveAutomaticMipBiasMax());
            if (maxAutoMipBias <= ZoomEpsilon)
                maxAutoMipBias = MaxAutoMipBiasFallback;

            float zoomOutMipBias = normalized * maxAutoMipBias;
            float closeZoomMipBias = 0f;
            if (ShouldUseMobileCloseZoomBias())
            {
                float closeBiasEndZoom = Mathf.Lerp(minZoom, maxZoom, MobileCloseZoomBiasEnd);
                float closePressure = 1f - Mathf.Clamp01(Mathf.InverseLerp(minZoom, closeBiasEndZoom, currentZoom));
                closeZoomMipBias = closePressure * MobileCloseZoomMipBias;
            }

            float mipBias = Mathf.Max(zoomOutMipBias, closeZoomMipBias);
            if (Mathf.Abs(_lastPushedMipBias - mipBias) <= ZoomEpsilon)
                return;

            Shader.SetGlobalFloat(GlobalMipBiasId, mipBias);
            _lastPushedMipBias = mipBias;
        }

        private static bool ShouldUseMobileCloseZoomBias()
        {
#if UNITY_ANDROID || UNITY_IOS
            return true;
#else
            return Application.isMobilePlatform;
#endif
        }
    }
}
