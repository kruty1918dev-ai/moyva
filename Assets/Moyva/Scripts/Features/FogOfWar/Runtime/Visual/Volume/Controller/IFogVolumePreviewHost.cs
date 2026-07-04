using Kruty1918.Moyva.FogOfWar.API;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Мінімальний host contract для editor preview build path.
    /// Реалізацію зазвичай надає <see cref="FogOfWarVolumeController"/>.
    /// </summary>
    internal interface IFogVolumePreviewHost
    {
        /// <summary>
        /// Fog settings, з якими будується preview.
        /// </summary>
        FogOfWarSettings Settings { get; }

        /// <summary>
        /// Дає змогу preview builder-у тимчасово прив'язати runtime updater до host-компонента.
        /// </summary>
        /// <param name="updater">Тимчасовий updater для preview path.</param>
        void AttachPreviewUpdater(IFogVolumeRuntimeUpdater updater);
    }
}
