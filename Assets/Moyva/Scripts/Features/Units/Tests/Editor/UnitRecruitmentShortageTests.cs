using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Units
{
    /// <summary>
    /// Structured recruitment shortages (P051/P052): the enqueue check must
    /// report every deficit in one response, count reserved stock as
    /// unavailable, and keep population separate from resources.
    /// </summary>
    public class UnitRecruitmentShortageTests
    {
        private const string Owner = "p1";
        private const string BuildingId = "barracks";
        private const string UnitType = "militia";
        private static readonly Vector2Int Position = new Vector2Int(3, 4);

        private FakeEconomy _economy;
        private UnitRecruitmentService _service;

        [SetUp]
        public void SetUp()
        {
            _economy = new FakeEconomy();
            _service = CreateService(_economy);
        }

        [Test]
        public void Shortages_ReportsEveryMissingResource_NotJustFirst()
        {
            _economy.SettlementAvailable["wood"] = 5f;
            _economy.SettlementAvailable["food"] = 0f;

            bool ok = _service.TryGetEnqueueShortages(
                Owner, Position, UnitType,
                out IReadOnlyList<UnitRecruitmentShortage> shortages,
                out string reason);

            Assert.IsFalse(ok);
            Assert.IsNull(reason, "Shortage-only failure must not set an eligibility reason.");
            Assert.NotNull(shortages);
            Assert.AreEqual(2, shortages.Count, "Both missing resources must be reported.");
            var wood = Find(shortages, "wood");
            var food = Find(shortages, "food");
            Assert.AreEqual(10f, wood.Required);
            Assert.AreEqual(5f, wood.Available);
            Assert.AreEqual(5f, wood.Missing);
            Assert.AreEqual(4f, food.Required);
            Assert.AreEqual(4f, food.Missing);
        }

        [Test]
        public void Shortages_ReservedStockCountsAsUnavailable()
        {
            // P052: 20 stored, 15 reserved, need 10 -> deficit 5.
            _economy.SettlementAvailable["wood"] = 5f;
            _economy.SettlementReserved["wood"] = 15f;
            _economy.SettlementAvailable["food"] = 10f;

            bool ok = _service.TryGetEnqueueShortages(
                Owner, Position, UnitType,
                out IReadOnlyList<UnitRecruitmentShortage> shortages,
                out _);

            Assert.IsFalse(ok, "Reserved stock must not satisfy the check.");
            var wood = Find(shortages, "wood");
            Assert.AreEqual(15f, wood.Reserved);
            Assert.AreEqual(5f, wood.Available);
            Assert.AreEqual(5f, wood.Missing);
        }

        [Test]
        public void Shortages_PopulationReportedSeparatelyAlongsideResources()
        {
            _economy.Population = new RecruitmentPopulationSnapshot(
                total: 3, available: 0, training: 1, military: 0, constructionSpeed: 1f);
            _economy.SettlementAvailable["wood"] = 0f;

            _service.TryGetEnqueueShortages(
                Owner, Position, UnitType,
                out IReadOnlyList<UnitRecruitmentShortage> shortages,
                out _);

            bool hasPopulation = false;
            bool hasWood = false;
            foreach (var shortage in shortages)
            {
                if (shortage.IsPopulation)
                {
                    hasPopulation = true;
                    Assert.AreEqual(2f, shortage.Required);
                    Assert.AreEqual(0f, shortage.Available);
                }
                if (shortage.ResourceId == "wood") hasWood = true;
            }
            Assert.IsTrue(hasPopulation, "Population deficit must be reported.");
            Assert.IsTrue(hasWood, "Resource deficit must be reported too.");
        }

        [Test]
        public void CanEnqueue_Affordable_ReturnsTrueWithEmptyShortages()
        {
            _economy.SettlementAvailable["wood"] = 50f;
            _economy.SettlementAvailable["food"] = 50f;

            bool ok = _service.TryGetEnqueueShortages(
                Owner, Position, UnitType,
                out IReadOnlyList<UnitRecruitmentShortage> shortages,
                out string reason);

            Assert.IsTrue(ok);
            Assert.IsNull(reason);
            Assert.AreEqual(0, shortages.Count);
            Assert.IsTrue(_service.CanEnqueue(Owner, Position, UnitType, out _));
        }

        [Test]
        public void CanEnqueue_FirstShortageFeedsLegacyReason()
        {
            _economy.SettlementAvailable["wood"] = 0f;
            _economy.SettlementAvailable["food"] = 0f;

            bool ok = _service.CanEnqueue(Owner, Position, UnitType, out string reason);

            Assert.IsFalse(ok);
            Assert.IsTrue(reason.StartsWith("Insufficient recruitment resource:"),
                $"CanEnqueue must keep the parseable resource reason, got: {reason}");
        }

        [Test]
        public void Shortages_IneligibleRequest_ReturnsReasonWithoutShortages()
        {
            bool ok = _service.TryGetEnqueueShortages(
                Owner, Position, "unknown-unit",
                out IReadOnlyList<UnitRecruitmentShortage> shortages,
                out string reason);

            Assert.IsFalse(ok);
            Assert.IsFalse(string.IsNullOrWhiteSpace(reason));
            Assert.IsTrue(shortages == null || shortages.Count == 0);
        }

        private static UnitRecruitmentShortage Find(
            IReadOnlyList<UnitRecruitmentShortage> shortages, string resourceId)
        {
            foreach (var shortage in shortages)
                if (shortage.ResourceId == resourceId)
                    return shortage;
            Assert.Fail($"Shortage for '{resourceId}' not reported.");
            return default;
        }

        private static UnitRecruitmentService CreateService(FakeEconomy economy)
        {
            var module = new UnitRecruitmentBuildingModule
            {
                QueueCapacity = 3,
                Recipes =
                {
                    new UnitRecruitmentRecipeDefinition
                    {
                        UnitTypeId = UnitType,
                        PopulationCost = 2,
                        TrainingTurns = 1,
                        Costs =
                        {
                            new BuildingResourceAmount { ResourceId = "wood", Amount = 10 },
                            new BuildingResourceAmount { ResourceId = "food", Amount = 4 },
                        },
                    },
                },
            };
            var definition = new BuildingDefinition
            {
                Id = BuildingId,
                DisplayName = "Barracks",
                Modules = { module },
            };

            return new UnitRecruitmentService(
                new FakeUnitClassConfig(),
                buildingRegistry: new FakeBuildingRegistry(definition),
                constructionSnapshot: new FakeSnapshotSource(
                    new ConstructionSavedPlacement(Position, BuildingId, Owner)),
                constructionLifecycle: new FakeLifecycle(),
                economy: economy,
                turns: new FakeTurns());
        }

        private sealed class FakeUnitClassConfig : IUnitClassConfig
        {
            public UnitClassConfig GetConfig(string typeId)
                => typeId == UnitType ? new UnitClassConfig { TypeId = typeId } : null;
        }

        private sealed class FakeBuildingRegistry : IBuildingRegistry
        {
            private readonly BuildingDefinition _definition;

            public FakeBuildingRegistry(BuildingDefinition definition)
                => _definition = definition;

            public BuildingDefinition[] GetAll() => new[] { _definition };
            public BuildingDefinition GetById(string id)
                => id == _definition.Id ? _definition : null;
            public BuildingDefinition[] GetByCategory(BuildingCategory category)
                => Array.Empty<BuildingDefinition>();
            public WallCollectionDefinition[] GetWallCollections()
                => Array.Empty<WallCollectionDefinition>();
            public WallCollectionDefinition GetWallCollectionByBuildingId(string buildingId)
                => null;
        }

        private sealed class FakeSnapshotSource : IConstructionSaveSnapshotSource
        {
            private readonly ConstructionSavedPlacement _placement;

            public FakeSnapshotSource(ConstructionSavedPlacement placement)
                => _placement = placement;

            public IReadOnlyList<ConstructionSavedPlacement> GetSavedPlacements()
                => new[] { _placement };
        }

        private sealed class FakeLifecycle : IConstructionLifecycle
        {
            public bool IsOperational(Vector2Int position) => true;
            public bool TryGetProgress(Vector2Int position, out int completedTurns, out int requiredTurns)
            {
                completedTurns = 1;
                requiredTurns = 1;
                return true;
            }
        }

        private sealed class FakeTurns : ITurnService
        {
            public event Action StateChanged { add { } remove { } }
            public TurnPhase Phase => TurnPhase.AwaitingInput;
            public int Round => 1;
            public long GlobalTurn => 1;
            public int ActionsThisTurn => 0;
            public string ActiveOwnerId => Owner;
            public string LocalOwnerId => Owner;
            public IReadOnlyList<TurnFaction> Factions => Array.Empty<TurnFaction>();
            public bool IsOwnerActive(string ownerId) => true;
            public bool CanOwnerAct(string ownerId, out string reason)
            {
                reason = null;
                return true;
            }
            public bool TryRecordAction(string ownerId, string actionId) => true;
            public bool TryEndTurn(string requesterOwnerId, out string reason)
            {
                reason = null;
                return true;
            }
        }

        private sealed class FakeEconomy : IEconomyInfoMediator
        {
            public RecruitmentPopulationSnapshot Population =
                new RecruitmentPopulationSnapshot(5, 5, 0, 0, 1f);
            public readonly Dictionary<string, float> SettlementAvailable =
                new Dictionary<string, float>(StringComparer.Ordinal);
            public readonly Dictionary<string, float> SettlementReserved =
                new Dictionary<string, float>(StringComparer.Ordinal);
            public readonly Dictionary<string, float> PoolTotals =
                new Dictionary<string, float>(StringComparer.Ordinal);
            public bool HasWarehouse = true;

            public RecruitmentPopulationSnapshot GetRecruitmentPopulation(string ownerId, Vector2Int position)
                => Population;
            public bool TryReserveRecruitmentPopulation(string ownerId, Vector2Int position, long queueId, int count, out string reason)
            {
                reason = null;
                return true;
            }
            public void ReleaseRecruitmentPopulation(string ownerId, long queueId) { }
            public void DeployRecruitmentPopulation(string ownerId, long queueId, string unitId) { }
            public bool TryGetSettlementContext(Vector2Int position, out EconomySettlementContext context)
            {
                context = new EconomySettlementContext("s1", "Town", Owner);
                return true;
            }
            public bool TryResolveConstructionSettlement(Vector2Int position, string ownerId, out EconomySettlementContext context)
            {
                context = new EconomySettlementContext("s1", "Town", ownerId);
                return true;
            }
            public bool TryGetBuildingContext(Vector2Int position, out string buildingId, out string ownerId)
            {
                buildingId = BuildingId;
                ownerId = Owner;
                return true;
            }
            public bool TryConsumeSettlementResources(string settlementId, IReadOnlyDictionary<string, float> resourceCosts, out string errorMessage)
            {
                errorMessage = null;
                return true;
            }
            public bool TryConsumeOwnerPoolResources(string ownerId, IReadOnlyDictionary<string, float> resourceCosts, out string errorMessage)
            {
                errorMessage = null;
                return true;
            }
            public void RefundOwnerPoolResources(string ownerId, IReadOnlyDictionary<string, float> resources) { }
            public void RefundRecruitmentResources(string ownerId, string settlementId, IReadOnlyDictionary<string, float> resources) { }
            public bool OwnerHasAnyWarehouse(string ownerId) => HasWarehouse;
            public IReadOnlyDictionary<string, float> GetWarehouseResourceTotals(Vector2Int warehousePosition)
                => SettlementAvailable;
            public IReadOnlyDictionary<string, float> GetSettlementWarehousesTotal(string settlementId)
                => SettlementAvailable;
            public IReadOnlyDictionary<string, float> GetSettlementResourceTotals(string settlementId)
                => SettlementAvailable;
            public IReadOnlyDictionary<string, float> GetSettlementReservedResourceTotals(string settlementId)
                => SettlementReserved;
            public IReadOnlyDictionary<string, float> GetSettlementAvailableResourceTotals(string settlementId)
                => SettlementAvailable;
            public void ReleaseConstructionSupplyReservations(Vector2Int placementPosition) { }
            public IReadOnlyDictionary<string, float> GetSettlementResourcesForPlacement(string settlementId, Vector2Int placementPosition)
                => SettlementAvailable;
            public IReadOnlyDictionary<string, float> GetOwnerPoolResourceTotals(string ownerId)
                => PoolTotals;
            public IReadOnlyDictionary<string, float> GetOwnerResourceTotals(string ownerId)
                => PoolTotals;
            public string GetResourceDisplayName(string resourceId) => resourceId;
        }
    }
}
