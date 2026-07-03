using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Diagnostics.API;
using Kruty1918.Moyva.Diagnostics.Runtime.Flows;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
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
        private const string GeneratorBootDiagTag = "[MoyvaGeneratorBootDiag]";
        private const string DirectDiagTag = "[MoyvaDirectStartDiag]";
        private const string WorldGenDiagTag = "[MoyvaWorldGenDiag]";
        private const int ObjectLayerSortingOrder = 10;
        private const int BuildingLayerMinSortingOrder = 5;

        private readonly IGridService _gridService;
        private readonly IGridProjection _gridProjection;
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
        private readonly HashSet<string> _loggedTileFallbacks = new();
        private readonly Dictionary<Vector2Int, float> _terrainSurfaceYByPosition = new();
        private readonly SignalBus _signalBus;
        private GeneratedWorldData _currentWorldData;
        private GeneratedWorldData _pendingWorldData;
        private readonly GraphTwcMapDataGenerator _graphTwcGenerator;
        private readonly WaterLayerMaterialSettings _waterLayerMaterialSettings;
        private readonly TileWorldCreatorWorldBuildBridge _tileWorldCreatorBridge;
        private readonly IWorldGenerationDiagnostics _worldDiagnostics;
        private readonly ISaveLoadDiagnostics _saveLoadDiagnostics;
        private readonly ISaveLoadDiagnosticsSession _saveLoadDiagnosticsSession;
        private readonly List<Sprite> _runtimeLayerSprites = new List<Sprite>();
        private readonly List<Material> _runtimeLayerMaterials = new List<Material>();

        internal bool HasPendingWorldData => _pendingWorldData != null;
        internal Vector2Int DiagnosticGridSize => new Vector2Int(_gridService.GridWidth, _gridService.GridHeight);
        internal string DiagnosticMapDataGeneratorTypeName => _mapDataGenerator?.GetType().Name ?? "null";
        internal bool HasGraphGenerator => _graphTwcGenerator != null;
        internal bool HasSceneGeneratorConfiguration => _graphTwcGenerator?.HasGraphAsset ?? false;
        internal bool HasSharedMapSize => _graphTwcGenerator?.HasSharedMapSize ?? false;
        internal string DiagnosticGraphName => _graphTwcGenerator?.DiagnosticGraphName ?? "null";
        internal int DiagnosticSeed => _graphTwcGenerator?.DiagnosticSeed ?? 0;

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
            [InjectOptional] GraphTwcMapDataGenerator graphTwcGenerator,
            [InjectOptional] IGridProjection gridProjection = null,
            [InjectOptional] WaterLayerMaterialSettings waterLayerMaterialSettings = null,
            [InjectOptional] TileWorldCreatorWorldBuildBridge tileWorldCreatorBridge = null,
            [InjectOptional] IWorldGenerationDiagnostics worldDiagnostics = null,
            [InjectOptional] ISaveLoadDiagnostics saveLoadDiagnostics = null,
            [InjectOptional] ISaveLoadDiagnosticsSession saveLoadDiagnosticsSession = null)
        {
            _tileRegistry = tileRegistry;
            _objectRegistry = objectRegistry;
            _mapObjectVisualRegistryService = mapObjectVisualRegistryService;
            _unitClassConfig = unitClassConfig;
            _unitFactory = unitFactory;
            _buildingRegistry = buildingRegistry;
            _gridService = gridService;
            _gridProjection = gridProjection ?? new OrthogonalGridProjection();
            _mapDataGenerator = mapDataGenerator;
            _container = container;
            _signalBus = signalBus;
            _graphTwcGenerator = graphTwcGenerator;
            _waterLayerMaterialSettings = waterLayerMaterialSettings;
            _tileWorldCreatorBridge = tileWorldCreatorBridge;
            _worldDiagnostics = worldDiagnostics;
            _saveLoadDiagnostics = saveLoadDiagnostics;
            _saveLoadDiagnosticsSession = saveLoadDiagnosticsSession;
        }

        public void Initialize()
        {
            if (_tileRegistry?.Definitions != null)
            {
                foreach (var def in _tileRegistry.Definitions)
                {
                    if (def == null || string.IsNullOrEmpty(def.Id))
                        continue;

                    _definitionsCache[def.Id] = def;
                }
            }

            _signalBus.Subscribe<WorldSpawnPositionsSignal>(OnWorldSpawnPositions);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<WorldSpawnPositionsSignal>(OnWorldSpawnPositions);
            ReleaseRuntimeLayerResources();
        }

        public void BuildWorld()
        {
            bool hasPendingWorld = _pendingWorldData != null;
            string source = hasPendingWorld
                ? "pending-save"
                : GameLaunchContext.Mode == GameLaunchMode.DirectGameplayTest
                    ? "direct-test"
                    : "new";
            _worldDiagnostics?.MapVisualBuildWorldCalled(
                $"source={source}, frame={Time.frameCount}, hasPendingWorld={hasPendingWorld}");
            Debug.Log(
                $"{GeneratorBootDiagTag} MapVisual.BuildWorld ENTER frame={Time.frameCount}, mode={GameLaunchContext.Mode}, " +
                $"hasWorldSettings={GameLaunchContext.HasWorldSettings}, hasPendingWorld={hasPendingWorld}");
            Debug.Log(
                $"{WorldGenDiagTag} MapVisual.BuildWorld ENTER frame={Time.frameCount}, mode={GameLaunchContext.Mode}, " +
                $"hasWorldSettings={GameLaunchContext.HasWorldSettings}, hasPendingWorld={hasPendingWorld}, source={source}");
            GeneratedWorldData worldData = _pendingWorldData;
            _pendingWorldData = null;

            if (worldData == null)
            {
                Debug.Log($"{WorldGenDiagTag} MapVisual.CALL GenerateNewWorldData source=new");
                worldData = GenerateNewWorldData();
                Debug.Log(
                    $"{WorldGenDiagTag} MapVisual.GenerateNewWorldData.RESULT map={worldData?.Width ?? 0}x{worldData?.Height ?? 0}, " +
                    $"heightMap={FormatMapSize(worldData?.HeightMap)}, terrainMap={FormatMapSize(worldData?.TerrainLevelMap)}, " +
                    $"objectMap={FormatMapSize(worldData?.ObjectMap)}, buildingMap={FormatMapSize(worldData?.BuildingMap)}");
            }
            else
            {
                Debug.Log(
                    $"{WorldGenDiagTag} MapVisual.PendingWorldData.USE map={worldData.Width}x{worldData.Height}, source=pending-world-data");
            }

            BuildWorldFromData(worldData, source);
            Debug.Log($"{WorldGenDiagTag} MapVisual.BuildWorld EXIT frame={Time.frameCount}, source={source}");
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
            int requestedWidth = _gridService.GridWidth;
            int requestedHeight = _gridService.GridHeight;
            Debug.Log(
                $"{GeneratorBootDiagTag} MapVisual.GenerateNewWorldData ENTER graph={DiagnosticGraphName}, map={requestedWidth}x{requestedHeight}, seed={DiagnosticSeed}");
            Debug.Log(
                $"{WorldGenDiagTag} MapVisual.GenerateNewWorldData ENTER graph={DiagnosticGraphName}, map={requestedWidth}x{requestedHeight}, " +
                $"seed={DiagnosticSeed}, mapDataGenerator={DiagnosticMapDataGeneratorTypeName}, hasGraphGenerator={HasGraphGenerator}, " +
                $"hasSharedMapSize={HasSharedMapSize}, hasWorldSettings={GameLaunchContext.HasWorldSettings}");
            string[,] virtualBiomeMap = null;
            string[,] virtualObjectMap = null;
            float[,] finalHeightMap = null;
            string[,] virtualBuildingMap = null;

            _mapDataGenerator.GenerateMapData(
                requestedWidth,
                requestedHeight,
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
                GridTopology = _gridProjection.Topology,
                ProjectionMode = _gridProjection.ProjectionMode,
                RenderMode = ResolveRenderMode(_gridProjection.ProjectionMode),
                NeighborhoodMode = ResolveNeighborhoodMode(_gridProjection),
                BiomeMap = virtualBiomeMap,
                ObjectMap = virtualObjectMap,
                HeightMap = finalHeightMap,
                TerrainLevelMap = null,
                BuildingMap = virtualBuildingMap,
            };

            ApplyLaunchMetadata(data);
            _worldDiagnostics?.GraphMapDataGenerated(
                $"graph={DiagnosticGraphName}, map={data.Width}x{data.Height}, seed={DiagnosticSeed}");
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

        private static GridRenderMode ResolveRenderMode(GridProjectionMode projectionMode)
        {
            return projectionMode == GridProjectionMode.Isometric3DPreview
                ? GridRenderMode.Mesh3DPreview
                : GridRenderMode.Mesh3D;
        }

        private static GridNeighborhoodMode ResolveNeighborhoodMode(IGridProjection projection)
        {
            if (projection == null)
                return GridNeighborhoodMode.Moore8;

            if (projection.Topology == GridTopology.HexAxial)
                return GridNeighborhoodMode.HexAxial6;

            return projection.ProjectionMode == GridProjectionMode.Isometric3DPreview
                ? GridNeighborhoodMode.VonNeumann4
                : GridNeighborhoodMode.Moore8;
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

        private void BuildWorldFromData(GeneratedWorldData worldData, string source)
        {
            if (worldData == null)
            {
                Debug.LogError("[MapVisualInstantiator] BuildWorldFromData received null world data.");
                _worldDiagnostics?.FailStartupStep(
                    WorldGenerationDiagnosticSteps.MapVisualBuildWorld,
                    "world-data-null");
                return;
            }

            Debug.Log(
                $"{WorldGenDiagTag} MapVisual.BuildWorldFromData ENTER source={source}, map={worldData.Width}x{worldData.Height}, " +
                $"heightMap={FormatMapSize(worldData.HeightMap)}, terrainMap={FormatMapSize(worldData.TerrainLevelMap)}, " +
                $"savedSpawns={worldData.SpawnPositions?.Length ?? 0}, graphBranch={_graphTwcGenerator != null}");

            EnsureGridMatchesWorld(worldData);
            EnsureRoots();
            _mapObjectVisualRegistryService?.Clear();
            ClearRoot(_tilesRoot);
            ClearRoot(_objectsRoot);
            ClearRoot(_buildingsRoot);
            ClearRoot(_layersRoot);
            _terrainSurfaceYByPosition.Clear();
            ReleaseRuntimeLayerResources();
            Debug.Log(
                $"{WorldGenDiagTag} MapVisual.SceneClear.DONE rootsCleared={AreRootsCleared()}, gridCleared=implicit, " +
                $"tileRootChildren={_tilesRoot?.childCount ?? -1}, objectRootChildren={_objectsRoot?.childCount ?? -1}, " +
                $"buildingRootChildren={_buildingsRoot?.childCount ?? -1}, layerRootChildren={_layersRoot?.childCount ?? -1}");

            NormalizeBiomeMapIds(worldData);
            int filledCells = BuildGridFromLayerIds(worldData);
            Debug.Log($"{WorldGenDiagTag} MapVisual.GridFill.DONE map={worldData.Width}x{worldData.Height}, filledCells={filledCells}");

            // Новий TWC-конвеєр: 3D-візуал вже побудував TileWorldCreator.
            // Тут лише наповнюємо GridService layer-id'ами для ігроладу (рух/будівництво).
            if (_graphTwcGenerator != null)
            {
                _currentWorldData = worldData.Clone();
                _worldDiagnostics?.TwcBuildCompleted(
                    $"source=graph-generator, map={_currentWorldData.Width}x{_currentWorldData.Height}");
                Debug.Log($"{WorldGenDiagTag} Signal.FIRE WorldBuiltSignal frame={Time.frameCount}, source={source}");
                _signalBus.Fire(new WorldBuiltSignal());
                Debug.Log($"{WorldGenDiagTag} Signal.FIRED WorldBuiltSignal frame={Time.frameCount}");
                FireSavedSpawnPositions(_currentWorldData);
                float graphCellSize = ResolveWorldCellSize(TileWorldCreatorWorldBuildResult.Disabled);
                bool hasGraphBounds = TryResolveGeneratedMapWorldBounds(
                    _currentWorldData,
                    TileWorldCreatorWorldBuildResult.Disabled,
                    out Bounds graphWorldBounds);
                Debug.Log(
                    $"{WorldGenDiagTag} Signal.FIRE WorldGeneratedDataSignal frame={Time.frameCount}, map={_currentWorldData.Width}x{_currentWorldData.Height}, " +
                    $"bounds={FormatBounds(hasGraphBounds, graphWorldBounds)}, cellSize={graphCellSize:0.###}, heightMap={FormatMapSize(_currentWorldData.HeightMap)}, " +
                    $"terrainMap={FormatMapSize(_currentWorldData.TerrainLevelMap)}, objectMap={FormatMapSize(_currentWorldData.ObjectMap)}, buildingMap={FormatMapSize(_currentWorldData.BuildingMap)}");
                Debug.Log($"{DirectDiagTag} WorldSignal.FIRE WorldGeneratedDataSignal map={_currentWorldData.Width}x{_currentWorldData.Height}, hasHeightMap={_currentWorldData.HeightMap != null}, hasSavedSpawns={_currentWorldData.SpawnPositions != null && _currentWorldData.SpawnPositions.Length > 0}.");
                _worldDiagnostics?.WorldGeneratedSignalFired(
                    $"map={_currentWorldData.Width}x{_currentWorldData.Height}, frame={Time.frameCount}");
                _saveLoadDiagnostics?.CompleteStep(
                    _saveLoadDiagnosticsSession?.CurrentFlow,
                    SaveLoadDiagnosticSteps.WorldGeneratedDataSignalFired,
                    $"source={source}, map={_currentWorldData.Width}x{_currentWorldData.Height}");
                _signalBus.Fire(new WorldGeneratedDataSignal
                {
                    Width = _currentWorldData.Width,
                    Height = _currentWorldData.Height,
                    GridTopology = (int)_currentWorldData.GridTopology,
                    ProjectionMode = (int)_currentWorldData.ProjectionMode,
                    RenderMode = (int)_currentWorldData.RenderMode,
                    NeighborhoodMode = (int)_currentWorldData.NeighborhoodMode,
                    CellSize = ResolveWorldCellSize(TileWorldCreatorWorldBuildResult.Disabled),
                    HasMapWorldBounds = hasGraphBounds,
                    MapWorldBoundsCenter = graphWorldBounds.center,
                    MapWorldBoundsSize = graphWorldBounds.size,
                    TileMap = MapArrayUtils.CloneStringMap(_currentWorldData.BiomeMap),
                    ObjectMap = MapArrayUtils.CloneStringMap(_currentWorldData.ObjectMap),
                    HeightMap = MapArrayUtils.CloneFloatMap(_currentWorldData.HeightMap),
                    TerrainLevelMap = MapArrayUtils.CloneIntMap(_currentWorldData.TerrainLevelMap),
                });
                Debug.Log($"{WorldGenDiagTag} Signal.FIRED WorldGeneratedDataSignal frame={Time.frameCount}");
                Debug.Log($"{DirectDiagTag} WorldSignal.FIRED WorldGeneratedDataSignal");
                return;
            }

            var tileWorldCreatorResult = _tileWorldCreatorBridge?.Build(worldData)
                ?? TileWorldCreatorWorldBuildResult.Disabled;
            _worldDiagnostics?.TwcBuildCompleted(
                $"source=legacy-bridge, cellSize={tileWorldCreatorResult.CellSize:0.###}, hasBounds={tileWorldCreatorResult.HasBaseMapWorldBounds}");

            bool useLayerOnlyTiles = false;

            for (int x = 0; x < worldData.Width; x++)
            {
                for (int y = 0; y < worldData.Height; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    float elevation = ResolveTerrainElevation(worldData, x, y);
                    RegisterFallbackTerrainSurface(pos, elevation);

                    string biomeId = worldData.BiomeMap[x, y];
                    if (!string.IsNullOrEmpty(biomeId))
                    {
                        string resolvedBiomeId = ResolveGridTileId(biomeId);
                        worldData.BiomeMap[x, y] = resolvedBiomeId;

                        bool replaceTerrain = tileWorldCreatorResult.ShouldReplaceTerrainVisual(biomeId)
                            || tileWorldCreatorResult.ShouldReplaceTerrainVisual(resolvedBiomeId);

                        if (useLayerOnlyTiles || replaceTerrain)
                            _gridService.SetTileData(pos, resolvedBiomeId);
                        else
                            CreateTileView(pos, resolvedBiomeId, _tilesRoot, -1, elevation);
                    }

                    string objectId = worldData.ObjectMap[x, y];
                    if (!string.IsNullOrEmpty(objectId))
                    {
                        if (tileWorldCreatorResult.ShouldReplaceObjectVisual(objectId))
                            RegisterTileWorldCreatorObjectEntity(pos, objectId);
                        else
                            CreateObjectLayerEntity(pos, objectId, elevation);
                    }

                    if (worldData.BuildingMap != null)
                    {
                        string buildingId = worldData.BuildingMap[x, y];
                        if (!string.IsNullOrEmpty(buildingId))
                        {
                            if (!tileWorldCreatorResult.ShouldReplaceBuildingVisual(buildingId))
                                CreateBuildingView(pos, buildingId, elevation);
                        }
                    }
                }
            }

            // Шари рендеримо після створення тайлів/об'єктів.
            if (!tileWorldCreatorResult.SuppressMoyvaLayerData)
                ApplyLayerData(worldData);

            _currentWorldData = worldData.Clone();
            Debug.Log($"{WorldGenDiagTag} Signal.FIRE WorldBuiltSignal frame={Time.frameCount}, source={source}");
            _signalBus.Fire(new WorldBuiltSignal());
            Debug.Log($"{WorldGenDiagTag} Signal.FIRED WorldBuiltSignal frame={Time.frameCount}");
            FireSavedSpawnPositions(_currentWorldData);
            float worldCellSize = ResolveWorldCellSize(tileWorldCreatorResult);
            bool hasBounds = TryResolveGeneratedMapWorldBounds(
                _currentWorldData,
                tileWorldCreatorResult,
                out Bounds worldBounds);
            Debug.Log(
                $"{WorldGenDiagTag} Signal.FIRE WorldGeneratedDataSignal frame={Time.frameCount}, map={_currentWorldData.Width}x{_currentWorldData.Height}, " +
                $"bounds={FormatBounds(hasBounds, worldBounds)}, cellSize={worldCellSize:0.###}, heightMap={FormatMapSize(_currentWorldData.HeightMap)}, " +
                $"terrainMap={FormatMapSize(_currentWorldData.TerrainLevelMap)}, objectMap={FormatMapSize(_currentWorldData.ObjectMap)}, buildingMap={FormatMapSize(_currentWorldData.BuildingMap)}");
            Debug.Log($"{DirectDiagTag} WorldSignal.FIRE WorldGeneratedDataSignal map={_currentWorldData.Width}x{_currentWorldData.Height}, hasHeightMap={_currentWorldData.HeightMap != null}, hasSavedSpawns={_currentWorldData.SpawnPositions != null && _currentWorldData.SpawnPositions.Length > 0}.");
            _worldDiagnostics?.WorldGeneratedSignalFired(
                $"map={_currentWorldData.Width}x{_currentWorldData.Height}, frame={Time.frameCount}");
            _saveLoadDiagnostics?.CompleteStep(
                _saveLoadDiagnosticsSession?.CurrentFlow,
                SaveLoadDiagnosticSteps.WorldGeneratedDataSignalFired,
                $"source={source}, map={_currentWorldData.Width}x{_currentWorldData.Height}");
            _signalBus.Fire(new WorldGeneratedDataSignal
            {
                Width = _currentWorldData.Width,
                Height = _currentWorldData.Height,
                GridTopology = (int)_currentWorldData.GridTopology,
                ProjectionMode = (int)_currentWorldData.ProjectionMode,
                RenderMode = (int)_currentWorldData.RenderMode,
                NeighborhoodMode = (int)_currentWorldData.NeighborhoodMode,
                CellSize = ResolveWorldCellSize(tileWorldCreatorResult),
                HasMapWorldBounds = hasBounds,
                MapWorldBoundsCenter = worldBounds.center,
                MapWorldBoundsSize = worldBounds.size,
                TileMap = MapArrayUtils.CloneStringMap(_currentWorldData.BiomeMap),
                ObjectMap = MapArrayUtils.CloneStringMap(_currentWorldData.ObjectMap),
                HeightMap = MapArrayUtils.CloneFloatMap(_currentWorldData.HeightMap),
                TerrainLevelMap = MapArrayUtils.CloneIntMap(_currentWorldData.TerrainLevelMap),
            });
            Debug.Log($"{WorldGenDiagTag} Signal.FIRED WorldGeneratedDataSignal frame={Time.frameCount}");
            Debug.Log($"{DirectDiagTag} WorldSignal.FIRED WorldGeneratedDataSignal");
        }

        private float ResolveWorldCellSize(TileWorldCreatorWorldBuildResult tileWorldCreatorResult)
        {
            if (tileWorldCreatorResult.CellSize > 0.0001f)
                return tileWorldCreatorResult.CellSize;

            if (_graphTwcGenerator != null && _graphTwcGenerator.LastCellSize > 0.0001f)
                return _graphTwcGenerator.LastCellSize;

            return 1f;
        }

        private bool TryResolveGeneratedMapWorldBounds(
            GeneratedWorldData worldData,
            TileWorldCreatorWorldBuildResult tileWorldCreatorResult,
            out Bounds bounds)
        {
            if (tileWorldCreatorResult.HasBaseMapWorldBounds)
            {
                bounds = tileWorldCreatorResult.BaseMapWorldBounds;
                return true;
            }

            if (_graphTwcGenerator != null
                && _graphTwcGenerator.TryGetLastBaseMapWorldBounds(out bounds))
            {
                return true;
            }

            if (_gridProjection != null && worldData != null)
            {
                bounds = _gridProjection.GetWorldBounds(worldData.Width, worldData.Height);
                return true;
            }

            bounds = default;
            return false;
        }

        private int BuildGridFromLayerIds(GeneratedWorldData worldData)
        {
            EnsureGridMatchesWorld(worldData);

            if (worldData.BiomeMap == null)
                return 0;

            int filledCells = 0;

            for (int x = 0; x < worldData.Width; x++)
            {
                for (int y = 0; y < worldData.Height; y++)
                {
                    string layerId = worldData.BiomeMap[x, y];
                    if (string.IsNullOrEmpty(layerId))
                        continue;

                    // У новому конвеєрі biomeMap уже містить id шару графа —
                    // не мапимо через legacy ResolveGridTileId.
                    _gridService.SetTileData(new Vector2Int(x, y), layerId);
                    filledCells++;
                }
            }

            return filledCells;
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

            Debug.Log($"{WorldGenDiagTag} Signal.FIRE WorldSpawnPositionsSignal source=saved assignments={worldData.SpawnPositions.Length}, frame={Time.frameCount}");
            Debug.Log($"{DirectDiagTag} WorldSignal.FIRE SavedWorldSpawnPositionsSignal assignments={worldData.SpawnPositions.Length}");
            _signalBus.Fire(new WorldSpawnPositionsSignal
            {
                Assignments = (SpawnPositionAssignment[])worldData.SpawnPositions.Clone(),
            });
            Debug.Log($"{WorldGenDiagTag} Signal.FIRED WorldSpawnPositionsSignal source=saved frame={Time.frameCount}");
            Debug.Log($"{DirectDiagTag} WorldSignal.FIRED SavedWorldSpawnPositionsSignal");
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

        private void NormalizeBiomeMapIds(GeneratedWorldData worldData)
        {
            if (worldData?.BiomeMap == null)
                return;

            int width = Mathf.Min(worldData.Width, worldData.BiomeMap.GetLength(0));
            int height = Mathf.Min(worldData.Height, worldData.BiomeMap.GetLength(1));

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    string biomeId = worldData.BiomeMap[x, y];
                    if (!string.IsNullOrEmpty(biomeId))
                        worldData.BiomeMap[x, y] = ResolveGridTileId(biomeId);
                }
            }
        }

        private void ApplyLayerData(GeneratedWorldData worldData)
        {
            // 2D sprite-based layer rendering is disabled in Full3D mode.
            // Terrain is built exclusively via TileWorldCreator bridge (mesh-based).
            return;

#pragma warning disable CS0162 // Unreachable code detected
            var layerData = worldData.LayerData;

            if (layerData == null || layerData.Length == 0)
                return;

            // Список матеріалів що мають _LandMaskTex — в них записуємо RT після побудови шарів.
            var waterMaterials = new List<Material>();

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
                _runtimeLayerSprites.Add(sprite);
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
                    if (mat.HasProperty("_GlobalMipBiasWeight"))
                        mat.SetFloat("_GlobalMipBiasWeight", 0f);
                    if (mat.HasProperty("_LandMaskTex"))
                        _waterLayerMaterialSettings?.ApplyTo(mat);
                    spriteRenderer.sharedMaterial = mat;
                    _runtimeLayerMaterials.Add(mat);

                    // Збираємо матеріали що мають _LandMaskTex — RT буде передано після побудови всіх шарів.
                    if (mat.HasProperty("_LandMaskTex"))
                        waterMaterials.Add(mat);
                }
                else if (!string.IsNullOrEmpty(layerData[i].LayerShaderName))
                {
                    Debug.LogWarning($"[MapVisualInstantiator] Shader '{layerData[i].LayerShaderName}' not found for layer '{layerData[i].LayerTileID}'. Using default SpriteRenderer material.");
                }

                Bounds projectionBounds = _gridProjection.GetWorldBounds(worldData.Width, worldData.Height);
                if (_gridProjection.WorldPlane == GridWorldPlane.XZ)
                {
                    layerObj.transform.position = new Vector3(projectionBounds.min.x, projectionBounds.min.y, projectionBounds.min.z);
                    layerObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                }
                else
                {
                    // Для сітки, де центри тайлів лежать у цілих координатах (x, y),
                    // шар із pivot (0,0) треба зсунути на пів тайла вліво/вниз,
                    // інакше центри текселів потрапляють у .5 і з'являється візуальний зсув.
                    layerObj.transform.position = new Vector3(projectionBounds.min.x, projectionBounds.min.y, 0f);
                    layerObj.transform.rotation = Quaternion.identity;
                }

                // Масштабуємо спрайт точно до розміру згенерованого світу незалежно від ppu/розміру текстури.
                float spriteWorldWidth = texture.width;
                float spriteWorldHeight = texture.height;

                float scaleX = spriteWorldWidth > 0f ? worldData.Width / spriteWorldWidth : 1f;
                float scaleY = spriteWorldHeight > 0f ? worldData.Height / spriteWorldHeight : 1f;
                layerObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);

                bool isWaterLayer = spriteRenderer.sharedMaterial != null
                    && spriteRenderer.sharedMaterial.HasProperty("_LandMaskTex");

                _ = isWaterLayer;
            }

            // Legacy ShoreMaskPrepass вимкнено разом зі старим генераторним конвеєром.
