using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Diagnostics.API;
using Kruty1918.Moyva.Diagnostics.Runtime.Flows;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;
using Kruty1918.Moyva.Turns.API;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService :
        IConstructionService,
        IConfirmedConstructionPlacementApplier,
        IConfirmedConstructionPlacementIntentApplier,
        IConstructionPendingPlacementIntentSource,
        IAuthoritativeConstructionPlacementExecutor,
        IConstructionPlacementQuery,
        IConstructionSelectionAvailabilityQuery,
        IConstructionPendingUndoBatch,
        IConstructionRotationService,
        IConstructionBootstrapQuery,
        IConstructionBuildingOwnershipQuery,
        IConstructionSaveSnapshotSource,
        IConstructionSaveRestorer,
        IConstructionPlacedBuildingDestruction,
        IInitializable,
        IDisposable
    {
        private const string DefaultOwnerId = "player_0";
        private const string ModuleLogTag =
            "[MoyvaConstructionModules]";
        private const string PerfLogTag = "[MoyvaConstructionPerf]";

        private readonly struct PendingPlacement
        {
            public PendingPlacement(
                Vector2Int position,
                string buildingId,
                Vector2Int? originalPosition = null,
                string replacedPendingBuildingId = null,
                ConstructionRotation rotation =
                    ConstructionRotation.Degrees0)
            {
                Position = position;
                BuildingId = buildingId;
                OriginalPosition = originalPosition;
                ReplacedPendingBuildingId =
                    replacedPendingBuildingId;
                Rotation = ConstructionRotationUtility.Normalize(
                    (int)rotation);
            }

            public Vector2Int Position { get; }
            public string BuildingId { get; }
            public Vector2Int? OriginalPosition { get; }
            public string ReplacedPendingBuildingId { get; }
            public ConstructionRotation Rotation { get; }
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
        private readonly IBuildingRegistry _placementBuildingRegistry;
        private readonly SignalBus _signalBus;
        private readonly int _minSpacing;
        private readonly int _townHallBuildRadius;
        private readonly IFogOfWarService _fogOfWarService;
        private readonly IWallTopologyService _wallTopologyService;
        private readonly IWallGateReplacementValidator _wallGateReplacementValidator;
        private readonly IEconomyInfoMediator _economyInfoMediator;
        private readonly IGridService _gridService;
        private readonly IGeneratedTerrainLevelQuery _generatedTerrainLevelQuery;
        private readonly ITileSettingsService _tileSettings;
        private readonly IConstructionPlacementRulesProvider _placementRulesProvider;
        private readonly IConstructionDiagnosticsSettingsProvider _diagnosticsSettingsProvider;
        private readonly IConstructionDiagnostics _diagnostics;
        private readonly IConstructionDiagnosticsSession _diagnosticsSession;
        private readonly IConstructionPlacementAuthorityPolicy
            _placementAuthorityPolicy;
        private readonly IReadOnlyList<IBuildingPlacementRuleEvaluator>
            _placementRuleEvaluators;
        private readonly ITurnService _turns;
        private readonly ConstructionPlacementEnvironmentRules _placementEnvironmentRules;
        private readonly ConstructionFootprintStore _footprints;
        private readonly ConstructionReplacementPolicy _replacementPolicy;
        private readonly ConstructionInfluencePolicy _influencePolicy;
        private readonly ConstructionBuildingFogEffects _buildingFogEffects;
        private bool _initialized;
        private bool _disposed;

        private string _selectedBuildingId;
        private ConstructionRotation _selectedRotation;
        private readonly List<PendingPlacement> _pendingPlacements = new();
        private readonly List<BuildingPlacementSimulationEntry> _placementSimulationSnapshot = new();
        private readonly List<BuildingPlacementSimulationEntry> _placedBuildingSimulationSnapshot = new();
        private readonly HashSet<Vector2Int> _placementTileMatchWorkspace = new();
        private readonly List<List<PendingPlacement>> _undoSnapshots = new();
        private readonly List<List<PendingPlacement>> _redoSnapshots = new();
        private int _pendingUndoBatchDepth;
        private List<PendingPlacement> _pendingUndoBatchSnapshot;
        private bool _pendingUndoBatchChanged;
        private bool _pendingUndoBatchClearRedoHistory;
        private int _pendingUndoBatchStartCount;
        private string _pendingUndoBatchReason;
        private readonly HashSet<Vector2Int> _pendingPositions = new();
        private readonly Dictionary<Vector2Int, PendingPlacement> _pendingPlacementByPosition = new();
        private readonly Dictionary<Vector2Int, ConstructionPendingPlacementStatus> _pendingPlacementStatuses = new();
        private readonly List<PendingDemolition> _pendingDemolitions = new();
        private readonly HashSet<Vector2Int> _pendingDemolitionPositions = new();
        private readonly Dictionary<Vector2Int, string> _playerPlacedBuildings = new();
        private readonly Dictionary<Vector2Int, ConstructionRotation>
            _placedRotationByOrigin = new();
        private string _activeOwnerId = DefaultOwnerId;
        private string _lastActionMessage = string.Empty;
        private readonly Dictionary<Vector2Int, (string BuildingId, string FactionId)> _factionPlacedBuildings = new();
        private bool _isActive;
        private int _pendingPlacementsVersion;
        private int _placementSimulationSnapshotVersion = -1;
        private int _lastModuleAuditRevision = -1;

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
            [InjectOptional] ITileSettingsService tileSettings = null,
            [InjectOptional] IConstructionPlacementRulesProvider placementRulesProvider = null,
            [InjectOptional] IConstructionDiagnosticsSettingsProvider diagnosticsSettingsProvider = null,
            [InjectOptional] IConstructionDiagnostics diagnostics = null,
            [InjectOptional] IConstructionDiagnosticsSession diagnosticsSession = null,
            [InjectOptional] IConstructionPlacementAuthorityPolicy
                placementAuthorityPolicy = null,
            [InjectOptional] List<IBuildingPlacementRuleEvaluator>
                placementRuleEvaluators = null,
            [InjectOptional] ITurnService turns = null)
        {
            _objectsMapService = objectsMapService;
            _buildingRegistry = buildingRegistry;
            _placementBuildingRegistry = new ConstructionBuildingRegistrySnapshot(buildingRegistry);
            _signalBus = signalBus;
            _minSpacing = minSpacing;
            _townHallBuildRadius = Mathf.Max(0, townHallBuildRadius);
            _fogOfWarService = fogOfWarService;
            _wallTopologyService = wallTopologyService;
            _wallGateReplacementValidator = wallGateReplacementValidator;
            _economyInfoMediator = economyInfoMediator;
            _gridService = gridService;
            _generatedTerrainLevelQuery = generatedTerrainLevelQuery;
            _tileSettings = tileSettings;
            _placementRulesProvider = placementRulesProvider;
            _diagnosticsSettingsProvider = diagnosticsSettingsProvider;
            _diagnostics = diagnostics;
            _diagnosticsSession = diagnosticsSession;
            _placementAuthorityPolicy = placementAuthorityPolicy;
            _placementRuleEvaluators = placementRuleEvaluators
                ?? (IReadOnlyList<IBuildingPlacementRuleEvaluator>)
                    Array.Empty<IBuildingPlacementRuleEvaluator>();
            _turns = turns;
            _placementEnvironmentRules =
                new ConstructionPlacementEnvironmentRules(
                    fogOfWarService,
                    gridService,
                    generatedTerrainLevelQuery,
                    tileSettings,
                    placementRulesProvider,
                    () => VerboseLogs);
            _footprints =
                new ConstructionFootprintStore(
                    objectsMapService,
                    _placementBuildingRegistry,
                    gridService,
                    ResolvePlacedRotation);
            _replacementPolicy =
                new ConstructionReplacementPolicy(
                    _placementBuildingRegistry,
                    objectsMapService,
                    wallTopologyService,
                    wallGateReplacementValidator,
                    _footprints);
            _influencePolicy =
                new ConstructionInfluencePolicy(
                    buildingRegistry,
                    _townHallBuildRadius,
                    placementRulesProvider,
                    () => VerboseLogs);
            _buildingFogEffects =
                new ConstructionBuildingFogEffects(
                    fogOfWarService,
                    buildingRegistry);
        }

        private bool CanActiveOwnerAct(out string reason)
            => CanActiveOwnerMutate(
                "construction mutation",
                out reason);

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
                _signalBus.Subscribe<SettlementResourceChangedSignal>(OnSettlementResourceChanged);
                BuildingDefinitionAsset.RuntimeRevisionChanged +=
                    OnBuildingDefinitionRuntimeRevisionChanged;
                _initialized = true;
                AuditModuleRegistryIfNeeded(force: true);
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
                _signalBus.TryUnsubscribe<SettlementResourceChangedSignal>(OnSettlementResourceChanged);
                BuildingDefinitionAsset.RuntimeRevisionChanged -=
                    OnBuildingDefinitionRuntimeRevisionChanged;
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

            if (_isActive)
            {
                ApplyBootstrapCastleSelectionIfNeeded();
                return;
            }

            IsDemolishMode = false;
            ResetSession(clearRedoHistory: true);
        }

        private void AuditModuleRegistryIfNeeded(bool force = false)
        {
            int revision = BuildingDefinitionAsset.RuntimeRevision;
            if (!force && _lastModuleAuditRevision == revision)
                return;

            _lastModuleAuditRevision = revision;
            BuildingDefinition[] definitions =
                _placementBuildingRegistry?.GetAll()
                ?? Array.Empty<BuildingDefinition>();
            int buildings = 0;
            int modules = 0;
            int canonicalModules = 0;
            int legacyModules = 0;
            int errors = 0;
            int warnings = 0;

            for (int definitionIndex = 0;
                 definitionIndex < definitions.Length;
                 definitionIndex++)
            {
                BuildingDefinition definition =
                    definitions[definitionIndex];
                if (definition == null)
                    continue;

                buildings++;
                if (definition.Modules != null)
                {
                    for (int moduleIndex = 0;
                         moduleIndex < definition.Modules.Count;
                         moduleIndex++)
                    {
                        BuildingModuleDefinition module =
                            definition.Modules[moduleIndex];
                        if (module?.IsEnabled != true)
                            continue;

                        modules++;
                        if (BuildingDefinitionCapabilities
                                .IsLegacyCompatibilityModule(
                                    module.GetType()))
                        {
                            legacyModules++;
                        }
                        else
                        {
                            canonicalModules++;
                        }
                    }
                }

                IReadOnlyList<BuildingValidationIssue> issues =
                    GetModuleValidationIssuesCached(definition);
                for (int issueIndex = 0;
                     issueIndex < issues.Count;
                     issueIndex++)
                {
                    BuildingValidationIssue issue = issues[issueIndex];
                    if (issue == null)
                        continue;
                    if (issue.Severity == BuildingValidationSeverity.Error)
                        errors++;
                    else if (issue.Severity == BuildingValidationSeverity.Warning)
                        warnings++;
                }
            }

            Debug.Log(
                $"{ModuleLogTag} registry-audit " +
                $"schema={BuildingDefinitionCapabilities.ModuleSchemaVersion} " +
                $"revision={revision} " +
                $"buildings={buildings} enabledModules={modules} " +
                $"canonical={canonicalModules} legacy={legacyModules} " +
                $"errors={errors} warnings={warnings}");
        }

        private void OnBuildingDefinitionRuntimeRevisionChanged(
            int revision)
        {
            int refreshed = 0;

            foreach (var pair in _playerPlacedBuildings)
            {
                _fogOfWarService?.UnregisterUnit(
                    GetBuildingFogVisionAreaId(pair.Key));
                _buildingFogEffects.Apply(
                    pair.Value,
                    pair.Key);
                refreshed++;
            }

            foreach (var pair in _factionPlacedBuildings)
            {
                if (_playerPlacedBuildings.ContainsKey(pair.Key))
                    continue;

                _fogOfWarService?.UnregisterUnit(
                    GetBuildingFogVisionAreaId(pair.Key));
                _buildingFogEffects.Apply(
                    pair.Value.BuildingId,
                    pair.Key);
                refreshed++;
            }

            InvalidatePlacementResourceValidationCache();
            AuditModuleRegistryIfNeeded(force: true);

            Debug.Log(
                $"{ModuleLogTag} live-refresh construction " +
                $"revision={revision} placed={refreshed}");
        }

        private void OnSettlementResourceChanged(
            SettlementResourceChangedSignal signal)
        {
            InvalidatePlacementResourceValidationCache();

            if (!RevalidateActiveSelectionAvailability(
                    "resource-change"))
            {
                Debug.LogWarning(
                    $"[MoyvaConstructionAvailability] resource-invalidated-selection " +
                    $"owner='{signal.OwnerId}' resource='{signal.ResourceId}' " +
                    $"new={signal.NewAmount:0.###} delta={signal.Delta:0.###}");
            }
        }
    }
}
