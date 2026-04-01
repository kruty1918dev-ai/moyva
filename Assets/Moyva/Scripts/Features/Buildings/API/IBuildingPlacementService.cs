using UnityEngine;

namespace Kruty1918.Moyva.Buildings.API
{
    /// <summary>
    /// Сервіс режиму розміщення будівель.
    /// Керує сесійними (непідтвердженими) будівлями, підтримує undo/redo.
    /// </summary>
    public interface IBuildingPlacementService
    {
        /// <summary>Чи активний режим розміщення будівель?</summary>
        bool IsPlacementModeActive { get; }

        /// <summary>TypeId будівлі, яку зараз розміщують (null якщо режим неактивний)</summary>
        string ActiveBuildingTypeId { get; }

        /// <summary>Розпочати режим розміщення для вказаного типу будівлі</summary>
        void StartPlacement(string buildingTypeId);

        /// <summary>Перевірити, чи можна розмістити будівлю на позиції</summary>
        bool CanPlace(Vector2Int position);

        /// <summary>Спробувати розмістити будівлю на позиції. Повертає true якщо успішно.</summary>
        bool TryPlace(Vector2Int position);

        /// <summary>Скасувати останнє розміщення (Ctrl+Z)</summary>
        bool Undo();

        /// <summary>Повернути скасоване розміщення (Ctrl+Y)</summary>
        bool Redo();

        /// <summary>Підтвердити всі розміщення поточної сесії та завершити режим</summary>
        void Confirm();

        /// <summary>Скасувати всі розміщення поточної сесії та завершити режим</summary>
        void Cancel();

        /// <summary>Завершити режим розміщення без підтвердження або скасування сесійних будівель</summary>
        void ExitPlacementMode();
    }
}
