using Kruty1918.Moyva.DTO;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator
{
    public class TileGenerator
    {
        private readonly IGridService _gridService;
        private readonly TileView _tileViewPrefab;

        public TileGenerator(TileView tileViewPrefab, IGridService gridService)
        {
            _tileViewPrefab = tileViewPrefab;
            _gridService = gridService;
        }

        public void GenerateTiles()
        {
            for (int x = 0; x < _gridService.GridWidth; x++)
            {
                for (int y = 0; y < _gridService.GridHeight; y++)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    TileData tileData = _gridService.GetTileData(position);
                    CreateTileView(position, tileData);
                }
            }
        }

        private void CreateTileView(Vector2Int position, TileData tileData)
        {
            TileView tileView = Object.Instantiate(_tileViewPrefab);
            tileView.transform.position = new Vector3(position.x, position.y, 0);
        }
    }
}