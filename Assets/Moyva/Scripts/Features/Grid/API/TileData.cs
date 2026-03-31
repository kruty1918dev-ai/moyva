using UnityEngine;

namespace Kruty1918.Moyva.Grid.API
{
    public struct TileData
    {
        public bool IsOccupied { get; internal set; }
        public string OccupantId { get; internal set; }
        public string TileTypeId { get; set; }
    }
}
