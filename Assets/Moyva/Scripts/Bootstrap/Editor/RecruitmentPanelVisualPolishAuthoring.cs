namespace Kruty1918.Moyva.Bootstrap.Editor
{
    /// <summary>
    /// Compatibility entry point for older P24UI tooling.
    /// The final authoring owns the current compact hierarchy.
    /// </summary>
    public static class RecruitmentPanelVisualPolishAuthoring
    {
        public static string ApplyAndSave()
            => RecruitmentPanelFinalAuthoring.ApplyAndSave();

        public static string ValidateCurrent()
            => RecruitmentPanelFinalAuthoring.ValidateCurrent();
    }
}
