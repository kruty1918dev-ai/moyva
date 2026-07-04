using System;
using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Thin DI adapter for fog volume visual updates.
    /// Real build/cache logic lives in <see cref="FogVolumeVisualUpdateEngine"/>.
    /// </summary>
    internal sealed class FogOfWarVolumeUpdater : IFogVisualUpdater, IFogVolumeRuntimeUpdater, ITickable, IDisposable
    {
        private readonly FogVolumeVisualUpdateEngine _engine;

        [Inject]
        public FogOfWarVolumeUpdater(FogVolumeVisualUpdateEngine engine)
        {
            _engine = engine;
        }

        public FogOfWarVolumeUpdater(
            [InjectOptional] FogOfWarSettings settings = null,
            [InjectOptional] IFogVolumeStateCache stateCache = null,
            [InjectOptional] IFogStartupFogServiceFactory startupFogServiceFactory = null,
            [InjectOptional] IFogDirtyClusterTracker dirtyClusterTracker = null,
            [InjectOptional] IFogClusteredVolumeRenderer clusteredVolumeRenderer = null)
            : this(new FogVolumeVisualUpdateEngine(
                settings,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                stateCache,
                startupFogServiceFactory,
                dirtyClusterTracker,
                clusteredVolumeRenderer))
        {
        }

        internal int DebugUnexploredCellCount => _engine.DebugUnexploredCellCount;
        internal int DebugExploredCellCount => _engine.DebugExploredCellCount;
        internal Configuration DebugRuntimeConfiguration => _engine.DebugRuntimeConfiguration;

        public void AttachController(FogOfWarVolumeController controller) => _engine.AttachController(controller);
        public void DetachController(FogOfWarVolumeController controller) => _engine.DetachController(controller);
        public void Initialize(int width, int height, FogWorldVisualContext context) => _engine.Initialize(width, height, context);
        public void SetWorldContext(FogWorldVisualContext context) => _engine.SetWorldContext(context);
        public void PreviewRevealArea(Vector2Int center, int radius, FogRevealShape shape, bool keepVisible) => _engine.PreviewRevealArea(center, radius, shape, keepVisible);
        public void UpdateDirtyTiles(IFogOfWarService fogService, IEnumerable<Vector2Int> dirtyTiles) => _engine.UpdateDirtyTiles(fogService, dirtyTiles);
        public void RequestCellsUpdate(IFogOfWarService fogService, IReadOnlyList<FogCellVisualChange> changes, FogWorldVisualContext context) => _engine.RequestCellsUpdate(fogService, changes, context);
        public void RebuildFullVisual(IFogOfWarService fogService) => _engine.RebuildFullVisual(fogService);
        public void Tick() => _engine.Tick();
        public void Dispose() => _engine.Dispose();

        internal bool DebugHasUnexploredCell(Vector2Int tile) => _engine.DebugHasUnexploredCell(tile);
        internal bool DebugHasExploredCell(Vector2Int tile) => _engine.DebugHasExploredCell(tile);
        public void RequestStartupBuildFromController(FogOfWarVolumeController controller, FogWorldVisualContext context) => _engine.RequestStartupBuildFromController(controller, context);
        public void RequestFullRebuildFromController(FogOfWarVolumeController controller) => _engine.RequestFullRebuildFromController(controller);
    }
}
