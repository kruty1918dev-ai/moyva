using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal interface IFogClusterMaterialProvider
    {
        bool ShouldRenderState(FogStateType state);
        FogVolumeStateTileSettings ResolveStateSettings(FogStateType state);
        Material ResolveMaterial(FogStateType state);
    }
}
