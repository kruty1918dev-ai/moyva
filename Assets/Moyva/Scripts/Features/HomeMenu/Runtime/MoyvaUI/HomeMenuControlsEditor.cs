using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Shared.Controls;
using Kruty1918.Moyva.UIActions.API;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    // Owns the unsaved selection only; the settings service remains the binding authority.
    internal sealed class HomeMenuControlsEditor
    {
        private readonly HomeMenuMoyvaUiState _state;
        private readonly HomeMenuMoyvaUiViewController _view;
        private readonly IUiHotkeyService _hotkeys;
        private readonly IReadOnlyList<UiHotkeyBinding> _defaultHotkeys = UiHotkeyBinding.CreateDefaults();
        private int _captureStartFrame;
        private float _captureDeadline;
        private bool _captureArmed;

        private readonly IPlayerControlSettingsService _settings;
        private readonly IInputDeviceContext _devices;
        private float _nextLiveUpdate;
        private string _liveSignature;
        public int DeviceTab { get; private set; }
        public int GestureSlot { get; private set; }
        public bool IsTesting { get; private set; }
        public ControlProfile EditingProfile => DeviceTab < 2 ? ControlProfile.KeyboardMouse : DeviceTab == 2 ? ControlProfile.KeyboardTouchpad : DeviceTab == 3 ? ControlProfile.Gamepad : ControlProfile.TouchPhone;
        public ControlProfile ActiveProfile => _devices?.ActiveProfile ?? ControlProfile.KeyboardMouse;
        public ControlProfile ProfileSelection => _devices?.Selection ?? ControlProfile.Auto;
        public PointerInterpretation PointerMode => _devices?.PointerMode ?? PointerInterpretation.Auto;
        public PlayerControlProfile Options => _settings?.Settings.Profile(EditingProfile);
        public IReadOnlyDictionary<PlayerControlAction, string> Bindings => Options?.ToBindings() ?? _view.ControlBindings;
        public static string ProfileLabel(ControlProfile profile) => profile switch {
            ControlProfile.KeyboardMouse => "Keyboard + Mouse", ControlProfile.KeyboardTouchpad => "Keyboard + Touchpad",
            ControlProfile.Gamepad => "Gamepad", ControlProfile.TouchPhone => "Touch / Phone", _ => "Auto" };

        public void SelectDevice(int tab)
        {
            if (tab < 0 || tab > 4) return;
            DeviceTab = tab; GestureSlot = 0; IsCapturing = false;
            SelectAction(tab == 1 || tab == 4 ? (int)PlayerControlAction.PrimarySelect : 0, true);
            if (tab == 2) SelectGestureSlot(1);
            _state.MarkDirty();
        }
        public void SelectProfile(int profile)
        {
            if (!_view.SettingsInteractable || !Enum.IsDefined(typeof(ControlProfile), profile)) return;
            _settings?.ConfigureDevices((ControlProfile)profile, PointerMode); _state.MarkDirty();
        }
        public void SelectPointerMode(int mode)
        {
            if (!_view.SettingsInteractable || !Enum.IsDefined(typeof(PointerInterpretation), mode)) return;
            _settings?.ConfigureDevices(ProfileSelection, (PointerInterpretation)mode); _state.MarkDirty();
        }
        public void ToggleTest() { IsTesting = !IsTesting; IsCapturing = false; _state.MarkDirty(); }
        public void ResetProfile()
        {
            if (!_view.SettingsInteractable) return;
            _settings?.ResetProfile(EditingProfile); ResetSelection();
        }
        public void ChangeOption(int option, float value)
        {
            if (!_view.SettingsInteractable || Options == null) return;
            var next = Options.Copy();
            if (option == 0) next.Sensitivity = value;
            if (option == 1) next.Deadzone = value;
            if (option == 2) next.EdgePan = !next.EdgePan;
            if (option == 3) next.Gestures = !next.Gestures;
            _settings.SetProfileOptions(next); _state.MarkDirty();
        }
        public void SelectGestureSlot(int slot)
        {
            if (slot < 1 || slot > 2 || Options == null) return;
            GestureSlot = slot;
            var path = slot == 1 ? Options.PanBinding : Options.OrbitBinding;
            if (PlayerControlBinding.TryParse(path, out var binding)) { SelectedKey = binding.ControlPath; Modifiers = binding.Modifiers; }
            _state.MarkDirty();
        }
        public bool IsLive(string key)
        {
            if (key == "<Touch>/drag" || key == "<Touch>/tap" || key == "<Touch>/longPress") return Touchscreen.current?.primaryTouch.press.isPressed == true;
            if (key.StartsWith("<Touch>/", StringComparison.Ordinal) && Touchscreen.current != null)
            {
                UnityEngine.InputSystem.Controls.TouchControl first = null, second = null;
                foreach (var touch in Touchscreen.current.touches)
                    if (touch.press.isPressed) { if (first == null) first = touch; else { second = touch; break; } }
                if (first == null || second == null) return false;
                var now = second.position.ReadValue() - first.position.ReadValue();
                var previous = now - (second.delta.ReadValue() - first.delta.ReadValue());
                if (key == "<Touch>/pinchOut") return now.magnitude - previous.magnitude > 1f;
                if (key == "<Touch>/pinchIn") return previous.magnitude - now.magnitude > 1f;
                if (key == "<Touch>/twist") return Mathf.Abs(Vector2.SignedAngle(previous, now)) > 0.5f;
            }
            if (PlayerControlBinding.TryParse(PlayerControlBinding.CreatePath(key, PlayerControlModifiers.None), out var binding))
                return binding.Key != Key.None ? Keyboard.current?[binding.Key].isPressed == true : binding.ReadValue(Options?.Deadzone ?? 0.2f) > 0f;
            if (Enum.TryParse(key, true, out Key keyboardKey) && keyboardKey != Key.None)
                return Keyboard.current?[keyboardKey].isPressed == true;
            return false;
        }
        public string TestReadout
        {
            get
            {
                string result = string.Empty;
                foreach (var pair in Bindings)
                    if (IsLive(pair.Value)) result += (result.Length == 0 ? "" : " · ") + _view.T(ActionLabel(pair.Key));
                if (Options != null && IsLive(Options.PanBinding)) result += " · " + _view.T("Pan");
                if (Options != null && IsLive(Options.OrbitBinding)) result += " · " + _view.T("Orbit");
                return result.Length == 0 ? _view.T("Ready · Try a key, button, stick or gesture") : result;
            }
        }

        private string LiveSignature()
        {
            var result = ActiveProfile.ToString();
            if (Keyboard.current != null)
                foreach (var key in Keyboard.current.allKeys) if (key.isPressed) result += key.name;
            foreach (var path in PlayerControlBinding.DevicePaths) if (IsLive(path)) result += path;
            return result;
        }

        public string SelectedKey { get; private set; } = "w";
        public PlayerControlModifiers Modifiers { get; private set; }
        public PlayerControlAction SelectedAction { get; private set; } = PlayerControlAction.MoveForward;
        public bool IsCapturing { get; private set; }
        private string _notice;
        public string Notice
        {
            get => _notice ?? _view.T("Choose a key to explore its bindings.");
            private set => _notice = value;
        }
        public string DraftPath => PlayerControlBinding.CreatePath(SelectedKey, Modifiers);
        public string DraftLabel => PlayerControlBinding.TryParse(DraftPath, out var binding) ? binding.DisplayName : SelectedKey.ToUpperInvariant();
        public bool CanApply => !IsCapturing && _view.SettingsInteractable && Conflict.Length == 0 && !IsCurrentBinding;
        private bool IsCurrentBinding => GestureSlot == 1 ? Options?.PanBinding == DraftPath : GestureSlot == 2 ? Options?.OrbitBinding == DraftPath : Bindings.TryGetValue(SelectedAction, out var current) && current == DraftPath;

        public string Conflict
        {
            get
            {
                var reserved = ReservedAction(SelectedKey, Modifiers);
                if (reserved.Length > 0) return _view.TF("Reserved for {0}. Choose another shortcut.", _view.T(reserved));
                foreach (var pair in Bindings)
                    if ((GestureSlot > 0 || pair.Key != SelectedAction) && PlayerControlBinding.Conflicts(pair.Value, DraftPath))
                        return _view.TF("Already used by {0}. Choose another key or add a modifier.", _view.T(ActionLabel(pair.Key)));
                if (GestureSlot != 1 && PlayerControlBinding.Conflicts(Options?.PanBinding, DraftPath)) return _view.T("Already used for pan.");
                if (GestureSlot != 2 && PlayerControlBinding.Conflicts(Options?.OrbitBinding, DraftPath)) return _view.T("Already used for orbit.");
                return string.Empty;
            }
        }

        public HomeMenuControlsEditor(HomeMenuMoyvaUiState state, HomeMenuMoyvaUiViewController view, IUiHotkeyService hotkeys, IPlayerControlSettingsService settings = null, IInputDeviceContext devices = null)
        {
            _state = state;
            _view = view;
            _hotkeys = hotkeys;
            _settings = settings; _devices = devices;
        }

        public void SelectKey(string keyName)
        {
            if (!PlayerControlBinding.TryParse(PlayerControlBinding.CreatePath(keyName, Modifiers), out var binding)) return;
            SelectedKey = binding.ControlPath.StartsWith("<Keyboard>/", StringComparison.Ordinal) ? binding.ControlPath.Substring("<Keyboard>/".Length) : binding.ControlPath;
            IsCapturing = false;
            Notice = _view.T("Choose an action below, then apply your shortcut.");
            var action = BoundAction(SelectedKey, Modifiers);
            if (action.HasValue) { SelectedAction = action.Value; GestureSlot = 0; }
            else if (PlayerControlBinding.Conflicts(Options?.PanBinding, DraftPath)) GestureSlot = 1;
            else if (PlayerControlBinding.Conflicts(Options?.OrbitBinding, DraftPath)) GestureSlot = 2;
            _state.MarkDirty();
        }

        public void ToggleModifier(int modifier)
        {
            if (modifier != (int)PlayerControlModifiers.Ctrl && modifier != (int)PlayerControlModifiers.Shift && modifier != (int)PlayerControlModifiers.Alt) return;
            Modifiers ^= (PlayerControlModifiers)modifier;
            IsCapturing = false;
            Notice = _view.T("Keyboard highlights show bindings for the selected modifiers.");
            _state.MarkDirty();
        }

        public void SelectAction(int actionIndex, bool revealBinding)
        {
            if (!Enum.IsDefined(typeof(PlayerControlAction), actionIndex)) return;
            GestureSlot = 0;
            SelectedAction = (PlayerControlAction)actionIndex;
            IsCapturing = false;
            if (revealBinding && Bindings.TryGetValue(SelectedAction, out var path) && PlayerControlBinding.TryParse(path, out var binding))
            {
                SelectedKey = binding.ControlPath.StartsWith("<Keyboard>/", StringComparison.Ordinal) ? binding.ControlPath.Substring("<Keyboard>/".Length) : binding.ControlPath;
                Modifiers = binding.Modifiers;
            }
            Notice = _view.TF("Select a key or record a shortcut for {0}.", _view.T(ActionLabel(SelectedAction)));
            _state.MarkDirty();
        }

        public void StartCapture()
        {
            if (!_view.SettingsInteractable) return;
            IsCapturing = true;
            _captureArmed = false;
            _captureStartFrame = Time.frameCount;
            _captureDeadline = Time.unscaledTime + 15f;
            Notice = _view.T("Press a key or device control. Choose gestures on the diagram. Esc cancels.");
            _state.MarkDirty();
        }

        public void CancelCapture()
        {
            if (!IsCapturing) return;
            IsCapturing = false;
            Notice = _view.T("Recording cancelled. Your saved bindings are unchanged.");
            _state.MarkDirty();
        }

        public void Tick()
        {
            bool visible = _state.CurrentRoute == "SettingsPanel" && _state.SettingsSection == HomeMenuSettingsSection.Controls;
            if (!visible) { IsTesting = false; CancelCapture(); return; }
            if (Time.unscaledTime >= _nextLiveUpdate)
            {
                _nextLiveUpdate = Time.unscaledTime + 0.08f;
                string activity = LiveSignature();
                if (_liveSignature != activity) { _liveSignature = activity; _state.MarkDirty(); }
            }
            if (!IsCapturing) return;
            if (_state.CurrentRoute != "SettingsPanel" || _state.SettingsSection != HomeMenuSettingsSection.Controls ||
                _view.ConfirmationVisible || !_view.SettingsInteractable || !Application.isFocused || Time.unscaledTime >= _captureDeadline)
            {
                CancelCapture();
                return;
            }
            var keyboard = Keyboard.current;
            if (Time.frameCount <= _captureStartFrame) return;
            if (!_captureArmed)
            {
                if (Mouse.current?.leftButton.isPressed == true || Mouse.current?.rightButton.isPressed == true || Mouse.current?.middleButton.isPressed == true) return;
                _captureArmed = true; return;
            }
            foreach (var path in PlayerControlBinding.DevicePaths)
            {
                if (!path.StartsWith("<Mouse>", StringComparison.Ordinal) && !path.StartsWith("<Gamepad>", StringComparison.Ordinal)) continue;
                if (!PlayerControlBinding.TryParse(path, out var candidate) || candidate.ReadValue(0.4f) <= 0f) continue;
                SelectedKey = path; Modifiers = PlayerControlBinding.ReadModifiers(keyboard);
                IsCapturing = false; Notice = _view.T("Recorded. Apply to save."); _state.MarkDirty(); return;
            }
            if (keyboard == null) return;
            if (keyboard.escapeKey.wasPressedThisFrame) { CancelCapture(); return; }
            if (keyboard.leftMetaKey.isPressed || keyboard.rightMetaKey.isPressed)
            {
                Notice = _view.T("Use Ctrl, Shift or Alt for shortcuts. Release the system key to continue.");
                _state.MarkDirty();
                return;
            }
            foreach (var key in keyboard.allKeys)
            {
                if (!key.wasPressedThisFrame || PlayerControlBinding.IsModifierKey(key.keyCode)) continue;
                var path = PlayerControlBinding.CreatePath(key.name, PlayerControlBinding.ReadModifiers(keyboard));
                if (!PlayerControlBinding.TryParse(path, out var binding)) continue;
                SelectedKey = binding.ControlPath.StartsWith("<Keyboard>/", StringComparison.Ordinal) ? binding.ControlPath.Substring("<Keyboard>/".Length) : binding.ControlPath;
                Modifiers = binding.Modifiers;
                IsCapturing = false;
                Notice = _view.T("Shortcut recorded. Apply to save, or keep editing.");
                _state.MarkDirty();
                break;
            }
        }

        public void Apply()
        {
            if (!CanApply) return;
            if (_settings != null)
            {
                if (GestureSlot > 0)
                {
                    var options = Options.Copy();
                    if (GestureSlot == 1) options.PanBinding = DraftPath; else options.OrbitBinding = DraftPath;
                    _settings.SetProfileOptions(options);
                }
                else _settings.TrySetProfileBinding(EditingProfile, SelectedAction, DraftPath, out _);
            }
            else _view.SetControlBinding(SelectedAction, DraftPath);
            Notice = IsCurrentBinding ? _view.TF("Saved: {0} — {1}.", DraftLabel, _view.T(ActionLabel(SelectedAction))) : _view.T("Could not save this shortcut. Please try again.");
            _state.MarkDirty();
        }

        public void ResetSelection()
        {
            IsCapturing = false;
            GestureSlot = 0;
            SelectedKey = "w";
            Modifiers = PlayerControlModifiers.None;
            SelectedAction = PlayerControlAction.MoveForward;
            Notice = _view.T("Default camera controls restored.");
            _state.MarkDirty();
        }

        public PlayerControlAction? BoundAction(string key, PlayerControlModifiers modifiers)
        {
            var path = PlayerControlBinding.CreatePath(key, modifiers);
            foreach (var pair in Bindings)
                if (pair.Value == path) return pair.Key;
            return null;
        }

        public string ReservedAction(string key, PlayerControlModifiers modifiers)
        {
            if (key == "escape") return _view.T("Back / pause");
            if (!PlayerControlBinding.TryParse(PlayerControlBinding.CreatePath(key, modifiers), out var candidate)) return string.Empty;
            if (candidate.Key == Key.None) return string.Empty;
            var label = string.Empty;
            foreach (var binding in _hotkeys?.Bindings ?? _defaultHotkeys)
            {
                if (binding.PrimaryKey != candidate.Key && binding.SecondaryKey != candidate.Key) continue;
                if (binding.Ctrl != modifiers.HasFlag(PlayerControlModifiers.Ctrl) || binding.Shift != modifiers.HasFlag(PlayerControlModifiers.Shift) || binding.Alt != modifiers.HasFlag(PlayerControlModifiers.Alt)) continue;
                var action = binding.ActionId;
                var name = action == UiActionIds.Construction.Toggle ? _view.T("Construction") :
                    action == UiActionIds.Construction.RotatePlacement ? _view.T("Rotate building") :
                    action == UiActionIds.Construction.ConfirmPlacement ? _view.T("Place building") :
                    action == UiActionIds.Construction.UndoPlacement ? _view.T("Undo placement") :
                    action == UiActionIds.Construction.RedoPlacement ? _view.T("Redo placement") :
                    action == UiActionIds.Deployment.Confirm ? _view.T("Deploy unit") : _view.T("Gameplay action");
                label += (label.Length == 0 ? "" : " / ") + name;
            }
            return label;
        }

        public static string ActionLabel(PlayerControlAction action) => action switch
        {
            PlayerControlAction.MoveForward => "Move forward", PlayerControlAction.MoveBackward => "Move backward",
            PlayerControlAction.MoveLeft => "Move left", PlayerControlAction.MoveRight => "Move right",
            PlayerControlAction.RotateLeft => "Rotate left", PlayerControlAction.RotateRight => "Rotate right",
            PlayerControlAction.ZoomIn => "Zoom in", PlayerControlAction.PrimarySelect => "Select", PlayerControlAction.SecondarySelect => "Secondary action",
            PlayerControlAction.ZoomOut => "Zoom out", _ => action.ToString()
        };

        public static string ActionGroup(PlayerControlAction action) => (int)action < 4 ? "movement" : (int)action < 6 ? "orbit" : (int)action < 8 ? "zoom" : "selection";
    }
}
