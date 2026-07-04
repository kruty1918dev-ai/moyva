using Sirenix.OdinInspector;

namespace Kruty1918.Moyva.FogOfWar.API
{
    public partial class FogOfWarSettings
    {
        /// <summary>
        /// Конфігурація TWC volume presentation для fog.
        /// </summary>
        [TitleGroup("TWC Volume")]
        [InlineProperty]
        [HideLabel]
        public FogVolumeTileSettings Volume = new FogVolumeTileSettings();
    }
}
