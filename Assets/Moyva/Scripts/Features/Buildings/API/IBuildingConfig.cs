using UnityEngine;

namespace Kruty1918.Moyva.Buildings.API
{
    /// <summary>
    /// Конфігурація одного типу будівлі.
    /// </summary>
    public interface IBuildingConfig
    {
        /// <summary>Унікальний ідентифікатор типу будівлі (наприклад, "barracks-01").</summary>
        string BuildingId { get; }

        /// <summary>Відображувана назва у меню будівництва.</summary>
        string DisplayName { get; }

        /// <summary>Категорія: Військова / Цивільна / Індустріальна.</summary>
        BuildingCategory Category { get; }

        /// <summary>Спрайт будівлі для відображення у меню та на карті.</summary>
        Sprite Sprite { get; }

        /// <summary>Розмір будівлі у тайлах (1×1, 2×2, тощо).</summary>
        Vector2Int Size { get; }

        /// <summary>Префаб, який розміщується при підтвердженні будівництва.</summary>
        GameObject Prefab { get; }
    }
}
