using System;
using Kruty1918.Moyva.FogOfWar.API;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Creates scheduler instances for fog volume visual updates.
    /// </summary>
    internal sealed class FogVisualUpdateSchedulerFactory : IFogVisualUpdateSchedulerFactory
    {
        public FogVisualUpdateScheduler Create(
            Func<FogVolumeUpdateMode> resolveUpdateMode,
            Func<float> resolveIntervalSeconds)
            => new FogVisualUpdateScheduler(resolveUpdateMode, resolveIntervalSeconds);
    }
}
