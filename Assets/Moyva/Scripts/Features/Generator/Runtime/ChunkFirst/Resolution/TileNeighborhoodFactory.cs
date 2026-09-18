using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal interface ITileNeighborhoodFactory
    {
        TileNeighborhood Create(LogicalTileMap map, Vector2Int cell);
    }

    internal sealed class TileNeighborhoodFactory : ITileNeighborhoodFactory
    {
        public TileNeighborhood Create(LogicalTileMap map, Vector2Int cell)
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

        private static TileStackCell Get(LogicalTileMap map, int x, int y)
            => map != null ? map.GetCellStack(x, y) : null;
    }
}
