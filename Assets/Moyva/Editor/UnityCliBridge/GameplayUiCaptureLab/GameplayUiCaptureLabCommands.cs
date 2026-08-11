#if MOYVA_LEGACY_SCRIPTABLEOBJECT_EDITOR
using Unity.Pipeline.Commands;

namespace Kruty1918.Moyva.Editor.UnityCliBridge.GameplayUiCaptureLab
{
    public static class GameplayUiCaptureLabCommands
    {
        [CliCommand("moyva-gameplay-ui-capture-window", "Capture actual visible GameView/SceneView editor-window pixels; unlike camera screenshots this can include Screen Space Overlay UI.")]
        public static string CaptureWindow(
            [CliArg("path", "Absolute PNG path under ~/.local/share/moyva-cli/reports/gameplay-ui-capture", Required = true)] string path,
            [CliArg("view", "game or scene", Required = true)] string view,
            [CliArg("preset", "SceneView: current/top/iso/low/close/restore; ignored for GameView")] string preset = "current",
            [CliArg("delay", "Editor update frames before capture")] int delay = 8)
            => GameplayUiCaptureLabService.CaptureWindow(path, view, preset, delay);

        [CliCommand("moyva-gameplay-ui-capture-runtime", "Schedule Unity ScreenCapture.CaptureScreenshot while in Play Mode; intended to include final game-frame UI.")]
        public static string CaptureRuntime(
            [CliArg("path", "Absolute PNG path under ~/.local/share/moyva-cli/reports/gameplay-ui-capture", Required = true)] string path,
            [CliArg("supersize", "1..4")] int superSize = 1)
            => GameplayUiCaptureLabService.CaptureRuntimeScreenshot(path, superSize);

        [CliCommand("moyva-gameplay-ui-capture-state", "Dump live GameView, Canvas and RectTransform visual geometry/state.")]
        public static string State()
            => GameplayUiCaptureLabService.VisualState();

        [CliCommand("moyva-gameplay-ui-capture-select-building", "Request a building selection during Play Mode for screenshot coverage.")]
        public static string SelectBuilding(
            [CliArg("id", "Building id such as castle-01", Required = true)] string id)
            => GameplayUiCaptureLabService.RuntimeSelectBuilding(id);

        [CliCommand("moyva-gameplay-ui-capture-restore-sceneview", "Restore SceneView camera after capture presets.")]
        public static string RestoreSceneView()
            => GameplayUiCaptureLabService.RestoreSceneView();
    }
}

#endif
