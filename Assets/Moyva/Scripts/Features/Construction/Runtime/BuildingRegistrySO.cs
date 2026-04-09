using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    [CreateAssetMenu(menuName = "Moyva/Construction/BuildingRegistry")]
    public class BuildingRegistrySO : ScriptableObject, IBuildingRegistry
    {
        public API.BuildingDefinition[] Buildings;
        public API.WallCollectionDefinition[] WallCollections;

        /// <summary>Отримати всі будівлі реєстру.</summary>
        public API.BuildingDefinition[] GetAll() => Buildings ?? System.Array.Empty<API.BuildingDefinition>();
        public API.WallCollectionDefinition[] GetWallCollections() => WallCollections ?? System.Array.Empty<API.WallCollectionDefinition>();

        /// <summary>Знайти будівлю за її ID. Повертає null якщо не знайдено.</summary>
        public API.BuildingDefinition GetById(string id) =>
            System.Array.Find(Buildings ?? System.Array.Empty<API.BuildingDefinition>(), b => b != null && b.Id == id);

        /// <summary>Отримати всі будівлі заданої категорії.</summary>
        public API.BuildingDefinition[] GetByCategory(API.BuildingCategory category) =>
            System.Array.FindAll(Buildings ?? System.Array.Empty<API.BuildingDefinition>(), b => b != null && b.Category == category);

        public API.WallCollectionDefinition GetWallCollectionByBuildingId(string buildingId)
        {
            if (string.IsNullOrWhiteSpace(buildingId))
                return null;

            var source = WallCollections ?? System.Array.Empty<API.WallCollectionDefinition>();
            for (int i = 0; i < source.Length; i++)
            {
                var collection = source[i];
                if (collection != null && collection.ContainsBuilding(buildingId))
                    return collection;
            }

            return null;
        }
    }
}
