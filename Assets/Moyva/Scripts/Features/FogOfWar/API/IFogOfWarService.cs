using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Основний runtime API для gameplay-стану туману.
    /// Реалізацію зазвичай надає <c>FogOfWarService</c>, а інші системи мають працювати через цей інтерфейс,
    /// не залежачи від concrete service або visual updaters.
    /// </summary>
    public interface IFogOfWarService
    {
        /// <summary>
        /// Створює внутрішній fog state для карти заданого розміру.
        /// Зазвичай викликається після генерації світу або відновлення save.
        /// </summary>
        /// <param name="width">Ширина карти у клітинках.</param>
        /// <param name="height">Висота карти у клітинках.</param>
        void Initialize(int width, int height);

        /// <summary>
        /// Реєструє юніта як джерело поточної видимості.
        /// </summary>
        /// <param name="unitId">Стабільний ідентифікатор юніта.</param>
        /// <param name="position">Поточна клітинка юніта.</param>
        /// <param name="visionRange">Базовий радіус видимості юніта.</param>
        void RegisterUnit(string unitId, Vector2Int position, int visionRange);

        /// <summary>
        /// Оновлює vision range уже зареєстрованого юніта і перебудовує пов'язані visible-області.
        /// </summary>
        /// <param name="unitId">Ідентифікатор юніта.</param>
        /// <param name="visionRange">Новий радіус видимості.</param>
        void UpdateUnitVisionRange(string unitId, int visionRange);

        /// <summary>
        /// Реєструє постійну область видимості, яка не прив'язана до рухомого юніта.
        /// Підходить для будівель, стартових anchor-ів та інших fixed reveal sources.
        /// </summary>
        /// <param name="areaId">Стабільний ідентифікатор області.</param>
        /// <param name="position">Центральна клітинка області.</param>
        /// <param name="visionRange">Радіус області.</param>
        /// <param name="shape">Форма reveal.</param>
        void RegisterFixedVisionArea(string areaId, Vector2Int position, int visionRange, FogRevealShape shape);

        /// <summary>
        /// Виконує одноразове або закріплене reveal-оновлення у вказаній області.
        /// </summary>
        /// <param name="center">Центр reveal.</param>
        /// <param name="radius">Радіус reveal у клітинках.</param>
        /// <param name="shape">Форма області.</param>
        /// <param name="keepVisible">Чи повинна область залишатись видимою надалі.</param>
        /// <param name="visibleAreaId">Необов'язковий id для постійної visible-області.</param>
        void RevealArea(Vector2Int center, int radius, FogRevealShape shape, bool keepVisible, string visibleAreaId = null);

        /// <summary>
        /// Оновлює позицію зареєстрованого юніта і перебудовує його вклад у visible/explored state.
        /// </summary>
        /// <param name="unitId">Ідентифікатор юніта.</param>
        /// <param name="newPosition">Нова клітинка юніта.</param>
        void UpdateUnitPosition(string unitId, Vector2Int newPosition);

        /// <summary>
        /// Видаляє юніта з системи видимості.
        /// </summary>
        /// <param name="unitId">Ідентифікатор юніта, який більше не повинен впливати на fog.</param>
        void UnregisterUnit(string unitId);

        /// <summary>
        /// Повертає поточний gameplay-стан конкретної клітинки.
        /// Це query-only метод.
        /// </summary>
        /// <param name="position">Клітинка карти.</param>
        /// <returns>Поточний fog state клітинки.</returns>
        FogStateType GetFogState(Vector2Int position);

        /// <summary>
        /// Перевіряє, чи клітинка видима прямо зараз.
        /// Це query-only метод.
        /// </summary>
        /// <param name="position">Клітинка карти.</param>
        /// <returns><see langword="true"/>, якщо клітинка зараз видима.</returns>
        bool IsVisible(Vector2Int position);

        /// <summary>
        /// Перевіряє, чи клітинка вже була відкрита раніше.
        /// Це query-only метод.
        /// </summary>
        /// <param name="position">Клітинка карти.</param>
        /// <returns><see langword="true"/>, якщо клітинка explored або visible.</returns>
        bool IsExplored(Vector2Int position);

        /// <summary>
        /// Формує snapshot explored-стану для save/load або відлагодження.
        /// Visible state окремо не зберігається: при завантаженні він має бути відновлений із поточних reveal sources.
        /// </summary>
        /// <returns>Двовимірний explored snapshot.</returns>
        bool[,] GetExploredSnapshot();

        /// <summary>
        /// Завантажує explored snapshot у runtime state.
        /// Метод змінює fog state і може спричинити повну visual rebuild.
        /// </summary>
        /// <param name="explored">Snapshot explored-клітинок.</param>
        void LoadFromSnapshot(bool[,] explored);

        /// <summary>
        /// Повертає перелік клітинок, що змінилися під час останнього оновлення стану.
        /// Використовується visual layer для інкрементальних update-ів.
        /// </summary>
        /// <returns>Колекція dirty-клітинок.</returns>
        IReadOnlyCollection<Vector2Int> GetLastDirtyTiles();
    }
}
