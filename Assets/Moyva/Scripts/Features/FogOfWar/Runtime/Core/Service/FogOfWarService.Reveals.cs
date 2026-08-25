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

            if (!_initialized)
            {
                _pendingRevealAreas.Add(new FogPendingRevealArea(center, radius, shape, keepVisible, visibleAreaId));
                return;
            }

            if (!RevealTouchesCurrentMap(center, radius))
            {
                _pendingRevealAreas.Add(new FogPendingRevealArea(center, radius, shape, keepVisible, visibleAreaId));
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
                var flushResult = FlushVisual();
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
                exploredFlushResult = FlushVisual();
            }
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
                return;
            }

            var reveals = _pendingRevealAreas.ToArray();
            _pendingRevealAreas.Clear();

            for (int index = 0; index < reveals.Length; index++)
            {
                var reveal = reveals[index];
                ApplyRevealArea(reveal.Center, reveal.Radius, reveal.Shape, reveal.KeepVisible, reveal.VisibleAreaId, $"pending:{reason}");
            }
        }

        private void ApplyStartupFallbackRevealIfNeeded(bool hasLoadedSnapshot)
        {
            bool settingsAllowsFallback = _settings != null && _settings.EnableStartupFallbackReveal;
            bool blockedByLoad = hasLoadedSnapshot || GameLaunchContext.IsAutoLoadEnabled();
            bool blockedByExistingState = _pendingRevealAreas.Count > 0
                || _unitPositions.Count > 0
                || _fixedVisionShapes.Count > 0
                || CountExploredTiles() > 0;

            if (!settingsAllowsFallback)
            {
                return;
            }

            if (blockedByLoad)
            {
                return;
            }

            if (blockedByExistingState)
            {
                return;
            }

            int radius = Mathf.Max(1, _settings.StartupFallbackRevealRadius);
            var center = PickStartupFallbackCenter();
            var shape = _settings.StartupFallbackRevealShape;
            ApplyRevealArea(center, radius, shape, true, StartupFallbackRevealAreaId, "startup-fallback");
        }

        private Vector2Int PickStartupFallbackCenter()
        {
            var selection = FogStartupFallbackRevealSelector.SelectCenter(_width, _height, _settings);
            return selection.Center;
        }

        private void LogStartupRevealFinalState(string reason)
        {
            CountFogStateTiles(out int visible, out int explored, out int unexplored);
            if (!_lastStartupRevealTrace.HasValue)
            {
                return;
            }
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
