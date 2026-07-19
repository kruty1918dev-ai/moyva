using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal interface IResolvedTileCompositionResolver
    {
        ResolvedTileComposition Resolve(
            Vector2Int cell,
            TileNeighborhood neighborhood,
            float lowestLayerHeight = 0f);
    }
}
