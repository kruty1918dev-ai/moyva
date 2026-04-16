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
        private const int BuildingLayerMinSortingOrder = 5;

        private static readonly int ShoreDistanceProp = Shader.PropertyToID("_ShoreDistance");
        private static readonly int ShoreMaskProp = Shader.PropertyToID("_ShoreMask");
        private static readonly int ShoreFlowProp = Shader.PropertyToID("_ShoreFlow");
        private static readonly int ShoreCornersProp = Shader.PropertyToID("_ShoreCorners");
        private static readonly int TileRectProp = Shader.PropertyToID("_TileRect");
        private static readonly int NeighborMaskProp = Shader.PropertyToID("_NeighborMask");
        private static readonly int WaterNeighborMaskProp = Shader.PropertyToID("_WaterNeighborMask");
        private static readonly int NeighborRectNProp = Shader.PropertyToID("_NeighborRectN");
        private static readonly int NeighborRectEProp = Shader.PropertyToID("_NeighborRectE");
        private static readonly int NeighborRectSProp = Shader.PropertyToID("_NeighborRectS");
        private static readonly int NeighborRectWProp = Shader.PropertyToID("_NeighborRectW");
        private static readonly int TileIdMapProp = Shader.PropertyToID("_TileIdMap");
        private static readonly int ShoreDistMapProp = Shader.PropertyToID("_ShoreDistMap");
        private static readonly int TileUVLookupProp = Shader.PropertyToID("_TileUVLookup");
        private static readonly int MapSizeProp = Shader.PropertyToID("_MapSize");
        private static readonly int WaterTileIdProp = Shader.PropertyToID("_WaterTileId");
        private static readonly int TileCountProp = Shader.PropertyToID("_TileCount");

        private readonly IGridService _gridService;
        private readonly IMapDataGenerator _mapDataGenerator;
        private readonly TileRegistrySO _tileRegistry;
        private readonly IMapObjectRegistryService _objectRegistry;
        private readonly IUnitClassConfig _unitClassConfig;
        private readonly IUnitFactory _unitFactory;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly DiContainer _container;
        private readonly Material _waterMaterial;
        private readonly Material _tileBlendMaterial;
        private readonly TileTextureAtlasSO _tileAtlas;

        private Transform _tilesRoot;
        private Transform _objectsRoot;
        private Transform _buildingsRoot;
        private readonly Dictionary<string, TileTypeDefinition> _definitionsCache = new();
        private readonly SignalBus _signalBus;
        private GeneratedWorldData _currentWorldData;
        private GeneratedWorldData _pendingWorldData;
        private readonly HashSet<string> _waterTileIds = new();
        private Texture2D _tileIdMapTexture;
        private Texture2D _shoreDistMapTexture;
        private Texture2D _uvLookupTexture;

        public MapVisualInstantiator(
            TileRegistrySO tileRegistry,
            IMapObjectRegistryService objectRegistry,
            [InjectOptional] IUnitClassConfig unitClassConfig,
            [InjectOptional] IUnitFactory unitFactory,
            [InjectOptional] IBuildingRegistry buildingRegistry,
            IGridService gridService,
            IMapDataGenerator mapDataGenerator,
            DiContainer container,
            SignalBus signalBus,
            [Inject(Id = "WaterMaterial", Optional = true)] Material waterMaterial = null,
            [Inject(Id = "TileBlendMaterial", Optional = true)] Material tileBlendMaterial = null,
            [Inject(Optional = true)] TileTextureAtlasSO tileAtlas = null,
            [Inject(Id = "WaterTileIds", Optional = true)] string[] waterTileIds = null)
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
            _waterMaterial = waterMaterial;
            _tileBlendMaterial = tileBlendMaterial;
            _tileAtlas = tileAtlas;

            if (waterTileIds != null)
            {
                foreach (var waterTileId in waterTileIds)
                {
                    string baseId = GetBaseTileType(waterTileId);
                    if (!string.IsNullOrEmpty(baseId))
                        _waterTileIds.Add(baseId);
                }
            }

            if (_waterTileIds.Count == 0)
                _waterTileIds.Add("water");
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

            int w = worldData.Width;
            int h = worldData.Height;

            // Compute visual data from BiomeMap if not provided
            var shoreDistanceMap = worldData.ShoreDistanceMap;
            var shoreMask = worldData.ShoreMask;
            var neighborMask = worldData.NeighborMask;
            Vector2[,] shoreFlowMap = null;

            bool hasWaterMaterial = _waterMaterial != null;

            if (hasWaterMaterial && _tileAtlas != null)
            {
                if (!_tileAtlas.IsBuilt)
                {
                    _tileAtlas.BuildAtlas();
                }

                if (_tileAtlas.IsBuilt && _tileAtlas.Atlas != null)
                {
                    _waterMaterial.SetTexture("_AtlasTex", _tileAtlas.Atlas);
                }
            }

            bool hasBlendMaterial = false;
            if (_tileBlendMaterial != null && _tileAtlas != null)
            {
                if (!_tileAtlas.IsBuilt)
                {
                    // Atlas is built in editor tools; build at runtime as a safety fallback.
                    _tileAtlas.BuildAtlas();
                }

                if (_tileAtlas.IsBuilt && _tileAtlas.Atlas != null)
                {
                    _tileBlendMaterial.SetTexture("_AtlasTex", _tileAtlas.Atlas);
                    hasBlendMaterial = true;
                }
            }

            if (hasWaterMaterial && (shoreDistanceMap == null || shoreMask == null))
                ComputeWaterData(worldData.BiomeMap, w, h, out shoreDistanceMap, out shoreMask);

            if (hasWaterMaterial && shoreDistanceMap != null)
                shoreFlowMap = ComputeShoreFlowMap(worldData.BiomeMap, shoreDistanceMap, w, h);

            if (hasWaterMaterial && shoreDistanceMap != null)
                BuildGlobalWaterTextures(worldData.BiomeMap, shoreDistanceMap, w, h);

            if (hasBlendMaterial && neighborMask == null)
                neighborMask = ComputeNeighborMask(worldData.BiomeMap, w, h);

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);

                    string biomeId = worldData.BiomeMap[x, y];
                    if (!string.IsNullOrEmpty(biomeId))
                    {
                        var tileGO = CreateTileView(pos, biomeId, _tilesRoot, -1);

                        if (tileGO != null)
                        {
                            bool isWater = IsWaterTile(biomeId);

                            if (isWater && hasWaterMaterial)
                            {
                                ApplyWaterMaterial(tileGO);
                            }
                            else if (!isWater && hasBlendMaterial)
                            {
                                ApplyBlendMaterial(tileGO, biomeId, worldData.BiomeMap,
                                    neighborMask[x, y], ComputeWaterNeighborMask(worldData.BiomeMap, x, y, w, h),
                                    x, y, w, h);
                            }
                        }
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

            _currentWorldData = worldData.Clone();
            _signalBus.Fire(new WorldBuiltSignal());
            _signalBus.Fire(new WorldGeneratedDataSignal
            {
                Width = _currentWorldData.Width,
                Height = _currentWorldData.Height,
                TileMap = MapArrayUtils.CloneStringMap(_currentWorldData.BiomeMap),
                ObjectMap = MapArrayUtils.CloneStringMap(_currentWorldData.ObjectMap),
                HeightMap = MapArrayUtils.CloneFloatMap(_currentWorldData.HeightMap),
            });
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

        private GameObject CreateTileView(Vector2Int position, string tileId, Transform root, int sortingOrder)
        {
            if (!_definitionsCache.TryGetValue(tileId, out var tileType))
            {
                Debug.LogError($"[MapInstantiator] Не знайдено TileTypeDefinition для ID: {tileId}");
                return null;
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

            _gridService.SetTileData(position, tileId);
            return instance;
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

        private bool IsWaterTile(string tileId)
        {
            if (string.IsNullOrEmpty(tileId))
                return false;
            if (_waterTileIds.Contains(tileId))
                return true;
            int dash = tileId.IndexOf('-');
            return dash > 0 && _waterTileIds.Contains(tileId.Substring(0, dash));
        }

        private void ApplyWaterMaterial(GameObject tileGO)
        {
            var sr = tileGO.GetComponentInChildren<SpriteRenderer>();
            if (sr == null) return;
            sr.sharedMaterial = _waterMaterial;
        }

        private void BuildGlobalWaterTextures(string[,] biomeMap, float[,] shoreDistanceMap, int w, int h)
        {
            if (_waterMaterial == null) return;

            bool hasAtlas = _tileAtlas != null && _tileAtlas.IsBuilt;

            if (_tileIdMapTexture != null) Object.Destroy(_tileIdMapTexture);
            _tileIdMapTexture = new Texture2D(w, h, TextureFormat.RFloat, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            if (_shoreDistMapTexture != null) Object.Destroy(_shoreDistMapTexture);
            _shoreDistMapTexture = new Texture2D(w, h, TextureFormat.RFloat, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    string baseTile = GetBaseTileType(biomeMap[x, y]);
                    float tileIdx = hasAtlas ? _tileAtlas.GetTileIndex(baseTile) : -1f;
                    _tileIdMapTexture.SetPixel(x, y, new Color(tileIdx, 0, 0, 0));
                    float dist = shoreDistanceMap != null ? shoreDistanceMap[x, y] : 0f;
                    _shoreDistMapTexture.SetPixel(x, y, new Color(dist, 0, 0, 0));
                }
            }
            _tileIdMapTexture.Apply(false, false);
            _shoreDistMapTexture.Apply(false, false);

            int waterIdx = -1;
            if (hasAtlas)
            {
                foreach (var waterTileId in _waterTileIds)
                {
                    int idx = _tileAtlas.GetTileIndex(waterTileId);
                    if (idx >= 0)
                    {
                        waterIdx = idx;
                        break;
                    }
                }
            }
            int tileCount = hasAtlas ? Mathf.Max(1, _tileAtlas.TileCount) : 1;

            _waterMaterial.SetTexture(TileIdMapProp, _tileIdMapTexture);
            _waterMaterial.SetTexture(ShoreDistMapProp, _shoreDistMapTexture);
            _waterMaterial.SetVector(MapSizeProp, new Vector4(w, h, 0, 0));
            _waterMaterial.SetFloat(WaterTileIdProp, waterIdx);
            _waterMaterial.SetFloat(TileCountProp, tileCount);

            if (hasAtlas)
            {
                if (_uvLookupTexture != null) Object.Destroy(_uvLookupTexture);
                _uvLookupTexture = _tileAtlas.BuildUVLookupTexture();
                _waterMaterial.SetTexture(TileUVLookupProp, _uvLookupTexture);
            }
        }

        private static float SampleShoreDistance(float[,] shoreDistanceMap, int x, int y, int w, int h)
        {
            if (shoreDistanceMap == null)
                return 0f;
            if (x < 0 || x >= w || y < 0 || y >= h)
                return 0f;
            return shoreDistanceMap[x, y];
        }

        private static float AverageShoreDistances(float[,] shoreDistanceMap, int w, int h,
            int x0, int y0,
            int x1, int y1,
            int x2, int y2,
            int x3, int y3)
        {
            return (
                SampleShoreDistance(shoreDistanceMap, x0, y0, w, h) +
                SampleShoreDistance(shoreDistanceMap, x1, y1, w, h) +
                SampleShoreDistance(shoreDistanceMap, x2, y2, w, h) +
                SampleShoreDistance(shoreDistanceMap, x3, y3, w, h)) * 0.25f;
        }

        private void ApplyBlendMaterial(GameObject tileGO, string tileId, string[,] biomeMap,
            int neighborMaskValue, int waterNeighborMaskValue, int x, int y, int w, int h)
        {
            var sr = tileGO.GetComponentInChildren<SpriteRenderer>();
            if (sr == null) return;

            sr.sharedMaterial = _tileBlendMaterial;
            var block = new MaterialPropertyBlock();
            sr.GetPropertyBlock(block);

            string baseTileId = GetBaseTileType(tileId);
            block.SetVector(TileRectProp, _tileAtlas.GetUVRectVector(baseTileId));
            block.SetFloat(NeighborMaskProp, neighborMaskValue);
            block.SetFloat(WaterNeighborMaskProp, waterNeighborMaskValue);

            // N neighbor
            if ((neighborMaskValue & 1) != 0 && y + 1 < h)
                block.SetVector(NeighborRectNProp, _tileAtlas.GetUVRectVector(GetBaseTileType(biomeMap[x, y + 1])));
            // E neighbor
            if ((neighborMaskValue & 2) != 0 && x + 1 < w)
                block.SetVector(NeighborRectEProp, _tileAtlas.GetUVRectVector(GetBaseTileType(biomeMap[x + 1, y])));
            // S neighbor
            if ((neighborMaskValue & 4) != 0 && y - 1 >= 0)
                block.SetVector(NeighborRectSProp, _tileAtlas.GetUVRectVector(GetBaseTileType(biomeMap[x, y - 1])));
            // W neighbor
            if ((neighborMaskValue & 8) != 0 && x - 1 >= 0)
                block.SetVector(NeighborRectWProp, _tileAtlas.GetUVRectVector(GetBaseTileType(biomeMap[x - 1, y])));

            sr.SetPropertyBlock(block);
        }

        private void ComputeWaterData(string[,] biomeMap, int w, int h,
            out float[,] shoreDistanceMap, out int[,] shoreMask)
        {
            shoreDistanceMap = new float[w, h];
            shoreMask = new int[w, h];

            var isWater = new bool[w, h];
            var queue = new Queue<(int x, int y)>();

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    isWater[x, y] = IsWaterTile(biomeMap[x, y]);
                    if (!isWater[x, y])
                    {
                        shoreDistanceMap[x, y] = 0f;
                        queue.Enqueue((x, y));
                    }
                    else
                    {
                        shoreDistanceMap[x, y] = float.MaxValue;
                    }
                }
            }

            int[] dx = { 0, 1, 0, -1 };
            int[] dy = { 1, 0, -1, 0 };

            while (queue.Count > 0)
            {
                var (cx, cy) = queue.Dequeue();
                float nextDist = shoreDistanceMap[cx, cy] + 1f;
                if (nextDist > 10f) continue;

                for (int d = 0; d < 4; d++)
                {
                    int nx = cx + dx[d];
                    int ny = cy + dy[d];
                    if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;
                    if (nextDist < shoreDistanceMap[nx, ny])
                    {
                        shoreDistanceMap[nx, ny] = nextDist;
                        queue.Enqueue((nx, ny));
                    }
                }
            }

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (!isWater[x, y])
                    {
                        shoreDistanceMap[x, y] = 0f;
                        continue;
                    }

                    int bits = 0;
                    if (y + 1 >= h || !isWater[x, y + 1]) bits |= 1;
                    if (x + 1 >= w || !isWater[x + 1, y]) bits |= 2;
                    if (y - 1 < 0 || !isWater[x, y - 1]) bits |= 4;
                    if (x - 1 < 0 || !isWater[x - 1, y]) bits |= 8;

                    // Diagonal neighbors for corner foam
                    if ((x + 1 >= w || y + 1 >= h) || !isWater[x + 1, y + 1]) bits |= 16;  // NE
                    if ((x + 1 >= w || y - 1 < 0)  || !isWater[x + 1, y - 1]) bits |= 32;  // SE
                    if ((x - 1 < 0  || y - 1 < 0)  || !isWater[x - 1, y - 1]) bits |= 64;  // SW
                    if ((x - 1 < 0  || y + 1 >= h)  || !isWater[x - 1, y + 1]) bits |= 128; // NW

                    shoreMask[x, y] = bits;
                }
            }
        }

        private int[,] ComputeNeighborMask(string[,] biomeMap, int w, int h)
        {
            var mask = new int[w, h];
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    // Water tiles use a separate water shader — skip blend mask entirely.
                    if (IsWaterTile(biomeMap[x, y])) continue;

                    string current = GetBaseTileType(biomeMap[x, y]);
                    int bits = 0;
                    string nb;

                    nb = (y + 1 < h) ? biomeMap[x, y + 1] : null;
                    if (nb != null && !IsWaterTile(nb) && GetBaseTileType(nb) != current) bits |= 1;

                    nb = (x + 1 < w) ? biomeMap[x + 1, y] : null;
                    if (nb != null && !IsWaterTile(nb) && GetBaseTileType(nb) != current) bits |= 2;

                    nb = (y - 1 >= 0) ? biomeMap[x, y - 1] : null;
                    if (nb != null && !IsWaterTile(nb) && GetBaseTileType(nb) != current) bits |= 4;

                    nb = (x - 1 >= 0) ? biomeMap[x - 1, y] : null;
                    if (nb != null && !IsWaterTile(nb) && GetBaseTileType(nb) != current) bits |= 8;

                    mask[x, y] = bits;
                }
            }
            return mask;
        }

        private Vector2[,] ComputeShoreFlowMap(string[,] biomeMap, float[,] shoreDistanceMap, int w, int h)
        {
            var flowMap = new Vector2[w, h];
            int[] dx = { 0, 1, 0, -1, 1, 1, -1, -1 };
            int[] dy = { 1, 0, -1, 0, 1, -1, -1, 1 };

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (!IsWaterTile(biomeMap[x, y]))
                    {
                        flowMap[x, y] = Vector2.zero;
                        continue;
                    }

                    float currentDistance = shoreDistanceMap[x, y];
                    Vector2 accumulated = Vector2.zero;

                    for (int i = 0; i < dx.Length; i++)
                    {
                        int nx = x + dx[i];
                        int ny = y + dy[i];
                        if (nx < 0 || nx >= w || ny < 0 || ny >= h)
                            continue;

                        float neighborDistance = IsWaterTile(biomeMap[nx, ny])
                            ? shoreDistanceMap[nx, ny]
                            : 0f;

                        float slope = currentDistance - neighborDistance;
                        if (slope <= 0f)
                            continue;

                        Vector2 dir = new Vector2(dx[i], dy[i]).normalized;
                        accumulated += dir * slope;
                    }

                    if (accumulated.sqrMagnitude > 0.0001f)
                    {
                        flowMap[x, y] = accumulated.normalized;
                        continue;
                    }

                    // Fallback for isolated or perfectly flat cases.
                    int shoreMask = 0;
                    if (y + 1 >= h || !IsWaterTile(biomeMap[x, y + 1])) shoreMask |= 1;
                    if (x + 1 >= w || !IsWaterTile(biomeMap[x + 1, y])) shoreMask |= 2;
                    if (y - 1 < 0 || !IsWaterTile(biomeMap[x, y - 1])) shoreMask |= 4;
                    if (x - 1 < 0 || !IsWaterTile(biomeMap[x - 1, y])) shoreMask |= 8;

                    Vector2 fallback = Vector2.zero;
                    if ((shoreMask & 1) != 0) fallback += Vector2.up;
                    if ((shoreMask & 2) != 0) fallback += Vector2.right;
                    if ((shoreMask & 4) != 0) fallback += Vector2.down;
                    if ((shoreMask & 8) != 0) fallback += Vector2.left;
                    flowMap[x, y] = fallback.sqrMagnitude > 0.0001f ? fallback.normalized : Vector2.up;
                }
            }

            return flowMap;
        }

        private int ComputeWaterNeighborMask(string[,] biomeMap, int x, int y, int w, int h)
        {
            if (IsWaterTile(biomeMap[x, y]))
                return 0;

            int mask = 0;

            if (y + 1 < h && IsWaterTile(biomeMap[x, y + 1])) mask |= 1;
            if (x + 1 < w && IsWaterTile(biomeMap[x + 1, y])) mask |= 2;
            if (y - 1 >= 0 && IsWaterTile(biomeMap[x, y - 1])) mask |= 4;
            if (x - 1 >= 0 && IsWaterTile(biomeMap[x - 1, y])) mask |= 8;
            if (x + 1 < w && y + 1 < h && IsWaterTile(biomeMap[x + 1, y + 1])) mask |= 16;
            if (x + 1 < w && y - 1 >= 0 && IsWaterTile(biomeMap[x + 1, y - 1])) mask |= 32;
            if (x - 1 >= 0 && y - 1 >= 0 && IsWaterTile(biomeMap[x - 1, y - 1])) mask |= 64;
            if (x - 1 >= 0 && y + 1 < h && IsWaterTile(biomeMap[x - 1, y + 1])) mask |= 128;

            return mask;
        }

        private static string GetBaseTileType(string tileId)
        {
            if (string.IsNullOrEmpty(tileId)) return string.Empty;
            int dash = tileId.IndexOf('-');
            return dash > 0 ? tileId.Substring(0, dash) : tileId;
        }
    }
}