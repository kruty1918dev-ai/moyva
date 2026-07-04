namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal interface IFogVolumePendingWorkMaintenance
    {
        void SetMapSize(int width, int height);

        void Complete();

        void ClearCellChanges();
    }
}
