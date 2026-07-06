using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    public interface IGraphToConfigurationCompilerService
    {
        List<CompiledLayerMap> Compile(
            GraphAsset graph,
            TileWorldCreatorManager manager,
            int seed,
            ISet<string> skippedLayerIds = null,
            Vector2Int? mapSizeOverride = null);
    }
}
