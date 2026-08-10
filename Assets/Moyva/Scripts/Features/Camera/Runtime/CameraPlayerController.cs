using System;
using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.InputRouting.API;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using Zenject;

namespace Kruty1918.Moyva.Camera.Runtime
{
    internal sealed class CameraPlayerController : ITickable, IDisposable
    {
        private readonly ICameraMovement _cameraMovement;
        private readonly ICameraZoom _cameraZoom;
        private readonly CameraSettingsSO _settings;
        private readonly IGameplayInputPolicy _inputPolicy;

        private readonly InputAction _moveAction;
        private readonly InputAction _zoomAction;
        private readonly InputAction _rotateAction;
        private bool _pointerPanCaptured;
        private bool _pointerOrbitCaptured;

        public CameraPlayerController(
            ICameraMovement cameraMovement,
            ICameraZoom cameraZoom,
            CameraSettingsSO settings,
            InputActionAsset inputAsset,
            [InjectOptional] IGameplayInputPolicy inputPolicy = null)
        {
            _cameraMovement = cameraMovement;
            _cameraZoom = cameraZoom;
            _settings = settings;
            _inputPolicy = inputPolicy;

            if (inputAsset == null)
                return;

            var map = inputAsset.FindActionMap("Player");
            if (map == null)
                return;

            _moveAction = map.FindAction("Move");
            _zoomAction = map.FindAction("Zoom");
            _rotateAction = map.FindAction("Rotate");

            map.Enable();
        }

        public void Tick()
        {
            if (TryHandleTouchGestures())
                return;

            if (_moveAction == null || _zoomAction == null)
                return;

            Mouse mouse = Mouse.current;
            Vector2 pointerPosition = mouse?.position.ReadValue() ?? ResolveScreenCenter();
            bool altPressed = IsAltPressed();
            bool middlePressed = mouse?.middleButton.isPressed ?? false;

            HandlePointerGestureCapture(mouse, pointerPosition, altPressed);

            Vector2 moveDelta = _moveAction.ReadValue<Vector2>();
            if (middlePressed)
            {
                if (altPressed)
                {
                    if (_pointerOrbitCaptured)
                        _cameraMovement.RotatePointerOrbit(mouse.delta.ReadValue().x);
                }
                else if (_pointerPanCaptured && moveDelta.sqrMagnitude > 0.001f)
                {
                    _cameraMovement.MoveCamera(moveDelta);
                }
            }
            else if (moveDelta.sqrMagnitude > 0.001f
                     && CanProcess(GameplayInputKind.KeyboardNavigation, pointerPosition))
            {
                _cameraMovement.MoveCameraKeyboard(moveDelta, Time.unscaledDeltaTime);
            }
            else
            {
                TryApplyEdgeScroll(pointerPosition);
            }

            // Зум (Scroll або Pinch)
            float zoomDelta = _zoomAction.ReadValue<float>();
            if (Mathf.Abs(zoomDelta) > 0.001f
                && CanProcess(GameplayInputKind.PointerZoom, pointerPosition))
            {
                _cameraZoom.ZoomCamera(zoomDelta, pointerPosition);
            }

            float rotationDirection = _rotateAction?.ReadValue<float>() ?? 0f;
            bool canRotateKeyboard = CanProcess(GameplayInputKind.KeyboardNavigation, pointerPosition);
            _cameraMovement.SetCameraOrbitInput(canRotateKeyboard ? rotationDirection : 0f);
        }

        private void TryApplyEdgeScroll(Vector2 pointerPosition)
        {
            if (!_settings.ResolveEdgeScrollEnabled()
                || !Application.isFocused
                || !CanProcess(GameplayInputKind.PointerPan, pointerPosition))
            {
                return;
            }

            Vector2 direction = CameraEdgeScrollMath.ResolveDirection(
                pointerPosition,
                new Vector2(Screen.width, Screen.height),
                _settings.ResolveEdgeScrollMarginPixels());
            if (direction.sqrMagnitude <= 0.001f)
                return;

            _cameraMovement.MoveCameraKeyboard(
                direction * _settings.ResolveEdgeScrollSpeedMultiplier(),
                Time.unscaledDeltaTime);
        }

