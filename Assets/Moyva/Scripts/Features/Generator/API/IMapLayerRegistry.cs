using Kruty1918.Moyva.Grid.API;

namespace Kruty1918.Moyva.Generator.API
{
    public interface IMapLayerRegistry
    {
        bool TryGetTileDefinition(string id, out TileTypeDefinition definition);
        bool TryGetObjectDefinition(string id, out MapObjectDefinition definition);
        bool IsKnownId(string id);
    }
}
