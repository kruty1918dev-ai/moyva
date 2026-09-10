using System;
using UnityEngine.InputSystem;

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

    /// <summary>A keyboard key with an exact set of modifiers; persisted as Ctrl+Shift+Alt+&lt;Keyboard&gt;/key.</summary>
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

        public static string CreatePath(string keyNameOrControlPath, PlayerControlModifiers modifiers)
        {
            if (string.IsNullOrWhiteSpace(keyNameOrControlPath) || (modifiers & ~AllModifiers) != 0)
                return string.Empty;

            string path = keyNameOrControlPath.Trim();
            if (!path.StartsWith(KeyboardPrefix, StringComparison.OrdinalIgnoreCase))
                path = KeyboardPrefix + path;
            if (!TryParse(path, out var binding) || binding.Modifiers != PlayerControlModifiers.None)
                return string.Empty;

            return new PlayerControlBinding(binding.Key, modifiers).CanonicalPath;
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
