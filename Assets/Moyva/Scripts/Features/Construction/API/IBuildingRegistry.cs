namespace Kruty1918.Moyva.Construction.API
{
    public interface IBuildingRegistry
    {
        BuildingDefinition GetById(string id);
        BuildingDefinition[] GetByCategory(BuildingCategory category);
    }
}
