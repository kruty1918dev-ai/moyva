using UnityEngine;

namespace Kruty1918.Moyva.Signals
{
    /// <summary>
    /// Стан preview-відображення будівлі на конкретному тайлі.
    /// Використовується замість двох булевих полів (уникає неконсистентних комбінацій).
    /// </summary>
    public enum BuildingPreviewState
    {
        None,    // Підсвітка знята (preview видалено або сесія завершена)
        Valid,   // Тайл вільний — будівля може бути розміщена
        Blocked  // Тайл зайнятий — будівля не може бути розміщена, підсвітити червоним
    }

    /// <summary>
    /// Надсилається ConstructionService.Confirm() для кожного підтвердженого розміщення.
    /// Отримується: підписники (спавнер об'єктів, UI).
    /// </summary>
    public struct BuildingPlacedSignal
    {
        public string BuildingId;
        public Vector2Int Position;
        public string OwnerId;
        public string SourceFactionId;
    }

    /// <summary>
    /// Надсилається ConstructionService.Cancel() при скасуванні всієї сесії будівництва.
    /// Отримується: UI.
    /// </summary>
    public struct BuildingCancelledSignal { }

    /// <summary>
    /// Надсилається ConstructionService при зміні стану preview на тайлі.
    /// Отримується: TileView (змінює відображення тайлу: None/Valid/Blocked).
    /// </summary>
    public struct BuildingPreviewChangedSignal
    {
        public Vector2Int Position;
        public string BuildingId;
        public BuildingPreviewState PreviewState;
    }

    /// <summary>
    /// Надсилається ConstructionService.TryDemolishAt() при успішному знесенні будівлі гравцем.
    /// Отримується: спавнер об'єктів (видалення візуалу), UI.
    /// </summary>
    public struct BuildingDemolishedSignal
    {
        public string BuildingId;
        public Vector2Int Position;
        public string OwnerId;
        public string SourceFactionId;
    }

    /// <summary>
    /// Надсилається WallPlacementService.ShowWallHandles() / EndDrag().
    /// Отримується: UI-компонент ручок стін.
    /// </summary>
    public struct ShowWallHandlesSignal
    {
        public Vector2Int Center;
        public bool Hide; // true — приховати ручки
    }
}
