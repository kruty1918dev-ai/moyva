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

    }
}
