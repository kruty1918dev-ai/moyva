using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Calendar.Core;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Economy.Runtime
{
    /// <summary>
    /// Головний фасад економічної системи.
    /// Підписується на Calendar (кожен хід) та Construction (розміщення/знесення будівель).
    /// Запускає <see cref="EconomyTickOrchestrator"/> для кожного поселення кожного ходу.
    ///
    /// Використання: додати <see cref="EconomyInstaller"/> в сцену — все інше автоматично.
    /// </summary>
    public sealed class EconomyManager : IInitializable, IDisposable
    {
        public const string DefaultOwnerId = "player_0";
        private const string StarterPackLogTag = "[Bootstrap][StarterPack]";

        private readonly ICalendarService _calendar;
        private readonly SignalBus _signalBus;
        private readonly EconomyDatabaseSO _database;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IEconomyOwnerResourcePoolService _ownerResourcePoolService;
        private readonly ISettlementRegistry _settlementRegistry;
        private readonly IEconomyBuildingIntegration _buildingIntegration;
        private readonly IEconomyTurnProcessor _turnProcessor;
        private EconomyRuntimeSaveSnapshot
            _pendingRuntimeSaveSnapshot;

        private EconomyRulesConfigSO Rules => _database?.RulesConfig;

        // Keeps direct construction in tests/backward-compatible call sites.
        public EconomyManager(
            ICalendarService calendar,
            SignalBus signalBus,
            EconomyDatabaseSO database,
            IBuildingRegistry buildingRegistry)
            : this(calendar, signalBus, database, buildingRegistry, null, null, null, null)
        {
        }

        [Inject]
        internal EconomyManager(
            ICalendarService calendar,
            SignalBus signalBus,
            EconomyDatabaseSO database,
            IBuildingRegistry buildingRegistry,
            [InjectOptional] IEconomyOwnerResourcePoolService ownerResourcePoolService = null,
            [InjectOptional] ISettlementRegistry settlementRegistry = null,
            [InjectOptional] IEconomyBuildingIntegration buildingIntegration = null,
            [InjectOptional] IEconomyTurnProcessor turnProcessor = null)
        {
            _calendar = calendar;
            _signalBus = signalBus;
            _database = database;
            _buildingRegistry = buildingRegistry;
            _ownerResourcePoolService = ownerResourcePoolService ?? new EconomyOwnerResourcePoolService();
            _settlementRegistry = settlementRegistry ?? new EconomySettlementRegistryService();
            _buildingIntegration = buildingIntegration ?? new EconomyBuildingIntegrationService();
            _turnProcessor = turnProcessor ?? new EconomyTurnProcessorService();
        }

        public IReadOnlyDictionary<string, EconomySettlementState> Settlements => _settlementRegistry.AllSettlements;

        // ───────────────────────── Lifecycle

        public void Initialize()
        {
            _calendar.OnHourChanged += OnTurnAdvanced;
            _signalBus.Subscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.Subscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
            _signalBus.Subscribe<GrantStarterPackResourcesSignal>(OnGrantStarterPackResources);
            BuildingDefinitionAsset.RuntimeRevisionChanged +=
                OnBuildingDefinitionRuntimeRevisionChanged;
        }

        public void Dispose()
        {
            _calendar.OnHourChanged -= OnTurnAdvanced;
            _signalBus.TryUnsubscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.TryUnsubscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
            _signalBus.TryUnsubscribe<GrantStarterPackResourcesSignal>(OnGrantStarterPackResources);
            BuildingDefinitionAsset.RuntimeRevisionChanged -=
                OnBuildingDefinitionRuntimeRevisionChanged;
        }

        // ───────────────────────── Turn Processing

        private void OnTurnAdvanced()
        {
            if (_database == null || Rules == null)
                return;

            float turnDurationSeconds = _calendar.Config.HoursPerTurn * 3600f;
            _turnProcessor.ProcessTurn(_settlementRegistry, _signalBus, _database, turnDurationSeconds);
        }

        // ───────────────────────── Construction Events

        private void OnBuildingPlaced(BuildingPlacedSignal signal)
        {
            _buildingIntegration.OnBuildingPlaced(
                signal,
                _settlementRegistry,
                _signalBus,
                _database,
                _buildingRegistry);

            var definition = string.IsNullOrWhiteSpace(signal.BuildingId)
                ? null
                : _buildingRegistry?.GetById(signal.BuildingId);

            if (definition != null && BuildingDefinitionCapabilities.IsWarehouse(definition))
                _ownerResourcePoolService.TransferOwnerResourcesToFirstWarehouse(signal.OwnerId, _settlementRegistry.AllSettlements, _signalBus, StarterPackLogTag);

            TryApplyPendingRuntimeSaveSnapshot();
        }

        private void OnBuildingDemolished(BuildingDemolishedSignal signal)
        {
            _buildingIntegration.OnBuildingDemolished(
                signal,
                _settlementRegistry,
                _signalBus,
                _database,
                _buildingRegistry);
        }

        private void OnGrantStarterPackResources(GrantStarterPackResourcesSignal signal)
        {
            if (signal.Entries == null || signal.Entries.Length == 0)
            {
                Debug.LogWarning($"{StarterPackLogTag} Economy received empty starter-pack payload for owner '{NormalizeOwnerId(signal.OwnerId)}'.");
                return;
            }

            string entriesDescription = DescribeStarterPackEntries(signal.Entries);

            if (string.IsNullOrWhiteSpace(signal.SettlementId))
            {
                string ownerId = NormalizeOwnerId(signal.OwnerId);
                Debug.Log($"{StarterPackLogTag} Economy applying starter-pack to owner pool: owner='{ownerId}', entries=[{entriesDescription}].");
                for (int index = 0; index < signal.Entries.Length; index++)
                {
                    var entry = signal.Entries[index];
                    if (string.IsNullOrWhiteSpace(entry.ResourceId) || entry.Amount <= 0f)
                        continue;

                    _ownerResourcePoolService.AddOwnerResource(ownerId, entry.ResourceId.Trim(), entry.Amount, _signalBus);
                }

                _ownerResourcePoolService.TransferOwnerResourcesToFirstWarehouse(ownerId, _settlementRegistry.AllSettlements, _signalBus, StarterPackLogTag);

                return;
            }

            var state = _settlementRegistry.GetSettlement(signal.SettlementId);
            if (state == null || !state.IsActive)
            {
                Debug.LogWarning($"{StarterPackLogTag} Economy cannot apply starter-pack: settlement '{signal.SettlementId}' is missing or inactive for owner '{NormalizeOwnerId(signal.OwnerId)}'. Entries=[{entriesDescription}].");
                return;
            }

            string ownerFromSignal = NormalizeOwnerId(signal.OwnerId);
            string ownerFromSettlement = NormalizeOwnerId(state.OwnerId);
            if (!string.Equals(ownerFromSignal, ownerFromSettlement, StringComparison.Ordinal))
            {
                Debug.LogWarning($"[Economy] Пропущено стартовий пакет: owner mismatch signal='{ownerFromSignal}', settlement='{ownerFromSettlement}'.");
                return;
            }

            Debug.Log($"{StarterPackLogTag} Economy applying starter-pack to settlement='{signal.SettlementId}', owner='{ownerFromSignal}', entries=[{entriesDescription}].");

            for (int index = 0; index < signal.Entries.Length; index++)
            {
                var entry = signal.Entries[index];
                if (string.IsNullOrWhiteSpace(entry.ResourceId) || entry.Amount <= 0f)
                    continue;

                AddResource(signal.SettlementId, entry.ResourceId.Trim(), entry.Amount);
            }
        }

        private static string DescribeStarterPackEntries(StarterPackResourceEntrySignal[] entries)
        {
            if (entries == null || entries.Length == 0)
                return "none";

            var parts = new List<string>(entries.Length);
            for (int index = 0; index < entries.Length; index++)
            {
                var entry = entries[index];
                if (string.IsNullOrWhiteSpace(entry.ResourceId) || entry.Amount <= 0f)
                    continue;

                parts.Add($"{entry.ResourceId.Trim()}={entry.Amount:0.##}");
            }

            return parts.Count == 0 ? "none" : string.Join(", ", parts);
        }

        private void OnBuildingDefinitionRuntimeRevisionChanged(
            int revision)
        {
            int refreshedBuildings = 0;
            int refreshedSettlements = 0;

            foreach (var settlementPair
                     in _settlementRegistry.AllSettlements)
            {
                EconomySettlementState state =
                    settlementPair.Value;
                if (state == null || !state.IsActive)
                    continue;

                int housingCapacity = 0;

                for (int index = 0;
                     index < state.Buildings.Count;
                     index++)
                {
                    EconomyBuildingState building =
                        state.Buildings[index];
                    if (building == null
                        || string.IsNullOrWhiteSpace(
                            building.BuildingId))
                    {
                        continue;
                    }

                    BuildingDefinition definition =
                        _buildingRegistry.GetById(
                            building.BuildingId);
                    if (definition == null)
                        continue;

                    building.RequiredWorkers =
                        BuildingDefinitionCapabilities
                            .GetRequiredWorkers(definition);
                    building.EconomyPriority =
                        BuildingDefinitionCapabilities
                            .GetEconomyPriority(definition);
                    building.WorkerTypeId =
                        BuildingDefinitionCapabilities
                            .GetWorkerTypeId(definition);

                    if (building.AssignedWorkers
                        > building.RequiredWorkers)
                    {
                        building.AssignedWorkers =
                            building.RequiredWorkers;
                    }

                    if (BuildingDefinitionCapabilities
                            .TryGetEnabledModule(
                                definition,
                                out ProductionBuildingModule production))
                    {
                        building.ProductionRecipes =
                            new List<ProductionRecipeDefinition>();

                        if (production.Recipes != null
                            && production.Recipes.Count > 0)
                        {
                            building.ProductionRecipes.AddRange(
                                production.Recipes);
                        }
                        else if (!string.IsNullOrWhiteSpace(
                                     production.ResourceId))
                        {
                            building.ProductionRecipes.Add(
                                new ProductionRecipeDefinition
                                {
                                    RecipeId =
                                        $"{building.BuildingId}:legacy",
                                    TurnsPerCycle = 1,
                                    RequiresWorkers =
                                        building.RequiredWorkers > 0,
                                    RequiresStorageSpace = false,
                                    Outputs =
                                        new List<BuildingResourceAmount>
                                        {
                                            new BuildingResourceAmount
                                            {
                                                ResourceId =
                                                    production.ResourceId.Trim(),
                                                Amount = 1,
                                            },
                                        },
                                });
                        }

                        building.ProductionProfileId =
                            building.BuildingId;
                    }
                    else
                    {
                        building.ProductionProfileId = null;
                        building.ProductionRecipes?.Clear();
                        building.RecipeProgress?.Clear();
                        building.ProductionProgress = 0f;
                    }

                    if (BuildingDefinitionCapabilities
                            .TryGetEnabledModule(
                                definition,
                                out HousingBuildingModule housing))
                    {
                        housingCapacity +=
                            Math.Max(0, housing.Capacity);
                    }

                    string warehouseKey =
                        ToWarehouseKey(building.GridPosition);
                    if (BuildingDefinitionCapabilities
                            .IsWarehouse(definition))
                    {
                        state.EnsureWarehousePool(warehouseKey);
                        state.ConfigureWarehousePolicy(
                            warehouseKey,
                            BuildingDefinitionCapabilities
                                .GetStorageCapacity(definition),
                            BuildingDefinitionCapabilities
                                .GetAcceptedStorageResourceIds(definition));
                    }
                    else if (state.WarehousePolicies.ContainsKey(
                                 warehouseKey))
                    {
                        state.RemoveWarehousePool(warehouseKey);
                    }

                    refreshedBuildings++;
                }

                state.TotalHousingCapacity =
                    housingCapacity;
                state.EnsureWarehouseConsistency();
                refreshedSettlements++;
            }

            Debug.Log(
                $"[MoyvaConstructionModules] live-refresh economy " +
                $"revision={revision} " +
                $"settlements={refreshedSettlements} " +
                $"buildings={refreshedBuildings}");
        }

        // ───────────────────────── Public API for UI / other systems

        /// <summary>Отримати стан поселення за ID.</summary>
        public EconomySettlementState GetSettlement(string settlementId)
        {
            return _settlementRegistry.GetSettlement(settlementId);
        }

        public bool TryGetSettlementByPosition(Vector2Int position, out EconomySettlementState state)
        {
            return _settlementRegistry.TryGetSettlementByPosition(position, out state);
        }

        public bool TryResolveConstructionSettlement(Vector2Int position, string ownerId, out EconomySettlementState state)
        {
            return _settlementRegistry.TryFindNearestSettlement(position, NormalizeOwnerId(ownerId), out state);
        }

        public bool TryConsumeSettlementResources(string settlementId, IReadOnlyDictionary<string, float> resourceCosts, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(settlementId))
            {
                errorMessage = "Не визначено поселення для списання ресурсів.";
                return false;
            }

            var state = _settlementRegistry.GetSettlement(settlementId);
            if (state == null || !state.IsActive)
            {
                errorMessage = $"Поселення '{settlementId}' недоступне або неактивне.";
                return false;
            }

            if (resourceCosts == null || resourceCosts.Count == 0)
                return true;

            foreach (var pair in resourceCosts)
            {
                if (string.IsNullOrWhiteSpace(pair.Key))
                {
                    errorMessage = "Спроба списати ресурс з порожнім ID.";
                    return false;
                }

                if (pair.Value <= 0f)
                    continue;

                float currentAmount = state.GetResource(pair.Key);
                if (currentAmount + 0.0001f < pair.Value)
                {
                    errorMessage = $"Недостатньо ресурсу '{ResolveResourceDisplayName(pair.Key)}' у поселенні '{GetSettlementNameOrFallback(settlementId)}': потрібно {pair.Value:0.#}, зараз {currentAmount:0.#}.";
                    return false;
                }
            }

            foreach (var pair in resourceCosts)
            {
                if (pair.Value <= 0f)
                    continue;

                float before = state.GetResource(pair.Key);
                if (!state.ConsumeResource(pair.Key, pair.Value))
                {
                    errorMessage = $"Не вдалося списати ресурс '{ResolveResourceDisplayName(pair.Key)}' у поселенні '{GetSettlementNameOrFallback(settlementId)}'.";
                    return false;
                }

                _signalBus.Fire(new SettlementResourceChangedSignal
                {
                    SettlementId = settlementId,
                    OwnerId = NormalizeOwnerId(state.OwnerId),
                    ResourceId = pair.Key,
                    NewAmount = state.GetResource(pair.Key),
                    Delta = state.GetResource(pair.Key) - before,
                });
            }

            return true;
        }

        public bool TryConsumeOwnerPoolResources(string ownerId, IReadOnlyDictionary<string, float> resourceCosts, out string errorMessage)
        {
            return _ownerResourcePoolService.TryConsumeOwnerPoolResources(
                ownerId,
                resourceCosts,
                ResolveResourceDisplayName,
                _signalBus,
                out errorMessage);
        }

        public bool TryGetBuildingAtPosition(Vector2Int position, out string buildingId, out string ownerId)
        {
            return _settlementRegistry.TryGetBuildingAtPosition(position, out buildingId, out ownerId);
        }

        public Dictionary<string, float> GetWarehouseResourceTotalsByPosition(Vector2Int warehousePosition)
        {
            if (!_settlementRegistry.TryGetSettlementByPosition(warehousePosition, out var state) || state == null)
                return new Dictionary<string, float>(StringComparer.Ordinal);

            return state.GetWarehouseSnapshot(ToWarehouseKey(warehousePosition));
        }

        public Dictionary<string, float> GetSettlementWarehousesTotal(string settlementId)
        {
            if (string.IsNullOrWhiteSpace(settlementId))
                return new Dictionary<string, float>(StringComparer.Ordinal);

            var state = _settlementRegistry.GetSettlement(settlementId);
            if (state == null)
                return new Dictionary<string, float>(StringComparer.Ordinal);

            return state.GetAllWarehousesTotalSnapshot();
        }

        public Dictionary<string, float> GetSettlementResourceTotals(string settlementId)
        {
            if (string.IsNullOrWhiteSpace(settlementId))
                return new Dictionary<string, float>(StringComparer.Ordinal);

            var state = _settlementRegistry.GetSettlement(settlementId);
            if (state == null)
                return new Dictionary<string, float>(StringComparer.Ordinal);

            return new Dictionary<string, float>(state.ResourcePool, StringComparer.Ordinal);
        }

        public Dictionary<string, float> GetOwnerPoolResourceTotals(string ownerId)
        {
            return _ownerResourcePoolService.GetOwnerPoolResourceTotals(ownerId);
        }

        public Dictionary<string, float> GetOwnerResourceTotals(string ownerId)
        {
            return _ownerResourcePoolService.GetOwnerResourceTotals(
                _settlementRegistry.AllSettlements,
                NormalizeOwnerId(ownerId));
        }

        public bool OwnerHasAnyWarehouse(string ownerId)
        {
            return _ownerResourcePoolService.OwnerHasAnyWarehouse(ownerId, _settlementRegistry.AllSettlements);
        }

        public Dictionary<string, Dictionary<string, float>> GetOwnerResourcePoolsSnapshot()
        {
            return _ownerResourcePoolService.GetOwnerResourcePoolsSnapshot();
        }

        public Dictionary<string, Dictionary<string, float>> GetOwnerResourceTotalsSnapshot()
        {
            return _ownerResourcePoolService.GetOwnerResourceTotalsSnapshot(_settlementRegistry.AllSettlements);
        }

        public void RestoreOwnerResourcePools(Dictionary<string, Dictionary<string, float>> snapshot)
        {
            _ownerResourcePoolService.RestoreOwnerResourcePools(snapshot, _signalBus);
            _ownerResourcePoolService.TransferOwnerResourcesToExistingWarehouses(_settlementRegistry.AllSettlements, _signalBus, StarterPackLogTag);
        }

        internal EconomyRuntimeSaveSnapshot
            CaptureRuntimeSaveSnapshot()
        {
            var result =
                new EconomyRuntimeSaveSnapshot();

            foreach (var settlementPair
                     in _settlementRegistry.AllSettlements)
            {
                EconomySettlementState state =
                    settlementPair.Value;
                if (state == null)
                    continue;

                var saved =
                    new EconomySettlementRuntimeSnapshot
                    {
                        SettlementId =
                            state.SettlementId,
                        CurrentTurn =
                            state.CurrentTurn,
                    };

                foreach (var resource
                         in state.ResourcePool)
                {
                    saved.ResourcePool[
                        resource.Key] = resource.Value;
                }

                foreach (var warehouse
                         in state.WarehouseResourcePools)
                {
                    var pool =
                        new Dictionary<string, float>(
                            StringComparer.Ordinal);
                    if (warehouse.Value != null)
                    {
                        foreach (var resource
                                 in warehouse.Value)
                        {
                            pool[resource.Key] =
                                resource.Value;
                        }
                    }
                    saved.Warehouses[
                        warehouse.Key] = pool;
                }

                foreach (var assignment
                         in state.WorkerAssignments)
                {
                    saved.WorkerAssignments[
                        assignment.Key] =
                        assignment.Value;
                }

                for (int buildingIndex = 0;
                     buildingIndex < state.Buildings.Count;
                     buildingIndex++)
                {
                    EconomyBuildingState building =
                        state.Buildings[buildingIndex];
                    if (building == null)
                        continue;

                    var savedBuilding =
                        new EconomyBuildingRuntimeSnapshot
                        {
                            InstanceKey =
                                building.InstanceKey,
                            BuildingId =
                                building.BuildingId,
                            GridPosition =
                                building.GridPosition,
                            AssignedWorkers =
                                building.AssignedWorkers,
                            ProductionProgress =
                                building.ProductionProgress,
                        };

                    if (building.RecipeProgress != null)
                    {
                        foreach (var progress
                                 in building.RecipeProgress)
                        {
                            savedBuilding.RecipeProgress[
                                progress.Key] =
                                progress.Value;
                        }
                    }

                    saved.Buildings.Add(
                        savedBuilding);
                }

                result.Settlements.Add(saved);
            }

            return result;
        }

        internal void RestoreRuntimeSaveSnapshot(
            EconomyRuntimeSaveSnapshot snapshot)
        {
            _pendingRuntimeSaveSnapshot = snapshot;
            TryApplyPendingRuntimeSaveSnapshot();
        }

        private void TryApplyPendingRuntimeSaveSnapshot()
        {
            if (_pendingRuntimeSaveSnapshot == null
                || _pendingRuntimeSaveSnapshot.Settlements.Count == 0)
            {
                return;
            }

            int applied = 0;
            int deferred = 0;

            for (int settlementIndex =
                     _pendingRuntimeSaveSnapshot
                         .Settlements.Count - 1;
                 settlementIndex >= 0;
                 settlementIndex--)
            {
                EconomySettlementRuntimeSnapshot saved =
                    _pendingRuntimeSaveSnapshot
                        .Settlements[settlementIndex];
                EconomySettlementState state =
                    _settlementRegistry.GetSettlement(
                        saved.SettlementId);

                if (state == null
                    || !HasAllSavedBuildingInstances(
                        state,
                        saved))
                {
                    deferred++;
                    continue;
                }

                ApplyRuntimeSnapshot(
                    state,
                    saved);
                _pendingRuntimeSaveSnapshot
                    .Settlements.RemoveAt(
                        settlementIndex);
                applied++;
            }

            if (_pendingRuntimeSaveSnapshot.Settlements.Count == 0)
                _pendingRuntimeSaveSnapshot = null;

            Debug.Log(
                $"[MoyvaConstructionModules] economy-runtime-restore " +
                $"applied={applied} deferred={deferred}");
        }

        private static bool HasAllSavedBuildingInstances(
            EconomySettlementState state,
            EconomySettlementRuntimeSnapshot saved)
        {
            for (int savedIndex = 0;
                 savedIndex < saved.Buildings.Count;
                 savedIndex++)
            {
                EconomyBuildingRuntimeSnapshot savedBuilding =
                    saved.Buildings[savedIndex];
                bool found = false;

                for (int currentIndex = 0;
                     currentIndex < state.Buildings.Count;
                     currentIndex++)
                {
                    if (IsSameBuildingInstance(
                            state.Buildings[currentIndex],
                            savedBuilding))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    return false;
            }

            return true;
        }

        private static void ApplyRuntimeSnapshot(
            EconomySettlementState state,
            EconomySettlementRuntimeSnapshot saved)
        {
            state.CurrentTurn = saved.CurrentTurn;

            state.ResourcePool.Clear();
            foreach (var resource in saved.ResourcePool)
                state.ResourcePool[resource.Key] = resource.Value;

            state.WarehouseResourcePools.Clear();
            foreach (var warehouse in saved.Warehouses)
            {
                state.WarehouseResourcePools[
                    warehouse.Key] =
                    new Dictionary<string, float>(
                        warehouse.Value,
                        StringComparer.Ordinal);
            }

            state.WorkerAssignments.Clear();
            foreach (var assignment
                     in saved.WorkerAssignments)
            {
                state.WorkerAssignments[
                    assignment.Key] =
                    assignment.Value;
            }

            for (int savedIndex = 0;
                 savedIndex < saved.Buildings.Count;
                 savedIndex++)
            {
                EconomyBuildingRuntimeSnapshot savedBuilding =
                    saved.Buildings[savedIndex];

                for (int currentIndex = 0;
                     currentIndex < state.Buildings.Count;
                     currentIndex++)
                {
                    EconomyBuildingState building =
                        state.Buildings[currentIndex];
                    if (!IsSameBuildingInstance(
                            building,
                            savedBuilding))
                    {
                        continue;
                    }

                    building.AssignedWorkers =
                        Math.Max(
                            0,
                            Math.Min(
                                building.RequiredWorkers,
                                savedBuilding.AssignedWorkers));
                    building.ProductionProgress =
                        Math.Max(
                            0f,
                            savedBuilding.ProductionProgress);

                    building.RecipeProgress?.Clear();
                    if (building.RecipeProgress != null)
                    {
                        foreach (var progress
                                 in savedBuilding.RecipeProgress)
                        {
                            building.RecipeProgress[
                                progress.Key] =
                                Math.Max(
                                    0f,
                                    progress.Value);
                        }
                    }

                    break;
                }
            }

            state.EnsureWarehouseConsistency();
        }

        private static bool IsSameBuildingInstance(
            EconomyBuildingState current,
            EconomyBuildingRuntimeSnapshot saved)
        {
            if (current == null || saved == null)
                return false;

            if (!string.IsNullOrWhiteSpace(
                    saved.InstanceKey)
                && string.Equals(
                    current.InstanceKey,
                    saved.InstanceKey,
                    StringComparison.Ordinal))
            {
                return true;
            }

            return string.Equals(
                       current.BuildingId,
                       saved.BuildingId,
                       StringComparison.Ordinal)
                   && current.GridPosition
                       == saved.GridPosition;
        }

        /// <summary>Додати ресурс до поселення вручну (караван, чіт, тестування).</summary>
        public void AddResource(string settlementId, string resourceId, float amount)
        {
            var state = _settlementRegistry.GetSettlement(settlementId);
            if (state == null)
                return;

            state.AddResource(resourceId, amount);

            _signalBus.Fire(new SettlementResourceChangedSignal
            {
                SettlementId = settlementId,
                OwnerId = NormalizeOwnerId(state.OwnerId),
                ResourceId = resourceId,
                NewAmount = state.GetResource(resourceId),
                Delta = amount,
            });
        }

        public string GetSettlementNameOrFallback(string settlementId)
        {
            return _settlementRegistry.GetSettlementNameOrFallback(settlementId);
        }

        private string ResolveResourceDisplayName(string resourceId)
        {
            string fallback = string.IsNullOrWhiteSpace(resourceId) ? string.Empty : resourceId.Trim();
            if (_database?.Resources == null || string.IsNullOrEmpty(fallback))
                return fallback;

            for (int i = 0; i < _database.Resources.Count; i++)
            {
                var resource = _database.Resources[i];
                if (resource == null || !string.Equals(resource.Id, fallback, StringComparison.Ordinal))
                    continue;

                return string.IsNullOrWhiteSpace(resource.DisplayName)
                    ? fallback
                    : resource.DisplayName;
            }

            return fallback;
        }

        private static string ToWarehouseKey(Vector2Int position)
        {
            return $"{position.x}:{position.y}";
        }

        private static string NormalizeOwnerId(string ownerId)
        {
            return string.IsNullOrWhiteSpace(ownerId) ? DefaultOwnerId : ownerId.Trim();
        }
    }
}
