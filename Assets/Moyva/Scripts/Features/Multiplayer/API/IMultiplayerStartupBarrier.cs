using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.Multiplayer.Core
{
    public interface IMultiplayerStartupBarrier
    {
        bool IsHostReady { get; }
        bool IsReadyToPlay { get; }
        void BeginStartup();
        Task WaitForLocalWorldAsync(CancellationToken ct);
        Task WaitForHostAsync(CancellationToken ct);
        void MarkHostReady();
    }
}
