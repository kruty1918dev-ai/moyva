using UnityEngine;

namespace Kruty1918.Moyva.Construction.API
{
    [System.Serializable]
    public class BuildingDefinition
    {
        public string Id;               // Унікальний ідентифікатор, наприклад "barracks"
        public string DisplayName;      // Назва для UI, наприклад "Казарма"
        public BuildingCategory Category;
        public Sprite Icon;             // Іконка будівлі для меню будівництва
        public GameObject Prefab;       // Prefab будівлі (stub: null поки арт не готовий)
    }
}
