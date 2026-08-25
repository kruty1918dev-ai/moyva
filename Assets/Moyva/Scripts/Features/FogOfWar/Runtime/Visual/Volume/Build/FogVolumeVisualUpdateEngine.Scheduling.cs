using Kruty1918.Moyva.FogOfWar.API;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed partial class FogVolumeVisualUpdateEngine
    {
        private void ExecutePendingVisualWork()
        {
            var work = _pendingWorkState.Snapshot;
            IFogOfWarService fogService = work.FogService;
            if (fogService == null)
                return;

            if (ShouldUseClusteredRuntimeRenderer())
            {
                if (_clusteredVolumeRenderer != null && _dirtyClusterTracker != null)
                {
                    ExecuteClusteredPendingVisualWork();
                    return;
                }
            }
            if (work.FullRebuildRequested)
                RebuildStateCaches(fogService);
            else
                ApplyDirtyStateCacheChanges(fogService);

            _pendingWorkMaintenance.Complete();

            if (!EnsureRuntimeConfiguration())
                return;

            ApplyCellsToRuntimeLayers();
            ExecuteTileWorldCreatorBuild();
        }

        private void ExecuteClusteredPendingVisualWork()
        {
            var work = _pendingWorkState.Snapshot;
            bool requiresFullRebuild = work.FullRebuildRequested || !_hasBuiltAtLeastOnce || _worldContextChangedSinceBuild;
            int requestedDirtyTiles = work.DirtyTileCount;
            int requestedChanges = work.CellChangeCount;

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

    }
}
