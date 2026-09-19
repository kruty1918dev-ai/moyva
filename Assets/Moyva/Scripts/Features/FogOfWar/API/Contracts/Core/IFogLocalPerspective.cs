using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Identifies which owner the local (non-owner-scoped) fog state represents.
    ///
    /// The global <see cref="IFogStateReader"/> surface of
    /// <see cref="IFogOfWarService"/> answers for the local player's
    /// perspective only. Vision sources owned by other owners feed their own
    /// per-owner grids via <see cref="IFogOwnerStateReader"/> and never reach
    /// the local grid.
    /// </summary>
    public interface IFogLocalPerspective
    {
        /// <summary>
        /// Owner id the local fog perspective belongs to, or null while the
        /// local owner is not resolved yet.
        /// </summary>
        string LocalPerspectiveOwnerId { get; }

        /// <summary>
        /// Sets the local perspective owner and rebuilds the local grid when
        /// it changes. Called once by the bootstrap layer after the local
        /// player identity is resolved.
        /// </summary>
        void SetLocalPerspectiveOwnerId(string ownerId);

        /// <summary>
        /// Returns true when a vision source owned by <paramref name="ownerId"/>
        /// contributes to the local perspective grid. Null/empty owner ids are
        /// treated as local (unowned reveal sources, previews, fallbacks).
        /// </summary>
        bool IsLocalPerspectiveOwner(string ownerId);
    }
}
