using System;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.Shared.Connectivity
{
    public interface IConnectivityService
    {
        bool IsOnline { get; }
        event Action<bool> StatusChanged;

        /// <summary>
        /// Waits until connectivity is detected or timeout elapses.
        /// Returns true if connectivity was detected within the timeout.
        /// </summary>
        Task<bool> WaitForOnlineAsync(TimeSpan timeout);
    }
}
