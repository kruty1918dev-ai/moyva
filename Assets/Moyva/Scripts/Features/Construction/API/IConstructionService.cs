using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public interface IConstructionService
    {
        /// <summary>Поточний стан сесії будівництва.</summary>
        BuildingPlacementState State { get; }

        /// <summary>True коли активний режим знесення (замість розміщення).</summary>
        bool IsDemolishMode { get; }

        /// <summary>
        /// Вибрати будівлю для розміщення. Перемикає State → Placing.
        /// </summary>
        void SelectBuilding(string buildingId);

        /// <summary>Поточний вибраний buildingId або null якщо нічого не вибрано.</summary>
        string GetSelectedBuildingId();

        /// <summary>
        /// Спробувати розмістити preview будівлі на тайлі.
        /// Надсилає BuildingPreviewChangedSignal з актуальним BuildingPreviewState.
        /// Повертає true якщо PreviewState = Valid, false якщо Blocked або State != Placing.
        /// </summary>
        bool TryPreviewAt(Vector2Int position);

        /// <summary>
        /// Повертає true, якщо на тайлі є непідтверджене preview-розміщення в поточній сесії.
        /// </summary>
        bool HasPendingPlacementAt(Vector2Int position);

        /// <summary>
        /// Повертає buildingId непідтвердженого preview-розміщення на тайлі.
        /// </summary>
        bool TryGetPendingBuildingIdAt(Vector2Int position, out string buildingId);

        /// <summary>
        /// Перемістити непідтверджену будівлю з одного тайлу на інший.
        /// Повертає true при успіху, false якщо нова позиція заблокована або preview не знайдено.
        /// </summary>
        bool TryMovePendingPlacement(Vector2Int fromPosition, Vector2Int toPosition);

        /// <summary>
        /// Підтвердити всі pending-розміщення.
        /// Реєструє кожне в ObjectsMapService, надсилає BuildingPlacedSignal.
        /// У режимі знесення — підтверджує всі позначені до знесення об'єкти.
        /// Після Confirm дія незворотна.
        /// </summary>
        void Confirm();

        /// <summary>
        /// Скасувати всю сесію будівництва.
        /// Видаляє всі pending, очищує Redo-стек, надсилає BuildingCancelledSignal.
        /// </summary>
        void Cancel();

        /// <summary>Відмінити останнє розміщення (Ctrl+Z / кнопка Undo).</summary>
        void UndoLast();

        /// <summary>Повернути скасоване розміщення (Ctrl+Y / кнопка Redo).</summary>
        void RedoLast();

        /// <summary>
        /// Увімкнути або вимкнути режим знесення.
        /// В режимі знесення <see cref="TryDemolishAt"/> замість розміщення видаляє будівлю.
        /// </summary>
        void ToggleDemolishMode();

        /// <summary>
        /// Позначити будівлю на позиції до знесення, якщо вона була поставлена гравцем у цій сесії гри.
        /// Надсилає BuildingPreviewChangedSignal(Valid) для preview-підсвітки.
        /// Фактичне знесення і BuildingDemolishedSignal відбуваються під час Confirm().
        /// Будівлі, що існували до початку гри або розміщені не гравцем — не знищуються.
        /// </summary>
        bool TryDemolishAt(Vector2Int position);

        /// <summary>
        /// Повертає словник усіх будівель, підтверджених гравцем у цій сесії.
        /// Ключ — позиція тайлу, значення — buildingId.
        /// </summary>
        IReadOnlyDictionary<Vector2Int, string> GetPlayerPlacedBuildings();

        /// <summary>
        /// Відновлює будівлю з saved data: реєструє в ObjectsMap, додає до
        /// playerPlacedBuildings і стріляє BuildingPlacedSignal для візуалів.
        /// Не потребує активного режиму будівництва.
        /// </summary>
        void RestoreFromSave(Vector2Int position, string buildingId);

        /// <summary>
        /// Видалити конкретне pending-розміщення за позицією.
        /// Використовується для перебудови шляху стін при drag.
        /// Повертає true якщо pending було успішно видалено.
        /// </summary>
        bool RemovePendingAt(Vector2Int position);

        /// <summary>
        /// Безпосередньо розміщує будівлю від імені фракції, минаючи UI flow.
        /// Не потребує активного режиму будівництва.
        /// Повертає true якщо розміщення успішне (тайл вільний).
        /// </summary>
        bool TryDirectPlace(string buildingId, Vector2Int position, string placedByFactionId);

        /// <summary>
        /// Знести будівлю від імені фракції (для AI/мережевих команд).
        /// Фракція повинна бути власником будівлі.
        /// Повертає true якщо успішно.
        /// </summary>
        bool TryDemolishByFaction(Vector2Int position, string factionId);
    }
}
