using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Multiplayer.Persistence;
namespace Kruty1918.Moyva.Multiplayer.Core
{
    public sealed class HostMigrationCheckpoint
    {
        public string SessionId { get; }
        public string LobbyId { get; }
        public string HostPlayerId { get; }
        public IReadOnlyList<Participant> Participants { get; }
        public WorldSnapshot WorldSnapshot { get; }
        public DateTime CreatedAtUtc { get; }

        public HostMigrationCheckpoint(
            string sessionId,
            string lobbyId,
            string hostPlayerId,
            IReadOnlyList<Participant> participants,
            WorldSnapshot worldSnapshot,
            DateTime createdAtUtc)
        {
            SessionId = sessionId ?? string.Empty;
            LobbyId = lobbyId ?? string.Empty;
            HostPlayerId = hostPlayerId ?? string.Empty;
            Participants = participants ?? Array.Empty<Participant>();
            WorldSnapshot = worldSnapshot;
            CreatedAtUtc = createdAtUtc;
        }
    }

    public interface IHostMigrationCheckpointService
    {
        void Save(HostMigrationCheckpoint checkpoint);
        bool TryGetLatest(out HostMigrationCheckpoint checkpoint);
    }

    public sealed class HostMigrationCheckpointService : IHostMigrationCheckpointService
    {
        private readonly object _sync = new object();
        private HostMigrationCheckpoint _latest;

        public void Save(HostMigrationCheckpoint checkpoint)
        {
            if (checkpoint == null)
                return;

            lock (_sync)
                _latest = checkpoint;
        }

        public bool TryGetLatest(out HostMigrationCheckpoint checkpoint)
        {
            lock (_sync)
            {
                checkpoint = _latest;
                return checkpoint != null;
            }
        }
    }
}