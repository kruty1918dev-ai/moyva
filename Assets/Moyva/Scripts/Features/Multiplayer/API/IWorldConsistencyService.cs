using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Persistence;

namespace Kruty1918.Moyva.Multiplayer.Core
{
    /// <summary>
    /// Compares client world metadata against the host's authoritative copy.
    /// </summary>
    public interface IWorldConsistencyService
    {
        ConsistencyCheckResult Compare(WorldSnapshot host, WorldSnapshot client);
    }
}
