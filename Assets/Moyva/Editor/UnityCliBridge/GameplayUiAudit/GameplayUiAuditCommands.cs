#if MOYVA_LEGACY_SCRIPTABLEOBJECT_EDITOR
using Unity.Pipeline.Commands;

namespace Kruty1918.Moyva.Editor.UnityCliBridge.GameplayUiAudit
{
    public static class GameplayUiAuditCommands
    {
        [CliCommand(
            "moyva-gameplay-ui-prepare",
            "Safely ensure Assets/Moyva/Scenes/Gamplay_Scene.unity is loaded and active. Refuses to replace dirty scenes.")]
        public static string Prepare()
            => GameplayUiAuditService.PrepareTargetScene();

        [CliCommand(
            "moyva-gameplay-ui-audit",
            "Write a comprehensive live Gameplay UI audit JSON and request Editor screenshots.")]
        public static string Audit(
            [CliArg(
                "output-dir",
                "External report directory under ~/.local/share/moyva-cli/reports/gameplay-ui/",
                Required = true)]
            string outputDir)
            => GameplayUiAuditService.RunAudit(outputDir);

        [CliCommand(
            "moyva-gameplay-ui-capture",
            "Capture current Gameplay SceneView and request a GameView screenshot into an external report directory.")]
        public static string Capture(
            [CliArg(
                "output-dir",
                "External report directory under ~/.local/share/moyva-cli/reports/gameplay-ui/",
                Required = true)]
            string outputDir)
            => GameplayUiAuditService.CaptureOnly(outputDir);

        [CliCommand(
            "moyva-gameplay-ui-audit-status",
            "Return whether the canonical Gameplay scene is currently loaded.")]
        public static string Status()
            => GameplayUiAuditService.Status();
    }
}

#endif
