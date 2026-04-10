using Kruty1918.Moyva.Multiplayer.Persistence;

namespace Kruty1918.Moyva.Multiplayer.Core
{
    /// <summary>
    /// Compares world snapshots using checksum and world id.
    /// </summary>
    public sealed class WorldConsistencyService : IWorldConsistencyService
    {
        private readonly IMultiplayerLogger _logger;

        public WorldConsistencyService(IMultiplayerLogger logger)
        {
            _logger = logger;
        }

        public ConsistencyCheckResult Compare(WorldSnapshot host, WorldSnapshot client)
        {
            if (host == null || client == null)
            {
                _logger.Warn("WorldConsistencyService.Compare: one or both snapshots are null.");
                return ConsistencyCheckResult.WorldMismatch;
            }

            if (!string.Equals(host.WorldId, client.WorldId, System.StringComparison.Ordinal))
            {
                _logger.Warn($"WorldId mismatch: host={host.WorldId}, client={client.WorldId}");
                return ConsistencyCheckResult.WorldMismatch;
            }

            if (host.Checksum != client.Checksum)
            {
                _logger.Warn($"Checksum mismatch for world {host.WorldId}: host={host.Checksum:X8}, client={client.Checksum:X8}");
                return ConsistencyCheckResult.WorldMismatch;
            }

            return ConsistencyCheckResult.Equal;
        }
    }
}
