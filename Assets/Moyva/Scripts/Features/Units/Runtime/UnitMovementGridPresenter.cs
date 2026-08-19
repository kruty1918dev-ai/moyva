using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitMovementGridPresenter : IInitializable, IDisposable
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
            _signalBus.Subscribe<LocalUnitSelectionChangedSignal>(OnLocalUnitSelectionChanged);
            _signalBus.Subscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.Subscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus.Subscribe<OnObjectsMapChangedSignal>(OnObjectsMapChanged);
            _signalBus.Subscribe<GridTileChangedSignal>(OnGridTileChanged);
            _signalBus.Subscribe<GameModeChangedSignal>(OnGameModeChanged);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<LocalUnitSelectionChangedSignal>(OnLocalUnitSelectionChanged);
            _signalBus.TryUnsubscribe<UnitMovedSignal>(OnUnitMoved);
            _signalBus.TryUnsubscribe<UnitDestroyedSignal>(OnUnitDestroyed);
            _signalBus.TryUnsubscribe<OnObjectsMapChangedSignal>(OnObjectsMapChanged);
            _signalBus.TryUnsubscribe<GridTileChangedSignal>(OnGridTileChanged);
            _signalBus.TryUnsubscribe<GameModeChangedSignal>(OnGameModeChanged);
            ClearOverlay();
        }

        private void OnLocalUnitSelectionChanged(LocalUnitSelectionChangedSignal signal)
        {
            if (!signal.IsSelected || string.IsNullOrWhiteSpace(signal.UnitId))
            {
                if (string.IsNullOrWhiteSpace(signal.UnitId)
                    || string.Equals(signal.UnitId, _selectedUnitId, StringComparison.Ordinal))
                {
                    _selectedUnitId = null;
                    ClearOverlay();
                }

                return;
            }

            _selectedUnitId = signal.UnitId.Trim();
            _selectedPosition = signal.Position;
            RefreshOverlay();
        }

        private void OnUnitMoved(UnitMovedSignal signal)
        {
            if (!string.Equals(signal.UnitId, _selectedUnitId, StringComparison.Ordinal))
                return;

            _selectedPosition = signal.NewPosition;
            RefreshOverlay();
        }

        private void OnUnitDestroyed(UnitDestroyedSignal signal)
        {
            if (!string.Equals(signal.UnitId, _selectedUnitId, StringComparison.Ordinal))
                return;

            _selectedUnitId = null;
            ClearOverlay();
        }

        private void OnObjectsMapChanged(OnObjectsMapChangedSignal _)
        {
            if (!string.IsNullOrWhiteSpace(_selectedUnitId))
                RefreshOverlay();
        }

        private void OnGridTileChanged(GridTileChangedSignal _)
        {
            if (!string.IsNullOrWhiteSpace(_selectedUnitId))
                RefreshOverlay();
        }

        private void OnGameModeChanged(GameModeChangedSignal signal)
        {
            _currentMode = signal.NewMode;
            if (_currentMode != GameModeType.Normal)
                ClearOverlay();
            else if (!string.IsNullOrWhiteSpace(_selectedUnitId))
                RefreshOverlay();
        }

        private void RefreshOverlay()
        {
            if (_overlay == null
                || _movementQuery == null
                || _currentMode != GameModeType.Normal
                || string.IsNullOrWhiteSpace(_selectedUnitId))
            {
                ClearOverlay();
                return;
            }

            IReadOnlyList<UnitMovementTileSnapshot> movementTiles =
                _movementQuery.GetMovementTiles(_selectedUnitId);
            _cells.Clear();
            for (int index = 0; index < movementTiles.Count; index++)
            {
                UnitMovementTileSnapshot tile = movementTiles[index];
                GridActionOverlayVisualState state =
                    tile.Position == _selectedPosition
                        ? GridActionOverlayVisualState.Selected
                        : tile.IsReachable
                            ? GridActionOverlayVisualState.Reachable
                            : GridActionOverlayVisualState.Blocked;

                _cells.Add(new GridActionOverlayCell(
                    tile.Position,
                    state,
                    tile.Reason));
            }

            _overlay.Show(OverlayOwner, _cells);
        }

        private void ClearOverlay()
        {
            _cells.Clear();
            _overlay?.Release(OverlayOwner);
        }
    }
}
