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
        private const string DefaultOwnerId = "player_0";

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

        private readonly struct PendingDemolition
        {
            public PendingDemolition(Vector2Int position, string buildingId)
            {
                Position = position;
                BuildingId = buildingId;
            }

            public Vector2Int Position { get; }
            public string BuildingId { get; }
        }

        private readonly IObjectsMapService _objectsMapService;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly SignalBus _signalBus;
        private readonly int _minSpacing;
        private readonly int _townHallBuildRadius;
        private readonly IFogOfWarService _fogOfWarService; // може бути null якщо туман не підключений
        private readonly IWallPlacementService _wallPlacementService;

        private string _selectedBuildingId;
        private readonly List<PendingPlacement> _pendingPlacements = new();
        private readonly List<List<PendingPlacement>> _undoSnapshots = new();
        private readonly List<List<PendingPlacement>> _redoSnapshots = new();
        private readonly HashSet<Vector2Int> _pendingPositions = new();
        private readonly List<PendingDemolition> _pendingDemolitions = new();
        private readonly HashSet<Vector2Int> _pendingDemolitionPositions = new();
        // Позиції будівель, підтверджених гравцем під час гри (знесення дозволено лише для них)
        private readonly Dictionary<Vector2Int, string> _playerPlacedBuildings = new();
        private string _activeOwnerId = DefaultOwnerId;
        private bool _isActive;

        public BuildingPlacementState State { get; private set; } = BuildingPlacementState.Idle;
        public bool IsDemolishMode { get; private set; }

        [Inject]
        public ConstructionService(
            IObjectsMapService objectsMapService,
            IBuildingRegistry buildingRegistry,
            SignalBus signalBus,
            [Inject(Id = "minSpacing")] int minSpacing,
            [Inject(Id = "townHallBuildRadius")] int townHallBuildRadius,
            [InjectOptional] IFogOfWarService fogOfWarService,
            [InjectOptional] IWallPlacementService wallPlacementService)
        {
            _objectsMapService = objectsMapService;
            _buildingRegistry = buildingRegistry;
            _signalBus = signalBus;
            _minSpacing = minSpacing;
            _townHallBuildRadius = Mathf.Max(0, townHallBuildRadius);
            _fogOfWarService = fogOfWarService;
            _wallPlacementService = wallPlacementService;
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
            ClearPendingDemolitionsPreview();
            IsDemolishMode = false;
            _selectedBuildingId = buildingId;
            State = BuildingPlacementState.Placing;

            if (VerboseLogs)
                Debug.Log($"[Construction] SelectBuilding -> id='{_selectedBuildingId}', state={State}");
        }

        public string GetSelectedBuildingId()
        {
            return _selectedBuildingId;
        }

        public void SetActiveOwner(string ownerId)
        {
            _activeOwnerId = string.IsNullOrWhiteSpace(ownerId) ? DefaultOwnerId : ownerId.Trim();
        }

        public string GetActiveOwner()
        {
            return _activeOwnerId;
        }

        public bool TryPreviewAt(Vector2Int position)
        {
            if (State != BuildingPlacementState.Placing)
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] TryPreviewAt({position}) ignored: state={State}");
                return false;
            }

            bool selectedIsGate = _wallPlacementService != null && _wallPlacementService.IsGate(_selectedBuildingId);

            if (_pendingPositions.Contains(position))
            {
                // Дозволяємо у preview заміняти pending-стіну на ворота на тому ж тайлі.
                if (selectedIsGate && TryReplacePendingWallWithGate(position, _selectedBuildingId))
                    return true;

                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = position,
                    BuildingId = _selectedBuildingId,
                    PreviewState = BuildingPreviewState.Blocked
                });
                return false;
            }

            bool gateReplacementAllowed = selectedIsGate
                && _wallPlacementService.CanReplaceWallWithGate(position, _selectedBuildingId, out _);

            if (selectedIsGate && !gateReplacementAllowed)
            {
                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = position,
                    BuildingId = _selectedBuildingId,
                    PreviewState = BuildingPreviewState.Blocked
                });
                return false;
            }

            if (!gateReplacementAllowed && !CanPlaceAt(position, null, _selectedBuildingId, out var tileOccupied, out var spacingBlocked, out var fogBlocked, out var townHallZoneBlocked))
            {
                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = position,
                    BuildingId = _selectedBuildingId,
                    PreviewState = BuildingPreviewState.Blocked
                });

                if (VerboseLogs)
                    Debug.Log($"[Construction] TryPreviewAt({position}) -> BLOCKED. occupied={tileOccupied}, spacingViolation={spacingBlocked}, fogBlocked={fogBlocked}, townHallZoneBlocked={townHallZoneBlocked}, minSpacing={_minSpacing}, townHallBuildRadius={_townHallBuildRadius}");

                return false;
            }

            return AddPendingPlacement(position, _selectedBuildingId, clearRedoHistory: true);
        }

        public bool HasPendingPlacementAt(Vector2Int position)
        {
            return _pendingPositions.Contains(position);
        }

        public bool TryGetPendingBuildingIdAt(Vector2Int position, out string buildingId)
        {
            int index = FindPendingPlacementIndex(position);
            if (index < 0)
            {
                buildingId = null;
                return false;
            }

            buildingId = _pendingPlacements[index].BuildingId;
            return !string.IsNullOrWhiteSpace(buildingId);
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
            bool movingGate = _wallPlacementService != null && _wallPlacementService.IsGate(placement.BuildingId);
            bool gateReplacementAllowed = movingGate
                && _wallPlacementService.CanReplaceWallWithGate(toPosition, placement.BuildingId, out _);

            if (movingGate && !gateReplacementAllowed)
            {
                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = toPosition,
                    BuildingId = placement.BuildingId,
                    PreviewState = BuildingPreviewState.Blocked
                });
                return false;
            }

            if (!gateReplacementAllowed && !CanPlaceAt(toPosition, fromPosition, placement.BuildingId, out var tileOccupied, out var spacingBlocked, out var fogBlocked, out var townHallZoneBlocked))
            {
                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = toPosition,
                    BuildingId = placement.BuildingId,
                    PreviewState = BuildingPreviewState.Blocked
                });

                if (VerboseLogs)
                    Debug.Log($"[Construction] TryMovePendingPlacement({fromPosition} -> {toPosition}) -> BLOCKED. occupied={tileOccupied}, spacingViolation={spacingBlocked}, fogBlocked={fogBlocked}, townHallZoneBlocked={townHallZoneBlocked}, minSpacing={_minSpacing}, townHallBuildRadius={_townHallBuildRadius}");

                return false;
            }

            SaveSnapshotForUndo(clearRedoHistory: true);

            _pendingPositions.Remove(fromPosition);
            _pendingPositions.Add(toPosition);
            _pendingPlacements[index] = new PendingPlacement(toPosition, placement.BuildingId);

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
            if (IsDemolishMode)
            {
                ConfirmPendingDemolitions();
                return;
            }

            if (VerboseLogs)
                Debug.Log($"[Construction] Confirm requested. count={_pendingPlacements.Count}");

            foreach (var placement in _pendingPlacements)
            {
                var pos = placement.Position;
                var id = placement.BuildingId;

                if (_objectsMapService.IsOccupied(pos))
                {
                    bool replaced = _wallPlacementService != null
                        && _wallPlacementService.IsGate(id)
                        && _wallPlacementService.CanReplaceWallWithGate(pos, id, out _);

                    if (!replaced)
                    {
                        Debug.LogWarning($"[Construction] Confirm skipped '{id}' at {pos}: tile occupied.");
                        continue;
                    }

                    _objectsMapService.Unregister(pos);
                }

                _objectsMapService.Register(pos, id);
                _playerPlacedBuildings[pos] = id;
                _signalBus.Fire(new BuildingPlacedSignal
                {
                    BuildingId = id,
                    Position = pos,
                    OwnerId = _activeOwnerId,
                });

                if (VerboseLogs)
                    Debug.Log($"[Construction] Confirm placed '{id}' at {pos}");
            }

            _pendingPlacements.Clear();
            _pendingPositions.Clear();
            _undoSnapshots.Clear();
            _redoSnapshots.Clear();
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
            if (_undoSnapshots.Count == 0)
            {
                if (VerboseLogs)
                    Debug.Log("[Construction] UndoLast ignored: undo history is empty.");
                return;
            }

            _redoSnapshots.Add(ClonePendingSnapshot());

            int snapshotIndex = _undoSnapshots.Count - 1;
            var snapshot = _undoSnapshots[snapshotIndex];
            _undoSnapshots.RemoveAt(snapshotIndex);

            ApplyPendingSnapshot(snapshot);

            if (_pendingPlacements.Count == 0 && State == BuildingPlacementState.Idle)
                State = BuildingPlacementState.Placing;

            if (VerboseLogs)
                Debug.Log($"[Construction] UndoLast completed. pendingCount={_pendingPlacements.Count}, undoCount={_undoSnapshots.Count}, redoCount={_redoSnapshots.Count}");
        }

        public void RedoLast()
        {
            if (_redoSnapshots.Count == 0)
            {
                if (VerboseLogs)
                    Debug.Log("[Construction] RedoLast ignored: redo stack is empty.");
                return;
            }

            if (!_isActive)
            {
                if (VerboseLogs)
                    Debug.Log("[Construction] RedoLast ignored: construction mode is not active.");
                return;
            }


            _undoSnapshots.Add(ClonePendingSnapshot());

            int snapshotIndex = _redoSnapshots.Count - 1;
            var snapshot = _redoSnapshots[snapshotIndex];
            _redoSnapshots.RemoveAt(snapshotIndex);

            ApplyPendingSnapshot(snapshot);

            if (State == BuildingPlacementState.Idle)
                State = BuildingPlacementState.Placing;

            if (_pendingPlacements.Count > 0)
                _selectedBuildingId = _pendingPlacements[_pendingPlacements.Count - 1].BuildingId;

            if (VerboseLogs)
                Debug.Log($"[Construction] RedoLast completed. pendingCount={_pendingPlacements.Count}, undoCount={_undoSnapshots.Count}, redoCount={_redoSnapshots.Count}");
        }

        public void ToggleDemolishMode()
        {
            if (!_isActive)
            {
                Debug.LogWarning("[Construction] ToggleDemolishMode called outside Construction mode.");
                return;
            }

            IsDemolishMode = !IsDemolishMode;

            if (!IsDemolishMode)
                ClearPendingDemolitionsPreview();

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

            if (_pendingDemolitionPositions.Contains(position))
            {
                for (int i = _pendingDemolitions.Count - 1; i >= 0; i--)
                {
                    if (_pendingDemolitions[i].Position == position)
                    {
                        var pendingBuildingId = _pendingDemolitions[i].BuildingId;
                        _pendingDemolitions.RemoveAt(i);
                        _pendingDemolitionPositions.Remove(position);

                        _signalBus.Fire(new BuildingPreviewChangedSignal
                        {
                            Position = position,
                            BuildingId = pendingBuildingId,
                            PreviewState = BuildingPreviewState.None
                        });

                        if (VerboseLogs)
                            Debug.Log($"[Construction] Pending demolition unmarked for '{pendingBuildingId}' at {position}. pendingCount={_pendingDemolitions.Count}");

                        return true;
                    }
                }

                if (VerboseLogs)
                    Debug.Log($"[Construction] TryDemolishAt({position}) ignored: pending position exists but mark entry was not found.");
                return true;
            }

            _pendingDemolitions.Add(new PendingDemolition(position, buildingId));
            _pendingDemolitionPositions.Add(position);

            _signalBus.Fire(new BuildingPreviewChangedSignal
            {
                Position = position,
                BuildingId = buildingId,
                PreviewState = BuildingPreviewState.Valid
            });

            if (VerboseLogs)
                Debug.Log($"[Construction] Pending demolition marked for '{buildingId}' at {position}. pendingCount={_pendingDemolitions.Count}");

            return true;
        }

        private void ConfirmPendingDemolitions()
        {
            if (VerboseLogs)
                Debug.Log($"[Construction] Confirm demolish requested. count={_pendingDemolitions.Count}");

            for (int i = 0; i < _pendingDemolitions.Count; i++)
            {
                var demolition = _pendingDemolitions[i];
                var pos = demolition.Position;
                var id = demolition.BuildingId;

                if (!_playerPlacedBuildings.ContainsKey(pos) || !_objectsMapService.IsOccupied(pos))
                    continue;

                _objectsMapService.Unregister(pos);
                _playerPlacedBuildings.Remove(pos);
                _signalBus.Fire(new BuildingDemolishedSignal
                {
                    BuildingId = id,
                    Position = pos,
                    OwnerId = _activeOwnerId,
                });

                if (VerboseLogs)
                    Debug.Log($"[Construction] Confirm demolished '{id}' at {pos}");
            }

            _pendingDemolitions.Clear();
            _pendingDemolitionPositions.Clear();

            if (VerboseLogs)
                Debug.Log("[Construction] Confirm demolish completed.");

            return;
        }

        public IReadOnlyDictionary<Vector2Int, string> GetPlayerPlacedBuildings()
            => new ReadOnlyDictionary<Vector2Int, string>(_playerPlacedBuildings);

        public bool RemovePendingAt(Vector2Int position)
        {
            int index = FindPendingPlacementIndex(position);
            if (index < 0)
                return false;

            var placement = _pendingPlacements[index];

            SaveSnapshotForUndo(clearRedoHistory: true);

            _pendingPlacements.RemoveAt(index);
            _pendingPositions.Remove(position);

            _signalBus.Fire(new BuildingPreviewChangedSignal
            {
                Position = position,
                BuildingId = placement.BuildingId,
                PreviewState = BuildingPreviewState.None
            });

            if (VerboseLogs)
                Debug.Log($"[Construction] RemovePendingAt({position}) -> removed '{placement.BuildingId}'. pendingCount={_pendingPlacements.Count}");

            if (_pendingPlacements.Count == 0)
                State = BuildingPlacementState.Placing;

            return true;
        }

        public void RestoreFromSave(Vector2Int position, string buildingId)
        {
            if (_objectsMapService.IsOccupied(position))
            {
                Debug.LogWarning($"[Construction] RestoreFromSave: позиція {position} вже зайнята, пропускаємо '{buildingId}'.");
                return;
            }

            _objectsMapService.Register(position, buildingId);
            _playerPlacedBuildings[position] = buildingId;
            _signalBus.Fire(new BuildingPlacedSignal
            {
                BuildingId = buildingId,
                Position = position,
                OwnerId = _activeOwnerId,
            });

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
        /// Повертає true якщо тайл НЕ видимий (тобто FogState != Visible).
        /// Для будівництва дозволяється тільки Visible.
        /// Якщо FogOfWar не підключений — завжди false (дозволено).
        /// </summary>
        private bool IsBlockedByFog(Vector2Int position)
        {
            if (_fogOfWarService == null) return false;
            return _fogOfWarService.GetFogState(position) != FogStateType.Visible;
        }

        private bool CanPlaceAt(
            Vector2Int position,
            Vector2Int? ignoredPendingPosition,
            string buildingId,
            out bool tileOccupied,
            out bool spacingBlocked,
            out bool fogBlocked,
            out bool townHallZoneBlocked)
        {
            tileOccupied = _objectsMapService.IsOccupied(position)
                || (_pendingPositions.Contains(position) && position != ignoredPendingPosition);
            spacingBlocked = !tileOccupied && IsBlockedBySpacing(position, ignoredPendingPosition);
            fogBlocked = !tileOccupied && !spacingBlocked && IsBlockedByFog(position);
            townHallZoneBlocked = !tileOccupied && !spacingBlocked && !fogBlocked
                && IsBlockedByTownHallZone(position, buildingId, ignoredPendingPosition);

            return !tileOccupied && !spacingBlocked && !fogBlocked && !townHallZoneBlocked;
        }

        /// <summary>
        /// Правило поселення: будь-яку не-ратушу можна ставити лише в зоні дії ратуші.
        /// Якщо ратуш ще немає — блокуємо будівництво всіх інших будівель.
        /// </summary>
        private bool IsBlockedByTownHallZone(Vector2Int position, string buildingId, Vector2Int? ignoredPendingPosition)
        {
            if (_townHallBuildRadius <= 0)
                return false;

            if (string.IsNullOrWhiteSpace(buildingId) || _buildingRegistry == null)
                return false;

            var candidate = _buildingRegistry.GetById(buildingId);
            if (candidate == null)
                return false;

            // Ратушу дозволяємо ставити будь-де (інші обмеження застосовуються окремо).
            if (candidate.IsTownHall)
                return false;

            int radiusSq = _townHallBuildRadius * _townHallBuildRadius;

            // 1) Уже підтверджені гравцем ратуші
            foreach (var kvp in _playerPlacedBuildings)
            {
                var def = _buildingRegistry.GetById(kvp.Value);
                if (def == null || !def.IsTownHall)
                    continue;

                if ((kvp.Key - position).sqrMagnitude <= radiusSq)
                    return false;
            }

            // 2) Ратуша в поточному pending-сеті (дозволяє в одній сесії: спочатку ратушу, потім інші)
            for (int i = 0; i < _pendingPlacements.Count; i++)
            {
                var pending = _pendingPlacements[i];
                if (pending.Position == ignoredPendingPosition)
                    continue;

                var pendingDef = _buildingRegistry.GetById(pending.BuildingId);
                if (pendingDef == null || !pendingDef.IsTownHall)
                    continue;

                if ((pending.Position - position).sqrMagnitude <= radiusSq)
                    return false;
            }

            return true;
        }

        private bool AddPendingPlacement(Vector2Int position, string buildingId, bool clearRedoHistory)
        {
            SaveSnapshotForUndo(clearRedoHistory);

            _pendingPlacements.Add(new PendingPlacement(position, buildingId));
            _pendingPositions.Add(position);

            _signalBus.Fire(new BuildingPreviewChangedSignal
            {
                Position = position,
                BuildingId = buildingId,
                PreviewState = BuildingPreviewState.Valid
            });

            if (VerboseLogs)
                Debug.Log($"[Construction] Pending placement added for '{buildingId}' at {position}. pendingCount={_pendingPlacements.Count}, undoCount={_undoSnapshots.Count}, redoCount={_redoSnapshots.Count}");

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

        private bool TryReplacePendingWallWithGate(Vector2Int position, string gateBuildingId)
        {
            if (_wallPlacementService == null || !_wallPlacementService.IsGate(gateBuildingId))
                return false;

            int index = FindPendingPlacementIndex(position);
            if (index < 0)
                return false;

            var current = _pendingPlacements[index];
            if (!_wallPlacementService.CanReplaceWallWithGate(position, gateBuildingId, out _))
                return false;

            if (current.BuildingId == gateBuildingId)
                return true;

            SaveSnapshotForUndo(clearRedoHistory: true);

            _pendingPlacements[index] = new PendingPlacement(position, gateBuildingId);
            _selectedBuildingId = gateBuildingId;

            _signalBus.Fire(new BuildingPreviewChangedSignal
            {
                Position = position,
                BuildingId = gateBuildingId,
                PreviewState = BuildingPreviewState.Valid
            });

            if (VerboseLogs)
                Debug.Log($"[Construction] Pending wall at {position} replaced with gate '{gateBuildingId}'.");

            return true;
        }

        private void ResetSession(bool clearRedoHistory)
        {
            if (VerboseLogs)
                Debug.Log($"[Construction] ResetSession requested. pendingCount={_pendingPlacements.Count}, redoCount={_redoSnapshots.Count}, clearRedoHistory={clearRedoHistory}");

            if (!clearRedoHistory && _pendingPlacements.Count > 0)
            {
                _redoSnapshots.Clear();
                // Після Cancel можемо повернути останній стан однією операцією Redo.
                _redoSnapshots.Add(ClonePendingSnapshot());
            }

            for (int i = _pendingPlacements.Count - 1; i >= 0; i--)
            {
                var placement = _pendingPlacements[i];
                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = placement.Position,
                    BuildingId = placement.BuildingId,
                    PreviewState = BuildingPreviewState.None
                });

            }

            _pendingPlacements.Clear();
            _pendingPositions.Clear();
            if (clearRedoHistory)
                _redoSnapshots.Clear();

            ClearPendingDemolitionsPreview();

            _undoSnapshots.Clear();

            _signalBus.Fire(new BuildingCancelledSignal());
            State = BuildingPlacementState.Idle;
            _selectedBuildingId = null;

            if (VerboseLogs)
                Debug.Log($"[Construction] ResetSession completed. state=Idle, undoCount={_undoSnapshots.Count}, redoCount={_redoSnapshots.Count}");
        }

        private void ClearPendingDemolitionsPreview()
        {
            for (int i = 0; i < _pendingDemolitions.Count; i++)
            {
                var demolition = _pendingDemolitions[i];
                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = demolition.Position,
                    BuildingId = demolition.BuildingId,
                    PreviewState = BuildingPreviewState.None
                });
            }

            _pendingDemolitions.Clear();
            _pendingDemolitionPositions.Clear();
        }

        private void SaveSnapshotForUndo(bool clearRedoHistory)
        {
            _undoSnapshots.Add(ClonePendingSnapshot());
            if (clearRedoHistory)
                _redoSnapshots.Clear();
        }

        private List<PendingPlacement> ClonePendingSnapshot()
        {
            return new List<PendingPlacement>(_pendingPlacements);
        }

        private void ApplyPendingSnapshot(List<PendingPlacement> snapshot)
        {
            var previous = ClonePendingSnapshot();

            _pendingPlacements.Clear();
            _pendingPositions.Clear();

            for (int i = 0; i < snapshot.Count; i++)
            {
                var placement = snapshot[i];
                _pendingPlacements.Add(placement);
                _pendingPositions.Add(placement.Position);
            }

            var previousByPosition = new Dictionary<Vector2Int, PendingPlacement>();
            for (int i = 0; i < previous.Count; i++)
                previousByPosition[previous[i].Position] = previous[i];

            var currentByPosition = new Dictionary<Vector2Int, PendingPlacement>();
            for (int i = 0; i < _pendingPlacements.Count; i++)
                currentByPosition[_pendingPlacements[i].Position] = _pendingPlacements[i];

            foreach (var pair in previousByPosition)
            {
                if (!currentByPosition.TryGetValue(pair.Key, out var current) || current.BuildingId != pair.Value.BuildingId)
                {
                    _signalBus.Fire(new BuildingPreviewChangedSignal
                    {
                        Position = pair.Key,
                        BuildingId = pair.Value.BuildingId,
                        PreviewState = BuildingPreviewState.None
                    });
                }
            }

            foreach (var pair in currentByPosition)
            {
                if (!previousByPosition.TryGetValue(pair.Key, out var previousPlacement) || previousPlacement.BuildingId != pair.Value.BuildingId)
                {
                    _signalBus.Fire(new BuildingPreviewChangedSignal
                    {
                        Position = pair.Key,
                        BuildingId = pair.Value.BuildingId,
                        PreviewState = BuildingPreviewState.Valid
                    });
                }
            }
        }
    }
}
