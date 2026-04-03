namespace Kruty1918.Moyva.Generator.API
{
    public interface IMapObjectRegistryService
    {
        bool TryGetDefinition(string id, out MapObjectDefinition definition);
    }
}
