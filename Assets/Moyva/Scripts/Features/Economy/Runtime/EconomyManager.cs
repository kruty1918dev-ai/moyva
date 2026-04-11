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
        }

        public void Dispose()
        {
            _calendar.OnHourChanged -= OnTurnAdvanced;
            _signalBus.TryUnsubscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signalBus.TryUnsubscribe<BuildingDemolishedSignal>(OnBuildingDemolished);
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

            // If this building is a TownHall, create a new settlement
            if (definition.IsTownHall)
            {
                CreateSettlement(signal.BuildingId, signal.Position, definition);
                return;
            }

            // Otherwise, assign building to nearest settlement
            var settlementId = FindNearestSettlement(signal.Position);
            if (settlementId == null)
            {
                Debug.LogWarning($"[Economy] Будівлю '{signal.BuildingId}' розміщено за межами поселень.");
                return;
            }

            var state = _settlements[settlementId];
            AddBuildingToSettlement(state, signal.BuildingId, definition);
            _positionToSettlement[signal.Position] = settlementId;

            // Update housing capacity
            if (definition.IsHousing)
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

            // TownHall destroyed → deactivate settlement
            if (definition.IsTownHall)
            {
                state.IsActive = false;
                _signalBus.Fire(new SettlementDeactivatedSignal
                {
                    SettlementId = settlementId,
                    Reason = "Ратушу знищено",
                });
            }

            if (definition.IsHousing)
                RecalculateHousing(state);
        }

        // ───────────────────────── Settlement Management

        private void CreateSettlement(string townHallBuildingId, Vector2Int position, BuildingDefinition definition)
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

            _signalBus.Fire(new SettlementCreatedSignal
            {
                SettlementId = id,
                TownHallPosition = position,
            });

            Debug.Log($"[Economy] Поселення '{id}' створено на позиції {position}.");
        }

        private static void AddBuildingToSettlement(EconomySettlementState state, string buildingId, BuildingDefinition definition)
        {
            state.Buildings.Add(new EconomyBuildingState
            {
                BuildingId = buildingId,
                RequiredWorkers = definition.RequiredWorkers,
                EconomyPriority = definition.EconomyPriority,
                IsActive = true,
                ProductionProgress = 0f,
            });
        }

        private void RecalculateHousing(EconomySettlementState state)
        {
            int total = 0;
            for (int i = 0; i < state.Buildings.Count; i++)
            {
                var def = FindBuildingDefinition(state.Buildings[i].BuildingId);
                if (def != null && def.IsHousing)
                    total += def.HousingCapacity;
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

        private string FindNearestSettlement(Vector2Int position)
        {
            // Simple approach: find closest town hall position
            string closest = null;
            float minDist = float.MaxValue;

            foreach (var kvp in _positionToSettlement)
            {
                if (!_settlements.TryGetValue(kvp.Value, out var state) || !state.IsActive)
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
                    ResourceId = resourceId,
                });
            }
        }

        // ───────────────────────── Public API for UI / other systems

        /// <summary>Отримати стан поселення за ID.</summary>
        public EconomySettlementState GetSettlement(string settlementId)
        {
            _settlements.TryGetValue(settlementId, out var state);
            return state;
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
                ResourceId = resourceId,
                NewAmount = state.GetResource(resourceId),
                Delta = amount,
            });
        }
    }
}
