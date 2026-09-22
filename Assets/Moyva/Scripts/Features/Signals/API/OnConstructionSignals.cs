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
        Blocked, // Тайл зайнятий — будівля не може бути розміщена, підсвітити червоним
        Unaffordable // Місце валідне, але ресурсів для підтвердження ще недостатньо
    }

    /// <summary>
    /// Надсилається ConstructionService.Confirm() для кожного підтвердженого розміщення.
    /// Отримується: підписники (спавнер об'єктів, UI).
    /// </summary>
    public struct BuildingPlacedSignal
    {
        /// <summary>будівлі ID — string.</summary>
        public string BuildingId;
        /// <summary>позицію — Vector2Int.</summary>
        public Vector2Int Position;
        /// <summary>власника ID — string.</summary>
        public string OwnerId;
        /// <summary>джерела Faction ID — string.</summary>
        public string SourceFactionId;
        /// <summary>Чи Relocation джерела — HasRelocationSource.</summary>
        public bool HasRelocationSource;
        /// <summary>Relocation джерела позицію — Vector2Int.</summary>
        public Vector2Int RelocationSourcePosition;
        /// <summary>поворот Quarter Turns — int.</summary>
        public int RotationQuarterTurns;

        /// <summary>миттєвого візуалу — bool.</summary>
        public bool InstantVisual;
    }

    /// <summary>
    /// Надсилається ConstructionService.Cancel() при скасуванні всієї сесії будівництва.
    /// Отримується: UI.
    /// </summary>
    public struct BuildingCancelledSignal { }

    /// <summary>
    /// Надсилається ConstructionService при зміні стану preview на тайлі.
    /// Отримується: TileView (None/Valid/Blocked/Unaffordable).
    /// </summary>
    public struct BuildingPreviewChangedSignal
    {
        /// <summary>позицію — Vector2Int.</summary>
        public Vector2Int Position;
        /// <summary>будівлі ID — string.</summary>
        public string BuildingId;
        /// <summary>превʼю стану — BuildingPreviewState.</summary>
        public BuildingPreviewState PreviewState;
        /// <summary>поворот Quarter Turns — int.</summary>
        public int RotationQuarterTurns;
    }

    /// <summary>
    /// Надсилається, коли гравець обирає іншу будівлю або перемикає demolish-режим.
    /// Дає visual-layer змогу перебудувати сітку під актуальні правила будівлі.
    /// </summary>
    public struct BuildingSelectionChangedSignal
    {
        /// <summary>будівлі ID — string.</summary>
        public string BuildingId;
        /// <summary>Чи Demolish режим — IsDemolishMode.</summary>
        public bool IsDemolishMode;
        /// <summary>поворот Quarter Turns — int.</summary>
        public int RotationQuarterTurns;
    }

    /// <summary>
    /// Надсилається, коли pending-preview переноситься між клітинками.
    /// Дає візуальному шару шанс переїхати плавно, не знищуючи GameObject.
    /// </summary>
    public struct BuildingPreviewMovedSignal
    {
        /// <summary>From позицію — Vector2Int.</summary>
        public Vector2Int FromPosition;
        /// <summary>позицію — Vector2Int.</summary>
        public Vector2Int ToPosition;
        /// <summary>будівлі ID — string.</summary>
        public string BuildingId;
        /// <summary>поворот Quarter Turns — int.</summary>
        public int RotationQuarterTurns;
    }

    /// <summary>
    /// Надсилається під час drag pending-preview для м'якого руху під курсором/пальцем.
    /// SnapToGrid = true означає повернути візуал у центр найближчої валідної клітинки.
    /// </summary>
    public struct BuildingPreviewDragVisualSignal
    {
        /// <summary>позицію — Vector2Int.</summary>
        public Vector2Int Position;
        /// <summary>будівлі ID — string.</summary>
        public string BuildingId;
        /// <summary>поворот Quarter Turns — int.</summary>
        public int RotationQuarterTurns;
        /// <summary>світу позицію — Vector3.</summary>
        public Vector3 WorldPosition;
        /// <summary>привʼязки  сітки — bool.</summary>
        public bool SnapToGrid;
        /// <summary>Чи привʼязки цілі — HasSnapTarget.</summary>
        public bool HasSnapTarget;
        /// <summary>привʼязки цілі позицію — Vector2Int.</summary>
        public Vector2Int SnapTargetPosition;
        /// <summary>Чи привʼязки цілі валідної — IsSnapTargetValid.</summary>
        public bool IsSnapTargetValid;
    }

    /// <summary>BuildGridHoverChangedSignal — struct: збірки сітки наведення Changed сигнал.</summary>
    public struct BuildGridHoverChangedSignal
    {
        /// <summary>Чи тайла — HasTile.</summary>
        public bool HasTile;
        /// <summary>позицію — Vector2Int.</summary>
        public Vector2Int Position;
        /// <summary>будівлі ID — string.</summary>
        public string BuildingId;
        /// <summary>Чи Placement валідної — IsPlacementValid.</summary>
        public bool IsPlacementValid;
        /// <summary>Чи Affordable — IsAffordable.</summary>
        public bool IsAffordable;
        /// <summary>поворот Quarter Turns — int.</summary>
        public int RotationQuarterTurns;
        /// <summary>Позиції футпринта будівлі.</summary>
        public Vector2Int[] FootprintPositions;
        /// <summary>невалідної Footprint Positions — Vector2Int[].</summary>
        public Vector2Int[] InvalidFootprintPositions;
    }

    /// <summary>
    /// Надсилається ConstructionService.TryDemolishAt() при успішному знесенні будівлі гравцем.
    /// Отримується: спавнер об'єктів (видалення візуалу), UI.
    /// </summary>
    public struct BuildingDemolishedSignal
    {
        /// <summary>будівлі ID — string.</summary>
        public string BuildingId;
        /// <summary>позицію — Vector2Int.</summary>
        public Vector2Int Position;
        /// <summary>власника ID — string.</summary>
        public string OwnerId;
        /// <summary>джерела Faction ID — string.</summary>
        public string SourceFactionId;
    }

    /// <summary>BuildingOwnershipTransferredSignal — struct: будівлі Ownership переданої сигнал.</summary>
    public struct BuildingOwnershipTransferredSignal
    {
        /// <summary>будівлі ID — string.</summary>
        public string BuildingId;
        /// <summary>позицію — Vector2Int.</summary>
        public Vector2Int Position;
        /// <summary>Previous власника ID — string.</summary>
        public string PreviousOwnerId;
        /// <summary>нового власника ID — string.</summary>
        public string NewOwnerId;
    }

    /// <summary>
    /// Надсилається WallPlacementService.ShowWallHandles() / EndDrag().
    /// Отримується: UI-компонент ручок стін.
    /// </summary>
    public struct ShowWallHandlesSignal
    {
        /// <summary>центру — Vector2Int.</summary>
        public Vector2Int Center;
        /// <summary>Приховує візуальний елемент.</summary>
        public bool Hide; // true — приховати ручки
    }

    /// <summary>
    /// Надсилається з UI, коли гравець натискає кнопку «Підтвердити» у режимі будівництва.
    /// MultiplayerAuthorityService перехоплює цей сигнал і або виконує Confirm() локально (хост/офлайн),
    /// або надсилає запит до хоста (клієнт).
    /// </summary>
    public struct PlaceBuildingConfirmRequestSignal { }

    /// <summary>
    /// Надсилається, коли авторитетна сторона відхиляє підтвердження розміщення
    /// будівлі (наприклад, хост відхилив запит клієнта). UI показує причину гравцю.
    /// </summary>
    public struct ConstructionPlacementRejectedSignal
    {
        /// <summary>будівлі ID — string.</summary>
        public string BuildingId;
        /// <summary>позицію — Vector2Int.</summary>
        public Vector2Int Position;
        /// <summary>причини — string.</summary>
        public string Reason;
    }
}
