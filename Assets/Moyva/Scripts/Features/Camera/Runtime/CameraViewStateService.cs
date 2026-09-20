using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Camera.Runtime
{
    /// <summary>CameraViewStateService — class: камери виду стану сервісу.</summary>
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

        /// <summary>Виконує CameraViewStateService.</summary>
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

        /// <summary>поточного зум — float.</summary>
        public float CurrentZoom => _currentZoom;
        /// <summary>Normalized зум — float.</summary>
        public float NormalizedZoom => _normalizedZoom;
        /// <summary>Smoothed Normalized зум — float.</summary>
        public float SmoothedNormalizedZoom => _smoothedNormalizedZoom;
        /// <summary>Чи перспективи — IsPerspective.</summary>
        public bool IsPerspective => _isPerspective;
        /// <summary>фокус світу точки — Vector3.</summary>
        public Vector3 FocusWorldPoint => _focusWorldPoint;
        /// <summary>орбіти рискання у градусах — float.</summary>
        public float OrbitYawDegrees => _orbitYawDegrees;
        /// <summary>Navigation режим — CameraNavigationMode.</summary>
        public CameraNavigationMode NavigationMode => _navigationMode;
        /// <summary>Чи фокус переходу активної — IsFocusTransitionActive.</summary>
        public bool IsFocusTransitionActive => _focus != null && _focus.IsFocusActive;
        /// <summary>Чи гравця Navigating — IsPlayerNavigating.</summary>
        public bool IsPlayerNavigating => _isPlayerNavigating;
        /// <summary>Чи імпульсу активної — IsImpulseActive.</summary>
        public bool IsImpulseActive => _feedback != null && _feedback.IsImpulseActive;

        /// <summary>Виконує LateTick.</summary>
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
