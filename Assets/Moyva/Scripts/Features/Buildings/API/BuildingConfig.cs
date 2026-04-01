using System;
using UnityEngine;

namespace Kruty1918.Moyva.Buildings.API
{
    [Serializable]
    public class BuildingConfig
    {
        /// <summary>
        /// Унікальний ідентифікатор типу будівлі (наприклад "barracks", "farm", "factory")
        /// </summary>
        public string TypeId;

        /// <summary>
        /// Назва для відображення в меню (наприклад "Казарма", "Ферма")
        /// </summary>
        public string DisplayName;

        /// <summary>Категорія для меню будівництва</summary>
        public BuildingCategory Category;

        /// <summary>Префаб будівлі для спавну</summary>
        public GameObject Prefab;

        /// <summary>Спрайт для попереднього перегляду в режимі розміщення</summary>
        public Sprite PreviewSprite;

        /// <summary>Чи є ця будівля стіною (для особливої логіки з'єднання стін)</summary>
        public bool IsWall;
    }
}
