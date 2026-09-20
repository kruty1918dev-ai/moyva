using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Shared.Controls;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Camera.Runtime
{
    /// <summary>
    /// Interruptible camera focus transitions. Steers the navigation target
    /// and zoom along an eased curve; any player navigation input cancels the
    /// transition immediately (player input always wins). When Smooth Camera
    /// Focus is disabled the transition applies instantly.
    /// </summary>
    internal sealed class CameraFocusService : ICameraFocusService, ILateTickable, IDisposable
    {
        private readonly CameraMovement _movement;
        private readonly CameraZoom _zoom;
        private readonly CameraSettingsSO _settings;
        private readonly UnityEngine.Camera _camera;
        private readonly IPlayerControlSettingsService _controlSettings;
        private readonly IGridProjection _gridProjection;

        private bool _active;
        private float _elapsed;
        private float _duration;
        private Vector3 _startFocus;
        private Vector3 _endFocus;
        private float _startZoom;
        private float _endZoom;

        public CameraFocusService(
            CameraMovement movement,
            CameraZoom zoom,
            CameraSettingsSO settings,
            UnityEngine.Camera camera,
            [InjectOptional] IPlayerControlSettingsService controlSettings = null,
            [InjectOptional] IGridProjection gridProjection = null)
        {
            _movement = movement;
            _zoom = zoom;
            _settings = settings;
            _camera = camera;
            _controlSettings = controlSettings;
            _gridProjection = gridProjection;

            _movement.ManualControlRequested += OnManualControlRequested;
            _zoom.ManualControlRequested += OnManualControlRequested;
        }

        public bool IsFocusActive => _active;

        public void Dispose()
        {
            _movement.ManualControlRequested -= OnManualControlRequested;
            _zoom.ManualControlRequested -= OnManualControlRequested;
        }

        public void FocusWorldPoint(Vector3 worldPoint, CameraFocusRequest request = default)
        {
            if (_camera == null)
                return;

            float targetZoom = ResolveTargetZoom(request, _zoom.CurrentZoom);
            BeginTransition(worldPoint, targetZoom, request);
        }

        public void FocusBounds(Bounds worldBounds, CameraFocusRequest request = default)
        {
            if (_camera == null)
                return;

            var focusSettings = _settings.ResolveFocus();
            float padding = request.Padding > 0f ? request.Padding : focusSettings.boundsPadding;
            float fitZoom = ResolveFitZoom(worldBounds, padding);
            float currentZoom = _zoom.CurrentZoom;

            float targetZoom;
            if (request.KeepCurrentZoom)
            {
                targetZoom = currentZoom;
            }
            else if (request.TargetZoom.HasValue)
            {
                targetZoom = ClampZoom(request.TargetZoom.Value);
            }
            else
            {
                // Never zoom out below the fit requirement; zoom in only when
                // the request explicitly allows framing the object closer.
                targetZoom = request.AllowZoomIn
                    ? fitZoom
                    : Mathf.Max(currentZoom, fitZoom);

                float relativeChange = Mathf.Abs(targetZoom - currentZoom) / Mathf.Max(0.01f, currentZoom);
                if (relativeChange < focusSettings.zoomChangeThreshold)
                    targetZoom = currentZoom;
            }

            BeginTransition(worldBounds.center, targetZoom, request);
        }

        public void FocusObject(GameObject target, CameraFocusRequest request = default)
        {
            if (target == null)
                return;

            var renderer = target.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                var boundsRequest = request;
                boundsRequest.AllowZoomIn = true;
                FocusBounds(renderer.bounds, boundsRequest);
                return;
            }

            FocusWorldPoint(target.transform.position, request);
        }

        public void CancelFocus()
        {
            if (!_active)
                return;

            _active = false;
            _movement.StopNavigationAtCurrent();
            _zoom.SetTargetZoom(_zoom.CurrentZoom);
        }

        public void LateTick()
        {
            if (!_active)
                return;

            _elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(_elapsed / Mathf.Max(0.0001f, _duration));
            float eased = CameraViewMath.EaseInOutCubic(t);

            _movement.MoveCameraFocusToWorldPoint(Vector3.Lerp(_startFocus, _endFocus, eased), immediate: true);
            _zoom.SetZoomImmediate(Mathf.Lerp(_startZoom, _endZoom, eased));

            if (t >= 1f)
                _active = false;
        }

        private void OnManualControlRequested()
        {
            CancelFocus();
        }

        private void BeginTransition(Vector3 endFocus, float endZoom, CameraFocusRequest request)
        {
            if (!_movement.TryGetNavigationFocusPoint(out Vector3 startFocus))
                startFocus = endFocus;

            _startFocus = startFocus;
            _endFocus = endFocus;
            _startZoom = _zoom.CurrentZoom;
            _endZoom = ClampZoom(endZoom);

            float distance = NavigationPlaneDistance(startFocus, endFocus);
            var focusSettings = _settings.ResolveFocus();
            float duration = request.DurationOverride
                ?? CameraViewMath.ResolveFocusDuration(distance, focusSettings);

            if (_controlSettings != null && _controlSettings.Settings.ReduceCameraMotion)
                duration *= focusSettings.reducedMotionDurationScale;

            bool smooth = _controlSettings == null || _controlSettings.Settings.SmoothCameraFocus;
            bool negligible = distance < 0.05f
                && Mathf.Abs(_endZoom - _startZoom) < 0.01f;

            if (!smooth || negligible || duration <= 0.05f)
            {
                _active = false;
                _movement.MoveCameraFocusToWorldPoint(endFocus, immediate: true);
                _zoom.SetZoomImmediate(_endZoom);
                return;
            }

            _elapsed = 0f;
            _duration = duration;
            _active = true;
        }

        private float ResolveTargetZoom(CameraFocusRequest request, float currentZoom)
        {
            if (request.KeepCurrentZoom)
                return currentZoom;
            return request.TargetZoom.HasValue ? ClampZoom(request.TargetZoom.Value) : currentZoom;
        }

        private float ResolveFitZoom(Bounds worldBounds, float padding)
        {
            Transform cameraTransform = _camera.transform;
            CameraViewMath.ResolveViewHalfExtents(
                worldBounds,
                cameraTransform.right,
                cameraTransform.up,
                out float halfWidth,
                out float halfHeight);

            if (_camera.orthographic)
                return CameraViewMath.ResolveOrthographicSizeForBounds(halfWidth, halfHeight, _camera.aspect, padding);

            float distance = Mathf.Max(0.1f, Vector3.Distance(cameraTransform.position, worldBounds.center));
            return CameraViewMath.ResolvePerspectiveFovForBounds(halfWidth, halfHeight, _camera.aspect, distance, padding);
        }

        private float ClampZoom(float zoom)
            => Mathf.Clamp(zoom, _settings.ResolveMinZoom(), _settings.ResolveMaxZoom());

        private float NavigationPlaneDistance(Vector3 a, Vector3 b)
        {
            Vector3 delta = b - a;
            if (_gridProjection != null && _gridProjection.WorldPlane == GridWorldPlane.XZ)
                return new Vector2(delta.x, delta.z).magnitude;
            return new Vector2(delta.x, delta.y).magnitude;
        }
    }
}
