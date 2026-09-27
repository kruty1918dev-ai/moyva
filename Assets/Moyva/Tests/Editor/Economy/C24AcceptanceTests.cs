using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Calendar.Config;
using Kruty1918.Calendar.Core;
using Kruty1918.Calendar.Domain;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.Signals;
using Kruty1918.SaveSystem;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.Economy
{
    /// <summary>
    /// C24: a save→real-restart→load roundtrip restores resources, residents,
    /// reservations and mid-production progress without re-granting starter
    /// stock. Deferred apply covers the order where the module loads before
    /// placements are rebuilt.
    /// </summary>
    public sealed class C24AcceptanceTests
    {
        private DiContainer _container;
        private SignalBus _signals;
        private EconomySettlementRegistryService _registry;
        private EconomyManager _economy;
        private EconomySaveModule _module;

        [SetUp]
        public void SetUp()
        {
            _container = new DiContainer();
            Zenject.SignalBusInstaller.Install(_container);
            _container.DeclareSignal<SettlementResourceChangedSignal>().OptionalSubscriber();
            _container.DeclareSignal<BuildingOperationalSignal>().OptionalSubscriber();
            _container.DeclareSignal<BuildingPlacedSignal>().OptionalSubscriber();
            _container.DeclareSignal<BuildingDemolishedSignal>().OptionalSubscriber();
            _container.DeclareSignal<UnitDestroyedSignal>().OptionalSubscriber();
            _container.DeclareSignal<GrantStarterPackResourcesSignal>().OptionalSubscriber();
            _signals = _container.Resolve<SignalBus>();
            _registry = new EconomySettlementRegistryService();
            _economy = new EconomyManager(null, _signals, null, null, null, _registry, null, null);
            _module = new EconomySaveModule(_economy);
        }

        private static EconomySettlementState SeedSettlement(EconomySettlementRegistryService registry)
        {
            var state = new EconomySettlementState
            {
                SettlementId = "s-1",
                OwnerId = "owner-a",
                SettlementName = "Capital",
                IsActive = true,
                CurrentTurn = 12,
            };
            state.Residents.Add(new EconomyResidentState(30, 80, 55f, false));
            state.Residents.Add(new EconomyResidentState(8, 95, 70f, false));
            state.EnsureWarehousePool("10:10");
            state.ResourcePool["wood"] = 40f;
            state.WarehouseResourcePools["10:10"]["wood"] = 40f;
            state.ReserveResourceAt("10:10", "wood", 15f);
            state.WorkerAssignments["mill-0"] = 2;
            state.Buildings.Add(new EconomyBuildingState
            {
                InstanceKey = "mill-0",
                BuildingId = "mill",
                GridPosition = new Vector2Int(3, 3),
                AssignedWorkers = 2,
                ProductionProgress = 0.5f,
                RecipeProgress = { ["grind"] = 1.5f },
            });
            registry.RegisterSettlement(state, new Vector2Int(10, 10));
            return state;
        }

        private static FakeSaveContext WriteContext()
        {
            var stream = new MemoryStream();
            return new FakeSaveContext(stream);
        }

        private static FakeSaveContext ReadContext(MemoryStream stream)
        {
            stream.Position = 0;
            return new FakeSaveContext(stream);
        }

        [Test]
        public void RoundTrip_RestoresResourcesResidentsReservations()
        {
            SeedSettlement(_registry);
            var stream = WriteContext();
            _module.OnSave(stream);
            byte[] payload = stream.Stream.ToArray();
            Assert.Greater(payload.Length, 0);

            // Real restart: new registry + manager + module.
            var registryB = new EconomySettlementRegistryService();
            var economyB = new EconomyManager(null, _signals, null, null, null, registryB, null, null);
            var restored = new EconomySettlementState
            {
                SettlementId = "s-1",
                OwnerId = "owner-a",
                SettlementName = "Capital",
                IsActive = true,
            };
            restored.EnsureWarehousePool("10:10");
            restored.Buildings.Add(new EconomyBuildingState
            {
                InstanceKey = "mill-0",
                BuildingId = "mill",
                GridPosition = new Vector2Int(3, 3),
                RequiredWorkers = 2,
            });
            registryB.RegisterSettlement(restored, new Vector2Int(10, 10));

            var moduleB = new EconomySaveModule(economyB);
            moduleB.OnLoad(ReadContext(new MemoryStream(payload)));

            var state = registryB.GetSettlement("s-1");
            Assert.AreEqual(12, state.CurrentTurn, "Turn counter survives.");
            Assert.AreEqual(2, state.Residents.Count, "Residents survive.");
            Assert.AreEqual(30, state.Residents[0].Age);
            Assert.AreEqual(40f, state.GetResource("wood"), "Pooled stock survives.");
            Assert.AreEqual(40f, state.WarehouseResourcePools["10:10"]["wood"],
                "Warehouse stock survives.");
            Assert.AreEqual(15f, state.GetReservedResourceAt("10:10", "wood"),
                "Reservations survive — in-transit supply is not lost or doubled.");
            Assert.AreEqual(2, state.WorkerAssignments["mill-0"]);
            var building = state.Buildings[0];
            Assert.AreEqual(0.5f, building.ProductionProgress, 0.001f,
                "Mid-production progress survives a restart.");
            Assert.AreEqual(1.5f, building.RecipeProgress["grind"], 0.001f);
            Assert.AreEqual(2, building.AssignedWorkers);
        }

        [Test]
        public void RoundTrip_DoesNotDoubleResources()
        {
            SeedSettlement(_registry);
            var stream = WriteContext();
            _module.OnSave(stream);
            byte[] payload = stream.Stream.ToArray();

            var registryB = new EconomySettlementRegistryService();
            var economyB = new EconomyManager(null, _signals, null, null, null, registryB, null, null);
            var restored = new EconomySettlementState
            {
                SettlementId = "s-1", OwnerId = "owner-a", IsActive = true,
            };
            restored.EnsureWarehousePool("10:10");
            restored.Buildings.Add(new EconomyBuildingState
                { InstanceKey = "mill-0", BuildingId = "mill" });
            registryB.RegisterSettlement(restored, new Vector2Int(10, 10));

            new EconomySaveModule(economyB).OnLoad(ReadContext(new MemoryStream(payload)));
            var once = registryB.GetSettlement("s-1").GetResource("wood");

            // Loading the same payload a second time must overwrite, not add.
            new EconomySaveModule(economyB).OnLoad(ReadContext(new MemoryStream(payload)));
            var twice = registryB.GetSettlement("s-1").GetResource("wood");
            Assert.AreEqual(once, twice, "Re-loading restores, never accumulates.");
            Assert.AreEqual(40f, twice);
        }

        [Test]
        public void Load_BeforeSettlementsRegistered_DeferredUntilPlaced()
        {
            SeedSettlement(_registry);
            var stream = WriteContext();
            _module.OnSave(stream);
            byte[] payload = stream.Stream.ToArray();

            var registryB = new EconomySettlementRegistryService();
            var economyB = new EconomyManager(
                new FakeCalendar(), _signals, null, null, null, registryB, null, null);
            economyB.Initialize();
            try
            {
                var moduleB = new EconomySaveModule(economyB);

                // Load while the world is still empty — snapshot parks as pending.
                moduleB.OnLoad(ReadContext(new MemoryStream(payload)));
                Assert.AreEqual(0, registryB.AllSettlements.Count);

                // Now the world registers the settlement shell + building.
                var shell = new EconomySettlementState
                {
                    SettlementId = "s-1", OwnerId = "owner-a", IsActive = true,
                };
                shell.EnsureWarehousePool("10:10");
                shell.Buildings.Add(new EconomyBuildingState
                    { InstanceKey = "mill-0", BuildingId = "mill" });
                registryB.RegisterSettlement(shell, new Vector2Int(10, 10));

                _signals.Fire(new BuildingOperationalSignal
                {
                    BuildingId = "mill",
                    Position = new Vector2Int(3, 3),
                    OwnerId = "owner-a",
                });

                var state = registryB.GetSettlement("s-1");
                Assert.AreEqual(40f, state.GetResource("wood"),
                    "Deferred snapshot applied once the world existed.");
                Assert.AreEqual(12, state.CurrentTurn);
            }
            finally
            {
                // Initialize() subscribes a static event — leaking it fails the
                // next domain reload inside the test session.
                economyB.Dispose();
            }
        }

        [Test]
        public void UnknownVersion_IgnoredWithoutThrow()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(999); // unsupported schema
            writer.Flush();
            stream.Position = 0;
            Assert.DoesNotThrow(() => _module.OnLoad(new FakeSaveContext(stream)));
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

            public void AdvanceTurn()
            {
                TotalHoursSinceEpoch++;
                OnHourChanged?.Invoke();
            }

            public void SetByTotalHours(long totalHours) => TotalHoursSinceEpoch = totalHours;
        }

        private sealed class FakeSaveContext : ISaveContext
        {
            public FakeSaveContext(MemoryStream stream)
            {
                Stream = stream;
                Writer = stream.CanWrite ? new BinaryWriter(stream) : null;
                Reader = stream.CanRead ? new BinaryReader(stream) : null;
            }
            public MemoryStream Stream { get; }
            public BinaryWriter Writer { get; }
            public BinaryReader Reader { get; }
        }
    }
}
