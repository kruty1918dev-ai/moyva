using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.InputRouting.API;
using Kruty1918.Moyva.UIActions.API;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Zenject;

namespace Kruty1918.Moyva.UIActions.Runtime
{
    internal sealed class UiHotkeyService : IUiHotkeyService, ITickable
    {
        private readonly IUiActionRouter _actions;
        private readonly IUiContextStack _contexts;
        private readonly IGameplayInputPolicy _inputPolicy;
        private readonly List<UiHotkeyBinding> _bindings = new();
        private readonly HashSet<UiActionId> _heldActionsTriggered = new();

        public UiHotkeyService(
            IUiActionRouter actions,
            IUiContextStack contexts,
            [InjectOptional] IGameplayInputPolicy inputPolicy = null)
        {
            _actions = actions;
            _contexts = contexts;
            _inputPolicy = inputPolicy;
            ResetDefaults();
        }

        public IReadOnlyList<UiHotkeyBinding> Bindings => _bindings;

        public void Tick()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            Vector2 pointerPosition = Mouse.current?.position.ReadValue() ?? Vector2.zero;
            if (!(_inputPolicy?.CanProcess(GameplayInputKind.KeyboardNavigation, pointerPosition) ?? true))
                return;

            bool typing = IsTextInputFocused();

            for (int i = 0; i < _bindings.Count; i++)
            {
                UiHotkeyBinding binding = _bindings[i];
                if (!binding.HasBinding || !_contexts.IsActionAllowedByContext(binding.ActionId))
                    continue;

                if (typing && binding.PrimaryKey != Key.Escape && binding.SecondaryKey != Key.Escape)
                    continue;

                if (!IsContextAllowed(binding))
                    continue;

                if (!WasTriggered(keyboard, binding))
                    continue;

                _actions.Execute(binding.ActionId, UiActionSource.Hotkey, _contexts.ActiveContextId);
            }
        }

        public string GetBindingLabel(UiActionId actionId)
        {
            UiHotkeyBinding binding = _bindings.FirstOrDefault(x => x.ActionId == actionId);
            return binding.HasBinding ? KeyLabel(binding.PrimaryKey, binding) : string.Empty;
        }

        public bool SetBinding(UiHotkeyBinding binding)
        {
            if (!UiActionId.IsValid(binding.ActionId.Value))
                return false;

            int index = _bindings.FindIndex(x => x.ActionId == binding.ActionId);
            if (index >= 0)
                _bindings[index] = binding;
            else
                _bindings.Add(binding);

            return DetectConflicts().Count == 0;
        }

        public void ResetDefaults()
        {
            _bindings.Clear();
            _bindings.Add(new UiHotkeyBinding(UiActionIds.Construction.Toggle, Key.B, allowedContexts: new[] { "Gameplay", "ConstructionMode" }));
            _bindings.Add(new UiHotkeyBinding(UiActionIds.Construction.ConfirmPlacement, Key.Enter, Key.NumpadEnter, allowedContexts: new[] { "ConstructionMode", "BuildingPlacement" }));
            _bindings.Add(new UiHotkeyBinding(UiActionIds.Construction.RotatePlacement, Key.R, allowedContexts: new[] { "ConstructionMode", "BuildingPlacement" }));
            _bindings.Add(new UiHotkeyBinding(UiActionIds.Construction.UndoPlacement, Key.Z, ctrl: true, allowedContexts: new[] { "ConstructionMode", "BuildingPlacement" }));
            _bindings.Add(new UiHotkeyBinding(UiActionIds.Construction.RedoPlacement, Key.Y, ctrl: true, allowedContexts: new[] { "ConstructionMode", "BuildingPlacement" }));
            _bindings.Add(new UiHotkeyBinding(UiActionIds.Deployment.Confirm, Key.Enter, Key.NumpadEnter, allowedContexts: new[] { "DeploymentMode" }));
        }

