using GiantGrey.TileWorldCreator;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphLogicalTileMapTwcLookup
    {
        TilesBuildLayer FindTilesBuildLayer(Configuration configuration, string blueprintLayerGuid);
        float ResolveSurfaceHeight(BlueprintLayer blueprint, TilesBuildLayer buildLayer);
    }
}
