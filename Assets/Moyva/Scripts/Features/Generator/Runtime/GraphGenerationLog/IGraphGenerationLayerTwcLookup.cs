using GiantGrey.TileWorldCreator;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphGenerationLayerTwcLookup
    {
        int CountGeneratedCells(TileWorldCreatorManager manager, CompiledLayerMap compiled);
        string ResolveBlueprintName(TileWorldCreatorManager manager, CompiledLayerMap compiled);
        string ResolveBuildLayerName(Configuration configuration, string buildLayerKey, string blueprintLayerGuid);
    }
}
