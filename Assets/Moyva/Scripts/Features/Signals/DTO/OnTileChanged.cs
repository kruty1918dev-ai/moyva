using UnityEngine;

namespace Kruty1918.Moyva.Signals
{
    public struct OnTileChanged
    {
       public Vector2Int Position { get; set; }
    }

    // Викликається, коли фабрика створила юніта
    public struct UnitCreatedSignal
    {
        public string UnitId;       // "warrior-01_1"
        public string UnitTypeId;   // "warrior" для пошуку в SO
        public UnityEngine.Vector2Int Position;
        public UnityEngine.GameObject UnitObject;
    }

    // Викликається, коли юніт перемістився
    public struct UnitMovedSignal
    {
        public string UnitId;
        public UnityEngine.Vector2Int NewPosition;
        public float Cost;
    }

    // Викликається при смерті/видаленні
    public struct UnitDestroyedSignal
    {
        public string UnitId;
    }
}
