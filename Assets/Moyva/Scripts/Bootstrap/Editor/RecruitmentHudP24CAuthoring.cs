namespace Kruty1918.Moyva.Bootstrap.Editor
{
    /// <summary>
    /// Compatibility entry point for the P24C authoring pass.
    /// The final recruitment authoring owns the current compact hierarchy.
    /// </summary>
    public static class RecruitmentHudP24CAuthoring
    {
        public static string ApplyAndSave()
            => RecruitmentPanelFinalAuthoring.ApplyAndSave();

        public static string ValidateCurrent()
            => RecruitmentPanelFinalAuthoring.ValidateCurrent();
    }
}
