using UnityEngine;

namespace Kruty1918.Moyva.ObjectsMap.API
{
    /// <summary>
    /// Єдина авторитетна карта обʼєктів. Не розрізняє тип (юніт/будівля/ресурс) —
    /// лише факт присутності та ідентифікатор.
    /// </summary>
    public interface IObjectsMapService
    {
        /// <summary>Чи зайнята позиція будь-яким обʼєктом?</summary>
        bool IsOccupied(Vector2Int position);

        /// <summary>
        /// Отримати ID окупанта. Повертає false якщо тайл вільний (occupantId буде null).
        /// </summary>
        bool TryGetOccupant(Vector2Int position, out string occupantId);

        /// <summary>
        /// Зареєструвати обʼєкт на позиції. Кидає InvalidOperationException якщо позиція зайнята.
        /// Надсилає OnObjectsMapChangedSignal.
        /// </summary>
        void Register(Vector2Int position, string occupantId);

        /// <summary>
        /// Перемістити обʼєкт з from → to. Звільняє from, займає to.
        /// Кидає InvalidOperationException якщо to зайнято або from вільно.
        /// Надсилає OnObjectsMapChangedSignal для обох позицій.
        /// </summary>
        void Move(Vector2Int from, Vector2Int to);

        /// <summary>
        /// Звільнити позицію. Нічого не робить якщо позиція вже вільна.
        /// Надсилає OnObjectsMapChangedSignal.
        /// </summary>
        void Unregister(Vector2Int position);

        /// <summary>Знайти позицію за ID окупанта. False якщо не зареєстровано.</summary>
        bool TryGetPosition(string occupantId, out Vector2Int position);
    }
}
