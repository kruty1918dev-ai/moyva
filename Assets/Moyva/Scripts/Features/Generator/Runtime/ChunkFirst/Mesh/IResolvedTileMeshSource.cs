using System.Collections.Generic;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal interface IResolvedTileMeshSource
    {
        int CollectMeshSources(ResolvedTileComposition composition, List<TileMeshSource> results);
    }
}
