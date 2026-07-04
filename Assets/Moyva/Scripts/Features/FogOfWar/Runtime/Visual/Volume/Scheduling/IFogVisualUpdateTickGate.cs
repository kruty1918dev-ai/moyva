namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal interface IFogVisualUpdateTickGate
    {
        bool ShouldExecute(FogVolumePendingWorkSnapshot work, out string waitingMessage);
    }
}
