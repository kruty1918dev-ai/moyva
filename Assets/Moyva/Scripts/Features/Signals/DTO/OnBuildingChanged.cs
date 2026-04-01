using UnityEngine;

namespace Kruty1918.Moyva.Signals
{
    /// <summary>
    /// Будівля розміщена в режимі будівництва (сесійна, ще не підтверджена)
    /// </summary>
    public struct BuildingPreviewPlacedSignal
    {
        /// <summary>Унікальний ID в рамках поточної сесії розміщення</summary>
        public string SessionId;
        public string TypeId;
        public Vector2Int Position;
    }

    /// <summary>
    /// Сесійна будівля видалена (скасована або undo)
    /// </summary>
    public struct BuildingPreviewRemovedSignal
    {
        public string SessionId;
        public Vector2Int Position;
    }

    /// <summary>
    /// Гравець підтвердив будівництво — всі сесійні будівлі стають постійними
    /// </summary>
    public struct BuildingConstructionConfirmedSignal { }

    /// <summary>
    /// Гравець скасував будівництво — всі сесійні будівлі видаляються
    /// </summary>
    public struct BuildingConstructionCanceledSignal { }
}
