using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionService : IConstructionService, IInitializable, IDisposable
    {
        private const bool VerboseLogs = true;

        private readonly IObjectsMapService _objectsMapService;
        private readonly SignalBus _signalBus;
        private readonly int _minSpacing;
        private readonly IFogOfWarService _fogOfWarService; // може бути null якщо туман не підключений

        private string _selectedBuildingId;
        private readonly Stack<(Vector2Int position, string buildingId)> _pendingPlacements = new();
        private readonly Stack<(Vector2Int position, string buildingId)> _redoStack = new();
        private readonly HashSet<Vector2Int> _pendingPositions = new();
        // Позиції будівель, підтверджених гравцем під час гри (знесення дозволено лише для них)
        private readonly Dictionary<Vector2Int, string> _playerPlacedBuildings = new();
        private bool _isActive;

        public BuildingPlacementState State { get; private set; } = BuildingPlacementState.Idle;
        public bool IsDemolishMode { get; private set; }

        [Inject]
        public ConstructionService(
            IObjectsMapService objectsMapService,
            SignalBus signalBus,
            [Inject(Id = "minSpacing")] int minSpacing,
            [InjectOptional] IFogOfWarService fogOfWarService)
        {
            _objectsMapService = objectsMapService;
            _signalBus = signalBus;
            _minSpacing = minSpacing;
            _fogOfWarService = fogOfWarService;
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
            if (VerboseLogs)
                Debug.Log($"[Construction] GameModeChanged -> active={_isActive}, state={State}, demolish={IsDemolishMode}");

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

            if (string.IsNullOrWhiteSpace(buildingId))
            {
                Debug.LogWarning("[Construction] SelectBuilding called with empty buildingId.");
                return;
            }

            _selectedBuildingId = buildingId;
            State = BuildingPlacementState.Placing;

            if (VerboseLogs)
                Debug.Log($"[Construction] SelectBuilding -> id='{_selectedBuildingId}', state={State}");
        }

        public bool TryPreviewAt(Vector2Int position)
        {
            if (State != BuildingPlacementState.Placing)
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] TryPreviewAt({position}) ignored: state={State}");
                return false;
            }

            bool tileOccupied = _objectsMapService.IsOccupied(position) || _pendingPositions.Contains(position);
            bool spacingBlocked = !tileOccupied && IsBlockedBySpacing(position);
            bool fogBlocked = !tileOccupied && !spacingBlocked && IsBlockedByFog(position);

            if (tileOccupied || spacingBlocked || fogBlocked)
            {
                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = position,
                    BuildingId = _selectedBuildingId,
                    PreviewState = BuildingPreviewState.Blocked
                });

                if (VerboseLogs)
                    Debug.Log($"[Construction] TryPreviewAt({position}) -> BLOCKED. occupied={tileOccupied}, spacingViolation={spacingBlocked}, fogBlocked={fogBlocked}, minSpacing={_minSpacing}");

                return false;
            }

            _pendingPlacements.Push((position, _selectedBuildingId));
            _pendingPositions.Add(position);
            _redoStack.Clear();

            _signalBus.Fire(new BuildingPreviewChangedSignal
            {
                Position = position,
                BuildingId = _selectedBuildingId,
                PreviewState = BuildingPreviewState.Valid
            });

            if (VerboseLogs)
                Debug.Log($"[Construction] TryPreviewAt({position}) -> VALID. pendingCount={_pendingPlacements.Count}");

            return true;
        }

        public void Confirm()
        {
            var confirmed = new List<(Vector2Int, string)>(_pendingPlacements);
            confirmed.Reverse();

            if (VerboseLogs)
                Debug.Log($"[Construction] Confirm requested. count={confirmed.Count}");

            foreach (var (pos, id) in confirmed)
            {
                _objectsMapService.Register(pos, id);
                _playerPlacedBuildings[pos] = id;
                _signalBus.Fire(new BuildingPlacedSignal { BuildingId = id, Position = pos });

                if (VerboseLogs)
                    Debug.Log($"[Construction] Confirm placed '{id}' at {pos}");
            }

            _pendingPlacements.Clear();
            _pendingPositions.Clear();
            _redoStack.Clear();
            State = BuildingPlacementState.Idle;
            _selectedBuildingId = null;

            if (VerboseLogs)
                Debug.Log("[Construction] Confirm completed. state=Idle");
        }

        public void Cancel()
        {
            if (VerboseLogs)
                Debug.Log($"[Construction] Cancel requested. pendingCount={_pendingPlacements.Count}");

            foreach (var (pos, _) in _pendingPlacements)
            {
                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = pos,
                    BuildingId = _selectedBuildingId,
                    PreviewState = BuildingPreviewState.None
                });
            }

            _pendingPlacements.Clear();
            _pendingPositions.Clear();
            _redoStack.Clear();
            _signalBus.Fire(new BuildingCancelledSignal());
            State = BuildingPlacementState.Idle;
            _selectedBuildingId = null;

            if (VerboseLogs)
                Debug.Log("[Construction] Cancel completed. state=Idle");
        }

        public void UndoLast()
        {
            if (_pendingPlacements.Count == 0)
            {
                if (VerboseLogs)
                    Debug.Log("[Construction] UndoLast ignored: no pending placements.");
                return;
            }

            var (pos, id) = _pendingPlacements.Pop();
            _pendingPositions.Remove(pos);
            _redoStack.Push((pos, id));

            _signalBus.Fire(new BuildingPreviewChangedSignal
            {
                Position = pos,
                BuildingId = id,
                PreviewState = BuildingPreviewState.None
            });

            if (VerboseLogs)
                Debug.Log($"[Construction] UndoLast -> removed '{id}' at {pos}. pendingCount={_pendingPlacements.Count}");
        }

        public void RedoLast()
        {
            if (_redoStack.Count == 0)
            {
                if (VerboseLogs)
                    Debug.Log("[Construction] RedoLast ignored: redo stack is empty.");
                return;
            }

            var (pos, _) = _redoStack.Pop();
            bool result = TryPreviewAt(pos);

            if (VerboseLogs)
                Debug.Log($"[Construction] RedoLast -> position={pos}, result={result}");
        }

        public void ToggleDemolishMode()
        {
            if (!_isActive)
            {
                Debug.LogWarning("[Construction] ToggleDemolishMode called outside Construction mode.");
                return;
            }

            IsDemolishMode = !IsDemolishMode;

            if (VerboseLogs)
                Debug.Log($"[Construction] ToggleDemolishMode -> {IsDemolishMode}");
        }

        public bool TryDemolishAt(Vector2Int position)
        {
            if (!_isActive || !IsDemolishMode)
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] TryDemolishAt({position}) ignored: active={_isActive}, demolish={IsDemolishMode}");
                return false;
            }

            if (!_playerPlacedBuildings.TryGetValue(position, out var buildingId))
            {
                Debug.LogWarning($"[Construction] TryDemolishAt({position}): будівля не була розміщена гравцем.");
                return false;
            }

            _objectsMapService.Unregister(position);
            _playerPlacedBuildings.Remove(position);
            _signalBus.Fire(new BuildingDemolishedSignal { BuildingId = buildingId, Position = position });

            if (VerboseLogs)
                Debug.Log($"[Construction] Demolished '{buildingId}' at {position}");

            return true;
        }

        public IReadOnlyDictionary<Vector2Int, string> GetPlayerPlacedBuildings()
            => new ReadOnlyDictionary<Vector2Int, string>(_playerPlacedBuildings);

        public void RestoreFromSave(Vector2Int position, string buildingId)
        {
            if (_objectsMapService.IsOccupied(position))
            {
                Debug.LogWarning($"[Construction] RestoreFromSave: позиція {position} вже зайнята, пропускаємо '{buildingId}'.");
                return;
            }

            _objectsMapService.Register(position, buildingId);
            _playerPlacedBuildings[position] = buildingId;
            _signalBus.Fire(new BuildingPlacedSignal { BuildingId = buildingId, Position = position });

            if (VerboseLogs)
                Debug.Log($"[Construction] RestoreFromSave: відновлено '{buildingId}' на {position}");
        }

        /// <summary>
        /// Перевіряє чи порушує позиція мінімальний відступ від існуючих / pending будівель.
        /// Chebyshev-дистанція (квадратна область навколо позиції).
        /// </summary>
        private bool IsBlockedBySpacing(Vector2Int position)
        {
            if (_minSpacing <= 0) return false;

            for (int dx = -_minSpacing; dx <= _minSpacing; dx++)
                for (int dy = -_minSpacing; dy <= _minSpacing; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    var neighbor = new Vector2Int(position.x + dx, position.y + dy);
                    if (_objectsMapService.IsOccupied(neighbor) || _pendingPositions.Contains(neighbor))
                        return true;
                }

            return false;
        }

        /// <summary>
        /// Повертає true якщо тайл повністю у тумані (Unexplored).
        /// Explored (раніше бачений) та Visible (зараз видно) дозволяють будівництво.
        /// Якщо FogOfWar не підключений — завжди false (дозволено).
        /// </summary>
        private bool IsBlockedByFog(Vector2Int position)
        {
            if (_fogOfWarService == null) return false;
            return _fogOfWarService.GetFogState(position) == FogStateType.Unexplored;
        }
    }
}
