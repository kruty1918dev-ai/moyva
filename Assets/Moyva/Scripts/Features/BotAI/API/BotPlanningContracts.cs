using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Kruty1918.Moyva.BotAI.API
{
public interface IBotActionExecutor
    {
        Task<BotActionExecutionResult> ExecuteAsync(
            string ownerId,
            BotActionCandidate action,
            CancellationToken token);
    }

public interface IBotStrategicPlanner
    {
        BotStrategicContext Plan(BotWorldSnapshot snapshot);
    }

public interface IBotTurnPlanner
    {
        IReadOnlyList<BotActionCandidate> GenerateCandidates(
            BotWorldSnapshot snapshot,
            BotStrategicContext strategy);
    }
}
