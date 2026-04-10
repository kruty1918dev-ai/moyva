using System.Collections.Generic;

namespace Kruty1918.Moyva.Multiplayer.Core
{
    /// <summary>
    /// Selects the best host candidate when the current host leaves.
    /// Carcass only.
    /// </summary>
    public interface IHostMigrationService
    {
        /// <summary>Returns the best candidate for the new host, or null if none available.</summary>
        Participant ChooseNewHost(IReadOnlyList<Participant> remaining);
    }
}
