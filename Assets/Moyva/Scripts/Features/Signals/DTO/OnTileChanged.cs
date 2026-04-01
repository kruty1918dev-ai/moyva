using UnityEngine;

namespace Kruty1918.Moyva.Signals
{
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

    public struct InterruptMovementSignal
    {
        public string UnitId;
    }

    /// <summary>
    /// Надсилається MapVisualInstantiator після спавну статичного обʼєкта карти (гора, річка, ліс…)
    /// </summary>
    public struct OnMapObjectSpawnedSignal
    {
        public string ObjectId;        // TileTypeId, наприклад "river", "mountain"
        public Vector2Int Position;
    }

    /// <summary>
    /// Надсилається ObjectsMapService після будь-якої зміни карти обʼєктів
    /// </summary>
    public struct OnObjectsMapChangedSignal
    {
        public Vector2Int Position;
        public string OccupantId;      // null якщо тайл звільнено
    }
}
