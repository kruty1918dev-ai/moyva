using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal interface ITileNeighborhoodFactory
    {
        TileNeighborhood Create(GraphLogicalTileMap map, Vector2Int cell);
    }

    internal sealed class TileNeighborhoodFactory : ITileNeighborhoodFactory
    {
        public TileNeighborhood Create(GraphLogicalTileMap map, Vector2Int cell)
        {
            return new TileNeighborhood(
                Get(map, cell.x, cell.y),
                Get(map, cell.x, cell.y + 1),
                Get(map, cell.x + 1, cell.y),
                Get(map, cell.x, cell.y - 1),
                Get(map, cell.x - 1, cell.y),
                Get(map, cell.x + 1, cell.y + 1),
                Get(map, cell.x + 1, cell.y - 1),
                Get(map, cell.x - 1, cell.y - 1),
                Get(map, cell.x - 1, cell.y + 1));
        }

        private static TileStackCell Get(GraphLogicalTileMap map, int x, int y)
            => map != null ? map.GetCellStack(x, y) : null;
    }
}
