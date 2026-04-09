namespace Kruty1918.Moyva.Construction.API
{
    public interface IBuildingRegistry
    {
        /// <summary>Отримати всі будівлі реєстру.</summary>
        BuildingDefinition[] GetAll();
        BuildingDefinition GetById(string id);
        BuildingDefinition[] GetByCategory(BuildingCategory category);
        WallCollectionDefinition[] GetWallCollections();

        /// <summary>Повертає налаштування стін/воріт для buildingId або null, якщо buildingId не належить wall-колекції.</summary>
        WallCollectionDefinition GetWallCollectionByBuildingId(string buildingId);
    }
}
