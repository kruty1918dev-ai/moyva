using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed partial class FogVolumeVisualUpdateEngine
    {
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
            _loggedMissingSettings = false;
            _loggedUnexploredPresetProblem = false;
            _loggedExploredPresetProblem = false;
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
            _loggedUnexploredPresetProblem = false;
            _loggedExploredPresetProblem = false;
            _cachedEffectiveHeightLayerSnap = -1f;
            _pendingWorkMaintenance.ClearCellChanges();
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
            _cachedEffectiveHeightLayerSnap = -1f;
            _pendingWorkMaintenance.ClearCellChanges();
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
                return;
            }

            if (!_context.IsValid)
                _context = CreateFallbackContext(_mapWidth, _mapHeight);
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
            _pendingWorkRequests.RequestDirtyTiles(fogService, dirtyTiles, out _);
            if (_visualUpdateRequestPolicy.ShouldExecuteImmediateRequest())
                ExecutePendingVisualWork();
        }

        public void RequestCellsUpdate(
            IFogOfWarService fogService,
            IReadOnlyList<FogCellVisualChange> changes,
            FogWorldVisualContext context)
        {
            if (context.IsValid)
            {
                _context = context.WithSize(context.Width, context.Height);
                _mapWidth = context.Width;
                _mapHeight = context.Height;
                _pendingWorkMaintenance.SetMapSize(_mapWidth, _mapHeight);
            }

            _pendingWorkRequests.RequestCellsUpdate(fogService, changes);
            if (_visualUpdateRequestPolicy.ShouldExecuteImmediateRequest())
                ExecutePendingVisualWork();
        }

        /// <summary>
        /// Прапорить повну перебудову volume зі стану gameplay fog service.
        /// </summary>
        /// <param name="fogService">Gameplay source of truth для fog state.</param>
        public void RebuildFullVisual(IFogOfWarService fogService)
        {
            _pendingWorkRequests.RequestFullRebuild(fogService);
            if (_visualUpdateRequestPolicy.ShouldExecuteFullRebuildRequestImmediately(_hasBuiltAtLeastOnce, _worldContextChangedSinceBuild))
                ExecutePendingVisualWork();
        }

        /// <summary>
        /// Виконує відкладену visual rebuild відповідно до обраного update mode.
        /// Викликається Zenject-ом щокадру як частина runtime lifecycle.
        /// </summary>
        public void Tick()
        {
            if (!_visualUpdateTickGate.ShouldExecute(_pendingWorkState.Snapshot, out _))
                return;

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
                return;
            }

            if (_hasBuiltAtLeastOnce)
            {
                return;
            }

            if (!context.IsValid)
                context = CreateFallbackContext(_mapWidth, _mapHeight);
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

            if (_pendingWorkState.HasPendingWork)
                ExecutePendingVisualWork();
        }

        void IFogVolumeRuntimeUpdater.RequestStartupBuildFromController(FogOfWarVolumeController controller, FogWorldVisualContext context)
            => RequestStartupBuildFromController(controller, context);

        void IFogVolumeRuntimeUpdater.RequestFullRebuildFromController(FogOfWarVolumeController controller)
            => RequestFullRebuildFromController(controller);

    }
}
