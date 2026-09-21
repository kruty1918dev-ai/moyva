using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.Units
{
    /// <summary>
    /// Characterization tests for <see cref="UnitsSaveModule"/>: the binary
    /// save format, the pending-until-WorldBuilt ordering and the restore
    /// pipeline that recreates units through IUnitFactory + UnitCreatedSignal.
    /// These lock the save contract before the UnitService storage refactor.
    /// </summary>
    [TestFixture]
    public sealed class UnitsSaveModuleTests
    {
        private const int SaveMagic = unchecked((int)0x554E4954);

        private SignalBus _bus;
        private FakeGrid _grid;
        private FakeTileSettings _tileSettings;
        private FakeUnitClassConfig _configs;
        private FakeObjectsMap _objectsMap;
        private FakeHealthRegistry _health;
        private UnitService _service;
        private SignalFiringUnitFactory _factory;
        private FakeRecruitmentStateStore _recruitment;
        private FakeGroupStateStore _groups;
        private UnitsSaveModule _module;
        private readonly List<IDisposable> _disposables = new();

        [SetUp]
        public void SetUp()
        {
            _bus = CreateSignalBus();
            _grid = new FakeGrid(32, 32);
            _tileSettings = new FakeTileSettings();
            _configs = new FakeUnitClassConfig();
            _objectsMap = new FakeObjectsMap();
            _health = new FakeHealthRegistry();
            _configs.Set("warrior", new UnitClassConfig
            {
                TypeId = "warrior",
                MovementPointsPerTurn = 5f,
                HitPoints = 30,
            });
            _configs.Set("archer", new UnitClassConfig
            {
                TypeId = "archer",
                MovementPointsPerTurn = 4f,
                HitPoints = 20,
            });

            _service = CreateService(_bus, _objectsMap, _health);
            _service.Initialize();
            _disposables.Add(_service);
            _factory = new SignalFiringUnitFactory(_bus);
            _recruitment = new FakeRecruitmentStateStore();
            _groups = new FakeGroupStateStore();
            _module = CreateModule();
            _module.Initialize();
            _disposables.Add(_module);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (IDisposable disposable in _disposables)
                disposable.Dispose();
            _disposables.Clear();
        }

        // ── Round trip ───────────────────────────────────────────────

        [Test]
        public void SaveLoad_RoundTrip_PreservesUnitRecords()
        {
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1", vision: 3);
            FireCreated("u2", "archer", new Vector2Int(5, 6), "p2", vision: 5);
            _service.SetStamina("u1", 2f);
            Assert.IsTrue(_health.TryGet("u1", out IHealth health));
            health.TakeDamage(18); // 30 -> 12

            byte[] payload = CaptureSave();
            World worldB = CreateWorld();
            try
            {
                worldB.Module.OnLoad(ReadContext(payload));
                worldB.Bus.Fire(new WorldBuiltSignal());

                AssertStateMatches(worldB.Service, "u1", "warrior", "p1",
                    new Vector2Int(1, 1), 2f);
                AssertStateMatches(worldB.Service, "u2", "archer", "p2",
                    new Vector2Int(5, 6), 4f);
                Assert.IsTrue(worldB.Health.TryGet("u1", out IHealth restored));
                Assert.AreEqual(12, restored.CurrentHp);
            }
            finally
            {
                worldB.Dispose();
            }
        }

        [Test]
        public void SaveLoad_RoundTrip_PreservesRecruitmentAndGroups()
        {
            _recruitment.Items.Add(new UnitRecruitmentQueueItemSnapshot(
                queueId: 7,
                ownerId: "p1",
                recruitingBuildingPosition: new Vector2Int(3, 3),
                recruitingBuildingId: "bld-1",
                unitTypeId: "warrior",
                completedTurns: 1,
                trainingTurns: 3,
                enqueuedGlobalTurn: 10,
                lastProgressGlobalTurn: 10,
                status: UnitRecruitmentQueueStatus.Training));
            _groups.Items.Add(new UnitGroupSnapshot(
                "grp-1", "p1", new[] { "u1" }));
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1");

            byte[] payload = CaptureSave();
            World worldB = CreateWorld();
            try
            {
                worldB.Module.OnLoad(ReadContext(payload));
                worldB.Bus.Fire(new WorldBuiltSignal());

                Assert.AreEqual(1, worldB.Recruitment.Items.Count);
                UnitRecruitmentQueueItemSnapshot item =
                    worldB.Recruitment.Items[0];
                Assert.AreEqual(7, item.QueueId);
                Assert.AreEqual("p1", item.OwnerId);
                Assert.AreEqual("warrior", item.UnitTypeId);
                Assert.AreEqual(1, item.CompletedTurns);
                Assert.AreEqual(3, item.TrainingTurns);

                Assert.AreEqual(1, worldB.Groups.Items.Count);
                UnitGroupSnapshot group = worldB.Groups.Items[0];
                Assert.AreEqual("grp-1", group.GroupId);
                Assert.AreEqual("p1", group.OwnerId);
                CollectionAssert.AreEqual(new[] { "u1" },
                    group.UnitIds.ToArray());
            }
            finally
            {
                worldB.Dispose();
            }
        }

        [Test]
        public void OnLoad_BeforeWorldBuilt_QueuesRecordsUntilWorldBuilt()
        {
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1");
            byte[] payload = CaptureSave();

            World worldB = CreateWorld();
            try
            {
                worldB.Module.OnLoad(ReadContext(payload));
                Assert.IsEmpty(worldB.Service.GetAllUnitIds());

                worldB.Bus.Fire(new WorldBuiltSignal());
                Assert.IsTrue(worldB.Service.TryGetUnitPosition("u1", out _));
            }
            finally
            {
                worldB.Dispose();
            }
        }

        [Test]
        public void OnLoad_AfterWorldBuilt_SpawnsImmediately()
        {
            FireCreated("u1", "warrior", new Vector2Int(1, 1), "p1");
            byte[] payload = CaptureSave();

            World worldB = CreateWorld();
            try
            {
                worldB.Bus.Fire(new WorldBuiltSignal());
                worldB.Module.OnLoad(ReadContext(payload));

                Assert.IsTrue(worldB.Service.TryGetUnitPosition(
                    "u1", out Vector2Int pos));
                Assert.AreEqual(new Vector2Int(1, 1), pos);
            }
            finally
            {
                worldB.Dispose();
            }
        }

        // ── Format guards ────────────────────────────────────────────

        [Test]
        public void OnLoad_UnsupportedVersion_Throws()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(SaveMagic);
            writer.Write(99);
            writer.Flush();
            stream.Position = 0;

            Assert.Throws<InvalidDataException>(
                () => _module.OnLoad(ReadContext(stream)));
        }

        [Test]
        public void OnLoad_RecordWithEmptyTypeId_IsSkipped()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(SaveMagic);
            writer.Write(6);
            writer.Write(1);            // one unit record
            writer.Write("u-x");        // unitId
            writer.Write(string.Empty); // empty typeId -> skipped on spawn
            writer.Write("p1");
            writer.Write(2);
            writer.Write(3);
            writer.Write(4f);
            writer.Write(7);
            writer.Write(0);            // recruitment queue count
            writer.Write(0);            // group count
            writer.Flush();
            stream.Position = 0;

            World worldB = CreateWorld();
            try
            {
                worldB.Module.OnLoad(ReadContext(stream));
                worldB.Bus.Fire(new WorldBuiltSignal());
                Assert.IsEmpty(worldB.Service.GetAllUnitIds());
                Assert.IsEmpty(worldB.Factory.Created);
            }
            finally
            {
                worldB.Dispose();
            }
        }

        [Test]
        public void OnLoad_LegacyWithStamina_RestoresWithDefaultOwner()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(1); // legacy record count, no magic header
            writer.Write("warrior");
            writer.Write(4);
            writer.Write(5);
            writer.Write(3f);
            writer.Flush();
            stream.Position = 0;

            World worldB = CreateWorld();
            try
            {
                worldB.Module.OnLoad(ReadContext(stream));
                worldB.Bus.Fire(new WorldBuiltSignal());

                string unitId = worldB.Service.GetAllUnitIds().Single();
                AssertStateMatches(worldB.Service, unitId, "warrior",
                    "player_0", new Vector2Int(4, 5), 3f);
            }
            finally
            {
                worldB.Dispose();
            }
        }

        [Test]
        public void OnLoad_LegacyWithoutStamina_UsesConfiguredStartStamina()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);
            writer.Write(1); // legacy record count, no magic header
            writer.Write("warrior");
            writer.Write(4);
            writer.Write(5);
            writer.Flush();
            stream.Position = 0;

            World worldB = CreateWorld();
            try
            {
                worldB.Module.OnLoad(ReadContext(stream));
                worldB.Bus.Fire(new WorldBuiltSignal());

                string unitId = worldB.Service.GetAllUnitIds().Single();
                Assert.AreEqual(5f,
                    worldB.Service.GetStamina(unitId), 0.0001f);
            }
            finally
            {
                worldB.Dispose();
            }
        }

        // ── Helpers ──────────────────────────────────────────────────

        private UnitsSaveModule CreateModule()
        {
            return new UnitsSaveModule(
                _service,
                _factory,
                _service,
                _bus,
                recruitmentState: _recruitment,
                healthRegistry: _health,
                groupState: _groups);
        }

        private World CreateWorld()
        {
            var bus = CreateSignalBus();
            var objectsMap = new FakeObjectsMap();
            var health = new FakeHealthRegistry();
            var service = CreateService(bus, objectsMap, health);
            var factory = new SignalFiringUnitFactory(bus);
            var recruitment = new FakeRecruitmentStateStore();
            var groups = new FakeGroupStateStore();
            var module = new UnitsSaveModule(
                service,
                factory,
                service,
                bus,
                recruitmentState: recruitment,
                healthRegistry: health,
                groupState: groups);
            var world = new World(
                bus, service, factory, recruitment, groups, health, module);
            _disposables.Add(world);
            return world;
        }

        private UnitService CreateService(
            SignalBus bus,
            IObjectsMapService objectsMap,
            IHealthRegistry health)
        {
            return new UnitService(
                bus,
                _grid,
                _tileSettings,
                _configs,
                objectsMap,
                healthRegistry: health,
                buildingRegistry: null,
                gateStateService: null);
        }

        private byte[] CaptureSave()
        {
            var stream = new MemoryStream();
            _module.OnSave(new FakeSaveContext(stream));
            return stream.ToArray();
        }

        private static ISaveContext ReadContext(byte[] payload)
            => new FakeSaveContext(new MemoryStream(payload, writable: false));

        private static ISaveContext ReadContext(MemoryStream stream)
            => new FakeSaveContext(stream);

        private void FireCreated(
            string unitId,
            string typeId,
            Vector2Int position,
            string ownerId,
            int vision = 3)
        {
            _bus.Fire(new UnitCreatedSignal
            {
                UnitId = unitId,
                UnitTypeId = typeId,
                Position = position,
                VisionRange = vision,
                OwnerId = ownerId,
            });
        }

        private static void AssertStateMatches(
            UnitService service,
            string unitId,
            string typeId,
            string ownerId,
            Vector2Int position,
            float stamina)
        {
            Assert.IsTrue(service.TryGetUnitPosition(unitId, out Vector2Int pos));
            Assert.AreEqual(position, pos);
            Assert.AreEqual(typeId, service.GetUnitTypeId(unitId));
            Assert.AreEqual(ownerId, service.GetUnitOwnerId(unitId));
            Assert.AreEqual(stamina, service.GetStamina(unitId), 0.0001f);
        }

        private static SignalBus CreateSignalBus()
        {
            var container = new DiContainer();
            Zenject.SignalBusInstaller.Install(container);
            container.DeclareSignal<UnitCreatedSignal>();
            container.DeclareSignal<UnitMovedSignal>();
            container.DeclareSignal<UnitDestroyedSignal>();
            container.DeclareSignal<UnitGarrisonStateChangedSignal>()
                .OptionalSubscriber();
            container.DeclareSignal<InterruptMovementSignal>()
                .OptionalSubscriber();
            container.DeclareSignal<WorldBuiltSignal>();
            return container.Resolve<SignalBus>();
        }

        // ── Fakes ────────────────────────────────────────────────────

        private sealed class World : IDisposable
        {
            public readonly SignalBus Bus;
            public readonly UnitService Service;
            public readonly SignalFiringUnitFactory Factory;
            public readonly FakeRecruitmentStateStore Recruitment;
            public readonly FakeGroupStateStore Groups;
            public readonly FakeHealthRegistry Health;
            public readonly UnitsSaveModule Module;

            public World(
                SignalBus bus,
                UnitService service,
                SignalFiringUnitFactory factory,
                FakeRecruitmentStateStore recruitment,
                FakeGroupStateStore groups,
                FakeHealthRegistry health,
                UnitsSaveModule module)
            {
                Bus = bus;
                Service = service;
                Factory = factory;
                Recruitment = recruitment;
                Groups = groups;
                Health = health;
                Module = module;
                service.Initialize();
                module.Initialize();
            }

            public void Dispose()
            {
                Module.Dispose();
                Service.Dispose();
            }
        }

        private sealed class FakeSaveContext : ISaveContext
        {
            public FakeSaveContext(MemoryStream stream)
            {
                Writer = stream.CanWrite ? new BinaryWriter(stream) : null;
                Reader = stream.CanRead ? new BinaryReader(stream) : null;
            }

            public BinaryWriter Writer { get; }
            public BinaryReader Reader { get; }
        }

        private sealed class SignalFiringUnitFactory : IUnitFactory
        {
            private readonly SignalBus _bus;
            private int _counter;

            public readonly List<string> Created = new();

            public SignalFiringUnitFactory(SignalBus bus) => _bus = bus;

            public string CreateUnit(string typeId, Vector2Int gridPosition)
                => CreateUnit(typeId, gridPosition, null);

            public string CreateUnit(
                string typeId, Vector2Int gridPosition, string ownerId)
                => Spawn($"{typeId}-{_counter++:D2}", typeId, gridPosition,
                    ownerId);

            public string CreateUnitWithId(
                string forcedUnitId,
                string typeId,
                Vector2Int gridPosition,
                string ownerId)
                => Spawn(forcedUnitId, typeId, gridPosition, ownerId);

            private string Spawn(
                string unitId, string typeId, Vector2Int position, string ownerId)
            {
                _bus.Fire(new UnitCreatedSignal
                {
                    UnitId = unitId,
                    UnitTypeId = typeId,
                    Position = position,
                    VisionRange = 3,
                    OwnerId = ownerId,
                });
                Created.Add(unitId);
                return unitId;
            }
        }

        private sealed class FakeRecruitmentStateStore : IUnitRecruitmentStateStore
        {
            public readonly List<UnitRecruitmentQueueItemSnapshot> Items = new();
            public int RestoreCalls;

            public IReadOnlyList<UnitRecruitmentQueueItemSnapshot> CaptureState()
                => Items;

            public void RestoreState(
                IReadOnlyList<UnitRecruitmentQueueItemSnapshot> items)
            {
                RestoreCalls++;
                Items.Clear();
                Items.AddRange(items);
            }
        }

        private sealed class FakeGroupStateStore : IUnitGroupStateStore
        {
            public readonly List<UnitGroupSnapshot> Items = new();
            public int RestoreCalls;

            public IReadOnlyList<UnitGroupSnapshot> CaptureState() => Items;

            public void RestoreState(IReadOnlyList<UnitGroupSnapshot> groups)
            {
                RestoreCalls++;
                Items.Clear();
                Items.AddRange(groups);
            }

            public void RestoreReplicated(
                IReadOnlyList<UnitGroupSnapshot> groups)
            {
                Items.Clear();
                Items.AddRange(groups);
            }
        }

        private sealed class FakeGrid : IGridService
        {
            public FakeGrid(int width, int height)
            {
                GridWidth = width;
                GridHeight = height;
            }

            public int GridWidth { get; }
            public int GridHeight { get; }
            public string GetTileData(Vector2Int position) => "grass";

            public bool TryGetTileData(Vector2Int position, out string tileTypeId)
            {
                if (position.x < 0 || position.y < 0
                    || position.x >= GridWidth || position.y >= GridHeight)
                {
                    tileTypeId = null;
                    return false;
                }
                tileTypeId = "grass";
                return true;
            }

            public void SetTileData(Vector2Int position, string tileTypeId) { }
        }

        private sealed class FakeTileSettings : ITileSettingsService
        {
            public float GetTileWeight(string tileId) => 1f;
            public bool IsBuildBlocked(string tileId) => false;
            public float GetSurfaceOffset(string tileId) => 0f;
        }

        private sealed class FakeUnitClassConfig : IUnitClassConfig
        {
            private readonly Dictionary<string, UnitClassConfig> _configs = new();
            public void Set(string typeId, UnitClassConfig config)
                => _configs[typeId] = config;
            public UnitClassConfig GetConfig(string typeId)
                => _configs.TryGetValue(typeId, out UnitClassConfig config)
                    ? config
                    : null;
        }

        private sealed class FakeObjectsMap : IObjectsMapService
        {
            private readonly Dictionary<Vector2Int, string> _occupants = new();

            public bool IsOccupied(Vector2Int position)
                => _occupants.ContainsKey(position);

            public bool TryGetOccupant(Vector2Int position, out string occupantId)
                => _occupants.TryGetValue(position, out occupantId);

            public void Register(Vector2Int position, string occupantId)
                => _occupants[position] = occupantId;

            public void Move(Vector2Int from, Vector2Int to)
            {
                if (_occupants.TryGetValue(from, out string occupantId))
                {
                    _occupants.Remove(from);
                    _occupants[to] = occupantId;
                }
            }

            public void Unregister(Vector2Int position)
                => _occupants.Remove(position);

            public bool TryGetPosition(string occupantId, out Vector2Int position)
            {
                foreach (var pair in _occupants)
                {
                    if (pair.Value == occupantId)
                    {
                        position = pair.Key;
                        return true;
                    }
                }
                position = default;
                return false;
            }
        }

        private sealed class FakeHealthRegistry : IHealthRegistry
        {
            private readonly Dictionary<string, IHealth> _entries =
                new(StringComparer.Ordinal);

            public void Register(IHealth health)
            {
                if (health != null)
                    _entries[health.EntityId] = health;
            }

            public void Unregister(string entityId) => _entries.Remove(entityId);

            public IHealth Get(string entityId)
                => TryGet(entityId, out IHealth health) ? health : null;

            public bool TryGet(string entityId, out IHealth health)
            {
                if (_entries.TryGetValue(entityId, out IHealth found)
                    && !found.IsDestroyed)
                {
                    health = found;
                    return true;
                }
                health = null;
                return false;
            }

            public IReadOnlyCollection<IHealth> GetAll()
                => _entries.Values
                    .Where(h => !h.IsDestroyed)
                    .ToArray();

            public IReadOnlyCollection<IHealth> GetMany(
                IEnumerable<string> entityIds)
                => entityIds?
                        .Select(id => Get(id))
                        .Where(h => h != null)
                        .ToArray()
                    ?? (IReadOnlyCollection<IHealth>)Array.Empty<IHealth>();

            public int Count => _entries.Count;
        }
    }
}
