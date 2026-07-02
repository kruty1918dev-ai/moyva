using System;
using System.Collections.Generic;
using System.Text;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Runtime visual updater для TWC dual-grid fog volume.
    /// Працює як presentation layer: читає gameplay fog state через <see cref="IFogOfWarService"/>,
    /// накопичує dirty-клітинки, керує runtime configuration clone і перебудовує volume geometry.
    /// Не є source of truth для visible/explored state.
    /// </summary>
    internal sealed class FogOfWarVolumeUpdater : IFogVisualUpdater, IFogVolumeRuntimeUpdater, ITickable, IDisposable
    {
        private const string LogTag = "[FogOfWarVolume]";
        private const string StartDiagTag = "[MoyvaFogStartDiag]";
        private const string FolderName = "Fog Volume";

        private readonly FogOfWarSettings _injectedSettings;
        private readonly HashSet<Vector2Int> _pendingDirtyTiles = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2> _unexploredCells = new HashSet<Vector2>();
        private readonly HashSet<Vector2> _exploredCells = new HashSet<Vector2>();
        private readonly HashSet<Vector2> _scratchCells = new HashSet<Vector2>();
        private readonly Dictionary<int, HashSet<Vector2>> _unexploredCellsByHeight = new Dictionary<int, HashSet<Vector2>>();
        private readonly Dictionary<int, HashSet<Vector2>> _exploredCellsByHeight = new Dictionary<int, HashSet<Vector2>>();
        private readonly Dictionary<int, float> _heightByKey = new Dictionary<int, float>();
        private readonly List<RuntimeLayer> _runtimeLayers = new List<RuntimeLayer>();

        private FogOfWarVolumeController _controller;
        private TileWorldCreatorManager _manager;
        private Configuration _previousManagerConfiguration;
        private Configuration _runtimeConfiguration;
        private FogWorldVisualContext _context;
        private IFogOfWarService _lastFogService;
        private bool _fullRebuildRequested;
        private bool _hasPendingVisualWork;
        private bool _runtimeConfigurationDirty = true;
        private string _runtimeLayerSignature;
        private bool _hasBuiltAtLeastOnce;
        private bool _worldContextChangedSinceBuild;
        private bool _loggedFirstBuild;
        private int _mapWidth = 1;
        private int _mapHeight = 1;
        private float _nextIntervalRebuildTime;
        private bool _loggedMissingController;
        private bool _loggedMissingManager;
        private bool _loggedMissingSettings;
        private bool _loggedMissingFogService;
        private bool _loggedNoRuntimeLayers;
        private bool _loggedUnexploredPresetProblem;
        private bool _loggedExploredPresetProblem;
        private bool _loggedAttach;
        private bool _loggedInitialize;
        private bool _loggedWorldContext;
        private bool _loggedRebuildRequest;
        private bool _loggedDirtyUpdate;
        private bool _loggedTickWaitingForInterval;

        /// <summary>
        /// Створює updater для volume visual path.
        /// У runtime зазвичай отримує settings через Zenject, а у preview може бути створений локально.
        /// </summary>
        /// <param name="settings">Fog settings для tuning і побудови runtime layers.</param>
        public FogOfWarVolumeUpdater([InjectOptional] FogOfWarSettings settings = null)
        {
            _injectedSettings = settings;
            if (settings != null)
                Debug.Log($"{LogTag} Updater constructed: injectedSettings='{settings.name}'.");
            else
                Debug.LogWarning($"{LogTag} Updater constructed without injected FogOfWarSettings. It will use settings from FogOfWarVolumeController if one attaches.");
        }

        /// <summary>
        /// Діагностична кількість unexplored-клітинок у поточному кеші visual state.
        /// </summary>
        internal int DebugUnexploredCellCount => _unexploredCells.Count;

        /// <summary>
        /// Діагностична кількість explored-клітинок у поточному кеші visual state.
        /// </summary>
        internal int DebugExploredCellCount => _exploredCells.Count;

        /// <summary>
        /// Діагностичний доступ до runtime TWC configuration clone.
        /// </summary>
        internal Configuration DebugRuntimeConfiguration => _runtimeConfiguration;

        /// <summary>
        /// Під'єднує scene controller як host для runtime visual update path.
        /// Side effect: updater починає працювати з його TWC manager-ом і може запланувати rebuild.
        /// </summary>
        /// <param name="controller">Scene host-компонент fog volume.</param>
        public void AttachController(FogOfWarVolumeController controller)
        {
            if (controller == null)
                return;

            if (_controller == controller)
                return;

            _controller = controller;
            _manager = controller.TileWorldCreatorManager;
            if (_runtimeConfiguration == null)
                _previousManagerConfiguration = _manager != null ? _manager.configuration : null;
            _runtimeConfigurationDirty = true;
            _loggedMissingController = false;
            _loggedMissingManager = false;
            _loggedMissingSettings = false;
            _loggedNoRuntimeLayers = false;
            _loggedUnexploredPresetProblem = false;
            _loggedExploredPresetProblem = false;
            Debug.Log($"{StartDiagTag} VolumeUpdater.AttachController controller={(controller != null ? controller.name : "null")}, manager={(_manager != null ? _manager.name : "null")}, settings={(controller.Settings != null ? controller.Settings.name : "null")}, hasLastFogService={_lastFogService != null}, hasBuilt={_hasBuiltAtLeastOnce}.");
            LogUpdaterOnce(ref _loggedAttach, $"AttachController: controller='{controller.name}', manager={(_manager != null ? _manager.name : "null")}, settings={(controller.Settings != null ? controller.Settings.name : "null")}, hasLastFogService={_lastFogService != null}, hasBuilt={_hasBuiltAtLeastOnce}.");
            RequestVisualRebuild();
            if (_lastFogService != null && !_hasBuiltAtLeastOnce)
                ExecutePendingVisualWork();
        }

        /// <summary>
        /// Від'єднує scene controller від updater-а.
        /// Side effect: наступні visual rebuild-и не зможуть використовувати попередній manager напряму.
        /// </summary>
        /// <param name="controller">Scene host-компонент, який відключається.</param>
        public void DetachController(FogOfWarVolumeController controller)
        {
            if (_controller != controller)
                return;

            _controller = null;
            _manager = null;
            _runtimeConfigurationDirty = true;
        }

        /// <summary>
        /// Ініціалізує runtime visual state для карти заданого розміру і world context.
        /// Side effect: позначає runtime configuration як dirty і планує повну перебудову.
        /// </summary>
        /// <param name="width">Ширина карти у клітинках.</param>
        /// <param name="height">Висота карти у клітинках.</param>
        /// <param name="context">Світовий контекст для volume build path.</param>
        public void Initialize(int width, int height, FogWorldVisualContext context)
        {
            _mapWidth = Mathf.Max(1, width);
            _mapHeight = Mathf.Max(1, height);
            if (context.IsValid)
                _context = context.WithSize(_mapWidth, _mapHeight);
            else if (!_context.IsValid)
                _context = CreateFallbackContext(_mapWidth, _mapHeight);
            else
                _context = _context.WithSize(_mapWidth, _mapHeight);

            _runtimeConfigurationDirty = true;
            _fullRebuildRequested = true;
            _hasPendingVisualWork = true;
            _hasBuiltAtLeastOnce = false;
            _worldContextChangedSinceBuild = true;
            _loggedNoRuntimeLayers = false;
            _loggedUnexploredPresetProblem = false;
            _loggedExploredPresetProblem = false;
            _loggedTickWaitingForInterval = false;
            LogUpdaterOnce(ref _loggedInitialize, $"Initialize: requested={width}x{height}, effective={_mapWidth}x{_mapHeight}, contextValid={context.IsValid}, contextCell={context.CellSize:0.###}, storedCell={_context.CellSize:0.###}, bounds={FormatBounds(_context)}, heightMap={FormatMapSize(_context.HeightMap)}, terrainLevelMap={FormatMapSize(_context.TerrainLevelMap)}, controller={(_controller != null ? _controller.name : "null")}.");
        }

        /// <summary>
        /// Оновлює world context без зміни gameplay fog state.
        /// Викликається, коли змінилися bounds, cell size або height/terrain maps.
        /// </summary>
        /// <param name="context">Оновлений visual context generated світу.</param>
        public void SetWorldContext(FogWorldVisualContext context)
        {
            if (!context.IsValid)
                return;

            bool sizeChanged = context.Width != _mapWidth || context.Height != _mapHeight;
            bool cellSizeChanged = !_context.IsValid || !Mathf.Approximately(context.CellSize, _context.CellSize);
            bool boundsChanged = !_context.IsValid || context.HasMapWorldBounds != _context.HasMapWorldBounds
                || context.HasMapWorldBounds && !ApproximatelyBounds(context.MapWorldBounds, _context.MapWorldBounds);

            _context = context;
            _mapWidth = context.Width;
            _mapHeight = context.Height;

            if (sizeChanged || cellSizeChanged || boundsChanged)
                _runtimeConfigurationDirty = true;

            _worldContextChangedSinceBuild = true;
            _fullRebuildRequested = true;
            _hasPendingVisualWork = true;
            _loggedWorldContext = false;
            LogUpdaterOnce(ref _loggedWorldContext, $"SetWorldContext: map={_mapWidth}x{_mapHeight}, cell={_context.CellSize:0.###}, sizeChanged={sizeChanged}, cellSizeChanged={cellSizeChanged}, boundsChanged={boundsChanged}, bounds={FormatBounds(_context)}, heightMap={FormatMapSize(_context.HeightMap)}, terrainLevelMap={FormatMapSize(_context.TerrainLevelMap)}.");
        }

        /// <summary>
        /// Будує тимчасовий preview reveal через startup-style preview fog service.
        /// Не змінює gameplay fog state.
        /// </summary>
        /// <param name="center">Центр preview reveal.</param>
        /// <param name="radius">Радіус preview reveal.</param>
        /// <param name="shape">Форма reveal області.</param>
        /// <param name="keepVisible">Чи має preview поводитись як постійна visible область.</param>
        public void PreviewRevealArea(Vector2Int center, int radius, FogRevealShape shape, bool keepVisible)
        {
            if (_lastFogService != null)
            {
                Debug.Log($"{LogTag} PreviewRevealArea skipped: gameplay fog service is active and will drive visual state.");
                return;
            }

            if (!_context.IsValid)
                _context = CreateFallbackContext(_mapWidth, _mapHeight);

            Debug.Log($"{LogTag} PreviewRevealArea center={center}, radius={Mathf.Max(0, radius)}, shape={shape}, keepVisible={keepVisible}, context={_context.Width}x{_context.Height}.");
            Initialize(_context.Width, _context.Height, _context);
            RebuildFullVisual(new StartupFogService(_context.Width, _context.Height, center, radius, shape, keepVisible));
        }

        /// <summary>
        /// Приймає dirty-клітинки від gameplay fog service і планує часткову або негайну visual rebuild.
        /// </summary>
        /// <param name="fogService">Gameplay source of truth для fog state.</param>
        /// <param name="dirtyTiles">Клітинки, чий стан змінився з останнього update.</param>
        public void UpdateDirtyTiles(IFogOfWarService fogService, IEnumerable<Vector2Int> dirtyTiles)
        {
            _lastFogService = fogService;
            if (fogService != null)
                _loggedMissingFogService = false;
            int requested = 0;
            if (dirtyTiles != null)
            {
                foreach (var tile in dirtyTiles)
                {
                    requested++;
                    if (IsInBounds(tile))
                        _pendingDirtyTiles.Add(tile);
                }
            }

            _hasPendingVisualWork = _hasPendingVisualWork || _pendingDirtyTiles.Count > 0;
            Debug.Log($"{StartDiagTag} VolumeUpdater.UpdateDirtyTiles requested={requested}, accepted={_pendingDirtyTiles.Count}, hasFogService={fogService != null}, controller={(_controller != null ? _controller.name : "null")}, map={_mapWidth}x{_mapHeight}, fullRebuildRequested={_fullRebuildRequested}.");
            if (ShouldLogLifecycle(_loggedDirtyUpdate))
            {
                _loggedDirtyUpdate = true;
                Debug.Log($"{LogTag} UpdateDirtyTiles: fogService={(fogService != null ? fogService.GetType().Name : "null")}, requested={requested}, acceptedPending={_pendingDirtyTiles.Count}, map={_mapWidth}x{_mapHeight}, updateMode={ResolveUpdateMode()}, immediate={ResolveUpdateMode() == FogVolumeUpdateMode.Immediate}, controller={(_controller != null ? _controller.name : "null")}.");
            }
            if (ResolveUpdateMode() == FogVolumeUpdateMode.Immediate)
                ExecutePendingVisualWork();
        }

        /// <summary>
        /// Прапорить повну перебудову volume зі стану gameplay fog service.
        /// </summary>
        /// <param name="fogService">Gameplay source of truth для fog state.</param>
        public void RebuildFullVisual(IFogOfWarService fogService)
        {
            _lastFogService = fogService;
            if (fogService != null)
                _loggedMissingFogService = false;
            _fullRebuildRequested = true;
            _hasPendingVisualWork = true;
            Debug.Log($"{StartDiagTag} VolumeUpdater.RebuildFullVisual hasFogService={fogService != null}, map={_mapWidth}x{_mapHeight}, controller={(_controller != null ? _controller.name : "null")}, manager={(_manager != null ? _manager.name : "null")}, contextValid={_context.IsValid}, hasBuilt={_hasBuiltAtLeastOnce}, contextChanged={_worldContextChangedSinceBuild}.");
            if (ShouldLogLifecycle(_loggedRebuildRequest))
            {
                _loggedRebuildRequest = true;
                Debug.Log($"{LogTag} RebuildFullVisual: fogService={(fogService != null ? fogService.GetType().Name : "null")}, map={_mapWidth}x{_mapHeight}, contextValid={_context.IsValid}, hasController={_controller != null}, hasManager={_manager != null}, hasBuilt={_hasBuiltAtLeastOnce}, contextChanged={_worldContextChangedSinceBuild}, updateMode={ResolveUpdateMode()}.");
            }
            if (!_hasBuiltAtLeastOnce || _worldContextChangedSinceBuild || ResolveUpdateMode() == FogVolumeUpdateMode.Immediate)
                ExecutePendingVisualWork();
        }

        /// <summary>
        /// Виконує відкладену visual rebuild відповідно до обраного update mode.
        /// Викликається Zenject-ом щокадру як частина runtime lifecycle.
        /// </summary>
        public void Tick()
        {
            if (!_hasPendingVisualWork)
                return;

            switch (ResolveUpdateMode())
            {
                case FogVolumeUpdateMode.Interval:
                    if (Time.unscaledTime < _nextIntervalRebuildTime)
                    {
                        LogUpdaterOnce(ref _loggedTickWaitingForInterval, $"Tick waiting for interval: now={Time.unscaledTime:0.###}, next={_nextIntervalRebuildTime:0.###}, pending={_hasPendingVisualWork}, fullRebuild={_fullRebuildRequested}, dirty={_pendingDirtyTiles.Count}.");
                        return;
                    }

                    _nextIntervalRebuildTime = Time.unscaledTime + ResolveRebuildIntervalSeconds();
                    ExecutePendingVisualWork();
                    break;
                default:
                    ExecutePendingVisualWork();
                    break;
            }
        }

        /// <summary>
        /// Звільняє runtime configuration clone і пов'язані ресурси updater-а.
        /// </summary>
        public void Dispose()
        {
            DisposeRuntimeConfiguration();
        }

        /// <summary>
        /// Діагностично перевіряє, чи кеш unexplored state містить задану клітинку.
        /// </summary>
        /// <param name="tile">Клітинка для перевірки.</param>
        /// <returns><see langword="true"/>, якщо клітинка входить до unexplored-кешу.</returns>
        internal bool DebugHasUnexploredCell(Vector2Int tile)
            => _unexploredCells.Contains(ToCell(tile));

        /// <summary>
        /// Діагностично перевіряє, чи кеш explored state містить задану клітинку.
        /// </summary>
        /// <param name="tile">Клітинка для перевірки.</param>
        /// <returns><see langword="true"/>, якщо клітинка входить до explored-кешу.</returns>
        internal bool DebugHasExploredCell(Vector2Int tile)
            => _exploredCells.Contains(ToCell(tile));

        /// <summary>
        /// Запитує первинну startup build для controller-а без локальної visible області.
        /// </summary>
        /// <param name="controller">Host-компонент сцени.</param>
        /// <param name="context">Світовий контекст для build path.</param>
        internal void RequestStartupBuildFromController(FogOfWarVolumeController controller, FogWorldVisualContext context)
            => RequestStartupBuildFromController(controller, context, null, 0, FogRevealShape.PixelCircle, keepVisible: false);

        /// <summary>
        /// Запитує первинну startup build для controller-а з необов'язковою visible preview областю.
        /// Side effect: може ініціалізувати updater і одразу виконати повну visual rebuild.
        /// </summary>
        /// <param name="controller">Host-компонент сцени.</param>
        /// <param name="context">Світовий контекст для build path.</param>
        /// <param name="visibleCenter">Необов'язковий центр початкової visible області.</param>
        /// <param name="visibleRadius">Радіус початкової visible області.</param>
        /// <param name="visibleShape">Форма початкової visible області.</param>
        /// <param name="keepVisible">Чи має початкова область залишатись visible надалі.</param>
        internal void RequestStartupBuildFromController(
            FogOfWarVolumeController controller,
            FogWorldVisualContext context,
            Vector2Int? visibleCenter,
            int visibleRadius,
            FogRevealShape visibleShape,
            bool keepVisible)
        {
            if (controller != null)
                AttachController(controller);

            if (_lastFogService != null)
            {
                Debug.Log($"{LogTag} Startup build skipped: gameplay fog service is already available.");
                return;
            }

            if (_hasBuiltAtLeastOnce)
            {
                Debug.Log($"{LogTag} Startup build skipped: fog volume has already been built.");
                return;
            }

            if (!context.IsValid)
                context = CreateFallbackContext(_mapWidth, _mapHeight);

            Debug.Log($"{StartDiagTag} VolumeUpdater.RequestStartupBuildFromController controller={(controller != null ? controller.name : "null")}, manager={(_manager != null ? _manager.name : "null")}, context={context.Width}x{context.Height}, cell={context.CellSize:0.###}, visibleCenter={(visibleCenter.HasValue ? visibleCenter.Value.ToString() : "none")}, visibleRadius={Mathf.Max(0, visibleRadius)}, keepVisible={keepVisible}, hasLastFogService={_lastFogService != null}, hasBuilt={_hasBuiltAtLeastOnce}.");
            Debug.Log($"{LogTag} Startup build requested by controller='{(controller != null ? controller.name : "null")}', context={context.Width}x{context.Height}, cell={context.CellSize:0.###}, bounds={FormatBounds(context)}, heightMap={FormatMapSize(context.HeightMap)}, terrainLevelMap={FormatMapSize(context.TerrainLevelMap)}, visibleCenter={(visibleCenter.HasValue ? visibleCenter.Value.ToString() : "none")}, visibleRadius={Mathf.Max(0, visibleRadius)}, visibleShape={visibleShape}, keepVisible={keepVisible}.");
            Initialize(context.Width, context.Height, context);
            RebuildFullVisual(visibleCenter.HasValue
                ? new StartupFogService(context.Width, context.Height, visibleCenter.Value, visibleRadius, visibleShape, keepVisible)
                : new StartupFogService(context.Width, context.Height));
        }

        /// <summary>
        /// Запитує повну runtime rebuild від scene controller-а.
        /// </summary>
        /// <param name="controller">Host-компонент сцени, який ініціює rebuild.</param>
        internal void RequestFullRebuildFromController(FogOfWarVolumeController controller)
        {
            if (controller != null)
                AttachController(controller);

            _runtimeConfigurationDirty = true;
            _fullRebuildRequested = true;
            _hasPendingVisualWork = _lastFogService != null;
            Debug.Log($"{StartDiagTag} VolumeUpdater.RequestFullRebuildFromController controller={(controller != null ? controller.name : "null")}, manager={(_manager != null ? _manager.name : "null")}, hasLastFogService={_lastFogService != null}, context={_context.Width}x{_context.Height}.");

            if (_hasPendingVisualWork)
                ExecutePendingVisualWork();
        }

        void IFogVolumeRuntimeUpdater.RequestStartupBuildFromController(FogOfWarVolumeController controller, FogWorldVisualContext context)
            => RequestStartupBuildFromController(controller, context);

        void IFogVolumeRuntimeUpdater.RequestFullRebuildFromController(FogOfWarVolumeController controller)
            => RequestFullRebuildFromController(controller);

        private void ExecutePendingVisualWork()
        {
            if (_lastFogService == null)
            {
                LogMissingFogServiceOnce();
                return;
            }

            Debug.Log($"{StartDiagTag} VolumeUpdater.ExecutePendingVisualWork hasFogService={_lastFogService != null}, pendingDirty={_pendingDirtyTiles.Count}, fullRebuildRequested={_fullRebuildRequested}, controller={(_controller != null ? _controller.name : "null")}, manager={(_manager != null ? _manager.name : "null")}, context={_context.Width}x{_context.Height}.");
            bool wasFullRebuild = _fullRebuildRequested;
            int requestedDirtyTiles = _pendingDirtyTiles.Count;
            if (_fullRebuildRequested)
                RebuildStateCaches(_lastFogService);
            else
                ApplyDirtyStateCacheChanges(_lastFogService);

            _fullRebuildRequested = false;
            _pendingDirtyTiles.Clear();
            _hasPendingVisualWork = false;

            if (!EnsureRuntimeConfiguration())
                return;

            ApplyCellsToRuntimeLayers();
            ExecuteTileWorldCreatorBuild(wasFullRebuild, requestedDirtyTiles);
        }

        private void RebuildStateCaches(IFogOfWarService fogService)
        {
            _unexploredCells.Clear();
            _exploredCells.Clear();
            ClearHeightCaches();

            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                    AddStateCell(new Vector2Int(x, y), fogService.GetFogState(new Vector2Int(x, y)));
            }

            UpdateRuntimeLayerSignature();
        }

        private void ApplyDirtyStateCacheChanges(IFogOfWarService fogService)
        {
            foreach (var tile in _pendingDirtyTiles)
            {
                var cell = ToCell(tile);
                _unexploredCells.Remove(cell);
                _exploredCells.Remove(cell);
                RemoveCellFromHeightCaches(cell);
                AddStateCell(tile, fogService.GetFogState(tile));
            }

            UpdateRuntimeLayerSignature();
        }

        private void AddStateCell(Vector2Int tile, FogStateType state)
        {
            if (!IsInBounds(tile))
                return;

            switch (state)
            {
                case FogStateType.Unexplored:
                    if (IsStateEnabled(GetSettings()?.Volume?.Unexplored, fallback: true))
                    {
                        _unexploredCells.Add(ToCell(tile));
                        AddHeightCell(_unexploredCellsByHeight, tile);
                    }
                    break;
                case FogStateType.Explored:
                    if (IsStateEnabled(GetSettings()?.Volume?.Explored, fallback: true))
                    {
                        _exploredCells.Add(ToCell(tile));
                        AddHeightCell(_exploredCellsByHeight, tile);
                    }
                    break;
            }
        }

        private void AddHeightCell(Dictionary<int, HashSet<Vector2>> stateCellsByHeight, Vector2Int tile)
        {
            int heightKey = ResolveHeightKey(tile);
            if (!stateCellsByHeight.TryGetValue(heightKey, out var cells))
            {
                cells = new HashSet<Vector2>();
                stateCellsByHeight.Add(heightKey, cells);
            }

            cells.Add(ToCell(tile));
        }

        private void RemoveCellFromHeightCaches(Vector2 cell)
        {
            RemoveCellFromHeightCache(_unexploredCellsByHeight, cell);
            RemoveCellFromHeightCache(_exploredCellsByHeight, cell);
        }

        private static void RemoveCellFromHeightCache(Dictionary<int, HashSet<Vector2>> cache, Vector2 cell)
        {
            foreach (var cells in cache.Values)
                cells.Remove(cell);
        }

        private void ClearHeightCaches()
        {
            _unexploredCellsByHeight.Clear();
            _exploredCellsByHeight.Clear();
            _heightByKey.Clear();
        }

        private void UpdateRuntimeLayerSignature()
        {
            string signature = BuildRuntimeLayerSignature();
            if (!string.Equals(signature, _runtimeLayerSignature, StringComparison.Ordinal))
            {
                _runtimeLayerSignature = signature;
                _runtimeConfigurationDirty = true;
            }
        }

        private string BuildRuntimeLayerSignature()
        {
            var parts = new List<string>();
            AppendLayerSignature(parts, "U", _unexploredCellsByHeight);
            AppendLayerSignature(parts, "E", _exploredCellsByHeight);
            parts.Sort(StringComparer.Ordinal);
            return string.Join("|", parts);
        }

        private void AppendLayerSignature(List<string> parts, string prefix, Dictionary<int, HashSet<Vector2>> cellsByHeight)
        {
            foreach (var pair in cellsByHeight)
            {
                if (pair.Value != null && pair.Value.Count > 0)
                    parts.Add($"{prefix}{pair.Key}");
            }
        }

        private bool EnsureRuntimeConfiguration()
        {
            if (_controller == null)
            {
                LogMissingControllerOnce();
                return false;
            }

            _manager = _controller.TileWorldCreatorManager;
            if (_manager == null)
            {
                LogMissingManagerOnce();
                return false;
            }

            if (GetSettings() == null)
                LogMissingSettingsOnce();

            if (!_runtimeConfigurationDirty && _runtimeConfiguration != null)
                return true;

            DisposeRuntimeConfiguration();
            _runtimeConfiguration = CreateRuntimeConfiguration(_context, _controller);

            _runtimeConfiguration.blueprintLayerFolders.Add(new BlueprintLayerFolder(FolderName));
            _runtimeConfiguration.buildLayerFolders.Add(new BuildLayerFolder(FolderName));
            CreateRuntimeLayers(
                _runtimeConfiguration,
                _unexploredCellsByHeight,
                FogRuntimeState.Unexplored,
                GetSettings()?.Volume?.Unexplored,
                "Fog_Unexplored");
            CreateRuntimeLayers(
                _runtimeConfiguration,
                _exploredCellsByHeight,
                FogRuntimeState.Explored,
                GetSettings()?.Volume?.Explored,
                "Fog_Explored");

            LogRuntimeLayerValidation();
            _manager.configuration = _runtimeConfiguration;
            ConfigureManagerTransform(_manager.transform, _context, _runtimeConfiguration.cellSize);
            _runtimeConfigurationDirty = false;
            return true;
        }

        private static Configuration CreateRuntimeConfiguration(
            FogWorldVisualContext context,
            FogOfWarVolumeController controller)
        {
            var configuration = ScriptableObject.CreateInstance<Configuration>();
            configuration.name = "FogOfWar_RuntimeConfiguration";
            configuration.width = Mathf.Max(1, context.Width);
            configuration.height = Mathf.Max(1, context.Height);
            configuration.cellSize = controller != null ? controller.ResolveCellSize(context.CellSize) : Mathf.Max(0.0001f, context.CellSize);
            configuration.lastCellSize = configuration.cellSize;
            configuration.clusterCellSize = 5;
            configuration.useGlobalRandomSeed = true;
            configuration.globalRandomSeed = 1;
            configuration.currentRandomSeed = 1u;
            configuration.useParallel = false;
            configuration.mergeTiles = false;
            configuration.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            configuration.objectLayer = 0;
            configuration.renderingLayer = 1;
            configuration.colliderType = Configuration.ColliderType.none;
            configuration.tileColliderHeight = 0f;
            configuration.tileColliderExtrusionHeight = 0f;
            configuration.invertCollisionWalls = false;
            configuration.blueprintLayerFolders = new List<BlueprintLayerFolder>();
            configuration.buildLayerFolders = new List<BuildLayerFolder>();
            configuration.zeroLayerPaddingBlueprintLayerGuids = new List<string>();
            configuration.tileMapLayers = new List<BlueprintLayer>();
            return configuration;
        }

        private void CreateRuntimeLayers(
            Configuration configuration,
            Dictionary<int, HashSet<Vector2>> cellsByHeight,
            FogRuntimeState state,
            FogVolumeStateTileSettings settings,
            string fallbackLayerName)
        {
            if (cellsByHeight == null)
                return;

            var heightKeys = new List<int>(cellsByHeight.Keys);
            heightKeys.Sort();
            for (int i = 0; i < heightKeys.Count; i++)
            {
                int heightKey = heightKeys[i];
                if (!cellsByHeight.TryGetValue(heightKey, out var cells) || cells == null || cells.Count == 0)
                    continue;

                var runtimeLayer = CreateRuntimeLayer(
                    configuration,
                    settings,
                    $"{fallbackLayerName}_{FormatHeightKey(heightKey)}",
                    ResolveLayerHeight(heightKey),
                    state,
                    heightKey);
                _runtimeLayers.Add(runtimeLayer);
                configuration.blueprintLayerFolders[0].blueprintLayers.Add(runtimeLayer.BlueprintLayer);
                configuration.buildLayerFolders[0].buildLayers.Add(runtimeLayer.BuildLayer);
            }
        }

        private RuntimeLayer CreateRuntimeLayer(
            Configuration configuration,
            FogVolumeStateTileSettings settings,
            string fallbackLayerName,
            float defaultLayerHeight,
            FogRuntimeState state,
            int heightKey)
        {
            settings ??= new FogVolumeStateTileSettings();
            settings.EnsureDefaults(fallbackLayerName);

            var blueprintLayer = ScriptableObject.CreateInstance<BlueprintLayer>();
            blueprintLayer.name = fallbackLayerName;
            blueprintLayer.layerName = fallbackLayerName;
            blueprintLayer.isEnabled = settings.Enabled;
            blueprintLayer.defaultLayerHeight = defaultLayerHeight;

            var buildLayer = ScriptableObject.CreateInstance<TilesBuildLayer>();
            buildLayer.name = fallbackLayerName;
            buildLayer.layerName = fallbackLayerName;
            buildLayer.isEnabled = settings.Enabled;
            buildLayer.configuration = configuration;
            buildLayer.currentBlueprintLayer = blueprintLayer;
            buildLayer.SetBlueprintLayer(blueprintLayer);
            buildLayer.useDualGrid = true;
            buildLayer.scaleTileToCellSize = true;
            buildLayer.layerYOffset = settings.LayerYOffset;
            buildLayer.scaleOffset = settings.ScaleOffset;
            buildLayer.generateFlatSurface = false;
            buildLayer.meshGenerationOverride = true;
            buildLayer.mergeTiles = false;
            buildLayer.shadowCastingMode = settings.ShadowCastingMode;
            buildLayer.objectLayer = settings.ObjectLayer;
            buildLayer.renderingLayer = settings.RenderingLayer;
            buildLayer.colliderType = settings.ColliderType;
            buildLayer.tileColliderHeight = Mathf.Max(0f, settings.TileColliderHeight);
            buildLayer.tileColliderExtrusionHeight = Mathf.Max(0f, settings.TileColliderExtrusionHeight);
            buildLayer.invertCollisionWalls = settings.InvertCollisionWalls;
            ApplyPresetSelections(buildLayer, settings);
            EnsureTileLayer(buildLayer);
            return new RuntimeLayer(blueprintLayer, buildLayer, state, heightKey);
        }

        private static void ApplyPresetSelections(TilesBuildLayer buildLayer, FogVolumeStateTileSettings settings)
        {
            buildLayer.tilePresetsTop ??= new List<TilesBuildLayer.TilePresetSelection>();
            buildLayer.tilePresetsMiddle ??= new List<TilesBuildLayer.TilePresetSelection>();
            buildLayer.tilePresetsBottom ??= new List<TilesBuildLayer.TilePresetSelection>();
            buildLayer.tilePresetsTop.Clear();
            buildLayer.tilePresetsMiddle.Clear();
            buildLayer.tilePresetsBottom.Clear();

            if (settings?.TileVariants == null)
                return;

            for (int i = 0; i < settings.TileVariants.Count; i++)
            {
                var variant = settings.TileVariants[i];
                if (variant == null || variant.Preset == null)
                    continue;

                var selection = new TilesBuildLayer.TilePresetSelection
                {
                    preset = variant.Preset,
                    weight = variant.NormalizedWeight,
                    tileHeight = Mathf.Max(0f, variant.TileHeight)
                };

                switch (variant.Slot)
                {
                    case FogVolumeTilePresetSlot.Middle:
                        buildLayer.tilePresetsMiddle.Add(selection);
                        break;
                    case FogVolumeTilePresetSlot.Bottom:
                        buildLayer.tilePresetsBottom.Add(selection);
                        break;
                    default:
                        buildLayer.tilePresetsTop.Add(selection);
                        break;
                }
            }
        }

        private static void EnsureTileLayer(TilesBuildLayer buildLayer)
        {
            buildLayer.tileLayers ??= new List<TilesBuildLayer.TileLayers>();
            if (buildLayer.tileLayers.Count == 0)
                buildLayer.tileLayers.Add(new TilesBuildLayer.TileLayers());

            var first = buildLayer.tileLayers[0] ?? new TilesBuildLayer.TileLayers();
            first.name = string.IsNullOrWhiteSpace(first.name) ? "Main" : first.name;
            first.ignoreFillTiles = false;
            first.layerOverrides ??= new List<TilesBuildLayer.TilePresetOverride>();
            buildLayer.tileLayers[0] = first;
        }

        private void ApplyCellsToLayer(BlueprintLayer layer, HashSet<Vector2> cells)
        {
            if (layer == null)
                return;

            layer.ClearLayer(false);
            if (cells == null || cells.Count == 0)
                return;

            _scratchCells.Clear();
            foreach (var cell in cells)
                _scratchCells.Add(cell);

            layer.AddCells(_scratchCells);
            _scratchCells.Clear();
        }

        private void ApplyCellsToRuntimeLayers()
        {
            Debug.Log($"{StartDiagTag} VolumeUpdater.ApplyCellsToRuntimeLayers runtimeLayers={_runtimeLayers.Count}, unexploredCells={_unexploredCells.Count}, exploredCells={_exploredCells.Count}, unexploredHeightLayers={_unexploredCellsByHeight.Count}, exploredHeightLayers={_exploredCellsByHeight.Count}.");
            for (int i = 0; i < _runtimeLayers.Count; i++)
            {
                var runtimeLayer = _runtimeLayers[i];
                ApplyCellsToLayer(runtimeLayer.BlueprintLayer, ResolveCells(runtimeLayer));
            }
        }

        private HashSet<Vector2> ResolveCells(RuntimeLayer runtimeLayer)
        {
            if (runtimeLayer == null)
                return null;

            var source = runtimeLayer.State == FogRuntimeState.Unexplored
                ? _unexploredCellsByHeight
                : _exploredCellsByHeight;

            return source.TryGetValue(runtimeLayer.HeightKey, out var cells) ? cells : null;
        }

        private void ExecuteTileWorldCreatorBuild(bool wasFullRebuild, int requestedDirtyTiles)
        {
            if (_manager == null || _runtimeConfiguration == null)
                return;

            bool isInitialBuild = !_hasBuiltAtLeastOnce;
            bool contextChanged = _worldContextChangedSinceBuild;
            _manager.configuration = _runtimeConfiguration;
            _manager.ExecuteBuildLayers(ExecutionMode.FromScratch);
            _hasBuiltAtLeastOnce = true;
            _worldContextChangedSinceBuild = false;
            Debug.Log($"{StartDiagTag} VolumeUpdater.ExecuteTileWorldCreatorBuild manager={_manager.name}, runtimeConfig={_runtimeConfiguration.name}, map={_mapWidth}x{_mapHeight}, rebuild={(wasFullRebuild ? "full" : "dirty")}, dirtyRequested={requestedDirtyTiles}, runtimeLayers={_runtimeLayers.Count}, unexploredCells={_unexploredCells.Count}, exploredCells={_exploredCells.Count}.");

            if (ShouldLogBuildSummary(contextChanged))
            {
                _loggedFirstBuild = true;
                Debug.Log(BuildSummaryLog(isInitialBuild, contextChanged, wasFullRebuild, requestedDirtyTiles));
            }
        }

        private bool ShouldLogBuildSummary(bool contextChanged)
        {
            if (_controller != null && !_controller.LogBuildSummary)
                return false;

            return !_loggedFirstBuild
                || contextChanged
                || (_controller != null && _controller.LogEveryVolumeUpdate);
        }

        private string BuildSummaryLog(
            bool isInitialBuild,
            bool contextChanged,
            bool wasFullRebuild,
            int requestedDirtyTiles)
        {
            var settings = GetSettings();
            var volume = settings?.Volume;
            int totalCells = Mathf.Max(1, _mapWidth * _mapHeight);
            int visibleCells = Mathf.Max(0, totalCells - _unexploredCells.Count - _exploredCells.Count);

            var sb = new StringBuilder(2048);
            sb.Append(LogTag)
                .Append(" TWC fog volume summary")
                .Append(" | operation=").Append(isInitialBuild ? "initial-build" : "update")
                .Append(" | rebuild=").Append(wasFullRebuild ? "full" : "dirty")
                .Append(" | dirtyRequested=").Append(requestedDirtyTiles)
                .Append(" | contextChanged=").Append(contextChanged)
                .Append(" | updateMode=").Append(ResolveUpdateMode())
                .Append(" | interval=").Append(ResolveRebuildIntervalSeconds().ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)).Append('s')
                .AppendLine();

            sb.Append("  Scene: controller='").Append(_controller != null ? _controller.name : "null")
                .Append("', manager='").Append(_manager != null ? _manager.name : "null")
                .Append("', settings='").Append(settings != null ? settings.name : "null")
                .Append("', runtimeConfig='").Append(_runtimeConfiguration != null ? _runtimeConfiguration.name : "null")
                .AppendLine("'");

            sb.Append("  World: map=").Append(_mapWidth).Append('x').Append(_mapHeight)
                .Append(", cellSize=").Append((_runtimeConfiguration != null ? _runtimeConfiguration.cellSize : _context.CellSize).ToString("0.###", System.Globalization.CultureInfo.InvariantCulture))
                .Append(", projection=").Append(_context.ProjectionMode)
                .Append(", render=").Append(_context.RenderMode)
                .Append(", topology=").Append(_context.GridTopology)
                .Append(", bounds=").Append(FormatBounds(_context))
                .AppendLine();

            sb.Append("  Height: source=").Append(volume != null ? volume.HeightSource : FogVolumeHeightSource.TerrainLevelMapThenHeightMap)
                .Append(", heightMap=").Append(FormatMapSize(_context.HeightMap))
                .Append(", terrainLevelMap=").Append(FormatMapSize(_context.TerrainLevelMap))
                .Append(", snap=").Append((volume != null ? volume.HeightLayerSnap : 0.01f).ToString("0.###", System.Globalization.CultureInfo.InvariantCulture))
                .Append(", clearance=").Append(((volume != null ? volume.TopClearance : 0.08f) + (_controller != null ? _controller.AdditionalTopClearance : 0f)).ToString("0.###", System.Globalization.CultureInfo.InvariantCulture))
                .Append(", heightKeys=").Append(_heightByKey.Count)
                .Append(FormatHeightRange())
                .AppendLine();

            sb.Append("  Fog Cells: total=").Append(totalCells)
                .Append(", unexplored=").Append(_unexploredCells.Count)
                .Append(", explored=").Append(_exploredCells.Count)
                .Append(", visible=").Append(visibleCells)
                .Append(", unexploredHeightLayers=").Append(CountNonEmptyHeightLayers(_unexploredCellsByHeight))
                .Append(", exploredHeightLayers=").Append(CountNonEmptyHeightLayers(_exploredCellsByHeight))
                .AppendLine();

            sb.Append("  State Config: ")
                .Append(FormatStateSettings("Unexplored", volume?.Unexplored))
                .Append(" | ")
                .Append(FormatStateSettings("Explored", volume?.Explored))
                .AppendLine();

            sb.Append("  Runtime Layers: count=").Append(_runtimeLayers.Count);
            for (int i = 0; i < _runtimeLayers.Count; i++)
            {
                var layer = _runtimeLayers[i];
                sb.AppendLine()
                    .Append("    - ").Append(layer.State)
                    .Append(" name='").Append(layer.BuildLayer != null ? layer.BuildLayer.layerName : "null")
                    .Append("', heightKey=").Append(layer.HeightKey)
                    .Append(", height=").Append(ResolveLayerHeight(layer.HeightKey).ToString("0.###", System.Globalization.CultureInfo.InvariantCulture))
                    .Append(", cells=").Append(ResolveCells(layer)?.Count ?? 0)
                    .Append(", dualGrid=").Append(layer.BuildLayer != null && layer.BuildLayer.useDualGrid)
                    .Append(", scaleToCell=").Append(layer.BuildLayer != null && layer.BuildLayer.scaleTileToCellSize)
                    .Append(", collider=").Append(layer.BuildLayer != null ? layer.BuildLayer.colliderType.ToString() : "null")
                    .Append(", presets=").Append(CountBuildLayerPresets(layer.BuildLayer));
            }

            return sb.ToString();
        }

        private static string FormatBounds(FogWorldVisualContext context)
        {
            if (!context.HasMapWorldBounds)
                return "none";

            var bounds = context.MapWorldBounds;
            return $"center={FormatVector(bounds.center)}, size={FormatVector(bounds.size)}";
        }

        private static string FormatVector(Vector3 value)
        {
            return $"({value.x.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}, {value.y.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}, {value.z.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)})";
        }

        private static string FormatMapSize(float[,] map)
            => map != null ? $"{map.GetLength(0)}x{map.GetLength(1)}" : "null";

        private static string FormatMapSize(int[,] map)
            => map != null ? $"{map.GetLength(0)}x{map.GetLength(1)}" : "null";

        private string FormatHeightRange()
        {
            if (_heightByKey.Count == 0)
                return ", range=none";

            bool hasValue = false;
            float min = 0f;
            float max = 0f;
            foreach (var height in _heightByKey.Values)
            {
                if (!hasValue)
                {
                    min = height;
                    max = height;
                    hasValue = true;
                    continue;
                }

                min = Mathf.Min(min, height);
                max = Mathf.Max(max, height);
            }

            return $", range={min.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}..{max.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}";
        }

        private static int CountNonEmptyHeightLayers(Dictionary<int, HashSet<Vector2>> cellsByHeight)
        {
            if (cellsByHeight == null)
                return 0;

            int count = 0;
            foreach (var pair in cellsByHeight)
            {
                if (pair.Value != null && pair.Value.Count > 0)
                    count++;
            }

            return count;
        }

        private static string FormatStateSettings(string label, FogVolumeStateTileSettings settings)
        {
            if (settings == null)
                return $"{label}=null";

            CountPresetVariants(settings, out int total, out int assigned, out int usable);
            return $"{label}[enabled={settings.Enabled}, layer='{settings.LayerName}', variants={total}, assigned={assigned}, usableDualGrid={usable}]";
        }

        private static void CountPresetVariants(FogVolumeStateTileSettings settings, out int total, out int assigned, out int usable)
        {
            total = 0;
            assigned = 0;
            usable = 0;
            if (settings?.TileVariants == null)
                return;

            total = settings.TileVariants.Count;
            for (int i = 0; i < settings.TileVariants.Count; i++)
            {
                var variant = settings.TileVariants[i];
                if (variant?.Preset == null)
                    continue;

                assigned++;
                if (FogOfWarSettings.HasUsableDualGridPreset(variant.Preset))
                    usable++;
            }
        }

        private static int CountBuildLayerPresets(TilesBuildLayer buildLayer)
        {
            if (buildLayer == null)
                return 0;

            return (buildLayer.tilePresetsTop?.Count ?? 0)
                + (buildLayer.tilePresetsMiddle?.Count ?? 0)
                + (buildLayer.tilePresetsBottom?.Count ?? 0);
        }

        private void RequestVisualRebuild()
        {
            _fullRebuildRequested = true;
            _hasPendingVisualWork = _lastFogService != null;
            LogUpdaterOnce(ref _loggedRebuildRequest, $"RequestVisualRebuild: hasLastFogService={_lastFogService != null}, pending={_hasPendingVisualWork}, controller={(_controller != null ? _controller.name : "null")}, manager={(_manager != null ? _manager.name : "null")}.");
        }

        private void LogUpdaterOnce(ref bool logged, string message)
        {
            if (!ShouldLogLifecycle(logged))
                return;

            logged = true;
            Debug.Log($"{LogTag} {message}");
        }

        private bool ShouldLogLifecycle(bool alreadyLogged)
        {
            if (_controller != null)
            {
                if (!_controller.LogBuildSummary && !_controller.LogValidationWarnings)
                    return false;

                return !alreadyLogged || _controller.LogEveryVolumeUpdate;
            }

            return !alreadyLogged;
        }

        private FogOfWarSettings GetSettings()
            => _controller != null && _controller.Settings != null ? _controller.Settings : _injectedSettings;

        private FogVolumeUpdateMode ResolveUpdateMode()
            => _controller != null ? _controller.EffectiveUpdateMode : (GetSettings()?.Volume.UpdateMode ?? FogVolumeUpdateMode.DebouncePerFrame);

        private float ResolveRebuildIntervalSeconds()
            => _controller != null ? _controller.EffectiveRebuildIntervalSeconds : Mathf.Max(0.02f, GetSettings()?.Volume.RebuildIntervalSeconds ?? 0.1f);

        private int ResolveHeightKey(Vector2Int tile)
        {
            float height = ResolveGeneratedSurfaceHeight(tile);
            float snap = Mathf.Max(0.001f, GetSettings()?.Volume.HeightLayerSnap ?? 0.01f);
            int key = Mathf.RoundToInt(height / snap);
            if (!_heightByKey.ContainsKey(key))
                _heightByKey.Add(key, key * snap);

            return key;
        }

        private float ResolveLayerHeight(int heightKey)
        {
            if (!_heightByKey.TryGetValue(heightKey, out float surfaceHeight))
                surfaceHeight = 0f;

            float clearance = GetSettings()?.Volume.TopClearance ?? 0.08f;
            if (_controller != null)
                clearance += _controller.AdditionalTopClearance;

            float managerY = _manager != null ? _manager.transform.position.y : 0f;
            return surfaceHeight + Mathf.Max(0f, clearance) - managerY;
        }

        private float ResolveGeneratedSurfaceHeight(Vector2Int tile)
        {
            var settings = GetSettings()?.Volume;
            var source = settings?.HeightSource ?? FogVolumeHeightSource.TerrainLevelMapThenHeightMap;

            switch (source)
            {
                case FogVolumeHeightSource.HeightMapThenTerrainLevelMap:
                    if (TryResolveHeightMapValue(tile, out float heightMapValue))
                        return heightMapValue;
                    if (TryResolveTerrainLevelValue(tile, out float terrainHeightValue))
                        return terrainHeightValue;
                    break;
                case FogVolumeHeightSource.Flat:
                    return 0f;
                default:
                    if (TryResolveTerrainLevelValue(tile, out terrainHeightValue))
                        return terrainHeightValue;
                    if (TryResolveHeightMapValue(tile, out heightMapValue))
                        return heightMapValue;
                    break;
            }

            return 0f;
        }

        private bool TryResolveTerrainLevelValue(Vector2Int tile, out float height)
        {
            height = 0f;
            if (_context.TerrainLevelMap == null
                || tile.x < 0
                || tile.y < 0
                || tile.x >= _context.TerrainLevelMap.GetLength(0)
                || tile.y >= _context.TerrainLevelMap.GetLength(1))
            {
                return false;
            }

            float step = Mathf.Max(0.001f, GetSettings()?.Volume.TerrainLevelHeightStep ?? 1f);
            height = Mathf.Max(0, _context.TerrainLevelMap[tile.x, tile.y]) * step;
            return IsFinite(height);
        }

        private bool TryResolveHeightMapValue(Vector2Int tile, out float height)
        {
            height = 0f;
            if (_context.HeightMap == null
                || tile.x < 0
                || tile.y < 0
                || tile.x >= _context.HeightMap.GetLength(0)
                || tile.y >= _context.HeightMap.GetLength(1))
            {
                return false;
            }

            height = _context.HeightMap[tile.x, tile.y];
            return IsFinite(height);
        }

        private static void ConfigureManagerTransform(Transform managerTransform, FogWorldVisualContext context, float cellSize)
        {
            if (managerTransform == null || !context.HasMapWorldBounds)
                return;

            Bounds bounds = context.MapWorldBounds;
            float halfCell = Mathf.Max(0.0001f, cellSize) * 0.5f;
            managerTransform.position = new Vector3(
                bounds.min.x + halfCell,
                managerTransform.position.y,
                bounds.min.z + halfCell);
        }

        private void DisposeRuntimeConfiguration()
        {
            if (_manager != null && _manager.configuration == _runtimeConfiguration)
                _manager.configuration = _previousManagerConfiguration;

            for (int i = 0; i < _runtimeLayers.Count; i++)
            {
                DestroyRuntimeObject(_runtimeLayers[i]?.BuildLayer);
                DestroyRuntimeObject(_runtimeLayers[i]?.BlueprintLayer);
            }

            DestroyRuntimeObject(_runtimeConfiguration);

            _runtimeLayers.Clear();
            _runtimeConfiguration = null;
        }

        private static void DestroyRuntimeObject(UnityEngine.Object obj)
        {
            if (obj == null)
                return;

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(obj);
            else
                UnityEngine.Object.DestroyImmediate(obj);
        }

        private bool IsInBounds(Vector2Int tile)
            => tile.x >= 0 && tile.x < _mapWidth && tile.y >= 0 && tile.y < _mapHeight;

        private static Vector2 ToCell(Vector2Int tile)
            => new Vector2(tile.x, tile.y);

        private static bool IsStateEnabled(FogVolumeStateTileSettings settings, bool fallback)
            => settings != null ? settings.Enabled : fallback;

        private static FogWorldVisualContext CreateFallbackContext(int width, int height)
        {
            return new FogWorldVisualContext(
                width,
                height,
                Kruty1918.Moyva.Grid.API.GridTopology.Orthogonal,
                Kruty1918.Moyva.Grid.API.GridProjectionMode.Orthographic3D,
                Kruty1918.Moyva.Grid.API.GridRenderMode.Mesh3D,
                Kruty1918.Moyva.Grid.API.GridNeighborhoodMode.Moore8,
                1f,
                false,
                default,
                null,
                null);
        }

        private static bool ApproximatelyBounds(Bounds a, Bounds b)
            => ApproximatelyVector(a.center, b.center) && ApproximatelyVector(a.size, b.size);

        private static bool ApproximatelyVector(Vector3 a, Vector3 b)
            => Mathf.Approximately(a.x, b.x)
                && Mathf.Approximately(a.y, b.y)
                && Mathf.Approximately(a.z, b.z);

        private static bool IsFinite(float value)
            => !float.IsNaN(value) && !float.IsInfinity(value);

        private static string FormatHeightKey(int heightKey)
            => heightKey.ToString(System.Globalization.CultureInfo.InvariantCulture).Replace("-", "m");

        private void LogMissingControllerOnce()
        {
            if (_loggedMissingController)
                return;

            _loggedMissingController = true;
            Debug.LogWarning($"{LogTag} No FogOfWarVolumeController is registered. Fog logic remains active, but 3D fog volume will not be built.");
        }

        private void LogMissingManagerOnce()
        {
            if (_loggedMissingManager)
                return;

            _loggedMissingManager = true;
            Debug.LogWarning($"{LogTag} FogOfWarVolumeController has no TileWorldCreatorManager assigned. Fog logic remains active, but 3D fog volume will not be built.");
        }

        private void LogMissingSettingsOnce()
        {
            if (_loggedMissingSettings || !ShouldLogValidationWarnings())
                return;

            _loggedMissingSettings = true;
            Debug.LogError($"{LogTag} FogOfWarSettings is missing. Runtime fog state can still update, but TWC fog volume has no configured TilePresets.");
        }

        private void LogMissingFogServiceOnce()
        {
            if (_loggedMissingFogService || !ShouldLogValidationWarnings())
                return;

            _loggedMissingFogService = true;
            Debug.LogWarning($"{LogTag} IFogOfWarService is not available yet. 3D fog volume build is queued until fog gameplay state is initialized.");
        }

        private void LogRuntimeLayerValidation()
        {
            if (!ShouldLogValidationWarnings())
                return;

            var volume = GetSettings()?.Volume;
            bool unexploredNeedsPreset = _unexploredCells.Count > 0 && IsStateEnabled(volume?.Unexplored, true);
            bool exploredNeedsPreset = _exploredCells.Count > 0 && IsStateEnabled(volume?.Explored, true);

            if (unexploredNeedsPreset && !HasUsablePreset(volume?.Unexplored) && !_loggedUnexploredPresetProblem)
            {
                _loggedUnexploredPresetProblem = true;
                Debug.LogError($"{LogTag} Unexplored fog has {_unexploredCells.Count} cells, but no usable dual-grid TilePreset is configured. Assign at least one preset in FogOfWarSettings > TWC Volume > Unexplored Fog.");
            }

            if (exploredNeedsPreset && !HasUsablePreset(volume?.Explored) && !_loggedExploredPresetProblem)
            {
                _loggedExploredPresetProblem = true;
                Debug.LogError($"{LogTag} Explored fog has {_exploredCells.Count} cells, but no usable dual-grid TilePreset is configured. Assign at least one preset in FogOfWarSettings > TWC Volume > Explored Fog.");
            }

            if (_runtimeLayers.Count == 0
                && (_unexploredCells.Count > 0 || _exploredCells.Count > 0)
                && !_loggedNoRuntimeLayers)
            {
                _loggedNoRuntimeLayers = true;
                Debug.LogWarning($"{LogTag} No runtime TWC layers were created although fog cells exist. states: unexplored={_unexploredCells.Count}, explored={_exploredCells.Count}, settings={(GetSettings() != null ? GetSettings().name : "null")}.");
            }
        }

        private bool ShouldLogValidationWarnings()
            => _controller == null || _controller.LogValidationWarnings;

        private static bool HasUsablePreset(FogVolumeStateTileSettings settings)
        {
            if (settings?.TileVariants == null)
                return false;

            for (int i = 0; i < settings.TileVariants.Count; i++)
            {
                var variant = settings.TileVariants[i];
                if (variant?.Preset != null && FogOfWarSettings.HasUsableDualGridPreset(variant.Preset))
                    return true;
            }

            return false;
        }

        private enum FogRuntimeState
        {
            Unexplored,
            Explored
        }

        private sealed class RuntimeLayer
        {
            /// <summary>
            /// Описує пару runtime blueprint/build layer для конкретного fog state і height bucket.
            /// </summary>
            /// <param name="blueprintLayer">Runtime blueprint layer.</param>
            /// <param name="buildLayer">Пов'язаний runtime build layer.</param>
            /// <param name="state">Fog state, який цей layer репрезентує.</param>
            /// <param name="heightKey">Нормалізований ключ висотного bucket-а.</param>
            public RuntimeLayer(BlueprintLayer blueprintLayer, TilesBuildLayer buildLayer, FogRuntimeState state, int heightKey)
            {
                BlueprintLayer = blueprintLayer;
                BuildLayer = buildLayer;
                State = state;
                HeightKey = heightKey;
            }

            /// <summary>
            /// Runtime blueprint layer для конкретного fog state.
            /// </summary>
            public BlueprintLayer BlueprintLayer { get; }

            /// <summary>
            /// Runtime build layer, який TWC фактично будує у сцені.
            /// </summary>
            public TilesBuildLayer BuildLayer { get; }

            /// <summary>
            /// Fog state, який репрезентує цей runtime layer.
            /// </summary>
            public FogRuntimeState State { get; }

            /// <summary>
            /// Bucket ключ для висоти, на якій будується цей layer.
            /// </summary>
            public int HeightKey { get; }
        }

        /// <summary>
        /// Мінімальний fog service для startup/preview build path, коли реальний gameplay fog service ще не готовий.
        /// </summary>
        private sealed class StartupFogService : IFogOfWarService
        {
            private readonly int _width;
            private readonly int _height;
            private readonly bool _hasVisibleReveal;
            private readonly Vector2Int _visibleCenter;
            private readonly int _visibleRadius;
            private readonly FogRevealShape _visibleShape;

            /// <summary>
            /// Створює startup fog service без початкової visible області.
            /// </summary>
            /// <param name="width">Ширина карти.</param>
            /// <param name="height">Висота карти.</param>
            public StartupFogService(int width, int height)
            {
                _width = Mathf.Max(1, width);
                _height = Mathf.Max(1, height);
            }

            /// <summary>
            /// Створює startup fog service з необов'язковою початковою visible областю.
            /// </summary>
            /// <param name="width">Ширина карти.</param>
            /// <param name="height">Висота карти.</param>
            /// <param name="visibleCenter">Центр visible області.</param>
            /// <param name="visibleRadius">Радіус visible області.</param>
            /// <param name="visibleShape">Форма visible області.</param>
            /// <param name="keepVisible">Чи має область залишатися visible.</param>
            public StartupFogService(int width, int height, Vector2Int visibleCenter, int visibleRadius, FogRevealShape visibleShape, bool keepVisible)
                : this(width, height)
            {
                _hasVisibleReveal = keepVisible;
                _visibleCenter = visibleCenter;
                _visibleRadius = Mathf.Max(0, visibleRadius);
                _visibleShape = visibleShape;
            }

            /// <summary>
            /// Startup stub не потребує окремої ініціалізації.
            /// </summary>
            public void Initialize(int width, int height) { }
            /// <summary>
            /// Startup stub не відстежує runtime юнітів.
            /// </summary>
            public void RegisterUnit(string unitId, Vector2Int position, int visionRange) { }
            /// <summary>
            /// Startup stub не оновлює runtime vision ranges.
            /// </summary>
            public void UpdateUnitVisionRange(string unitId, int visionRange) { }
            /// <summary>
            /// Startup stub не реєструє fixed vision areas окремо.
            /// </summary>
            public void RegisterFixedVisionArea(string areaId, Vector2Int position, int visionRange, FogRevealShape shape) { }
            /// <summary>
            /// Startup stub не приймає додаткові runtime reveal operations.
            /// </summary>
            public void RevealArea(Vector2Int center, int radius, FogRevealShape shape, bool keepVisible, string visibleAreaId = null) { }
            /// <summary>
            /// Startup stub не відстежує переміщення юнітів.
            /// </summary>
            public void UpdateUnitPosition(string unitId, Vector2Int newPosition) { }
            /// <summary>
            /// Startup stub не тримає runtime unit registrations.
            /// </summary>
            public void UnregisterUnit(string unitId) { }

            /// <summary>
            /// Повертає startup fog state для клітинки: visible лише всередині початкової області, інакше unexplored.
            /// </summary>
            /// <param name="position">Клітинка, яку запитують.</param>
            /// <returns>Startup fog state для цієї клітинки.</returns>
            public FogStateType GetFogState(Vector2Int position)
            {
                if (_hasVisibleReveal && IsInsideVisibleReveal(position))
                    return FogStateType.Visible;

                return FogStateType.Unexplored;
            }

            /// <summary>
            /// Перевіряє, чи клітинка входить до початкової visible області.
            /// </summary>
            public bool IsVisible(Vector2Int position) => GetFogState(position) == FogStateType.Visible;

            /// <summary>
            /// У startup stub explored трактується так само, як visible.
            /// </summary>
            public bool IsExplored(Vector2Int position) => IsVisible(position);

            /// <summary>
            /// Повертає порожній explored snapshot для startup build path.
            /// </summary>
            public bool[,] GetExploredSnapshot() => new bool[_width, _height];

            /// <summary>
            /// Startup stub не завантажує snapshot-и.
            /// </summary>
            public void LoadFromSnapshot(bool[,] explored) { }

            /// <summary>
            /// Startup stub не накопичує dirty tiles.
            /// </summary>
            public IReadOnlyCollection<Vector2Int> GetLastDirtyTiles() => Array.Empty<Vector2Int>();

            private bool IsInsideVisibleReveal(Vector2Int position)
            {
                if (position.x < 0 || position.y < 0 || position.x >= _width || position.y >= _height)
                    return false;

                int dx = position.x - _visibleCenter.x;
                int dy = position.y - _visibleCenter.y;
                float radiusWithCellCoverage = _visibleRadius + 0.5f;
                float sqrRadius = radiusWithCellCoverage * radiusWithCellCoverage;
                return _visibleShape switch
                {
                    FogRevealShape.Square => Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) <= _visibleRadius,
                    FogRevealShape.Diamond => Mathf.Abs(dx) + Mathf.Abs(dy) <= _visibleRadius,
                    _ => dx * dx + dy * dy <= sqrRadius,
                };
            }
        }
    }
}
