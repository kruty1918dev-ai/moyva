using System;
using System.Collections.Generic;
using System.Text;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Heavyweight runtime engine for TWC dual-grid fog volume visuals.
    /// Owns caches, runtime configuration clone and build orchestration.
    /// </summary>
    internal sealed class FogVolumeVisualUpdateEngine : IFogVisualUpdater, IFogVolumeRuntimeUpdater, ITickable, IDisposable
    {
        private const string LogTag = "[FogOfWarVolume]";
        private const string StartDiagTag = "[MoyvaFogStartDiag]";
        private const string StartupChainTag = "[MoyvaStartupChain]";
        private const string ClusterDiagTag = "[MoyvaFogClusterDiag]";
        private const string FolderName = "Fog Volume";
        private const int TargetFogClusterBudget = 32;
        private const int TargetFogHeightLayerBudget = 8;
        private const int MinimumFogClusterCellSize = 8;

        private readonly FogOfWarSettings _injectedSettings;
        private readonly IFogVolumePendingWorkState _pendingWorkState;
        private readonly IFogVolumePendingWorkRequests _pendingWorkRequests;
        private readonly IFogVolumePendingWorkMaintenance _pendingWorkMaintenance;
        private readonly IFogVisualUpdateScheduleState _visualUpdateScheduleState;
        private readonly IFogVisualUpdateTickGate _visualUpdateTickGate;
        private readonly IFogVisualUpdateRequestPolicy _visualUpdateRequestPolicy;
        private readonly HashSet<Vector2> _scratchCells = new HashSet<Vector2>();
        private readonly IFogVolumeStateCache _stateCache;
        private readonly IFogStartupFogServiceFactory _startupFogServiceFactory;
        private readonly Dictionary<int, float> _heightByKey = new Dictionary<int, float>();
        private readonly List<RuntimeLayer> _runtimeLayers = new List<RuntimeLayer>();
        private readonly IFogDirtyClusterTracker _dirtyClusterTracker;
        private readonly IFogClusteredVolumeRenderer _clusteredVolumeRenderer;
        private readonly IFogVolumeOutputCleaner _outputCleaner;

        private FogOfWarVolumeController _controller;
        private TileWorldCreatorManager _manager;
        private Configuration _previousManagerConfiguration;
        private Configuration _runtimeConfiguration;
        private FogWorldVisualContext _context;
        private bool _runtimeConfigurationDirty = true;
        private bool _hasBuiltAtLeastOnce;
        private bool _worldContextChangedSinceBuild;
        private bool _loggedFirstBuild;
        private int _mapWidth = 1;
        private int _mapHeight = 1;
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
        private float _cachedEffectiveHeightLayerSnap = -1f;

        /// <summary>
        /// Створює updater для volume visual path.
        /// У runtime зазвичай отримує settings через Zenject, а у preview може бути створений локально.
        /// </summary>
        /// <param name="settings">Fog settings для tuning і побудови runtime layers.</param>
        public FogVolumeVisualUpdateEngine(
            [InjectOptional] FogOfWarSettings settings = null,
            [InjectOptional] IFogVolumePendingWorkState pendingWorkState = null,
            [InjectOptional] IFogVolumePendingWorkRequests pendingWorkRequests = null,
            [InjectOptional] IFogVolumePendingWorkMaintenance pendingWorkMaintenance = null,
            [InjectOptional] IFogVisualUpdateScheduleState visualUpdateScheduleState = null,
            [InjectOptional] IFogVisualUpdateTickGate visualUpdateTickGate = null,
            [InjectOptional] IFogVisualUpdateRequestPolicy visualUpdateRequestPolicy = null,
            [InjectOptional] IFogVisualUpdateSchedulerFactory visualUpdateSchedulerFactory = null,
            [InjectOptional] IFogVolumeStateCache stateCache = null,
            [InjectOptional] IFogStartupFogServiceFactory startupFogServiceFactory = null,
            [InjectOptional] IFogDirtyClusterTracker dirtyClusterTracker = null,
            [InjectOptional] IFogClusteredVolumeRenderer clusteredVolumeRenderer = null,
            [InjectOptional] IFogVolumeOutputCleaner outputCleaner = null)
        {
            _injectedSettings = settings;
            var fallbackQueue = ResolvePendingWorkQueueFallback(pendingWorkState, pendingWorkRequests, pendingWorkMaintenance);
            _pendingWorkState = pendingWorkState ?? fallbackQueue;
            _pendingWorkRequests = pendingWorkRequests ?? fallbackQueue;
            _pendingWorkMaintenance = pendingWorkMaintenance ?? fallbackQueue;
            var fallbackScheduler = ResolveVisualUpdateSchedulerFallback(
                visualUpdateScheduleState,
                visualUpdateTickGate,
                visualUpdateRequestPolicy,
                visualUpdateSchedulerFactory);
            _visualUpdateScheduleState = visualUpdateScheduleState ?? fallbackScheduler;
            _visualUpdateTickGate = visualUpdateTickGate ?? fallbackScheduler;
            _visualUpdateRequestPolicy = visualUpdateRequestPolicy ?? fallbackScheduler;
            _stateCache = stateCache ?? new FogVolumeStateCache();
            _startupFogServiceFactory = startupFogServiceFactory ?? new FogStartupFogServiceFactory();
            _dirtyClusterTracker = dirtyClusterTracker;
            _clusteredVolumeRenderer = clusteredVolumeRenderer;
            _outputCleaner = outputCleaner ?? new FogVolumeOutputCleaner();
            if (settings != null)
                Debug.Log($"{LogTag} Updater constructed: injectedSettings='{settings.name}'.");
            else
                Debug.LogWarning($"{LogTag} Updater constructed without injected FogOfWarSettings. It will use settings from FogOfWarVolumeController if one attaches.");
        }

        private static FogVolumePendingWorkQueue ResolvePendingWorkQueueFallback(
            IFogVolumePendingWorkState pendingWorkState,
            IFogVolumePendingWorkRequests pendingWorkRequests,
            IFogVolumePendingWorkMaintenance pendingWorkMaintenance)
        {
            return pendingWorkState as FogVolumePendingWorkQueue
                ?? pendingWorkRequests as FogVolumePendingWorkQueue
                ?? pendingWorkMaintenance as FogVolumePendingWorkQueue
                ?? new FogVolumePendingWorkQueue();
        }

        private FogVisualUpdateScheduler ResolveVisualUpdateSchedulerFallback(
            IFogVisualUpdateScheduleState visualUpdateScheduleState,
            IFogVisualUpdateTickGate visualUpdateTickGate,
            IFogVisualUpdateRequestPolicy visualUpdateRequestPolicy,
            IFogVisualUpdateSchedulerFactory visualUpdateSchedulerFactory)
        {
            return visualUpdateScheduleState as FogVisualUpdateScheduler
                ?? visualUpdateTickGate as FogVisualUpdateScheduler
                ?? visualUpdateRequestPolicy as FogVisualUpdateScheduler
                ?? (visualUpdateSchedulerFactory ?? new FogVisualUpdateSchedulerFactory())
                    .Create(ResolveUpdateMode, ResolveRebuildIntervalSeconds);
        }

        /// <summary>
        /// Діагностична кількість unexplored-клітинок у поточному кеші visual state.
        /// </summary>
        internal int DebugUnexploredCellCount => _stateCache.UnexploredCellCount;

        /// <summary>
        /// Діагностична кількість explored-клітинок у поточному кеші visual state.
        /// </summary>
        internal int DebugExploredCellCount => _stateCache.ExploredCellCount;

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
            _clusteredVolumeRenderer?.ConfigureRoot(_manager != null ? _manager.transform : controller.transform);
            if (_runtimeConfiguration == null)
                _previousManagerConfiguration = _manager != null ? _manager.configuration : null;
            _runtimeConfigurationDirty = true;
            _loggedMissingController = false;
            _loggedMissingManager = false;
            _loggedMissingSettings = false;
            _loggedNoRuntimeLayers = false;
            _loggedUnexploredPresetProblem = false;
            _loggedExploredPresetProblem = false;
            Debug.Log($"{StartDiagTag} VolumeUpdater.AttachController controller={(controller != null ? controller.name : "null")}, manager={(_manager != null ? _manager.name : "null")}, settings={(controller.Settings != null ? controller.Settings.name : "null")}, hasLastFogService={_pendingWorkState.FogService != null}, hasBuilt={_hasBuiltAtLeastOnce}.");
            LogUpdaterOnce(ref _loggedAttach, $"AttachController: controller='{controller.name}', manager={(_manager != null ? _manager.name : "null")}, settings={(controller.Settings != null ? controller.Settings.name : "null")}, hasLastFogService={_pendingWorkState.FogService != null}, hasBuilt={_hasBuiltAtLeastOnce}.");
            RequestVisualRebuild();
            if (_pendingWorkState.FogService != null && !_hasBuiltAtLeastOnce)
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
            _pendingWorkMaintenance.SetMapSize(_mapWidth, _mapHeight);
            _stateCache.InitializeMapSize(_mapWidth, _mapHeight);
            if (context.IsValid)
                _context = context.WithSize(_mapWidth, _mapHeight);
            else if (!_context.IsValid)
                _context = CreateFallbackContext(_mapWidth, _mapHeight);
            else
                _context = _context.WithSize(_mapWidth, _mapHeight);

            _runtimeConfigurationDirty = true;
            _pendingWorkRequests.RequestFullRebuild();
            _hasBuiltAtLeastOnce = false;
            _worldContextChangedSinceBuild = true;
            _loggedNoRuntimeLayers = false;
            _loggedUnexploredPresetProblem = false;
            _loggedExploredPresetProblem = false;
            _loggedTickWaitingForInterval = false;
            _cachedEffectiveHeightLayerSnap = -1f;
            _pendingWorkMaintenance.ClearCellChanges();
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
            _pendingWorkMaintenance.SetMapSize(_mapWidth, _mapHeight);
            _stateCache.InitializeMapSize(_mapWidth, _mapHeight);

            if (sizeChanged || cellSizeChanged || boundsChanged)
                _runtimeConfigurationDirty = true;

            _worldContextChangedSinceBuild = true;
            _pendingWorkRequests.RequestFullRebuild();
            _loggedWorldContext = false;
            _cachedEffectiveHeightLayerSnap = -1f;
            _pendingWorkMaintenance.ClearCellChanges();
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
            if (_pendingWorkState.FogService != null)
            {
                Debug.Log($"{LogTag} PreviewRevealArea skipped: gameplay fog service is active and will drive visual state.");
                return;
            }

            if (!_context.IsValid)
                _context = CreateFallbackContext(_mapWidth, _mapHeight);

            Debug.Log($"{LogTag} PreviewRevealArea center={center}, radius={Mathf.Max(0, radius)}, shape={shape}, keepVisible={keepVisible}, context={_context.Width}x{_context.Height}.");
            Initialize(_context.Width, _context.Height, _context);
            RebuildFullVisual(_startupFogServiceFactory.Create(_context.Width, _context.Height, center, radius, shape, keepVisible));
        }

        /// <summary>
        /// Приймає dirty-клітинки від gameplay fog service і планує часткову або негайну visual rebuild.
        /// </summary>
        /// <param name="fogService">Gameplay source of truth для fog state.</param>
        /// <param name="dirtyTiles">Клітинки, чий стан змінився з останнього update.</param>
        public void UpdateDirtyTiles(IFogOfWarService fogService, IEnumerable<Vector2Int> dirtyTiles)
        {
            if (fogService != null)
                _loggedMissingFogService = false;
            int accepted = _pendingWorkRequests.RequestDirtyTiles(fogService, dirtyTiles, out int requested);
            Debug.Log($"{StartDiagTag} VolumeUpdater.UpdateDirtyTiles requested={requested}, accepted={accepted}, hasFogService={fogService != null}, controller={(_controller != null ? _controller.name : "null")}, map={_mapWidth}x{_mapHeight}, fullRebuildRequested={_pendingWorkState.FullRebuildRequested}.");
            if (ShouldLogLifecycle(_loggedDirtyUpdate))
            {
                _loggedDirtyUpdate = true;
                Debug.Log($"{LogTag} UpdateDirtyTiles: fogService={(fogService != null ? fogService.GetType().Name : "null")}, requested={requested}, acceptedPending={accepted}, map={_mapWidth}x{_mapHeight}, updateMode={_visualUpdateScheduleState.CurrentUpdateMode}, immediate={_visualUpdateRequestPolicy.ShouldExecuteImmediateRequest()}, controller={(_controller != null ? _controller.name : "null")}.");
            }
            if (_visualUpdateRequestPolicy.ShouldExecuteImmediateRequest())
                ExecutePendingVisualWork();
        }

        public void RequestCellsUpdate(
            IFogOfWarService fogService,
            IReadOnlyList<FogCellVisualChange> changes,
            FogWorldVisualContext context)
        {
            if (fogService != null)
                _loggedMissingFogService = false;

            if (context.IsValid)
            {
                _context = context.WithSize(context.Width, context.Height);
                _mapWidth = context.Width;
                _mapHeight = context.Height;
                _pendingWorkMaintenance.SetMapSize(_mapWidth, _mapHeight);
            }

            int accepted = _pendingWorkRequests.RequestCellsUpdate(fogService, changes);
            Debug.Log($"{ClusterDiagTag} RequestCellsUpdate changes={(changes?.Count ?? 0)}, accepted={accepted}, context={_context.Width}x{_context.Height}, hasFogService={fogService != null}.");
            if (_visualUpdateRequestPolicy.ShouldExecuteImmediateRequest())
                ExecutePendingVisualWork();
        }

        /// <summary>
        /// Прапорить повну перебудову volume зі стану gameplay fog service.
        /// </summary>
        /// <param name="fogService">Gameplay source of truth для fog state.</param>
        public void RebuildFullVisual(IFogOfWarService fogService)
        {
            if (fogService != null)
                _loggedMissingFogService = false;
            _pendingWorkRequests.RequestFullRebuild(fogService);
            Debug.Log($"{StartDiagTag} VolumeUpdater.RebuildFullVisual hasFogService={fogService != null}, map={_mapWidth}x{_mapHeight}, controller={(_controller != null ? _controller.name : "null")}, manager={(_manager != null ? _manager.name : "null")}, contextValid={_context.IsValid}, hasBuilt={_hasBuiltAtLeastOnce}, contextChanged={_worldContextChangedSinceBuild}.");
            if (ShouldLogLifecycle(_loggedRebuildRequest))
            {
                _loggedRebuildRequest = true;
                Debug.Log($"{LogTag} RebuildFullVisual: fogService={(fogService != null ? fogService.GetType().Name : "null")}, map={_mapWidth}x{_mapHeight}, contextValid={_context.IsValid}, hasController={_controller != null}, hasManager={_manager != null}, hasBuilt={_hasBuiltAtLeastOnce}, contextChanged={_worldContextChangedSinceBuild}, updateMode={_visualUpdateScheduleState.CurrentUpdateMode}.");
            }
            if (_visualUpdateRequestPolicy.ShouldExecuteFullRebuildRequestImmediately(_hasBuiltAtLeastOnce, _worldContextChangedSinceBuild))
                ExecutePendingVisualWork();
        }

        /// <summary>
        /// Виконує відкладену visual rebuild відповідно до обраного update mode.
        /// Викликається Zenject-ом щокадру як частина runtime lifecycle.
        /// </summary>
        public void Tick()
        {
            if (!_visualUpdateTickGate.ShouldExecute(_pendingWorkState.Snapshot, out string waitingMessage))
            {
                if (!string.IsNullOrEmpty(waitingMessage))
                    LogUpdaterOnce(ref _loggedTickWaitingForInterval, waitingMessage);
                return;
            }

            ExecutePendingVisualWork();
        }

        /// <summary>
        /// Звільняє runtime configuration clone і пов'язані ресурси updater-а.
        /// </summary>
        public void Dispose()
        {
            _clusteredVolumeRenderer?.Clear();
            _dirtyClusterTracker?.Clear();
            DisposeRuntimeConfiguration();
        }

        /// <summary>
        /// Діагностично перевіряє, чи кеш unexplored state містить задану клітинку.
        /// </summary>
        /// <param name="tile">Клітинка для перевірки.</param>
        /// <returns><see langword="true"/>, якщо клітинка входить до unexplored-кешу.</returns>
        internal bool DebugHasUnexploredCell(Vector2Int tile)
            => _stateCache.HasUnexploredCell(tile);

        /// <summary>
        /// Діагностично перевіряє, чи кеш explored state містить задану клітинку.
        /// </summary>
        /// <param name="tile">Клітинка для перевірки.</param>
        /// <returns><see langword="true"/>, якщо клітинка входить до explored-кешу.</returns>
        internal bool DebugHasExploredCell(Vector2Int tile)
            => _stateCache.HasExploredCell(tile);

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

            if (_pendingWorkState.FogService != null)
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

            Debug.Log($"{StartDiagTag} VolumeUpdater.RequestStartupBuildFromController controller={(controller != null ? controller.name : "null")}, manager={(_manager != null ? _manager.name : "null")}, context={context.Width}x{context.Height}, cell={context.CellSize:0.###}, visibleCenter={(visibleCenter.HasValue ? visibleCenter.Value.ToString() : "none")}, visibleRadius={Mathf.Max(0, visibleRadius)}, keepVisible={keepVisible}, hasLastFogService={_pendingWorkState.FogService != null}, hasBuilt={_hasBuiltAtLeastOnce}.");
            Debug.Log($"{LogTag} Startup build requested by controller='{(controller != null ? controller.name : "null")}', context={context.Width}x{context.Height}, cell={context.CellSize:0.###}, bounds={FormatBounds(context)}, heightMap={FormatMapSize(context.HeightMap)}, terrainLevelMap={FormatMapSize(context.TerrainLevelMap)}, visibleCenter={(visibleCenter.HasValue ? visibleCenter.Value.ToString() : "none")}, visibleRadius={Mathf.Max(0, visibleRadius)}, visibleShape={visibleShape}, keepVisible={keepVisible}.");
            Initialize(context.Width, context.Height, context);
            RebuildFullVisual(visibleCenter.HasValue
                ? _startupFogServiceFactory.Create(context.Width, context.Height, visibleCenter.Value, visibleRadius, visibleShape, keepVisible)
                : _startupFogServiceFactory.Create(context.Width, context.Height));
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
            _pendingWorkRequests.RequestFullRebuildWhenFogServiceAvailable();
            Debug.Log($"{StartDiagTag} VolumeUpdater.RequestFullRebuildFromController controller={(controller != null ? controller.name : "null")}, manager={(_manager != null ? _manager.name : "null")}, hasLastFogService={_pendingWorkState.FogService != null}, context={_context.Width}x{_context.Height}.");

            if (_pendingWorkState.HasPendingWork)
                ExecutePendingVisualWork();
        }

        void IFogVolumeRuntimeUpdater.RequestStartupBuildFromController(FogOfWarVolumeController controller, FogWorldVisualContext context)
            => RequestStartupBuildFromController(controller, context);

        void IFogVolumeRuntimeUpdater.RequestFullRebuildFromController(FogOfWarVolumeController controller)
            => RequestFullRebuildFromController(controller);

        private void ExecutePendingVisualWork()
        {
            var work = _pendingWorkState.Snapshot;
            IFogOfWarService fogService = work.FogService;
            if (fogService == null)
            {
                LogMissingFogServiceOnce();
                return;
            }

            if (ShouldUseClusteredRuntimeRenderer())
            {
                if (_clusteredVolumeRenderer != null && _dirtyClusterTracker != null)
                {
                    ExecuteClusteredPendingVisualWork();
                    return;
                }

                Debug.LogWarning($"{ClusterDiagTag} FullRebuildFallback reason=missing-clustered-services, changes={work.CellChangeCount}, dirty={work.DirtyTileCount}. Falling back to legacy TWC volume path.");
            }

            Debug.Log($"{StartDiagTag} VolumeUpdater.ExecutePendingVisualWork hasFogService={fogService != null}, pendingDirty={work.DirtyTileCount}, fullRebuildRequested={work.FullRebuildRequested}, controller={(_controller != null ? _controller.name : "null")}, manager={(_manager != null ? _manager.name : "null")}, context={_context.Width}x{_context.Height}.");
            CountAuthoritativeFogStates(
                fogService,
                out int authoritativeVisible,
                out int authoritativeExplored,
                out int authoritativeUnexplored,
                out int dirtyVisible,
                out int dirtyExplored,
                out int dirtyUnexplored,
                out int dirtyOutOfBounds);
            Debug.Log($"{StartupChainTag} FogVolume.ExecutePendingVisualWork ENTER rebuild={(work.FullRebuildRequested ? "full" : "dirty")}, map={_mapWidth}x{_mapHeight}, pendingDirty={work.DirtyTileCount}, authoritativeVisible={authoritativeVisible}, authoritativeExplored={authoritativeExplored}, authoritativeUnexplored={authoritativeUnexplored}, dirtyVisible={dirtyVisible}, dirtyExplored={dirtyExplored}, dirtyUnexplored={dirtyUnexplored}, dirtyOutOfBounds={dirtyOutOfBounds}, dirtySamples={FormatPendingDirtySamples(fogService)}.");
            bool wasFullRebuild = work.FullRebuildRequested;
            int requestedDirtyTiles = work.DirtyTileCount;
            if (work.FullRebuildRequested)
                RebuildStateCaches(fogService);
            else
                ApplyDirtyStateCacheChanges(fogService);

            int cachedVisible = Mathf.Max(0, _mapWidth * _mapHeight - _stateCache.UnexploredCellCount - _stateCache.ExploredCellCount);
            Debug.Log($"{StartupChainTag} FogVolume.StateCache AFTER_UPDATE rebuild={(wasFullRebuild ? "full" : "dirty")}, requestedDirty={requestedDirtyTiles}, cachedVisible={cachedVisible}, cachedExplored={_stateCache.ExploredCellCount}, cachedUnexplored={_stateCache.UnexploredCellCount}, exploredHeightLayers={_stateCache.CountNonEmptyHeightLayers(_stateCache.ExploredCellsByHeight)}, unexploredHeightLayers={_stateCache.CountNonEmptyHeightLayers(_stateCache.UnexploredCellsByHeight)}.");
            if (TryFindVisibleDirtyCacheMismatch(fogService, out Vector2Int mismatchTile, out string mismatchCache))
                Debug.LogWarning($"{StartupChainTag} FogVolume.StateCache MISMATCH visible dirty tile is still in fog cache tile={mismatchTile}, cache={mismatchCache}, state={fogService.GetFogState(mismatchTile)}.");

            _pendingWorkMaintenance.Complete();

            if (!EnsureRuntimeConfiguration())
                return;

            ApplyCellsToRuntimeLayers();
            ExecuteTileWorldCreatorBuild(
                wasFullRebuild,
                requestedDirtyTiles,
                wasFullRebuild ? "explicit-or-context-full-rebuild" : "legacy-dirty-update-fallback");
            Debug.Log($"{StartupChainTag} FogVolume.VisualDispersalResult visualBuildRequested=true, visualBuildExecuted={_hasBuiltAtLeastOnce}, rebuild={(wasFullRebuild ? "full" : "dirty")}, requestedDirty={requestedDirtyTiles}, visibleCells={cachedVisible}, exploredCells={_stateCache.ExploredCellCount}, unexploredCells={_stateCache.UnexploredCellCount}, runtimeLayers={_runtimeLayers.Count}, manager={(_manager != null ? _manager.name : "null")}.");
        }

        private void ExecuteClusteredPendingVisualWork()
        {
            var work = _pendingWorkState.Snapshot;
            bool requiresFullRebuild = work.FullRebuildRequested || !_hasBuiltAtLeastOnce || _worldContextChangedSinceBuild;
            int requestedDirtyTiles = work.DirtyTileCount;
            int requestedChanges = work.CellChangeCount;
            Debug.Log($"{ClusterDiagTag} ExecuteClusteredPendingVisualWork full={requiresFullRebuild}, changes={requestedChanges}, dirty={requestedDirtyTiles}, context={_context.Width}x{_context.Height}.");

            if (requiresFullRebuild)
            {
                _clusteredVolumeRenderer.RebuildFull(_context, work.FogService);
                CompleteClusteredVisualWork();
                return;
            }

            if (requestedChanges == 0)
            {
                if (requestedDirtyTiles > 0 && ResolveAllowFullRebuildFallback())
                {
                    Debug.LogWarning($"{ClusterDiagTag} FullRebuildFallback reason=changes-unavailable, changes=0, dirty={requestedDirtyTiles}.");
                    _clusteredVolumeRenderer.RebuildFull(_context, work.FogService);
                }

                CompleteClusteredVisualWork();
                return;
            }

            _dirtyClusterTracker.MarkChanges(work.PendingCellChanges, _context);
            var dirtyClusters = _dirtyClusterTracker.ConsumeDirtyClusters();
            float ratio = ResolveDirtyClusterRatio(dirtyClusters.Count);
            if (ResolveAllowFullRebuildFallback() && ratio > ResolveFullRebuildDirtyClusterRatioThreshold())
            {
                Debug.LogWarning($"{ClusterDiagTag} FullRebuildFallback reason=dirty-cluster-threshold, changes={requestedChanges}, clusters={dirtyClusters.Count}, ratio={ratio:0.###}, threshold={ResolveFullRebuildDirtyClusterRatioThreshold():0.###}.");
                _clusteredVolumeRenderer.RebuildFull(_context, work.FogService);
                CompleteClusteredVisualWork();
                return;
            }

            _clusteredVolumeRenderer.RebuildClusters(dirtyClusters, _context, work.FogService);
            CompleteClusteredVisualWork();
        }

        private void CompleteClusteredVisualWork()
        {
            _pendingWorkMaintenance.Complete();
            _hasBuiltAtLeastOnce = true;
            _worldContextChangedSinceBuild = false;
        }

        private void RebuildStateCaches(IFogOfWarService fogService)
        {
            _heightByKey.Clear();
            _cachedEffectiveHeightLayerSnap = -1f;
            _stateCache.Rebuild(
                fogService,
                ResolveHeightKey,
                IsStateEnabled(GetSettings()?.Volume?.Unexplored, fallback: true),
                IsStateEnabled(GetSettings()?.Volume?.Explored, fallback: true));
            if (_stateCache.RuntimeLayerSignatureChanged)
                _runtimeConfigurationDirty = true;
        }

        private void ApplyDirtyStateCacheChanges(IFogOfWarService fogService)
        {
            _heightByKey.Clear();
            _cachedEffectiveHeightLayerSnap = -1f;
            _stateCache.ApplyDirty(
                fogService,
                _pendingWorkState.PendingDirtyTiles,
                ResolveHeightKey,
                IsStateEnabled(GetSettings()?.Volume?.Unexplored, fallback: true),
                IsStateEnabled(GetSettings()?.Volume?.Explored, fallback: true));
            if (_stateCache.RuntimeLayerSignatureChanged)
                _runtimeConfigurationDirty = true;
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
                _stateCache.UnexploredCellsByHeight,
                FogRuntimeState.Unexplored,
                GetSettings()?.Volume?.Unexplored,
                "Fog_Unexplored");
            CreateRuntimeLayers(
                _runtimeConfiguration,
                _stateCache.ExploredCellsByHeight,
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
            configuration.clusterCellSize = MinimumFogClusterCellSize;
            configuration.useGlobalRandomSeed = true;
            configuration.globalRandomSeed = 1;
            configuration.currentRandomSeed = 1u;
            configuration.useParallel = false;
            configuration.mergeTiles = true;
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
            IReadOnlyDictionary<int, HashSet<Vector2>> cellsByHeight,
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
            buildLayer.mergeTiles = true;
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
            Debug.Log($"{StartDiagTag} VolumeUpdater.ApplyCellsToRuntimeLayers runtimeLayers={_runtimeLayers.Count}, unexploredCells={_stateCache.UnexploredCellCount}, exploredCells={_stateCache.ExploredCellCount}, unexploredHeightLayers={_stateCache.UnexploredCellsByHeight.Count}, exploredHeightLayers={_stateCache.ExploredCellsByHeight.Count}.");
            int visibleCells = Mathf.Max(0, _mapWidth * _mapHeight - _stateCache.UnexploredCellCount - _stateCache.ExploredCellCount);
            Debug.Log($"{StartupChainTag} FogVolume.ApplyCellsToRuntimeLayers runtimeLayers={_runtimeLayers.Count}, visibleCells={visibleCells}, exploredCells={_stateCache.ExploredCellCount}, unexploredCells={_stateCache.UnexploredCellCount}, exploredHeightLayers={_stateCache.CountNonEmptyHeightLayers(_stateCache.ExploredCellsByHeight)}, unexploredHeightLayers={_stateCache.CountNonEmptyHeightLayers(_stateCache.UnexploredCellsByHeight)}, layerSummary={FormatRuntimeLayerSummary()}.");
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

            return runtimeLayer.State == FogRuntimeState.Unexplored
                ? _stateCache.ResolveUnexploredCells(runtimeLayer.HeightKey)
                : _stateCache.ResolveExploredCells(runtimeLayer.HeightKey);
        }

        private void ExecuteTileWorldCreatorBuild(bool wasFullRebuild, int requestedDirtyTiles, string reason)
        {
            if (_manager == null || _runtimeConfiguration == null)
                return;

            bool isInitialBuild = !_hasBuiltAtLeastOnce;
            bool contextChanged = _worldContextChangedSinceBuild;
            int clustersBeforeBuild = CountGeneratedClusters();
            int layerObjectsBeforeBuild = CountLayerObjects();
            int generatedChildrenBeforeClear = CountGeneratedOutputChildren();
            bool stoppedPendingBuildCoroutines = StopPendingTileWorldBuildCoroutines(generatedChildrenBeforeClear, layerObjectsBeforeBuild);
            int clearedGeneratedChildren = ClearGeneratedOutputBeforeBuild();
            int generatedChildrenAfterClear = CountGeneratedOutputChildren();
            Debug.Log($"{StartupChainTag} FogVolume.TWCBuild BEFORE manager={_manager.name}, rebuild={(wasFullRebuild ? "full" : "dirty")}, reason={reason}, requestedDirty={requestedDirtyTiles}, runtimeLayers={_runtimeLayers.Count}, clusters={clustersBeforeBuild}, layerObjects={layerObjectsBeforeBuild}, generatedChildrenBeforeClear={generatedChildrenBeforeClear}, stoppedPendingBuildCoroutines={stoppedPendingBuildCoroutines}, clearedGeneratedChildren={clearedGeneratedChildren}, generatedChildrenAfterClear={generatedChildrenAfterClear}, config={_runtimeConfiguration.name}.");
            _manager.configuration = _runtimeConfiguration;
            ApplyFogBatchingBudget();
            _manager.ExecuteBuildLayers(ExecutionMode.FromScratch);
            _hasBuiltAtLeastOnce = true;
            _worldContextChangedSinceBuild = false;
            int clustersAfterBuild = CountGeneratedClusters();
            int layerObjectsAfterBuild = CountLayerObjects();
            Debug.Log($"{StartDiagTag} VolumeUpdater.ExecuteTileWorldCreatorBuild manager={_manager.name}, runtimeConfig={_runtimeConfiguration.name}, map={_mapWidth}x{_mapHeight}, rebuild={(wasFullRebuild ? "full" : "dirty")}, reason={reason}, dirtyRequested={requestedDirtyTiles}, runtimeLayers={_runtimeLayers.Count}, unexploredCells={_stateCache.UnexploredCellCount}, exploredCells={_stateCache.ExploredCellCount}.");
            Debug.Log($"{StartupChainTag} FogVolume.TWCBuild AFTER manager={_manager.name}, rebuild={(wasFullRebuild ? "full" : "dirty")}, reason={reason}, requestedDirty={requestedDirtyTiles}, runtimeLayers={_runtimeLayers.Count}, clustersBefore={clustersBeforeBuild}, clustersAfterImmediate={clustersAfterBuild}, layerObjectsBefore={layerObjectsBeforeBuild}, layerObjectsAfterImmediate={layerObjectsAfterBuild}, stoppedPendingBuildCoroutines={stoppedPendingBuildCoroutines}, clearedGeneratedChildren={clearedGeneratedChildren}.");
            Debug.Log($"{StartupChainTag} FogVolume.VisualBuildApplied visualFogDispersed={Mathf.Max(0, _mapWidth * _mapHeight - _stateCache.UnexploredCellCount - _stateCache.ExploredCellCount) > 0}, visibleCells={Mathf.Max(0, _mapWidth * _mapHeight - _stateCache.UnexploredCellCount - _stateCache.ExploredCellCount)}, exploredCells={_stateCache.ExploredCellCount}, unexploredCells={_stateCache.UnexploredCellCount}, runtimeLayers={_runtimeLayers.Count}, clustersAfter={clustersAfterBuild}, layerObjectsAfter={layerObjectsAfterBuild}, stoppedPendingBuildCoroutines={stoppedPendingBuildCoroutines}, clearedGeneratedChildren={clearedGeneratedChildren}, reason={reason}.");

            if (ShouldLogBuildSummary(contextChanged))
            {
                _loggedFirstBuild = true;
                Debug.Log(BuildSummaryLog(isInitialBuild, contextChanged, wasFullRebuild, requestedDirtyTiles));
            }
        }

        private void ApplyFogBatchingBudget()
        {
            if (_runtimeConfiguration == null)
                return;

            _runtimeConfiguration.mergeTiles = true;

            int activeLayerCount = 0;
            int maxLayerWidth = Mathf.Max(1, _runtimeConfiguration.width);
            int maxLayerHeight = Mathf.Max(1, _runtimeConfiguration.height);
            int mergeOverrideCount = 0;

            for (int i = 0; i < _runtimeLayers.Count; i++)
            {
                var runtimeLayer = _runtimeLayers[i];
                if (runtimeLayer?.BuildLayer == null || !runtimeLayer.BuildLayer.isEnabled)
                    continue;

                var cells = ResolveCells(runtimeLayer);
                if (cells == null || cells.Count == 0)
                    continue;

                activeLayerCount++;
                if (!runtimeLayer.BuildLayer.mergeTiles)
                {
                    runtimeLayer.BuildLayer.mergeTiles = true;
                    mergeOverrideCount++;
                }

                if (runtimeLayer.BlueprintLayer != null)
                {
                    maxLayerWidth = Mathf.Max(maxLayerWidth, _runtimeConfiguration.GetBlueprintLayerWidth(runtimeLayer.BlueprintLayer));
                    maxLayerHeight = Mathf.Max(maxLayerHeight, _runtimeConfiguration.GetBlueprintLayerHeight(runtimeLayer.BlueprintLayer));
                }
            }

            int safeLayerCount = Mathf.Max(1, activeLayerCount);
            int perLayerClusterBudget = Mathf.Max(1, TargetFogClusterBudget / safeLayerCount);
            int requestedClusterCellSize = ResolveClusterCellSizeForBudget(
                maxLayerWidth,
                maxLayerHeight,
                perLayerClusterBudget);
            requestedClusterCellSize = Mathf.Max(MinimumFogClusterCellSize, requestedClusterCellSize);

            bool clusterChanged = _runtimeConfiguration.clusterCellSize < requestedClusterCellSize;
            if (clusterChanged)
                _runtimeConfiguration.clusterCellSize = requestedClusterCellSize;

            if (clusterChanged || mergeOverrideCount > 0)
            {
                int estimatedClusters = EstimateClusterCount(maxLayerWidth, maxLayerHeight, _runtimeConfiguration.clusterCellSize) * safeLayerCount;
                Debug.Log(
                    $"{LogTag} Fog batching applied: mergeTiles={_runtimeConfiguration.mergeTiles}, " +
                    $"clusterCellSize={_runtimeConfiguration.clusterCellSize}, activeLayers={activeLayerCount}, " +
                    $"estimatedClusters={estimatedClusters}, target={TargetFogClusterBudget}, " +
                    $"layerMergeOverridesEnabled={mergeOverrideCount}.");
            }
        }

        private static int ResolveClusterCellSizeForBudget(int width, int height, int clusterBudget)
        {
            int safeWidth = Mathf.Max(1, width);
            int safeHeight = Mathf.Max(1, height);
            int safeBudget = Mathf.Max(1, clusterBudget);
            int maxSide = Mathf.Max(safeWidth, safeHeight);

            for (int cellSize = 1; cellSize <= maxSide; cellSize++)
            {
                if (EstimateClusterCount(safeWidth, safeHeight, cellSize) <= safeBudget)
                    return cellSize;
            }

            return maxSide;
        }

        private static int EstimateClusterCount(int width, int height, int clusterCellSize)
        {
            int safeClusterCellSize = Mathf.Max(1, clusterCellSize);
            return Mathf.CeilToInt(Mathf.Max(1, width) / (float)safeClusterCellSize)
                * Mathf.CeilToInt(Mathf.Max(1, height) / (float)safeClusterCellSize);
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
            int visibleCells = Mathf.Max(0, totalCells - _stateCache.UnexploredCellCount - _stateCache.ExploredCellCount);

            var sb = new StringBuilder(2048);
            sb.Append(LogTag)
                .Append(" TWC fog volume summary")
                .Append(" | operation=").Append(isInitialBuild ? "initial-build" : "update")
                .Append(" | rebuild=").Append(wasFullRebuild ? "full" : "dirty")
                .Append(" | dirtyRequested=").Append(requestedDirtyTiles)
                .Append(" | contextChanged=").Append(contextChanged)
                .Append(" | updateMode=").Append(_visualUpdateScheduleState.CurrentUpdateMode)
                .Append(" | interval=").Append(_visualUpdateScheduleState.CurrentIntervalSeconds.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)).Append('s')
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
                .Append(", snap=").Append(ResolveEffectiveHeightLayerSnap().ToString("0.###", System.Globalization.CultureInfo.InvariantCulture))
                .Append(", configuredSnap=").Append(ResolveConfiguredHeightLayerSnap().ToString("0.###", System.Globalization.CultureInfo.InvariantCulture))
                .Append(", clearance=").Append(((volume != null ? volume.TopClearance : 0.08f) + (_controller != null ? _controller.AdditionalTopClearance : 0f)).ToString("0.###", System.Globalization.CultureInfo.InvariantCulture))
                .Append(", heightKeys=").Append(_heightByKey.Count)
                .Append(FormatHeightRange())
                .AppendLine();

            sb.Append("  Fog Cells: total=").Append(totalCells)
                .Append(", unexplored=").Append(_stateCache.UnexploredCellCount)
                .Append(", explored=").Append(_stateCache.ExploredCellCount)
                .Append(", visible=").Append(visibleCells)
                .Append(", unexploredHeightLayers=").Append(_stateCache.CountNonEmptyHeightLayers(_stateCache.UnexploredCellsByHeight))
                .Append(", exploredHeightLayers=").Append(_stateCache.CountNonEmptyHeightLayers(_stateCache.ExploredCellsByHeight))
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

        private void CountAuthoritativeFogStates(
            IFogOfWarService fogService,
            out int visible,
            out int explored,
            out int unexplored,
            out int dirtyVisible,
            out int dirtyExplored,
            out int dirtyUnexplored,
            out int dirtyOutOfBounds)
        {
            visible = 0;
            explored = 0;
            unexplored = 0;
            dirtyVisible = 0;
            dirtyExplored = 0;
            dirtyUnexplored = 0;
            dirtyOutOfBounds = 0;

            if (fogService == null)
                return;

            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    switch (fogService.GetFogState(new Vector2Int(x, y)))
                    {
                        case FogStateType.Visible:
                            visible++;
                            break;
                        case FogStateType.Explored:
                            explored++;
                            break;
                        default:
                            unexplored++;
                            break;
                    }
                }
            }

            foreach (var tile in _pendingWorkState.PendingDirtyTiles)
            {
                if (!IsInBounds(tile))
                {
                    dirtyOutOfBounds++;
                    continue;
                }

                switch (fogService.GetFogState(tile))
                {
                    case FogStateType.Visible:
                        dirtyVisible++;
                        break;
                    case FogStateType.Explored:
                        dirtyExplored++;
                        break;
                    default:
                        dirtyUnexplored++;
                        break;
                }
            }
        }

        private string FormatPendingDirtySamples(IFogOfWarService fogService, int maxSamples = 8)
        {
            if (_pendingWorkState.DirtyTileCount == 0)
                return "none";

            var sb = new StringBuilder();
            int count = 0;
            foreach (var tile in _pendingWorkState.PendingDirtyTiles)
            {
                if (count > 0)
                    sb.Append(", ");

                sb.Append(tile)
                    .Append('=')
                    .Append(fogService != null ? fogService.GetFogState(tile).ToString() : "no-fog-service");

                count++;
                if (count >= maxSamples)
                    break;
            }

            if (_pendingWorkState.DirtyTileCount > count)
                sb.Append(", ...");

            return sb.ToString();
        }

        private string FormatRuntimeLayerSummary()
        {
            if (_runtimeLayers.Count == 0)
                return "none";

            var sb = new StringBuilder();
            for (int i = 0; i < _runtimeLayers.Count; i++)
            {
                if (i > 0)
                    sb.Append(" | ");

                var layer = _runtimeLayers[i];
                sb.Append(layer.State)
                    .Append(":heightKey=").Append(layer.HeightKey)
                    .Append(",cells=").Append(ResolveCells(layer)?.Count ?? 0)
                    .Append(",enabled=").Append(layer.BuildLayer != null && layer.BuildLayer.isEnabled)
                    .Append(",presets=").Append(CountBuildLayerPresets(layer.BuildLayer));
            }

            return sb.ToString();
        }

        private bool TryFindVisibleDirtyCacheMismatch(IFogOfWarService fogService, out Vector2Int tile, out string cache)
        {
            tile = default;
            cache = null;

            if (fogService == null)
                return false;

            foreach (var dirtyTile in _pendingWorkState.PendingDirtyTiles)
            {
                if (!IsInBounds(dirtyTile) || fogService.GetFogState(dirtyTile) != FogStateType.Visible)
                    continue;

                if (_stateCache.HasUnexploredCell(dirtyTile))
                {
                    tile = dirtyTile;
                    cache = "unexplored";
                    return true;
                }

                if (_stateCache.HasExploredCell(dirtyTile))
                {
                    tile = dirtyTile;
                    cache = "explored";
                    return true;
                }
            }

            return false;
        }

        private int CountGeneratedClusters()
        {
            return _manager != null
                ? _manager.GetComponentsInChildren<ClusterIdentifier>(true).Length
                : 0;
        }

        private int CountLayerObjects()
        {
            return _manager != null
                ? _manager.GetComponentsInChildren<LayerIdentifier>(true).Length
                : 0;
        }

        private int CountGeneratedOutputChildren()
            => _manager != null ? _manager.transform.childCount : 0;

        private int ClearGeneratedOutputBeforeBuild()
        {
            if (_manager == null || _outputCleaner == null)
                return 0;

            int removed = _outputCleaner.ClearGeneratedChildren(_manager, forceImmediate: true);
            ClearRuntimeBuildLayerClusterCaches();
            return removed;
        }

        private bool StopPendingTileWorldBuildCoroutines(int generatedChildrenBeforeClear, int layerObjectsBeforeBuild)
        {
            if (_manager == null || !Application.isPlaying)
                return false;

            if (!_hasBuiltAtLeastOnce && generatedChildrenBeforeClear <= 0 && layerObjectsBeforeBuild <= 0)
                return false;

            _manager.StopAllCoroutines();
            return true;
        }

        private void ClearRuntimeBuildLayerClusterCaches()
        {
            for (int i = 0; i < _runtimeLayers.Count; i++)
                _runtimeLayers[i]?.BuildLayer?.availableClusters?.Clear();
        }

        private void RequestVisualRebuild()
        {
            _pendingWorkRequests.RequestFullRebuildWhenFogServiceAvailable();
            LogUpdaterOnce(ref _loggedRebuildRequest, $"RequestVisualRebuild: hasLastFogService={_pendingWorkState.FogService != null}, pending={_pendingWorkState.HasPendingWork}, controller={(_controller != null ? _controller.name : "null")}, manager={(_manager != null ? _manager.name : "null")}.");
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

        private bool ShouldUseClusteredRuntimeRenderer()
            => GetSettings()?.Volume.UseClusteredRuntimeFogRenderer ?? false;

        private bool ResolveAllowFullRebuildFallback()
            => GetSettings()?.Volume.AllowFullRebuildFallback ?? true;

        private float ResolveFullRebuildDirtyClusterRatioThreshold()
            => Mathf.Clamp(GetSettings()?.Volume.FullRebuildDirtyClusterRatioThreshold ?? 0.35f, 0.01f, 1f);

        private float ResolveDirtyClusterRatio(int dirtyClusterCount)
        {
            int clusterSize = Mathf.Max(1, GetSettings()?.Volume.ClusterSize ?? 16);
            int clusterCountX = Mathf.Max(1, Mathf.CeilToInt(_mapWidth / (float)clusterSize));
            int clusterCountY = Mathf.Max(1, Mathf.CeilToInt(_mapHeight / (float)clusterSize));
            int totalComparableClusters = Mathf.Max(1, clusterCountX * clusterCountY);
            return Mathf.Clamp01(dirtyClusterCount / (float)totalComparableClusters);
        }

        private int ResolveHeightKey(Vector2Int tile)
        {
            float height = ResolveGeneratedSurfaceHeight(tile);
            float snap = ResolveEffectiveHeightLayerSnap();
            int key = Mathf.RoundToInt(height / snap);
            if (!_heightByKey.ContainsKey(key))
                _heightByKey.Add(key, key * snap);

            return key;
        }

        private float ResolveEffectiveHeightLayerSnap()
        {
            if (_cachedEffectiveHeightLayerSnap > 0f)
                return _cachedEffectiveHeightLayerSnap;

            float configuredSnap = ResolveConfiguredHeightLayerSnap();
            if (!TryResolveGeneratedHeightRange(out float minHeight, out float maxHeight))
            {
                _cachedEffectiveHeightLayerSnap = configuredSnap;
                return _cachedEffectiveHeightLayerSnap;
            }

            float heightRange = Mathf.Max(0f, maxHeight - minHeight);
            float budgetedSnap = heightRange > 0f
                ? heightRange / Mathf.Max(1, TargetFogHeightLayerBudget - 1)
                : configuredSnap;

            _cachedEffectiveHeightLayerSnap = Mathf.Max(configuredSnap, budgetedSnap);
            return _cachedEffectiveHeightLayerSnap;
        }

        private float ResolveConfiguredHeightLayerSnap()
            => Mathf.Max(0.001f, GetSettings()?.Volume.HeightLayerSnap ?? 0.01f);

        private bool TryResolveGeneratedHeightRange(out float minHeight, out float maxHeight)
        {
            minHeight = 0f;
            maxHeight = 0f;
            bool hasHeight = false;

            for (int y = 0; y < _mapHeight; y++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    float height = ResolveGeneratedSurfaceHeight(new Vector2Int(x, y));
                    if (!IsFinite(height))
                        continue;

                    if (!hasHeight)
                    {
                        minHeight = height;
                        maxHeight = height;
                        hasHeight = true;
                        continue;
                    }

                    minHeight = Mathf.Min(minHeight, height);
                    maxHeight = Mathf.Max(maxHeight, height);
                }
            }

            return hasHeight;
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
            bool unexploredNeedsPreset = _stateCache.UnexploredCellCount > 0 && IsStateEnabled(volume?.Unexplored, true);
            bool exploredNeedsPreset = _stateCache.ExploredCellCount > 0 && IsStateEnabled(volume?.Explored, true);

            if (unexploredNeedsPreset && !HasUsablePreset(volume?.Unexplored) && !_loggedUnexploredPresetProblem)
            {
                _loggedUnexploredPresetProblem = true;
                Debug.LogError($"{LogTag} Unexplored fog has {_stateCache.UnexploredCellCount} cells, but no usable dual-grid TilePreset is configured. Assign at least one preset in FogOfWarSettings > TWC Volume > Unexplored Fog.");
            }

            if (exploredNeedsPreset && !HasUsablePreset(volume?.Explored) && !_loggedExploredPresetProblem)
            {
                _loggedExploredPresetProblem = true;
                Debug.LogError($"{LogTag} Explored fog has {_stateCache.ExploredCellCount} cells, but no usable dual-grid TilePreset is configured. Assign at least one preset in FogOfWarSettings > TWC Volume > Explored Fog.");
            }

            if (_runtimeLayers.Count == 0
                && (_stateCache.UnexploredCellCount > 0 || _stateCache.ExploredCellCount > 0)
                && !_loggedNoRuntimeLayers)
            {
                _loggedNoRuntimeLayers = true;
                Debug.LogWarning($"{LogTag} No runtime TWC layers were created although fog cells exist. states: unexplored={_stateCache.UnexploredCellCount}, explored={_stateCache.ExploredCellCount}, settings={(GetSettings() != null ? GetSettings().name : "null")}.");
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

    }
}
