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
        /// <summary>PlayerId of the local participant when available (empty if unknown).</summary>
        string LocalPlayerId { get; }
        /// <summary>True when the local participant is the current session host.</summary>
        bool IsLocalPlayerHost { get; }

        Task<bool> CreateOrJoinSessionAsync(SessionConnectOptions options, CancellationToken ct = default);
        Task LeaveSessionAsync(CancellationToken ct = default);
    }
}
