using UnityEngine;

namespace Kruty1918.Moyva.Buildings.API
{
    /// <summary>
    /// Сервіс розміщення будівель.
    /// Відповідає за режим вибору будівлі, попередній перегляд, підтвердження та скасування.
    /// </summary>
    public interface IBuildingPlacementService
    {
        /// <summary>Чи активний режим розміщення будівлі.</summary>
        bool IsPlacingMode { get; }

        /// <summary>ID вибраного типу будівлі, або null якщо нічого не вибрано.</summary>
        string SelectedBuildingId { get; }

        /// <summary>
        /// Перейти в режим розміщення для вказаного типу будівлі.
        /// </summary>
        void SelectBuilding(string buildingId);

        /// <summary>
        /// Вийти з режиму розміщення без підтвердження.
        /// </summary>
        void ExitPlacingMode();

        /// <summary>
        /// Перевіряє, чи можна розмістити вибрану будівлю на позиції (тайл вільний і дійсний).
        /// </summary>
        bool CanPlaceAt(Vector2Int position);

        /// <summary>
        /// Розмістити будівлю на позиції (додає до списку pending).
        /// Нічого не робить, якщо позиція недоступна або режим не активний.
        /// </summary>
        void PlaceBuilding(Vector2Int position);

        /// <summary>
        /// Підтвердити всі розміщені будівлі: зареєструвати в ObjectsMap і надіслати сигнали.
        /// Очищає pending-список і виходить з режиму розміщення.
        /// </summary>
        void Confirm();

        /// <summary>
        /// Скасувати всі розміщені (pending) будівлі: звільнити позиції та видалити з pending-списку.
        /// Виходить з режиму розміщення.
        /// </summary>
        void Cancel();

        /// <summary>
        /// Скасувати останнє розміщення (Ctrl+Z). Переміщує у redo-стек.
        /// </summary>
        void Undo();

        /// <summary>
        /// Повторити скасоване розміщення (Ctrl+Y). Переміщує з redo-стека назад у pending.
        /// </summary>
        void Redo();
    }
}
