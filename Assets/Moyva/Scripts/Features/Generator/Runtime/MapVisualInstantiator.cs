using System.Collections.Generic;
using System.Threading.Tasks;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Generator.API; // Додано для IMapDataGenerator
using Kruty1918.Moyva.Visuals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal class MapVisualInstantiator : IMapInstantiator, IInitializable
    {
        private readonly IGridService _gridService;
        private readonly IMapDataGenerator _mapDataGenerator;
        private readonly TileRegistrySO _tileRegistry;
        private readonly DiContainer _container;

        private Transform _tilesRoot;
        private readonly Dictionary<string, TileTypeDefinition> _definitionsCache = new();

        public MapVisualInstantiator(
            TileRegistrySO tileRegistry,
            IGridService gridService,
            IMapDataGenerator mapDataGenerator, // Додано в конструктор
            DiContainer container)
        {
            _tileRegistry = tileRegistry;
            _gridService = gridService;
            _mapDataGenerator = mapDataGenerator;
            _container = container;
        }

        public void Initialize()
        {
            // Кешуємо дефініції для швидкого пошуку за ID
            foreach (var def in _tileRegistry.Definitions)
            {
                _definitionsCache[def.Id] = def;
            }
        }

        public async Task BuildWorldAsync()
        {
            // 1. Отримуємо матрицю ідентифікаторів з конвеєра (Шум -> Біоми -> Фічі)
            string[,] virtualMap = await _mapDataGenerator.GenerateMapDataAsync(
                _gridService.GridWidth,
                _gridService.GridHeight);

            if (_tilesRoot == null)
                _tilesRoot = new GameObject("TilesRoot").transform;

            // 2. Створюємо візуальні об'єкти на основі отриманих даних
            for (int x = 0; x < _gridService.GridWidth; x++)
            {
                for (int y = 0; y < _gridService.GridHeight; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    string tileId = virtualMap[x, y];

                    CreateTileView(pos, tileId);
                }
            }
        }

        private void CreateTileView(Vector2Int position, string tileId)
        {
            // Шукаємо конфігурацію тайла за його ID
            if (!_definitionsCache.TryGetValue(tileId, out var tileType))
            {
                Debug.LogError($"[TileGenerator] Не вдалося знайти TileTypeDefinition для ID: {tileId}");
                return;
            }

            Vector3 worldPos = new Vector3(position.x, position.y, 0);
            var tilePrefab = tileType.VisualPrefab;

            // Створюємо інстанс через Zenject (ін'єкція в TileView відбудеться автоматично)
            var instance = _container.InstantiatePrefab(
                tilePrefab,
                worldPos,
                Quaternion.identity,
                _tilesRoot);

            instance.name = $"Tile_{tileId}_{position.x}_{position.y}";

            var tileView = instance.GetComponent<TileView>();
            if (tileView != null)
            {
                tileView.Setup(position);
            }

            // Оновлюємо дані в GridService, щоб логіка (Pathfinding тощо) знала, що тут лежить
            var tileData = _gridService.GetTileData(position);
            tileData.TileTypeId = tileId;
            _gridService.SetTileData(position, tileData);
        }
    }
}