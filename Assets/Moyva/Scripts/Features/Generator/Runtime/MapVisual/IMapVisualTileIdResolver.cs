using Kruty1918.Moyva.Generator.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IMapVisualTileIdResolver
    {
        bool TryResolve(string tileId, out TileTypeDefinition tileType, out string resolvedTileId);
        string ResolveGridTileId(string tileId);
    }
}
