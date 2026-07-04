using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Read-only fog state queries without mutation side effects.
    /// </summary>
    public interface IFogStateReader
    {
        FogStateType GetFogState(Vector2Int position);
        bool IsVisible(Vector2Int position);
        bool IsExplored(Vector2Int position);
    }
}
