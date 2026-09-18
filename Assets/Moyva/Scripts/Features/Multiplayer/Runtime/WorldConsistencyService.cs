using Kruty1918.Moyva.Multiplayer.Persistence;

namespace Kruty1918.Moyva.Multiplayer.Core
{
    /// <summary>
    /// Compares world snapshots using checksum and world id.
    /// </summary>
    public sealed class WorldConsistencyService : IWorldConsistencyService
    {
        public ConsistencyCheckResult Compare(WorldSnapshot host, WorldSnapshot client)
        {
            if (host == null || client == null)
            {
                return ConsistencyCheckResult.WorldMismatch;
            }

            if (!string.Equals(host.WorldId, client.WorldId, System.StringComparison.Ordinal))
            {
                return ConsistencyCheckResult.WorldMismatch;
            }

            if (host.Checksum != client.Checksum)
            {
                return ConsistencyCheckResult.WorldMismatch;
            }

            return ConsistencyCheckResult.Equal;
        }
    }
}
