using UnityEngine;

namespace Kruty1918.Moyva.Signals
{
    /// <summary>
    /// Надсилається TileView при наведенні курсора/пальця на тайл
    /// (використовується для показу попереднього перегляду будівлі)
    /// </summary>
    public struct TileHoveredSignal
    {
        public Vector2Int Position;
    }
}
