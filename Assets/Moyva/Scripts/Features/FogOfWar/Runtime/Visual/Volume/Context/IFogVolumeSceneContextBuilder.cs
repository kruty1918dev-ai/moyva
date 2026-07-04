using Kruty1918.Moyva.FogOfWar.API;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Ізолює побудову world visual context від MonoBehaviour host-а.
    /// Сервіс читає scene/world geometry, але не змінює gameplay fog state.
    /// </summary>
    internal interface IFogVolumeSceneContextBuilder
    {
        /// <summary>
        /// Формує visual context для fog preview/runtime build path.
        /// </summary>
        /// <param name="host">Host-компонент, з якого беруться settings та fallback transform.</param>
        /// <returns>Побудований world visual context.</returns>
        FogWorldVisualContext BuildContext(IFogVolumeSceneContextHost host);
    }
}
