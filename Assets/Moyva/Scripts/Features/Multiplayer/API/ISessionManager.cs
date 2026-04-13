using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;

namespace Kruty1918.Moyva.Multiplayer.Core
{
    /// <summary>
    /// High-level session orchestrator.
    /// </summary>
    public interface ISessionManager
    {
        /// <summary>Read-only snapshot of current participants.</summary>
        IReadOnlyList<Participant> Participants { get; }

        Task<bool> CreateOrJoinSessionAsync(SessionConnectOptions options, CancellationToken ct = default);
        Task LeaveSessionAsync(CancellationToken ct = default);
    }
}
