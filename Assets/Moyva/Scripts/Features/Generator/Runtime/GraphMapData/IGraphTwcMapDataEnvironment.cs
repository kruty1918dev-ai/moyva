using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphTwcMapDataEnvironment
    {
        GraphAsset Graph { get; }
        TileWorldCreatorManager Manager { get; }
    }
}
