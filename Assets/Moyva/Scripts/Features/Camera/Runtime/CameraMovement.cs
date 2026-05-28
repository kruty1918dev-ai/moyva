using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Camera.Runtime
{
    internal sealed class CameraMovement : ICameraMovement, IInitializable, ILateTickable
    {
        private readonly UnityEngine.Camera _camera;
        private readonly CameraSettingsSO _settings;
        private readonly ICameraBoundsProvider _boundsProvider;
        private readonly IGridProjection _gridProjection;

        private Vector3 _targetPosition;
        private Vector3 _currentVelocity; // Необхідно для Vector3.SmoothDamp
        private float _fixedPlaneAxisValue;
        private readonly Vector3[] _viewportWorldCorners = new Vector3[4];

        private float _forceBlockTimer;
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

        public void MoveCameraImmediate(Vector3 delta, float speedMultiplier)
            => ApplyScreenDelta(delta, speedMultiplier, immediate: true);

        public void ShiftCameraWorld(Vector3 worldDelta, bool immediate)
        {
            if (_forceBlockTimer > 0f) return;

            if (UsesXzPlane)
                worldDelta.y = 0f;
            else
                worldDelta.z = 0f;

            _targetPosition += worldDelta;
            ApplyFixedPlaneAxis();
            ClampTargetToBounds();

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

            _targetPosition += moveDirection * speedMultiplier;

            ApplyFixedPlaneAxis();
            ClampTargetToBounds();

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
            _targetPosition = position;
            ApplyFixedPlaneAxis();
            ClampTargetToBounds();
            _currentVelocity = Vector3.zero;
            _camera.transform.position = _targetPosition;
        }

        public void TeleportCameraToFocusPoint(Vector3 focusPoint, float distance)
        {
            float resolvedDistance = Mathf.Max(0.1f, distance);
            _targetPosition = focusPoint - _camera.transform.forward * resolvedDistance;
            _fixedPlaneAxisValue = UsesXzPlane ? _targetPosition.y : _targetPosition.z;
            ClampTargetToBounds();
            _currentVelocity = Vector3.zero;
            _camera.transform.position = _targetPosition;
        }

        public void LateTick()
        {
            // Зменшуємо таймер блокування
            if (_forceBlockTimer > 0f)
            {
                _forceBlockTimer -= Time.deltaTime;
            }

            // Тримаємо ціль усередині bounds на кожному кадрі, бо зум міг змінити
            // допустиму "напіввисоту" viewport.
            ClampTargetToBounds();

            // Плавно рухаємо камеру до _targetPosition
            _camera.transform.position = Vector3.SmoothDamp(
                _camera.transform.position,
                _targetPosition,
                ref _currentVelocity,
                _settings.ResolveSmoothTime()
            );
        }

        private void ClampTargetToBounds()
        {
            if (_boundsProvider == null || _camera == null)
                return;

            var bounds = _boundsProvider.GetWorldBounds();
            if (!bounds.HasValue)
                return;

            Vector2 overflow = _settings.ResolveBoundsOverflowWorldUnits();
            float minBoundX = bounds.MinX - overflow.x;
            float maxBoundX = bounds.MaxX + overflow.x;
            float minBoundY = bounds.MinY - overflow.y;
            float maxBoundY = bounds.MaxY + overflow.y;

<<<<<<< HEAD
            if (!TryResolveNavigationPlaneCenter(_targetPosition, out var targetPlaneCenter, out float distanceToPlane))
            {
                ApplyFixedPlaneAxis();
                return;
            }

            ResolveViewportHalfExtents(out float halfW, out float halfH);
=======
            float halfH = _camera.orthographicSize;
            float halfW = halfH * _camera.aspect;
>>>>>>> origin/main
            float width = Mathf.Max(0.01f, maxBoundX - minBoundX);
            float height = Mathf.Max(0.01f, maxBoundY - minBoundY);

            float minX, maxX;
            if (width <= halfW * 2f)
            {
                // Якщо viewport ширший за bounds, єдина валідна позиція — центр.
                minX = maxX = (minBoundX + maxBoundX) * 0.5f;
            }
            else
            {
                minX = minBoundX + halfW;
                maxX = maxBoundX - halfW;
            }

            float minY, maxY;
            if (height <= halfH * 2f)
            {
                minY = maxY = (minBoundY + maxBoundY) * 0.5f;
            }
            else
            {
                minY = minBoundY + halfH;
                maxY = maxBoundY - halfH;
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

        private bool TryScreenPointToNavigationPlane(Vector2 screenPoint, out Vector3 worldPoint)
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

        private void ResolveViewportHalfExtents(out float halfWidth, out float halfHeight)
        {
            halfHeight = _camera.orthographic ? _camera.orthographicSize : 0f;
            halfWidth = halfHeight * _camera.aspect;

            if (!TryResolveNavigationPlaneViewportHalfExtents(out var planeHalfExtents))
                return;

            halfWidth = planeHalfExtents.x;
            halfHeight = planeHalfExtents.y;
        }

        private bool TryResolveNavigationPlaneViewportHalfExtents(out Vector2 halfExtents)
        {
            halfExtents = Vector2.zero;
            if (_camera == null || Screen.width <= 0 || Screen.height <= 0)
                return false;

            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            if (!TryScreenPointToNavigationPlane(screenCenter, out var centerWorld))
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
                if (!TryScreenPointToNavigationPlane(new Vector2(screenCorner.x, screenCorner.y), out var cornerWorld))
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

        private Vector2 ToNavigationPlaneCoordinates(Vector3 worldPosition)
        {
            return UsesXzPlane
                ? new Vector2(worldPosition.x, worldPosition.z)
                : new Vector2(worldPosition.x, worldPosition.y);
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