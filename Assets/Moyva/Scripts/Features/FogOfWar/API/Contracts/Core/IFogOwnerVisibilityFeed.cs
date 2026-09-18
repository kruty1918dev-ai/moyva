using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Emits batched per-owner notifications when cells transition into
    /// <see cref="FogStateType.Visible"/>. Used by fog intel reconciliation and
    /// multiplayer observer correction — not by per-tile rendering.
    /// </summary>
    public interface IFogOwnerVisibilityFeed
    {
        /// <summary>
        /// Fired once per mutation batch per owner with the deduplicated set of
        /// cells that became visible for that owner.
        /// </summary>
        event Action<string, IReadOnlyCollection<Vector2Int>> CellsBecameVisible;
    }
}
