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
        private readonly DiContainer _container;

        private Transform _tilesRoot;
        private Transform _objectsRoot; // Окремий корінь для об'єктів (річок тощо)
        private readonly Dictionary<string, TileTypeDefinition> _definitionsCache = new();
        private readonly SignalBus _signalBus;
        private GeneratedWorldData _currentWorldData;
        private GeneratedWorldData _pendingWorldData;

        public MapVisualInstantiator(
            TileRegistrySO tileRegistry,
            IGridService gridService,
            IMapDataGenerator mapDataGenerator,
            DiContainer container,
            SignalBus signalBus)
        {
            _tileRegistry = tileRegistry;
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
            GeneratedWorldData worldData = _pendingWorldData;
            _pendingWorldData = null;

            if (worldData == null)
            {
                worldData = GenerateNewWorldData();
            }

            BuildWorldFromData(worldData);
        }

        internal void SetPendingWorldData(GeneratedWorldData data)
        {
            _pendingWorldData = data?.Clone();
        }

        internal bool TryGetCurrentWorldData(out GeneratedWorldData data)
        {
            data = _currentWorldData?.Clone();
            return data != null;
        }

        private GeneratedWorldData GenerateNewWorldData()
        {
            string[,] virtualBiomeMap = null;
            string[,] virtualObjectMap = null;
            float[,] finalHeightMap = null;

            _mapDataGenerator.GenerateMapData(
                _gridService.GridWidth,
                _gridService.GridHeight,
                (biomes, objects, heightMap) =>
                {
                    virtualBiomeMap = biomes;
                    virtualObjectMap = objects;
                    finalHeightMap = heightMap;
                });

            return new GeneratedWorldData
            {
                Width = _gridService.GridWidth,
                Height = _gridService.GridHeight,
                BiomeMap = virtualBiomeMap,
                ObjectMap = virtualObjectMap,
                HeightMap = finalHeightMap,
            };
        }

        private void BuildWorldFromData(GeneratedWorldData worldData)
        {
            EnsureRoots();
            ClearRoot(_tilesRoot);
            ClearRoot(_objectsRoot);

            for (int x = 0; x < worldData.Width; x++)
            {
                for (int y = 0; y < worldData.Height; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);

                    string biomeId = worldData.BiomeMap[x, y];
                    if (!string.IsNullOrEmpty(biomeId))
                        CreateTileView(pos, biomeId, _tilesRoot, -1);

                    string objectId = worldData.ObjectMap[x, y];
                    if (!string.IsNullOrEmpty(objectId))
                    {
                        CreateTileView(pos, objectId, _objectsRoot, 0);
                        _signalBus.Fire(new OnMapObjectSpawnedSignal
                        {
                            ObjectId = objectId,
                            Position = pos
                        });
                    }
                }
            }

            _currentWorldData = worldData.Clone();
            _signalBus.Fire(new WorldBuiltSignal());
        }

        private void EnsureRoots()
        {
            if (_tilesRoot == null) _tilesRoot = new GameObject("TilesRoot").transform;
            if (_objectsRoot == null) _objectsRoot = new GameObject("ObjectsRoot").transform;
        }

        private static void ClearRoot(Transform root)
        {
            if (root == null)
                return;

            for (int i = root.childCount - 1; i >= 0; i--)
            {
                var child = root.GetChild(i).gameObject;
                Object.Destroy(child);
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
    }
}