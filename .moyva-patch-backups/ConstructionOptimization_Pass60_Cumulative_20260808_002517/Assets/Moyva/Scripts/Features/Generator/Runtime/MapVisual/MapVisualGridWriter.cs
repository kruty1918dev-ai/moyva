using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MapVisualGridWriter : IMapVisualGridWriter
    {
        private readonly IGridService _gridService;
        private readonly IMapVisualTileIdResolver _tileIds;

        public MapVisualGridWriter(IGridService gridService, IMapVisualTileIdResolver tileIds)
        {
            _gridService = gridService;
            _tileIds = tileIds;
        }

        public int Write(GeneratedWorldData worldData)
        {
            EnsureGridMatchesWorld(worldData);
            int count = WriteMap(worldData?.BiomeMap, false);
            WriteMap(worldData?.ObjectMap, true);
            return count;
        }

        private int WriteMap(string[,] map, bool resolveTileIds)
        {
            if (map == null)
                return 0;

            int filled = 0;
            for (int x = 0; x < map.GetLength(0); x++)
            for (int y = 0; y < map.GetLength(1); y++)
            {
                string id = map[x, y];
                if (string.IsNullOrEmpty(id))
                    continue;
                if (resolveTileIds && !_tileIds.TryResolve(id, out _, out id))
                    continue;

                map[x, y] = id;
                _gridService.SetTileData(new Vector2Int(x, y), id);
                filled++;
            }

            return filled;
        }

        private void EnsureGridMatchesWorld(GeneratedWorldData worldData)
        {
            int width = Mathf.Max(1, worldData.Width);
            int height = Mathf.Max(1, worldData.Height);
            if (_gridService.GridWidth == width && _gridService.GridHeight == height)
                return;

            if (_gridService is IGridResizeService resizeService)
                resizeService.Resize(width, height);
            else
                Debug.LogWarning($"[MapVisualInstantiator] Grid size {_gridService.GridWidth}x{_gridService.GridHeight} does not match world size {width}x{height}, and the grid service cannot resize.");
        }
    }
}
