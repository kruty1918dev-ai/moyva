using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Kruty1918.Moyva.Shared.Controls
{
    [Flags]
    public enum PlayerControlModifiers
    {
        None = 0,
        Ctrl = 1,
        Shift = 2,
        Alt = 4
    }

    /// <summary>An allow-listed device control or gesture with exact keyboard modifiers.</summary>
    public readonly struct PlayerControlBinding
    {
        private const string KeyboardPrefix = "<Keyboard>/";
        private const PlayerControlModifiers AllModifiers =
            PlayerControlModifiers.Ctrl | PlayerControlModifiers.Shift | PlayerControlModifiers.Alt;

        public Key Key { get; }
        public PlayerControlModifiers Modifiers { get; }
        public string ControlPath { get; }
        public string CanonicalPath { get; }
        public string DisplayName { get; }

        private PlayerControlBinding(Key key, PlayerControlModifiers modifiers)
        {
            Key = key;
            Modifiers = modifiers;
            string keyName = key.ToString();
            string controlName = keyName.StartsWith("Digit", StringComparison.Ordinal)
                ? keyName.Substring(5)
                : char.ToLowerInvariant(keyName[0]) + keyName.Substring(1);
            ControlPath = KeyboardPrefix + controlName;
            CanonicalPath = ModifierPrefix(modifiers, "+") + ControlPath;
            DisplayName = ModifierPrefix(modifiers, " + ") + KeyLabel(key, controlName);
        }

        private PlayerControlBinding(string path, PlayerControlModifiers modifiers)
        {
            Key = Key.None; Modifiers = modifiers; ControlPath = path;
            CanonicalPath = ModifierPrefix(modifiers, "+") + path;
            DisplayName = ModifierPrefix(modifiers, " + ") + path.Replace("<", "").Replace(">/", " · ");
        }

        // Stable allow-list, including synthetic gestures evaluated by the camera adapter.
        public static readonly string[] DevicePaths = {
            "<Mouse>/leftButton", "<Mouse>/rightButton", "<Mouse>/middleButton",
            "<Mouse>/forwardButton", "<Mouse>/backButton", "<Mouse>/scroll/up", "<Mouse>/scroll/down",
            "<Gamepad>/start", "<Gamepad>/select", "<Gamepad>/leftStickPress", "<Gamepad>/rightStickPress",
            "<Gamepad>/buttonSouth", "<Gamepad>/buttonNorth", "<Gamepad>/buttonEast", "<Gamepad>/buttonWest",
            "<Gamepad>/leftShoulder", "<Gamepad>/rightShoulder", "<Gamepad>/leftTrigger", "<Gamepad>/rightTrigger",
            "<Gamepad>/leftStick/up", "<Gamepad>/leftStick/down", "<Gamepad>/leftStick/left", "<Gamepad>/leftStick/right",
            "<Gamepad>/rightStick/up", "<Gamepad>/rightStick/down", "<Gamepad>/rightStick/left", "<Gamepad>/rightStick/right",
            "<Gamepad>/dpad/up", "<Gamepad>/dpad/down", "<Gamepad>/dpad/left", "<Gamepad>/dpad/right",
            "<Touchpad>/spaceDrag", "<Touchpad>/scroll/up", "<Touchpad>/scroll/down",
            "<Touch>/tap", "<Touch>/longPress", "<Touch>/drag", "<Touch>/pinchIn", "<Touch>/pinchOut", "<Touch>/twist"
        };

        public float ReadValue(float deadzone = 0.2f)
        {
            if (Key != Key.None) return IsPressed(Keyboard.current) ? 1f : 0f;
            if (ReadModifiers(Keyboard.current) != Modifiers) return 0f;
            string path = ControlPath;
            if (path == "<Touchpad>/spaceDrag") return Keyboard.current?.spaceKey.isPressed == true ? 1f : 0f;
            if (path?.StartsWith("<Touchpad>/scroll/", StringComparison.Ordinal) == true)
                path = path.Replace("<Touchpad>", "<Mouse>");
            if (path == null || path.StartsWith("<Touch>", StringComparison.Ordinal)) return 0f;
            float value = 0f;
            using var controls = InputSystem.FindControls(path);
            foreach (var control in controls)
                if (control is AxisControl axis) value = UnityEngine.Mathf.Max(value, axis.ReadValue());
            if (path.Contains("scroll/")) return value / 120f;
            return value <= deadzone ? 0f : UnityEngine.Mathf.Clamp01((value - deadzone) / (1f - deadzone));
        }

        public static bool TryParse(string value, out PlayerControlBinding binding)
        {
            binding = default;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var parts = value.Trim().Split('+');
            var modifiers = PlayerControlModifiers.None;
            for (int i = 0; i < parts.Length - 1; i++)
            {
                PlayerControlModifiers modifier;
                switch (parts[i].Trim().ToLowerInvariant())
                {
                    case "ctrl":
                    case "control": modifier = PlayerControlModifiers.Ctrl; break;
                    case "shift": modifier = PlayerControlModifiers.Shift; break;
                    case "alt": modifier = PlayerControlModifiers.Alt; break;
                    default: return false;
                }

                if ((modifiers & modifier) != 0)
                    return false;
                modifiers |= modifier;
            }

            string path = parts[parts.Length - 1].Trim();
            foreach (var devicePath in DevicePaths)
                if (string.Equals(path, devicePath, StringComparison.OrdinalIgnoreCase))
                { binding = new PlayerControlBinding(devicePath, modifiers); return true; }
            if (!path.StartsWith(KeyboardPrefix, StringComparison.OrdinalIgnoreCase))
                return false;

            string keyName = path.Substring(KeyboardPrefix.Length);
            if (keyName.Length == 1 && char.IsDigit(keyName[0]))
                keyName = "Digit" + keyName;
            else if (keyName.Length == 0 || !char.IsLetter(keyName[0]))
                return false;

            for (int i = 0; i < keyName.Length; i++)
            {
                if (!char.IsLetterOrDigit(keyName[i]))
                    return false;
            }

            if (!Enum.TryParse(keyName, true, out Key key)
                || !Enum.IsDefined(typeof(Key), key)
                || key == Key.None
                || IsModifierKey(key))
                return false;

            binding = new PlayerControlBinding(key, modifiers);
            return true;
        }

        public static bool Conflicts(string first, string second)
        {
            if (!TryParse(first, out var a) || !TryParse(second, out var b) || a.Modifiers != b.Modifiers) return false;
            string Physical(string path) => path.Replace("<Touchpad>/spaceDrag", "<Keyboard>/space").Replace("<Touchpad>/scroll/", "<Mouse>/scroll/");
            return string.Equals(Physical(a.ControlPath), Physical(b.ControlPath), StringComparison.OrdinalIgnoreCase);
        }

        public static string CreatePath(string keyNameOrControlPath, PlayerControlModifiers modifiers)
        {
            if (string.IsNullOrWhiteSpace(keyNameOrControlPath) || (modifiers & ~AllModifiers) != 0)
                return string.Empty;

            string path = keyNameOrControlPath.Trim();
            if (!path.StartsWith("<", StringComparison.Ordinal))
                path = KeyboardPrefix + path;
            if (!TryParse(path, out var binding) || binding.Modifiers != PlayerControlModifiers.None)
                return string.Empty;

            return binding.Key == Key.None ? new PlayerControlBinding(binding.ControlPath, modifiers).CanonicalPath
                : new PlayerControlBinding(binding.Key, modifiers).CanonicalPath;
        }

        public static bool IsModifierKey(Key key)
            => key == Key.LeftCtrl || key == Key.RightCtrl
               || key == Key.LeftShift || key == Key.RightShift
               || key == Key.LeftAlt || key == Key.RightAlt
               || key == Key.LeftMeta || key == Key.RightMeta;

        public static PlayerControlModifiers ReadModifiers(Keyboard keyboard)
        {
            var modifiers = PlayerControlModifiers.None;
            if (keyboard == null)
                return modifiers;
            if (keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed)
                modifiers |= PlayerControlModifiers.Ctrl;
            if (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed)
                modifiers |= PlayerControlModifiers.Shift;
            if (keyboard.leftAltKey.isPressed || keyboard.rightAltKey.isPressed)
                modifiers |= PlayerControlModifiers.Alt;
            return modifiers;
        }

        public bool IsPressed(Keyboard keyboard)
            => Key != Key.None && keyboard != null
               && !keyboard.leftMetaKey.isPressed && !keyboard.rightMetaKey.isPressed
               && ReadModifiers(keyboard) == Modifiers && keyboard[Key].isPressed;

        private static string ModifierPrefix(PlayerControlModifiers modifiers, string separator)
            => ((modifiers & PlayerControlModifiers.Ctrl) != 0 ? "Ctrl" + separator : string.Empty)
               + ((modifiers & PlayerControlModifiers.Shift) != 0 ? "Shift" + separator : string.Empty)
               + ((modifiers & PlayerControlModifiers.Alt) != 0 ? "Alt" + separator : string.Empty);

        private static string KeyLabel(Key key, string controlName)
        {
            switch (key)
            {
                case Key.Backquote: return "`";
                case Key.Minus: return "−";
                case Key.Equals: return "=";
                case Key.LeftBracket: return "[";
                case Key.RightBracket: return "]";
                case Key.Backslash: return "\\";
                case Key.Semicolon: return ";";
                case Key.Quote: return "'";
                case Key.Comma: return ",";
                case Key.Period: return ".";
                case Key.Slash: return "/";
                case Key.UpArrow: return "↑";
                case Key.DownArrow: return "↓";
                case Key.LeftArrow: return "←";
                case Key.RightArrow: return "→";
                case Key.PageUp: return "Page Up";
                case Key.PageDown: return "Page Down";
                case Key.CapsLock: return "Caps Lock";
                case Key.NumLock: return "Num Lock";
                case Key.PrintScreen: return "Print Screen";
                case Key.ScrollLock: return "Scroll Lock";
                case Key.ContextMenu: return "Menu";
                default:
                    if (controlName.StartsWith("numpad", StringComparison.Ordinal))
                        return "Num " + controlName.Substring(6);
                    return controlName.Length == 1 || key >= Key.F1 && key <= Key.F12
                        ? controlName.ToUpperInvariant()
                        : key.ToString();
            }
        }
    }
}
