using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.Camera.Runtime
{
    internal sealed class CameraPlayerController : ITickable, IDisposable
    {
        private readonly ICameraMovement _cameraMovement;
        private readonly ICameraZoom _cameraZoom;
        private readonly UnityEngine.Camera _camera;
        private readonly CameraSettingsSO _settings;
        private readonly IGridProjection _gridProjection;

        private readonly InputAction _moveAction;
        private readonly InputAction _zoomAction;
        private readonly List<RaycastResult> _uiRaycastResults = new List<RaycastResult>(8);
        private PointerEventData _pointerEventData;

        public CameraPlayerController(
            ICameraMovement cameraMovement,
            ICameraZoom cameraZoom,
            UnityEngine.Camera camera,
            CameraSettingsSO settings,
            InputActionAsset inputAsset,
            [InjectOptional] IGridProjection gridProjection = null)
        {
            _cameraMovement = cameraMovement;
            _cameraZoom = cameraZoom;
            _camera = camera;
            _settings = settings;
            _gridProjection = gridProjection;

            var map = inputAsset.FindActionMap("Player");
            _moveAction = map.FindAction("Move");
            _zoomAction = map.FindAction("Zoom");

            map.Enable();
        }

        public void Tick()
        {
            if (TryHandleTouchGestures())
                return;

            // Отримуємо сирі пікселі дельти (Mouse Delta або Touch Delta)
            Vector2 moveDelta = _moveAction.ReadValue<Vector2>();

            if (moveDelta.sqrMagnitude > 0.001f)
            {
                // Просто передаємо Vector2. Сервіс сам знає, як його перетворити для 2D.
                _cameraMovement.MoveCamera(moveDelta);
            }

            // Зум (Scroll або Pinch)
            float zoomDelta = _zoomAction.ReadValue<float>();
            if (Mathf.Abs(zoomDelta) > 0.001f)
            {
                _cameraZoom.ZoomCamera(zoomDelta);
            }
        }

        private bool TryHandleTouchGestures()
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen == null)
                return false;

            int activeTouchCount = TryReadActiveTouches(touchscreen, out var firstTouch, out var secondTouch);
            if (activeTouchCount <= 0)
                return false;

            if (IsTouchOverInteractiveUi(firstTouch) || (activeTouchCount > 1 && IsTouchOverInteractiveUi(secondTouch)))
                return true;

            if (activeTouchCount > 1)
            {
                HandleTwoFingerGesture(firstTouch, secondTouch);
                return true;
            }

            Vector2 touchDelta = ClampTouchDelta(firstTouch.Delta);
            float dragDeadZone = _settings.ResolveTouchDragDeadZonePixels();
            if (touchDelta.sqrMagnitude <= dragDeadZone * dragDeadZone)
                return true;

            _cameraMovement.MoveCameraImmediate(touchDelta, Mathf.Max(0.01f, _settings.ResolveTouchMoveSpeed()));
            return true;
        }

        private void HandleTwoFingerGesture(TouchGestureSample firstTouch, TouchGestureSample secondTouch)
        {
            Vector2 firstPreviousPosition = firstTouch.Position - firstTouch.Delta;
            Vector2 secondPreviousPosition = secondTouch.Position - secondTouch.Delta;

            Vector2 currentCenter = (firstTouch.Position + secondTouch.Position) * 0.5f;
            Vector2 previousCenter = (firstPreviousPosition + secondPreviousPosition) * 0.5f;
            Vector2 centerDelta = ClampTouchDelta(currentCenter - previousCenter);

            float dragDeadZone = _settings.ResolveTouchDragDeadZonePixels();
            if (centerDelta.sqrMagnitude > dragDeadZone * dragDeadZone)
                _cameraMovement.MoveCameraImmediate(centerDelta, Mathf.Max(0.01f, _settings.ResolveTouchMoveSpeed()));

            float currentDistance = Vector2.Distance(firstTouch.Position, secondTouch.Position);
            float previousDistance = Vector2.Distance(firstPreviousPosition, secondPreviousPosition);
            if (previousDistance <= 0.01f || currentDistance <= 0.01f)
                return;

            float pinchDelta = currentDistance - previousDistance;
            if (Mathf.Abs(pinchDelta) <= _settings.ResolveTouchPinchDeadZonePixels())
                return;

            Vector3 worldBeforeZoom = ScreenToWorld(currentCenter);
            float scaleFactor = previousDistance / currentDistance;
            bool immediate = _settings.ResolveUseImmediateTouchGestures();
            _cameraZoom.ZoomCameraByScale(scaleFactor, immediate);

            if (immediate && _settings.ResolveKeepPinchFocusUnderFingers())
            {
                Vector3 worldAfterZoom = ScreenToWorld(currentCenter);
                Vector3 correction = worldBeforeZoom - worldAfterZoom;
                if (_gridProjection != null && _gridProjection.WorldPlane == GridWorldPlane.XZ)
                    correction.y = 0f;
                else
                    correction.z = 0f;
                _cameraMovement.ShiftCameraWorld(correction, immediate: true);
            }
        }

        private int TryReadActiveTouches(Touchscreen touchscreen, out TouchGestureSample firstTouch, out TouchGestureSample secondTouch)
        {
            firstTouch = default;
            secondTouch = default;
            int activeTouchCount = 0;

            var touches = touchscreen.touches;
            for (int touchIndex = 0; touchIndex < touches.Count; touchIndex++)
            {
                TouchControl touch = touches[touchIndex];
                if (!touch.press.isPressed)
                    continue;

                var sample = new TouchGestureSample(
                    touch.position.ReadValue(),
                    touch.delta.ReadValue(),
                    touch.touchId.ReadValue());

                if (activeTouchCount == 0)
                    firstTouch = sample;
                else if (activeTouchCount == 1)
                    secondTouch = sample;

                activeTouchCount++;
            }

            return activeTouchCount;
        }

        private Vector2 ClampTouchDelta(Vector2 delta)
        {
            float maxDelta = Mathf.Max(1f, _settings.ResolveMaxTouchDeltaPixels());
            return delta.sqrMagnitude > maxDelta * maxDelta
                ? delta.normalized * maxDelta
                : delta;
        }

        private Vector3 ScreenToWorld(Vector2 screenPosition)
        {
            if (_camera == null)
                return Vector3.zero;

            if (_gridProjection == null)
                return _camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -_camera.transform.position.z));

            Ray ray = _camera.ScreenPointToRay(screenPosition);
            Plane plane = _gridProjection.WorldPlane == GridWorldPlane.XZ
                ? new Plane(Vector3.up, Vector3.zero)
                : new Plane(Vector3.forward, Vector3.zero);

            return plane.Raycast(ray, out float distance)
                ? ray.GetPoint(distance)
                : _camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -_camera.transform.position.z));
        }

        private bool IsTouchOverInteractiveUi(TouchGestureSample touch)
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
                return false;

            _pointerEventData ??= new PointerEventData(eventSystem);
            _pointerEventData.Reset();
            _pointerEventData.pointerId = touch.TouchId;
            _pointerEventData.position = touch.Position;

            _uiRaycastResults.Clear();
            eventSystem.RaycastAll(_pointerEventData, _uiRaycastResults);

            for (int resultIndex = 0; resultIndex < _uiRaycastResults.Count; resultIndex++)
            {
                if (_uiRaycastResults[resultIndex].gameObject.GetComponentInParent<Selectable>() != null)
                    return true;
            }

            return false;
        }

        private readonly struct TouchGestureSample
        {
            public readonly Vector2 Position;
            public readonly Vector2 Delta;
            public readonly int TouchId;

            public TouchGestureSample(Vector2 position, Vector2 delta, int touchId)
            {
                Position = position;
                Delta = delta;
                TouchId = touchId;
            }
        }

        public void Dispose()
        {
            _moveAction?.Disable();
            _zoomAction?.Disable();
        }
    }
}