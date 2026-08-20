using System.Collections.Generic;

namespace Kruty1918.Moyva.BotAI.API
{
    public interface IBotDeploymentPlanner
    {
        IReadOnlyList<BotActionCandidate> Generate(BotWorldSnapshot snapshot, BotStrategicContext strategy);
    }
}
