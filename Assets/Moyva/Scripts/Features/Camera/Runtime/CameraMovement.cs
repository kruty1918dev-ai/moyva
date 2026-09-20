using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Camera.Runtime
{
    /// <summary>CameraMovement — class: камери руху.</summary>
    internal sealed class CameraMovement : ICameraMovement, IInitializable, ILateTickable
    {
        private const float RotationEpsilon = 0.0001f;

        private readonly UnityEngine.Camera _camera;
        private readonly CameraSettingsSO _settings;
        private readonly ICameraBoundsProvider _boundsProvider;
        private readonly IGridProjection _gridProjection;
        private readonly CameraImpulseService _impulse;

        private Vector3 _targetPosition;
        private Vector3 _currentVelocity; // Необхідно для Vector3.SmoothDamp
        private Vector3 _lastComposedImpulseOffset;
        private float _fixedPlaneAxisValue;
        private readonly Vector3[] _viewportWorldCorners = new Vector3[4];
        private float _keyboardOrbitInput;
        private float _orbitAngularVelocity;
        private bool _pointerOrbitActive;
        private bool _hasOrbitPivot;
        private Vector3 _orbitPivot;

        /// <summary>ручного керування запитаного.</summary>
        public event System.Action ManualControlRequested;

        /// <summary>Unscaled time of the most recent player navigation intent.</summary>
        internal float LastManualControlTime { get; private set; } = float.NegativeInfinity;

        // Zenject автоматично підставить активну камеру та налаштування
        /// <summary>Виконує CameraMovement.</summary>
        public CameraMovement(
            UnityEngine.Camera camera,
            CameraSettingsSO settings,
            [InjectOptional] ICameraBoundsProvider boundsProvider = null,
            [InjectOptional] IGridProjection gridProjection = null,
            [InjectOptional] CameraImpulseService impulse = null)
        {
            _camera = camera;
            _settings = settings;
            _boundsProvider = boundsProvider;
            _gridProjection = gridProjection;
            _impulse = impulse;
        }

        /// <summary>Ініціалізує компонент і підписує на події.</summary>
        public void Initialize()
        {
            // На старті синхронізуємо цільову позицію з поточною, щоб камера не відлітала
            _targetPosition = _camera.transform.position;
            _fixedPlaneAxisValue = UsesXzPlane ? _targetPosition.y : _settings.defaultCameraZ;
            ClampTargetToBounds();
        }

        /// <summary>Переміщує камери.</summary>
        public void MoveCamera(Vector3 delta) // delta — це чисті пікселі з Action Map
        {
            if (delta.sqrMagnitude <= RotationEpsilon)
                return;

            NotifyManualControl();
            ApplyScreenDelta(delta, _settings.ResolveMoveSpeed(), immediate: false);
        }

        /// <summary>Переміщує камери клавіатури.</summary>
        public void MoveCameraKeyboard(Vector2 direction, float unscaledDeltaTime)
        {
            if (direction.sqrMagnitude <= RotationEpsilon)
                return;

            NotifyManualControl();

            Vector2 normalized = direction.sqrMagnitude > 1f ? direction.normalized : direction;
            Transform cameraTransform = _camera.transform;
            Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, ResolveNavigationPlaneNormal()).normalized;
            Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, ResolveNavigationPlaneNormal()).normalized;

            if (right.sqrMagnitude <= RotationEpsilon || forward.sqrMagnitude <= RotationEpsilon)
                return;

            Vector3 worldDelta = (-right * normalized.x - forward * normalized.y)
                * _settings.ResolveMoveSpeed()
                * ResolveMoveZoomMultiplier()
                * Mathf.Max(0f, unscaledDeltaTime);
            ApplyNavigationDelta(worldDelta);
        }

        /// <summary>Переміщує камери негайного.</summary>
        public void MoveCameraImmediate(Vector3 delta, float speedMultiplier)
        {
            if (delta.sqrMagnitude <= RotationEpsilon)
                return;

            NotifyManualControl();
            ApplyScreenDelta(delta, speedMultiplier, immediate: true);
        }

        /// <summary>Обертає камери Around фокус точки.</summary>
        public void RotateCameraAroundFocusPoint(float angleDegrees)
        {
            if (_camera == null
                || float.IsNaN(angleDegrees)
                || float.IsInfinity(angleDegrees)
                || Mathf.Abs(angleDegrees) <= RotationEpsilon)
            {
                return;
            }

            if (!EnsureOrbitPivot())
                return;

            NotifyManualControl();
            ApplyOrbitAngle(angleDegrees);
        }

        /// <summary>Встановлює камери орбіти вводу.</summary>
        public void SetCameraOrbitInput(float normalizedInput)
        {
            _keyboardOrbitInput = Mathf.Clamp(normalizedInput, -1f, 1f);
            if (Mathf.Abs(_keyboardOrbitInput) > RotationEpsilon)
            {
                NotifyManualControl();
                EnsureOrbitPivot();
            }
        }

        /// <summary>Починає курсора орбіти.</summary>
        public void BeginPointerOrbit()
        {
            _pointerOrbitActive = EnsureOrbitPivot();
            if (_pointerOrbitActive)
                NotifyManualControl();
        }

        /// <summary>Обертає курсора орбіти.</summary>
        public void RotatePointerOrbit(float horizontalScreenDelta)
        {
            if (!_pointerOrbitActive || Mathf.Abs(horizontalScreenDelta) <= RotationEpsilon)
                return;

            NotifyManualControl();
            float angle = horizontalScreenDelta
                * _settings.ResolvePointerOrbitSensitivity()
                * ResolveOrbitZoomMultiplier();
            ApplyOrbitAngle(angle);
        }

        /// <summary>Завершує курсора орбіти.</summary>
        public void EndPointerOrbit()
        {
            _pointerOrbitActive = false;
            ReleaseOrbitPivotWhenIdle();
        }

        private void ApplyOrbitAngle(float angleDegrees)
        {
            if (!_hasOrbitPivot)
                return;

            Quaternion rotationDelta = Quaternion.AngleAxis(angleDegrees, ResolveNavigationPlaneNormal());
            Transform cameraTransform = _camera.transform;

            Vector3 rotatedPosition = _orbitPivot + rotationDelta * (cameraTransform.position - _orbitPivot);
            Quaternion rotatedRotation = rotationDelta * cameraTransform.rotation;
            cameraTransform.rotation = rotatedRotation;
            WriteCameraPosition(rotatedPosition);

            _targetPosition = _orbitPivot + rotationDelta * (_targetPosition - _orbitPivot);
            _currentVelocity = rotationDelta * _currentVelocity;
            ApplyFixedPlaneAxis();
            ClampTargetToBounds();
        }

        /// <summary>Виконує ShiftCameraWorld.</summary>
        public void ShiftCameraWorld(Vector3 worldDelta, bool immediate)
        {
            NotifyManualControl();

            if (UsesXzPlane)
                worldDelta.y = 0f;
            else
                worldDelta.z = 0f;

            ApplyNavigationDelta(worldDelta);

            if (!immediate)
                return;

            _currentVelocity = Vector3.zero;
            WriteCameraPosition(_targetPosition);
        }

        /// <summary>Переміщує камери фокус  світу точки.</summary>
        public void MoveCameraFocusToWorldPoint(Vector3 focusPoint, bool immediate)
        {
            if (_camera == null)
                return;

            if (!TryResolveNavigationPlaneCenter(_targetPosition, out _, out float distanceToPlane))
                distanceToPlane = ResolveDefaultDistanceToNavigationPlane();

            _targetPosition = focusPoint - _camera.transform.forward * Mathf.Max(0.1f, distanceToPlane);
            _fixedPlaneAxisValue = UsesXzPlane ? _targetPosition.y : _targetPosition.z;
            ClampTargetToBounds();

            if (!immediate)
                return;

            _currentVelocity = Vector3.zero;
            WriteCameraPosition(_targetPosition);
        }

        /// <summary>Встановлює камери відстані  Navigation площини.</summary>
        public void SetCameraDistanceToNavigationPlane(float distance, bool immediate)
        {
            if (_camera == null)
                return;

            float resolvedDistance = Mathf.Max(0.1f, distance);
            if (!TryResolveNavigationPlaneCenter(_targetPosition, out Vector3 focusPoint, out _)
                && !TryResolveNavigationPlaneCenter(_camera.transform.position, out focusPoint, out _))
            {
                return;
            }

            _targetPosition = focusPoint - _camera.transform.forward * resolvedDistance;
            _fixedPlaneAxisValue = UsesXzPlane ? _targetPosition.y : _targetPosition.z;

            if (!immediate)
                return;

            _currentVelocity = Vector3.zero;
            WriteCameraPosition(_targetPosition);
        }

        private void ApplyScreenDelta(Vector3 delta, float speedMultiplier, bool immediate)
        {
            Vector3 moveDirection = TryResolveDragWorldDelta(new Vector2(delta.x, delta.y), out var dragWorldDelta)
                ? dragWorldDelta
                : ResolveFallbackScreenDelta(delta);

            ApplyNavigationDelta(moveDirection * speedMultiplier);

            if (!immediate)
                return;

            _currentVelocity = Vector3.zero;
            WriteCameraPosition(_targetPosition);
        }

        /// <summary>Примусово задає переміщення камери  позицію.</summary>
        public void ForceMoveCameraToPosition(Vector3 position)
        {
            // Programmatic steer toward a position. Player input always wins —
            // there is no input lock; use CameraFocusService for transitions.
            _targetPosition = position;
            ApplyFixedPlaneAxis();
            ClampTargetToBounds();
        }

        /// <summary>Виконує TeleportCamera.</summary>
        public void TeleportCamera(Vector3 position)
        {
            _targetPosition = position;
            ApplyFixedPlaneAxis();
            ClampTargetToBounds();
            _currentVelocity = Vector3.zero;
            WriteCameraPosition(_targetPosition);
        }

        /// <summary>Виконує TeleportCameraToFocusPoint.</summary>
        public void TeleportCameraToFocusPoint(Vector3 focusPoint, float distance)
        {
            float resolvedDistance = Mathf.Max(0.1f, distance);
            _targetPosition = focusPoint - _camera.transform.forward * resolvedDistance;
            _fixedPlaneAxisValue = UsesXzPlane ? _targetPosition.y : _targetPosition.z;
            ClampTargetToBounds();
            _currentVelocity = Vector3.zero;
            WriteCameraPosition(_targetPosition);
        }

        /// <summary>
        /// Stops navigation at the current visual position. Used when an
        /// automatic transition is cancelled — the pose must not keep drifting.
        /// </summary>
        internal void StopNavigationAtCurrent()
        {
            _targetPosition = _camera.transform.position - _lastComposedImpulseOffset;
            _currentVelocity = Vector3.zero;
            ApplyFixedPlaneAxis();
        }

        /// <summary>Focus point of the current pose on the navigation plane.</summary>
        internal bool TryGetNavigationFocusPoint(out Vector3 focusPoint)
            => TryResolveNavigationPlaneCenter(_camera.transform.position, out focusPoint, out _);

        private void NotifyManualControl()
        {
            LastManualControlTime = Time.unscaledTime;
            ManualControlRequested?.Invoke();
        }

        /// <summary>
        /// Writes the composed final pose: navigation pose plus the active
        /// impulse offset. The impulse offset is tracked so the canonical
        /// navigation pose can be recovered exactly on the next frame.
        /// </summary>
        private void WriteCameraPosition(Vector3 navigationPosition)
        {
            _lastComposedImpulseOffset = _impulse != null && _impulse.IsImpulseActive
                ? _impulse.PositionOffset
                : Vector3.zero;
            _camera.transform.position = navigationPosition + _lastComposedImpulseOffset;
        }

        /// <summary>Виконує LateTick.</summary>
        public void LateTick()
        {
            UpdateKeyboardOrbit(Time.unscaledDeltaTime);
            ApplySoftBoundsReturn(Time.unscaledDeltaTime);

            // Плавно рухаємо камеру до _targetPosition. The impulse offset is
            // stripped before smoothing so shake never corrupts the base pose.
            Vector3 basePosition = _camera.transform.position - _lastComposedImpulseOffset;
            Vector3 smoothed = Vector3.SmoothDamp(
                basePosition,
                _targetPosition,
                ref _currentVelocity,
                _settings.ResolveSmoothTime()
            );
            WriteCameraPosition(smoothed);
        }

        /// <summary>
        /// Elastic pull-back when the navigation target sits inside the soft
        /// bounds margin: the edge flexes under input instead of hard-stopping,
        /// then eases back once the player releases.
        /// </summary>
        private void ApplySoftBoundsReturn(float unscaledDeltaTime)
        {
            if (_settings.ResolveBoundsSoftMargin() <= 0.0001f)
                return;

            if (!TryClampTargetPosition(_targetPosition, 0f, out Vector3 strictTarget))
                return;

            float pull = 1f - Mathf.Exp(-_settings.ResolveBoundsSoftReturnSpeed() * Mathf.Max(0f, unscaledDeltaTime));
            _targetPosition = Vector3.Lerp(_targetPosition, strictTarget, pull);
        }

        private void ClampTargetToBounds()
            => ClampTargetToBounds(0f);

        private void ClampTargetToBounds(float margin)
        {
            if (TryClampTargetPosition(_targetPosition, margin, out Vector3 clamped))
                _targetPosition = clamped;
            ApplyFixedPlaneAxis();
        }

        private bool TryClampTargetPosition(Vector3 target, float margin, out Vector3 clamped)
        {
            clamped = target;
            if (_boundsProvider == null || _camera == null)
                return false;

            if (!TryResolveNavigationPlaneCenter(target, out var targetPlaneCenter, out float distanceToPlane))
                return false;

            if (!TryResolveTargetPlaneBounds(
                    target,
                    margin,
                    out float minX,
                    out float maxX,
                    out float minY,
                    out float maxY))
            {
                return false;
            }

            Vector2 targetPlane = ToNavigationPlaneCoordinates(targetPlaneCenter);
            targetPlane.x = Mathf.Clamp(targetPlane.x, minX, maxX);
            targetPlane.y = Mathf.Clamp(targetPlane.y, minY, maxY);
            var clampedFocus = UsesXzPlane
                ? new Vector3(targetPlane.x, 0f, targetPlane.y)
                : new Vector3(targetPlane.x, targetPlane.y, 0f);
            clamped = clampedFocus - _camera.transform.forward * distanceToPlane;
            return true;
        }

        private bool UsesXzPlane => _gridProjection != null && _gridProjection.WorldPlane == GridWorldPlane.XZ;

        private void ApplyNavigationDelta(Vector3 worldDelta)
        {
            if (UsesXzPlane)
                worldDelta.y = 0f;
            else
                worldDelta.z = 0f;

            if (TryFilterNavigationDeltaAgainstBounds(worldDelta, out Vector3 filteredDelta))
                worldDelta = filteredDelta;

            _targetPosition += worldDelta;
            ApplyFixedPlaneAxis();
            ClampTargetToBounds(_settings.ResolveBoundsSoftMargin());
        }

        private bool TryFilterNavigationDeltaAgainstBounds(
            Vector3 worldDelta,
            out Vector3 filteredDelta)
        {
            filteredDelta = worldDelta;
            if (!TryResolveNavigationPlaneCenter(_targetPosition, out Vector3 currentCenter, out _)
                || !TryResolveTargetPlaneBounds(
                    _targetPosition,
                    out float minX,
                    out float maxX,
                    out float minY,
                    out float maxY))
            {
                return false;
            }

            Vector2 currentPlane = ToNavigationPlaneCoordinates(currentCenter);
            Vector2 deltaPlane = ToNavigationPlaneDelta(worldDelta);
            deltaPlane.x = ClampDeltaAxis(currentPlane.x, deltaPlane.x, minX, maxX);
            deltaPlane.y = ClampDeltaAxis(currentPlane.y, deltaPlane.y, minY, maxY);
            filteredDelta = FromNavigationPlaneDelta(deltaPlane);
            return true;
        }

        private bool TryResolveTargetPlaneBounds(
            Vector3 cameraPosition,
            out float minX,
            out float maxX,
            out float minY,
            out float maxY)
            => TryResolveTargetPlaneBounds(cameraPosition, 0f, out minX, out maxX, out minY, out maxY);

        private bool TryResolveTargetPlaneBounds(
            Vector3 cameraPosition,
            float margin,
            out float minX,
            out float maxX,
            out float minY,
            out float maxY)
        {
            minX = maxX = minY = maxY = 0f;
            if (_boundsProvider == null || _camera == null)
                return false;

            var bounds = _boundsProvider.GetWorldBounds();
            if (!bounds.HasValue)
                return false;

            Vector2 overflow = _settings.ResolveBoundsOverflowWorldUnits();
            float minBoundX = bounds.MinX - overflow.x;
            float maxBoundX = bounds.MaxX + overflow.x;
            float minBoundY = bounds.MinY - overflow.y;
            float maxBoundY = bounds.MaxY + overflow.y;

            ResolveViewportHalfExtents(cameraPosition, out float halfW, out float halfH);
            float width = Mathf.Max(0.01f, maxBoundX - minBoundX);
            float height = Mathf.Max(0.01f, maxBoundY - minBoundY);

            if (width <= halfW * 2f)
            {
                minX = maxX = (minBoundX + maxBoundX) * 0.5f;
            }
            else
            {
                minX = minBoundX + halfW;
                maxX = maxBoundX - halfW;
            }

            if (height <= halfH * 2f)
            {
                minY = maxY = (minBoundY + maxBoundY) * 0.5f;
            }
            else
            {
                minY = minBoundY + halfH;
                maxY = maxBoundY - halfH;
            }

            minX -= margin;
            maxX += margin;
            minY -= margin;
            maxY += margin;

            return true;
        }

        /// <summary>
        /// Clamps a per-axis navigation delta against the bounds. Inside the
        /// soft margin, outward motion is resisted proportionally to the
        /// remaining room instead of hard-stopping; the target never rests
        /// past min/max ± margin.
        /// </summary>
        private float ClampDeltaAxis(
            float current,
            float delta,
            float min,
            float max)
        {
            const float BoundaryEpsilon = 0.001f;
            float margin = _settings.ResolveBoundsSoftMargin();

            float next = current + delta;
            if (next >= min && next <= max)
                return delta;

            // Moving inward while outside the range is always free.
            if (delta > 0f && next <= min)
                return delta;
            if (delta < 0f && next >= max)
                return delta;

            float outwardDepth = delta > 0f
                ? Mathf.Max(0f, current - max)
                : Mathf.Max(0f, min - current);

            float magnitude = Mathf.Abs(delta);
            float freeRoom = Mathf.Max(0f, Mathf.Abs((delta > 0f ? max : min) - current));
            if (outwardDepth > BoundaryEpsilon)
                freeRoom = 0f;

            if (margin <= BoundaryEpsilon)
                return Mathf.Sign(delta) * Mathf.Min(magnitude, freeRoom);

            float outward = magnitude - Mathf.Min(magnitude, freeRoom);
            float softRoom = Mathf.Max(0f, margin - outwardDepth);
            float resisted = outward * Mathf.Clamp01(softRoom / margin);
            float applied = Mathf.Min(magnitude, freeRoom + Mathf.Min(resisted, softRoom));
            return Mathf.Sign(delta) * applied;
        }

        private Vector3 ResolveFallbackScreenDelta(Vector3 delta)
        {
            float unitsPerPixel = ResolveFallbackUnitsPerPixel();
            return UsesXzPlane
                ? new Vector3(-delta.x * unitsPerPixel, 0f, -delta.y * unitsPerPixel)
                : new Vector3(-delta.x * unitsPerPixel, -delta.y * unitsPerPixel, 0f);
        }

        private float ResolveFallbackUnitsPerPixel()
        {
            float screenHeight = Mathf.Max(1f, Screen.height);
            if (_camera.orthographic)
                return (_camera.orthographicSize * 2f) / screenHeight;

            float distanceToPlane = Mathf.Max(1f, Mathf.Abs(Vector3.Dot(_camera.transform.position, ResolveNavigationPlaneNormal())));
            float worldHeight = 2f * distanceToPlane * Mathf.Tan(_camera.fieldOfView * Mathf.Deg2Rad * 0.5f);
            return Mathf.Max(0.001f, worldHeight / screenHeight);
        }

        private float ResolveDefaultDistanceToNavigationPlane()
        {
            if (_camera == null)
                return 1f;

            Vector3 normal = ResolveNavigationPlaneNormal();
            float denominator = Vector3.Dot(normal, _camera.transform.forward);
            if (Mathf.Abs(denominator) <= 0.0001f)
                return 1f;

            float distance = -Vector3.Dot(normal, _camera.transform.position) / denominator;
            return distance > 0.1f && !float.IsNaN(distance) && !float.IsInfinity(distance)
                ? distance
                : 1f;
        }

        private bool TryResolveDragWorldDelta(Vector2 screenDelta, out Vector3 worldDelta)
        {
            worldDelta = Vector3.zero;
            if (_camera == null || Screen.width <= 0 || Screen.height <= 0)
                return false;

            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            if (!TryScreenPointToNavigationPlane(screenCenter, out var before)
                || !TryScreenPointToNavigationPlane(screenCenter + screenDelta, out var after))
            {
                return false;
            }

            worldDelta = before - after;
            if (UsesXzPlane)
                worldDelta.y = 0f;
            else
                worldDelta.z = 0f;

            return worldDelta.sqrMagnitude > 0.000001f;
        }

        /// <summary>Намагається екрана точки  Navigation площини.</summary>
        public bool TryScreenPointToNavigationPlane(Vector2 screenPoint, out Vector3 worldPoint)
        {
            worldPoint = Vector3.zero;
            if (_camera == null)
                return false;

            Ray ray = _camera.ScreenPointToRay(screenPoint);
            Plane navigationPlane = new Plane(ResolveNavigationPlaneNormal(), Vector3.zero);
            if (!navigationPlane.Raycast(ray, out float distance) || distance < 0f)
                return false;

            worldPoint = ray.GetPoint(distance);
            return true;
        }

        private bool TryResolveNavigationPlaneCenter(Vector3 cameraPosition, out Vector3 worldPoint, out float distance)
        {
            worldPoint = Vector3.zero;
            distance = 0f;
            if (_camera == null)
                return false;

            Vector3 normal = ResolveNavigationPlaneNormal();
            Vector3 direction = _camera.transform.forward;
            float denominator = Vector3.Dot(normal, direction);
            if (Mathf.Abs(denominator) <= 0.0001f)
                return false;

            distance = -Vector3.Dot(normal, cameraPosition) / denominator;
            if (distance < 0f)
                return false;

            worldPoint = cameraPosition + direction * distance;
            return true;
        }

        private bool TryResolveRotationPivot(out Vector3 pivot)
        {
            Transform cameraTransform = _camera.transform;
            Ray forwardRay = new Ray(cameraTransform.position, cameraTransform.forward);
            if (Physics.Raycast(
                    forwardRay,
                    out RaycastHit hit,
                    _settings.ResolveRotationPivotRaycastDistance(),
                    _settings.ResolveRotationPivotLayerMask(),
                    QueryTriggerInteraction.Ignore))
            {
                pivot = hit.point;
                return true;
            }

            return TryResolveNavigationPlaneCenter(cameraTransform.position, out pivot, out _);
        }

        private bool EnsureOrbitPivot()
        {
            if (_hasOrbitPivot)
                return true;

            if (!TryResolveRotationPivot(out _orbitPivot))
                return false;

            _hasOrbitPivot = true;
            return true;
        }

        private void UpdateKeyboardOrbit(float unscaledDeltaTime)
        {
            float targetVelocity = _keyboardOrbitInput
                * _settings.ResolveRotationSpeed()
                * ResolveOrbitZoomMultiplier();
            float acceleration = Mathf.Abs(targetVelocity) > RotationEpsilon
                ? _settings.ResolveRotationAcceleration()
                : _settings.ResolveRotationDeceleration();

            _orbitAngularVelocity = CameraOrbitMath.Advance(
                _orbitAngularVelocity,
                targetVelocity,
                acceleration,
                unscaledDeltaTime,
                out float angleDelta);

            if (Mathf.Abs(angleDelta) > RotationEpsilon && EnsureOrbitPivot())
                ApplyOrbitAngle(angleDelta);

            ReleaseOrbitPivotWhenIdle();
        }

        private float ResolveZoomNormalized()
        {
            float zoom = _camera.orthographic ? _camera.orthographicSize : _camera.fieldOfView;
            return CameraViewMath.NormalizeZoom(zoom, _settings.ResolveMinZoom(), _settings.ResolveMaxZoom());
        }

        private float ResolveOrbitZoomMultiplier()
        {
            return Mathf.Lerp(
                _settings.ResolveCloseZoomRotationMultiplier(),
                _settings.ResolveFarZoomRotationMultiplier(),
                ResolveZoomNormalized());
        }

        /// <summary>
        /// Pan feels too fast up close and too slow zoomed out — scale the
        /// keyboard/edge/gamepad pan speed with the normalized zoom.
        /// </summary>
        private float ResolveMoveZoomMultiplier()
        {
            return Mathf.Lerp(
                _settings.ResolveCloseZoomMoveMultiplier(),
                _settings.ResolveFarZoomMoveMultiplier(),
                ResolveZoomNormalized());
        }

        private void ReleaseOrbitPivotWhenIdle()
        {
            if (_pointerOrbitActive
                || Mathf.Abs(_keyboardOrbitInput) > RotationEpsilon
                || Mathf.Abs(_orbitAngularVelocity) > RotationEpsilon)
            {
                return;
            }

            _hasOrbitPivot = false;
        }

        private void ResolveViewportHalfExtents(Vector3 cameraPosition, out float halfWidth, out float halfHeight)
        {
            halfHeight = _camera.orthographic ? _camera.orthographicSize : 0f;
            halfWidth = halfHeight * _camera.aspect;

            if (_camera.orthographic)
                return;

            if (!TryResolveNavigationPlaneViewportHalfExtents(cameraPosition, out var planeHalfExtents))
                return;

            halfWidth = planeHalfExtents.x;
            halfHeight = planeHalfExtents.y;
        }

        private bool TryResolveNavigationPlaneViewportHalfExtents(Vector3 cameraPosition, out Vector2 halfExtents)
        {
            halfExtents = Vector2.zero;
            if (_camera == null || Screen.width <= 0 || Screen.height <= 0)
                return false;

            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            if (!TryScreenPointToNavigationPlane(screenCenter, cameraPosition, out var centerWorld))
                return false;

            _viewportWorldCorners[0] = new Vector3(0f, 0f, 0f);
            _viewportWorldCorners[1] = new Vector3(Screen.width, 0f, 0f);
            _viewportWorldCorners[2] = new Vector3(0f, Screen.height, 0f);
            _viewportWorldCorners[3] = new Vector3(Screen.width, Screen.height, 0f);

            Vector2 centerPlane = ToNavigationPlaneCoordinates(centerWorld);
            Vector2 maxOffset = Vector2.zero;
            for (int cornerIndex = 0; cornerIndex < _viewportWorldCorners.Length; cornerIndex++)
            {
                Vector3 screenCorner = _viewportWorldCorners[cornerIndex];
                if (!TryScreenPointToNavigationPlane(new Vector2(screenCorner.x, screenCorner.y), cameraPosition, out var cornerWorld))
                    return false;

                Vector2 offset = ToNavigationPlaneCoordinates(cornerWorld) - centerPlane;
                maxOffset.x = Mathf.Max(maxOffset.x, Mathf.Abs(offset.x));
                maxOffset.y = Mathf.Max(maxOffset.y, Mathf.Abs(offset.y));
            }

            if (maxOffset.x <= 0.0001f || maxOffset.y <= 0.0001f)
                return false;

            halfExtents = maxOffset;
            return true;
        }

        private bool TryScreenPointToNavigationPlane(Vector2 screenPoint, Vector3 cameraPosition, out Vector3 worldPoint)
        {
            worldPoint = Vector3.zero;
            if (_camera == null)
                return false;

            Ray cameraRay = _camera.ScreenPointToRay(screenPoint);
            Ray ray = new Ray(cameraPosition, cameraRay.direction);
            Plane navigationPlane = new Plane(ResolveNavigationPlaneNormal(), Vector3.zero);
            if (!navigationPlane.Raycast(ray, out float distance) || distance < 0f)
                return false;

            worldPoint = ray.GetPoint(distance);
            return true;
        }

        private Vector2 ToNavigationPlaneCoordinates(Vector3 worldPosition)
        {
            return UsesXzPlane
                ? new Vector2(worldPosition.x, worldPosition.z)
                : new Vector2(worldPosition.x, worldPosition.y);
        }

        private Vector2 ToNavigationPlaneDelta(Vector3 worldDelta)
        {
            return UsesXzPlane
                ? new Vector2(worldDelta.x, worldDelta.z)
                : new Vector2(worldDelta.x, worldDelta.y);
        }

        private Vector3 FromNavigationPlaneDelta(Vector2 planeDelta)
        {
            return UsesXzPlane
                ? new Vector3(planeDelta.x, 0f, planeDelta.y)
                : new Vector3(planeDelta.x, planeDelta.y, 0f);
        }

        private Vector3 ResolveNavigationPlaneNormal()
            => UsesXzPlane ? Vector3.up : Vector3.forward;

        private void ApplyFixedPlaneAxis()
        {
            if (UsesXzPlane)
                _targetPosition.y = _fixedPlaneAxisValue;
            else
                _targetPosition.z = _fixedPlaneAxisValue;
        }
    }
}
