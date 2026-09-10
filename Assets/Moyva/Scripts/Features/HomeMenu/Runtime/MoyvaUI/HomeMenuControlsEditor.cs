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

        public string SelectedKey { get; private set; } = "w";
        public PlayerControlModifiers Modifiers { get; private set; }
        public PlayerControlAction SelectedAction { get; private set; } = PlayerControlAction.MoveForward;
        public bool IsCapturing { get; private set; }
        public string Notice { get; private set; } = "Choose a key to explore its bindings.";
        public string DraftPath => PlayerControlBinding.CreatePath(SelectedKey, Modifiers);
        public string DraftLabel => PlayerControlBinding.TryParse(DraftPath, out var binding) ? binding.DisplayName : SelectedKey.ToUpperInvariant();
        public bool CanApply => !IsCapturing && _view.SettingsInteractable && Conflict.Length == 0 && !IsCurrentBinding;
        private bool IsCurrentBinding => _view.ControlBindings.TryGetValue(SelectedAction, out var current) && current == DraftPath;

        public string Conflict
        {
            get
            {
                var reserved = ReservedAction(SelectedKey, Modifiers);
                if (reserved.Length > 0) return "Reserved for " + reserved + ". Choose another shortcut.";
                foreach (var pair in _view.ControlBindings)
                    if (pair.Key != SelectedAction && pair.Value == DraftPath)
                        return "Already used by " + ActionLabel(pair.Key) + ". Choose another key or add a modifier.";
                return string.Empty;
            }
        }

        public HomeMenuControlsEditor(HomeMenuMoyvaUiState state, HomeMenuMoyvaUiViewController view, IUiHotkeyService hotkeys)
        {
            _state = state;
            _view = view;
            _hotkeys = hotkeys;
        }

        public void SelectKey(string keyName)
        {
            if (!PlayerControlBinding.TryParse(PlayerControlBinding.CreatePath(keyName, Modifiers), out var binding)) return;
            SelectedKey = binding.ControlPath.Substring("<Keyboard>/".Length);
            IsCapturing = false;
            Notice = "Choose an action below, then apply your shortcut.";
            var action = BoundAction(SelectedKey, Modifiers);
            if (action.HasValue) SelectedAction = action.Value;
            _state.MarkDirty();
        }

        public void ToggleModifier(int modifier)
        {
            if (modifier != (int)PlayerControlModifiers.Ctrl && modifier != (int)PlayerControlModifiers.Shift && modifier != (int)PlayerControlModifiers.Alt) return;
            Modifiers ^= (PlayerControlModifiers)modifier;
            IsCapturing = false;
            Notice = "Keyboard highlights show bindings for the selected modifiers.";
            _state.MarkDirty();
        }

        public void SelectAction(int actionIndex, bool revealBinding)
        {
            if (!Enum.IsDefined(typeof(PlayerControlAction), actionIndex)) return;
            SelectedAction = (PlayerControlAction)actionIndex;
            IsCapturing = false;
            if (revealBinding && _view.ControlBindings.TryGetValue(SelectedAction, out var path) && PlayerControlBinding.TryParse(path, out var binding))
            {
                SelectedKey = binding.ControlPath.Substring("<Keyboard>/".Length);
                Modifiers = binding.Modifiers;
            }
            Notice = "Select a key or record a shortcut for " + ActionLabel(SelectedAction) + ".";
            _state.MarkDirty();
        }

        public void StartCapture()
        {
            if (!_view.SettingsInteractable) return;
            IsCapturing = true;
            _captureStartFrame = Time.frameCount;
            _captureDeadline = Time.unscaledTime + 15f;
            Notice = "Press a key, optionally holding Ctrl, Shift or Alt. Esc cancels.";
            _state.MarkDirty();
        }

        public void CancelCapture()
        {
            if (!IsCapturing) return;
            IsCapturing = false;
            Notice = "Recording cancelled. Your saved bindings are unchanged.";
            _state.MarkDirty();
        }

        public void Tick()
        {
            if (!IsCapturing) return;
            if (_state.CurrentRoute != "SettingsPanel" || _state.SettingsSection != HomeMenuSettingsSection.Controls ||
                _view.ConfirmationVisible || !_view.SettingsInteractable || !Application.isFocused || Time.unscaledTime >= _captureDeadline)
            {
                CancelCapture();
                return;
            }
            var keyboard = Keyboard.current;
            if (keyboard == null || Time.frameCount <= _captureStartFrame) return;
            if (keyboard.escapeKey.wasPressedThisFrame) { CancelCapture(); return; }
            if (keyboard.leftMetaKey.isPressed || keyboard.rightMetaKey.isPressed)
            {
                Notice = "Use Ctrl, Shift or Alt for shortcuts. Release the system key to continue.";
                _state.MarkDirty();
                return;
            }
            foreach (var key in keyboard.allKeys)
            {
                if (!key.wasPressedThisFrame || PlayerControlBinding.IsModifierKey(key.keyCode)) continue;
                var path = PlayerControlBinding.CreatePath(key.name, PlayerControlBinding.ReadModifiers(keyboard));
                if (!PlayerControlBinding.TryParse(path, out var binding)) continue;
                SelectedKey = binding.ControlPath.Substring("<Keyboard>/".Length);
                Modifiers = binding.Modifiers;
                IsCapturing = false;
                Notice = "Shortcut recorded. Apply to save, or keep editing.";
                _state.MarkDirty();
                break;
            }
        }

        public void Apply()
        {
            if (!CanApply) return;
            _view.SetControlBinding(SelectedAction, DraftPath);
            Notice = IsCurrentBinding ? "Saved: " + DraftLabel + " — " + ActionLabel(SelectedAction) + "." : "Could not save this shortcut. Please try again.";
            _state.MarkDirty();
        }

        public void ResetSelection()
        {
            IsCapturing = false;
            SelectedKey = "w";
            Modifiers = PlayerControlModifiers.None;
            SelectedAction = PlayerControlAction.MoveForward;
            Notice = "Default camera controls restored.";
            _state.MarkDirty();
        }

        public PlayerControlAction? BoundAction(string key, PlayerControlModifiers modifiers)
        {
            var path = PlayerControlBinding.CreatePath(key, modifiers);
            foreach (var pair in _view.ControlBindings)
                if (pair.Value == path) return pair.Key;
            return null;
        }

        public string ReservedAction(string key, PlayerControlModifiers modifiers)
        {
            if (key == "escape") return "Back / pause";
            if (!PlayerControlBinding.TryParse(PlayerControlBinding.CreatePath(key, modifiers), out var candidate)) return string.Empty;
            var label = string.Empty;
            foreach (var binding in _hotkeys?.Bindings ?? _defaultHotkeys)
            {
                if (binding.PrimaryKey != candidate.Key && binding.SecondaryKey != candidate.Key) continue;
                if (binding.Ctrl != modifiers.HasFlag(PlayerControlModifiers.Ctrl) || binding.Shift != modifiers.HasFlag(PlayerControlModifiers.Shift) || binding.Alt != modifiers.HasFlag(PlayerControlModifiers.Alt)) continue;
                var action = binding.ActionId;
                var name = action == UiActionIds.Construction.Toggle ? "Construction" :
                    action == UiActionIds.Construction.RotatePlacement ? "Rotate building" :
                    action == UiActionIds.Construction.ConfirmPlacement ? "Place building" :
                    action == UiActionIds.Construction.UndoPlacement ? "Undo placement" :
                    action == UiActionIds.Construction.RedoPlacement ? "Redo placement" :
                    action == UiActionIds.Deployment.Confirm ? "Deploy unit" : "Gameplay action";
                label += (label.Length == 0 ? "" : " / ") + name;
            }
            return label;
        }

        public static string ActionLabel(PlayerControlAction action) => action switch
        {
            PlayerControlAction.MoveForward => "Move forward", PlayerControlAction.MoveBackward => "Move backward",
            PlayerControlAction.MoveLeft => "Move left", PlayerControlAction.MoveRight => "Move right",
            PlayerControlAction.RotateLeft => "Rotate left", PlayerControlAction.RotateRight => "Rotate right",
            PlayerControlAction.ZoomIn => "Zoom in", PlayerControlAction.ZoomOut => "Zoom out", _ => action.ToString()
        };

        public static string ActionGroup(PlayerControlAction action) => (int)action < 4 ? "movement" : (int)action < 6 ? "orbit" : "zoom";
    }
}