        private void HandlePointerGestureCapture(Mouse mouse, Vector2 pointerPosition, bool altPressed)
        {
            if (mouse == null)
                return;

            if (mouse.middleButton.wasPressedThisFrame)
            {
                if (altPressed)
                {
                    _pointerOrbitCaptured = TryBeginCapture(GameplayInputKind.PointerRotate, pointerPosition);
                    if (_pointerOrbitCaptured)
                        _cameraMovement.BeginPointerOrbit();
                }
                else
                {
                    _pointerPanCaptured = TryBeginCapture(GameplayInputKind.PointerPan, pointerPosition);
                }
            }

            if (!mouse.middleButton.wasReleasedThisFrame)
                return;

            if (_pointerPanCaptured)
                _inputPolicy?.EndPointerCapture(GameplayInputKind.PointerPan);

            if (_pointerOrbitCaptured)
            {
                _cameraMovement.EndPointerOrbit();
                _inputPolicy?.EndPointerCapture(GameplayInputKind.PointerRotate);
            }

            _pointerPanCaptured = false;
            _pointerOrbitCaptured = false;
        }

        private bool TryBeginCapture(GameplayInputKind inputKind, Vector2 pointerPosition)
            => _inputPolicy?.TryBeginPointerCapture(inputKind, pointerPosition) ?? true;

        private bool CanProcess(GameplayInputKind inputKind, Vector2 pointerPosition)
            => _inputPolicy?.CanProcess(inputKind, pointerPosition) ?? true;

        private static bool IsAltPressed()
        {
            Keyboard keyboard = Keyboard.current;
            return keyboard != null && (keyboard.leftAltKey.isPressed || keyboard.rightAltKey.isPressed);
        }

        private static Vector2 ResolveScreenCenter()
            => new(Screen.width * 0.5f, Screen.height * 0.5f);

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

            float currentDistance = Vector2.Distance(firstTouch.Position, secondTouch.Position);
            float previousDistance = Vector2.Distance(firstPreviousPosition, secondPreviousPosition);
            if (previousDistance <= 0.01f || currentDistance <= 0.01f)
                return;

            float pinchDelta = currentDistance - previousDistance;
            float pinchDeadZone = _settings.ResolveTouchPinchDeadZonePixels();
            bool isPinching = Mathf.Abs(pinchDelta) > pinchDeadZone;

            if (!isPinching)
            {
                Vector2 previousCenter = (firstPreviousPosition + secondPreviousPosition) * 0.5f;
                Vector2 centerDelta = ClampTouchDelta(currentCenter - previousCenter);
                float dragDeadZone = _settings.ResolveTouchDragDeadZonePixels();
                if (centerDelta.sqrMagnitude > dragDeadZone * dragDeadZone)
                    _cameraMovement.MoveCameraImmediate(centerDelta, Mathf.Max(0.01f, _settings.ResolveTouchMoveSpeed()));
                return;
            }

            float scaleFactor = previousDistance / currentDistance;
            bool immediate = _settings.ResolveUseImmediateTouchGestures();
            if (_settings.ResolveKeepPinchFocusUnderFingers())
                _cameraZoom.ZoomCameraByScale(scaleFactor, immediate, currentCenter);
            else
                _cameraZoom.ZoomCameraByScale(scaleFactor, immediate);
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

        private bool IsTouchOverInteractiveUi(TouchGestureSample touch)
        {
            return _inputPolicy?.IsPointerOverUi(touch.Position, touch.TouchId, interactiveOnly: true) ?? false;
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
            _inputPolicy?.EndPointerCapture(GameplayInputKind.PointerPan);
            _inputPolicy?.EndPointerCapture(GameplayInputKind.PointerRotate);
            _cameraMovement.SetCameraOrbitInput(0f);
            _cameraMovement.EndPointerOrbit();
            _moveAction?.Disable();
            _zoomAction?.Disable();
            _rotateAction?.Disable();
        }
    }
}
