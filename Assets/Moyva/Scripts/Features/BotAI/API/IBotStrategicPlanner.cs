namespace Kruty1918.Moyva.BotAI.API
{
    public interface IBotStrategicPlanner
    {
        BotStrategicContext Plan(BotWorldSnapshot snapshot);
    }
}
