using System.Collections.Generic;

namespace Kruty1918.Moyva.BotAI.API
{
    public interface IBotMovementPlanner
    {
        IReadOnlyList<BotActionCandidate> Generate(BotWorldSnapshot snapshot, BotStrategicContext strategy);
    }
}
