using Kruty1918.Moyva.FogOfWar.API;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Ізолює editor preview build від основного MonoBehaviour host-а.
    /// Preview path не повинен змішуватись із gameplay runtime fog state.
    /// </summary>
    internal interface IFogVolumePreviewBuilder
    {
        /// <summary>
        /// Будує editor preview fog volume для вказаного host-а.
        /// </summary>
        /// <param name="host">Host-компонент з settings та точкою підключення updater-а.</param>
        /// <param name="context">World context для preview build.</param>
        void BuildPreview(IFogVolumePreviewHost host, FogWorldVisualContext context);
    }
}
