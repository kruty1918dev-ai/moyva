using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Camera.Runtime
{
    /// <summary>
    /// Canonical read-only camera state for presentation consumers (audio,
    /// shaders, clouds, VFX, HUD). Sampled once per frame after navigation and
    /// impulse evaluation; never allocates, never mutates the camera.
    /// </summary>
    internal sealed class CameraViewStateService : ICameraViewState, ILateTickable
    {
        private const float PlayerNavigatingWindowSeconds = 0.45f;
        private const float ZoomSmoothingSpeed = 10f;

        private readonly UnityEngine.Camera _camera;
        private readonly CameraSettingsSO _settings;
        private readonly CameraMovement _movement;
        private readonly CameraZoom _zoom;
        private readonly ICameraFocusService _focus;
        private readonly ICameraFeedbackService _feedback;
        private readonly IGridProjection _gridProjection;

        private float _currentZoom;
        private float _normalizedZoom;
        private float _smoothedNormalizedZoom;
        private bool _isPerspective;
        private Vector3 _focusWorldPoint;
        private float _orbitYawDegrees;
        private CameraNavigationMode _navigationMode;
        private bool _isPlayerNavigating;

        public CameraViewStateService(
            UnityEngine.Camera camera,
            CameraSettingsSO settings,
            CameraMovement movement,
            CameraZoom zoom,
            [InjectOptional] ICameraFocusService focus = null,
            [InjectOptional] ICameraFeedbackService feedback = null,
            [InjectOptional] IGridProjection gridProjection = null)
        {
            _camera = camera;
            _settings = settings;
            _movement = movement;
            _zoom = zoom;
            _focus = focus;
            _feedback = feedback;
            _gridProjection = gridProjection;
        }

        public float CurrentZoom => _currentZoom;
        public float NormalizedZoom => _normalizedZoom;
        public float SmoothedNormalizedZoom => _smoothedNormalizedZoom;
        public bool IsPerspective => _isPerspective;
        public Vector3 FocusWorldPoint => _focusWorldPoint;
        public float OrbitYawDegrees => _orbitYawDegrees;
        public CameraNavigationMode NavigationMode => _navigationMode;
        public bool IsFocusTransitionActive => _focus != null && _focus.IsFocusActive;
        public bool IsPlayerNavigating => _isPlayerNavigating;
        public bool IsImpulseActive => _feedback != null && _feedback.IsImpulseActive;

        public void LateTick()
        {
            if (_camera == null)
                return;

            _isPerspective = !_camera.orthographic;
            _currentZoom = _camera.orthographic ? _camera.orthographicSize : _camera.fieldOfView;
            _normalizedZoom = CameraViewMath.NormalizeZoom(
                _currentZoom, _settings.ResolveMinZoom(), _settings.ResolveMaxZoom());
            _smoothedNormalizedZoom = Mathf.Lerp(
                _smoothedNormalizedZoom,
                _normalizedZoom,
                CameraViewMath.SmoothingFactor(ZoomSmoothingSpeed, Time.unscaledDeltaTime));

            if (_movement.TryGetNavigationFocusPoint(out Vector3 focusPoint))
                _focusWorldPoint = focusPoint;

            bool usesXz = _gridProjection != null && _gridProjection.WorldPlane == GridWorldPlane.XZ;
            _orbitYawDegrees = usesXz
                ? _camera.transform.eulerAngles.y
                : _camera.transform.eulerAngles.z;

            float lastInput = Mathf.Max(_movement.LastManualControlTime, _zoom.LastManualControlTime);
            _isPlayerNavigating = lastInput > float.NegativeInfinity
                && Time.unscaledTime - lastInput < PlayerNavigatingWindowSeconds;

            _navigationMode = IsFocusTransitionActive
                ? CameraNavigationMode.FocusTransition
                : _isPlayerNavigating ? CameraNavigationMode.Player : CameraNavigationMode.Idle;
        }
    }
}
