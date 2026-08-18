#if MOYVA_LEGACY_SCRIPTABLEOBJECT_EDITOR
using Unity.Pipeline.Commands;

namespace Kruty1918.Moyva.Editor.UnityCliBridge.GameplayUiRedesign
{
    public static class GameplayUiRedesignCommands
    {
        [CliCommand("moyva-gameplay-ui-redesign-plan", "Analyze whether the audited Gameplay UI can receive Pass73 safely.")]
        public static string Plan() => GameplayUiRedesignService.Plan();

        [CliCommand("moyva-gameplay-ui-redesign-apply", "Apply Pass73 Gameplay UI foundation in-memory. Requires --confirm APPLY and does not save.")]
        public static string Apply(
            [CliArg("confirm", "Must be exactly APPLY", Required = true)] string confirm)
            => GameplayUiRedesignService.Apply(confirm);

        [CliCommand("moyva-gameplay-ui-redesign-validate", "Validate Pass73 hierarchy, bindings, touch target and missing-script constraints.")]
        public static string Validate() => GameplayUiRedesignService.Validate();

        [CliCommand("moyva-gameplay-ui-redesign-preview", "Toggle edit-mode UI shell for screenshot verification: normal|construction|restore.")]
        public static string Preview(
            [CliArg("mode", "normal, construction, or restore", Required = true)] string mode)
            => GameplayUiRedesignService.PreviewMode(mode);
    }
}

#endif
