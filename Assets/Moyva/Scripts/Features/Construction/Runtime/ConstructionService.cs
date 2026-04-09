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

        private readonly struct PendingPlacement
        {
            public PendingPlacement(Vector2Int position, string buildingId)
            {
                Position = position;
                BuildingId = buildingId;
            }

            public Vector2Int Position { get; }
            public string BuildingId { get; }
        }

        private readonly IObjectsMapService _objectsMapService;
        private readonly SignalBus _signalBus;
        private readonly int _minSpacing;
        private readonly IFogOfWarService _fogOfWarService; // може бути null якщо туман не підключений

        private string _selectedBuildingId;
        private readonly List<PendingPlacement> _pendingPlacements = new();
        private readonly List<PendingPlacement> _redoPlacements = new();
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

            if (!_isActive)
            {
                ResetSession(clearRedoHistory: true);
                IsDemolishMode = false;
            }
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

            // Вибір будівлі завжди переводить UX у режим розміщення.
            IsDemolishMode = false;
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

            if (!CanPlaceAt(position, null, out var tileOccupied, out var spacingBlocked, out var fogBlocked))
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

            return AddPendingPlacement(position, _selectedBuildingId, clearRedoHistory: true);
        }

        public bool HasPendingPlacementAt(Vector2Int position)
        {
            return _pendingPositions.Contains(position);
        }

        public bool TryMovePendingPlacement(Vector2Int fromPosition, Vector2Int toPosition)
        {
            if (State != BuildingPlacementState.Placing)
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] TryMovePendingPlacement({fromPosition} -> {toPosition}) ignored: state={State}");
                return false;
            }

            int index = FindPendingPlacementIndex(fromPosition);
            if (index < 0)
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] TryMovePendingPlacement({fromPosition} -> {toPosition}) ignored: source preview not found.");
                return false;
            }

            if (fromPosition == toPosition)
                return true;

            var placement = _pendingPlacements[index];
            if (!CanPlaceAt(toPosition, fromPosition, out var tileOccupied, out var spacingBlocked, out var fogBlocked))
            {
                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = toPosition,
                    BuildingId = placement.BuildingId,
                    PreviewState = BuildingPreviewState.Blocked
                });

                if (VerboseLogs)
                    Debug.Log($"[Construction] TryMovePendingPlacement({fromPosition} -> {toPosition}) -> BLOCKED. occupied={tileOccupied}, spacingViolation={spacingBlocked}, fogBlocked={fogBlocked}, minSpacing={_minSpacing}");

                return false;
            }

            _pendingPositions.Remove(fromPosition);
            _pendingPositions.Add(toPosition);
            _pendingPlacements[index] = new PendingPlacement(toPosition, placement.BuildingId);
            _redoPlacements.Clear();

            _signalBus.Fire(new BuildingPreviewChangedSignal
            {
                Position = fromPosition,
                BuildingId = placement.BuildingId,
                PreviewState = BuildingPreviewState.None
            });

            _signalBus.Fire(new BuildingPreviewChangedSignal
            {
                Position = toPosition,
                BuildingId = placement.BuildingId,
                PreviewState = BuildingPreviewState.Valid
            });

            _selectedBuildingId = placement.BuildingId;

            if (VerboseLogs)
                Debug.Log($"[Construction] TryMovePendingPlacement({fromPosition} -> {toPosition}) -> VALID.");

            return true;
        }

        public void Confirm()
        {
            if (VerboseLogs)
                Debug.Log($"[Construction] Confirm requested. count={_pendingPlacements.Count}");

            foreach (var placement in _pendingPlacements)
            {
                var pos = placement.Position;
                var id = placement.BuildingId;
                _objectsMapService.Register(pos, id);
                _playerPlacedBuildings[pos] = id;
                _signalBus.Fire(new BuildingPlacedSignal { BuildingId = id, Position = pos });

                if (VerboseLogs)
                    Debug.Log($"[Construction] Confirm placed '{id}' at {pos}");
            }

            _pendingPlacements.Clear();
            _pendingPositions.Clear();
            _redoPlacements.Clear();
            State = BuildingPlacementState.Idle;
            _selectedBuildingId = null;

            if (VerboseLogs)
                Debug.Log("[Construction] Confirm completed. state=Idle");
        }

        public void Cancel()
        {
            ResetSession(clearRedoHistory: false);
        }

        public void UndoLast()
        {
            if (_pendingPlacements.Count == 0)
            {
                if (VerboseLogs)
                    Debug.Log("[Construction] UndoLast ignored: no pending placements.");
                return;
            }

            int lastIndex = _pendingPlacements.Count - 1;
            var placement = _pendingPlacements[lastIndex];
            _pendingPlacements.RemoveAt(lastIndex);
            _pendingPositions.Remove(placement.Position);
            _redoPlacements.Add(placement);

            _signalBus.Fire(new BuildingPreviewChangedSignal
            {
                Position = placement.Position,
                BuildingId = placement.BuildingId,
                PreviewState = BuildingPreviewState.None
            });

            if (VerboseLogs)
                Debug.Log($"[Construction] UndoLast -> removed '{placement.BuildingId}' at {placement.Position}. pendingCount={_pendingPlacements.Count}");
        }

        public void RedoLast()
        {
            if (_redoPlacements.Count == 0)
            {
                if (VerboseLogs)
                    Debug.Log("[Construction] RedoLast ignored: redo stack is empty.");
                return;
            }

            int lastIndex = _redoPlacements.Count - 1;
            var placement = _redoPlacements[lastIndex];

            if (!_isActive)
            {
                if (VerboseLogs)
                    Debug.Log("[Construction] RedoLast ignored: construction mode is not active.");
                return;
            }

            State = BuildingPlacementState.Placing;
            _selectedBuildingId = placement.BuildingId;

            bool result = AddPendingPlacement(placement.Position, placement.BuildingId, clearRedoHistory: false);
            if (result)
                _redoPlacements.RemoveAt(lastIndex);

            if (VerboseLogs)
                Debug.Log($"[Construction] RedoLast -> position={placement.Position}, result={result}, remainingRedo={_redoPlacements.Count}");
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
        private bool IsBlockedBySpacing(Vector2Int position, Vector2Int? ignoredPendingPosition)
        {
            if (_minSpacing <= 0) return false;

            for (int dx = -_minSpacing; dx <= _minSpacing; dx++)
                for (int dy = -_minSpacing; dy <= _minSpacing; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    var neighbor = new Vector2Int(position.x + dx, position.y + dy);

                    bool blockedByPending = _pendingPositions.Contains(neighbor) && neighbor != ignoredPendingPosition;
                    if (_objectsMapService.IsOccupied(neighbor) || blockedByPending)
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
            return _fogOfWarService.GetFogState(position) != FogStateType.Visible;
        }

        private bool CanPlaceAt(Vector2Int position, Vector2Int? ignoredPendingPosition, out bool tileOccupied, out bool spacingBlocked, out bool fogBlocked)
        {
            tileOccupied = _objectsMapService.IsOccupied(position)
                || (_pendingPositions.Contains(position) && position != ignoredPendingPosition);
            spacingBlocked = !tileOccupied && IsBlockedBySpacing(position, ignoredPendingPosition);
            fogBlocked = !tileOccupied && !spacingBlocked && IsBlockedByFog(position);
            return !tileOccupied && !spacingBlocked && !fogBlocked;
        }

        private bool AddPendingPlacement(Vector2Int position, string buildingId, bool clearRedoHistory)
        {
            _pendingPlacements.Add(new PendingPlacement(position, buildingId));
            _pendingPositions.Add(position);
            if (clearRedoHistory)
                _redoPlacements.Clear();

            _signalBus.Fire(new BuildingPreviewChangedSignal
            {
                Position = position,
                BuildingId = buildingId,
                PreviewState = BuildingPreviewState.Valid
            });

            if (VerboseLogs)
                Debug.Log($"[Construction] Pending placement added for '{buildingId}' at {position}. pendingCount={_pendingPlacements.Count}, redoCount={_redoPlacements.Count}");

            return true;
        }

        private int FindPendingPlacementIndex(Vector2Int position)
        {
            for (int i = 0; i < _pendingPlacements.Count; i++)
            {
                if (_pendingPlacements[i].Position == position)
                    return i;
            }

            return -1;
        }

        private void ResetSession(bool clearRedoHistory)
        {
            if (VerboseLogs)
                Debug.Log($"[Construction] ResetSession requested. pendingCount={_pendingPlacements.Count}, redoCount={_redoPlacements.Count}, clearRedoHistory={clearRedoHistory}");

            for (int i = _pendingPlacements.Count - 1; i >= 0; i--)
            {
                var placement = _pendingPlacements[i];
                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = placement.Position,
                    BuildingId = placement.BuildingId,
                    PreviewState = BuildingPreviewState.None
                });

                if (!clearRedoHistory)
                    _redoPlacements.Add(placement);
            }

            _pendingPlacements.Clear();
            _pendingPositions.Clear();
            if (clearRedoHistory)
                _redoPlacements.Clear();

            _signalBus.Fire(new BuildingCancelledSignal());
            State = BuildingPlacementState.Idle;
            _selectedBuildingId = null;

            if (VerboseLogs)
                Debug.Log($"[Construction] ResetSession completed. state=Idle, redoCount={_redoPlacements.Count}");
        }
    }
}
