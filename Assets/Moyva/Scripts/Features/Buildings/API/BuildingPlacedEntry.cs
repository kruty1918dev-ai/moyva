using UnityEngine;

namespace Kruty1918.Moyva.Buildings.API
{
    /// <summary>
    /// Запис про будівлю, розміщену у поточній сесії (ще не підтверджену).
    /// </summary>
    public struct BuildingPlacedEntry
    {
        /// <summary>ID типу будівлі.</summary>
        public string BuildingId;

        /// <summary>Позиція на сітці.</summary>
        public Vector2Int Position;

        /// <summary>Унікальний ID конкретного екземпляру (генерується при розміщенні).</summary>
        public string InstanceId;
    }
}
