using System;
using System.Collections.Generic;
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
    internal sealed partial class ConstructionService : IConstructionService, IInitializable, IDisposable
    {
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
        private readonly IFogOfWarService _fogOfWarService;
        private readonly IWallTopologyService _wallTopologyService;
        private readonly IWallGateReplacementValidator _wallGateReplacementValidator;
        private readonly IEconomyInfoMediator _economyInfoMediator;
        private readonly IGridService _gridService;
        private readonly IGeneratedTerrainLevelQuery _generatedTerrainLevelQuery;
        private readonly WorldCreationDefaultsSO _worldDefaults;
        private readonly ITileSettingsService _tileSettings;
        private readonly IConstructionPlacementRulesProvider _placementRulesProvider;
        private readonly IConstructionDiagnosticsSettingsProvider _diagnosticsSettingsProvider;
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
        private readonly Dictionary<Vector2Int, string> _playerPlacedBuildings = new();
        private string _activeOwnerId = DefaultOwnerId;
        private string _lastActionMessage = string.Empty;
        private readonly Dictionary<Vector2Int, (string BuildingId, string FactionId)> _factionPlacedBuildings = new();
        private bool _isActive;

        public BuildingPlacementState State { get; private set; } = BuildingPlacementState.Idle;
        public bool IsDemolishMode { get; private set; }
        private bool VerboseLogs => _diagnosticsSettingsProvider?.EnableVerboseLogs ?? (Application.isEditor && Debug.isDebugBuild);

        [Inject]
        public ConstructionService(
            IObjectsMapService objectsMapService,
            IBuildingRegistry buildingRegistry,
            SignalBus signalBus,
            [Inject(Id = "minSpacing")] int minSpacing,
            [Inject(Id = "townHallBuildRadius")] int townHallBuildRadius,
            [InjectOptional] IFogOfWarService fogOfWarService,
            [InjectOptional] IWallTopologyService wallTopologyService,
            [InjectOptional] IWallGateReplacementValidator wallGateReplacementValidator,
            [InjectOptional] IEconomyInfoMediator economyInfoMediator,
            [InjectOptional] IGridService gridService,
            [InjectOptional] IGeneratedTerrainLevelQuery generatedTerrainLevelQuery,
            [InjectOptional] WorldCreationDefaultsSO worldDefaults = null,
            [InjectOptional] ITileSettingsService tileSettings = null,
            [InjectOptional] IConstructionPlacementRulesProvider placementRulesProvider = null,
            [InjectOptional] IConstructionDiagnosticsSettingsProvider diagnosticsSettingsProvider = null,
            [InjectOptional] IConstructionDiagnostics diagnostics = null,
            [InjectOptional] IConstructionDiagnosticsSession diagnosticsSession = null)
        {
            _objectsMapService = objectsMapService;
            _buildingRegistry = buildingRegistry;
            _signalBus = signalBus;
            _minSpacing = minSpacing;
            _townHallBuildRadius = Mathf.Max(0, townHallBuildRadius);
            _fogOfWarService = fogOfWarService;
            _wallTopologyService = wallTopologyService;
            _wallGateReplacementValidator = wallGateReplacementValidator;
            _economyInfoMediator = economyInfoMediator;
            _gridService = gridService;
            _generatedTerrainLevelQuery = generatedTerrainLevelQuery;
            _worldDefaults = worldDefaults;
            _tileSettings = tileSettings;
            _placementRulesProvider = placementRulesProvider;
            _diagnosticsSettingsProvider = diagnosticsSettingsProvider;
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
            catch (Exception ex)
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
            catch (Exception ex)
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
    }
}
