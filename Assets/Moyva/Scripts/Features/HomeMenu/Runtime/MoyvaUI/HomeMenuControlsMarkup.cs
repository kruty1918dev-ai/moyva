using System;
using System.Text;
using Kruty1918.Moyva.Shared.Controls;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal static class HomeMenuControlsMarkup
    {
        private static readonly string[] Rows =
        {
            "escape:ESC:1.5 f1:F1 f2:F2 f3:F3 f4:F4 f5:F5 f6:F6 f7:F7 f8:F8 f9:F9 f10:F10 f11:F11 f12:F12 delete:DEL:1.5",
            "backquote:~ digit1:1 digit2:2 digit3:3 digit4:4 digit5:5 digit6:6 digit7:7 digit8:8 digit9:9 digit0:0 minus:- equals:= backspace:BACK:2",
            "tab:TAB:1.5 q:Q w:W e:E r:R t:T y:Y u:U i:I o:O p:P leftBracket:[ rightBracket:] backslash:BACKSLASH:1.5",
            "capsLock:CAPS:1.75 a:A s:S d:D f:F g:G h:H j:J k:K l:L semicolon:; quote:' enter:ENTER:2.25",
            "leftShift:SHIFT:2.25 z:Z x:X c:C v:V b:B n:N m:M comma:, period:. slash:/ rightShift:SHIFT:1.75 upArrow:UP",
            "leftCtrl:CTRL:1.5 leftAlt:ALT:1.5 space:SPACE:6 rightAlt:ALT:1.5 rightCtrl:CTRL:1.5 leftArrow:LEFT downArrow:DOWN rightArrow:RIGHT"
        };

        public static void Append(StringBuilder sb, HomeMenuMoyvaUiViewController view)
        {
            var editor = view.Controls;
            sb.Append("<view className=\"controls-intro\"><view><text className=\"section-label\">MAKE EVERY KEY YOURS</text>")
                .Append("<text className=\"controls-title\">Your keyboard. Your playstyle.</text><text className=\"section-copy\">Explore a key, choose an action and build your own camera shortcuts.</text></view>")
                .Append("<text className=\"controls-badge\">KEYBOARD + MOUSE</text></view><view className=\"controls-workspace\"><view className=\"keyboard-panel\">")
                .Append("<view className=\"keyboard-toolbar\"><text className=\"section-label\">KEYBOARD MAP</text><text className=\"keyboard-layer\">")
                .Append(E(editor.Modifiers == PlayerControlModifiers.None ? "BASE LAYER" : editor.Modifiers.ToString().Replace(", ", " + ").ToUpperInvariant()))
                .Append("</text></view><view className=\"keyboard-board\">");
            foreach (var row in Rows)
            {
                sb.Append("<view className=\"keyboard-row\">");
                foreach (var key in row.Split(' ')) AppendKey(sb, editor, key);
                sb.Append("</view>");
            }
            sb.Append("</view><view className=\"keyboard-legend\">");
            Legend(sb, "movement", "Movement"); Legend(sb, "orbit", "Orbit"); Legend(sb, "zoom", "Zoom"); Legend(sb, "reserved", "Gameplay");
            sb.Append("</view><text className=\"keyboard-hint\">Click Ctrl, Shift or Alt to explore a shortcut layer. Record any key, including numpad keys.</text></view>");
            AppendInspector(sb, view);
            sb.Append("</view><view className=\"controls-section-heading\"><text className=\"section-label\">CAMERA BINDINGS</text><text className=\"control-help\">Select an action to edit its shortcut</text></view><view className=\"binding-cards\">");
            foreach (PlayerControlAction action in Enum.GetValues(typeof(PlayerControlAction)))
            {
                view.ControlBindings.TryGetValue(action, out var path);
                var label = PlayerControlBinding.TryParse(path, out var binding) ? binding.DisplayName : "UNBOUND";
                sb.Append("<button className=\"binding-card ").Append(HomeMenuControlsEditor.ActionGroup(action));
                if (action == editor.SelectedAction) sb.Append(" active");
                sb.Append("\" onClick=\"Globals.moyvaMenu.EditControlAction(").Append((int)action).Append(")\"><text className=\"binding-card-title\">")
                    .Append(E(HomeMenuControlsEditor.ActionLabel(action))).Append("</text><text className=\"binding-card-key\">").Append(E(label)).Append("</text></button>");
            }
            sb.Append("</view><view className=\"controls-section-heading\"><text className=\"section-label\">CAMERA FEEL</text><text className=\"control-help\">Fine-tune movement and sensitivity</text></view>");
        }

        private static void AppendKey(StringBuilder sb, HomeMenuControlsEditor editor, string definition)
        {
            var parts = definition.Split(':');
            var key = parts[0]; var label = parts[1]; var width = parts.Length > 2 ? parts[2] : "1";
            var modifier = key.EndsWith("Ctrl", StringComparison.Ordinal) ? PlayerControlModifiers.Ctrl :
                key.EndsWith("Shift", StringComparison.Ordinal) ? PlayerControlModifiers.Shift :
                key.EndsWith("Alt", StringComparison.Ordinal) ? PlayerControlModifiers.Alt : PlayerControlModifiers.None;
            var action = editor.BoundAction(key, editor.Modifiers);
            var reserved = editor.ReservedAction(key, editor.Modifiers);
            var hint = modifier != PlayerControlModifiers.None ? "Toggle " + label + " layer" : action.HasValue ? HomeMenuControlsEditor.ActionLabel(action.Value) : reserved.Length > 0 ? reserved : "No binding on this layer";
            sb.Append("<button className=\"keyboard-key");
            if (modifier != PlayerControlModifiers.None && editor.Modifiers.HasFlag(modifier)) sb.Append(" modifier-active");
            if (modifier == PlayerControlModifiers.None && PlayerControlBinding.CreatePath(key, editor.Modifiers) == editor.DraftPath) sb.Append(" selected");
            if (action.HasValue) sb.Append(' ').Append(HomeMenuControlsEditor.ActionGroup(action.Value));
            else if (reserved.Length > 0) sb.Append(" reserved");
            sb.Append("\" style=\"flex-grow:").Append(width).Append(";flex-basis:0;\" data-tooltip=\"").Append(E(hint)).Append("\" onClick=\"Globals.moyvaMenu.");
            if (modifier != PlayerControlModifiers.None) sb.Append("ToggleControlModifier(").Append((int)modifier).Append(')');
            else sb.Append("SelectControlKey('").Append(key).Append("')");
            sb.Append("\"><text className=\"keycap-label\">").Append(E(label)).Append("</text><view className=\"keycap-marker\"></view></button>");
        }

        private static void AppendInspector(StringBuilder sb, HomeMenuMoyvaUiViewController view)
        {
            var editor = view.Controls;
            var bound = editor.BoundAction(editor.SelectedKey, editor.Modifiers);
            var reserved = editor.ReservedAction(editor.SelectedKey, editor.Modifiers);
            sb.Append("<view className=\"key-inspector\"><text className=\"section-label\">SELECTED SHORTCUT</text><text className=\"selected-key-display\">")
                .Append(E(editor.DraftLabel)).Append("</text><text className=\"selected-key-assignment\">")
                .Append(E(bound.HasValue ? HomeMenuControlsEditor.ActionLabel(bound.Value) : reserved.Length > 0 ? reserved + " (gameplay)" : "Available for a camera action"))
                .Append("</text><view className=\"modifier-switches\">");
            Modifier(sb, editor, "CTRL", PlayerControlModifiers.Ctrl); Modifier(sb, editor, "SHIFT", PlayerControlModifiers.Shift); Modifier(sb, editor, "ALT", PlayerControlModifiers.Alt);
            sb.Append("</view><text className=\"inspector-label\">ASSIGN TO</text><select className=\"menu-select inspector-select\" options=\"");
            foreach (PlayerControlAction action in Enum.GetValues(typeof(PlayerControlAction)))
            {
                if ((int)action > 0) sb.Append('|');
                sb.Append(HomeMenuControlsEditor.ActionLabel(action));
            }
            sb.Append("\" value=\"").Append((int)editor.SelectedAction).Append("\" onChange=\"Globals.moyvaMenu.SelectControlAction(event)\"></select>");
            ActionButton(sb, editor.IsCapturing ? "CANCEL RECORDING" : "RECORD SHORTCUT", editor.IsCapturing ? "CancelControlCapture()" : "RecordControlShortcut()", "record-shortcut", view.SettingsInteractable);
            ActionButton(sb, "APPLY SHORTCUT", "ApplyControlShortcut()", "apply-shortcut", editor.CanApply);
            var conflict = editor.Conflict;
            sb.Append("<text className=\"binding-notice").Append(editor.IsCapturing ? " listening" : conflict.Length > 0 ? " conflict" : "")
                .Append("\">").Append(E(editor.IsCapturing || conflict.Length == 0 ? editor.Notice : conflict)).Append("</text></view>");
        }

        private static void Modifier(StringBuilder sb, HomeMenuControlsEditor editor, string label, PlayerControlModifiers modifier)
        {
            sb.Append("<button className=\"modifier-switch").Append(editor.Modifiers.HasFlag(modifier) ? " active" : "")
                .Append("\" onClick=\"Globals.moyvaMenu.ToggleControlModifier(").Append((int)modifier).Append(")\"><text>").Append(label).Append("</text></button>");
        }

        private static void Legend(StringBuilder sb, string group, string label) => sb.Append("<view className=\"legend-item ").Append(group)
            .Append("\"><view className=\"legend-dot\"></view><text>").Append(label).Append("</text></view>");

        private static void ActionButton(StringBuilder sb, string label, string call, string css, bool enabled)
        {
            sb.Append("<button className=\"shortcut-button ").Append(css).Append("\" onClick=\"Globals.moyvaMenu.").Append(call).Append('"');
            if (!enabled) sb.Append(" disabled=\"true\"");
            sb.Append("><text>").Append(label).Append("</text></button>");
        }

        private static string E(string value) => System.Security.SecurityElement.Escape(value ?? string.Empty);
    }
}
