using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.HomeMenu.Runtime.Startup
{
    internal interface IGameplayStartupPipeline
    {
        GameplayStartupPhase CurrentPhase { get; }
        Task RunAsync(CancellationToken ct = default);
    }
}
