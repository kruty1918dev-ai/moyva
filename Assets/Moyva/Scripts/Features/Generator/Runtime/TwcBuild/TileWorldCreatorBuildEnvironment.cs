using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITileWorldCreatorBuildEnvironment
    {
        TileWorldCreatorManager Manager { get; }
        TileWorldCreatorIdMappingSO Mapping { get; }
        TileWorldCreatorBuildOptions Options { get; }
    }

    internal sealed class TileWorldCreatorBuildEnvironment : ITileWorldCreatorBuildEnvironment
    {
        public TileWorldCreatorBuildEnvironment(
            TileWorldCreatorManager manager,
            TileWorldCreatorIdMappingSO mapping,
            TileWorldCreatorBuildOptions options)
        {
            Manager = manager;
            Mapping = mapping;
            Options = options ?? new TileWorldCreatorBuildOptions();
        }

        public TileWorldCreatorManager Manager { get; }
        public TileWorldCreatorIdMappingSO Mapping { get; }
        public TileWorldCreatorBuildOptions Options { get; }
    }
}
