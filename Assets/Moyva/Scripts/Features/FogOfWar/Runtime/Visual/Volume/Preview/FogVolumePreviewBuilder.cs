using Kruty1918.Moyva.FogOfWar.API;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{

    /// <summary>
    /// Реалізує editor preview build через тимчасовий runtime updater і спеціальний preview fog service.
    /// </summary>
    internal sealed partial class FogVolumePreviewBuilder : IFogVolumePreviewBuilder
    {
        /// <summary>
        /// Перебудовує preview fog volume без зміни gameplay fog state.
        /// </summary>
        /// <param name="host">Host-компонент preview path.</param>
        /// <param name="context">Поточний world context для preview.</param>
        public void BuildPreview(IFogVolumePreviewHost host, FogWorldVisualContext context)
        {
            if (host?.Settings == null)
                return;

            var updater = new FogOfWarVolumeUpdater(host.Settings);
            host.AttachPreviewUpdater(updater);
            updater.Initialize(context.Width, context.Height, context);
            updater.RebuildFullVisual(new PreviewFogService(context.Width, context.Height));
            updater.Tick();
        }
    }
}
