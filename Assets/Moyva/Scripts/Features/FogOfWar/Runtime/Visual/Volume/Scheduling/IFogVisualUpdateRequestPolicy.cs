namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal interface IFogVisualUpdateRequestPolicy
    {
        bool ShouldExecuteImmediateRequest();

        bool ShouldExecuteFullRebuildRequestImmediately(bool hasBuiltAtLeastOnce, bool worldContextChangedSinceBuild);
    }
}
