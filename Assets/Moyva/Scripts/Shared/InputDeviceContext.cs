using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using Zenject;

namespace Kruty1918.Moyva.Shared.Controls
{
    public enum ControlProfile { Auto, KeyboardMouse, KeyboardTouchpad, Gamepad, TouchPhone }
    public enum PointerInterpretation { Auto, Mouse, Touchpad }

    /// <summary>Presentation/activity only. Gameplay permission remains with IGameplayInputPolicy.</summary>
    public interface IInputDeviceContext
    {
        ControlProfile ActiveProfile { get; }
        ControlProfile Selection { get; }
        PointerInterpretation PointerMode { get; }
        event Action<ControlProfile> Changed;
        bool IsPresent(ControlProfile profile);
        void Configure(ControlProfile selection, PointerInterpretation pointerMode, float gamepadDeadzone = 0.3f);
    }

    public sealed class InputDeviceContext : IInputDeviceContext, ITickable
    {
        public ControlProfile ActiveProfile => Selection == ControlProfile.Auto ? _recentProfile : Selection;
        public ControlProfile Selection { get; private set; }
        public PointerInterpretation PointerMode { get; private set; }
        public event Action<ControlProfile> Changed;
        private ControlProfile _recentProfile = ControlProfile.KeyboardMouse;
        private float _lastActivity = float.NegativeInfinity;
        private float _motionDuration;
        private float _gamepadDeadzone = 0.3f;
        private ControlProfile _motionProfile;
        private readonly System.Collections.Generic.Dictionary<int, Vector2> _lastSticks = new();

        public void Configure(ControlProfile selection, PointerInterpretation pointerMode, float gamepadDeadzone = 0.3f)
        {
            var before = ActiveProfile;
            _gamepadDeadzone = float.IsNaN(gamepadDeadzone) ? 0.3f : Mathf.Clamp(gamepadDeadzone, 0.3f, 0.85f);
            Selection = Enum.IsDefined(typeof(ControlProfile), selection) ? selection : ControlProfile.Auto;
            PointerMode = Enum.IsDefined(typeof(PointerInterpretation), pointerMode) ? pointerMode : PointerInterpretation.Auto;
            if (_recentProfile == ControlProfile.KeyboardMouse || _recentProfile == ControlProfile.KeyboardTouchpad)
                _recentProfile = ResolvePointerProfile(Mouse.current);
            if (before != ActiveProfile) Changed?.Invoke(ActiveProfile);
        }

        public bool IsPresent(ControlProfile profile)
        {
            if (profile == ControlProfile.Gamepad) return Gamepad.all.Count > 0;
            if (profile == ControlProfile.TouchPhone) return Touchscreen.current != null;
            if (profile == ControlProfile.KeyboardTouchpad)
            {
                foreach (var device in InputSystem.devices)
                    if (IsExplicitTouchpad(device)) return true;
                return PointerMode == PointerInterpretation.Touchpad && Mouse.current != null;
            }
            return Keyboard.current != null || Mouse.current != null;
        }

        public static bool IsExplicitTouchpad(InputDevice device)
        {
            if (device == null) return false;
            // Metadata only: scrolling granularity cannot reliably identify a generic mouse.
            string identity = device.layout + " " + device.description.deviceClass + " " + device.description.product;
            return identity.IndexOf("touchpad", StringComparison.OrdinalIgnoreCase) >= 0
                || identity.IndexOf("trackpad", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private ControlProfile ResolvePointerProfile(InputDevice device)
            => PointerMode == PointerInterpretation.Touchpad ||
               PointerMode == PointerInterpretation.Auto && IsExplicitTouchpad(device)
                ? ControlProfile.KeyboardTouchpad : ControlProfile.KeyboardMouse;

        public void RecordActivity(ControlProfile profile, bool deliberate, float time, float deltaTime)
        {
            if (profile == ControlProfile.Auto) return;
            if (!deliberate)
            {
                if (_motionProfile != profile) { _motionProfile = profile; _motionDuration = 0f; }
                _motionDuration += Mathf.Min(deltaTime, 0.05f);
                if (_motionDuration < 0.12f || time - _lastActivity < 0.65f) return;
            }
            var before = ActiveProfile;
            _recentProfile = profile;
            _lastActivity = time;
            if (before != ActiveProfile) Changed?.Invoke(ActiveProfile);
        }

        public void Tick()
        {
            if (!Application.isFocused) { _motionDuration = 0f; return; }
            float time = Time.unscaledTime;
            var touch = Touchscreen.current;
            if (touch != null)
                foreach (var finger in touch.touches)
                    if (finger.press.wasPressedThisFrame || finger.press.isPressed && finger.delta.ReadValue().sqrMagnitude > 4f)
                    { RecordActivity(ControlProfile.TouchPhone, true, time, 0f); return; }
            foreach (var pad in Gamepad.all)
            {
                foreach (var control in pad.allControls)
                    if (control is ButtonControl button && button.wasPressedThisFrame && !(control.parent is StickControl))
                    { RecordActivity(ControlProfile.Gamepad, true, time, 0f); return; }
                var stick = pad.leftStick.ReadValue().sqrMagnitude >= pad.rightStick.ReadValue().sqrMagnitude
                    ? pad.leftStick.ReadValue() : pad.rightStick.ReadValue();
                _lastSticks.TryGetValue(pad.deviceId, out var previousStick);
                if (stick.magnitude > _gamepadDeadzone && (stick - previousStick).sqrMagnitude > 0.0025f)
                { _lastSticks[pad.deviceId] = stick; RecordActivity(ControlProfile.Gamepad, true, time, 0f); return; }
                _lastSticks[pad.deviceId] = stick;
            }
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.anyKey.wasPressedThisFrame)
            { RecordActivity(ResolvePointerProfile(Mouse.current), true, time, 0f); return; }
            foreach (var device in InputSystem.devices)
            {
                if (!(device is Mouse mouse)) continue;
                if (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame ||
                    mouse.middleButton.wasPressedThisFrame || mouse.scroll.ReadValue().sqrMagnitude > 0.01f)
                { RecordActivity(ResolvePointerProfile(mouse), true, time, 0f); return; }
                if (mouse.delta.ReadValue().sqrMagnitude > 9f)
                { RecordActivity(ResolvePointerProfile(mouse), false, time, Time.unscaledDeltaTime); return; }
            }
            _motionDuration = 0f;
        }
    }
}
