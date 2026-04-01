namespace Kruty1918.Moyva.FogOfWar.API
{
    public interface IFogSaveDataProvider
    {
        bool[,] LoadExploredData();
        void SaveExploredData(bool[,] explored);
    }
}
