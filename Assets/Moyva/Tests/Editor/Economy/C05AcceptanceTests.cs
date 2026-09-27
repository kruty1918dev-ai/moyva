using System;
using System.Collections.Generic;
using Kruty1918.Calendar.Config;
using Kruty1918.Calendar.Core;
using Kruty1918.Calendar.Domain;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.Economy
{
    /// <summary>
    /// C05: exactly-one starter grant + exactly-one building registration.
    /// Repeated placed/operational delivery must not duplicate residents,
    /// buildings or resources; relocation/demolition must not touch another
    /// instance of the same type id.
    /// </summary>
    [TestFixture]
    public class C05AcceptanceTests
    {
        private const string Owner = "player_0";
        private static readonly Vector2Int TownHallPos = new(10, 10);
        private static readonly Vector2Int FarmA = new(5, 5);
        private static readonly Vector2Int FarmB = new(6, 5);
        private static readonly Vector2Int FarmARelocated = new(7, 5);

        private FakeCalendar _calendar;
        private SignalBus _signals;
        private EconomyDatabaseSO _database;
        private FakeBuildingRegistry _buildings;
        private EconomyOwnerResourcePoolService _ownerPools;
        private EconomySettlementRegistryService _registry;
        private EconomyManager _manager;
        private List<SettlementCreatedSignal> _created;

        [SetUp]
        public void SetUp()
        {
            var container = new DiContainer();
            Zenject.SignalBusInstaller.Install(container);
            container.DeclareSignal<BuildingPlacedSignal>().OptionalSubscriber();
            container.DeclareSignal<BuildingOperationalSignal>().OptionalSubscriber();
            container.DeclareSignal<BuildingDemolishedSignal>().OptionalSubscriber();
            container.DeclareSignal<GrantStarterPackResourcesSignal>().OptionalSubscriber();
            container.DeclareSignal<SettlementCreatedSignal>().OptionalSubscriber();
            container.DeclareSignal<SettlementDeactivatedSignal>().OptionalSubscriber();
            container.DeclareSignal<FactionEliminatedSignal>().OptionalSubscriber();
            container.DeclareSignal<SettlementResourceChangedSignal>().OptionalSubscriber();
            container.DeclareSignal<SettlementPopulationChangedSignal>().OptionalSubscriber();
            container.DeclareSignal<ResourceDeficitSignal>().OptionalSubscriber();
            container.DeclareSignal<EconomyTickCompletedSignal>().OptionalSubscriber();
            container.DeclareSignal<UnitDestroyedSignal>().OptionalSubscriber();
            container.DeclareSignal<CaravanDeliveryCompletedSignal>().OptionalSubscriber();
            container.DeclareSignal<ConstructionSupplyReadySignal>().OptionalSubscriber();
            container.DeclareSignal<ConstructionSupplyOrderClosedSignal>().OptionalSubscriber();
            _signals = container.Resolve<SignalBus>();

            _created = new List<SettlementCreatedSignal>();
            _signals.Subscribe<SettlementCreatedSignal>(s => _created.Add(s));

            _database = new EconomyDatabaseSO();
            SetField(_database, "_rulesConfig", new EconomyRulesConfigSO());

            _buildings = new FakeBuildingRegistry();
            _calendar = new FakeCalendar();
            _ownerPools = new EconomyOwnerResourcePoolService();
            _registry = new EconomySettlementRegistryService();
            _manager = new EconomyManager(
                _calendar,
                _signals,
                _database,
                _buildings,
                _ownerPools,
                _registry,
                null,
                null);
            _manager.Initialize();
        }

        [TearDown]
        public void TearDown() => _manager.Dispose();

        private static void SetField(object target, string name, object value)
        {
            var field = target.GetType().GetField(
                name,
                System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Missing field '{name}'.");
            field.SetValue(target, value);
        }

        [Test]
        public void TimedTownHall_PlaceThenOperational_CreatesSettlementOnce()
        {
            _buildings.Add(TownHall(buildTurns: 2));

            FirePlaced("townhall", TownHallPos);
            Assert.AreEqual(0, _registry.AllSettlements.Count,
                "Town hall under construction must not register a settlement.");

            FireOperational("townhall", TownHallPos);
            Assert.AreEqual(1, _registry.AllSettlements.Count);
            int residentsAfterFirst = Settlement().Residents.Count;
            Assert.AreEqual(15, residentsAfterFirst);

            // Repeated operational delivery (instant buildings publish both
            // events; retries must not double-register).
            FireOperational("townhall", TownHallPos);
            FirePlaced("townhall", TownHallPos);

            Assert.AreEqual(1, _registry.AllSettlements.Count);
            Assert.AreEqual(1, _created.Count, "Exactly one SettlementCreated signal.");
            Assert.AreEqual(residentsAfterFirst, Settlement().Residents.Count,
                "Repeated delivery must not duplicate initial residents.");
            Assert.AreEqual(1, Settlement().Buildings.Count,
                "Town hall instance must not be duplicated.");
        }

        [Test]
        public void InstantFarm_RepeatPlaced_RegisteredOnce()
        {
            RegisterSettlementWithInstantTownHall();

            FirePlaced("farm", FarmA);
            FirePlaced("farm", FarmA);
            FireOperational("farm", FarmA);

            Assert.AreEqual(2, Settlement().Buildings.Count,
                "Town hall + one farm; the repeated placed/operational pair adds nothing.");
            Assert.AreEqual(1, CountBuildings("farm"));
        }

        [Test]
        public void TwoFarms_GetDistinctInstanceKeys()
        {
            RegisterSettlementWithInstantTownHall();

            FirePlaced("farm", FarmA);
            FirePlaced("farm", FarmB);

            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var building in Settlement().Buildings)
            {
                if (building.BuildingId == "farm")
                    keys.Add(building.InstanceKey);
            }
            Assert.AreEqual(2, keys.Count, "Each farm needs a unique instance key.");
            Assert.IsTrue(_registry.TryGetBuildingAtPosition(FarmA, out string idA, out _));
            Assert.IsTrue(_registry.TryGetBuildingAtPosition(FarmB, out string idB, out _));
            Assert.AreEqual("farm", idA);
            Assert.AreEqual("farm", idB);
        }

        [Test]
        public void Demolish_OneFarm_LeavesOtherIntact()
        {
            RegisterSettlementWithInstantTownHall();
            FirePlaced("farm", FarmA);
            FirePlaced("farm", FarmB);

            _signals.Fire(new BuildingDemolishedSignal
            {
                BuildingId = "farm",
                Position = FarmA,
                OwnerId = Owner,
            });

            Assert.AreEqual(1, CountBuildings("farm"), "Only the targeted farm is removed.");
            Assert.IsFalse(_registry.TryGetBuildingAtPosition(FarmA, out _, out _));
            Assert.IsTrue(_registry.TryGetBuildingAtPosition(FarmB, out string idB, out _));
            Assert.AreEqual("farm", idB);
            var remaining = Settlement().Buildings.Find(b => b.BuildingId == "farm");
            Assert.AreEqual(FarmB, remaining.GridPosition);
        }

        [Test]
        public void Relocation_MovesInstanceKey_WithoutTouchingOtherFarm()
        {
            RegisterSettlementWithInstantTownHall();
            FirePlaced("farm", FarmA);
            FirePlaced("farm", FarmB);

            _signals.Fire(new BuildingPlacedSignal
            {
                BuildingId = "farm",
                Position = FarmARelocated,
                OwnerId = Owner,
                HasRelocationSource = true,
                RelocationSourcePosition = FarmA,
            });

            Assert.AreEqual(2, CountBuildings("farm"),
                "Relocation updates the existing entry instead of adding one.");
            Assert.IsFalse(_registry.TryGetBuildingAtPosition(FarmA, out _, out _),
                "Source position must be unmapped.");
            Assert.IsTrue(_registry.TryGetBuildingAtPosition(FarmARelocated, out string moved, out _));
            Assert.AreEqual("farm", moved);
            Assert.IsTrue(_registry.TryGetBuildingAtPosition(FarmB, out string other, out _));
            Assert.AreEqual("farm", other, "Second farm instance keeps its registration.");
            var relocated = Settlement().Buildings.Find(b => b.GridPosition == FarmARelocated);
            Assert.IsNotNull(relocated);
            StringAssert.Contains("7,5", relocated.InstanceKey);
        }

        [Test]
        public void TimedFarm_NoProductionUntilOperational_ThenOncePerCycle()
        {
            _buildings.Add(Farm(buildTurns: 1));
            RegisterSettlementWithInstantTownHall();

            // Placed only: still under construction, must not produce.
            FirePlaced("farm", FarmA);
            Tick();
            Assert.AreEqual(0f, Settlement().GetResource("test-ingot"),
                "Incomplete building must not produce.");

            FireOperational("farm", FarmA);
            Tick();
            Assert.AreEqual(5f, Settlement().GetResource("test-ingot"),
                "Completed farm produces exactly one cycle per tick.");

            // Repeated operational must not create a second producer.
            FireOperational("farm", FarmA);
            FirePlaced("farm", FarmA);
            Tick();
            Assert.AreEqual(10f, Settlement().GetResource("test-ingot"),
                "Re-delivered events must not double the output rate.");
        }

        [Test]
        public void StarterPack_SettlementGrant_AppliesListedEntriesOnce()
        {
            RegisterSettlementWithInstantTownHall();
            string settlementId = Settlement().SettlementId;

            _signals.Fire(new GrantStarterPackResourcesSignal
            {
                SettlementId = settlementId,
                OwnerId = "intruder",
                Entries = new[]
                {
                    new StarterPackResourceEntrySignal { ResourceId = "wood", Amount = 999f },
                },
            });
            Assert.AreEqual(0f, Settlement().GetResource("wood"),
                "Grant with mismatched owner must be rejected.");

            var entries = new[]
            {
                new StarterPackResourceEntrySignal { ResourceId = "wood", Amount = 40f },
                new StarterPackResourceEntrySignal { ResourceId = "food", Amount = 25f },
                new StarterPackResourceEntrySignal { ResourceId = "", Amount = 10f },
                new StarterPackResourceEntrySignal { ResourceId = "stone", Amount = 0f },
            };
            _signals.Fire(new GrantStarterPackResourcesSignal
            {
                SettlementId = settlementId,
                OwnerId = Owner,
                Entries = entries,
            });

            Assert.AreEqual(40f, Settlement().GetResource("wood"));
            Assert.AreEqual(25f, Settlement().GetResource("food"));
            Assert.AreEqual(0f, Settlement().GetResource("stone"),
                "Zero-amount entries are skipped.");
        }

        [Test]
        public void StarterPack_OwnerScopedGrant_LandsInOwnerPool()
        {
            _signals.Fire(new GrantStarterPackResourcesSignal
            {
                SettlementId = string.Empty,
                OwnerId = Owner,
                Entries = new[]
                {
                    new StarterPackResourceEntrySignal { ResourceId = "wood", Amount = 40f },
                    new StarterPackResourceEntrySignal { ResourceId = "wood", Amount = -5f },
                },
            });

            var pools = _ownerPools.GetOwnerResourcePoolsSnapshot();
            Assert.IsTrue(pools.ContainsKey(Owner));
            Assert.AreEqual(40f, pools[Owner]["wood"],
                "Only the positive entry lands in the owner pool.");
        }

        // ── helpers ──────────────────────────────────────────────────────

        private void RegisterSettlementWithInstantTownHall()
        {
            if (!_buildings.Contains("townhall"))
                _buildings.Add(TownHall(buildTurns: 0));
            if (!_buildings.Contains("farm"))
                _buildings.Add(Farm(buildTurns: 0));
            FirePlaced("townhall", TownHallPos);
            Assert.AreEqual(1, _registry.AllSettlements.Count, "Setup: settlement must exist.");
        }

        private EconomySettlementState Settlement()
        {
            foreach (var pair in _registry.AllSettlements)
                return pair.Value;
            return null;
        }

        private int CountBuildings(string buildingId)
        {
            int count = 0;
            foreach (var building in Settlement().Buildings)
            {
                if (building.BuildingId == buildingId)
                    count++;
            }
            return count;
        }

        private void FirePlaced(string buildingId, Vector2Int position)
            => _signals.Fire(new BuildingPlacedSignal
            {
                BuildingId = buildingId,
                Position = position,
                OwnerId = Owner,
            });

        private void FireOperational(string buildingId, Vector2Int position)
            => _signals.Fire(new BuildingOperationalSignal
            {
                BuildingId = buildingId,
                Position = position,
                OwnerId = Owner,
            });

        private void Tick() => _calendar.FireHourChanged();

        private static BuildingDefinition TownHall(int buildTurns) => new BuildingDefinition
        {
            Id = "townhall",
            DisplayName = "Town Hall",
            BuildTurns = buildTurns,
            Modules = new List<BuildingModuleDefinition>
            {
                new TownHallBuildingModule { IsCentral = true },
            },
        };

        private static BuildingDefinition Farm(int buildTurns) => new BuildingDefinition
        {
            Id = "farm",
            DisplayName = "Farm",
            BuildTurns = buildTurns,
            Modules = new List<BuildingModuleDefinition>
            {
                new ProductionBuildingModule
                {
                    ResourceId = "test-ingot",
                    Recipes = new List<ProductionRecipeDefinition>
                    {
                        new ProductionRecipeDefinition
                        {
                            RecipeId = "ingot",
                            TurnsPerCycle = 1,
                            RequiresWorkers = false,
                            RequiresStorageSpace = false,
                            Outputs = new List<BuildingResourceAmount>
                            {
                                new BuildingResourceAmount
                                {
                                    ResourceId = "test-ingot",
                                    Amount = 5,
                                },
                            },
                        },
                    },
                },
            },
        };

        private sealed class FakeBuildingRegistry : IBuildingRegistry
        {
            private readonly Dictionary<string, BuildingDefinition> _byId =
                new Dictionary<string, BuildingDefinition>(StringComparer.Ordinal);

            public void Add(BuildingDefinition definition) => _byId[definition.Id] = definition;
            public bool Contains(string id) => _byId.ContainsKey(id);
            public BuildingDefinition[] GetAll() => new List<BuildingDefinition>(_byId.Values).ToArray();
            public BuildingDefinition GetById(string id)
                => _byId.TryGetValue(id, out var definition) ? definition : null;
            public BuildingDefinition[] GetByCategory(BuildingCategory category) => Array.Empty<BuildingDefinition>();
            public WallCollectionDefinition[] GetWallCollections() => Array.Empty<WallCollectionDefinition>();
            public WallCollectionDefinition GetWallCollectionByBuildingId(string buildingId) => null;
        }

        private sealed class FakeCalendar : ICalendarService
        {
            private static readonly CalendarConfig TestConfig = new CalendarConfig(
                schemaVersion: 1, startYear: 0, startMonth: 0, startDay: 0, startHour: 0,
                monthsInYear: 12, daysInMonth: 30, hoursInDay: 24,
                dayStartHour: 6, nightStartHour: 20,
                dawnDurationHours: 1, duskDurationHours: 1, hoursPerTurn: 1);

            public GameDateTime Current => default;
            public long TotalHoursSinceEpoch { get; private set; }
            public DayPhase CurrentDayPhase => DayPhase.Day;
            public CalendarConfig Config => TestConfig;

            public event Action OnHourChanged;
            public event Action OnDayChanged;
            public event Action OnMonthChanged;
            public event Action OnYearChanged;
            public event Action<DayPhase> OnDayPhaseChanged;

            public void FireHourChanged()
            {
                TotalHoursSinceEpoch++;
                OnHourChanged?.Invoke();
            }

            public void AdvanceTurn() => FireHourChanged();
            public void SetByTotalHours(long totalHours) => TotalHoursSinceEpoch = totalHours;
        }
    }
}
