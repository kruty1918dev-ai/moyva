using UnityEngine;

namespace Kruty1918.Moyva.Buildings.API
{
    /// <summary>
    /// Дані розміщеної (підтвердженої) будівлі
    /// </summary>
    public class PlacedBuilding
    {
        /// <summary>Унікальний ID цього екземпляру будівлі</summary>
        public string BuildingId;

        /// <summary>Тип будівлі (посилання на BuildingConfig.TypeId)</summary>
        public string TypeId;

        /// <summary>Позиція на сітці</summary>
        public Vector2Int Position;

        /// <summary>Ігровий об'єкт будівлі в сцені</summary>
        public GameObject GameObject;
    }
}
