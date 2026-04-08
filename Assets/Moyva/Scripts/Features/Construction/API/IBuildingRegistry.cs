namespace Kruty1918.Moyva.Construction.API
{
    public interface IBuildingRegistry
    {
        /// <summary>Отримати всі будівлі реєстру.</summary>
        BuildingDefinition[] GetAll();
        BuildingDefinition GetById(string id);
        BuildingDefinition[] GetByCategory(BuildingCategory category);
    }
}
