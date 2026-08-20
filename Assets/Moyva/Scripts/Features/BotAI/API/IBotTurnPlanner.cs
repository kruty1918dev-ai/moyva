using System.Collections.Generic;

namespace Kruty1918.Moyva.BotAI.API
{
    public interface IBotTurnPlanner
    {
        IReadOnlyList<BotActionCandidate> GenerateCandidates(
            BotWorldSnapshot snapshot,
            BotStrategicContext strategy);
    }
}
