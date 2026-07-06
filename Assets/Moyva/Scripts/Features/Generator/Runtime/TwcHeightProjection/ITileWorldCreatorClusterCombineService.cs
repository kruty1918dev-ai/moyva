using GiantGrey.TileWorldCreator.Components;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITileWorldCreatorClusterCombineService
    {
        TileWorldCreatorClusterCombineResult Combine(ClusterIdentifier cluster, bool deactivateSourceObjects);
    }
}
