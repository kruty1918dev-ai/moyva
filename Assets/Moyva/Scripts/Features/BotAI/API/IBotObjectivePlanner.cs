using System.Collections.Generic;

namespace Kruty1918.Moyva.BotAI.API
{
    public interface IBotObjectivePlanner
    {
        IReadOnlyList<BotActionCandidate> Generate(
            BotWorldSnapshot snapshot,
            BotStrategicContext strategy);
    }
}
