using Kruty1918.Moyva.Camera.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Camera.Runtime
{
    internal sealed class CameraZoom : ICameraZoom, IInitializable, ILateTickable
    {
        private const string GlobalMipBiasProperty = "_MoyvaTexLodBias";
        private const float MaxAutoMipBias = 3.0f;
        private const float MobileCloseZoomMipBias = 0.75f;
        private const float MobileCloseZoomBiasEnd = 0.22f;
        private const float ZoomEpsilon = 0.0005f;
        private const float MouseWheelStepDelta = 120f;
        private const float RawMouseWheelThreshold = 10f;
        private const float MaxWheelZoomStepFraction = 0.075f;
        private const float MaxPinchTargetStepFraction = 0.1f;
        private const float MaxImmediatePinchStepFraction = 0.04f;
        private const float MinZoomStep = 0.1f;
        private static readonly int GlobalMipBiasId = Shader.PropertyToID(GlobalMipBiasProperty);

        private readonly UnityEngine.Camera _camera;
        private readonly CameraSettingsSO _settings;

        private float _targetZoom;
        private float _currentVelocity; // Необхідно для Mathf.SmoothDamp
        private float _lastPushedMipBias = float.NaN;

        private float _forceBlockTimer;
        private const float ForceBlockDuration = 1.5f; // Час затримки після форсованого зуму

        public CameraZoom(UnityEngine.Camera camera, CameraSettingsSO settings)
        {
            _camera = camera;
            _settings = settings;
        }

        public void Initialize()
        {
            // На старті синхронізуємо цільовий зум з поточним Field of View або Orthographic Size
            if (_camera.orthographic)
            {
                _targetZoom = _camera.orthographicSize;
            }
            else
            {
                _targetZoom = _camera.fieldOfView;
            }

            UpdateGlobalMipBias();
        }

        public void ZoomCamera(float delta)
        {
            // Якщо діє блокування від форсованого зуму — ігноруємо інпут гравця
            if (_forceBlockTimer > 0f) return;

            float normalizedDelta = NormalizeWheelDelta(delta);
            if (Mathf.Abs(normalizedDelta) <= ZoomEpsilon)
                return;

            // Віднімаємо дельту (щоб скрол вперед наближав, а назад — віддаляв)
            // і обмежуємо крок, щоб raw wheel delta не кидав камеру відразу в min/max
            float zoomStep = Mathf.Min(Mathf.Max(0.01f, _settings.zoomSpeed), ResolveMaxZoomStep(MaxWheelZoomStepFraction));
            _targetZoom = Mathf.Clamp(
                _targetZoom - normalizedDelta * zoomStep,
                _settings.minZoom,
                _settings.maxZoom
            );
        }

        public void ZoomCameraByScale(float scaleFactor, bool immediate)
        {
            if (_forceBlockTimer > 0f) return;
            if (scaleFactor <= 0f || float.IsNaN(scaleFactor) || float.IsInfinity(scaleFactor)) return;

            float sensitivity = Mathf.Max(0.01f, _settings.touchPinchZoomSensitivity);
            float adjustedScale = Mathf.Pow(scaleFactor, sensitivity);
            float nextTarget = Mathf.Clamp(_targetZoom * adjustedScale, _settings.minZoom, _settings.maxZoom);
            _targetZoom = Mathf.MoveTowards(_targetZoom, nextTarget, ResolveMaxZoomStep(MaxPinchTargetStepFraction));

            if (!immediate)
                return;

            _currentVelocity = 0f;
            ApplyZoomValue(Mathf.MoveTowards(GetCurrentZoom(), _targetZoom, ResolveMaxZoomStep(MaxImmediatePinchStepFraction)));
            UpdateGlobalMipBias();
        }

        public void ForceZoomCamera(float zoomLevel)
        {
            // Встановлюємо новий цільовий зум (теж обмежуємо про всяк випадок)
            _targetZoom = Mathf.Clamp(zoomLevel, _settings.minZoom, _settings.maxZoom);

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

            if (_camera.orthographic)
            {
                // Логіка для 2D (Orthographic)
                ApplyZoomValue(Mathf.SmoothDamp(
                    _camera.orthographicSize,
                    _targetZoom,
                    ref _currentVelocity,
                    _settings.smoothTime));
            }
            else
            {
                // Логіка для 3D (Perspective)
                ApplyZoomValue(Mathf.SmoothDamp(
                    _camera.fieldOfView,
                    _targetZoom,
                    ref _currentVelocity,
                    _settings.smoothTime));
            }

            UpdateGlobalMipBias();
        }

        private void ApplyZoomValue(float zoomValue)
        {
            if (_camera.orthographic)
                _camera.orthographicSize = zoomValue;
            else
                _camera.fieldOfView = zoomValue;
        }

        private float GetCurrentZoom()
        {
            return _camera.orthographic ? _camera.orthographicSize : _camera.fieldOfView;
        }

        private float ResolveMaxZoomStep(float fraction)
        {
            float zoomRange = Mathf.Max(MinZoomStep, _settings.maxZoom - _settings.minZoom);
            return Mathf.Max(MinZoomStep, zoomRange * Mathf.Clamp01(fraction));
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
            float currentZoom = GetCurrentZoom();
            float minZoom = _settings.minZoom;
            float maxZoom = _settings.maxZoom;

            float normalized = maxZoom > minZoom
                ? Mathf.InverseLerp(minZoom, maxZoom, currentZoom)
                : 0f;

            float zoomOutMipBias = normalized * MaxAutoMipBias;
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