#pragma warning restore CS0162
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

        private void ReleaseRuntimeLayerResources()
        {
            for (int i = 0; i < _runtimeLayerSprites.Count; i++)
            {
                if (_runtimeLayerSprites[i] != null)
                    Object.Destroy(_runtimeLayerSprites[i]);
            }

            for (int i = 0; i < _runtimeLayerMaterials.Count; i++)
            {
                if (_runtimeLayerMaterials[i] != null)
                    Object.Destroy(_runtimeLayerMaterials[i]);
            }

            _runtimeLayerSprites.Clear();
            _runtimeLayerMaterials.Clear();
        }

        private bool AreRootsCleared()
        {
            return (_tilesRoot == null || _tilesRoot.childCount == 0)
                && (_objectsRoot == null || _objectsRoot.childCount == 0)
                && (_buildingsRoot == null || _buildingsRoot.childCount == 0)
                && (_layersRoot == null || _layersRoot.childCount == 0);
        }

        private static string FormatMapSize(System.Array map)
        {
            return map is { Rank: 2 }
                ? $"{map.GetLength(0)}x{map.GetLength(1)}"
                : "null";
        }

        private static string FormatBounds(bool hasBounds, Bounds bounds)
        {
            return hasBounds
                ? $"{bounds.center} / {bounds.size}"
                : "none";
        }

        private void CreateTileView(Vector2Int position, string tileId, Transform root, int sortingOrder, float elevation)
        {
            if (!TryResolveTileDefinition(tileId, out var tileType, out string resolvedTileId))
            {
                Debug.LogError($"[MapInstantiator] Не знайдено TileTypeDefinition для ID: {tileId}");
                return;
            }

            var tilePrefab = tileType.VisualPrefab;
            if (tilePrefab == null)
            {
                Debug.LogWarning($"[MapInstantiator] Tile ID '{resolvedTileId}' має запис у реєстрі, але prefab відсутній. Візуал пропущено, grid data збережено.");
                _gridService.SetTileData(position, resolvedTileId);
                return;
            }

            Vector3 worldPos = ResolveWorldPosition(position, elevation, sortingOrder);

            var instance = _container.InstantiatePrefab(
                tilePrefab,
                worldPos,
                tilePrefab.transform.rotation,
                root);

            instance.name = $"{(sortingOrder < 0 ? "Tile" : "Obj")}_{resolvedTileId}_{position.x}_{position.y}";
            if (sortingOrder < 0)
                RegisterTerrainSurface(position, instance, worldPos.y);

            var tileView = instance.GetComponent<TileView>();
            if (tileView != null)
            {
                tileView.Setup(position, worldPos);
            }

            // Оновлюємо дані в GridService
            // Якщо це об'єкт (річка), він може перезаписувати властивості прохідності клітинки
            // Логіка: якщо ми спавнимо об'єкт, він стає пріоритетним типом для цієї клітинки в логіці
            _gridService.SetTileData(position, resolvedTileId);
        }

        private string ResolveGridTileId(string tileId)
        {
            return TryResolveTileDefinition(tileId, out _, out string resolvedTileId)
                ? resolvedTileId
                : tileId;
        }

        private bool TryResolveTileDefinition(string tileId, out TileTypeDefinition tileType, out string resolvedTileId)
        {
            resolvedTileId = tileId;

            if (!string.IsNullOrEmpty(tileId) && TryGetRegisteredTileDefinition(tileId, out tileType))
                return true;

            foreach (string fallbackTileId in ResolveFallbackTileIds(tileId))
            {
                if (TryGetRegisteredTileDefinition(fallbackTileId, out tileType))
                {
                    resolvedTileId = fallbackTileId;
                    LogTileFallbackOnce(tileId, fallbackTileId);
                    return true;
                }
            }

            if (!string.IsNullOrEmpty(tileId) && _definitionsCache.TryGetValue(tileId, out tileType))
                return true;

            tileType = null;
            return false;
        }

        private bool TryGetRegisteredTileDefinition(string tileId, out TileTypeDefinition tileType)
        {
            if (!string.IsNullOrEmpty(tileId)
                && _definitionsCache.TryGetValue(tileId, out tileType)
                && tileType != null)
            {
                return true;
            }

            tileType = null;
            return false;
        }

        private void LogTileFallbackOnce(string sourceTileId, string fallbackTileId)
        {
            string key = $"{sourceTileId}->{fallbackTileId}";
            if (!_loggedTileFallbacks.Add(key))
                return;

            Debug.LogWarning($"[MapInstantiator] Tile ID '{sourceTileId}' відсутній у поточному реєстрі. Використано сумісний fallback '{fallbackTileId}'.");
        }

        private static IEnumerable<string> ResolveFallbackTileIds(string tileId)
        {
            if (string.IsNullOrWhiteSpace(tileId))
                yield break;

            string normalized = tileId.ToLowerInvariant();
            if (normalized.Contains("deep-depth") || normalized.Contains("ocean-deep"))
            {
                yield return "water-deep-depth-tile-001";
                yield return "water-deep-depth-tile-002";
                yield return "ocean-deep";
                yield break;
            }

            if (normalized.StartsWith("water") || normalized.Contains("water-"))
            {
                yield return "water-middle-depth-tile-002";
                yield return "water-deep-depth-tile-001";
                yield return "ocean-shallow";
                yield break;
            }

            if (normalized.Contains("stone-hill") || normalized.Contains("hill"))
            {
                yield return "grass-tile-level-3-001";
                yield return "grass-tile-level-2-001";
                yield return "hill";
                yield break;
            }

            if (normalized.Contains("mountain"))
            {
                yield return "grass-tile-level-3-001";
                yield return "hill";
                yield return "mountain";
                yield break;
            }

            if (normalized.Contains("snow"))
            {
                yield return "snow-tile-001";
                yield return "grass-tile-level-1-001";
                yield break;
            }

            if (normalized.Contains("sand") || normalized.Contains("coast"))
            {
                yield return "sand-tile-003";
                yield return "sand-tile-001";
                yield return "beach";
                yield break;
            }

            if (normalized.Contains("grass") || normalized.Contains("lowland"))
            {
                yield return "grass-tile-001";
                yield return "texture-grass-tile-001";
                yield return "grass-tile-level-1-001";
                yield return "grass";
            }
        }

        private static float ResolveTerrainElevation(GeneratedWorldData worldData, int x, int y)
        {
            if (worldData?.TerrainLevelMap != null
                && x >= 0 && x < worldData.TerrainLevelMap.GetLength(0)
                && y >= 0 && y < worldData.TerrainLevelMap.GetLength(1))
            {
                return worldData.TerrainLevelMap[x, y];
            }

            if (worldData?.HeightMap != null
                && x >= 0 && x < worldData.HeightMap.GetLength(0)
                && y >= 0 && y < worldData.HeightMap.GetLength(1))
            {
                return worldData.HeightMap[x, y];
            }

            return 0f;
        }

        private void CreateObjectLayerEntity(Vector2Int position, string layerEntityId, float elevation)
        {
            if (_objectRegistry.TryGetDefinition(layerEntityId, out _))
            {
                CreateObjectView(position, layerEntityId, _objectsRoot, 0, elevation);

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
            if (TryResolveTileDefinition(layerEntityId, out _, out string resolvedTileId))
            {
                CreateTileView(position, resolvedTileId, _objectsRoot, 0, elevation);
                return;
            }

            Debug.LogWarning($"[MapVisualInstantiator] Object layer ID '{layerEntityId}' не знайдено ні в MapObjectRegistry, ні в UnitRegistry.");
        }

        private void RegisterTileWorldCreatorObjectEntity(Vector2Int position, string objectId)
        {
            if (_objectRegistry.TryGetDefinition(objectId, out _))
            {
                _signalBus.Fire(new OnMapObjectSpawnedSignal
                {
                    ObjectId = objectId,
                    Position = position
                });
                return;
            }

            if (TryResolveTileDefinition(objectId, out _, out string resolvedTileId))
            {
                _gridService.SetTileData(position, resolvedTileId);
                return;
            }

            Debug.LogWarning($"[MapVisualInstantiator] TWC object ID '{objectId}' has no MapObjectRegistry or TileRegistry entry. Gameplay occupancy was not registered.");
        }

        private void CreateObjectView(Vector2Int position, string objectId, Transform root, int sortingOrder, float elevation)
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

            Vector3 worldPos = ResolveWorldPosition(position, elevation, sortingOrder);
            Quaternion prefabRotation = objectDef.VisualPrefab.transform.rotation;

            var instance = _container.InstantiatePrefab(
                objectDef.VisualPrefab,
                worldPos,
                prefabRotation,
                root);

            instance.name = $"Obj_{objectId}_{position.x}_{position.y}";
            AlignInstanceToTerrainSurface(instance, position, elevation);
            worldPos = instance.transform.position;

            var tileView = instance.GetComponent<TileView>();
            if (tileView != null)
            {
                tileView.Setup(position, worldPos);
            }

            ApplySortingOrder(instance, ObjectLayerSortingOrder);
            _mapObjectVisualRegistryService?.Register(objectId, position, instance);
        }

        private void CreateBuildingView(Vector2Int position, string buildingId, float elevation)
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

            Vector3 worldPos = ResolveWorldPosition(position, elevation, 1);

            var instance = _container.InstantiatePrefab(
                def.Prefab,
                worldPos,
                def.Prefab.transform.rotation,
                _buildingsRoot);

            instance.name = $"Building_{buildingId}_{position.x}_{position.y}";
            AlignInstanceToTerrainSurface(instance, position, elevation);
            worldPos = instance.transform.position;

            var tileView = instance.GetComponent<TileView>();
            if (tileView != null)
            {
                tileView.Setup(position, worldPos);
            }

            EnsureBuildingSortingOrder(instance, BuildingLayerMinSortingOrder);
        }

        private Vector3 ResolveWorldPosition(Vector2Int position, float elevation, int sortingOrder)
        {
            float layerOffset = sortingOrder * 0.1f;
            if (_gridProjection.WorldPlane == GridWorldPlane.XZ && sortingOrder < 0)
                layerOffset = 0f;

            return _gridProjection.GridToWorld(position, elevation, layerOffset);
        }

        private void RegisterFallbackTerrainSurface(Vector2Int position, float elevation)
        {
            if (!GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection))
                return;

            _terrainSurfaceYByPosition[position] = _gridProjection.GridToWorld(position, elevation, 0f).y;
        }

        private void RegisterTerrainSurface(Vector2Int position, GameObject tileInstance, float fallbackSurfaceY)
        {
            if (!GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection))
                return;

            _terrainSurfaceYByPosition[position] = GridSurfacePlacementUtility.TryResolveRendererBounds(tileInstance, out var bounds)
                ? bounds.max.y
                : fallbackSurfaceY;
        }

        private void AlignInstanceToTerrainSurface(GameObject instance, Vector2Int position, float elevation)
        {
            if (!GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection) || instance == null)
                return;

            float surfaceY = _terrainSurfaceYByPosition.TryGetValue(position, out float cachedSurfaceY)
                ? cachedSurfaceY
                : _gridProjection.GridToWorld(position, elevation, 0f).y;

            GridSurfacePlacementUtility.AlignBottomToSurface(instance, surfaceY);
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
