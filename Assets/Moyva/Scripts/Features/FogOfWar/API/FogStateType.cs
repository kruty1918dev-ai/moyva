namespace Kruty1918.Moyva.FogOfWar.API
{
    public enum FogStateType : byte
    {
        Unexplored = 0,
        Forgotten  = 1,
        Explored   = Forgotten,
        Visible    = 2
    }
}
