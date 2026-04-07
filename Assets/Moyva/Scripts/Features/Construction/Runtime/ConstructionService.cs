using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionService : IConstructionService, IInitializable, IDisposable
    {
        private readonly IObjectsMapService _objectsMapService;
        private readonly SignalBus _signalBus;

        private string _selectedBuildingId;
        private readonly Stack<(Vector2Int position, string buildingId)> _pendingPlacements = new();
        private readonly Stack<(Vector2Int position, string buildingId)> _redoStack = new();
        // Позиції будівель, підтверджених гравцем під час гри (знесення дозволено лише для них)
        private readonly Dictionary<Vector2Int, string> _playerPlacedBuildings = new();
        private bool _isActive;

        public BuildingPlacementState State { get; private set; } = BuildingPlacementState.Idle;
        public bool IsDemolishMode { get; private set; }

        [Inject]
        public ConstructionService(IObjectsMapService objectsMapService, SignalBus signalBus)
        {
            _objectsMapService = objectsMapService;
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<GameModeChangedSignal>(OnGameModeChanged);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<GameModeChangedSignal>(OnGameModeChanged);
        }

        private void OnGameModeChanged(GameModeChangedSignal signal)
        {
            _isActive = signal.NewMode == GameModeType.Construction;
            if (!_isActive && State != BuildingPlacementState.Idle)
                Cancel();
            if (!_isActive)
                IsDemolishMode = false;
        }

        public void SelectBuilding(string buildingId)
        {
            if (!_isActive)
            {
                Debug.LogWarning("[Construction] SelectBuilding called outside Construction mode.");
                return;
            }

            _selectedBuildingId = buildingId;
            State = BuildingPlacementState.Placing;
        }

        public bool TryPreviewAt(Vector2Int position)
        {
            if (State != BuildingPlacementState.Placing)
                return false;

            if (_objectsMapService.IsOccupied(position))
            {
                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = position,
                    PreviewState = BuildingPreviewState.Blocked
                });
                return false;
            }

            _pendingPlacements.Push((position, _selectedBuildingId));
            _redoStack.Clear();

            _signalBus.Fire(new BuildingPreviewChangedSignal
            {
                Position = position,
                PreviewState = BuildingPreviewState.Valid
            });
            return true;
        }

        public void Confirm()
        {
            var confirmed = new List<(Vector2Int, string)>(_pendingPlacements);
            confirmed.Reverse();

            foreach (var (pos, id) in confirmed)
            {
                _objectsMapService.Register(pos, id);
                _playerPlacedBuildings[pos] = id;
                _signalBus.Fire(new BuildingPlacedSignal { BuildingId = id, Position = pos });
            }

            _pendingPlacements.Clear();
            _redoStack.Clear();
            State = BuildingPlacementState.Idle;
            _selectedBuildingId = null;
        }

        public void Cancel()
        {
            foreach (var (pos, _) in _pendingPlacements)
            {
                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = pos,
                    PreviewState = BuildingPreviewState.None
                });
            }

            _pendingPlacements.Clear();
            _redoStack.Clear();
            _signalBus.Fire(new BuildingCancelledSignal());
            State = BuildingPlacementState.Idle;
            _selectedBuildingId = null;
        }

        public void UndoLast()
        {
            if (_pendingPlacements.Count == 0)
                return;

            var (pos, id) = _pendingPlacements.Pop();
            _redoStack.Push((pos, id));

            _signalBus.Fire(new BuildingPreviewChangedSignal
            {
                Position = pos,
                PreviewState = BuildingPreviewState.None
            });
        }

        public void RedoLast()
        {
            if (_redoStack.Count == 0)
                return;

            var (pos, _) = _redoStack.Pop();
            TryPreviewAt(pos);
        }

        public void ToggleDemolishMode()
        {
            if (!_isActive)
            {
                Debug.LogWarning("[Construction] ToggleDemolishMode called outside Construction mode.");
                return;
            }

            IsDemolishMode = !IsDemolishMode;
        }

        public bool TryDemolishAt(Vector2Int position)
        {
            if (!_isActive || !IsDemolishMode)
                return false;

            if (!_playerPlacedBuildings.TryGetValue(position, out var buildingId))
            {
                Debug.LogWarning($"[Construction] TryDemolishAt({position}): будівля не була розміщена гравцем.");
                return false;
            }

            _objectsMapService.Unregister(position);
            _playerPlacedBuildings.Remove(position);
            _signalBus.Fire(new BuildingDemolishedSignal { BuildingId = buildingId, Position = position });
            return true;
        }
    }
}
