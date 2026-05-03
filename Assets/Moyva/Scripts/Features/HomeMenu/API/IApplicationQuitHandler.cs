using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.HomeMenu.API
{
    public interface IApplicationQuitHandler
    {
        Task QuitApplicationIfAsync(Func<Task<bool>> match, CancellationToken ct = default);
    }
}