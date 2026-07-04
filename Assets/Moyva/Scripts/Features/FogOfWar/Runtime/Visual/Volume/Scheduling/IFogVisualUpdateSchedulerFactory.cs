using System;
using Kruty1918.Moyva.FogOfWar.API;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal interface IFogVisualUpdateSchedulerFactory
    {
        FogVisualUpdateScheduler Create(
            Func<FogVolumeUpdateMode> resolveUpdateMode,
            Func<float> resolveIntervalSeconds);
    }
}
