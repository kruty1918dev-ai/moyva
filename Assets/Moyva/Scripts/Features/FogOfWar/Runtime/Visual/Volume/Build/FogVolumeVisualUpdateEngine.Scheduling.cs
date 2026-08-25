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
    internal sealed partial class FogVolumeVisualUpdateEngine
    {
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
            }
            CountAuthoritativeFogStates(
                fogService,
                out int authoritativeVisible,
                out int authoritativeExplored,
                out int authoritativeUnexplored,
                out int dirtyVisible,
                out int dirtyExplored,
                out int dirtyUnexplored,
                out int dirtyOutOfBounds);
            bool wasFullRebuild = work.FullRebuildRequested;
            int requestedDirtyTiles = work.DirtyTileCount;
            if (work.FullRebuildRequested)
                RebuildStateCaches(fogService);
            else
                ApplyDirtyStateCacheChanges(fogService);

            int cachedVisible = Mathf.Max(0, _mapWidth * _mapHeight - _stateCache.UnexploredCellCount - _stateCache.ExploredCellCount);

            _pendingWorkMaintenance.Complete();

            if (!EnsureRuntimeConfiguration())
                return;

            ApplyCellsToRuntimeLayers();
            ExecuteTileWorldCreatorBuild(
                wasFullRebuild,
                requestedDirtyTiles,
                wasFullRebuild ? "explicit-or-context-full-rebuild" : "legacy-dirty-update-fallback");
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
