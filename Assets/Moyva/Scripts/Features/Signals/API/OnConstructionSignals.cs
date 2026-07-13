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
        public bool HasRelocationSource;
        public Vector2Int RelocationSourcePosition;
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
    /// Надсилається, коли гравець обирає іншу будівлю або перемикає demolish-режим.
    /// Дає visual-layer змогу перебудувати сітку під актуальні правила будівлі.
    /// </summary>
    public struct BuildingSelectionChangedSignal
    {
        public string BuildingId;
        public bool IsDemolishMode;
    }

    /// <summary>
    /// Надсилається, коли pending-preview переноситься між клітинками.
    /// Дає візуальному шару шанс переїхати плавно, не знищуючи GameObject.
    /// </summary>
    public struct BuildingPreviewMovedSignal
    {
        public Vector2Int FromPosition;
        public Vector2Int ToPosition;
        public string BuildingId;
    }

    /// <summary>
    /// Надсилається під час drag pending-preview для м'якого руху під курсором/пальцем.
    /// SnapToGrid = true означає повернути візуал у центр найближчої валідної клітинки.
    /// </summary>
    public struct BuildingPreviewDragVisualSignal
    {
        public Vector2Int Position;
        public string BuildingId;
        public Vector3 WorldPosition;
        public bool SnapToGrid;
        public bool HasSnapTarget;
        public Vector2Int SnapTargetPosition;
        public bool IsSnapTargetValid;
    }

    /// <summary>
    /// Emitted only when the actual grid cell below the pointer (or its validity)
    /// changes. The arrays describe the selected building footprint for rendering.
    /// </summary>
    public struct BuildGridHoverChangedSignal
    {
        public bool HasTile;
        public Vector2Int Position;
        public string BuildingId;
        public bool IsPlacementValid;
        public Vector2Int[] FootprintPositions;
        public Vector2Int[] InvalidFootprintPositions;
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

    /// <summary>
    /// Надсилається з UI, коли гравець натискає кнопку «Підтвердити» у режимі будівництва.
    /// MultiplayerAuthorityService перехоплює цей сигнал і або виконує Confirm() локально (хост/офлайн),
    /// або надсилає запит до хоста (клієнт).
    /// </summary>
    public struct PlaceBuildingConfirmRequestSignal { }
}
