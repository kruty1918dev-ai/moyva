using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;

namespace Kruty1918.Moyva.Multiplayer.Core
{
    public enum LocalGameplayRole
    {
        Offline = 0,
        Host = 1,
        Client = 2,
    }

    public readonly struct LocalGameplayRoleSnapshot
    {
        public LocalGameplayRoleSnapshot(
            LocalGameplayRole role,
            string playerId)
        {
            Role = role;
            PlayerId = playerId?.Trim() ?? string.Empty;
        }

        public LocalGameplayRole Role { get; }
        public string PlayerId { get; }
        public bool IsAuthoritative =>
            Role == LocalGameplayRole.Offline
            || Role == LocalGameplayRole.Host;
    }

    public interface ILocalGameplayRoleResolver
    {
        LocalGameplayRoleSnapshot Resolve();
    }

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
