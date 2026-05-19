using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Economy.Runtime
{
    internal sealed class EconomyBuildingIntegrationService : IEconomyBuildingIntegration
    {
        public EconomySettlementState OnBuildingPlaced(BuildingPlacedSignal signal, ISettlementRegistry registry, SignalBus signalBus, EconomyDatabaseSO database, IBuildingRegistry buildingRegistry)
        {
            var definition = FindBuildingDefinition(signal.BuildingId, buildingRegistry);
            if (definition == null)
                return null;

            var validationIssues = BuildingModuleValidation.Validate(definition);
            if (BuildingModuleValidation.HasErrors(validationIssues))
            {
                Debug.LogError($"[Economy] Будівля '{signal.BuildingId}' містить невалідну модульну конфігурацію. Розміщення в економіці пропущено.");
                return null;
            }

            var ownerId = NormalizeOwnerId(signal.OwnerId);

            // If this building is a TownHall or Castle, create a new settlement
            if (BuildingDefinitionCapabilities.IsTownHall(definition) ||
                BuildingDefinitionCapabilities.IsCastle(definition))
            {
                return CreateSettlement(signal.BuildingId, signal.Position, definition, ownerId, registry, signalBus, database);
            }

            // Otherwise, assign building to nearest settlement of the same owner
            if (!registry.TryFindNearestSettlement(signal.Position, ownerId, out var state))
            {
                Debug.LogWarning($"[Economy] Будівлю '{signal.BuildingId}' (owner='{ownerId}') розміщено за межами поселень цього власника.");
                return null;
            }

            AddBuildingToSettlement(state, signal.BuildingId, definition);
            registry.RegisterBuildingPosition(signal.Position, state.SettlementId, signal.BuildingId, ownerId);

            if (BuildingDefinitionCapabilities.IsWarehouse(definition))
            {
                state.EnsureWarehousePool(ToWarehouseKey(signal.Position));
                state.EnsureWarehouseConsistency();
            }

            // Update housing capacity
            if (BuildingDefinitionCapabilities.IsHousing(definition))
                RecalculateHousing(state, buildingRegistry);

            return null;
        }

        public void OnBuildingDemolished(BuildingDemolishedSignal signal, ISettlementRegistry registry, SignalBus signalBus, EconomyDatabaseSO database, IBuildingRegistry buildingRegistry)
        {
            var definition = FindBuildingDefinition(signal.BuildingId, buildingRegistry);
            if (definition == null)
                return;

            // Find which settlement this building belongs to
            if (!registry.TryGetSettlementByPosition(signal.Position, out var state))
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

            registry.UnregisterBuildingPosition(signal.Position);

            if (BuildingDefinitionCapabilities.IsWarehouse(definition))
                state.RemoveWarehousePool(ToWarehouseKey(signal.Position));

            // TownHall destroyed → deactivate settlement
            if (BuildingDefinitionCapabilities.IsTownHall(definition))
            {
                state.IsActive = false;
                signalBus.Fire(new SettlementDeactivatedSignal
                {
                    SettlementId = state.SettlementId,
                    OwnerId = NormalizeOwnerId(state.OwnerId),
                    Reason = "Ратушу знищено",
                });
            }

            if (BuildingDefinitionCapabilities.IsHousing(definition))
                RecalculateHousing(state, buildingRegistry);
        }

        private EconomySettlementState CreateSettlement(string townHallBuildingId, Vector2Int position, BuildingDefinition definition, string ownerId, ISettlementRegistry registry, SignalBus signalBus, EconomyDatabaseSO database)
        {
            var rules = database?.RulesConfig;
            if (rules == null)
            return null;

            // Check settlement limit
            int activeCount = 0;
            foreach (var kvp in registry.AllSettlements)
                if (kvp.Value.IsActive) activeCount++;

            if (activeCount >= rules.Settlement.MaxSettlements)
            {
                Debug.LogWarning($"[Economy] Ліміт поселень ({rules.Settlement.MaxSettlements}) досягнуто.");
                return null;
            }

            var id = $"settlement-{registry.AllSettlements.Count + 1}";
            var state = new EconomySettlementState
            {
                SettlementId = id,
                SettlementName = $"Settlement {registry.AllSettlements.Count + 1}",
                OwnerId = ownerId,
                IsActive = true,
            };

            // Add town hall as a building
            AddBuildingToSettlement(state, townHallBuildingId, definition);

            // Start with initial population (2 residents)
            state.Residents.Add(new EconomyResidentState(age: 25, hp: 100f, comfort: 50f, houseCollapsed: false));
            state.Residents.Add(new EconomyResidentState(age: 22, hp: 100f, comfort: 50f, houseCollapsed: false));

            RecalculateHousing(state, null);

            registry.RegisterSettlement(state, position);
            registry.RegisterBuildingPosition(position, id, townHallBuildingId, ownerId);

            signalBus.Fire(new SettlementCreatedSignal
            {
                SettlementId = id,
                OwnerId = ownerId,
                TownHallPosition = position,
            });

            Debug.Log($"[Economy] Поселення '{id}' (owner='{ownerId}') створено на позиції {position}.");
            return state;
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

        private static void RecalculateHousing(EconomySettlementState state, IBuildingRegistry buildingRegistry)
        {
            int total = 0;
            for (int i = 0; i < state.Buildings.Count; i++)
            {
                var def = buildingRegistry?.GetById(state.Buildings[i].BuildingId);
                if (def != null && BuildingDefinitionCapabilities.IsHousing(def))
                    total += BuildingDefinitionCapabilities.GetHousingCapacity(def);
            }
            state.TotalHousingCapacity = total;
        }

        private static BuildingDefinition FindBuildingDefinition(string buildingId, IBuildingRegistry buildingRegistry)
        {
            if (buildingRegistry == null)
                return null;

            return buildingRegistry.GetById(buildingId);
        }

        private static string NormalizeOwnerId(string ownerId)
        {
            return string.IsNullOrWhiteSpace(ownerId) ? EconomyManager.DefaultOwnerId : ownerId.Trim();
        }
    }
}
