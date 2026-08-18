namespace Kruty1918.Moyva.Bootstrap.Editor
{
    /// <summary>
    /// Compatibility entry point for the P24B authoring pass.
    /// The final recruitment authoring owns the current compact hierarchy.
    /// </summary>
    public static class RecruitmentHudP24BAuthoring
    {
        public static string ApplyAndSave()
            => RecruitmentPanelFinalAuthoring.ApplyAndSave();

        public static string ValidateCurrent()
            => RecruitmentPanelFinalAuthoring.ValidateCurrent();
    }
}
