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
            sb.Append("<view className=\"controls-intro\"><view><text className=\"section-label\">CONTROLS</text><text className=\"controls-title\">Make yourself comfortable.</text></view><text className=\"controls-badge\">")
                .Append(E(HomeMenuControlsEditor.ProfileLabel(editor.ActiveProfile))).Append("</text></view>");
            sb.Append("<view className=\"device-toolbar\"><text>Input profile</text>");
            sb.Append("<select className=\"menu-select device-profile-select\" options=\"Auto|Keyboard + Mouse|Keyboard + Touchpad|Gamepad|Touch / Phone\" value=\"")
                .Append((int)editor.ProfileSelection).Append("\" onChange=\"Globals.moyvaMenu.ControlProfileSelection(event)\"></select>");
            string[] tabs = { "Keyboard", "Mouse", "Touchpad", "Gamepad", "Phone" };
            for (int i = 0; i < tabs.Length; i++) Choice(sb, tabs[i], "SelectControlDevice(" + i + ")", editor.DeviceTab == i);
            Choice(sb, editor.IsTesting ? "Exit test" : "Test Controls", "TestControls()", editor.IsTesting);
            Choice(sb, "Reset this profile", "ResetControlProfile()", false);
            sb.Append("</view>");
            if (editor.DeviceTab < 3)
            {
                sb.Append("<view className=\"device-toolbar\"><text>Generic pointer</text>");
                Choice(sb, "Auto", "SelectControlPointerMode(0)", editor.PointerMode == PointerInterpretation.Auto);
                Choice(sb, "Use as Mouse", "SelectControlPointerMode(1)", editor.PointerMode == PointerInterpretation.Mouse);
                Choice(sb, "Use as Touchpad", "SelectControlPointerMode(2)", editor.PointerMode == PointerInterpretation.Touchpad);
                sb.Append("</view>");
            }
            sb.Append("<text className=\"keyboard-hint\">").Append(E(editor.IsTesting
                ? editor.TestReadout
                : "Select a control to see its action. Choose an action, then Apply to rebind.")).Append("</text>");
            sb.Append("<view className=\"controls-workspace adaptive-workspace\"><view className=\"keyboard-panel\">");
            if (editor.DeviceTab < 2) sb.Append("<view className=\"desktop-device-row\">");
            if (editor.DeviceTab < 3)
            {
                sb.Append("<view className=\"keyboard-board\">");
                foreach (var row in Rows)
                {
                    sb.Append("<view className=\"keyboard-row\">");
                    foreach (var key in row.Split(' ')) AppendKey(sb, editor, key);
                    sb.Append("</view>");
                }
                sb.Append("</view>");
            }
            AppendDevice(sb, editor);
            if (editor.DeviceTab < 2) sb.Append("</view>");
            AppendDeviceOptions(sb, editor);
            sb.Append("</view>");
            AppendInspector(sb, view);
            sb.Append("</view><view className=\"controls-section-heading\"><text className=\"section-label\">CAMERA BINDINGS</text><text className=\"control-help\">Select an action to edit its shortcut</text></view><view className=\"binding-cards\">");
            foreach (PlayerControlAction action in Enum.GetValues(typeof(PlayerControlAction)))
            {
                editor.Bindings.TryGetValue(action, out var path);
                var label = PlayerControlBinding.TryParse(path, out var binding) ? binding.DisplayName : "UNBOUND";
                sb.Append("<button className=\"binding-card ").Append(HomeMenuControlsEditor.ActionGroup(action));
                if (action == editor.SelectedAction) sb.Append(" active");
                sb.Append("\" onClick=\"Globals.moyvaMenu.EditControlAction(").Append((int)action).Append(")\"><text className=\"binding-card-title\">")
                    .Append(E(HomeMenuControlsEditor.ActionLabel(action))).Append("</text><text className=\"binding-card-key\">").Append(E(label)).Append("</text></button>");
            }
            sb.Append("</view><view className=\"controls-section-heading\"><text className=\"section-label\">CAMERA FEEL</text><text className=\"control-help\">Fine-tune movement and sensitivity</text></view>");
        }

        private static void Choice(StringBuilder sb, string label, string call, bool active)
            => ActionButton(sb, E(label), call, active ? "device-choice active" : "device-choice", true);

        private static void AppendDevice(StringBuilder sb, HomeMenuControlsEditor editor)
        {
            if (editor.DeviceTab < 2)
            {
                sb.Append("<view className=\"device-diagram mouse-diagram\"><text>MOUSE</text><view className=\"device-toolbar\">");
                DeviceControl(sb, editor, "<Mouse>/leftButton", "L");
                DeviceControl(sb, editor, "<Mouse>/middleButton", "M");
                DeviceControl(sb, editor, "<Mouse>/rightButton", "R");
                sb.Append("</view><view className=\"device-toolbar\">");
                DeviceControl(sb, editor, "<Mouse>/scroll/up", "↑"); DeviceControl(sb, editor, "<Mouse>/scroll/down", "↓");
                sb.Append("</view><view className=\"device-toolbar\">");
                DeviceControl(sb, editor, "<Mouse>/backButton", "Back"); DeviceControl(sb, editor, "<Mouse>/forwardButton", "Next");
                sb.Append("</view></view>");
            }
            else if (editor.DeviceTab == 2)
            {
                sb.Append("<view className=\"device-diagram touchpad-diagram\"><text>TOUCHPAD · Pointer / tap to select</text><view className=\"device-toolbar\">");
                DeviceControl(sb, editor, "<Touchpad>/spaceDrag", "Space + drag ↔");
                DeviceControl(sb, editor, "<Touchpad>/scroll/up", "Two fingers ↑");
                DeviceControl(sb, editor, "<Touchpad>/scroll/down", "Two fingers ↓");
                sb.Append("</view><text>Alt + Space + drag to orbit · WASD fallback</text></view>");
            }
            else if (editor.DeviceTab == 3)
            {
                sb.Append("<view className=\"device-diagram gamepad-diagram\"><view className=\"device-toolbar\">");
                DeviceControl(sb, editor, "<Gamepad>/leftTrigger", "LT"); DeviceControl(sb, editor, "<Gamepad>/leftShoulder", "LB");
                DeviceControl(sb, editor, "<Gamepad>/select", "View"); DeviceControl(sb, editor, "<Gamepad>/start", "Menu");
                DeviceControl(sb, editor, "<Gamepad>/rightShoulder", "RB"); DeviceControl(sb, editor, "<Gamepad>/rightTrigger", "RT");
                sb.Append("</view><view className=\"device-toolbar\">");
                Stick(sb, editor, "leftStick", "LEFT STICK");
                sb.Append("<view className=\"controller-face\">");
                DeviceControl(sb, editor, "<Gamepad>/buttonNorth", "Y / △");
                sb.Append("<view className=\"device-toolbar\">");
                DeviceControl(sb, editor, "<Gamepad>/buttonWest", "X / □"); DeviceControl(sb, editor, "<Gamepad>/buttonEast", "B / ○");
                sb.Append("</view>"); DeviceControl(sb, editor, "<Gamepad>/buttonSouth", "A / ×"); sb.Append("</view></view><view className=\"device-toolbar\">");
                Stick(sb, editor, "dpad", "D-PAD"); Stick(sb, editor, "rightStick", "RIGHT STICK");
                sb.Append("</view></view>");
            }
            else
            {
                sb.Append("<view className=\"device-diagram phone-diagram\"><view className=\"phone-speaker\"></view><text>PHONE · Tap to select</text>");
                DeviceControl(sb, editor, "<Touch>/tap", "Tap");
                DeviceControl(sb, editor, "<Touch>/longPress", "Hold");
                DeviceControl(sb, editor, "<Touch>/drag", "One finger · Drag ↔");
                DeviceControl(sb, editor, "<Touch>/pinchOut", "Two fingers · Spread ↗ ↙");
                DeviceControl(sb, editor, "<Touch>/pinchIn", "Two fingers · Pinch ↘ ↖");
                DeviceControl(sb, editor, "<Touch>/twist", "Two fingers · Twist ⟳");
                sb.Append("<text>Gestures start outside interface controls</text></view>");
            }
        }

        private static void AppendDeviceOptions(StringBuilder sb, HomeMenuControlsEditor editor)
        {
            var options = editor.Options;
            if (options == null) return;
            sb.Append("<view className=\"device-toolbar\">");
            if (editor.DeviceTab != 3)
            {
            Choice(sb, "Pan gesture", "SelectControlGesture(1)", editor.GestureSlot == 1);
            Choice(sb, "Orbit gesture", "SelectControlGesture(2)", editor.GestureSlot == 2);
            }
            if (editor.DeviceTab < 3) Choice(sb, "Edge pan: " + (options.EdgePan ? "On" : "Off"), "ControlOption(2,0)", options.EdgePan);
            if (editor.DeviceTab == 2 || editor.DeviceTab == 4) Choice(sb, "Gestures: " + (options.Gestures ? "On" : "Off"), "ControlOption(3,0)", options.Gestures);
            sb.Append("</view><view className=\"device-toolbar\"><text>Sensitivity ").Append(options.Sensitivity.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)).Append("</text>");
            Choice(sb, "−", "ControlOption(0," + (options.Sensitivity - 0.25f).ToString(System.Globalization.CultureInfo.InvariantCulture) + ")", false);
            Choice(sb, "+", "ControlOption(0," + (options.Sensitivity + 0.25f).ToString(System.Globalization.CultureInfo.InvariantCulture) + ")", false);
            if (editor.DeviceTab == 3)
            {
                sb.Append("<text>Deadzone ").Append(options.Deadzone.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)).Append("</text>");
                Choice(sb, "−", "ControlOption(1," + (options.Deadzone - 0.05f).ToString(System.Globalization.CultureInfo.InvariantCulture) + ")", false);
                Choice(sb, "+", "ControlOption(1," + (options.Deadzone + 0.05f).ToString(System.Globalization.CultureInfo.InvariantCulture) + ")", false);
            }
            sb.Append("</view>");
        }

        private static void Stick(StringBuilder sb, HomeMenuControlsEditor editor, string control, string label)
        {
            sb.Append("<view className=\"controller-stick\"><text>").Append(label).Append("</text>");
            DeviceControl(sb, editor, "<Gamepad>/" + control + "/up", "↑");
            sb.Append("<view className=\"device-toolbar\">");
            DeviceControl(sb, editor, "<Gamepad>/" + control + "/left", "←");
            if (control != "dpad") DeviceControl(sb, editor, "<Gamepad>/" + control + "Press", "●");
            DeviceControl(sb, editor, "<Gamepad>/" + control + "/right", "→");
            sb.Append("</view>"); DeviceControl(sb, editor, "<Gamepad>/" + control + "/down", "↓");
            sb.Append("</view>");
        }

        private static void DeviceControl(StringBuilder sb, HomeMenuControlsEditor editor, string path, string label)
        {
            string css = "device-control";
            if (editor.IsLive(path)) css += " control-live";
            if (editor.SelectedKey == path) css += editor.Conflict.Length > 0 ? " control-conflict" : " selected";
            ActionButton(sb, E(label), "SelectControlKey('" + E(path) + "')", css, true);
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
            if (editor.IsLive(key)) sb.Append(" control-live");
            if (PlayerControlBinding.CreatePath(key, editor.Modifiers) == editor.DraftPath && editor.Conflict.Length > 0) sb.Append(" control-conflict");
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
