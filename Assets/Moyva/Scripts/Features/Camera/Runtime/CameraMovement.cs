using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Camera.Runtime
{
    internal sealed class CameraMovement : ICameraMovement, IInitializable, ILateTickable
    {
        private const string StartupChainTag = "[MoyvaStartupChain]";
        private const float RotationEpsilon = 0.0001f;

        private readonly UnityEngine.Camera _camera;
        private readonly CameraSettingsSO _settings;
        private readonly ICameraBoundsProvider _boundsProvider;
        private readonly IGridProjection _gridProjection;

        private Vector3 _targetPosition;
        private Vector3 _currentVelocity; // Необхідно для Vector3.SmoothDamp
        private float _fixedPlaneAxisValue;
        private readonly Vector3[] _viewportWorldCorners = new Vector3[4];
        private float _keyboardOrbitInput;
        private float _orbitAngularVelocity;
        private bool _pointerOrbitActive;
        private bool _hasOrbitPivot;
        private Vector3 _orbitPivot;

        private float _forceBlockTimer;
        private bool _pendingTeleportLateTickLog;
        private string _lastTeleportSource;
        private const float ForceBlockDuration = 1.5f; // Час затримки після форсованого руху (можна винести в SO)

        // Zenject автоматично підставить активну камеру та налаштування
        public CameraMovement(
            UnityEngine.Camera camera,
            CameraSettingsSO settings,
            [InjectOptional] ICameraBoundsProvider boundsProvider = null,
            [InjectOptional] IGridProjection gridProjection = null)
        {
            _camera = camera;
            _settings = settings;
            _boundsProvider = boundsProvider;
            _gridProjection = gridProjection;
        }

        public void Initialize()
        {
            // На старті синхронізуємо цільову позицію з поточною, щоб камера не відлітала
            _targetPosition = _camera.transform.position;
            _fixedPlaneAxisValue = UsesXzPlane ? _targetPosition.y : _settings.defaultCameraZ;
            ClampTargetToBounds();
        }

        public void MoveCamera(Vector3 delta) // delta — це чисті пікселі з Action Map
            => ApplyScreenDelta(delta, _settings.ResolveMoveSpeed(), immediate: false);

        public void MoveCameraKeyboard(Vector2 direction, float unscaledDeltaTime)
        {
            if (_forceBlockTimer > 0f || direction.sqrMagnitude <= RotationEpsilon)
                return;

            Vector2 normalized = direction.sqrMagnitude > 1f ? direction.normalized : direction;
            Transform cameraTransform = _camera.transform;
            Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, ResolveNavigationPlaneNormal()).normalized;
            Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, ResolveNavigationPlaneNormal()).normalized;

            if (right.sqrMagnitude <= RotationEpsilon || forward.sqrMagnitude <= RotationEpsilon)
                return;

            Vector3 worldDelta = (-right * normalized.x - forward * normalized.y)
                * _settings.ResolveMoveSpeed()
                * Mathf.Max(0f, unscaledDeltaTime);
            ShiftCameraWorld(worldDelta, immediate: false);
        }

        public void MoveCameraImmediate(Vector3 delta, float speedMultiplier)
            => ApplyScreenDelta(delta, speedMultiplier, immediate: true);

        public void RotateCameraAroundFocusPoint(float angleDegrees)
        {
            if (_forceBlockTimer > 0f
                || _camera == null
                || float.IsNaN(angleDegrees)
                || float.IsInfinity(angleDegrees)
                || Mathf.Abs(angleDegrees) <= RotationEpsilon)
            {
                return;
            }

            if (!EnsureOrbitPivot())
                return;

            ApplyOrbitAngle(angleDegrees);
        }

        public void SetCameraOrbitInput(float normalizedInput)
        {
            _keyboardOrbitInput = Mathf.Clamp(normalizedInput, -1f, 1f);
            if (Mathf.Abs(_keyboardOrbitInput) > RotationEpsilon)
                EnsureOrbitPivot();
        }

        public void BeginPointerOrbit()
        {
            _pointerOrbitActive = EnsureOrbitPivot();
        }

        public void RotatePointerOrbit(float horizontalScreenDelta)
        {
            if (!_pointerOrbitActive || Mathf.Abs(horizontalScreenDelta) <= RotationEpsilon)
                return;

            float angle = horizontalScreenDelta
                * _settings.ResolvePointerOrbitSensitivity()
                * ResolveOrbitZoomMultiplier();
            ApplyOrbitAngle(angle);
        }

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
            cameraTransform.SetPositionAndRotation(rotatedPosition, rotatedRotation);

            _targetPosition = _orbitPivot + rotationDelta * (_targetPosition - _orbitPivot);
            _currentVelocity = rotationDelta * _currentVelocity;
            ApplyFixedPlaneAxis();
            ClampTargetToBounds();
        }

        public void ShiftCameraWorld(Vector3 worldDelta, bool immediate)
        {
            if (_forceBlockTimer > 0f) return;

            if (UsesXzPlane)
                worldDelta.y = 0f;
            else
                worldDelta.z = 0f;

            ApplyNavigationDelta(worldDelta);

            if (!immediate)
                return;

            _currentVelocity = Vector3.zero;
            _camera.transform.position = _targetPosition;
        }

        public void MoveCameraFocusToWorldPoint(Vector3 focusPoint, bool immediate)
        {
            if (_forceBlockTimer > 0f || _camera == null)
                return;

            if (!TryResolveNavigationPlaneCenter(_targetPosition, out _, out float distanceToPlane))
                distanceToPlane = ResolveDefaultDistanceToNavigationPlane();

            _targetPosition = focusPoint - _camera.transform.forward * Mathf.Max(0.1f, distanceToPlane);
            _fixedPlaneAxisValue = UsesXzPlane ? _targetPosition.y : _targetPosition.z;
            ClampTargetToBounds();

            if (!immediate)
                return;

            _currentVelocity = Vector3.zero;
            _camera.transform.position = _targetPosition;
        }

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
            _camera.transform.position = _targetPosition;
        }

        private void ApplyScreenDelta(Vector3 delta, float speedMultiplier, bool immediate)
        {
            if (_forceBlockTimer > 0f) return;

            Vector3 moveDirection = TryResolveDragWorldDelta(new Vector2(delta.x, delta.y), out var dragWorldDelta)
                ? dragWorldDelta
                : ResolveFallbackScreenDelta(delta);

            ApplyNavigationDelta(moveDirection * speedMultiplier);

            if (!immediate)
                return;

            _currentVelocity = Vector3.zero;
            _camera.transform.position = _targetPosition;
        }

        public void ForceMoveCameraToPosition(Vector3 position)
        {
            // Встановлюємо нову ціль і блокуємо звичайний рух на заданий час
            _targetPosition = position;
            ApplyFixedPlaneAxis();
            ClampTargetToBounds();
            _forceBlockTimer = ForceBlockDuration;
        }

        public void TeleportCamera(Vector3 position)
        {
            Vector3 before = _camera.transform.position;
            _targetPosition = position;
            ApplyFixedPlaneAxis();
            ClampTargetToBounds();
            _currentVelocity = Vector3.zero;
            _camera.transform.position = _targetPosition;
            _pendingTeleportLateTickLog = true;
            _lastTeleportSource = "TeleportCamera";
        }

        public void TeleportCameraToFocusPoint(Vector3 focusPoint, float distance)
        {
            Vector3 before = _camera.transform.position;
            float resolvedDistance = Mathf.Max(0.1f, distance);
            _targetPosition = focusPoint - _camera.transform.forward * resolvedDistance;
            _fixedPlaneAxisValue = UsesXzPlane ? _targetPosition.y : _targetPosition.z;
            ClampTargetToBounds();
            _currentVelocity = Vector3.zero;
            _camera.transform.position = _targetPosition;
            _pendingTeleportLateTickLog = true;
            _lastTeleportSource = "TeleportCameraToFocusPoint";
        }

        public void LateTick()
        {
            // Зменшуємо таймер блокування
            if (_forceBlockTimer > 0f)
            {
                _forceBlockTimer -= Time.unscaledDeltaTime;
            }

            UpdateKeyboardOrbit(Time.unscaledDeltaTime);

            // Плавно рухаємо камеру до _targetPosition
            _camera.transform.position = Vector3.SmoothDamp(
                _camera.transform.position,
                _targetPosition,
                ref _currentVelocity,
                _settings.ResolveSmoothTime()
            );

            if (_pendingTeleportLateTickLog)
            {
                _pendingTeleportLateTickLog = false;
            }
        }

        private static string FormatVector(Vector3 value)
        {
            return $"({value.x:0.###}, {value.y:0.###}, {value.z:0.###})";
        }

        private void ClampTargetToBounds()
        {
            if (_boundsProvider == null || _camera == null)
                return;

            if (!TryResolveNavigationPlaneCenter(_targetPosition, out var targetPlaneCenter, out float distanceToPlane))
            {
                ApplyFixedPlaneAxis();
                return;
            }

            if (!TryResolveTargetPlaneBounds(
                    _targetPosition,
                    out float minX,
                    out float maxX,
                    out float minY,
                    out float maxY))
            {
                ApplyFixedPlaneAxis();
                return;
            }

            Vector2 targetPlane = ToNavigationPlaneCoordinates(targetPlaneCenter);
            targetPlane.x = Mathf.Clamp(targetPlane.x, minX, maxX);
            if (UsesXzPlane)
            {
                targetPlane.y = Mathf.Clamp(targetPlane.y, minY, maxY);
                var clampedFocus = new Vector3(targetPlane.x, 0f, targetPlane.y);
                _targetPosition = clampedFocus - _camera.transform.forward * distanceToPlane;
            }
            else
            {
                targetPlane.y = Mathf.Clamp(targetPlane.y, minY, maxY);
                var clampedFocus = new Vector3(targetPlane.x, targetPlane.y, 0f);
                _targetPosition = clampedFocus - _camera.transform.forward * distanceToPlane;
            }

            ApplyFixedPlaneAxis();
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
            ClampTargetToBounds();
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

            return true;
        }

        private static float ClampDeltaAxis(
            float current,
            float delta,
            float min,
            float max)
        {
            const float BoundaryEpsilon = 0.001f;
            if (delta < 0f && current <= min + BoundaryEpsilon)
                return 0f;
            if (delta > 0f && current >= max - BoundaryEpsilon)
                return 0f;

            return Mathf.Clamp(current + delta, min, max) - current;
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
            if (_forceBlockTimer > 0f)
                return;

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

        private float ResolveOrbitZoomMultiplier()
        {
            float zoom = _camera.orthographic ? _camera.orthographicSize : _camera.fieldOfView;
            float normalized = Mathf.InverseLerp(_settings.ResolveMinZoom(), _settings.ResolveMaxZoom(), zoom);
            return Mathf.Lerp(
                _settings.ResolveCloseZoomRotationMultiplier(),
                _settings.ResolveFarZoomRotationMultiplier(),
                normalized);
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
