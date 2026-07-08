using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal interface IChunkFirstWorldBuildService
    {
        TileWorldCreatorWorldBuildResult Build(
            GeneratedWorldData worldData,
            Configuration configuration,
            TileWorldCreatorTerrainBuildPolicyResult terrainPolicy);
    }
}
