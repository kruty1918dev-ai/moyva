namespace Kruty1918.Moyva.BotAI.API
{
    public interface IBotWorldSnapshotBuilder
    {
        BotWorldSnapshot Build(string ownerId, long globalTurn);
    }
}
