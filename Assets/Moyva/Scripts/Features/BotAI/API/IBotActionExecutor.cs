using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.BotAI.API
{
    public interface IBotActionExecutor
    {
        Task<BotActionExecutionResult> ExecuteAsync(
            string ownerId,
            BotActionCandidate action,
            CancellationToken token);
    }
}
