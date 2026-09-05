using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
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
        IConstructionConfirmationCommands,
        IConstructionRotationService,
        IConstructionBootstrapQuery,
        IConstructionBuildingOwnershipQuery,
        IConstructionPortfolioQuery,
        IConstructionSaveSnapshotSource,
        IConstructionSaveRestorer,
        IConstructionPlacedBuildingDestruction,
        IInitializable,
        IDisposable
    {
        private const string DefaultOwnerId = "player_0";

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
            [InjectOptional] IConstructionPlacementAuthorityPolicy
                placementAuthorityPolicy = null,
            [InjectOptional] List<IBuildingPlacementRuleEvaluator>
                placementRuleEvaluators = null,
            [InjectOptional] ITurnService turns = null,
            [InjectOptional] FogOfWarSettings fogSettings = null)
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
                    placementRulesProvider);
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
                    placementRulesProvider);
            _buildingFogEffects =
                new ConstructionBuildingFogEffects(
                    fogOfWarService,
                    buildingRegistry,
                    fogSettings);
        }

        private bool CanActiveOwnerAct(out string reason)
            => CanActiveOwnerMutate(
                "construction mutation",
                out reason);

        public void Initialize()
        {
            if (_disposed || _initialized)
                return;

            try
            {
                if (_signalBus == null)
                {
                    Debug.LogError("[Construction] Initialize: _signalBus == null");
                    return;
                }

                _signalBus.Subscribe<GameModeChangedSignal>(OnGameModeChanged);
                _signalBus.Subscribe<SettlementResourceChangedSignal>(OnSettlementResourceChanged);
                _signalBus.Subscribe<BuildingOperationalSignal>(OnBuildingOperational);
                BuildingDefinitionAsset.RuntimeRevisionChanged +=
                    OnBuildingDefinitionRuntimeRevisionChanged;
                _initialized = true;
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

            try
            {
                if (_signalBus == null)
                {
                    return;
                }

                _signalBus.TryUnsubscribe<GameModeChangedSignal>(OnGameModeChanged);
                _signalBus.TryUnsubscribe<SettlementResourceChangedSignal>(OnSettlementResourceChanged);
                _signalBus.TryUnsubscribe<BuildingOperationalSignal>(OnBuildingOperational);
                BuildingDefinitionAsset.RuntimeRevisionChanged -=
                    OnBuildingDefinitionRuntimeRevisionChanged;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Construction] ПОМИЛКА в Dispose(): {ex.GetType().Name} - {ex.Message}");
            }
        }

        private void OnGameModeChanged(GameModeChangedSignal signal)
        {
            _isActive = signal.NewMode == GameModeType.Construction;

            if (_isActive)
            {
                ApplyBootstrapCastleSelectionIfNeeded();
                return;
            }

            IsDemolishMode = false;
            ResetSession(clearRedoHistory: true);
        }

        private void OnBuildingDefinitionRuntimeRevisionChanged(
            int _)
        {
            foreach (var pair in _playerPlacedBuildings)
            {
                _buildingFogEffects.Remove(pair.Key);
                _buildingFogEffects.ApplyOnPlaced(
                    pair.Value,
                    pair.Key);
            }

            foreach (var pair in _factionPlacedBuildings)
            {
                if (_playerPlacedBuildings.ContainsKey(pair.Key))
                    continue;

                _buildingFogEffects.Remove(pair.Key);
                _buildingFogEffects.ApplyOnPlaced(
                    pair.Value.BuildingId,
                    pair.Key);
            }

            InvalidatePlacementResourceValidationCache();
        }

        private void OnBuildingOperational(
            BuildingOperationalSignal signal)
        {
            try
            {
                _buildingFogEffects.ApplyOnOperational(
                    signal.BuildingId,
                    signal.Position);
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[Construction] ПОМИЛКА застосування радіуса зору для завершеної будівлі '{signal.BuildingId}' at {signal.Position}: {ex.GetType().Name} - {ex.Message}");
            }
        }

        private void OnSettlementResourceChanged(
            SettlementResourceChangedSignal signal)
        {
            InvalidatePlacementResourceValidationCache();
            RevalidateActiveSelectionAvailability("resource-change");
        }
    }
}
