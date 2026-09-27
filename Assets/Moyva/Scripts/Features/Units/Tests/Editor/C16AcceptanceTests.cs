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
    /// C16: recruitment must atomically reserve population and pay costs through
    /// the canonical service. Population buckets stay distinct
    /// (civilians / reserved recruits / enlisted), a failed enqueue leaves no
    /// reservation behind, and cancel/deploy/death never return a resident twice.
    /// </summary>
    public class C16AcceptanceTests
    {
        private const string Owner = "p1";
        private const string OtherOwner = "p2";
        private const string BuildingId = "barracks";
        private const string UnitType = "militia";
        private static readonly Vector2Int Position = new Vector2Int(3, 4);

        private FakeEconomy _economy;
        private FakeLifecycle _lifecycle;
        private FakeSnapshotSource _snapshot;
        private UnitRecruitmentService _service;

        [SetUp]
        public void SetUp()
        {
            _economy = new FakeEconomy();
            _lifecycle = new FakeLifecycle();
            _snapshot = new FakeSnapshotSource(
                new ConstructionSavedPlacement(Position, BuildingId, Owner));
            _service = CreateService(_economy, _lifecycle, _snapshot);
        }

        [Test]
        public void Enqueue_ReservesPopulationThenConsumesCosts()
        {
            Assert.IsTrue(_service.TryEnqueue(Owner, Position, UnitType, out string reason), reason);

            Assert.AreEqual(1, _economy.Reservations.Count, "Exactly one population reservation.");
            int reservedCount;
            Assert.IsTrue(_economy.Reservations.TryGetValue(1L, out reservedCount));
            Assert.AreEqual(2, reservedCount, "PopulationCost=2 must reserve two residents.");
            Assert.AreEqual(3, _economy.AvailablePopulation, "Reserved residents leave the available pool.");
            Assert.AreEqual(1, _economy.Consumed.Count, "Costs are debited at enqueue, not later.");
            Assert.AreEqual(1, _service.GetQueue(Owner, Position).Count);
        }

        [Test]
        public void Enqueue_InsufficientPopulation_LeavesNoReserveNoConsume()
        {
            _economy.AvailablePopulation = 0;

            Assert.IsFalse(_service.TryEnqueue(Owner, Position, UnitType, out string reason));
            Assert.IsFalse(string.IsNullOrWhiteSpace(reason));
            Assert.AreEqual(0, _economy.Reservations.Count);
            Assert.AreEqual(0, _economy.Consumed.Count);
            Assert.AreEqual(0, _service.GetQueue(Owner, Position).Count);
        }

        [Test]
        public void Enqueue_ResourceConsumeFails_RollsBackPopulation()
        {
            _economy.ConsumeFails = true;

            Assert.IsFalse(_service.TryEnqueue(Owner, Position, UnitType, out _));
            Assert.AreEqual(0, _economy.Reservations.Count,
                "Failed payment must release the population reservation.");
            Assert.AreEqual(1, _economy.ReleasedQueueIds.Count, "Release must run exactly once.");
            Assert.AreEqual(5, _economy.AvailablePopulation);
            Assert.AreEqual(0, _service.GetQueue(Owner, Position).Count);
        }

        [Test]
        public void Enqueue_SecondEnqueueCannotReuseReservedResident()
        {
            _economy.AvailablePopulation = 2;

            Assert.IsTrue(_service.TryEnqueue(Owner, Position, UnitType, out _));
            Assert.IsFalse(_service.TryEnqueue(Owner, Position, UnitType, out string reason),
                "A second job must not reuse the reserved residents.");
            StringAssert.Contains("population", reason.ToLowerInvariant());
            Assert.AreEqual(1, _economy.Reservations.Count);
            Assert.AreEqual(1, _service.GetQueue(Owner, Position).Count);
        }

        [Test]
        public void Enqueue_QueueFull_RejectedWithoutMutation()
        {
            var module = BuildModule(capacity: 1);
            _service = CreateService(_economy, _lifecycle, _snapshot, module);

            Assert.IsTrue(_service.TryEnqueue(Owner, Position, UnitType, out _));
            Assert.IsFalse(_service.TryEnqueue(Owner, Position, UnitType, out string reason));
            StringAssert.Contains("full", reason.ToLowerInvariant());
            Assert.AreEqual(1, _economy.Consumed.Count, "Rejected enqueue must not pay.");
            Assert.AreEqual(1, _economy.Reservations.Count);
        }

        [Test]
        public void Enqueue_InactiveBarrack_Rejected()
        {
            _lifecycle.Operational = false;

            Assert.IsFalse(_service.TryEnqueue(Owner, Position, UnitType, out _));
            Assert.AreEqual(0, _economy.Reservations.Count);
            Assert.AreEqual(0, _economy.Consumed.Count);
        }

        [Test]
        public void Enqueue_WrongOwner_Rejected()
        {
            Assert.IsFalse(_service.TryEnqueue(OtherOwner, Position, UnitType, out _));
            Assert.AreEqual(0, _economy.Reservations.Count);
            Assert.AreEqual(0, _economy.Consumed.Count);
        }

        [Test]
        public void Enqueue_ReentrantIntentDuringPayment_Rejected()
        {
            _economy.OnConsume = () =>
            {
                bool inner = _service.TryEnqueue(Owner, Position, UnitType, out string innerReason);
                Assert.IsFalse(inner, "Nested enqueue during payment must be rejected.");
                StringAssert.Contains("in progress", innerReason);
            };

            Assert.IsTrue(_service.TryEnqueue(Owner, Position, UnitType, out _));
            Assert.AreEqual(1, _economy.Reservations.Count);
            Assert.AreEqual(1, _service.GetQueue(Owner, Position).Count);
        }

        [Test]
        public void Enqueue_CostsDebitedAtEnqueue_NotPerTurn()
        {
            Assert.IsTrue(_service.TryEnqueue(Owner, Position, UnitType, out _));
            Assert.AreEqual(1, _economy.Consumed.Count);

            _service.OnTurnStarted(new TurnContext(1, 2L, 0, new TurnFaction(Owner, Vector2Int.zero)));

            Assert.AreEqual(1, _economy.Consumed.Count, "Turn progress must not re-debit costs.");
        }

        [Test]
        public void Cancel_ReleasesPopulationAndRefunds_ExactlyOnce()
        {
            Assert.IsTrue(_service.TryEnqueue(Owner, Position, UnitType, out _));

            Assert.IsTrue(_service.TryCancel(Owner, Position, 1L, out _));
            Assert.AreEqual(0, _economy.Reservations.Count);
            Assert.AreEqual(5, _economy.AvailablePopulation, "Cancelled resident returns once.");
            Assert.AreEqual(1, _economy.Refunded.Count);

            Assert.IsFalse(_service.TryCancel(Owner, Position, 1L, out _),
                "Second cancel must not return the resident again.");
            Assert.AreEqual(5, _economy.AvailablePopulation);
            Assert.AreEqual(1, _economy.Refunded.Count);
        }

        [Test]
        public void Cancel_ReadyItem_Rejected_PopulationStaysReserved()
        {
            Assert.IsTrue(_service.TryEnqueue(Owner, Position, UnitType, out _));
            _service.OnTurnStarted(new TurnContext(1, 2L, 0, new TurnFaction(Owner, Vector2Int.zero)));

            UnitRecruitmentQueueItemSnapshot item = _service.GetQueue(Owner, Position)[0];
            Assert.IsTrue(item.IsReady, "TrainingTurns=1 must complete after one owner turn.");

            Assert.IsFalse(_service.TryCancel(Owner, Position, 1L, out _));
            Assert.AreEqual(1, _economy.Reservations.Count, "Ready job keeps its reservation for deploy.");
            Assert.AreEqual(0, _economy.Refunded.Count);
        }

        [Test]
        public void Restore_LegacyItemWithoutReceipt_CannotCancel()
        {
            var legacy = new UnitRecruitmentQueueItemSnapshot(
                queueId: 7L,
                ownerId: Owner,
                recruitingBuildingPosition: Position,
                unitTypeId: UnitType,
                completedTurns: 0,
                trainingTurns: 2,
                enqueuedGlobalTurn: 1L,
                status: UnitRecruitmentQueueStatus.Training);
            ((IUnitRecruitmentStateStore)_service).RestoreState(new[] { legacy });

            Assert.IsFalse(_service.TryCancel(Owner, Position, 7L, out _),
                "Missing payment receipt must refuse cancel instead of refunding nothing.");
            Assert.AreEqual(1, _service.GetQueue(Owner, Position).Count);
            Assert.AreEqual(0, _economy.Refunded.Count);
        }

        [Test]
        public void Enqueue_NoWarehouse_PreviewMirrorsCombinedFunding()
        {
            // Anti-softlock commit path draws owner pool + settlement pools; the
            // shortage preview must not reject what commit can afford.
            _economy.HasWarehouse = false;
            _economy.OwnerAggregate["wood"] = 10f;
            _economy.OwnerAggregate["food"] = 4f;

            Assert.IsTrue(_service.CanEnqueue(Owner, Position, UnitType, out string reason), reason);
            Assert.IsTrue(_service.TryEnqueue(Owner, Position, UnitType, out reason), reason);
            Assert.AreEqual(1, _economy.Consumed.Count);
        }

        [Test]
        public void GetOptions_NoEconomy_ReturnsEmptyWithoutThrow()
        {
            var service = CreateService(null, _lifecycle, _snapshot);
            Assert.DoesNotThrow(() =>
            {
                var options = service.GetOptions(Owner);
                Assert.AreEqual(0, options.Count);
            });
        }

        private static UnitRecruitmentBuildingModule BuildModule(int capacity = 3)
        {
            return new UnitRecruitmentBuildingModule
            {
                QueueCapacity = capacity,
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
        }

        private static UnitRecruitmentService CreateService(
            FakeEconomy economy, FakeLifecycle lifecycle, FakeSnapshotSource snapshot,
            UnitRecruitmentBuildingModule module = null)
        {
            var definition = new BuildingDefinition
            {
                Id = BuildingId,
                DisplayName = "Barracks",
                Modules = { module ?? BuildModule() },
            };

            return new UnitRecruitmentService(
                new FakeUnitClassConfig(),
                buildingRegistry: new FakeBuildingRegistry(definition),
                constructionSnapshot: snapshot,
                constructionLifecycle: lifecycle,
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
            public FakeBuildingRegistry(BuildingDefinition definition) => _definition = definition;
            public BuildingDefinition[] GetAll() => new[] { _definition };
            public BuildingDefinition GetById(string id) => id == _definition.Id ? _definition : null;
            public BuildingDefinition[] GetByCategory(BuildingCategory category)
                => Array.Empty<BuildingDefinition>();
            public WallCollectionDefinition[] GetWallCollections()
                => Array.Empty<WallCollectionDefinition>();
            public WallCollectionDefinition GetWallCollectionByBuildingId(string buildingId) => null;
        }

        private sealed class FakeSnapshotSource : IConstructionSaveSnapshotSource
        {
            private readonly ConstructionSavedPlacement _placement;
            public FakeSnapshotSource(ConstructionSavedPlacement placement) => _placement = placement;
            public IReadOnlyList<ConstructionSavedPlacement> GetSavedPlacements() => new[] { _placement };
        }

        private sealed class FakeLifecycle : IConstructionLifecycle
        {
            public bool Operational = true;
            public bool IsOperational(Vector2Int position) => Operational;
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
            public int AvailablePopulation = 5;
            public bool HasWarehouse = true;
            public bool ConsumeFails;
            public Action OnConsume;
            public readonly Dictionary<long, int> Reservations = new Dictionary<long, int>();
            public readonly List<long> ReleasedQueueIds = new List<long>();
            public readonly List<IReadOnlyDictionary<string, float>> Consumed =
                new List<IReadOnlyDictionary<string, float>>();
            public readonly List<IReadOnlyDictionary<string, float>> Refunded =
                new List<IReadOnlyDictionary<string, float>>();
            public readonly Dictionary<string, float> SettlementAvailable =
                new Dictionary<string, float>(StringComparer.Ordinal);
            public readonly Dictionary<string, float> OwnerAggregate =
                new Dictionary<string, float>(StringComparer.Ordinal);

            public RecruitmentPopulationSnapshot GetRecruitmentPopulation(string ownerId, Vector2Int position)
                => new RecruitmentPopulationSnapshot(AvailablePopulation, AvailablePopulation, 0, 0, 1f);

            public bool TryReserveRecruitmentPopulation(string ownerId, Vector2Int position,
                long queueId, int count, out string reason)
            {
                reason = null;
                if (AvailablePopulation < count)
                {
                    reason = "Not enough available population.";
                    return false;
                }
                AvailablePopulation -= count;
                Reservations[queueId] = count;
                return true;
            }

            public void ReleaseRecruitmentPopulation(string ownerId, long queueId)
            {
                ReleasedQueueIds.Add(queueId);
                if (Reservations.TryGetValue(queueId, out int count))
                {
                    Reservations.Remove(queueId);
                    AvailablePopulation += count;
                }
            }

            public void DeployRecruitmentPopulation(string ownerId, long queueId, string unitId)
                => Reservations.Remove(queueId);

            public bool TryGetSettlementContext(Vector2Int position, out EconomySettlementContext context)
            {
                context = new EconomySettlementContext("s1", "Town", Owner);
                return true;
            }

            public bool TryResolveConstructionSettlement(Vector2Int position, string ownerId,
                out EconomySettlementContext context)
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

            public bool TryConsumeSettlementResources(string settlementId,
                IReadOnlyDictionary<string, float> resourceCosts, out string errorMessage)
            {
                return Consume(resourceCosts, out errorMessage);
            }

            public bool TryConsumeOwnerPoolResources(string ownerId,
                IReadOnlyDictionary<string, float> resourceCosts, out string errorMessage)
            {
                return Consume(resourceCosts, out errorMessage);
            }

            private bool Consume(IReadOnlyDictionary<string, float> costs, out string errorMessage)
            {
                OnConsume?.Invoke();
                if (ConsumeFails)
                {
                    errorMessage = "Insufficient.";
                    return false;
                }
                Consumed.Add(costs);
                errorMessage = null;
                return true;
            }

            public void RefundOwnerPoolResources(string ownerId,
                IReadOnlyDictionary<string, float> resources)
                => Refunded.Add(resources);

            public void RefundRecruitmentResources(string ownerId, string settlementId,
                IReadOnlyDictionary<string, float> resources)
                => Refunded.Add(resources);

            public bool OwnerHasAnyWarehouse(string ownerId) => HasWarehouse;
            public IReadOnlyDictionary<string, float> GetWarehouseResourceTotals(Vector2Int warehousePosition)
                => SettlementAvailable;
            public IReadOnlyDictionary<string, float> GetSettlementWarehousesTotal(string settlementId)
                => SettlementAvailable;
            public IReadOnlyDictionary<string, float> GetSettlementResourceTotals(string settlementId)
                => SettlementAvailable;
            public IReadOnlyDictionary<string, float> GetSettlementReservedResourceTotals(string settlementId)
                => new Dictionary<string, float>(StringComparer.Ordinal);
            public IReadOnlyDictionary<string, float> GetSettlementAvailableResourceTotals(string settlementId)
                => SettlementAvailable;
            public void ReleaseConstructionSupplyReservations(Vector2Int placementPosition) { }
            public IReadOnlyDictionary<string, float> GetSettlementResourcesForPlacement(
                string settlementId, Vector2Int placementPosition) => SettlementAvailable;
            public IReadOnlyDictionary<string, float> GetOwnerPoolResourceTotals(string ownerId)
                => new Dictionary<string, float>(StringComparer.Ordinal);
            public IReadOnlyDictionary<string, float> GetOwnerResourceTotals(string ownerId)
                => OwnerAggregate;
            public string GetResourceDisplayName(string resourceId) => resourceId;
        }
    }
}
