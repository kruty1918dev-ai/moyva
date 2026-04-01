using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    [CreateAssetMenu(menuName = "Moyva/Construction/BuildingRegistry")]
    public class BuildingRegistrySO : ScriptableObject
    {
        public API.BuildingDefinition[] Buildings;

        /// <summary>Знайти будівлю за її ID. Повертає null якщо не знайдено.</summary>
        public API.BuildingDefinition GetById(string id) =>
            System.Array.Find(Buildings, b => b.Id == id);

        /// <summary>Отримати всі будівлі заданої категорії.</summary>
        public API.BuildingDefinition[] GetByCategory(API.BuildingCategory category) =>
            System.Array.FindAll(Buildings, b => b.Category == category);
    }
}
