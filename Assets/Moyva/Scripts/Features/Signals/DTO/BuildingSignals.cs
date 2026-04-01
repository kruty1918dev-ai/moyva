using UnityEngine;

namespace Kruty1918.Moyva.Signals
{
    /// <summary>
    /// Надсилається <c>BuildingPlacementService</c> після підтвердження будівництва (Confirm).
    /// Отримується: ObjectsMapService, UI, та інші зацікавлені системи.
    /// </summary>
    public struct BuildingPlacedSignal
    {
        /// <summary>ID типу будівлі (наприклад, "barracks-01").</summary>
        public string BuildingId;

        /// <summary>Унікальний ID екземпляру будівлі.</summary>
        public string InstanceId;

        /// <summary>Позиція на сітці.</summary>
        public Vector2Int Position;
    }

    /// <summary>
    /// Надсилається <c>BuildingPlacementService</c> при скасуванні (Cancel або Undo).
    /// Отримується: UI, системи відображення.
    /// </summary>
    public struct BuildingCancelledSignal
    {
        /// <summary>Унікальний ID екземпляру будівлі, що скасовується.</summary>
        public string InstanceId;

        /// <summary>Позиція на сітці.</summary>
        public Vector2Int Position;
    }
}
