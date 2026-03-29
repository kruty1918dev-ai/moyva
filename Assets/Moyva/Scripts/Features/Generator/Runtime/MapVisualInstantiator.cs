using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Visuals;
using UnityEngine;
using Zenject;
using System.Collections;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal class MapVisualInstantiator : IMapInstantiator, IInitializable
    {
        private readonly IGridService _gridService;
        private readonly IMapDataGenerator _mapDataGenerator;
        private readonly TileRegistrySO _tileRegistry;
        private readonly DiContainer _container;

        private Transform _tilesRoot;
        private Transform _objectsRoot; // Окремий корінь для об'єктів (річок тощо)
        private readonly Dictionary<string, TileTypeDefinition> _definitionsCache = new();

        public MapVisualInstantiator(
            TileRegistrySO tileRegistry,
            IGridService gridService,
            IMapDataGenerator mapDataGenerator,
            DiContainer container)
        {
            _tileRegistry = tileRegistry;
            _gridService = gridService;
            _mapDataGenerator = mapDataGenerator;
            _container = container;
        }

        public void Initialize()
        {
            foreach (var def in _tileRegistry.Definitions)
            {
                _definitionsCache[def.Id] = def;
            }
        }

        public IEnumerator BuildWorldRoutine()
        {
            string[,] virtualBiomeMap = null;
            string[,] virtualObjectMap = null;

            // 1. Отримуємо дві карти з генератора через OnComplete
            yield return _mapDataGenerator.GenerateMapDataRoutine(
                _gridService.GridWidth,
                _gridService.GridHeight,
                (biomes, objects) => 
                {
                    virtualBiomeMap = biomes;
                    virtualObjectMap = objects;
                });

            if (_tilesRoot == null) _tilesRoot = new GameObject("TilesRoot").transform;
            if (_objectsRoot == null) _objectsRoot = new GameObject("ObjectsRoot").transform;

            // 2. Проходимо по сітці
            for (int x = 0; x < _gridService.GridWidth; x++)
            {
                for (int y = 0; y < _gridService.GridHeight; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);

                    // Спавнимо основний тайл біому (трава, пісок тощо)
                    string biomeId = virtualBiomeMap[x, y];
                    if (!string.IsNullOrEmpty(biomeId))
                    {
                        CreateTileView(pos, biomeId, _tilesRoot, -1); // LayerIndex -1 для фону
                    }

                    // Спавнимо об'єкт поверх (якщо він є в цій клітинці)
                    string objectId = virtualObjectMap[x, y];
                    if (!string.IsNullOrEmpty(objectId))
                    {
                        // Об'єкти спавнимо в _objectsRoot з трохи меншим Z, щоб вони були зверху
                        CreateTileView(pos, objectId, _objectsRoot, 0); 
                    }
                }
            }
        }

        private void CreateTileView(Vector2Int position, string tileId, Transform root, int sortingOrder)
        {
            if (!_definitionsCache.TryGetValue(tileId, out var tileType))
            {
                Debug.LogError($"[MapInstantiator] Не знайдено TileTypeDefinition для ID: {tileId}");
                return;
            }

            // Використовуємо sortingOrder для Z, щоб річка точно була над травою
            Vector3 worldPos = new Vector3(position.x, position.y, sortingOrder * 0.1f);
            var tilePrefab = tileType.VisualPrefab;

            var instance = _container.InstantiatePrefab(
                tilePrefab,
                worldPos,
                Quaternion.identity,
                root);

            instance.name = $"{(sortingOrder < 0 ? "Tile" : "Obj")}_{tileId}_{position.x}_{position.y}";

            var tileView = instance.GetComponent<TileView>();
            if (tileView != null)
            {
                tileView.Setup(position);
            }

            // Оновлюємо дані в GridService
            // Якщо це об'єкт (річка), він може перезаписувати властивості прохідності клітинки
            var tileData = _gridService.GetTileData(position);
            
            // Логіка: якщо ми спавнимо об'єкт, він стає пріоритетним типом для цієї клітинки в логіці
            tileData.TileTypeId = tileId; 
            _gridService.SetTileData(position, tileData);
        }
    }
}