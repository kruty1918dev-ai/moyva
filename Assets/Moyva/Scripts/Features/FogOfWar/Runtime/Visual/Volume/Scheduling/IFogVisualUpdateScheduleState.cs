using Kruty1918.Moyva.FogOfWar.API;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal interface IFogVisualUpdateScheduleState
    {
        FogVolumeUpdateMode CurrentUpdateMode { get; }

        float CurrentIntervalSeconds { get; }
    }
}
