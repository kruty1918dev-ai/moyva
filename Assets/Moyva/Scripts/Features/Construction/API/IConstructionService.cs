using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    public interface IConstructionService
    {
        /// <summary>Поточний стан сесії будівництва.</summary>
        BuildingPlacementState State { get; }

        /// <summary>
        /// Вибрати будівлю для розміщення. Перемикає State → Placing.
        /// </summary>
        void SelectBuilding(string buildingId);

        /// <summary>
        /// Спробувати розмістити preview будівлі на тайлі.
        /// Надсилає BuildingPreviewChangedSignal з актуальним BuildingPreviewState.
        /// Повертає true якщо PreviewState = Valid, false якщо Blocked або State != Placing.
        /// </summary>
        bool TryPreviewAt(Vector2Int position);

        /// <summary>
        /// Підтвердити всі pending-розміщення.
        /// Реєструє кожне в ObjectsMapService, надсилає BuildingPlacedSignal.
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
    }
}
