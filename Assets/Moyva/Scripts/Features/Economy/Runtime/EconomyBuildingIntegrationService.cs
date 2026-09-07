using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Economy.Runtime
{
    internal sealed class EconomyBuildingIntegrationService : IEconomyBuildingIntegration
    {
        private const string PerfLogTag =
            "[MoyvaConstructionPerf]";
        private const double EconomyPlacementPerfThresholdMs = 0.5d;

        private readonly Dictionary<BuildingDefinition, bool>
            _moduleValidationErrorsByDefinition = new();

        public EconomySettlementState OnBuildingPlaced(BuildingPlacedSignal signal, ISettlementRegistry registry, SignalBus signalBus, EconomyDatabaseSO database, IBuildingRegistry buildingRegistry)
        {
            var definition = FindBuildingDefinition(signal.BuildingId, buildingRegistry);
            if (definition == null)
                return null;

            if (HasModuleValidationErrors(definition))
            {
                Debug.LogError(
                    $"[Economy] Будівля '{signal.BuildingId}' містить " +
                    "невалідну модульну конфігурацію. " +
                    "Розміщення в економіці пропущено.");
                return null;
            }

            var ownerId = NormalizeOwnerId(signal.OwnerId);

            if (signal.HasRelocationSource
                && signal.RelocationSourcePosition != signal.Position
                && registry.TryGetSettlementByPosition(signal.RelocationSourcePosition, out var relocatedSettlement))
            {
                if (BuildingDefinitionCapabilities.IsWarehouse(definition))
                {
                    string previousWarehouseKey = ToWarehouseKey(signal.RelocationSourcePosition);
                    string nextWarehouseKey = ToWarehouseKey(signal.Position);
                    if (relocatedSettlement.WarehouseResourcePools.TryGetValue(
                            previousWarehouseKey,
                            out var warehouseResources))
                    {
                        relocatedSettlement.WarehouseResourcePools.Remove(previousWarehouseKey);
                        relocatedSettlement.WarehouseResourcePools[nextWarehouseKey] = warehouseResources;
                        relocatedSettlement.MoveWarehousePolicy(
                            previousWarehouseKey,
                            nextWarehouseKey);
                    }
                }

                string previousInstanceKey =
                    ToBuildingInstanceKey(
                        signal.BuildingId,
                        signal.RelocationSourcePosition);
                string nextInstanceKey =
                    ToBuildingInstanceKey(
                        signal.BuildingId,
                        signal.Position);
                for (int index = 0;
                     index < relocatedSettlement.Buildings.Count;
                     index++)
                {
                    EconomyBuildingState building =
                        relocatedSettlement.Buildings[index];
                    bool matchesInstance = string.Equals(
                        building.InstanceKey,
                        previousInstanceKey,
                        System.StringComparison.Ordinal);
                    bool matchesLegacyInstance =
                        string.IsNullOrWhiteSpace(building.InstanceKey)
                        && building.BuildingId == signal.BuildingId
                        && building.GridPosition
                            == signal.RelocationSourcePosition;
                    if (!matchesInstance && !matchesLegacyInstance)
                        continue;

                    building.InstanceKey = nextInstanceKey;
                    building.GridPosition = signal.Position;
                    if (relocatedSettlement.WorkerAssignments.TryGetValue(
                            previousInstanceKey,
                            out int assigned))
                    {
                        relocatedSettlement.WorkerAssignments.Remove(
                            previousInstanceKey);
                        relocatedSettlement.WorkerAssignments[
                            nextInstanceKey] = assigned;
                    }
                    break;
                }

                registry.UnregisterBuildingPosition(signal.RelocationSourcePosition);
                registry.RegisterBuildingPosition(
                    signal.Position,
                    relocatedSettlement.SettlementId,
                    signal.BuildingId,
                    ownerId);
                return null;
            }

            // If this building is a TownHall or Castle, create a new settlement
            if (BuildingDefinitionCapabilities.IsTownHall(definition) ||
                BuildingDefinitionCapabilities.IsCastle(definition))
            {
                return CreateSettlement(
                    signal.BuildingId,
                    signal.Position,
                    definition,
                    ownerId,
                    registry,
                    signalBus,
                    database,
                    buildingRegistry);
            }

            // Otherwise, assign building to nearest settlement of the same owner
            if (!registry.TryFindNearestSettlement(signal.Position, ownerId, out var state))
            {
                return null;
            }

            AddBuildingToSettlement(
                state,
                signal.BuildingId,
                signal.Position,
                definition);
            registry.RegisterBuildingPosition(signal.Position, state.SettlementId, signal.BuildingId, ownerId);

            EnsureWarehouseForBuilding(
                state,
                signal.Position,
                definition,
                database);

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

            // Remove the exact building instance, not the first building
            // with the same type ID.
            string removedInstanceKey =
                ToBuildingInstanceKey(
                    signal.BuildingId,
                    signal.Position);
            for (int i = state.Buildings.Count - 1; i >= 0; i--)
            {
                if (string.Equals(
                        state.Buildings[i].InstanceKey,
                        removedInstanceKey,
                        System.StringComparison.Ordinal)
                    || (string.IsNullOrWhiteSpace(
                            state.Buildings[i].InstanceKey)
                        && state.Buildings[i].BuildingId == signal.BuildingId
                        && state.Buildings[i].GridPosition == signal.Position))
                {
                    state.WorkerAssignments.Remove(
                        state.Buildings[i].InstanceKey);
                    state.Buildings.RemoveAt(i);
                    break;
                }
            }

            registry.UnregisterBuildingPosition(signal.Position);

            if (BuildingDefinitionCapabilities.IsWarehouse(definition))
            {
                state.RemoveWarehousePool(ToWarehouseKey(signal.Position));
                state.EnsureWarehouseConsistency();
            }

            if (BuildingDefinitionCapabilities.IsTownHall(definition)
                || BuildingDefinitionCapabilities.IsCastle(definition))
            {
                state.IsActive = false;
                string ownerId = NormalizeOwnerId(state.OwnerId);
                signalBus.Fire(new SettlementDeactivatedSignal
                {
                    SettlementId = state.SettlementId,
                    OwnerId = ownerId,
                    Reason = BuildingDefinitionCapabilities.IsCastle(definition)
                        ? "Castle destroyed"
                        : "Town Hall destroyed",
                });
                if (!OwnerHasActiveSettlementCenter(ownerId, registry))
                {
                    signalBus.Fire(new FactionEliminatedSignal
                    {
                        FactionId = ownerId,
                    });
                }
            }

            if (BuildingDefinitionCapabilities.IsHousing(definition))
                RecalculateHousing(state, buildingRegistry);
        }

        private EconomySettlementState CreateSettlement(string townHallBuildingId, Vector2Int position, BuildingDefinition definition, string ownerId, ISettlementRegistry registry, SignalBus signalBus, EconomyDatabaseSO database, IBuildingRegistry buildingRegistry)
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
            AddBuildingToSettlement(
                state,
                townHallBuildingId,
                position,
                definition);

            EnsureWarehouseForBuilding(
                state,
                position,
                definition,
                database);

            // Start with initial population (2 residents)
            state.Residents.Add(new EconomyResidentState(age: 25, hp: 100f, comfort: 50f, houseCollapsed: false));
            state.Residents.Add(new EconomyResidentState(age: 22, hp: 100f, comfort: 50f, houseCollapsed: false));

            RecalculateHousing(state, buildingRegistry);

            registry.RegisterSettlement(state, position);
            registry.RegisterBuildingPosition(position, id, townHallBuildingId, ownerId);

            signalBus.Fire(new SettlementCreatedSignal
            {
                SettlementId = id,
                OwnerId = ownerId,
                TownHallPosition = position,
            });
            return state;
        }

        private static void AddBuildingToSettlement(
            EconomySettlementState state,
            string buildingId,
            Vector2Int position,
            BuildingDefinition definition)
        {
            var buildingState = new EconomyBuildingState
            {
                InstanceKey = ToBuildingInstanceKey(
                    buildingId,
                    position),
                GridPosition = position,
                BuildingId = buildingId,
                RequiredWorkers =
                    BuildingDefinitionCapabilities
                        .GetRequiredWorkers(definition),
                EconomyPriority =
                    BuildingDefinitionCapabilities
                        .GetEconomyPriority(definition),
                WorkerTypeId =
                    BuildingDefinitionCapabilities
                        .GetWorkerTypeId(definition),
                IsActive = true,
                ProductionProgress = 0f,
            };

            if (BuildingDefinitionCapabilities.TryGetEnabledModule(
                    definition,
                    out ProductionBuildingModule production))
            {
                if (production.Recipes != null
                    && production.Recipes.Count > 0)
                {
                    buildingState.ProductionRecipes =
                        new List<ProductionRecipeDefinition>(
                            production.Recipes);
                }
                else if (!string.IsNullOrWhiteSpace(
                             production.ResourceId))
                {
                    // Legacy ResourceId becomes an explicit one-turn recipe,
                    // so adding ProductionBuildingModule always has a runtime effect.
                    buildingState.ProductionRecipes.Add(
                        new ProductionRecipeDefinition
                        {
                            RecipeId =
                                $"{buildingId}:legacy",
                            TurnsPerCycle = 1,
                            RequiresWorkers =
                                buildingState.RequiredWorkers > 0,
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

                // Old EconomyProductionProfile assets remain a fallback when
                // the module has no data-driven recipe.
                buildingState.ProductionProfileId =
                    buildingId;
            }

            state.Buildings.Add(buildingState);
        }

        private static IReadOnlyList<string>
            ResolveAcceptedStorageResources(
                BuildingDefinition definition,
                EconomyDatabaseSO database)
        {
            IReadOnlyList<string> explicitIds =
                BuildingDefinitionCapabilities
                    .GetAcceptedStorageResourceIds(definition);
            if (explicitIds != null && explicitIds.Count > 0)
                return explicitIds;

            EconomyResourceCategory? category = null;

            if (BuildingDefinitionCapabilities.TryGetEnabledModule(
                    definition,
                    out StorageBuildingModule storage))
            {
                category = storage.StorageKind switch
                {
                    BuildingStorageKind.Food =>
                        EconomyResourceCategory.Food,
                    BuildingStorageKind.Material =>
                        EconomyResourceCategory.Materials,
                    _ => null,
                };
            }
            else if (BuildingDefinitionCapabilities.TryGetEnabledModule(
                         definition,
                         out BarnBuildingModule _))
            {
                category = EconomyResourceCategory.Food;
            }

            if (!category.HasValue
                || database?.Resources == null)
            {
                return Array.Empty<string>();
            }

            var result = new List<string>();
            for (int index = 0;
                 index < database.Resources.Count;
                 index++)
            {
                EconomyResourceDefinition resource =
                    database.Resources[index];
                if (resource == null
                    || resource.Category != category.Value
                    || string.IsNullOrWhiteSpace(resource.Id))
                {
                    continue;
                }

                result.Add(resource.Id);
            }

            return result;
        }

        private static void EnsureWarehouseForBuilding(
            EconomySettlementState state,
            Vector2Int position,
            BuildingDefinition definition,
            EconomyDatabaseSO database)
        {
            if (state == null
                || !BuildingDefinitionCapabilities.IsWarehouse(definition))
            {
                return;
            }

            string warehouseKey = ToWarehouseKey(position);
            state.EnsureWarehousePool(warehouseKey);
            state.ConfigureWarehousePolicy(
                warehouseKey,
                BuildingDefinitionCapabilities.GetStorageCapacity(definition),
                ResolveAcceptedStorageResources(definition, database));
            state.EnsureWarehouseConsistency();
        }

        private static string ToBuildingInstanceKey(
            string buildingId,
            Vector2Int position)
            => $"{buildingId}@{position.x},{position.y}";

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

        private static bool OwnerHasActiveSettlementCenter(
            string ownerId,
            ISettlementRegistry registry)
        {
            string normalizedOwnerId = NormalizeOwnerId(ownerId);
            foreach (var pair in registry.AllSettlements)
            {
                EconomySettlementState settlement = pair.Value;
                if (settlement == null
                    || !settlement.IsActive
                    || !string.Equals(
                        NormalizeOwnerId(settlement.OwnerId),
                        normalizedOwnerId,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private bool HasModuleValidationErrors(
            BuildingDefinition definition)
        {
            if (definition == null)
                return true;

            if (_moduleValidationErrorsByDefinition.TryGetValue(
                    definition,
                    out bool cached))
            {
                return cached;
            }

            double startedAt =
                Time.realtimeSinceStartupAsDouble;

            var issues =
                BuildingModuleValidation.Validate(definition);
            bool hasErrors =
                BuildingModuleValidation.HasErrors(issues);

            _moduleValidationErrorsByDefinition[definition] =
                hasErrors;

            if (Debug.isDebugBuild)
            {
                double elapsedMs =
                    (Time.realtimeSinceStartupAsDouble - startedAt)
                    * 1000d;
                if (elapsedMs >= EconomyPlacementPerfThresholdMs)
                {
                }
            }

            return hasErrors;
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