        public IReadOnlyList<string> DetectConflicts()
        {
            var conflicts = new List<string>();
            for (int i = 0; i < _bindings.Count; i++)
            {
                for (int j = i + 1; j < _bindings.Count; j++)
                {
                    if (!SameChord(_bindings[i], _bindings[j]))
                        continue;

                    if (ContextsOverlap(_bindings[i].AllowedContexts, _bindings[j].AllowedContexts))
                    {
                        conflicts.Add(
                            $"{_bindings[i].ActionId} conflicts with {_bindings[j].ActionId} on {KeyLabel(_bindings[i].PrimaryKey, _bindings[i])}");
                    }
                }
            }

            return conflicts;
        }

        private bool WasTriggered(
            Keyboard keyboard,
            UiHotkeyBinding binding)
        {
            if (!ModifiersMatch(keyboard, binding))
            {
                _heldActionsTriggered.Remove(binding.ActionId);
                return false;
            }

            bool pressed = KeyPressedThisFrame(keyboard, binding.PrimaryKey)
                || KeyPressedThisFrame(keyboard, binding.SecondaryKey);

            if (binding.TriggerMode == UiHotkeyTriggerMode.TriggeredOnce)
                return pressed;

            bool held = KeyHeld(keyboard, binding.PrimaryKey)
                || KeyHeld(keyboard, binding.SecondaryKey);
            if (!held)
            {
                _heldActionsTriggered.Remove(binding.ActionId);
                return false;
            }

            if (binding.TriggerMode == UiHotkeyTriggerMode.Held)
                return true;

            return _heldActionsTriggered.Add(binding.ActionId);
        }

        private bool IsContextAllowed(UiHotkeyBinding binding)
        {
            if (binding.AllowedContexts == null || binding.AllowedContexts.Count == 0)
                return true;

            IReadOnlyList<UiContextRegistration> active = _contexts.ActiveContexts;
            for (int i = 0; i < active.Count; i++)
            {
                if (binding.AllowedContexts.Contains(active[i].ContextId))
                    return true;
            }

            return binding.AllowedContexts.Contains("Gameplay");
        }

        private static bool IsTextInputFocused()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null || eventSystem.currentSelectedGameObject == null)
                return false;

            TMP_InputField input = eventSystem.currentSelectedGameObject.GetComponent<TMP_InputField>();
            return input != null && input.isFocused;
        }

        private static bool ModifiersMatch(Keyboard keyboard, UiHotkeyBinding binding)
        {
            bool ctrl = keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;
            bool shift = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            bool alt = keyboard.leftAltKey.isPressed || keyboard.rightAltKey.isPressed;
            return ctrl == binding.Ctrl && shift == binding.Shift && alt == binding.Alt;
        }

        private static bool KeyPressedThisFrame(Keyboard keyboard, Key key)
            => key != Key.None && keyboard[key].wasPressedThisFrame;

        private static bool KeyHeld(Keyboard keyboard, Key key)
            => key != Key.None && keyboard[key].isPressed;

        private static bool SameChord(UiHotkeyBinding left, UiHotkeyBinding right)
            => left.PrimaryKey == right.PrimaryKey
                && left.Ctrl == right.Ctrl
                && left.Shift == right.Shift
                && left.Alt == right.Alt;

        private static bool ContextsOverlap(
            IReadOnlyCollection<string> left,
            IReadOnlyCollection<string> right)
        {
            if (left == null || left.Count == 0 || right == null || right.Count == 0)
                return true;

            return left.Any(right.Contains);
        }

        private static string KeyLabel(Key key, UiHotkeyBinding binding)
        {
            if (key == Key.None)
                return string.Empty;

            var parts = new List<string>();
            if (binding.Ctrl)
                parts.Add("Ctrl");
            if (binding.Shift)
                parts.Add("Shift");
            if (binding.Alt)
                parts.Add("Alt");
            parts.Add(key.ToString());
            return string.Join("+", parts);
        }
    }
}
