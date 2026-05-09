using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Visuals;
using UnityEngine;
using Zenject;
using Kruty1918.Moyva.Signals;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MapVisualInstantiator : IMapInstantiator, IInitializable, System.IDisposable
    {
        private const int ObjectLayerSortingOrder = 10;
        private const int BuildingLayerMinSortingOrder = 5;

        private readonly IGridService _gridService;
        private readonly IMapDataGenerator _mapDataGenerator;
        private readonly TileRegistrySO _tileRegistry;
        private readonly IMapObjectRegistryService _objectRegistry;
        private readonly IMapObjectVisualRegistryService _mapObjectVisualRegistryService;
        private readonly IUnitClassConfig _unitClassConfig;
        private readonly IUnitFactory _unitFactory;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly DiContainer _container;

        private Transform _tilesRoot;
        private Transform _objectsRoot;
        private Transform _buildingsRoot;
        private Transform _layersRoot;
        private readonly Dictionary<string, TileTypeDefinition> _definitionsCache = new();
        private readonly SignalBus _signalBus;
        private GeneratedWorldData _currentWorldData;
        private GeneratedWorldData _pendingWorldData;
        private readonly GraphBasedMapDataGenerator _graphBasedGenerator;
        private readonly ShoreMaskPrepass _shoreMaskPrepass;
        private readonly WaterLayerMaterialSettings _waterLayerMaterialSettings;

        public MapVisualInstantiator(
            TileRegistrySO tileRegistry,
            IMapObjectRegistryService objectRegistry,
            IMapObjectVisualRegistryService mapObjectVisualRegistryService,
            [InjectOptional] IUnitClassConfig unitClassConfig,
            [InjectOptional] IUnitFactory unitFactory,
            [InjectOptional] IBuildingRegistry buildingRegistry,
            IGridService gridService,
            IMapDataGenerator mapDataGenerator,
            DiContainer container,
            SignalBus signalBus,
            [InjectOptional] GraphBasedMapDataGenerator graphBasedGenerator,
            [InjectOptional] ShoreMaskPrepass shoreMaskPrepass,
            [InjectOptional] WaterLayerMaterialSettings waterLayerMaterialSettings = null)
        {
            _tileRegistry = tileRegistry;
            _objectRegistry = objectRegistry;
            _mapObjectVisualRegistryService = mapObjectVisualRegistryService;
            _unitClassConfig = unitClassConfig;
            _unitFactory = unitFactory;
            _buildingRegistry = buildingRegistry;
            _gridService = gridService;
            _mapDataGenerator = mapDataGenerator;
            _container = container;
            _signalBus = signalBus;
            _graphBasedGenerator = graphBasedGenerator;
            _shoreMaskPrepass = shoreMaskPrepass;
            _waterLayerMaterialSettings = waterLayerMaterialSettings;
        }

        public void Initialize()
        {
            foreach (var def in _tileRegistry.Definitions)
            {
                _definitionsCache[def.Id] = def;
            }

            _signalBus.Subscribe<WorldSpawnPositionsSignal>(OnWorldSpawnPositions);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<WorldSpawnPositionsSignal>(OnWorldSpawnPositions);
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

            int width = ResolveWidth(virtualBiomeMap, virtualObjectMap, finalHeightMap, virtualBuildingMap);
            int height = ResolveHeight(virtualBiomeMap, virtualObjectMap, finalHeightMap, virtualBuildingMap);

            var data = new GeneratedWorldData
            {
                Width = width,
                Height = height,
                BiomeMap = virtualBiomeMap,
                ObjectMap = virtualObjectMap,
                HeightMap = finalHeightMap,
                BuildingMap = virtualBuildingMap,
            };

            ApplyLaunchMetadata(data);
            return data;
        }

        private static int ResolveWidth(string[,] biomeMap, string[,] objectMap, float[,] heightMap, string[,] buildingMap)
        {
            if (biomeMap != null) return biomeMap.GetLength(0);
            if (objectMap != null) return objectMap.GetLength(0);
            if (heightMap != null) return heightMap.GetLength(0);
            if (buildingMap != null) return buildingMap.GetLength(0);
            return 0;
        }

        private static int ResolveHeight(string[,] biomeMap, string[,] objectMap, float[,] heightMap, string[,] buildingMap)
        {
            if (biomeMap != null) return biomeMap.GetLength(1);
            if (objectMap != null) return objectMap.GetLength(1);
            if (heightMap != null) return heightMap.GetLength(1);
            if (buildingMap != null) return buildingMap.GetLength(1);
            return 0;
        }

        private static void ApplyLaunchMetadata(GeneratedWorldData data)
        {
            if (data == null || !GameLaunchContext.HasWorldSettings)
                return;

            data.WorldName = GameLaunchContext.WorldName;
            data.Seed = GameLaunchContext.Seed;
            data.Size = GameLaunchContext.Size;
            data.MapType = GameLaunchContext.MapType;
            data.Difficulty = GameLaunchContext.Difficulty;
        }

        private void BuildWorldFromData(GeneratedWorldData worldData)
        {
            if (worldData == null)
            {
                Debug.LogError("[MapVisualInstantiator] BuildWorldFromData received null world data.");
                return;
            }

            EnsureGridMatchesWorld(worldData);
            EnsureRoots();
            _mapObjectVisualRegistryService?.Clear();
            ClearRoot(_tilesRoot);
            ClearRoot(_objectsRoot);
            ClearRoot(_buildingsRoot);
            ClearRoot(_layersRoot);

            // Ініціалізуємо LayerData до основного проходу, щоб знати чи працюємо у layer-only режимі.
            if (worldData.LayerData == null || worldData.LayerData.Length == 0)
                worldData.LayerData = _graphBasedGenerator?.LastLayerData;

            bool useLayerOnlyTiles = _graphBasedGenerator != null
                && _graphBasedGenerator.LastBiomeMapDerivedFromLayers
                && worldData.LayerData != null
                && worldData.LayerData.Length > 0;

            for (int x = 0; x < worldData.Width; x++)
            {
                for (int y = 0; y < worldData.Height; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);

                    string biomeId = worldData.BiomeMap[x, y];
                    if (!string.IsNullOrEmpty(biomeId))
                    {
                        if (useLayerOnlyTiles)
                            _gridService.SetTileData(pos, biomeId);
                        else
                            CreateTileView(pos, biomeId, _tilesRoot, -1);
                    }

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

            // Шари рендеримо після створення тайлів/об'єктів.
            ApplyLayerData(worldData);

            _currentWorldData = worldData.Clone();
            _signalBus.Fire(new WorldBuiltSignal());
            FireSavedSpawnPositions(_currentWorldData);
            _signalBus.Fire(new WorldGeneratedDataSignal
            {
                Width = _currentWorldData.Width,
                Height = _currentWorldData.Height,
                TileMap = MapArrayUtils.CloneStringMap(_currentWorldData.BiomeMap),
                ObjectMap = MapArrayUtils.CloneStringMap(_currentWorldData.ObjectMap),
                HeightMap = MapArrayUtils.CloneFloatMap(_currentWorldData.HeightMap),
            });
        }

        private void OnWorldSpawnPositions(WorldSpawnPositionsSignal signal)
        {
            if (_currentWorldData == null || signal.Assignments == null || signal.Assignments.Length == 0)
                return;

            _currentWorldData.SpawnPositions = (SpawnPositionAssignment[])signal.Assignments.Clone();
        }

        private void FireSavedSpawnPositions(GeneratedWorldData worldData)
        {
            if (worldData?.SpawnPositions == null || worldData.SpawnPositions.Length == 0)
                return;

            _signalBus.Fire(new WorldSpawnPositionsSignal
            {
                Assignments = (SpawnPositionAssignment[])worldData.SpawnPositions.Clone(),
            });
        }

        private void EnsureGridMatchesWorld(GeneratedWorldData worldData)
        {
            int width = Mathf.Max(1, worldData.Width);
            int height = Mathf.Max(1, worldData.Height);
            if (_gridService.GridWidth == width && _gridService.GridHeight == height)
                return;

            if (_gridService is IGridResizeService resizeService)
            {
                resizeService.Resize(width, height);
                return;
            }

            Debug.LogWarning($"[MapVisualInstantiator] Grid size {_gridService.GridWidth}x{_gridService.GridHeight} does not match world size {width}x{height}, and the grid service cannot resize. World build may fail if tiles exceed grid bounds.");
        }

        private void ApplyLayerData(GeneratedWorldData worldData)
        {
            var layerData = worldData.LayerData;

            if (layerData == null || layerData.Length == 0)
                return;

            // Список матеріалів що мають _LandMaskTex — в них записуємо RT після побудови шарів.
            var waterMaterials = new List<Material>();
            var shoreMaskSources = new List<ShoreMaskLayerSource>();

            for (int i = 0; i < layerData.Length; i++)
            {
                if (layerData[i].TileTexture == null)
                {
                    Debug.LogWarning($"[MapVisualInstantiator] Layer '{layerData[i].LayerTileID}' has null texture. Skipping.");
                    continue;
                }

                GameObject layerObj = new GameObject($"Layer_{i}_{layerData[i].LayerTileID}");
                layerObj.transform.SetParent(_layersRoot, false);

                SpriteRenderer spriteRenderer = layerObj.AddComponent<SpriteRenderer>();

                var texture = layerData[i].TileTexture;
                var sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    Vector2.zero,
                    1f);

                spriteRenderer.sprite = sprite;
                spriteRenderer.sortingLayerName = string.IsNullOrEmpty(layerData[i].SortingLayerName)
                    ? "Default"
                    : layerData[i].SortingLayerName;
                spriteRenderer.sortingOrder = layerData[i].SortingOrder;

                var layerShader = layerData[i].LayerShader;
                if (layerShader == null && !string.IsNullOrEmpty(layerData[i].LayerShaderName))
                    layerShader = Shader.Find(layerData[i].LayerShaderName);

                if (layerShader != null)
                {
                    var mat = new Material(layerShader) { mainTexture = texture };
                    if (mat.HasProperty("_LandMaskTex"))
                        _waterLayerMaterialSettings?.ApplyTo(mat);
                    spriteRenderer.material = mat;

                    // Збираємо матеріали що мають _LandMaskTex — RT буде передано після побудови всіх шарів.
                    if (mat.HasProperty("_LandMaskTex"))
                        waterMaterials.Add(mat);
                }
                else if (!string.IsNullOrEmpty(layerData[i].LayerShaderName))
                {
                    Debug.LogWarning($"[MapVisualInstantiator] Shader '{layerData[i].LayerShaderName}' not found for layer '{layerData[i].LayerTileID}'. Using default SpriteRenderer material.");
                }

                // Для сітки, де центри тайлів лежать у цілих координатах (x, y),
                // шар із pivot (0,0) треба зсунути на пів тайла вліво/вниз,
                // інакше центри текселів потрапляють у .5 і з'являється візуальний зсув.
                layerObj.transform.position = new Vector3(-0.5f, -0.5f, 0f);

                // Масштабуємо спрайт точно до розміру згенерованого світу незалежно від ppu/розміру текстури.
                float spriteWorldWidth = texture.width;
                float spriteWorldHeight = texture.height;

                float scaleX = spriteWorldWidth > 0f ? worldData.Width / spriteWorldWidth : 1f;
                float scaleY = spriteWorldHeight > 0f ? worldData.Height / spriteWorldHeight : 1f;
                layerObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);

                bool isWaterLayer = spriteRenderer.sharedMaterial != null
                    && spriteRenderer.sharedMaterial.HasProperty("_LandMaskTex");

                shoreMaskSources.Add(new ShoreMaskLayerSource
                {
                    Texture = texture,
                    LayerTransform = layerObj.transform,
                    BasePosition = layerObj.transform.position,
                    BaseScale = layerObj.transform.localScale,
                    IsWater = isWaterLayer,
                });
            }

            // Якщо є хоча б один матеріал з _LandMaskTex і ShoreMaskPrepass доступний —
            // рендеримо RT-маску суші на GPU і передаємо у всі водні матеріали.
            if (waterMaterials.Count > 0 && _shoreMaskPrepass != null)
            {
                // Розмір RT — розмір мапи (1 піксель = 1 тайл).
                _shoreMaskPrepass.RebuildMask(
                    shoreMaskSources,
                    waterMaterials,
                    worldData.BiomeMap,
                    worldData.Width,
                    worldData.Height);
            }
        }

        private void EnsureRoots()
        {
            if (_tilesRoot == null) _tilesRoot = new GameObject("TilesRoot").transform;
            if (_objectsRoot == null) _objectsRoot = new GameObject("ObjectsRoot").transform;
            if (_buildingsRoot == null) _buildingsRoot = new GameObject("BuildingsRoot").transform;
            if (_layersRoot == null) _layersRoot = new GameObject("LayersRoot").transform;
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

            // Fallback: object layer може містити tile ID (наприклад, river), який
            // має перезаписати базовий біом у GridService.
            if (_definitionsCache.ContainsKey(layerEntityId))
            {
                CreateTileView(position, layerEntityId, _objectsRoot, 0);
                return;
            }

            Debug.LogWarning($"[MapVisualInstantiator] Object layer ID '{layerEntityId}' не знайдено ні в MapObjectRegistry, ні в UnitRegistry.");
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
            _mapObjectVisualRegistryService?.Register(objectId, position, instance);
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

            EnsureBuildingSortingOrder(instance, BuildingLayerMinSortingOrder);
        }

        private static void EnsureBuildingSortingOrder(GameObject rootObject, int minOrder)
        {
            var spriteRenderers = rootObject.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (var sr in spriteRenderers)
            {
                if (sr.sortingOrder < minOrder)
                    sr.sortingOrder = minOrder;
            }

            var sortingGroups = rootObject.GetComponentsInChildren<UnityEngine.Rendering.SortingGroup>(true);
            foreach (var sg in sortingGroups)
            {
                if (sg.sortingOrder < minOrder)
                    sg.sortingOrder = minOrder;
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