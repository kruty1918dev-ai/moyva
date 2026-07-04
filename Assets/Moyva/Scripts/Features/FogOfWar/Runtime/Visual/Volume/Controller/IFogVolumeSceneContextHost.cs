using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Мінімальний host contract для побудови <see cref="FogWorldVisualContext"/> з scene/world data.
    /// </summary>
    internal interface IFogVolumeSceneContextHost
    {
        /// <summary>
        /// Fog settings, які задають preview fallback values.
        /// </summary>
        FogOfWarSettings Settings { get; }

        /// <summary>
        /// Поточний fog volume manager на об'єкті host-а.
        /// </summary>
        TileWorldCreatorManager TileWorldCreatorManager { get; }

        /// <summary>
        /// Transform host-а, який використовується як fallback origin.
        /// </summary>
        Transform transform { get; }
    }
}
