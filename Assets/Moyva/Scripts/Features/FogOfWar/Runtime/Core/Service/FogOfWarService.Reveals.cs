using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed partial class FogOfWarService
    {
        private StartupRevealTrace _lastStartupRevealTrace;

        /// <summary>
        /// Виконує reveal для заданої області.
        /// Якщо fog service ще не готовий до поточного розміру карти, reveal може бути відкладений.
        /// </summary>
        public void RevealArea(Vector2Int center, int radius, FogRevealShape shape, bool keepVisible, string visibleAreaId = null)
        {
            radius = Mathf.Max(0, radius);
            bool inBounds = IsInBounds(center);
            bool touchesCurrentMap = _initialized && RevealTouchesCurrentMap(center, radius);
            Debug.Log($"{DirectDiagTag} FogService.RevealArea RECEIVED center={center}, radius={radius}, shape={shape}, keepVisible={keepVisible}, initialized={_initialized}, map={_width}x{_height}, inBounds={inBounds}.");
            Debug.Log($"{StartDiagTag} FogService.RevealArea request initialized={_initialized}, map={_width}x{_height}, center={center}, inBounds={inBounds}, radius={radius}, shape={shape}, keepVisible={keepVisible}, id={visibleAreaId ?? "<auto>"}, touchesCurrentMap={touchesCurrentMap}.");
            Debug.Log($"{StartupRevealDiagTag} RevealAreaDecision case={ResolveRevealRequestCase(inBounds, touchesCurrentMap)}, source=direct-request, startPoint={center}, center={center}, radius={radius}, shape={shape}, mode={FormatRevealMode(keepVisible)}, initialized={_initialized}, map={_width}x{_height}, inBounds={inBounds}, touchesCurrentMap={touchesCurrentMap}, pendingBefore={_pendingRevealAreas.Count}, visibleBefore={CountVisibleTiles()}, exploredBefore={CountExploredTiles()}, visualUpdater={(_visualUpdater != null ? _visualUpdater.GetType().Name : "null")}, visualContextValid={_visualContext.IsValid}.");
            if (!inBounds)
                Debug.LogWarning($"{StartDiagTag} FogService.RevealArea center outside map center={center}, map={_width}x{_height}, initialized={_initialized}.");
            if (radius <= 0)
                Debug.LogWarning($"{StartDiagTag} FogService.RevealArea radius<=0 center={center}, radius={radius}, shape={shape}, keepVisible={keepVisible}.");

            if (!_initialized)
            {
                _pendingRevealAreas.Add(new FogPendingRevealArea(center, radius, shape, keepVisible, visibleAreaId));
                Debug.LogWarning($"{StartDiagTag} FogService.RevealArea queued reason=not-initialized center={center}, radius={radius}, shape={shape}, keepVisible={keepVisible}, id={visibleAreaId ?? "<auto>"}, pendingRevealCount={_pendingRevealAreas.Count}.");
                Debug.Log($"{DebugTag} FogService.RevealArea queued-not-initialized center={center}, radius={radius}, shape={shape}, keepVisible={keepVisible}, id={visibleAreaId ?? "<auto>"}, pending={_pendingRevealAreas.Count}.");
                return;
            }

            if (!RevealTouchesCurrentMap(center, radius))
            {
                _pendingRevealAreas.Add(new FogPendingRevealArea(center, radius, shape, keepVisible, visibleAreaId));
                Debug.LogWarning($"{StartDiagTag} FogService.RevealArea queued reason=outside-current-map center={center}, radius={radius}, shape={shape}, keepVisible={keepVisible}, id={visibleAreaId ?? "<auto>"}, map={_width}x{_height}, pendingRevealCount={_pendingRevealAreas.Count}.");
                Debug.LogWarning($"{DebugTag} FogService.RevealArea queued-outside-current-map center={center}, radius={radius}, shape={shape}, keepVisible={keepVisible}, id={visibleAreaId ?? "<auto>"}, currentMap={_width}x{_height}, pending={_pendingRevealAreas.Count}. This usually means startup reveal arrived before fog resized to the generated world.");
                return;
            }

            ApplyRevealArea(center, radius, shape, keepVisible, visibleAreaId, "direct-request");
        }

        private void ApplyRevealArea(Vector2Int center, int radius, FogRevealShape shape, bool keepVisible, string visibleAreaId, string revealSource)
        {
            radius = Mathf.Max(0, radius);
            var centerStateBefore = GetFogState(center);
            int visibleBefore = CountVisibleTiles();
            int exploredBefore = CountExploredTiles();
            CountFogStateTiles(out int stateVisibleBefore, out int stateExploredBefore, out int stateUnexploredBefore);
            string revealMode = FormatRevealMode(keepVisible);
            string revealCase = ResolveRevealApplyCase(revealSource, keepVisible, visibleAreaId);
            Debug.Log($"{StartupRevealDiagTag} ApplyRevealAreaBegin source={revealSource}, case={revealCase}, mode={revealMode}, startPoint={center}, center={center}, radius={radius}, shape={shape}, keepVisible={keepVisible}, requestedId={visibleAreaId ?? "<auto>"}, initialized={_initialized}, map={_width}x{_height}, centerInBounds={IsInBounds(center)}, centerStateBefore={centerStateBefore}, visibleBefore={visibleBefore}, exploredBefore={exploredBefore}, stateVisible={stateVisibleBefore}, stateExplored={stateExploredBefore}, stateUnexplored={stateUnexploredBefore}, visualUpdater={(_visualUpdater != null ? _visualUpdater.GetType().Name : "null")}, visualContextValid={_visualContext.IsValid}.");

            string areaId = null;
            bool removedOldVisibility = false;
            if (keepVisible)
            {
                areaId = ResolveRevealVisibilityAreaId(center, radius, shape, visibleAreaId);
                removedOldVisibility = RemoveVisibleTiles(areaId);
                _unitVisionRange.Remove(areaId);
                _unitPositions.Remove(areaId);
                _fixedVisionShapes.Remove(areaId);
                _unitVisionModifiers.Remove(areaId);
            }

            var tiles = FogRevealShapeTileCalculator.ComputeShapeTiles(center, radius, shape, _width, _height);
            bool centerIncluded = false;
            for (int index = 0; index < tiles.Count; index++)
            {
                if (tiles[index] == center)
                {
                    centerIncluded = true;
                    break;
                }
            }
            if (tiles.Count == 0)
            {
                Debug.LogWarning($"{StartDiagTag} ApplyRevealArea computed tiles=0, center={center}, radius={radius}, shape={shape}, keepVisible={keepVisible}, id={areaId ?? visibleAreaId ?? "<explored-only>"}, map={_width}x{_height}.");
                Debug.LogWarning($"{DebugTag} FogService.ApplyRevealArea zero-tiles center={center}, radius={radius}, shape={shape}, keepVisible={keepVisible}, id={areaId ?? visibleAreaId ?? "<explored-only>"}, map={_width}x{_height}.");
                FogVisualFlushResult zeroTileFlushResult = removedOldVisibility
                    ? FlushVisual()
                    : default;
                LogRevealApplyResult(
                    revealSource,
                    revealCase,
                    revealMode,
                    center,
                    radius,
                    shape,
                    keepVisible,
                    areaId ?? visibleAreaId ?? "<none>",
                    tiles.Count,
                    centerIncluded,
                    removedOldVisibility,
                    centerStateBefore,
                    GetFogState(center),
                    visibleBefore,
                    CountVisibleTiles(),
                    exploredBefore,
                    CountExploredTiles(),
                    zeroTileFlushResult,
                    gameplayChanged: removedOldVisibility,
                    zeroTiles: true);
                return;
            }

            if (keepVisible)
            {
                _unitVisionRange[areaId] = radius;
                _unitPositions[areaId] = center;
                _unitVisionModifiers[areaId] = default;
                _fixedVisionShapes[areaId] = shape;
                _unitVisibleTiles[areaId] = tiles;

                foreach (var tile in tiles)
                    AddVisibleTile(tile);

                int dirtyBeforeFlush = _visualDirtyBuffer.DirtyCount;
                int changesBeforeFlush = _visualDirtyBuffer.ChangeCount;
                CountFogStateTiles(out int stateVisibleBeforeFlush, out int stateExploredBeforeFlush, out int stateUnexploredBeforeFlush);
                Debug.Log($"{StartupChainTag} Fog.ApplyRevealArea PRE_FLUSH center={center}, radius={radius}, shape={shape}, keepVisible=true, id={areaId}, tiles={tiles.Count}, centerIncluded={centerIncluded}, centerStateBefore={centerStateBefore}, centerStateNow={GetFogState(center)}, dirty={dirtyBeforeFlush}, stateVisible={stateVisibleBeforeFlush}, stateExplored={stateExploredBeforeFlush}, stateUnexplored={stateUnexploredBeforeFlush}, dirtySamples={FormatDirtyTileSamples()}.");
                var flushResult = FlushVisual();
                Debug.Log($"{DirectDiagTag} FogService.ApplyRevealArea RESULT tiles={tiles.Count}, centerStateBefore={centerStateBefore}, centerStateAfter={GetFogState(center)}, dirtyBeforeFlush={dirtyBeforeFlush}, dirtyAfterFlush={_visualDirtyBuffer.DirtyCount}.");
                Debug.Log($"{StartDiagTag} ApplyRevealArea computed tiles={tiles.Count}, centerIncluded={centerIncluded}, centerStateBefore={centerStateBefore}, centerStateAfter={GetFogState(center)}, visibleBefore={visibleBefore}, visibleAfter={CountVisibleTiles()}, exploredBefore={exploredBefore}, exploredAfter={CountExploredTiles()}, dirtyBeforeFlush={dirtyBeforeFlush}, dirtyAfterFlush={_visualDirtyBuffer.DirtyCount}, keepVisible={keepVisible}, id={areaId}.");
                Debug.Log($"{StartupChainTag} Fog.ApplyRevealArea POST_FLUSH center={center}, centerState={GetFogState(center)}, visible={CountVisibleTiles()}, explored={CountExploredTiles()}, dirtyAfterFlush={_visualDirtyBuffer.DirtyCount}.");
                Debug.Log($"{DebugTag} FogService.ApplyRevealArea visible center={center}, radius={radius}, shape={shape}, id={areaId}, tiles={tiles.Count}, map={_width}x{_height}, centerState={GetFogState(center)}, visible={CountVisibleTiles()}, explored={CountExploredTiles()}.");
                LogRevealApplyResult(
                    revealSource,
                    revealCase,
                    revealMode,
                    center,
                    radius,
                    shape,
                    keepVisible,
                    areaId,
                    tiles.Count,
                    centerIncluded,
                    removedOldVisibility,
                    centerStateBefore,
                    GetFogState(center),
                    visibleBefore,
                    CountVisibleTiles(),
                    exploredBefore,
                    CountExploredTiles(),
                    flushResult,
                    gameplayChanged: dirtyBeforeFlush > 0 || changesBeforeFlush > 0 || visibleBefore != CountVisibleTiles() || exploredBefore != CountExploredTiles(),
                    zeroTiles: false);
                return;
            }

            bool changed = false;
            foreach (var tile in tiles)
            {
                if (_stateGrid.IsExplored(tile))
                    continue;

                FogStateType oldState = GetFogState(tile);
                int oldHeightKey = ResolveVisualHeightKey(tile);
                _stateGrid.MarkExplored(tile);
                TrackVisualChange(tile, oldState, oldHeightKey);
                changed = true;
            }

            int exploredDirtyBeforeFlush = _visualDirtyBuffer.DirtyCount;
            int exploredChangesBeforeFlush = _visualDirtyBuffer.ChangeCount;
            FogVisualFlushResult exploredFlushResult = default;
            if (changed)
            {
                CountFogStateTiles(out int stateVisibleBeforeFlush, out int stateExploredBeforeFlush, out int stateUnexploredBeforeFlush);
                Debug.Log($"{StartupChainTag} Fog.ApplyRevealArea PRE_FLUSH center={center}, radius={radius}, shape={shape}, keepVisible=false, tiles={tiles.Count}, centerIncluded={centerIncluded}, centerStateBefore={centerStateBefore}, centerStateNow={GetFogState(center)}, dirty={exploredDirtyBeforeFlush}, stateVisible={stateVisibleBeforeFlush}, stateExplored={stateExploredBeforeFlush}, stateUnexplored={stateUnexploredBeforeFlush}, dirtySamples={FormatDirtyTileSamples()}.");
                exploredFlushResult = FlushVisual();
            }

            Debug.Log($"{DirectDiagTag} FogService.ApplyRevealArea RESULT tiles={tiles.Count}, centerStateBefore={centerStateBefore}, centerStateAfter={GetFogState(center)}, dirtyBeforeFlush={exploredDirtyBeforeFlush}, dirtyAfterFlush={_visualDirtyBuffer.DirtyCount}.");
            Debug.Log($"{StartDiagTag} ApplyRevealArea computed tiles={tiles.Count}, centerIncluded={centerIncluded}, centerStateBefore={centerStateBefore}, centerStateAfter={GetFogState(center)}, visibleBefore={visibleBefore}, visibleAfter={CountVisibleTiles()}, exploredBefore={exploredBefore}, exploredAfter={CountExploredTiles()}, dirtyBeforeFlush={exploredDirtyBeforeFlush}, dirtyAfterFlush={_visualDirtyBuffer.DirtyCount}, keepVisible={keepVisible}, id={areaId ?? visibleAreaId ?? "<explored-only>"}.");
            Debug.Log($"{StartupChainTag} Fog.ApplyRevealArea POST_FLUSH center={center}, centerState={GetFogState(center)}, visible={CountVisibleTiles()}, explored={CountExploredTiles()}, dirtyAfterFlush={_visualDirtyBuffer.DirtyCount}, changed={changed}.");
            Debug.Log($"{DebugTag} FogService.ApplyRevealArea explored center={center}, radius={radius}, shape={shape}, tiles={tiles.Count}, changed={changed}, map={_width}x{_height}, centerState={GetFogState(center)}, visible={CountVisibleTiles()}, explored={CountExploredTiles()}.");
            LogRevealApplyResult(
                revealSource,
                revealCase,
                revealMode,
                center,
                radius,
                shape,
                keepVisible,
                areaId ?? visibleAreaId ?? "<explored-only>",
                tiles.Count,
                centerIncluded,
                removedOldVisibility,
                centerStateBefore,
                GetFogState(center),
                visibleBefore,
                CountVisibleTiles(),
                exploredBefore,
                CountExploredTiles(),
                exploredFlushResult,
                gameplayChanged: changed || exploredDirtyBeforeFlush > 0 || exploredChangesBeforeFlush > 0,
                zeroTiles: false);
        }

        private static string ResolveRevealVisibilityAreaId(Vector2Int center, int radius, FogRevealShape shape, string visibleAreaId)
            => !string.IsNullOrWhiteSpace(visibleAreaId)
                ? visibleAreaId
                : $"fog-reveal:{center.x}:{center.y}:{radius}:{(int)shape}";

        private bool RevealTouchesCurrentMap(Vector2Int center, int radius)
        {
            radius = Mathf.Max(0, radius);
            return center.x + radius >= 0
                && center.y + radius >= 0
                && center.x - radius < _width
                && center.y - radius < _height;
        }

        private void ApplyPendingRevealAreas(string reason)
        {
            if (_pendingRevealAreas.Count == 0)
            {
                Debug.Log($"{StartupRevealDiagTag} ApplyPendingRevealAreas skipped reason={reason}, pending=0, initialized={_initialized}, map={_width}x{_height}.");
                return;
            }

            var reveals = _pendingRevealAreas.ToArray();
            _pendingRevealAreas.Clear();
            Debug.Log($"{DebugTag} FogService.ApplyPendingRevealAreas reason={reason}, count={reveals.Length}, map={_width}x{_height}.");
            Debug.Log($"{StartupRevealDiagTag} ApplyPendingRevealAreas begin reason={reason}, count={reveals.Length}, initialized={_initialized}, map={_width}x{_height}, visualUpdater={(_visualUpdater != null ? _visualUpdater.GetType().Name : "null")}, visibleBefore={CountVisibleTiles()}, exploredBefore={CountExploredTiles()}.");

            for (int index = 0; index < reveals.Length; index++)
            {
                var reveal = reveals[index];
                Debug.Log($"{StartupRevealDiagTag} ApplyPendingRevealAreas item={index + 1}/{reveals.Length}, reason={reason}, startPoint={reveal.Center}, radius={reveal.Radius}, shape={reveal.Shape}, mode={FormatRevealMode(reveal.KeepVisible)}, id={reveal.VisibleAreaId ?? "<auto>"}.");
                ApplyRevealArea(reveal.Center, reveal.Radius, reveal.Shape, reveal.KeepVisible, reveal.VisibleAreaId, $"pending:{reason}");
            }

            Debug.Log($"{StartupRevealDiagTag} ApplyPendingRevealAreas end reason={reason}, visibleAfter={CountVisibleTiles()}, exploredAfter={CountExploredTiles()}, pendingAfter={_pendingRevealAreas.Count}.");
        }

        private void ApplyStartupFallbackRevealIfNeeded(bool hasLoadedSnapshot)
        {
            bool settingsAllowsFallback = _settings != null && _settings.EnableStartupFallbackReveal;
            bool blockedByLoad = hasLoadedSnapshot || GameLaunchContext.IsAutoLoadEnabled();
            bool blockedByExistingState = _pendingRevealAreas.Count > 0
                || _unitPositions.Count > 0
                || _fixedVisionShapes.Count > 0
                || CountExploredTiles() > 0;

            Debug.Log($"{StartupRevealDiagTag} StartupFallbackDecision settingsAllows={settingsAllowsFallback}, hasSettings={_settings != null}, hasLoadedSnapshot={hasLoadedSnapshot}, autoLoad={GameLaunchContext.IsAutoLoadEnabled()}, mode={GameLaunchContext.Mode}, pendingReveals={_pendingRevealAreas.Count}, units={_unitPositions.Count}, fixedAreas={_fixedVisionShapes.Count}, explored={CountExploredTiles()}, willApply={settingsAllowsFallback && !blockedByLoad && !blockedByExistingState}.");

            if (!settingsAllowsFallback)
            {
                Debug.Log($"{DebugTag} FogService.StartupFallback skipped settingsDisabled={_settings == null || !_settings.EnableStartupFallbackReveal}.");
                return;
            }

            if (blockedByLoad)
            {
                Debug.Log($"{DebugTag} FogService.StartupFallback skipped loadContext hasSnapshot={hasLoadedSnapshot}, autoLoad={GameLaunchContext.IsAutoLoadEnabled()}, mode={GameLaunchContext.Mode}.");
                return;
            }

            if (blockedByExistingState)
            {
                Debug.Log($"{DebugTag} FogService.StartupFallback skipped existingState pending={_pendingRevealAreas.Count}, units={_unitPositions.Count}, fixedAreas={_fixedVisionShapes.Count}, explored={CountExploredTiles()}.");
                return;
            }

            int radius = Mathf.Max(1, _settings.StartupFallbackRevealRadius);
            var center = PickStartupFallbackCenter();
            var shape = _settings.StartupFallbackRevealShape;
            Debug.LogWarning($"{DebugTag} FogService.StartupFallback applying center={center}, radius={radius}, shape={shape}, map={_width}x{_height}, mode={GameLaunchContext.Mode}. Bootstrap reveal did not arrive before fog init.");
            Debug.LogWarning($"{StartupRevealDiagTag} StartupFallbackApply source=startup-fallback, case=no-bootstrap-reveal, startPoint={center}, radius={radius}, shape={shape}, mode={FormatRevealMode(keepVisible: true)}, id={StartupFallbackRevealAreaId}, map={_width}x{_height}.");
            ApplyRevealArea(center, radius, shape, true, StartupFallbackRevealAreaId, "startup-fallback");
        }

        private Vector2Int PickStartupFallbackCenter()
        {
            var selection = FogStartupFallbackRevealSelector.SelectCenter(_width, _height, _settings);
            Debug.Log($"{DebugTag} FogService.StartupFallback picked random center={selection.Center}, seed={selection.Seed}, xRange={selection.XMin}..{selection.XMax}, yRange={selection.YMin}..{selection.YMax}.");
            Debug.Log($"{StartupRevealDiagTag} StartupFallbackCenter selected={selection.Center}, seed={selection.Seed}, xRange={selection.XMin}..{selection.XMax}, yRange={selection.YMin}..{selection.YMax}, map={_width}x{_height}.");
            return selection.Center;
        }

        private void LogStartupRevealFinalState(string reason)
        {
            CountFogStateTiles(out int visible, out int explored, out int unexplored);
            if (!_lastStartupRevealTrace.HasValue)
            {
                Debug.Log($"{StartupRevealDiagTag} FinalState reason={reason}, revealApplied=false, initialized={_initialized}, map={_width}x{_height}, stateVisible={visible}, stateExplored={explored}, stateUnexplored={unexplored}, visualUpdater={(_visualUpdater != null ? _visualUpdater.GetType().Name : "null")}, visualContextValid={_visualContext.IsValid}, version={Version}.");
                return;
            }

            Debug.Log($"{StartupRevealDiagTag} FinalState reason={reason}, revealApplied=true, source={_lastStartupRevealTrace.Source}, case={_lastStartupRevealTrace.Case}, mode={_lastStartupRevealTrace.Mode}, startPoint={_lastStartupRevealTrace.Center}, radius={_lastStartupRevealTrace.Radius}, shape={_lastStartupRevealTrace.Shape}, id={_lastStartupRevealTrace.AreaId}, gameplayChanged={_lastStartupRevealTrace.GameplayChanged}, visualUpdateDispatched={_lastStartupRevealTrace.VisualUpdateDispatched}, visualFogDispersed={_lastStartupRevealTrace.VisualFogDispersed}, tiles={_lastStartupRevealTrace.TileCount}, centerIncluded={_lastStartupRevealTrace.CenterIncluded}, centerStateAfter={_lastStartupRevealTrace.CenterStateAfter}, stateVisible={visible}, stateExplored={explored}, stateUnexplored={unexplored}, visualUpdater={(_visualUpdater != null ? _visualUpdater.GetType().Name : "null")}, visualContextValid={_visualContext.IsValid}, version={Version}.");
        }

        private void LogRevealApplyResult(
            string revealSource,
            string revealCase,
            string revealMode,
            Vector2Int center,
            int radius,
            FogRevealShape shape,
            bool keepVisible,
            string areaId,
            int tileCount,
            bool centerIncluded,
            bool removedOldVisibility,
            FogStateType centerStateBefore,
            FogStateType centerStateAfter,
            int visibleBefore,
            int visibleAfter,
            int exploredBefore,
            int exploredAfter,
            FogVisualFlushResult flushResult,
            bool gameplayChanged,
            bool zeroTiles)
        {
            bool visualUpdateDispatched = flushResult.UpdaterCalled;
            bool visualFogDispersed = gameplayChanged && flushResult.VisualFogDispersalRequested;
            CountFogStateTiles(out int stateVisibleAfter, out int stateExploredAfter, out int stateUnexploredAfter);
            _lastStartupRevealTrace = new StartupRevealTrace(
                revealSource,
                revealCase,
                revealMode,
                center,
                radius,
                shape,
                keepVisible,
                areaId,
                tileCount,
                centerIncluded,
                gameplayChanged,
                visualUpdateDispatched,
                visualFogDispersed,
                centerStateAfter);

            Debug.Log($"{StartupRevealDiagTag} ApplyRevealAreaResult source={revealSource}, case={revealCase}, mode={revealMode}, startPoint={center}, center={center}, radius={radius}, shape={shape}, keepVisible={keepVisible}, id={areaId}, zeroTiles={zeroTiles}, tiles={tileCount}, centerIncluded={centerIncluded}, removedOldVisibility={removedOldVisibility}, centerStateBefore={centerStateBefore}, centerStateAfter={centerStateAfter}, visibleBefore={visibleBefore}, visibleAfter={visibleAfter}, exploredBefore={exploredBefore}, exploredAfter={exploredAfter}, stateVisibleAfter={stateVisibleAfter}, stateExploredAfter={stateExploredAfter}, stateUnexploredAfter={stateUnexploredAfter}, gameplayChanged={gameplayChanged}, dirtySent={flushResult.DirtyCount}, changesSent={flushResult.ChangeCount}, visualUpdater={flushResult.UpdaterType ?? "null"}, visualUpdateDispatched={visualUpdateDispatched}, visualFogDispersed={visualFogDispersed}, versionBeforeFlush={flushResult.VersionBefore}, versionAfterFlush={flushResult.VersionAfter}.");
        }

        private string ResolveRevealRequestCase(bool inBounds, bool touchesCurrentMap)
        {
            if (!_initialized)
                return "queued-not-initialized";

            if (!touchesCurrentMap)
                return "queued-outside-current-map";

            if (!inBounds)
                return "apply-overlapping-map-center-outside";

            return "apply-now";
        }

        private static string ResolveRevealApplyCase(string revealSource, bool keepVisible, string visibleAreaId)
        {
            if (string.Equals(revealSource, "startup-fallback", System.StringComparison.Ordinal))
                return "startup-fallback";

            if (revealSource != null && revealSource.StartsWith("pending:", System.StringComparison.Ordinal))
                return "pending-bootstrap-reveal";

            if (keepVisible)
                return string.IsNullOrWhiteSpace(visibleAreaId) ? "direct-visible-auto-id" : "direct-visible-explicit-id";

            return "direct-explored-only";
        }

        private static string FormatRevealMode(bool keepVisible)
            => keepVisible ? "visible-persistent" : "explored-only";

        private readonly struct StartupRevealTrace
        {
            public StartupRevealTrace(
                string source,
                string revealCase,
                string mode,
                Vector2Int center,
                int radius,
                FogRevealShape shape,
                bool keepVisible,
                string areaId,
                int tileCount,
                bool centerIncluded,
                bool gameplayChanged,
                bool visualUpdateDispatched,
                bool visualFogDispersed,
                FogStateType centerStateAfter)
            {
                HasValue = true;
                Source = source;
                Case = revealCase;
                Mode = mode;
                Center = center;
                Radius = radius;
                Shape = shape;
                KeepVisible = keepVisible;
                AreaId = areaId;
                TileCount = tileCount;
                CenterIncluded = centerIncluded;
                GameplayChanged = gameplayChanged;
                VisualUpdateDispatched = visualUpdateDispatched;
                VisualFogDispersed = visualFogDispersed;
                CenterStateAfter = centerStateAfter;
            }

            public bool HasValue { get; }
            public string Source { get; }
            public string Case { get; }
            public string Mode { get; }
            public Vector2Int Center { get; }
            public int Radius { get; }
            public FogRevealShape Shape { get; }
            public bool KeepVisible { get; }
            public string AreaId { get; }
            public int TileCount { get; }
            public bool CenterIncluded { get; }
            public bool GameplayChanged { get; }
            public bool VisualUpdateDispatched { get; }
            public bool VisualFogDispersed { get; }
            public FogStateType CenterStateAfter { get; }
        }
    }
}
