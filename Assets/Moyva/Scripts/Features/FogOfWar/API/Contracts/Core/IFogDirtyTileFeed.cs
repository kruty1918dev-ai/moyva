using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Публікує перелік dirty fog tiles для visual sync.
    /// </summary>
    public interface IFogDirtyTileFeed
    {
        IReadOnlyCollection<Vector2Int> GetLastDirtyTiles();
    }
}
