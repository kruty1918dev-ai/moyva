using Sirenix.OdinInspector;

namespace Kruty1918.Moyva.FogOfWar.API
{
    public partial class FogOfWarSettings
    {
        [TitleGroup("Visual Presentation")]
        public FogVisualPresentationMode PresentationMode =
            FogVisualPresentationMode.ScreenSpace;

        [TitleGroup("Screen-Space Fog")]
        [InlineProperty]
        [HideLabel]
        public FogScreenSpaceSettings ScreenSpace =
            new FogScreenSpaceSettings();
    }
}