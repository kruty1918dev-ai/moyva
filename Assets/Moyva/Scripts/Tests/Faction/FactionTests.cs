using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Faction.API;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.Faction
{
    [TestFixture]
    public sealed class FactionIdTests
    {
        [Test]
        public void Constructor_WithValue_SetsValue()
        {
            var id = new FactionId("player_0");
            Assert.AreEqual("player_0", id.Value);
        }

        [Test]
        public void Constructor_WithNull_SetsEmpty()
        {
            var id = new FactionId(null);
            Assert.AreEqual(string.Empty, id.Value);
        }

        [Test]
        public void Empty_IsEmpty_ReturnsTrue()
        {
            Assert.IsTrue(FactionId.Empty.IsEmpty);
        }

        [Test]
        public void NonEmpty_IsEmpty_ReturnsFalse()
        {
            var id = new FactionId("bot_0");
            Assert.IsFalse(id.IsEmpty);
        }

        [Test]
        public void EmptyString_IsEmpty_ReturnsTrue()
        {
            var id = new FactionId("");
            Assert.IsTrue(id.IsEmpty);
        }

        [Test]
        public void Equals_SameValue_ReturnsTrue()
        {
            var a = new FactionId("player_0");
            var b = new FactionId("player_0");
            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void Equals_DifferentValue_ReturnsFalse()
        {
            var a = new FactionId("player_0");
            var b = new FactionId("player_1");
            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void OperatorEquals_SameValue_ReturnsTrue()
        {
            var a = new FactionId("x");
            var b = new FactionId("x");
            Assert.IsTrue(a == b);
        }

        [Test]
        public void OperatorNotEquals_DifferentValue_ReturnsTrue()
        {
            var a = new FactionId("x");
            var b = new FactionId("y");
            Assert.IsTrue(a != b);
        }

        [Test]
        public void GetHashCode_SameValue_SameHash()
        {
            var a = new FactionId("test");
            var b = new FactionId("test");
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void ImplicitToString_ReturnsValue()
        {
            var id = new FactionId("bot_1");
            string s = id;
            Assert.AreEqual("bot_1", s);
        }

        [Test]
        public void ExplicitFromString_ReturnsFactionId()
        {
            var id = (FactionId)"player_2";
            Assert.AreEqual("player_2", id.Value);
        }

        [Test]
        public void ToString_ReturnsValue()
        {
            var id = new FactionId("abc");
            Assert.AreEqual("abc", id.ToString());
        }

        [Test]
        public void Equals_Object_SameValue_ReturnsTrue()
        {
            var a = new FactionId("x");
            object b = new FactionId("x");
            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void Equals_Object_DifferentType_ReturnsFalse()
        {
            var a = new FactionId("x");
            Assert.IsFalse(a.Equals("x"));
        }

        [Test]
        public void Null_GetHashCode_ReturnsZero()
        {
            var id = FactionId.Empty;
            Assert.AreEqual(string.Empty.GetHashCode(), id.GetHashCode());
        }
    }

    [TestFixture]
    public sealed class FactionDefinitionTests
    {
        [Test]
        public void Constructor_SetsAllProperties()
        {
            var def = new FactionDefinition(
                new FactionId("p0"), FactionType.Human, "warrior",
                new Vector2Int(5, 10), Color.red);

            Assert.AreEqual("p0", def.FactionId.Value);
            Assert.AreEqual(FactionType.Human, def.FactionType);
            Assert.AreEqual("warrior", def.DefaultUnitTypeId);
            Assert.AreEqual(new Vector2Int(5, 10), def.StartPosition);
            Assert.AreEqual(Color.red, def.TeamColor);
        }

        [Test]
        public void Constructor_BotType_SetsCorrectly()
        {
            var def = new FactionDefinition(
                new FactionId("b0"), FactionType.Bot, "scout",
                Vector2Int.zero, Color.blue);

            Assert.AreEqual(FactionType.Bot, def.FactionType);
        }

        [Test]
        public void Constructor_NetworkType_SetsCorrectly()
        {
            var def = new FactionDefinition(
                new FactionId("n0"), FactionType.Network, "archer",
                Vector2Int.one, Color.green);

            Assert.AreEqual(FactionType.Network, def.FactionType);
        }
    }

    [TestFixture]
    public sealed class FactionRegistryTests
    {
        private static FactionDefinition MakeDef(string id, FactionType type)
        {
            return new FactionDefinition(new FactionId(id), type, "warrior", Vector2Int.zero, Color.white);
        }

        private IFactionRegistry CreateRegistry(params FactionDefinition[] defs)
        {
            // FactionRegistry is internal, access through reflection (same pattern as ConstructionServiceTests)
            var type = typeof(IFactionRegistry).Assembly.GetType("Kruty1918.Moyva.Faction.Runtime.FactionRegistry");
            Assert.IsNotNull(type, "FactionRegistry type not found");
            var instance = System.Activator.CreateInstance(type, new object[] { defs.ToList() as IEnumerable<FactionDefinition> });
            return (IFactionRegistry)instance;
        }

        [Test]
        public void GetAll_ReturnsAllDefinitions()
        {
            var reg = CreateRegistry(MakeDef("p0", FactionType.Human), MakeDef("b0", FactionType.Bot));
            Assert.AreEqual(2, reg.GetAll().Count);
        }

        [Test]
        public void GetAll_EmptyList_ReturnsEmpty()
        {
            var reg = CreateRegistry();
            Assert.AreEqual(0, reg.GetAll().Count);
        }

        [Test]
        public void GetBotFactions_ReturnsOnlyBots()
        {
            var reg = CreateRegistry(
                MakeDef("p0", FactionType.Human),
                MakeDef("b0", FactionType.Bot),
                MakeDef("b1", FactionType.Bot));
            Assert.AreEqual(2, reg.GetBotFactions().Count);
        }

        [Test]
        public void GetBotFactions_NoBots_ReturnsEmpty()
        {
            var reg = CreateRegistry(MakeDef("p0", FactionType.Human));
            Assert.AreEqual(0, reg.GetBotFactions().Count);
        }

        [Test]
        public void LocalPlayerFaction_ReturnsFirstHuman()
        {
            var reg = CreateRegistry(
                MakeDef("b0", FactionType.Bot),
                MakeDef("p0", FactionType.Human));
            Assert.IsNotNull(reg.LocalPlayerFaction);
            Assert.AreEqual("p0", reg.LocalPlayerFaction.FactionId.Value);
        }

        [Test]
        public void LocalPlayerFaction_NoHumans_ReturnsNull()
        {
            var reg = CreateRegistry(MakeDef("b0", FactionType.Bot));
            Assert.IsNull(reg.LocalPlayerFaction);
        }

        [Test]
        public void TryGet_ExistingId_ReturnsTrue()
        {
            var reg = CreateRegistry(MakeDef("p0", FactionType.Human));
            Assert.IsTrue(reg.TryGet(new FactionId("p0"), out var def));
            Assert.AreEqual("p0", def.FactionId.Value);
        }

        [Test]
        public void TryGet_NonExistingId_ReturnsFalse()
        {
            var reg = CreateRegistry(MakeDef("p0", FactionType.Human));
            Assert.IsFalse(reg.TryGet(new FactionId("unknown"), out _));
        }

        [Test]
        public void TryGet_EmptyId_ReturnsFalse()
        {
            var reg = CreateRegistry(MakeDef("p0", FactionType.Human));
            Assert.IsFalse(reg.TryGet(FactionId.Empty, out _));
        }

        [Test]
        public void GetAll_PreservesOrder()
        {
            var reg = CreateRegistry(
                MakeDef("a", FactionType.Human),
                MakeDef("b", FactionType.Bot),
                MakeDef("c", FactionType.Network));

            var all = reg.GetAll();
            Assert.AreEqual("a", all[0].FactionId.Value);
            Assert.AreEqual("b", all[1].FactionId.Value);
            Assert.AreEqual("c", all[2].FactionId.Value);
        }

        [Test]
        public void GetBotFactions_DoesNotIncludeNetworkType()
        {
            var reg = CreateRegistry(
                MakeDef("n0", FactionType.Network),
                MakeDef("b0", FactionType.Bot));

            var bots = reg.GetBotFactions();
            Assert.AreEqual(1, bots.Count);
            Assert.AreEqual("b0", bots[0].FactionId.Value);
        }
    }

    [TestFixture]
    public sealed class FactionOwnershipServiceTests
    {
        private Zenject.DiContainer _container;
        private IFactionOwnershipService _service;
        private Zenject.SignalBus _signalBus;

        [SetUp]
        public void SetUp()
        {
            _container = new Zenject.DiContainer();
            Zenject.SignalBusInstaller.Install(_container);
            _container.DeclareSignal<Signals.UnitCreatedSignal>();
            _container.DeclareSignal<Signals.UnitDestroyedSignal>();

            var type = typeof(IFactionOwnershipService).Assembly
                .GetType("Kruty1918.Moyva.Faction.Runtime.FactionOwnershipService");
            _container.BindInterfacesAndSelfTo(type).AsSingle().NonLazy();
            _container.ResolveRoots();

            _signalBus = _container.Resolve<Zenject.SignalBus>();
            _service = _container.Resolve<IFactionOwnershipService>();
            (_service as Zenject.IInitializable)?.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            (_service as System.IDisposable)?.Dispose();
        }

        [Test]
        public void Register_SetsOwner()
        {
            _service.Register("unit_1", new FactionId("p0"));
            Assert.AreEqual("p0", _service.GetOwner("unit_1").Value);
        }

        [Test]
        public void GetOwner_UnknownUnit_ReturnsEmpty()
        {
            Assert.IsTrue(_service.GetOwner("unknown").IsEmpty);
        }

        [Test]
        public void Unregister_RemovesOwnership()
        {
            _service.Register("unit_1", new FactionId("p0"));
            _service.Unregister("unit_1");
            Assert.IsTrue(_service.GetOwner("unit_1").IsEmpty);
        }

        [Test]
        public void GetUnitIds_ReturnsRegisteredUnits()
        {
            _service.Register("u1", new FactionId("p0"));
            _service.Register("u2", new FactionId("p0"));
            var ids = _service.GetUnitIds(new FactionId("p0"));
            Assert.AreEqual(2, ids.Count);
            Assert.IsTrue(ids.Contains("u1"));
            Assert.IsTrue(ids.Contains("u2"));
        }

        [Test]
        public void GetUnitIds_EmptyFaction_ReturnsEmpty()
        {
            var ids = _service.GetUnitIds(new FactionId("p0"));
            Assert.AreEqual(0, ids.Count);
        }

        [Test]
        public void Register_SameFaction_NoDuplicates()
        {
            _service.Register("u1", new FactionId("p0"));
            _service.Register("u1", new FactionId("p0"));
            Assert.AreEqual(1, _service.GetUnitIds(new FactionId("p0")).Count);
        }

        [Test]
        public void Register_DifferentFaction_MovesUnit()
        {
            _service.Register("u1", new FactionId("p0"));
            _service.Register("u1", new FactionId("p1"));

            Assert.AreEqual("p1", _service.GetOwner("u1").Value);
            Assert.AreEqual(0, _service.GetUnitIds(new FactionId("p0")).Count);
            Assert.AreEqual(1, _service.GetUnitIds(new FactionId("p1")).Count);
        }

        [Test]
        public void Register_NullUnitId_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _service.Register(null, new FactionId("p0")));
        }

        [Test]
        public void Register_EmptyFactionId_DoesNotRegister()
        {
            _service.Register("u1", FactionId.Empty);
            Assert.IsTrue(_service.GetOwner("u1").IsEmpty);
        }

        [Test]
        public void Unregister_UnknownUnit_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _service.Unregister("unknown"));
        }

        [Test]
        public void UnitCreatedSignal_RegistersOwnership()
        {
            _signalBus.Fire(new Signals.UnitCreatedSignal
            {
                UnitId = "u1",
                OwnerId = "p0"
            });
            Assert.AreEqual("p0", _service.GetOwner("u1").Value);
        }

        [Test]
        public void UnitCreatedSignal_NullOwnerId_DoesNotRegister()
        {
            _signalBus.Fire(new Signals.UnitCreatedSignal
            {
                UnitId = "u1",
                OwnerId = null
            });
            Assert.IsTrue(_service.GetOwner("u1").IsEmpty);
        }

        [Test]
        public void UnitCreatedSignal_EmptyOwnerId_DoesNotRegister()
        {
            _signalBus.Fire(new Signals.UnitCreatedSignal
            {
                UnitId = "u1",
                OwnerId = ""
            });
            Assert.IsTrue(_service.GetOwner("u1").IsEmpty);
        }

        [Test]
        public void UnitDestroyedSignal_UnregistersUnit()
        {
            _service.Register("u1", new FactionId("p0"));
            _signalBus.Fire(new Signals.UnitDestroyedSignal { UnitId = "u1" });
            Assert.IsTrue(_service.GetOwner("u1").IsEmpty);
        }

        [Test]
        public void UnitDestroyedSignal_UnknownUnit_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                _signalBus.Fire(new Signals.UnitDestroyedSignal { UnitId = "unknown" }));
        }

        [Test]
        public void MultipleUnits_MultipleFactions_IndependentTracking()
        {
            _service.Register("u1", new FactionId("p0"));
            _service.Register("u2", new FactionId("p1"));
            _service.Register("u3", new FactionId("p0"));

            Assert.AreEqual(2, _service.GetUnitIds(new FactionId("p0")).Count);
            Assert.AreEqual(1, _service.GetUnitIds(new FactionId("p1")).Count);
            Assert.AreEqual("p0", _service.GetOwner("u1").Value);
            Assert.AreEqual("p1", _service.GetOwner("u2").Value);
        }
    }
}
