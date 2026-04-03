using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Visuals;
using UnityEngine;
using Zenject;
using Kruty1918.Moyva.Signals;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MapVisualInstantiator : IMapInstantiator, IInitializable
    {
        private readonly IGridService _gridService;
        private readonly IMapDataGenerator _mapDataGenerator;
        private readonly TileRegistrySO _tileRegistry;
        private readonly IMapObjectRegistryService _objectRegistry;
        private readonly DiContainer _container;

        private Transform _tilesRoot;
        private Transform _objectsRoot; // Окремий корінь для об'єктів (річок тощо)
        private readonly Dictionary<string, TileTypeDefinition> _definitionsCache = new();
        private readonly SignalBus _signalBus;

        public MapVisualInstantiator(
            TileRegistrySO tileRegistry,
            IMapObjectRegistryService objectRegistry,
            IGridService gridService,
            IMapDataGenerator mapDataGenerator,
            DiContainer container,
            SignalBus signalBus)
        {
            _tileRegistry = tileRegistry;
            _objectRegistry = objectRegistry;
            _gridService = gridService;
            _mapDataGenerator = mapDataGenerator;
            _container = container;
            _signalBus = signalBus; 
        }

        public void Initialize()
        {
            foreach (var def in _tileRegistry.Definitions)
            {
                _definitionsCache[def.Id] = def;
            }
        }

        public void BuildWorld()
        {
            string[,] virtualBiomeMap = null;
            string[,] virtualObjectMap = null;
            float[,] finalHeightMap = null;
            // 1. Отримуємо дві карти з генератора через OnComplete
            _mapDataGenerator.GenerateMapData(
                _gridService.GridWidth,
                _gridService.GridHeight,
                (biomes, objects, heightMap) =>
                {
                    virtualBiomeMap = biomes;
                    virtualObjectMap = objects;
                    finalHeightMap = heightMap;
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
                        CreateObjectView(pos, objectId, _objectsRoot, 0);

                        // Сповіщаємо ObjectsMap про статичний обʼєкт карти
                        _signalBus.Fire(new OnMapObjectSpawnedSignal
                        {
                            ObjectId = objectId,
                            Position = pos
                        });
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
            // Логіка: якщо ми спавнимо об'єкт, він стає пріоритетним типом для цієї клітинки в логіці
            _gridService.SetTileData(position, tileId);
        }

        private void CreateObjectView(Vector2Int position, string objectId, Transform root, int sortingOrder)
        {
            if (!_objectRegistry.TryGetDefinition(objectId, out var objectDef))
            {
                Debug.LogError($"[MapVisualInstantiator] Не знайдено MapObjectDefinition для ID: {objectId}");
                return;
            }

            Vector3 worldPos = new Vector3(position.x, position.y, sortingOrder * 0.1f);

            var instance = _container.InstantiatePrefab(
                objectDef.VisualPrefab,
                worldPos,
                Quaternion.identity,
                root);

            instance.name = $"Obj_{objectId}_{position.x}_{position.y}";

            var tileView = instance.GetComponent<TileView>();
            if (tileView != null)
            {
                tileView.Setup(position);
            }
        }
    }
}