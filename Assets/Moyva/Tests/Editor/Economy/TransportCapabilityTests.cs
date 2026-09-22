using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using NUnit.Framework;
using UnityEngine;
using Zenject;
using Kruty1918.EntityHealth;

namespace Kruty1918.Moyva.Tests.Economy
{
    [TestFixture]
    public class TransportCapabilityTests
    {
        private FakeUnitService _units;
        private FakeUnitClasses _classes;
        private FakeHealthRegistry _health;
        private FakeOwnership _ownership;

        [SetUp]
        public void SetUp()
        {
            _units = new FakeUnitService();
            _classes = new FakeUnitClasses();
            _health = new FakeHealthRegistry();
            _ownership = new FakeOwnership();

            _classes.Add("caravan-wagon", new UnitClassConfig
            {
                TypeId = "caravan-wagon",
                Role = UnitRole.Worker,
                CanTransportCargo = true,
                CargoCapacity = 120f,
                CrushingDamage = 0,
                AttackRange = 1,
            });
            _classes.Add("raider", new UnitClassConfig
            {
                TypeId = "raider",
                Role = UnitRole.Military,
                CuttingDamage = 5,
                AttackRange = 1,
            });
        }

        [Test]
        public void TransportUnit_CannotAttackEnemy()
        {
            _units.Add("wagon-1", new Vector2Int(0, 0), "caravan-wagon");
            _units.Add("enemy-1", new Vector2Int(1, 0), "raider");
            _health.Add(new FakeHealth("wagon-1", 40));
            _health.Add(new FakeHealth("enemy-1", 30));
            _ownership.SetOwner("wagon-1", "player_0");
            _ownership.SetOwner("enemy-1", "player_1");

            var combat = new UnitCombatService(_units, _classes, _health, _ownership);

            Assert.IsFalse(combat.CanAttack("wagon-1", "enemy-1", out var reason));
            Assert.AreEqual(UnitAttackRejectReason.AttackerNotCombatCapable, reason);
        }

        [Test]
        public void MilitaryUnit_StillCanAttackEnemy()
        {
            _units.Add("raider-1", new Vector2Int(0, 0), "raider");
            _units.Add("enemy-1", new Vector2Int(1, 0), "raider");
            _health.Add(new FakeHealth("raider-1", 30));
            _health.Add(new FakeHealth("enemy-1", 30));
            _ownership.SetOwner("raider-1", "player_0");
            _ownership.SetOwner("enemy-1", "player_1");

            var combat = new UnitCombatService(_units, _classes, _health, _ownership);

            Assert.IsTrue(combat.CanAttack("raider-1", "enemy-1", out _));
        }

        [Test]
        public void TransportUnit_CannotCaptureSettlement()
        {
            _units.Add("wagon-1", new Vector2Int(0, 0), "caravan-wagon");
            _ownership.SetOwner("wagon-1", "player_0");

            var container = new DiContainer();
            SignalBusInstaller.Install(container);
            var registry = new EconomySettlementRegistryService();
            var transfer = new FakeOwnershipTransfer();
            container.Bind<IUnitService>().FromInstance(_units);
            container.Bind<IUnitOwnershipQuery>().FromInstance(_ownership);
            container.Bind<IUnitClassConfig>().FromInstance(_classes);
            container.Bind<Kruty1918.Moyva.Turns.API.ITurnService>().FromInstance(new FakeTurns());
            container.Bind<Kruty1918.Moyva.Turns.API.ITurnAuthorityPolicy>().FromInstance(new FakeAuthority());
            container.Bind<Kruty1918.Moyva.FogOfWar.API.IFogOwnerStateReader>().FromInstance(new FakeFog());
            container.Bind<Kruty1918.Moyva.Construction.API.IConstructionBuildingCombatTargetQuery>()
                .FromInstance(new FakeCombatTargets());
            container.Bind<Kruty1918.Moyva.Construction.API.IBuildingRegistry>()
                .FromInstance(new FakeBuildingRegistry());
            container.Bind<Kruty1918.Moyva.Combat.API.IHealthRegistry>().FromInstance(_health);

            var capture = new SettlementCaptureService(registry, transfer, container.Resolve<SignalBus>());
            container.Inject(capture);

            bool allowed = capture.TryEvaluateCapture("player_0", "wagon-1", "castle-1",
                new Vector2Int(1, 0), out _, out string reason);

            Assert.IsFalse(allowed);
            Assert.AreEqual("Only a military unit can capture a settlement.", reason);
        }
    }
}
