using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Visuals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator
{
    public class TileGenerator
    {
        private readonly IGridService _gridService;
        private readonly TileRegistrySO _tileRegistry;
        private readonly DiContainer _container;

        // Бажано спавнити тайли в якийсь спільний Parent, щоб не смітити в ієрархії
        private Transform _tilesRoot;

        public TileGenerator(TileRegistrySO tileRegistry, IGridService gridService, DiContainer container)
        {
            _tileRegistry = tileRegistry;
            _gridService = gridService;
            _container = container;
        }

        public void GenerateTiles()
        {
            // Створюємо контейнер для тайлів, якщо його ще немає
            if (_tilesRoot == null) _tilesRoot = new GameObject("TilesRoot").transform;

            for (int x = 0; x < _gridService.GridWidth; x++)
            {
                for (int y = 0; y < _gridService.GridHeight; y++)
                {
                    Vector2Int position = new Vector2Int(x, y);
                    CreateTileView(position);
                }
            }
        }

        private void CreateTileView(Vector2Int position)
        {
            // Використовуємо Zenject для створення префаба з автоматичною ін'єкцією
            Vector3 worldPos = new Vector3(position.x, position.y, 0);

            var tileType = GetRandomTile();
            var tilePrefab = tileType.VisualPrefab;

            var instance = _container.InstantiatePrefab(
                tilePrefab,
                worldPos,
                Quaternion.identity,
                _tilesRoot);

            instance.name = tileType.Id;

            // Тепер TileView вже МАЄ ін'єктований SignalBus і готовий до роботи
            var tileView = instance.GetComponent<TileView>();
            if (tileView == null)
            {
                Debug.LogError("Tile prefab does not have a TileView component!");
                return;
            }
            tileView.Setup(position);

            // Ініціалізуємо дані тайла в GridService
            var tileData = _gridService.GetTileData(position);
            tileData.TileTypeId = tileType.Id;
            _gridService.SetTileData(position, tileData);
        }

        private TileTypeDefinition GetRandomTile()
        {
            return _tileRegistry.Definitions[Random.Range(0, _tileRegistry.Definitions.Length)];
        }
    }
}