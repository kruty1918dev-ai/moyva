namespace Kruty1918.Moyva.Shared.Controls
{
    public static class ControlPromptText
    {
        public static string Camera(ControlProfile profile, PlayerControlProfile options)
        {
            if (options == null) return string.Empty;
            var bindings = options.ToBindings();
            string Label(PlayerControlAction action)
                => bindings.TryGetValue(action, out var path) ? Compact(path) : "—";
            if (profile == ControlProfile.Gamepad)
                return "Move " + Label(PlayerControlAction.MoveForward) + "  ·  Orbit " + Label(PlayerControlAction.RotateRight)
                    + "  ·  Zoom " + Label(PlayerControlAction.ZoomIn) + " / " + Label(PlayerControlAction.ZoomOut)
                    + "  ·  Select " + Label(PlayerControlAction.PrimarySelect);
            return "Pan " + Compact(options.PanBinding) + "  ·  Orbit " + Compact(options.OrbitBinding)
                + "  ·  Zoom " + (profile == ControlProfile.TouchPhone ? Label(PlayerControlAction.ZoomIn) + " / " + Label(PlayerControlAction.ZoomOut) : "Scroll / " + Label(PlayerControlAction.ZoomIn));
        }

        private static string Compact(string path)
        {
            if (!PlayerControlBinding.TryParse(path, out var binding)) return "—";
            if (binding.Key != UnityEngine.InputSystem.Key.None) return binding.DisplayName;
            return binding.CanonicalPath.Replace("<Mouse>/middleButton", "MMB").Replace("<Mouse>/", "Mouse ")
                .Replace("<Touchpad>/spaceDrag", "Space + drag").Replace("<Touchpad>/", "Touchpad ")
                .Replace("<Gamepad>/leftStick/up", "LS ↑").Replace("<Gamepad>/rightStick/right", "RS →")
                .Replace("<Gamepad>/buttonSouth", "A / ×").Replace("<Gamepad>/buttonEast", "B / ○")
                .Replace("<Gamepad>/rightTrigger", "RT").Replace("<Gamepad>/leftTrigger", "LT").Replace("<Gamepad>/", "Pad ")
                .Replace("<Touch>/pinchOut", "Spread").Replace("<Touch>/pinchIn", "Pinch")
                .Replace("<Touch>/drag", "Drag").Replace("<Touch>/twist", "Twist");
        }
    }
}
