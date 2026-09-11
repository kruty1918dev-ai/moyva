using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.InputRouting.API;
using Kruty1918.Moyva.Shared.Controls;
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
        private readonly IPlayerControlSettingsService _controlSettings;
        private readonly IInputDeviceContext _devices;
        private PlayerControlProfile _profile;
        private bool _panHeld;
        private bool _orbitHeld;
        private readonly Dictionary<int, bool> _touchCaptures = new();
        private readonly List<int> _releasedTouches = new();
        private readonly Dictionary<PlayerControlAction, PlayerControlBinding> _bindings = new();

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
            [InjectOptional] IGameplayInputPolicy inputPolicy = null,
            [InjectOptional] IPlayerControlSettingsService controlSettings = null,
            [InjectOptional] IInputDeviceContext devices = null)
        {
            _cameraMovement = cameraMovement;
            _cameraZoom = cameraZoom;
            _settings = settings;
            _inputPolicy = inputPolicy;
            _controlSettings = controlSettings;
            _devices = devices;
            if (_devices != null) _devices.Changed += OnProfileChanged;
            ApplyControlSettings(_controlSettings?.Settings ?? PlayerControlSettingsData.CreateDefault());
            if (_controlSettings != null)
                _controlSettings.OnSettingsChanged += ApplyControlSettings;

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
            if (!Application.isFocused)
            { ReleaseTouchCaptures(); ReleasePointerCapture(); _cameraMovement.SetCameraOrbitInput(0f); return; }
            if (TryHandleTouchGestures())
            { ReleasePointerCapture(); _cameraMovement.SetCameraOrbitInput(0f); return; }

            Mouse mouse = Mouse.current;
            Vector2 pointerPosition = _devices?.ActiveProfile == ControlProfile.Gamepad
                ? ResolveScreenCenter() : mouse?.position.ReadValue() ?? ResolveScreenCenter();
            bool canNavigate = CanProcess(GameplayInputKind.KeyboardNavigation, pointerPosition);
            bool pan = ReadGesture(_profile?.PanBinding, false) && canNavigate;
            bool orbit = ReadGesture(_profile?.OrbitBinding, true) && canNavigate;
            HandlePointerGestureCapture(pointerPosition, pan, orbit);
            var delta = mouse?.delta.ReadValue() ?? Vector2.zero;
            if (_pointerOrbitCaptured && CanProcess(GameplayInputKind.PointerRotate, pointerPosition))
                _cameraMovement.RotatePointerOrbit(delta.x * ResolveMouseSensitivity() * ResolveOrbitSpeed());
            else if (_pointerPanCaptured && CanProcess(GameplayInputKind.PointerPan, pointerPosition))
                _cameraMovement.MoveCamera(delta * ResolveMouseSensitivity() * ResolveMovementSpeed());
            else if (!pan && !orbit && canNavigate)
            {
                var movement = ResolveConfiguredMove();
                if (movement.sqrMagnitude > 0.001f)
                    _cameraMovement.MoveCameraKeyboard(movement * ResolveMovementSpeed(), Time.unscaledDeltaTime);
                else if (mouse != null) TryApplyEdgeScroll(pointerPosition);
            }
            float zoom = ResolveConfiguredZoom();
            // A generic OS two-finger scroll arrives through the ordinary mouse wheel path.
            if (_profile?.Profile != ControlProfile.Gamepad && _profile?.Profile != ControlProfile.TouchPhone)
            {
                float scroll = (mouse?.scroll.ReadValue().y ?? 0f) / 120f;
                string direction = scroll >= 0f ? "/scroll/up" : "/scroll/down";
                bool assigned = false;
                foreach (var binding in _bindings.Values)
                    if (binding.ControlPath.EndsWith(direction, StringComparison.Ordinal)) { assigned = true; break; }
                if (!assigned) zoom += scroll;
            }
            if (Mathf.Abs(zoom) > 0.001f && canNavigate && CanProcess(GameplayInputKind.PointerZoom, pointerPosition))
                _cameraZoom.ZoomCamera(zoom * ResolveZoomSpeed(), pointerPosition);
            float rotation = ResolveConfiguredRotation();
            _cameraMovement.SetCameraOrbitInput(canNavigate && !pan && !orbit ? Mathf.Clamp(rotation * ResolveOrbitSpeed(), -1f, 1f) : 0f);
        }

        private bool ReadGesture(string path, bool orbit)
        {
            // Preserve the normal middle-mouse gesture in every desktop profile.
            if (Mouse.current?.middleButton.isPressed == true && IsAltPressed() == orbit) return true;
            if (_profile?.Gestures == false && path?.StartsWith("<Touchpad>", StringComparison.Ordinal) == true) return false;
            return PlayerControlBinding.TryParse(path, out var binding) && binding.ReadValue(_profile?.Deadzone ?? 0.2f) > 0f;
        }

        private void TryApplyEdgeScroll(Vector2 pointerPosition)
        {
            if (!(_profile?.EdgePan ?? _settings.ResolveEdgeScrollEnabled())
                || _devices?.ActiveProfile == ControlProfile.Gamepad || _devices?.ActiveProfile == ControlProfile.TouchPhone
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
                direction * _settings.ResolveEdgeScrollSpeedMultiplier() * ResolveMovementSpeed(),
                Time.unscaledDeltaTime);
        }

        private void HandlePointerGestureCapture(Vector2 pointerPosition, bool pan, bool orbit)
        {
            if (pan == _panHeld && orbit == _orbitHeld) return;
            ReleasePointerCapture();
            _panHeld = pan; _orbitHeld = orbit;
            if (orbit)
            {
                _pointerOrbitCaptured = TryBeginCapture(GameplayInputKind.PointerRotate, pointerPosition);
                if (_pointerOrbitCaptured) _cameraMovement.BeginPointerOrbit();
            }
            else if (pan) _pointerPanCaptured = TryBeginCapture(GameplayInputKind.PointerPan, pointerPosition);
        }

        private void ReleasePointerCapture()
        {
            _inputPolicy?.EndPointerCapture(GameplayInputKind.PointerPan);
            _inputPolicy?.EndPointerCapture(GameplayInputKind.PointerRotate);
            if (_pointerOrbitCaptured) _cameraMovement.EndPointerOrbit();
            _pointerPanCaptured = false; _pointerOrbitCaptured = false;
            _panHeld = false; _orbitHeld = false;
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
            if (touchscreen == null || _profile?.Gestures == false)
                return false;

            UpdateTouchCaptures(touchscreen);
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

            ApplyTouchDrag(touchDelta, firstTouch.Position);
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
            float twist = Vector2.SignedAngle(secondPreviousPosition - firstPreviousPosition, secondTouch.Position - firstTouch.Position);
            if (!isPinching && Mathf.Abs(twist) > 0.5f && _profile?.OrbitBinding == "<Touch>/twist")
            {
                _cameraMovement.RotateCameraAroundFocusPoint(twist * ResolveOrbitSpeed() * (_profile?.Sensitivity ?? 1f));
                return;
            }

            if (!isPinching)
            {
                Vector2 previousCenter = (firstPreviousPosition + secondPreviousPosition) * 0.5f;
                Vector2 centerDelta = ClampTouchDelta(currentCenter - previousCenter);
                float dragDeadZone = _settings.ResolveTouchDragDeadZonePixels();
                if (centerDelta.sqrMagnitude > dragDeadZone * dragDeadZone)
                    ApplyTouchDrag(centerDelta, currentCenter);
                return;
            }

            string gesture = pinchDelta > 0f ? "<Touch>/pinchOut" : "<Touch>/pinchIn";
            float zoomDirection = GestureAction(gesture, PlayerControlAction.ZoomIn) - GestureAction(gesture, PlayerControlAction.ZoomOut);
            if (_profile?.Profile != ControlProfile.TouchPhone) zoomDirection = Mathf.Sign(pinchDelta);
            if (Mathf.Approximately(zoomDirection, 0f)) { ApplyTouchAction(gesture, Mathf.Abs(pinchDelta) * 0.02f, currentCenter); return; }
            float scaleFactor = Mathf.Pow(previousDistance / currentDistance, zoomDirection * Mathf.Sign(pinchDelta));
            if (!Mathf.Approximately(ResolveZoomSpeed(), 1f))
                scaleFactor = Mathf.Pow(scaleFactor, ResolveZoomSpeed());
            bool immediate = _settings.ResolveUseImmediateTouchGestures();
            if (_settings.ResolveKeepPinchFocusUnderFingers())
                _cameraZoom.ZoomCameraByScale(scaleFactor, immediate, currentCenter);
            else
                _cameraZoom.ZoomCameraByScale(scaleFactor, immediate);
        }

        private float GestureAction(string path, PlayerControlAction action)
            => _bindings.TryGetValue(action, out var binding) && binding.ControlPath == path ? 1f : 0f;

        private void ApplyTouchDrag(Vector2 delta, Vector2 position)
        {
            if (_profile?.Profile != ControlProfile.TouchPhone || _profile.PanBinding == "<Touch>/drag")
                _cameraMovement.MoveCameraImmediate(delta, Mathf.Max(0.01f, _settings.ResolveTouchMoveSpeed()) * ResolveMovementSpeed() * (_profile?.Sensitivity ?? 1f));
            else ApplyTouchAction("<Touch>/drag", delta.magnitude * 0.02f, position);
        }

        private void ApplyTouchAction(string gesture, float amount, Vector2 position)
        {
            Vector2 move = new Vector2(GestureAction(gesture, PlayerControlAction.MoveLeft) - GestureAction(gesture, PlayerControlAction.MoveRight),
                GestureAction(gesture, PlayerControlAction.MoveBackward) - GestureAction(gesture, PlayerControlAction.MoveForward));
            if (move.sqrMagnitude > 0f) _cameraMovement.MoveCameraKeyboard(move * amount * ResolveMovementSpeed(), Time.unscaledDeltaTime);
            float rotation = GestureAction(gesture, PlayerControlAction.RotateRight) - GestureAction(gesture, PlayerControlAction.RotateLeft);
            if (rotation != 0f) _cameraMovement.RotateCameraAroundFocusPoint(rotation * amount * ResolveOrbitSpeed());
            float zoom = GestureAction(gesture, PlayerControlAction.ZoomIn) - GestureAction(gesture, PlayerControlAction.ZoomOut);
            if (zoom != 0f) _cameraZoom.ZoomCamera(zoom * amount * ResolveZoomSpeed(), position);
        }

        private void UpdateTouchCaptures(Touchscreen touchscreen)
        {
            const GameplayInputKind kinds = GameplayInputKind.PointerPan | GameplayInputKind.PointerZoom | GameplayInputKind.PointerRotate;
            _releasedTouches.Clear();
            foreach (var pair in _touchCaptures)
            {
                bool held = false;
                foreach (var touch in touchscreen.touches)
                    if (touch.press.isPressed && touch.touchId.ReadValue() == pair.Key) { held = true; break; }
                if (!held) _releasedTouches.Add(pair.Key);
            }
            foreach (int id in _releasedTouches) { _inputPolicy?.EndPointerCapture(kinds, id); _touchCaptures.Remove(id); }
            foreach (var touch in touchscreen.touches)
            {
                int id = touch.touchId.ReadValue();
                if (touch.press.isPressed && !_touchCaptures.ContainsKey(id))
                    _touchCaptures[id] = _inputPolicy?.TryBeginPointerCapture(kinds, touch.position.ReadValue(), id) ?? true;
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

        private bool IsTouchOverInteractiveUi(TouchGestureSample touch)
        {
            return !_touchCaptures.TryGetValue(touch.TouchId, out bool allowed) || !allowed || !(_inputPolicy?.CanProcess(GameplayInputKind.PointerPan | GameplayInputKind.PointerZoom | GameplayInputKind.PointerRotate,
                touch.Position, touch.TouchId) ?? true);
        }

        private Vector2 ResolveConfiguredMove()
        {
            var value = Vector2.zero;
            // CameraMovement consumes a pan vector and negates it into camera-world motion.
            value.x += ReadBinding(PlayerControlAction.MoveLeft);
            value.x -= ReadBinding(PlayerControlAction.MoveRight);
            value.y += ReadBinding(PlayerControlAction.MoveBackward);
            value.y -= ReadBinding(PlayerControlAction.MoveForward);
            return value.sqrMagnitude > 1f ? value.normalized : value;
        }

        private float ResolveConfiguredRotation()
        {
            float value = 0f;
            value -= ReadBinding(PlayerControlAction.RotateLeft);
            value += ReadBinding(PlayerControlAction.RotateRight);
            return value;
        }

        private float ResolveConfiguredZoom()
        {
            float value = 0f;
            value -= ReadBinding(PlayerControlAction.ZoomOut);
            value += ReadBinding(PlayerControlAction.ZoomIn);
            return value;
        }

        private static float ReadNonKeyboardAxis(InputAction action)
            => action == null || action.activeControl?.device is Keyboard ? 0f : action.ReadValue<float>();

        private float ReadBinding(PlayerControlAction action)
            => _bindings.TryGetValue(action, out var binding) ? binding.ReadValue(_profile?.Deadzone ?? 0.2f) : 0f;

        private void OnProfileChanged(ControlProfile profile)
        {
            ReleasePointerCapture();
            ApplyControlSettings(_controlSettings?.Settings ?? PlayerControlSettingsData.CreateDefault());
        }

        private void ApplyControlSettings(PlayerControlSettingsData data)
        {
            ReleaseTouchCaptures();
            _profile = data.Profile(_devices?.ActiveProfile ?? ControlProfile.KeyboardMouse);
            var bindings = _profile?.ToBindings() ?? data.Bindings;
            _bindings.Clear();
            if (bindings == null)
                return;
            foreach (var pair in bindings)
            {
                if (PlayerControlBinding.TryParse(pair.Value, out var binding))
                    _bindings[pair.Key] = binding;
            }
        }

        private float ResolveMouseSensitivity()
            => Mathf.Clamp((_controlSettings?.Settings.MouseSensitivity ?? 1f) * (_profile?.Sensitivity ?? 1f), 0.25f, 3f);

        private float ResolveMovementSpeed()
            => Mathf.Clamp((_controlSettings?.Settings.MovementSpeed ?? 1f) * (_profile?.Profile == ControlProfile.Gamepad ? _profile.Sensitivity : 1f), 0.25f, 3f);

        private float ResolveOrbitSpeed()
            => Mathf.Clamp((_controlSettings?.Settings.OrbitSpeed ?? 1f) * (_profile?.Profile == ControlProfile.Gamepad ? _profile.Sensitivity : 1f), 0.25f, 3f);

        private float ResolveZoomSpeed()
            => Mathf.Clamp((_controlSettings?.Settings.ZoomSpeed ?? 1f) * (_profile?.Sensitivity ?? 1f), 0.25f, 3f);

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

        private void ReleaseTouchCaptures()
        {
            foreach (int id in _touchCaptures.Keys)
                _inputPolicy?.EndPointerCapture(GameplayInputKind.PointerPan | GameplayInputKind.PointerZoom | GameplayInputKind.PointerRotate, id);
            _touchCaptures.Clear();
        }

        public void Dispose()
        {
            if (_devices != null) _devices.Changed -= OnProfileChanged;
            ReleaseTouchCaptures();
            if (_controlSettings != null)
                _controlSettings.OnSettingsChanged -= ApplyControlSettings;
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
