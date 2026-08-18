using UnityEngine;

namespace Kruty1918.Moyva.Signals
{
    public enum TilePointerButton
    {
        Primary = 0,
        Secondary = 1,
    }

    public class TileClickedSignal
    {
        public Vector2Int Position;
        public TilePointerButton Button;
    }
}
