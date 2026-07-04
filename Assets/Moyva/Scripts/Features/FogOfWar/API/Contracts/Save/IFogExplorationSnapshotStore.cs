namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Відповідає лише за snapshot save/load explored fog state.
    /// </summary>
    public interface IFogExplorationSnapshotStore
    {
        bool[,] GetExploredSnapshot();
        void LoadFromSnapshot(bool[,] explored);
    }
}
