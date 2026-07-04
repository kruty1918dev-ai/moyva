using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.FogOfWar.API;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Мінімальний host contract для валідації fog volume scene setup.
    /// </summary>
    internal interface IFogVolumeValidationHost
    {
        /// <summary>
        /// Fog settings, які перевіряються на валідність.
        /// </summary>
        FogOfWarSettings Settings { get; }

        /// <summary>
        /// TWC manager, який має бути присутній на тому ж GameObject.
        /// </summary>
        TileWorldCreatorManager TileWorldCreatorManager { get; }
    }
}
