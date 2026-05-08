using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.HomeMenu.API
{
    public interface IHomeMenuGameStarter
    {
        Task StartGameAsync(CancellationToken ct = default);
    }
}