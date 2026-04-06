using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Visuals;
using UnityEngine;
using Zenject;
using Kruty1918.Moyva.Signals;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MapVisualInstantiator : IMapInstantiator, IInitializable
    {
        private const int ObjectLayerSortingOrder = 10;

        private readonly IGridService _gridService;
        private readonly IMapDataGenerator _mapDataGenerator;
        private readonly TileRegistrySO _tileRegistry;
        private readonly IMapObjectRegistryService _objectRegistry;
        private readonly IUnitClassConfig _unitClassConfig;
        private readonly IUnitFactory _unitFactory;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly DiContainer _container;

        private Transform _tilesRoot;
        private Transform _objectsRoot;
        private Transform _buildingsRoot;
        private readonly Dictionary<string, TileTypeDefinition> _definitionsCache = new();
        private readonly SignalBus _signalBus;
        private GeneratedWorldData _currentWorldData;
        private GeneratedWorldData _pendingWorldData;

        public MapVisualInstantiator(
            TileRegistrySO tileRegistry,
            IMapObjectRegistryService objectRegistry,
            [InjectOptional] IUnitClassConfig unitClassConfig,
            [InjectOptional] IUnitFactory unitFactory,
            [InjectOptional] IBuildingRegistry buildingRegistry,
            IGridService gridService,
            IMapDataGenerator mapDataGenerator,
            DiContainer container,
            SignalBus signalBus)
        {
            _tileRegistry = tileRegistry;
            _objectRegistry = objectRegistry;
            _unitClassConfig = unitClassConfig;
            _unitFactory = unitFactory;
            _buildingRegistry = buildingRegistry;
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
            string[,] virtualBuildingMap = null;

            _mapDataGenerator.GenerateMapData(
                _gridService.GridWidth,
                _gridService.GridHeight,
                (biomes, objects, heightMap, buildings) =>
                {
                    virtualBiomeMap = biomes;
                    virtualObjectMap = objects;
                    finalHeightMap = heightMap;
                    virtualBuildingMap = buildings;
                });

            return new GeneratedWorldData
            {
                Width = _gridService.GridWidth,
                Height = _gridService.GridHeight,
                BiomeMap = virtualBiomeMap,
                ObjectMap = virtualObjectMap,
                HeightMap = finalHeightMap,
                BuildingMap = virtualBuildingMap,
            };
        }

        private void BuildWorldFromData(GeneratedWorldData worldData)
        {
            EnsureRoots();
            ClearRoot(_tilesRoot);
            ClearRoot(_objectsRoot);
            ClearRoot(_buildingsRoot);

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
                        CreateObjectLayerEntity(pos, objectId);
                    }

                    if (worldData.BuildingMap != null)
                    {
                        string buildingId = worldData.BuildingMap[x, y];
                        if (!string.IsNullOrEmpty(buildingId))
                        {
                            CreateBuildingView(pos, buildingId);
                        }
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
            if (_buildingsRoot == null) _buildingsRoot = new GameObject("BuildingsRoot").transform;
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

        private void CreateObjectLayerEntity(Vector2Int position, string layerEntityId)
        {
            if (_objectRegistry.TryGetDefinition(layerEntityId, out _))
            {
                CreateObjectView(position, layerEntityId, _objectsRoot, 0);

                _signalBus.Fire(new OnMapObjectSpawnedSignal
                {
                    ObjectId = layerEntityId,
                    Position = position
                });
                return;
            }

            if (_unitClassConfig?.GetConfig(layerEntityId) != null)
            {
                if (_unitFactory == null)
                {
                    Debug.LogError($"[MapVisualInstantiator] Для unit ID '{layerEntityId}' не знайдено IUnitFactory.");
                    return;
                }

                _unitFactory.CreateUnit(layerEntityId, position);
                return;
            }

            Debug.LogError($"[MapVisualInstantiator] Object layer ID '{layerEntityId}' не знайдено ні в MapObjectRegistry, ні в UnitRegistry.");
        }

        private void CreateObjectView(Vector2Int position, string objectId, Transform root, int sortingOrder)
        {
            if (!_objectRegistry.TryGetDefinition(objectId, out var objectDef))
            {
                Debug.LogError($"[MapVisualInstantiator] Не знайдено MapObjectDefinition для ID: {objectId}");
                return;
            }

            if (objectDef.VisualPrefab == null)
            {
                Debug.LogWarning($"[MapVisualInstantiator] Object '{objectId}' не має префабу. Позиція: {position}. Об'єкт пропущено.");
                return;
            }

            Vector3 worldPos = new Vector3(position.x, position.y, sortingOrder * 0.1f);
            Quaternion prefabRotation = objectDef.VisualPrefab.transform.rotation;

            var instance = _container.InstantiatePrefab(
                objectDef.VisualPrefab,
                worldPos,
                prefabRotation,
                root);

            instance.name = $"Obj_{objectId}_{position.x}_{position.y}";

            var tileView = instance.GetComponent<TileView>();
            if (tileView != null)
            {
                tileView.Setup(position);
            }

            ApplySortingOrder(instance, ObjectLayerSortingOrder);
        }

        private void CreateBuildingView(Vector2Int position, string buildingId)
        {
            if (_buildingRegistry == null)
            {
                return;
            }

            var def = _buildingRegistry.GetById(buildingId);
            if (def == null)
            {
                Debug.LogError($"[MapVisualInstantiator] Не знайдено BuildingDefinition для ID: {buildingId}");
                return;
            }

            if (def.Prefab == null)
            {
                Debug.LogWarning($"[MapVisualInstantiator] Building '{buildingId}' не має префабу.");
                return;
            }

            Vector3 worldPos = new Vector3(position.x, position.y, 0.1f);

            var instance = _container.InstantiatePrefab(
                def.Prefab,
                worldPos,
                def.Prefab.transform.rotation,
                _buildingsRoot);

            instance.name = $"Building_{buildingId}_{position.x}_{position.y}";

            var tileView = instance.GetComponent<TileView>();
            if (tileView != null)
            {
                tileView.Setup(position);
            }
        }

        private static void ApplySortingOrder(GameObject rootObject, int sortingOrder)
        {
            var sortingGroups = rootObject.GetComponentsInChildren<UnityEngine.Rendering.SortingGroup>(true);
            foreach (var sortingGroup in sortingGroups)
            {
                sortingGroup.sortingOrder = sortingOrder;
            }

            var spriteRenderers = rootObject.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var spriteRenderer in spriteRenderers)
            {
                spriteRenderer.sortingOrder = sortingOrder;
            }
        }
    }
}