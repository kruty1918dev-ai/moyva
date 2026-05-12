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

        private readonly ICalendarService _calendar;
        private readonly SignalBus _signalBus;
        private readonly EconomyDatabaseSO _database;
        private readonly IBuildingRegistry _buildingRegistry;

        private readonly EconomyTickOrchestrator _orchestrator = new EconomyTickOrchestrator();
        private readonly EconomySettlementLifecycleService _lifecycleService = new EconomySettlementLifecycleService();

        // Active settlement states keyed by settlementId
        private readonly Dictionary<string, EconomySettlementState> _settlements =
            new Dictionary<string, EconomySettlementState>(StringComparer.Ordinal);

        // Map of position → settlementId for quick lookup when building is placed
        private readonly Dictionary<Vector2Int, string> _positionToSettlement =
            new Dictionary<Vector2Int, string>();

        // Map of position → building id/owner for ownership checks and contextual UI.
        private readonly Dictionary<Vector2Int, string> _positionToBuildingId =
            new Dictionary<Vector2Int, string>();

        private readonly Dictionary<Vector2Int, string> _positionToOwnerId =
            new Dictionary<Vector2Int, string>();

        private EconomyRulesConfigSO Rules => _database?.RulesConfig;

        [Inject]
        public EconomyManager(
            ICalendarService calendar,
            SignalBus signalBus,
            EconomyDatabaseSO database,
            IBuildingRegistry buildingRegistry)
        {
            _calendar = calendar;
            _signalBus = signalBus;
            _database = database;
            _buildingRegistry = buildingRegistry;
        }

        public IReadOnlyDictionary<string, EconomySettlementState> Settlements => _settlements;

        // ───────────────────────── Lifecycle

        public void Initialize()
        {
            _calendar.OnHourChanged += OnTurnAdvanced;
            _signalBus.Subscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.Subscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
            _signalBus.Subscribe<GrantStarterPackResourcesSignal>(OnGrantStarterPackResources);
        }

        public void Dispose()
        {
            _calendar.OnHourChanged -= OnTurnAdvanced;
            _signalBus.TryUnsubscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.TryUnsubscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
            _signalBus.TryUnsubscribe<GrantStarterPackResourcesSignal>(OnGrantStarterPackResources);
        }

        // ───────────────────────── Turn Processing

        private void OnTurnAdvanced()
        {
            if (_database == null || Rules == null)
                return;

            float turnDurationSeconds = _calendar.Config.HoursPerTurn * 3600f;

            foreach (var kvp in _settlements)
            {
                var state = kvp.Value;
                if (!state.IsActive)
                    continue;

                // Run full economy tick
                var result = _orchestrator.Tick(state, _database, Rules, turnDurationSeconds);

                // Fire signal so UI and other systems can react
                _signalBus.Fire(new EconomyTickCompletedSignal
                {
                    SettlementId = state.SettlementId,
                    OwnerId = NormalizeOwnerId(state.OwnerId),
                    Turn = result.Turn,
                    TotalPopulation = result.TotalPopulation,
                    Arrivals = result.Arrivals,
                    Deaths = result.Deaths,
                    ProductionCyclesCompleted = result.ProductionCyclesCompleted,
                });

                // Check deactivation
                if (!state.IsActive)
                {
                    _signalBus.Fire(new SettlementDeactivatedSignal
                    {
                        SettlementId = state.SettlementId,
                        OwnerId = NormalizeOwnerId(state.OwnerId),
                        Reason = "Населення = 0",
                    });
                }

                // Check resource deficits
                CheckDeficits(state);
            }
        }

        // ───────────────────────── Construction Events

        private void OnBuildingPlaced(BuildingPlacedSignal signal)
        {
            var definition = FindBuildingDefinition(signal.BuildingId);
            if (definition == null)
                return;

            var validationIssues = BuildingModuleValidation.Validate(definition);
            if (BuildingModuleValidation.HasErrors(validationIssues))
            {
                Debug.LogError($"[Economy] Будівля '{signal.BuildingId}' містить невалідну модульну конфігурацію. Розміщення в економіці пропущено.");
                return;
            }

            var ownerId = NormalizeOwnerId(signal.OwnerId);

            // If this building is a TownHall or Castle, create a new settlement
            if (BuildingDefinitionCapabilities.IsTownHall(definition) ||
                BuildingDefinitionCapabilities.IsCastle(definition))
            {
                CreateSettlement(signal.BuildingId, signal.Position, definition, ownerId);
                return;
            }

            // Otherwise, assign building to nearest settlement of the same owner
            var settlementId = FindNearestSettlement(signal.Position, ownerId);
            if (settlementId == null)
            {
                Debug.LogWarning($"[Economy] Будівлю '{signal.BuildingId}' (owner='{ownerId}') розміщено за межами поселень цього власника.");
                return;
            }

            var state = _settlements[settlementId];
            AddBuildingToSettlement(state, signal.BuildingId, definition);
            _positionToSettlement[signal.Position] = settlementId;
            _positionToBuildingId[signal.Position] = signal.BuildingId;
            _positionToOwnerId[signal.Position] = ownerId;

            if (BuildingDefinitionCapabilities.IsWarehouse(definition))
                state.EnsureWarehousePool(ToWarehouseKey(signal.Position));

            // Update housing capacity
            if (BuildingDefinitionCapabilities.IsHousing(definition))
                RecalculateHousing(state);
        }

        private void OnBuildingDemolished(BuildingDemolishedSignal signal)
        {
            var definition = FindBuildingDefinition(signal.BuildingId);
            if (definition == null)
                return;

            // Find which settlement this building belongs to
            if (!_positionToSettlement.TryGetValue(signal.Position, out var settlementId))
                return;

            if (!_settlements.TryGetValue(settlementId, out var state))
                return;

            // Remove building from settlement
            for (int i = state.Buildings.Count - 1; i >= 0; i--)
            {
                if (state.Buildings[i].BuildingId == signal.BuildingId)
                {
                    state.Buildings.RemoveAt(i);
                    break;
                }
            }

            _positionToSettlement.Remove(signal.Position);
            _positionToBuildingId.Remove(signal.Position);
            _positionToOwnerId.Remove(signal.Position);

            if (BuildingDefinitionCapabilities.IsWarehouse(definition))
                state.RemoveWarehousePool(ToWarehouseKey(signal.Position));

            // TownHall destroyed → deactivate settlement
            if (BuildingDefinitionCapabilities.IsTownHall(definition))
            {
                state.IsActive = false;
                _signalBus.Fire(new SettlementDeactivatedSignal
                {
                    SettlementId = settlementId,
                    OwnerId = NormalizeOwnerId(state.OwnerId),
                    Reason = "Ратушу знищено",
                });
            }

            if (BuildingDefinitionCapabilities.IsHousing(definition))
                RecalculateHousing(state);
        }

        // ───────────────────────── Settlement Management

        private void CreateSettlement(string townHallBuildingId, Vector2Int position, BuildingDefinition definition, string ownerId)
        {
            var rules = Rules;
            if (rules == null)
                return;

            // Check settlement limit
            int activeCount = 0;
            foreach (var kvp in _settlements)
                if (kvp.Value.IsActive) activeCount++;

            if (activeCount >= rules.Settlement.MaxSettlements)
            {
                Debug.LogWarning($"[Economy] Ліміт поселень ({rules.Settlement.MaxSettlements}) досягнуто.");
                return;
            }

            // Check min distance to other town halls
            // (simplified — real implementation should check grid distance)

            var id = $"settlement-{_settlements.Count + 1}";
            var state = new EconomySettlementState
            {
                SettlementId = id,
                SettlementName = $"Settlement {_settlements.Count + 1}",
                OwnerId = ownerId,
                IsActive = true,
            };

            // Add town hall as a building
            AddBuildingToSettlement(state, townHallBuildingId, definition);

            // Start with initial population (2 residents)
            state.Residents.Add(new EconomyResidentState(age: 25, hp: 100f, comfort: 50f, houseCollapsed: false));
            state.Residents.Add(new EconomyResidentState(age: 22, hp: 100f, comfort: 50f, houseCollapsed: false));

            RecalculateHousing(state);

            _settlements[id] = state;
            _positionToSettlement[position] = id;
            _positionToBuildingId[position] = townHallBuildingId;
            _positionToOwnerId[position] = ownerId;

            _signalBus.Fire(new SettlementCreatedSignal
            {
                SettlementId = id,
                OwnerId = ownerId,
                TownHallPosition = position,
            });

            Debug.Log($"[Economy] Поселення '{id}' (owner='{ownerId}') створено на позиції {position}.");
        }

        private static void AddBuildingToSettlement(EconomySettlementState state, string buildingId, BuildingDefinition definition)
        {
            state.Buildings.Add(new EconomyBuildingState
            {
                BuildingId = buildingId,
                RequiredWorkers = BuildingDefinitionCapabilities.GetRequiredWorkers(definition),
                EconomyPriority = BuildingDefinitionCapabilities.GetEconomyPriority(definition),
                IsActive = true,
                ProductionProgress = 0f,
            });
        }

        private static string ToWarehouseKey(Vector2Int position)
        {
            return $"{position.x}:{position.y}";
        }

        private void RecalculateHousing(EconomySettlementState state)
        {
            int total = 0;
            for (int i = 0; i < state.Buildings.Count; i++)
            {
                var def = FindBuildingDefinition(state.Buildings[i].BuildingId);
                if (def != null && BuildingDefinitionCapabilities.IsHousing(def))
                    total += BuildingDefinitionCapabilities.GetHousingCapacity(def);
            }
            state.TotalHousingCapacity = total;
        }

        // ───────────────────────── Helpers

        private BuildingDefinition FindBuildingDefinition(string buildingId)
        {
            if (_buildingRegistry == null)
                return null;

            return _buildingRegistry.GetById(buildingId);
        }

        private string FindNearestSettlement(Vector2Int position, string ownerId)
        {
            // Simple approach: find closest town hall position
            string closest = null;
            float minDist = float.MaxValue;

            foreach (var kvp in _positionToSettlement)
            {
                if (!_settlements.TryGetValue(kvp.Value, out var state) || !state.IsActive)
                    continue;

                if (!string.Equals(state.OwnerId, ownerId, StringComparison.Ordinal))
                    continue;

                float dist = Vector2Int.Distance(kvp.Key, position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = kvp.Value;
                }
            }

            return closest;
        }

        private void CheckDeficits(EconomySettlementState state)
        {
            CheckSingleDeficit(state, "Food");
            CheckSingleDeficit(state, "Water");
            CheckSingleDeficit(state, "Firewood");
        }

        private void CheckSingleDeficit(EconomySettlementState state, string resourceId)
        {
            if (state.GetResource(resourceId) <= 0f && state.Residents.Count > 0)
            {
                _signalBus.Fire(new ResourceDeficitSignal
                {
                    SettlementId = state.SettlementId,
                    OwnerId = NormalizeOwnerId(state.OwnerId),
                    ResourceId = resourceId,
                });
            }
        }

        private static string NormalizeOwnerId(string ownerId)
        {
            return string.IsNullOrWhiteSpace(ownerId) ? DefaultOwnerId : ownerId.Trim();
        }

        private void OnGrantStarterPackResources(GrantStarterPackResourcesSignal signal)
        {
            if (string.IsNullOrWhiteSpace(signal.SettlementId) || signal.Entries == null || signal.Entries.Length == 0)
                return;

            if (!_settlements.TryGetValue(signal.SettlementId, out var state) || state == null || !state.IsActive)
                return;

            string ownerFromSignal = NormalizeOwnerId(signal.OwnerId);
            string ownerFromSettlement = NormalizeOwnerId(state.OwnerId);
            if (!string.Equals(ownerFromSignal, ownerFromSettlement, StringComparison.Ordinal))
            {
                Debug.LogWarning($"[Economy] Пропущено стартовий пакет: owner mismatch signal='{ownerFromSignal}', settlement='{ownerFromSettlement}'.");
                return;
            }

            for (int index = 0; index < signal.Entries.Length; index++)
            {
                var entry = signal.Entries[index];
                if (string.IsNullOrWhiteSpace(entry.ResourceId) || entry.Amount <= 0f)
                    continue;

                AddResource(signal.SettlementId, entry.ResourceId.Trim(), entry.Amount);
            }
        }

        // ───────────────────────── Public API for UI / other systems

        /// <summary>Отримати стан поселення за ID.</summary>
        public EconomySettlementState GetSettlement(string settlementId)
        {
            if (string.IsNullOrWhiteSpace(settlementId))
                return null;

            _settlements.TryGetValue(settlementId, out var state);
            return state;
        }

        public bool TryGetSettlementByPosition(Vector2Int position, out EconomySettlementState state)
        {
            state = null;
            if (!_positionToSettlement.TryGetValue(position, out var settlementId))
                return false;

            if (!_settlements.TryGetValue(settlementId, out state))
                return false;

            return state != null;
        }

        public bool TryResolveConstructionSettlement(Vector2Int position, string ownerId, out EconomySettlementState state)
        {
            state = null;
            var settlementId = FindNearestSettlement(position, NormalizeOwnerId(ownerId));
            if (string.IsNullOrWhiteSpace(settlementId))
                return false;

            if (!_settlements.TryGetValue(settlementId, out state) || state == null || !state.IsActive)
                return false;

            return string.Equals(NormalizeOwnerId(state.OwnerId), NormalizeOwnerId(ownerId), StringComparison.Ordinal);
        }

        public bool TryConsumeSettlementResources(string settlementId, IReadOnlyDictionary<string, float> resourceCosts, out string errorMessage)
        {
            errorMessage = null;

            if (string.IsNullOrWhiteSpace(settlementId))
            {
                errorMessage = "Не визначено поселення для списання ресурсів.";
                return false;
            }

            if (!_settlements.TryGetValue(settlementId, out var state) || state == null || !state.IsActive)
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
                    errorMessage = $"Недостатньо ресурсу '{pair.Key}' у поселенні '{GetSettlementNameOrFallback(settlementId)}': потрібно {pair.Value:0.#}, зараз {currentAmount:0.#}.";
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
                    errorMessage = $"Не вдалося списати ресурс '{pair.Key}' у поселенні '{GetSettlementNameOrFallback(settlementId)}'.";
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

        public bool TryGetBuildingAtPosition(Vector2Int position, out string buildingId, out string ownerId)
        {
            buildingId = null;
            ownerId = null;

            if (!_positionToBuildingId.TryGetValue(position, out buildingId) || string.IsNullOrWhiteSpace(buildingId))
                return false;

            _positionToOwnerId.TryGetValue(position, out ownerId);
            ownerId = NormalizeOwnerId(ownerId);
            return true;
        }

        public Dictionary<string, float> GetWarehouseResourceTotalsByPosition(Vector2Int warehousePosition)
        {
            if (!_positionToSettlement.TryGetValue(warehousePosition, out var settlementId))
                return new Dictionary<string, float>(StringComparer.Ordinal);

            if (!_settlements.TryGetValue(settlementId, out var state) || state == null)
                return new Dictionary<string, float>(StringComparer.Ordinal);

            return state.GetWarehouseSnapshot(ToWarehouseKey(warehousePosition));
        }

        public Dictionary<string, float> GetSettlementWarehousesTotal(string settlementId)
        {
            if (string.IsNullOrWhiteSpace(settlementId))
                return new Dictionary<string, float>(StringComparer.Ordinal);

            if (!_settlements.TryGetValue(settlementId, out var state) || state == null)
                return new Dictionary<string, float>(StringComparer.Ordinal);

            return state.GetAllWarehousesTotalSnapshot();
        }

        public Dictionary<string, float> GetSettlementResourceTotals(string settlementId)
        {
            if (string.IsNullOrWhiteSpace(settlementId))
                return new Dictionary<string, float>(StringComparer.Ordinal);

            if (!_settlements.TryGetValue(settlementId, out var state) || state == null)
                return new Dictionary<string, float>(StringComparer.Ordinal);

            return new Dictionary<string, float>(state.ResourcePool, StringComparer.Ordinal);
        }

        public Dictionary<string, float> GetOwnerResourceTotals(string ownerId)
        {
            var normalized = NormalizeOwnerId(ownerId);
            var totals = new Dictionary<string, float>(StringComparer.Ordinal);

            foreach (var settlement in _settlements.Values)
            {
                if (settlement == null)
                    continue;

                if (!string.Equals(NormalizeOwnerId(settlement.OwnerId), normalized, StringComparison.Ordinal))
                    continue;

                foreach (var resource in settlement.ResourcePool)
                {
                    if (totals.ContainsKey(resource.Key))
                        totals[resource.Key] += resource.Value;
                    else
                        totals[resource.Key] = resource.Value;
                }
            }

            return totals;
        }

        /// <summary>Додати ресурс до поселення вручну (караван, чіт, тестування).</summary>
        public void AddResource(string settlementId, string resourceId, float amount)
        {
            if (!_settlements.TryGetValue(settlementId, out var state))
                return;

            float before = state.GetResource(resourceId);
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
            if (string.IsNullOrWhiteSpace(settlementId))
                return "Без поселення";

            if (!_settlements.TryGetValue(settlementId, out var state) || state == null)
                return settlementId;

            if (!string.IsNullOrWhiteSpace(state.SettlementName))
                return state.SettlementName;

            return state.SettlementId;
        }
    }
}
