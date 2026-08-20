using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitMovementGridPresenter :
        IInitializable,
        IDisposable,
        ILateTickable
    {
        private static readonly GridActionOverlayOwner OverlayOwner =
            GridActionOverlayOwner.Movement;

        private readonly SignalBus _signalBus;
        private readonly IUnitMovementQuery _movementQuery;
        private readonly IGridActionOverlayService _overlay;
        private readonly List<GridActionOverlayCell> _cells = new();

        private string _selectedUnitId;
        private Vector2Int _selectedPosition;
        private GameModeType _currentMode = GameModeType.Normal;
        private bool _refreshPending;

        private int _pendingInvalidations;
        private int _selectionInvalidations;
        private int _moveInvalidations;
        private int _objectsMapInvalidations;
        private int _gridInvalidations;
        private int _modeInvalidations;
        private double _firstInvalidationMs;

        public UnitMovementGridPresenter(
            SignalBus signalBus,
            [InjectOptional] IUnitMovementQuery movementQuery = null,
            [InjectOptional] IGridActionOverlayService overlay = null)
        {
            _signalBus = signalBus;
            _movementQuery = movementQuery;
            _overlay = overlay;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<LocalUnitSelectionChangedSignal>(
                OnLocalUnitSelectionChanged);
            _signalBus.Subscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.Subscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus.Subscribe<OnObjectsMapChangedSignal>(
                OnObjectsMapChanged);
            _signalBus.Subscribe<GridTileChangedSignal>(OnGridTileChanged);
            _signalBus.Subscribe<GameModeChangedSignal>(OnGameModeChanged);

            UnitMovementDiagnostics.Log(
                UnitMovementDiagnostics.CurrentTraceId,
                "GRID_PRESENTER_INIT",
                $"movementQueryBound={_movementQuery != null}; " +
                $"overlayBound={_overlay != null}");
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<LocalUnitSelectionChangedSignal>(
                OnLocalUnitSelectionChanged);
            _signalBus.TryUnsubscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.TryUnsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus.TryUnsubscribe<OnObjectsMapChangedSignal>(
                OnObjectsMapChanged);
            _signalBus.TryUnsubscribe<GridTileChangedSignal>(
                OnGridTileChanged);
            _signalBus.TryUnsubscribe<GameModeChangedSignal>(
                OnGameModeChanged);

            _refreshPending = false;
            ClearOverlay("dispose");
        }

        public void LateTick()
        {
            if (!_refreshPending)
                return;

            _refreshPending = false;
            long trace = UnitMovementDiagnostics.TraceForUnit(_selectedUnitId);

            UnitMovementDiagnostics.Log(
                trace,
                "GRID_LATE_TICK",
                $"unit={UnitMovementDiagnostics.Safe(_selectedUnitId)}; " +
                $"coalescedInvalidations={_pendingInvalidations}; " +
                $"reasons=selection:{_selectionInvalidations}," +
                $"move:{_moveInvalidations}," +
                $"objectsMap:{_objectsMapInvalidations}," +
                $"grid:{_gridInvalidations}," +
                $"mode:{_modeInvalidations}; " +
                $"queuedForMs={UnitMovementDiagnostics.Ms(UnitMovementDiagnostics.NowMs() - _firstInvalidationMs)}");

            RefreshOverlay(trace);
            ResetInvalidationCounters();
        }

        private void OnLocalUnitSelectionChanged(
            LocalUnitSelectionChangedSignal signal)
        {
            long trace = UnitMovementDiagnostics.TraceForUnit(signal.UnitId);

            UnitMovementDiagnostics.Log(
                trace,
                "SELECTION_SIGNAL",
                $"unit={UnitMovementDiagnostics.Safe(signal.UnitId)}; " +
                $"selected={signal.IsSelected}; pos={signal.Position}; " +
                $"previous={UnitMovementDiagnostics.Safe(_selectedUnitId)}");

            if (!signal.IsSelected
                || string.IsNullOrWhiteSpace(signal.UnitId))
            {
                if (string.IsNullOrWhiteSpace(signal.UnitId)
                    || string.Equals(
                        signal.UnitId,
                        _selectedUnitId,
                        StringComparison.Ordinal))
                {
                    _selectedUnitId = null;
                    _refreshPending = false;
                    ResetInvalidationCounters();
                    ClearOverlay("selection-cleared");
                }

                return;
            }

            _selectedUnitId = signal.UnitId.Trim();
            _selectedPosition = signal.Position;
            UnitMovementDiagnostics.AssociateUnit(_selectedUnitId, trace);
            RequestRefresh("selection");
        }

        private void OnUnitMoved(UnitMovedSignal signal)
        {
            if (!string.Equals(
                    signal.UnitId,
                    _selectedUnitId,
                    StringComparison.Ordinal))
            {
                return;
            }

            _selectedPosition = signal.NewPosition;
            RequestRefresh("move");
        }

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
        {
            if (!string.Equals(
                    signal.UnitId,
                    _selectedUnitId,
                    StringComparison.Ordinal))
            {
                return;
            }

            long trace = UnitMovementDiagnostics.TraceForUnit(signal.UnitId);
            UnitMovementDiagnostics.Log(
                trace,
                "UNIT_DESTROYED_GRID_CLEAR",
                $"unit={signal.UnitId}");

            _selectedUnitId = null;
            _refreshPending = false;
            ResetInvalidationCounters();
            ClearOverlay("unit-destroyed");
        }

        private void OnObjectsMapChanged(OnObjectsMapChangedSignal _)
        {
            if (!string.IsNullOrWhiteSpace(_selectedUnitId))
                RequestRefresh("objects-map");
        }

        private void OnGridTileChanged(GridTileChangedSignal _)
        {
            if (!string.IsNullOrWhiteSpace(_selectedUnitId))
                RequestRefresh("grid-tile");
        }

        private void OnGameModeChanged(GameModeChangedSignal signal)
        {
            _currentMode = signal.NewMode;

            if (_currentMode != GameModeType.Normal)
            {
                _refreshPending = false;
                ResetInvalidationCounters();
                ClearOverlay("mode-not-normal");
            }
            else if (!string.IsNullOrWhiteSpace(_selectedUnitId))
            {
                RequestRefresh("mode");
            }
        }

        private void RequestRefresh(string reason)
        {
            if (!_refreshPending)
                _firstInvalidationMs = UnitMovementDiagnostics.NowMs();

            _refreshPending = true;
            _pendingInvalidations++;

            switch (reason)
            {
                case "selection":
                    _selectionInvalidations++;
                    break;
                case "move":
                    _moveInvalidations++;
                    break;
                case "objects-map":
                    _objectsMapInvalidations++;
                    break;
                case "grid-tile":
                    _gridInvalidations++;
                    break;
                case "mode":
                    _modeInvalidations++;
                    break;
            }
        }

        private void RefreshOverlay(long trace)
        {
            double totalStart = UnitMovementDiagnostics.NowMs();

            if (_overlay == null
                || _movementQuery == null
                || _currentMode != GameModeType.Normal
                || string.IsNullOrWhiteSpace(_selectedUnitId))
            {
                UnitMovementDiagnostics.Warn(
                    trace,
                    "GRID_REFRESH_SKIPPED",
                    $"overlayBound={_overlay != null}; " +
                    $"queryBound={_movementQuery != null}; " +
                    $"mode={_currentMode}; " +
                    $"unit={UnitMovementDiagnostics.Safe(_selectedUnitId)}");
                ClearOverlay("refresh-precondition");
                return;
            }

            UnitMovementDiagnostics.Log(
                trace,
                "GRID_REFRESH_BEGIN",
                $"unit={_selectedUnitId}; selectedPos={_selectedPosition}");

            double queryStart = UnitMovementDiagnostics.NowMs();
            IReadOnlyList<UnitMovementTileSnapshot> movementTiles =
                _movementQuery.GetMovementTiles(_selectedUnitId);
            double queryMs =
                UnitMovementDiagnostics.NowMs() - queryStart;

            int reachable = 0;
            int blocked = 0;
            double convertStart = UnitMovementDiagnostics.NowMs();

            _cells.Clear();
            if (_cells.Capacity < movementTiles.Count)
                _cells.Capacity = movementTiles.Count;

            for (int index = 0; index < movementTiles.Count; index++)
            {
                UnitMovementTileSnapshot tile = movementTiles[index];

                if (tile.IsReachable)
                    reachable++;
                else
                    blocked++;

                GridActionOverlayVisualState state =
                    tile.Position == _selectedPosition
                        ? GridActionOverlayVisualState.Selected
                        : tile.IsReachable
                            ? GridActionOverlayVisualState.Reachable
                            : GridActionOverlayVisualState.Blocked;

                _cells.Add(
                    new GridActionOverlayCell(
                        tile.Position,
                        state,
                        tile.Reason));
            }

            double convertMs =
                UnitMovementDiagnostics.NowMs() - convertStart;

            double overlayStart = UnitMovementDiagnostics.NowMs();
            _overlay.Show(OverlayOwner, _cells);
            double overlayMs =
                UnitMovementDiagnostics.NowMs() - overlayStart;

            double totalMs =
                UnitMovementDiagnostics.NowMs() - totalStart;

            string summary =
                $"unit={_selectedUnitId}; tiles={movementTiles.Count}; " +
                $"reachable={reachable}; blocked={blocked}; " +
                $"queryMs={UnitMovementDiagnostics.Ms(queryMs)}; " +
                $"convertMs={UnitMovementDiagnostics.Ms(convertMs)}; " +
                $"overlayShowMs={UnitMovementDiagnostics.Ms(overlayMs)}; " +
                $"totalMs={UnitMovementDiagnostics.Ms(totalMs)}";

            if (totalMs >= 100.0)
                UnitMovementDiagnostics.Warn(trace, "GRID_REFRESH_SLOW", summary);
            else
                UnitMovementDiagnostics.Log(trace, "GRID_REFRESH_DONE", summary);
        }

        private void ClearOverlay(string reason)
        {
            long trace = UnitMovementDiagnostics.TraceForUnit(_selectedUnitId);
            double start = UnitMovementDiagnostics.NowMs();
            int cellsBefore = _cells.Count;

            _cells.Clear();
            _overlay?.Release(OverlayOwner);

            double elapsed = UnitMovementDiagnostics.NowMs() - start;
            UnitMovementDiagnostics.Log(
                trace,
                "GRID_CLEAR",
                $"reason={reason}; previousCells={cellsBefore}; " +
                $"releaseMs={UnitMovementDiagnostics.Ms(elapsed)}");
        }

        private void ResetInvalidationCounters()
        {
            _pendingInvalidations = 0;
            _selectionInvalidations = 0;
            _moveInvalidations = 0;
            _objectsMapInvalidations = 0;
            _gridInvalidations = 0;
            _modeInvalidations = 0;
            _firstInvalidationMs = 0.0;
        }
    }
}
