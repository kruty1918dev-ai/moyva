using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Diagnostics.API;
using Kruty1918.Moyva.Diagnostics.Runtime.Flows;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.WorldCreation.API;
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
        private readonly IEconomyInfoMediator _economyInfoMediator;
        private readonly IGridService _gridService;
        private readonly IGeneratedTerrainLevelQuery _generatedTerrainLevelQuery;
        private readonly WorldCreationDefaultsSO _worldDefaults;
        private readonly ITileSettingsService _tileSettings; // може бути null
        private readonly IConstructionDiagnostics _diagnostics;
        private readonly IConstructionDiagnosticsSession _diagnosticsSession;
        private bool _initialized;
        private bool _disposed;

        private string _selectedBuildingId;
        private readonly List<PendingPlacement> _pendingPlacements = new();
        private readonly List<List<PendingPlacement>> _undoSnapshots = new();
        private readonly List<List<PendingPlacement>> _redoSnapshots = new();
        private readonly HashSet<Vector2Int> _pendingPositions = new();
        private readonly Dictionary<Vector2Int, ConstructionPendingPlacementStatus> _pendingPlacementStatuses = new();
        private readonly List<PendingDemolition> _pendingDemolitions = new();
        private readonly HashSet<Vector2Int> _pendingDemolitionPositions = new();
        // Позиції будівель, підтверджених гравцем під час гри (знесення дозволено лише для них)
        private readonly Dictionary<Vector2Int, string> _playerPlacedBuildings = new();
        private string _activeOwnerId = DefaultOwnerId;
        private string _lastActionMessage = string.Empty;
        private readonly Dictionary<Vector2Int, (string BuildingId, string FactionId)> _factionPlacedBuildings = new();
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
            [InjectOptional] IWallPlacementService wallPlacementService,
            [InjectOptional] IEconomyInfoMediator economyInfoMediator,
            [InjectOptional] IGridService gridService,
            [InjectOptional] IGeneratedTerrainLevelQuery generatedTerrainLevelQuery,
            [InjectOptional] WorldCreationDefaultsSO worldDefaults = null,
            [InjectOptional] ITileSettingsService tileSettings = null,
            [InjectOptional] IConstructionDiagnostics diagnostics = null,
            [InjectOptional] IConstructionDiagnosticsSession diagnosticsSession = null)
        {
            _objectsMapService = objectsMapService;
            _buildingRegistry = buildingRegistry;
            _signalBus = signalBus;
            _minSpacing = minSpacing;
            _townHallBuildRadius = Mathf.Max(0, townHallBuildRadius);
            _fogOfWarService = fogOfWarService;
            _wallPlacementService = wallPlacementService;
            _economyInfoMediator = economyInfoMediator;
            _gridService = gridService;
            _generatedTerrainLevelQuery = generatedTerrainLevelQuery;
            _worldDefaults = worldDefaults;
            _tileSettings = tileSettings;
            _diagnostics = diagnostics;
            _diagnosticsSession = diagnosticsSession;
        }

        public void Initialize()
        {
            if (_disposed || _initialized)
                return;

            Debug.Log("[Construction] Initialize() почало роботу...");
            
            try
            {
                if (_signalBus == null)
                {
                    Debug.LogError("[Construction] Initialize: _signalBus == null");
                    return;
                }

                _signalBus.Subscribe<GameModeChangedSignal>(OnGameModeChanged);
                _initialized = true;
                Debug.Log("[Construction] ✓ GameModeChangedSignal підписано");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Construction] ПОМИЛКА в Initialize(): {ex.GetType().Name} - {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            Debug.Log("[Construction] Dispose() почало роботу...");
            
            try
            {
                if (_signalBus == null)
                {
                    Debug.LogWarning("[Construction] Dispose: _signalBus == null");
                    return;
                }

                _signalBus.TryUnsubscribe<GameModeChangedSignal>(OnGameModeChanged);
                Debug.Log("[Construction] ✓ GameModeChangedSignal відписано");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Construction] ПОМИЛКА в Dispose(): {ex.GetType().Name} - {ex.Message}");
            }
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
            Debug.Log($"[Construction] SelectBuilding('{buildingId}') викликана. active={_isActive}");

            if (!_isActive)
            {
                Debug.LogWarning("[Construction] SelectBuilding: Construction mode ВИМКНЕНА");
                return;
            }

            if (string.IsNullOrWhiteSpace(buildingId))
            {
                Debug.LogWarning("[Construction] SelectBuilding: buildingId порожня");
                return;
            }

            try
            {
                // Вибір будівлі завжди переводить UX у режим розміщення.
                ClearPendingDemolitionsPreview();
                IsDemolishMode = false;
                _selectedBuildingId = buildingId;
                State = BuildingPlacementState.Placing;

                Debug.Log($"[Construction] ✓ SelectBuilding -> id='{_selectedBuildingId}', state={State}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Construction] ПОМИЛКА в SelectBuilding('{buildingId}'): {ex.GetType().Name} - {ex.Message}");
            }
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
            // Перевірка стану
            if (State != BuildingPlacementState.Placing)
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] TryPreviewAt({position}) проігнорована: неправильний стан {State}");
                return false;
            }

            // Перевірка, чи вибрано будівлю
            if (string.IsNullOrWhiteSpace(_selectedBuildingId))
            {
                Debug.LogWarning("[Construction] TryPreviewAt: _selectedBuildingId порожній або null");
                return false;
            }

            bool selectedIsGate = _wallPlacementService != null && _wallPlacementService.IsGate(_selectedBuildingId);

            // Перевірка на дублювання позиції
            if (_pendingPositions.Contains(position))
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] TryPreviewAt({position}): позиція вже у pending-списку");
                
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
                if (VerboseLogs)
                    Debug.Log($"[Construction] Ворота на {position} не можуть замінити стіну");
                
                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = position,
                    BuildingId = _selectedBuildingId,
                    PreviewState = BuildingPreviewState.Blocked
                });
                return false;
            }

            // Основна перевірка розміщення
            if (!gateReplacementAllowed && !CanPlaceAt(position, null, _selectedBuildingId, out var tileOccupied, out var spacingBlocked, out var fogBlocked, out var influenceZoneBlocked, out var terrainBlocked))
            {
                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = position,
                    BuildingId = _selectedBuildingId,
                    PreviewState = BuildingPreviewState.Blocked
                });

                if (VerboseLogs)
                    Debug.Log($"[Construction] TryPreviewAt({position}) -> BLOCKED. occupied={tileOccupied}, spacingViolation={spacingBlocked}, fogBlocked={fogBlocked}, influenceZoneBlocked={influenceZoneBlocked}, terrainBlocked={terrainBlocked}, minSpacing={_minSpacing}, townHallBuildRadius={_townHallBuildRadius}");

                return false;
            }

            if (!TryValidateConstructionResources(position, _selectedBuildingId, _activeOwnerId, ignoredPendingPosition: null, out var resourceReason))
            {
                _lastActionMessage = resourceReason;
                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = position,
                    BuildingId = _selectedBuildingId,
                    PreviewState = BuildingPreviewState.Blocked
                });

                if (VerboseLogs)
                    Debug.Log($"[Construction] TryPreviewAt({position}) -> BLOCKED. resourcesBlocked=True, reason={resourceReason}");

                return false;
            }

            if (VerboseLogs)
                Debug.Log($"[Construction] TryPreviewAt({position}) -> VALID для {_selectedBuildingId}");

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

        public IReadOnlyDictionary<Vector2Int, string> GetPendingPlacements()
        {
            var snapshot = new Dictionary<Vector2Int, string>(_pendingPlacements.Count);
            for (int index = 0; index < _pendingPlacements.Count; index++)
            {
                var placement = _pendingPlacements[index];
                if (string.IsNullOrWhiteSpace(placement.BuildingId))
                    continue;

                snapshot[placement.Position] = placement.BuildingId;
            }

            return new ReadOnlyDictionary<Vector2Int, string>(snapshot);
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

            if (!gateReplacementAllowed && !CanPlaceAt(toPosition, fromPosition, placement.BuildingId, out var tileOccupied, out var spacingBlocked, out var fogBlocked, out var influenceZoneBlocked, out var terrainBlocked))
            {
                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = toPosition,
                    BuildingId = placement.BuildingId,
                    PreviewState = BuildingPreviewState.Blocked
                });

                if (VerboseLogs)
                    Debug.Log($"[Construction] TryMovePendingPlacement({fromPosition} -> {toPosition}) -> BLOCKED. occupied={tileOccupied}, spacingViolation={spacingBlocked}, fogBlocked={fogBlocked}, influenceZoneBlocked={influenceZoneBlocked}, terrainBlocked={terrainBlocked}, minSpacing={_minSpacing}, townHallBuildRadius={_townHallBuildRadius}");

                return false;
            }

            if (!TryValidateConstructionResources(toPosition, placement.BuildingId, _activeOwnerId, fromPosition, out var resourceReason))
            {
                _lastActionMessage = resourceReason;
                _signalBus.Fire(new BuildingPreviewChangedSignal
                {
                    Position = toPosition,
                    BuildingId = placement.BuildingId,
                    PreviewState = BuildingPreviewState.Blocked
                });

                if (VerboseLogs)
                    Debug.Log($"[Construction] TryMovePendingPlacement({fromPosition} -> {toPosition}) -> BLOCKED. resourcesBlocked=True, reason={resourceReason}");

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
            IDiagnosticFlow flow = _diagnosticsSession?.CurrentFlow;
            if (IsDemolishMode)
            {
                ConfirmPendingDemolitions();
                _diagnostics?.SkipStep(flow, ConstructionDiagnosticSteps.BuildConfirmed, "demolish-mode");
                _diagnostics?.Report(flow);
                _diagnosticsSession?.Clear(flow);
                return;
            }

            if (VerboseLogs)
                Debug.Log($"[Construction] Confirm requested. count={_pendingPlacements.Count}");

            if (_pendingPlacements.Count == 0)
            {
                if (VerboseLogs)
                    Debug.Log("[Construction] Confirm ignored: no pending placements.");
                _diagnostics?.FailStep(flow, ConstructionDiagnosticSteps.BuildConfirmed, "no-pending-placements");
                _diagnostics?.Report(flow);
                _diagnosticsSession?.Clear(flow);
                return;
            }

            _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.BuildConfirmed, $"pending={_pendingPlacements.Count}");

            var pendingSnapshot = new List<PendingPlacement>(_pendingPlacements);
            var confirmedPositions = new HashSet<Vector2Int>();
            int confirmedCount = 0;
            int skippedCount = 0;

            foreach (var placement in pendingSnapshot)
            {
                var pos = placement.Position;
                var id = placement.BuildingId;
                try
                {
                    bool gateReplacementAllowed = _wallPlacementService != null
                        && _wallPlacementService.IsGate(id)
                        && _wallPlacementService.CanReplaceWallWithGate(pos, id, out _);

                    if (!gateReplacementAllowed && !CanPlaceAt(pos, pos, id, out var tileOccupied, out var spacingBlocked, out var fogBlocked, out var influenceZoneBlocked, out var terrainBlocked))
                    {
                        skippedCount++;
                        _diagnostics?.FailStep(flow, ConstructionDiagnosticSteps.GridCellValidated, "placement-invalid", $"building={id}, pos={pos}");
                        Debug.LogWarning($"[Construction] Confirm skipped '{id}' at {pos}: placement became invalid. occupied={tileOccupied}, spacingViolation={spacingBlocked}, fogBlocked={fogBlocked}, influenceZoneBlocked={influenceZoneBlocked}, terrainBlocked={terrainBlocked}, townHallBuildRadius={_townHallBuildRadius}.");
                        continue;
                    }

                    _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.GridCellValidated, $"building={id}, pos={pos}");
                    _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.TerrainValidated, $"building={id}, pos={pos}");

                    if (_objectsMapService.IsOccupied(pos))
                    {
                        if (!gateReplacementAllowed)
                        {
                            skippedCount++;
                            Debug.LogWarning($"[Construction] Confirm skipped '{id}' at {pos}: tile occupied.");
                            continue;
                        }

                        _objectsMapService.Unregister(pos);
                    }

                    if (!TryConsumeConstructionResources(pos, id, _activeOwnerId, out var resourceReason))
                    {
                        skippedCount++;
                        _lastActionMessage = resourceReason;
                        _diagnostics?.FailStep(flow, ConstructionDiagnosticSteps.ResourcesChecked, "resources-blocked", resourceReason);
                        Debug.LogWarning($"[Construction] Confirm skipped '{id}' at {pos}: {resourceReason}");
                        continue;
                    }

                    _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.ResourcesChecked, $"building={id}, owner={_activeOwnerId}");
                    _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.ResourcesReserved, $"building={id}, owner={_activeOwnerId}");

                    _objectsMapService.Register(pos, id);
                    _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.BuildingRegistered, $"building={id}, pos={pos}");
                    _playerPlacedBuildings[pos] = id;

                    _signalBus.Fire(new BuildingPlacedSignal
                    {
                        BuildingId = id,
                        Position = pos,
                        OwnerId = _activeOwnerId,
                    });
                    _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.BuildingSpawned, $"building={id}, pos={pos}");
                    _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.ConstructionSignalFired, $"building={id}, pos={pos}");

                    try
                    {
                        ApplyBuildingFogReveal(id, pos);
                    }
                    catch (Exception fogEx)
                    {
                        Debug.LogError($"[Construction] Fog reveal failed for '{id}' at {pos}: {fogEx.GetType().Name} - {fogEx.Message}");
                    }

                    confirmedPositions.Add(pos);
                    confirmedCount++;

                    if (VerboseLogs)
                        Debug.Log($"[Construction] Confirm placed '{id}' at {pos}");
                }
                catch (Exception ex)
                {
                    skippedCount++;
                    Debug.LogError($"[Construction] Confirm failed for '{id}' at {pos}: {ex.GetType().Name} - {ex.Message}");
                }
            }

            if (confirmedPositions.Count > 0)
            {
                for (int index = _pendingPlacements.Count - 1; index >= 0; index--)
                {
                    var pending = _pendingPlacements[index];
                    if (!confirmedPositions.Contains(pending.Position))
                        continue;

                    _pendingPlacements.RemoveAt(index);
                    _pendingPositions.Remove(pending.Position);
                    _pendingPlacementStatuses.Remove(pending.Position);

                    _signalBus.Fire(new BuildingPreviewChangedSignal
                    {
                        Position = pending.Position,
                        BuildingId = pending.BuildingId,
                        PreviewState = BuildingPreviewState.None
                    });
                }
            }

            _undoSnapshots.Clear();
            _redoSnapshots.Clear();

            if (_pendingPlacements.Count == 0)
            {
                State = BuildingPlacementState.Idle;
                _selectedBuildingId = null;
            }
            else
            {
                State = BuildingPlacementState.Placing;
                _selectedBuildingId = _pendingPlacements[_pendingPlacements.Count - 1].BuildingId;
            }

            if (VerboseLogs)
                Debug.Log($"[Construction] Confirm completed. confirmed={confirmedCount}, skipped={skippedCount}, remainingPending={_pendingPlacements.Count}, state={State}");

            if (confirmedCount > 0)
                _diagnostics?.CompleteStep(flow, ConstructionDiagnosticSteps.UiUpdated, $"confirmed={confirmedCount}, skipped={skippedCount}");
            else
                _diagnostics?.FailStep(flow, ConstructionDiagnosticSteps.BuildingSpawned, "no-buildings-confirmed", $"skipped={skippedCount}");

            _diagnostics?.Report(flow);
            _diagnosticsSession?.Clear(flow);
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
            ApplyBuildingFogReveal(buildingId, position);

            if (VerboseLogs)
                Debug.Log($"[Construction] RestoreFromSave: відновлено '{buildingId}' на {position}");
        }

        public bool TryDirectPlace(string buildingId, Vector2Int position, string placedByFactionId)
        {
            if (string.IsNullOrWhiteSpace(buildingId))
            {
                Debug.LogWarning($"[Construction] TryDirectPlace({position}): buildingId порожній.");
                return false;
            }

            string ownerId = string.IsNullOrWhiteSpace(placedByFactionId)
                ? DefaultOwnerId
                : placedByFactionId.Trim();

            if (_objectsMapService.IsOccupied(position))
            {
                if (VerboseLogs) Debug.Log($"[Construction] TryDirectPlace({buildingId},{position}): тайл зайнятий.");
                return false;
            }

            if (IsBlockedByInfluenceZone(position, buildingId, ignoredPendingPosition: null))
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] TryDirectPlace({buildingId},{position}) -> BLOCKED. influenceZoneBlocked=True, townHallBuildRadius={_townHallBuildRadius}.");
                return false;
            }

            if (IsBlockedByTerrain(position, out var terrainReason))
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] TryDirectPlace({buildingId},{position}) -> BLOCKED. terrainBlocked=True, reason={terrainReason}.");
                return false;
            }

            if (!TryConsumeConstructionResources(position, buildingId, ownerId, out var resourceReason))
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] TryDirectPlace({buildingId},{position}) -> BLOCKED. resourcesBlocked=True, reason={resourceReason}.");
                return false;
            }

            _objectsMapService.Register(position, buildingId);
            _factionPlacedBuildings[position] = (buildingId, ownerId);
            _signalBus.Fire(new BuildingPlacedSignal
            {
                BuildingId = buildingId,
                Position = position,
                OwnerId = ownerId,
                SourceFactionId = ownerId,
            });
            ApplyBuildingFogReveal(buildingId, position);
            if (VerboseLogs) Debug.Log($"[Construction] TryDirectPlace: розміщено '{buildingId}' на {position} від '{ownerId}'.");
            return true;
        }

        public bool TryDemolishByFaction(Vector2Int position, string factionId)
        {
            if (!_factionPlacedBuildings.TryGetValue(position, out var entry) || entry.FactionId != factionId)
            {
                if (VerboseLogs) Debug.Log($"[Construction] TryDemolishByFaction({position}): будівля не знайдена або не належить фракції '{factionId}'.");
                return false;
            }
            _objectsMapService.Unregister(position);
            _factionPlacedBuildings.Remove(position);
            _signalBus.Fire(new BuildingDemolishedSignal { BuildingId = entry.BuildingId, Position = position, SourceFactionId = factionId });
            if (VerboseLogs) Debug.Log($"[Construction] TryDemolishByFaction: знесено '{entry.BuildingId}' на {position} від '{factionId}'.");
            return true;
        }

        public bool HasPlacedBuilding(string buildingId, string ownerId = null)
        {
            if (string.IsNullOrWhiteSpace(buildingId))
                return false;

            string normalizedOwner = string.IsNullOrWhiteSpace(ownerId) ? null : ownerId.Trim();

            foreach (var pair in _factionPlacedBuildings)
            {
                if (!string.Equals(pair.Value.BuildingId, buildingId, StringComparison.Ordinal))
                    continue;

                if (normalizedOwner == null || string.Equals(pair.Value.FactionId, normalizedOwner, StringComparison.Ordinal))
                    return true;
            }

            if (normalizedOwner == null || string.Equals(normalizedOwner, _activeOwnerId, StringComparison.Ordinal))
            {
                foreach (var pair in _playerPlacedBuildings)
                {
                    if (string.Equals(pair.Value, buildingId, StringComparison.Ordinal))
                        return true;
                }
            }

            return false;
        }

        public bool TryGetPendingPlacementStatus(Vector2Int position, out ConstructionPendingPlacementStatus status)
        {
            // Перевіряємо, чи є непідтверджена будівля на цій позиції
            int index = FindPendingPlacementIndex(position);
            if (index < 0)
            {
                status = default;
                return false;
            }

            var placement = _pendingPlacements[index];
            var projection = BuildResourceProjectionForPlacement(
                placement.Position,
                placement.BuildingId,
                _activeOwnerId,
                ignoredPendingPosition: placement.Position);

            status = new ConstructionPendingPlacementStatus(
                position: position,
                buildingId: placement.BuildingId,
                settlementId: projection.SettlementId ?? "Unknown",
                settlementName: projection.SettlementName ?? "Unknown",
                hasSettlement: projection.HasSettlement,
                isAffordable: !projection.HasDeficit,
                errorMessage: projection.HasDeficit ? projection.Message : string.Empty
            );

            return true;
        }

        public ConstructionResourceProjection GetResourceProjection(Vector2Int position)
        {
            // Якщо цій позиції немає предпросмотру, повертаємо Empty
            if (!HasPendingPlacementAt(position))
                return ConstructionResourceProjection.Empty;

            try
            {
                if (!TryGetPendingBuildingIdAt(position, out var buildingId))
                    return ConstructionResourceProjection.Empty;

                return BuildResourceProjectionForPlacement(position, buildingId, _activeOwnerId, ignoredPendingPosition: position);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[Construction] GetResourceProjection error: {ex.Message}");
                return ConstructionResourceProjection.Empty;
            }
        }

        private bool TryValidateConstructionResources(
            Vector2Int position,
            string buildingId,
            string ownerId,
            Vector2Int? ignoredPendingPosition,
            out string reason)
        {
            var projection = BuildResourceProjectionForPlacement(position, buildingId, ownerId, ignoredPendingPosition);
            if (!projection.HasDeficit)
            {
                reason = null;
                return true;
            }

            reason = projection.Message;
            return false;
        }

        private bool TryConsumeConstructionResources(
            Vector2Int position,
            string buildingId,
            string ownerId,
            out string reason)
        {
            reason = null;
            string normalizedOwnerId = NormalizeOwnerId(ownerId);

            var costs = BuildConstructionCostMap(buildingId);
            if (costs.Count == 0)
                return true;

            if (_economyInfoMediator == null)
            {
                reason = "Економіка не підключена: неможливо перевірити ресурси для будівництва.";
                return false;
            }

            if (ShouldUseOwnerPoolConstructionFunding(normalizedOwnerId))
            {
                // Завдання: перші будівлі мають будуватися зі стартового owner-pool, навіть коли складу ще немає.
                if (!_economyInfoMediator.TryConsumeOwnerPoolResources(normalizedOwnerId, costs, out reason))
                    return false;

                reason = null;
                return true;
            }

            if (!_economyInfoMediator.TryResolveConstructionSettlement(position, normalizedOwnerId, out var settlement)
                || string.IsNullOrWhiteSpace(settlement.SettlementId))
            {
                reason = "Не знайдено поселення/замок для списання ресурсів у цій зоні будівництва.";
                return false;
            }

            if (!_economyInfoMediator.TryConsumeSettlementResources(settlement.SettlementId, costs, out reason))
                return false;

            reason = null;
            return true;
        }

        private ConstructionResourceProjection BuildResourceProjectionForPlacement(
            Vector2Int position,
            string buildingId,
            string ownerId,
            Vector2Int? ignoredPendingPosition)
        {
            string normalizedOwnerId = NormalizeOwnerId(ownerId);
            var costs = BuildConstructionCostMap(buildingId);
            if (costs.Count == 0)
            {
                return new ConstructionResourceProjection(
                    normalizedOwnerId,
                    null,
                    null,
                    hasSettlement: false,
                    hasDeficit: false,
                    message: string.Empty,
                    balances: new List<ConstructionResourceBalance>());
            }

            if (_economyInfoMediator == null)
            {
                return new ConstructionResourceProjection(
                    normalizedOwnerId,
                    null,
                    null,
                    hasSettlement: false,
                    hasDeficit: true,
                    message: "Економіка не підключена: неможливо перевірити ресурси для будівництва.",
                    balances: new List<ConstructionResourceBalance>());
            }

            bool hasSettlement = _economyInfoMediator.TryResolveConstructionSettlement(position, normalizedOwnerId, out var settlement)
                && !string.IsNullOrWhiteSpace(settlement.SettlementId);

            if (ShouldUseOwnerPoolConstructionFunding(normalizedOwnerId))
            {
                // Завдання: preview має показувати ті самі стартові ресурси, які confirm реально спише з owner-pool.
                var ownerPoolAvailable = _economyInfoMediator.GetOwnerPoolResourceTotals(normalizedOwnerId);
                var ownerPoolReserved = BuildReservedOwnerPoolCosts(normalizedOwnerId, ignoredPendingPosition);
                AddCosts(ownerPoolReserved, costs);

                var ownerPoolBalances = new List<ConstructionResourceBalance>(ownerPoolReserved.Count);
                bool ownerPoolHasDeficit = false;
                string ownerPoolDeficitMessage = string.Empty;

                foreach (var pair in ownerPoolReserved)
                {
                    float availableAmount = ownerPoolAvailable != null && ownerPoolAvailable.TryGetValue(pair.Key, out var value)
                        ? value
                        : 0f;
                    var balance = new ConstructionResourceBalance(pair.Key, availableAmount, pair.Value);
                    ownerPoolBalances.Add(balance);
                    if (balance.IsDeficit && string.IsNullOrEmpty(ownerPoolDeficitMessage))
                    {
                        ownerPoolHasDeficit = true;
                        ownerPoolDeficitMessage = $"Недостатньо ресурсу '{ResolveResourceDisplayName(pair.Key)}' у стартовому запасі власника '{normalizedOwnerId}': потрібно {pair.Value:0.#}, доступно {availableAmount:0.#}.";
                    }
                }

                ownerPoolBalances.Sort((left, right) => string.CompareOrdinal(left.ResourceId, right.ResourceId));

                return new ConstructionResourceProjection(
                    normalizedOwnerId,
                    hasSettlement ? settlement.SettlementId : null,
                    hasSettlement ? settlement.SettlementName : null,
                    hasSettlement: hasSettlement,
                    hasDeficit: ownerPoolHasDeficit,
                    message: ownerPoolHasDeficit ? ownerPoolDeficitMessage : string.Empty,
                    balances: ownerPoolBalances);
            }

            if (!hasSettlement)
            {
                return new ConstructionResourceProjection(
                    normalizedOwnerId,
                    null,
                    null,
                    hasSettlement: false,
                    hasDeficit: true,
                    message: "Не знайдено поселення/замок для ресурсів у цій зоні будівництва.",
                    balances: new List<ConstructionResourceBalance>());
            }

            var available = _economyInfoMediator.GetSettlementResourceTotals(settlement.SettlementId);
            var reserved = BuildReservedConstructionCosts(settlement.SettlementId, ownerId, ignoredPendingPosition);
            AddCosts(reserved, costs);

            var balances = new List<ConstructionResourceBalance>(reserved.Count);
            bool hasDeficit = false;
            string deficitMessage = string.Empty;

            foreach (var pair in reserved)
            {
                float availableAmount = available != null && available.TryGetValue(pair.Key, out var value)
                    ? value
                    : 0f;
                var balance = new ConstructionResourceBalance(pair.Key, availableAmount, pair.Value);
                balances.Add(balance);
                if (balance.IsDeficit && string.IsNullOrEmpty(deficitMessage))
                {
                    hasDeficit = true;
                    deficitMessage = $"Недостатньо ресурсу '{ResolveResourceDisplayName(pair.Key)}' у поселенні '{settlement.SettlementName}': потрібно {pair.Value:0.#}, доступно {availableAmount:0.#}.";
                }
            }

            balances.Sort((left, right) => string.CompareOrdinal(left.ResourceId, right.ResourceId));

            return new ConstructionResourceProjection(
                settlement.OwnerId,
                settlement.SettlementId,
                settlement.SettlementName,
                hasSettlement: true,
                hasDeficit: hasDeficit,
                message: hasDeficit ? deficitMessage : string.Empty,
                balances: balances);
        }

        private Dictionary<string, float> BuildReservedOwnerPoolCosts(
            string ownerId,
            Vector2Int? ignoredPendingPosition)
        {
            var reserved = new Dictionary<string, float>(StringComparer.Ordinal);
            if (!ShouldUseOwnerPoolConstructionFunding(ownerId))
                return reserved;

            for (int i = 0; i < _pendingPlacements.Count; i++)
            {
                var placement = _pendingPlacements[i];
                if (ignoredPendingPosition.HasValue && placement.Position == ignoredPendingPosition.Value)
                    continue;

                AddCosts(reserved, BuildConstructionCostMap(placement.BuildingId));
            }

            return reserved;
        }

        private Dictionary<string, float> BuildReservedConstructionCosts(
            string settlementId,
            string ownerId,
            Vector2Int? ignoredPendingPosition)
        {
            var reserved = new Dictionary<string, float>(StringComparer.Ordinal);
            if (string.IsNullOrWhiteSpace(settlementId) || _economyInfoMediator == null)
                return reserved;

            for (int i = 0; i < _pendingPlacements.Count; i++)
            {
                var placement = _pendingPlacements[i];
                if (ignoredPendingPosition.HasValue && placement.Position == ignoredPendingPosition.Value)
                    continue;

                if (!_economyInfoMediator.TryResolveConstructionSettlement(placement.Position, ownerId, out var pendingSettlement)
                    || !string.Equals(pendingSettlement.SettlementId, settlementId, StringComparison.Ordinal))
                {
                    continue;
                }

                AddCosts(reserved, BuildConstructionCostMap(placement.BuildingId));
            }

            return reserved;
        }

        private bool ShouldUseOwnerPoolConstructionFunding(string ownerId)
        {
            // Завдання: до першого складу стартова економіка існує на owner-level, а не в settlement storage.
            // Щойно в owner є warehouse, construction повертається до стандартного settlement/warehouse funding.
            return _economyInfoMediator != null
                && !_economyInfoMediator.OwnerHasAnyWarehouse(NormalizeOwnerId(ownerId));
        }

        private static string NormalizeOwnerId(string ownerId)
        {
            return string.IsNullOrWhiteSpace(ownerId) ? DefaultOwnerId : ownerId.Trim();
        }

        private string ResolveResourceDisplayName(string resourceId)
            => _economyInfoMediator?.GetResourceDisplayName(resourceId)
               ?? (string.IsNullOrWhiteSpace(resourceId) ? string.Empty : resourceId.Trim());

        private Dictionary<string, float> BuildConstructionCostMap(string buildingId)
        {
            var result = new Dictionary<string, float>(StringComparer.Ordinal);
            var definition = string.IsNullOrWhiteSpace(buildingId)
                ? null
                : _buildingRegistry?.GetById(buildingId);

            var costs = BuildingDefinitionCapabilities.GetConstructionCost(definition);
            for (int i = 0; i < costs.Count; i++)
            {
                var entry = costs[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.ResourceId) || entry.Amount <= 0)
                    continue;

                AddCost(result, entry.ResourceId.Trim(), entry.Amount);
            }

            return result;
        }

        private static void AddCosts(Dictionary<string, float> target, IReadOnlyDictionary<string, float> source)
        {
            if (target == null || source == null)
                return;

            foreach (var pair in source)
                AddCost(target, pair.Key, pair.Value);
        }

        private static void AddCost(Dictionary<string, float> target, string resourceId, float amount)
        {
            if (target == null || string.IsNullOrWhiteSpace(resourceId) || amount <= 0f)
                return;

            string normalizedId = resourceId.Trim();
            if (target.ContainsKey(normalizedId))
                target[normalizedId] += amount;
            else
                target[normalizedId] = amount;
        }

        public string GetLastActionMessage()
        {
            return _lastActionMessage;
        }

        /// <summary>
        /// Перевіряє чи порушує позиція мінімальний відступ від існуючих / pending будівель.
        /// Chebyshev-дистанція (квадратна область навколо позиції).
        /// </summary>
        private bool IsBlockedBySpacing(Vector2Int position, Vector2Int? ignoredPendingPosition)
        {
            try
            {
                if (_minSpacing <= 0)
                {
                    if (VerboseLogs)
                        Debug.Log($"[Construction] IsBlockedBySpacing({position}): _minSpacing <= 0, spacing-перевірка відключена");
                    return false;
                }

                if (_objectsMapService == null)
                {
                    Debug.LogError("[Construction] IsBlockedBySpacing: _objectsMapService == null");
                    return false;
                }

                if (_pendingPositions == null)
                {
                    Debug.LogError("[Construction] IsBlockedBySpacing: _pendingPositions == null");
                    return false;
                }

                for (int dx = -_minSpacing; dx <= _minSpacing; dx++)
                {
                    for (int dy = -_minSpacing; dy <= _minSpacing; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        var neighbor = new Vector2Int(position.x + dx, position.y + dy);

                        bool blockedByPending = _pendingPositions.Contains(neighbor) && neighbor != ignoredPendingPosition;
                        bool isOccupied = _objectsMapService.IsOccupied(neighbor);

                        if (isOccupied || blockedByPending)
                        {
                            if (VerboseLogs)
                                Debug.Log($"[Construction] IsBlockedBySpacing({position}): BLOCKED біля {neighbor} (occupied={isOccupied}, pending={blockedByPending})");
                            return true;
                        }
                    }
                }

                if (VerboseLogs)
                    Debug.Log($"[Construction] IsBlockedBySpacing({position}): OK (spacing={_minSpacing})");

                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Construction] ПОМИЛКА в IsBlockedBySpacing({position}): {ex.GetType().Name} - {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Повертає true якщо тайл НЕ видимий (тобто FogState != Visible).
        /// Для будівництва дозволяється тільки Visible.
        /// Якщо FogOfWar не підключений — завжди false (дозволено).
        /// </summary>
        private bool IsBlockedByFog(Vector2Int position)
        {
            try
            {
                if (_fogOfWarService == null)
                {
                    if (VerboseLogs)
                        Debug.Log($"[Construction] IsBlockedByFog({position}): _fogOfWarService == null, fog-перевірка відключена");
                    return false;
                }

                var fogState = _fogOfWarService.GetFogState(position);
                bool isBlocked = fogState != FogStateType.Visible;

                if (VerboseLogs && isBlocked)
                    Debug.Log($"[Construction] IsBlockedByFog({position}): BLOCKED (fogState={fogState})");

                return isBlocked;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Construction] ПОМИЛКА в IsBlockedByFog({position}): {ex.GetType().Name} - {ex.Message}");
                return false;
            }
        }

        private void ApplyBuildingFogReveal(string buildingId, Vector2Int position)
        {
            if (_fogOfWarService == null || _buildingRegistry == null)
                return;

            var definition = _buildingRegistry.GetById(buildingId);
            if (!BuildingDefinitionCapabilities.TryGetFogReveal(definition, out var fogReveal))
                return;

            int radius = Mathf.Max(0, fogReveal.RevealRadius);
            if (radius <= 0)
            {
                _fogOfWarService.UnregisterUnit(GetBuildingFogVisionAreaId(position));
                return;
            }

            string areaId = GetBuildingFogVisionAreaId(position);
            if (fogReveal.RevealWhileActive)
            {
                _fogOfWarService.RegisterFixedVisionArea(areaId, position, radius, fogReveal.Shape);
                return;
            }

            _fogOfWarService.UnregisterUnit(areaId);

            if (fogReveal.RevealOnBuilt)
                _fogOfWarService.RevealArea(position, radius, fogReveal.Shape, keepVisible: false, areaId);
        }

        private static string GetBuildingFogVisionAreaId(Vector2Int position)
            => $"building:{position.x}:{position.y}";

        private bool IsBlockedByTerrain(Vector2Int position, out string reason)
        {
            reason = null;

            if (_generatedTerrainLevelQuery != null
                && _generatedTerrainLevelQuery.TryGetTerrainLevel(position, out int terrainLevel)
                && terrainLevel > 0)
            {
                if (HasCustomBuildingHillRestrictions())
                {
                    if (IsTerrainLevelBlocked(_worldDefaults.BlockedBuildingHillLevelRanges, terrainLevel))
                    {
                        reason = $"blocked hill level {terrainLevel}";
                        return true;
                    }
                }
            }

            if (_gridService == null)
                return false;

            if (!_gridService.TryGetTileData(position, out var tileTypeId))
            {
                reason = "outside generated grid";
                return true;
            }

            if (IsBlockedBuildingTile(tileTypeId))
            {
                reason = $"blocked tile '{tileTypeId}'";
                return true;
            }

            // Нова layer-based ідентичність: блокування за профілем шару terrain.
            if (_tileSettings != null && _tileSettings.IsBuildBlocked(tileTypeId))
            {
                reason = $"build-blocked layer '{tileTypeId}'";
                return true;
            }

            return false;
        }

        private bool HasCustomBuildingHillRestrictions()
        {
            return _worldDefaults != null
                && _worldDefaults.BlockedBuildingHillLevelRanges != null
                && _worldDefaults.BlockedBuildingHillLevelRanges.Count > 0;
        }

        private bool IsBlockedBuildingTile(string tileTypeId)
        {
            if (string.IsNullOrWhiteSpace(tileTypeId))
                return false;

            var blockedTileIds = _worldDefaults?.BlockedBuildingTileIds;
            if (blockedTileIds != null && blockedTileIds.Count > 0)
            {
                for (int i = 0; i < blockedTileIds.Count; i++)
                {
                    string blockedId = blockedTileIds[i];
                    if (string.IsNullOrWhiteSpace(blockedId))
                        continue;

                    if (string.Equals(blockedId.Trim(), tileTypeId, StringComparison.OrdinalIgnoreCase))
                        return true;
                }

                return false;
            }

            return false;
        }

        private static bool IsTerrainLevelBlocked(IReadOnlyList<TerrainLevelRestrictionRange> ranges, int terrainLevel)
        {
            if (ranges == null || ranges.Count == 0)
                return false;

            for (int i = 0; i < ranges.Count; i++)
            {
                var range = ranges[i];
                if (range == null)
                    continue;

                int min = Mathf.Max(1, range.MinLevel);
                int max = Mathf.Max(1, range.MaxLevel);
                if (max < min)
                {
                    int swap = min;
                    min = max;
                    max = swap;
                }

                if (terrainLevel >= min && terrainLevel <= max)
                    return true;
            }

            return false;
        }

        private bool CanPlaceAt(
            Vector2Int position,
            Vector2Int? ignoredPendingPosition,
            string buildingId,
            out bool tileOccupied,
            out bool spacingBlocked,
            out bool fogBlocked,
            out bool influenceZoneBlocked,
            out bool terrainBlocked)
        {
            if (VerboseLogs)
                Debug.Log($"[Construction] CanPlaceAt({position}, buildingId={buildingId}) проверка ПОЧАЛАСЬ");

            try
            {
                var result = BuildingPlacementEvaluator.Evaluate(new BuildingPlacementEvaluationRequest
                {
                    BuildingRegistry = _buildingRegistry,
                    BuildingId = buildingId,
                    Position = position,
                    IgnoredPendingPosition = ignoredPendingPosition,
                    MinSpacing = _minSpacing,
                    TownHallBuildRadius = _townHallBuildRadius,
                    IsOccupied = _objectsMapService.IsOccupied,
                    GetOccupantId = GetObjectOccupantId,
                    IsFogBlocked = IsBlockedByFog,
                    PendingPlacements = BuildPlacementSimulationEntries(),
                });

                tileOccupied = result.TileOccupied;
                if (VerboseLogs)
                    Debug.Log($"[Construction] CanPlaceAt({position}): tileOccupied={tileOccupied}");

                spacingBlocked = result.SpacingBlocked;
                if (VerboseLogs)
                    Debug.Log($"[Construction] CanPlaceAt({position}): spacingBlocked={spacingBlocked}");

                fogBlocked = result.FogBlocked;
                if (VerboseLogs)
                    Debug.Log($"[Construction] CanPlaceAt({position}): fogBlocked={fogBlocked}");

                influenceZoneBlocked = result.InfluenceZoneBlocked;

                if (VerboseLogs)
                    Debug.Log($"[Construction] CanPlaceAt({position}): influenceZoneBlocked={influenceZoneBlocked}");

                terrainBlocked = IsBlockedByTerrain(position, out var terrainReason);

                if (VerboseLogs)
                    Debug.Log($"[Construction] CanPlaceAt({position}): terrainBlocked={terrainBlocked}, terrainReason={terrainReason}");

                bool allowed = result.IsValid && !terrainBlocked;
                
                if (VerboseLogs)
                    Debug.Log($"[Construction] CanPlaceAt({position}) результат: {(allowed ? "✓ VALID" : "❌ BLOCKED")}");

                return allowed;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Construction] ПОМИЛКА в CanPlaceAt({position}, {buildingId}): {ex.GetType().Name} - {ex.Message}");
                tileOccupied = false;
                spacingBlocked = false;
                fogBlocked = false;
                influenceZoneBlocked = false;
                terrainBlocked = false;
                return false;
            }
        }

        private string GetObjectOccupantId(Vector2Int position)
        {
            return _objectsMapService.TryGetOccupant(position, out var occupantId)
                ? occupantId
                : null;
        }

        private List<BuildingPlacementSimulationEntry> BuildPlacementSimulationEntries()
        {
            var entries = new List<BuildingPlacementSimulationEntry>(_pendingPlacements.Count);
            for (int index = 0; index < _pendingPlacements.Count; index++)
            {
                var placement = _pendingPlacements[index];
                entries.Add(new BuildingPlacementSimulationEntry(placement.Position, placement.BuildingId));
            }

            return entries;
        }

        /// <summary>
        /// Правило поселення: будь-яку не-центральну будівлю можна ставити лише в зоні дії ратуші або замку.
        /// Якщо в реєстрі взагалі немає центральних будівель — правило ігнорується.
        /// </summary>
        private bool IsBlockedByInfluenceZone(Vector2Int position, string buildingId, Vector2Int? ignoredPendingPosition)
        {
            // Базові перевірки
            if (string.IsNullOrWhiteSpace(buildingId))
            {
                Debug.LogWarning("[Construction] IsBlockedByInfluenceZone: buildingId порожній");
                return false;
            }

            if (_buildingRegistry == null)
            {
                Debug.LogError("[Construction] IsBlockedByInfluenceZone: _buildingRegistry == null");
                return false;
            }

            var candidate = _buildingRegistry.GetById(buildingId);
            if (candidate == null)
            {
                Debug.LogWarning($"[Construction] IsBlockedByInfluenceZone: будівля '{buildingId}' не знайдена у реєстрі");
                return false;
            }

            bool anyInfluenceCenterDefined = System.Array.Exists(
                _buildingRegistry.GetAll(),
                IsInfluenceCenter);
            
            if (!anyInfluenceCenterDefined)
            {
                if (VerboseLogs)
                    Debug.Log("[Construction] IsBlockedByInfluenceZone: RuleDisabled - немає ратуші або замку у реєстрі");
                return false;
            }

            int ruleRadius = IsInfluenceCenter(candidate)
                ? ResolveInfluenceRadius(candidate)
                : ResolveMaxInfluenceRadius();

            if (ruleRadius <= 0)
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] IsBlockedByInfluenceZone: ruleRadius <= 0 ({ruleRadius}) - правило відключено");
                return false;
            }

            bool hasInfluenceCenterInRange = HasInfluenceCenterCoveringPosition(position, candidate, ignoredPendingPosition);
            
            if (VerboseLogs)
                Debug.Log($"[Construction] IsBlockedByInfluenceZone({position}, {buildingId}): ruleRadius={ruleRadius}, hasInfluenceCenterInRange={hasInfluenceCenterInRange}");

            bool requireInfluenceCenterInRange;
            bool blockWhenInfluenceCenterExists;

            if (candidate.UseCustomTownHallRules)
            {
                requireInfluenceCenterInRange = candidate.RequireTownHallInRange;
                blockWhenInfluenceCenterExists = candidate.BlockIfTownHallAlreadyInRange;
                
                if (VerboseLogs)
                    Debug.Log($"[Construction] IsBlockedByInfluenceZone: CustomRules - require={requireInfluenceCenterInRange}, blockWhenExists={blockWhenInfluenceCenterExists}");
            }
            else
            {
                // Базова логіка за типом будівлі.
                bool isInfluenceCenter = IsInfluenceCenter(candidate);
                requireInfluenceCenterInRange = !isInfluenceCenter;
                blockWhenInfluenceCenterExists = isInfluenceCenter;
                
                if (VerboseLogs)
                    Debug.Log($"[Construction] IsBlockedByInfluenceZone: DefaultRules - isInfluenceCenter={isInfluenceCenter}, require={requireInfluenceCenterInRange}, blockWhenExists={blockWhenInfluenceCenterExists}");
            }

            if (requireInfluenceCenterInRange && !hasInfluenceCenterInRange)
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] IsBlockedByInfluenceZone: BLOCKED - потрібна ратуша або замок у радіусі {ruleRadius}");
                return true;
            }

            int candidateInfluenceRadius = ResolveInfluenceRadius(candidate);
            if (blockWhenInfluenceCenterExists && HasOverlappingInfluenceCenter(position, candidateInfluenceRadius, ignoredPendingPosition, out var overlapPosition, out var overlapBuildingId, out var overlapRadius))
            {
                if (VerboseLogs)
                    Debug.Log($"[Construction] IsBlockedByInfluenceZone: BLOCKED - зона '{buildingId}' radius={candidateInfluenceRadius} перетинається з '{overlapBuildingId}' на {overlapPosition} radius={overlapRadius}");
                return true;
            }

            if (VerboseLogs)
                Debug.Log("[Construction] IsBlockedByInfluenceZone: ALLOWED");

            return false;
        }

        private bool HasInfluenceCenterCoveringPosition(Vector2Int position, BuildingDefinition candidate, Vector2Int? ignoredPendingPosition)
        {
            int candidateLimit = ResolveCandidateProximityLimit(candidate);

            // 1) Вже зайняті тайли (будь-які джерела: placed, restored, world bootstrap)
            if (HasPlacedInfluenceCenter(position, ignoredPendingPosition, candidateLimit))
                return true;

            // 2) Центральна будівля в поточному pending-сеті (дозволяє в одній сесії будувати пачкою)
            for (int i = 0; i < _pendingPlacements.Count; i++)
            {
                var pending = _pendingPlacements[i];
                if (pending.Position == ignoredPendingPosition)
                    continue;

                var pendingDef = _buildingRegistry.GetById(pending.BuildingId);
                if (!IsInfluenceCenter(pendingDef))
                    continue;

                int allowedRadius = ResolveCoverageRadius(pendingDef, candidateLimit);
                if (allowedRadius <= 0)
                    continue;

                if (GetChebyshevDistance(pending.Position, position) <= allowedRadius)
                    return true;
            }

            return false;
        }

        private bool HasPlacedInfluenceCenter(Vector2Int position, Vector2Int? ignoredPendingPosition, int candidateLimit)
        {
            int searchRadius = ResolvePlacedCenterSearchRadius(candidateLimit);
            if (searchRadius <= 0)
                return false;

            for (int dx = -searchRadius; dx <= searchRadius; dx++)
            {
                for (int dy = -searchRadius; dy <= searchRadius; dy++)
                {
                    var centerPosition = new Vector2Int(position.x + dx, position.y + dy);
                    if (centerPosition == ignoredPendingPosition)
                        continue;

                    if (!_objectsMapService.TryGetOccupant(centerPosition, out var occupantId) || string.IsNullOrWhiteSpace(occupantId))
                        continue;

                    var definition = _buildingRegistry.GetById(occupantId);
                    if (!IsInfluenceCenter(definition))
                        continue;

                    int allowedRadius = ResolveCoverageRadius(definition, candidateLimit);
                    if (allowedRadius > 0 && GetChebyshevDistance(centerPosition, position) <= allowedRadius)
                        return true;
                }
            }

            return false;
        }

        private bool HasOverlappingInfluenceCenter(
            Vector2Int candidatePosition,
            int candidateRadius,
            Vector2Int? ignoredPendingPosition,
            out Vector2Int overlappingPosition,
            out string overlappingBuildingId,
            out int overlappingRadius)
        {
            overlappingPosition = default;
            overlappingBuildingId = null;
            overlappingRadius = 0;

            if (candidateRadius <= 0)
                return false;

            int searchRadius = candidateRadius + ResolveMaxInfluenceRadius();
            for (int dx = -searchRadius; dx <= searchRadius; dx++)
            {
                for (int dy = -searchRadius; dy <= searchRadius; dy++)
                {
                    var centerPosition = new Vector2Int(candidatePosition.x + dx, candidatePosition.y + dy);
                    if (centerPosition == ignoredPendingPosition)
                        continue;

                    if (!_objectsMapService.TryGetOccupant(centerPosition, out var occupantId) || string.IsNullOrWhiteSpace(occupantId))
                        continue;

                    var definition = _buildingRegistry.GetById(occupantId);
                    if (!IsInfluenceCenter(definition))
                        continue;

                    int existingRadius = ResolveInfluenceRadius(definition);
                    if (existingRadius <= 0)
                        continue;

                    if (GetChebyshevDistance(centerPosition, candidatePosition) > candidateRadius + existingRadius)
                        continue;

                    overlappingPosition = centerPosition;
                    overlappingBuildingId = occupantId;
                    overlappingRadius = existingRadius;
                    return true;
                }
            }

            for (int i = 0; i < _pendingPlacements.Count; i++)
            {
                var pending = _pendingPlacements[i];
                if (pending.Position == ignoredPendingPosition)
                    continue;

                var pendingDef = _buildingRegistry.GetById(pending.BuildingId);
                if (!IsInfluenceCenter(pendingDef))
                    continue;

                int existingRadius = ResolveInfluenceRadius(pendingDef);
                if (existingRadius <= 0)
                    continue;

                if (GetChebyshevDistance(pending.Position, candidatePosition) <= candidateRadius + existingRadius)
                {
                    overlappingPosition = pending.Position;
                    overlappingBuildingId = pending.BuildingId;
                    overlappingRadius = existingRadius;
                    return true;
                }
            }

            return false;
        }

        private int ResolveCoverageRadius(BuildingDefinition centerDefinition, int candidateLimit)
        {
            int sourceRadius = ResolveInfluenceRadius(centerDefinition);
            if (sourceRadius <= 0)
                return 0;

            return candidateLimit > 0
                ? Mathf.Min(sourceRadius, candidateLimit)
                : sourceRadius;
        }

        private int ResolvePlacedCenterSearchRadius(int candidateLimit)
        {
            int maxRadius = ResolveMaxInfluenceRadius();
            return candidateLimit > 0
                ? Mathf.Min(maxRadius, candidateLimit)
                : maxRadius;
        }

        private int ResolveMaxInfluenceRadius()
        {
            int maxRadius = _townHallBuildRadius;
            var definitions = _buildingRegistry.GetAll();
            for (int i = 0; i < definitions.Length; i++)
            {
                var definition = definitions[i];
                if (!IsInfluenceCenter(definition))
                    continue;

                maxRadius = Mathf.Max(maxRadius, ResolveInfluenceRadius(definition));
            }

            return Mathf.Max(0, maxRadius);
        }

        private int ResolveCandidateProximityLimit(BuildingDefinition candidate)
        {
            return candidate != null && candidate.TownHallProximityRadiusOverride > 0
                ? candidate.TownHallProximityRadiusOverride
                : 0;
        }

        private int ResolveInfluenceRadius(BuildingDefinition definition)
        {
            return BuildingDefinitionCapabilities.GetInfluenceRadius(definition, _townHallBuildRadius);
        }

        private static int GetChebyshevDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
        }

        private static bool IsInfluenceCenter(BuildingDefinition definition)
        {
            return BuildingDefinitionCapabilities.IsTownHall(definition)
                || BuildingDefinitionCapabilities.IsCastle(definition);
        }

        private bool AddPendingPlacement(Vector2Int position, string buildingId, bool clearRedoHistory)
        {
            // Базові перевірки
            if (string.IsNullOrWhiteSpace(buildingId))
            {
                Debug.LogError($"[Construction] AddPendingPlacement: buildingId порожній на позиції {position}");
                return false;
            }

            if (_signalBus == null)
            {
                Debug.LogError("[Construction] AddPendingPlacement: _signalBus == null");
                return false;
            }

            if (_pendingPlacements == null || _pendingPositions == null)
            {
                Debug.LogError("[Construction] AddPendingPlacement: _pendingPlacements або _pendingPositions == null");
                return false;
            }

            try
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
                    Debug.Log($"[Construction] ✓ Pending placement додана для '{buildingId}' at {position}. pendingCount={_pendingPlacements.Count}, undoCount={_undoSnapshots.Count}, redoCount={_redoSnapshots.Count}");

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Construction] ПОМИЛКА в AddPendingPlacement({position}, {buildingId}): {ex.GetType().Name} - {ex.Message}");
                return false;
            }
        }

        private int FindPendingPlacementIndex(Vector2Int position)
        {
            try
            {
                if (_pendingPlacements == null)
                {
                    Debug.LogError("[Construction] FindPendingPlacementIndex: _pendingPlacements == null");
                    return -1;
                }

                for (int i = 0; i < _pendingPlacements.Count; i++)
                {
                    if (_pendingPlacements[i].Position == position)
                    {
                        if (VerboseLogs)
                            Debug.Log($"[Construction] FindPendingPlacementIndex({position}): знайдена на індексі {i}");
                        return i;
                    }
                }

                if (VerboseLogs)
                    Debug.Log($"[Construction] FindPendingPlacementIndex({position}): не знайдена (повертаю -1)");

                return -1;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Construction] ПОМИЛКА в FindPendingPlacementIndex({position}): {ex.GetType().Name} - {ex.Message}");
                return -1;
            }
        }

        private bool TryReplacePendingWallWithGate(Vector2Int position, string gateBuildingId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(gateBuildingId))
                {
                    if (VerboseLogs)
                        Debug.Log("[Construction] TryReplacePendingWallWithGate: gateBuildingId порожній");
                    return false;
                }

                if (_wallPlacementService == null)
                {
                    if (VerboseLogs)
                        Debug.Log("[Construction] TryReplacePendingWallWithGate: _wallPlacementService == null");
                    return false;
                }

                if (!_wallPlacementService.IsGate(gateBuildingId))
                {
                    if (VerboseLogs)
                        Debug.Log($"[Construction] TryReplacePendingWallWithGate: '{gateBuildingId}' не є воротами");
                    return false;
                }

                int index = FindPendingPlacementIndex(position);
                if (index < 0)
                {
                    if (VerboseLogs)
                        Debug.Log($"[Construction] TryReplacePendingWallWithGate({position}): pending placement не знайдена");
                    return false;
                }

                var current = _pendingPlacements[index];
                if (!_wallPlacementService.CanReplaceWallWithGate(position, gateBuildingId, out _))
                {
                    if (VerboseLogs)
                        Debug.Log($"[Construction] TryReplacePendingWallWithGate({position}): заміна недозволена");
                    return false;
                }

                if (current.BuildingId == gateBuildingId)
                {
                    if (VerboseLogs)
                        Debug.Log($"[Construction] TryReplacePendingWallWithGate({position}): вже ворота '{gateBuildingId}'");
                    return true;
                }

                if (!TryValidateConstructionResources(position, gateBuildingId, _activeOwnerId, position, out var resourceReason))
                {
                    _lastActionMessage = resourceReason;
                    if (VerboseLogs)
                        Debug.Log($"[Construction] TryReplacePendingWallWithGate({position}) -> BLOCKED. resourcesBlocked=True, reason={resourceReason}");
                    return false;
                }

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
                    Debug.Log($"[Construction] ✓ Pending wall at {position} replaced with gate '{gateBuildingId}'.");

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Construction] ПОМИЛКА в TryReplacePendingWallWithGate({position}, {gateBuildingId}): {ex.GetType().Name} - {ex.Message}");
                return false;
            }
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
