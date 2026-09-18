using System;
using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Canonical FogOfWar visual path. The screen-space texture updater is the
    /// only production implementation.
    /// </summary>
    internal sealed class FogVisualUpdaterRouter
        : IFogVisualUpdater,
          ITickable,
          IDisposable
    {
        private readonly FogScreenSpaceTextureUpdater _screenSpaceUpdater;
        private bool _disposed;

        [Inject]
        public FogVisualUpdaterRouter(
            FogScreenSpaceTextureUpdater screenSpaceUpdater)
        {
            _screenSpaceUpdater = screenSpaceUpdater;
        }

        public void Initialize(
            int width,
            int height,
            FogWorldVisualContext context)
        {
            _screenSpaceUpdater.Initialize(width, height, context);
        }

        public void SetWorldContext(
            FogWorldVisualContext context)
        {
            _screenSpaceUpdater.SetWorldContext(context);
        }

        public void PreviewRevealArea(
            Vector2Int center,
            int radius,
            FogRevealShape shape,
            bool keepVisible)
        {
            _screenSpaceUpdater.PreviewRevealArea(center, radius, shape, keepVisible);
        }

        public void UpdateDirtyTiles(
            IFogOfWarService fogService,
            IEnumerable<Vector2Int> dirtyTiles)
        {
            _screenSpaceUpdater.UpdateDirtyTiles(fogService, dirtyTiles);
        }

        public void RequestCellsUpdate(
            IFogOfWarService fogService,
            IReadOnlyList<FogCellVisualChange> changes,
            FogWorldVisualContext context)
        {
            _screenSpaceUpdater.RequestCellsUpdate(fogService, changes, context);
        }

        public void RebuildFullVisual(
            IFogOfWarService fogService)
        {
            _screenSpaceUpdater.RebuildFullVisual(fogService);
        }

        public void Tick()
        {
            // The screen-space updater applies dirty/full work synchronously and
            // does not need its own tick.
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _screenSpaceUpdater?.Dispose();
        }
    }
}
