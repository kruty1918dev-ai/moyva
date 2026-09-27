using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.Editor.Economy
{
    /// <summary>
    /// C19: a route carries real cargo between settlements conserving every
    /// gram — load debits the source warehouse/pool, the wagon holds the
    /// physical stock in transit, unload credits the destination. Capacity,
    /// reservations, whitelist, ownership, interruption and retry covered.
    /// </summary>
    public sealed class C19AcceptanceTests
    {
        private const string Owner = "owner-a";
        private const string WagonId = "wagon-1";
        private const string SourceWh = "10:10";
        private const string TargetWh = "40:40";

        private DiContainer _container;
        private CaravanService _service;
        private EconomySettlementRegistryService _registry;
        private C19Gameplay _gameplay;
        private EconomySettlementState _source;
        private EconomySettlementState _target;

        [SetUp]
        public void SetUp()
        {
            _registry = new EconomySettlementRegistryService();
            _gameplay = new C19Gameplay();

            _source = Settlement("s-src", Owner);
            _source.WarehouseResourcePools[SourceWh] = new Dictionary<string, float> { ["wood"] = 100f };
            _source.ResourcePool["wood"] = 100f;
            _registry.RegisterSettlement(_source, new Vector2Int(10, 10));

            _target = Settlement("s-dst", Owner);
            _target.WarehouseResourcePools[TargetWh] = new Dictionary<string, float>();
            _registry.RegisterSettlement(_target, new Vector2Int(40, 40));

            _container = new DiContainer();
            Zenject.SignalBusInstaller.Install(_container);
            _container.DeclareSignal<SettlementResourceChangedSignal>();
            _container.DeclareSignal<CaravanDeliveryCompletedSignal>();
            _container.Bind<EconomySettlementRegistryService>().FromInstance(_registry);
            _container.Bind<ICaravanGameplayAccess>().FromInstance(_gameplay);
            _service = _container.Instantiate<CaravanService>();
        }



        private static EconomySettlementState Settlement(string id, string owner)
            => new EconomySettlementState
            {
                SettlementId = id, OwnerId = owner, IsActive = true,
            };

        private CaravanCargoRequest Load(string res, float amount,
            string settlement = "s-src", string warehouse = SourceWh)
            => new CaravanCargoRequest(Owner, WagonId, CaravanCargoOperation.Load,
                settlement, warehouse, new Dictionary<string, float> { [res] = amount });

        private CaravanCargoRequest Unload(string res, float amount,
            string settlement = "s-dst", string warehouse = TargetWh)
            => new CaravanCargoRequest(Owner, WagonId, CaravanCargoOperation.Unload,
                settlement, warehouse, new Dictionary<string, float> { [res] = amount });

        [Test]
        public void LoadThenUnload_ConservesAmount()
        {
            _gameplay.Wagon(WagonId, Owner, capacity: 100f, new Vector2Int(10, 10));

            Assert.IsTrue(_service.Execute(Load("wood", 40f)).Succeeded);
            Assert.AreEqual(60f, _source.WarehouseResourcePools[SourceWh]["wood"], 0.001f);
            Assert.AreEqual(60f, _source.GetResource("wood"), 0.001f);
            Assert.IsTrue(_service.TryGetCargo(Owner, WagonId, out var cargo));
            Assert.AreEqual(40f, cargo.Resources["wood"], 0.001f);

            _gameplay.Move(WagonId, new Vector2Int(40, 40));
            Assert.IsTrue(_service.Execute(Unload("wood", 40f)).Succeeded);
            Assert.AreEqual(40f, _target.WarehouseResourcePools[TargetWh]["wood"], 0.001f);
            Assert.AreEqual(40f, _target.GetResource("wood"), 0.001f);
            Assert.IsTrue(_service.TryGetCargo(Owner, WagonId, out cargo));
            Assert.IsFalse(cargo.Resources.ContainsKey("wood"), "Wagon emptied after unload.");
        }

        [Test]
        public void Load_BeyondStock_Rejected_NoPartialMutation()
        {
            _gameplay.Wagon(WagonId, Owner, capacity: 100f, new Vector2Int(10, 10));
            var batch = new CaravanCargoRequest(Owner, WagonId, CaravanCargoOperation.Load,
                "s-src", SourceWh, new Dictionary<string, float> { ["wood"] = 60f, ["stone"] = 10f });
            var result = _service.Execute(batch);
            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(100f, _source.WarehouseResourcePools[SourceWh]["wood"],
                "Rejected batch must not partially drain the warehouse.");
        }

        [Test]
        public void ReservedStock_IsNotLoadable()
        {
            _gameplay.Wagon(WagonId, Owner, capacity: 100f, new Vector2Int(10, 10));
            _source.ReserveResourceAt(SourceWh, "wood", 80f);
            var result = _service.Execute(Load("wood", 30f));
            Assert.IsFalse(result.Succeeded, "Only 20 wood is unreserved.");
            Assert.AreEqual(100f, _source.GetResource("wood"));
        }

        [Test]
        public void WagonCapacity_BlocksOverload()
        {
            _gameplay.Wagon(WagonId, Owner, capacity: 50f, new Vector2Int(10, 10));
            Assert.IsFalse(_service.Execute(Load("wood", 60f)).Succeeded);
            Assert.IsTrue(_service.Execute(Load("wood", 50f)).Succeeded);
        }

        [Test]
        public void DestinationCapacity_BlocksUnload_CargoKept()
        {
            _gameplay.Wagon(WagonId, Owner, capacity: 100f, new Vector2Int(10, 10));
            _target.WarehousePolicies[TargetWh] = new EconomyBuildingWarehousePolicy { Capacity = 10 };
            _service.Execute(Load("wood", 40f));
            _gameplay.Move(WagonId, new Vector2Int(40, 40));
            Assert.IsFalse(_service.Execute(Unload("wood", 40f)).Succeeded);
            Assert.IsTrue(_service.TryGetCargo(Owner, WagonId, out var cargo));
            Assert.AreEqual(40f, cargo.Resources["wood"], "Rejected unload keeps the cargo on the wagon.");
        }

        [Test]
        public void DestinationWhitelist_BlocksUnload()
        {
            _gameplay.Wagon(WagonId, Owner, capacity: 100f, new Vector2Int(10, 10));
            _target.WarehousePolicies[TargetWh] = new EconomyBuildingWarehousePolicy
            { Capacity = -1, AcceptedResourceIds = new[] { "stone" } };
            _service.Execute(Load("wood", 10f));
            _gameplay.Move(WagonId, new Vector2Int(40, 40));
            Assert.IsFalse(_service.Execute(Unload("wood", 10f)).Succeeded);
        }

        [Test]
        public void ForeignWagon_Rejected()
        {
            _gameplay.Wagon(WagonId, "enemy", capacity: 100f, new Vector2Int(10, 10));
            var result = _service.Execute(Load("wood", 10f));
            Assert.IsFalse(result.Succeeded);
        }

        [Test]
        public void ForeignSettlement_Rejected()
        {
            _gameplay.Wagon(WagonId, Owner, capacity: 100f, new Vector2Int(10, 10));
            var enemy = Settlement("s-enemy", "enemy");
            enemy.WarehouseResourcePools["1:1"] = new Dictionary<string, float> { ["wood"] = 50f };
            _registry.RegisterSettlement(enemy, new Vector2Int(1, 1));
            var result = _service.Execute(Load("wood", 10f, "s-enemy", "1:1"));
            Assert.IsFalse(result.Succeeded);
        }

        [Test]
        public void Route_LoadArriveUnload_RunsEndToEnd()
        {
            _gameplay.Wagon(WagonId, Owner, capacity: 100f, new Vector2Int(10, 10));
            var route = new CaravanRouteRequest(Owner, WagonId,
                "s-src", SourceWh, "s-dst", TargetWh,
                new Dictionary<string, float> { ["wood"] = 40f }, repeat: false);

            Assert.IsTrue(_service.SetRoute(route).Succeeded);
            _service.Tick(); // synchronous fake movement completes both legs inline

            Assert.IsTrue(_service.TryGetRoute(Owner, WagonId, out var snapshot));
            Assert.AreEqual(CaravanRoutePhase.Completed, snapshot.Phase);
            Assert.AreEqual(60f, _source.WarehouseResourcePools[SourceWh]["wood"], 0.001f);
            Assert.AreEqual(40f, _target.WarehouseResourcePools[TargetWh]["wood"], 0.001f);
            Assert.AreEqual(100f,
                _source.GetResource("wood") + _target.GetResource("wood") + WagonWood(),
                "Total stock conserved across the whole route.");
        }

        [Test]
        public void Route_Interruption_KeepsCargoOnWagon()
        {
            _gameplay.Wagon(WagonId, Owner, capacity: 100f, new Vector2Int(10, 10));
            _gameplay.HangSecondMove = true; // destination leg never arrives
            var route = new CaravanRouteRequest(Owner, WagonId,
                "s-src", SourceWh, "s-dst", TargetWh,
                new Dictionary<string, float> { ["wood"] = 40f }, repeat: false);
            _service.SetRoute(route);
            _service.Tick();

            Assert.IsTrue(_service.StopRoute(Owner, WagonId).Succeeded);
            _service.TryGetCargo(Owner, WagonId, out var cargo);
            Assert.AreEqual(40f, cargo.Resources["wood"],
                "Interrupted route keeps loaded cargo on the wagon.");
            Assert.AreEqual(0f, _target.GetResource("wood"), "Nothing was delivered.");
        }

        [Test]
        public void ManualCargo_BlockedWhileRouteActive()
        {
            _gameplay.Wagon(WagonId, Owner, capacity: 100f, new Vector2Int(10, 10));
            _gameplay.HangFirstMove = true; // route never reaches load
            var route = new CaravanRouteRequest(Owner, WagonId,
                "s-src", SourceWh, "s-dst", TargetWh,
                new Dictionary<string, float> { ["wood"] = 10f }, repeat: false);
            _service.SetRoute(route);
            _service.Tick();

            Assert.IsFalse(_service.Execute(Load("wood", 10f)).Succeeded,
                "Active route owns the wagon's cargo.");
            Assert.IsFalse(_service.CanExecute(Load("wood", 10f)).Succeeded);
        }

        [Test]
        public void SecondRoute_WhileActive_Rejected()
        {
            _gameplay.Wagon(WagonId, Owner, capacity: 100f, new Vector2Int(10, 10));
            _gameplay.HangFirstMove = true;
            var route = new CaravanRouteRequest(Owner, WagonId,
                "s-src", SourceWh, "s-dst", TargetWh,
                new Dictionary<string, float> { ["wood"] = 10f }, repeat: false);
            _service.SetRoute(route);
            _service.Tick();
            Assert.IsFalse(_service.SetRoute(route).Succeeded);
        }

        [Test]
        public void Route_ExistingCargo_Rejected()
        {
            _gameplay.Wagon(WagonId, Owner, capacity: 100f, new Vector2Int(10, 10));
            _service.Execute(Load("wood", 10f));
            var route = new CaravanRouteRequest(Owner, WagonId,
                "s-src", SourceWh, "s-dst", TargetWh,
                new Dictionary<string, float> { ["wood"] = 10f }, repeat: false);
            Assert.IsFalse(_service.SetRoute(route).Succeeded,
                "A route starts empty-handed — unload or finish first.");
        }

        private float WagonWood()
            => _service.TryGetCargo(Owner, WagonId, out var cargo)
               && cargo.Resources.TryGetValue("wood", out var w) ? w : 0f;

        private sealed class C19Gameplay : ICaravanGameplayAccess
        {
            private readonly Dictionary<string, CaravanUnitSnapshot> _wagons = new();
            private int _moves;
            public bool HangFirstMove;
            public bool HangSecondMove;

            public event System.Action ProgressAvailable { add { } remove { } }
            public bool IsAuthoritative => true;

            public void Wagon(string id, string owner, float capacity, Vector2Int pos)
                => _wagons[id] = new CaravanUnitSnapshot(owner, pos, capacity);
            public void Move(string id, Vector2Int pos)
                => _wagons[id] = new CaravanUnitSnapshot(_wagons[id].OwnerId, pos, _wagons[id].Capacity);

            public bool TryGetWagon(string unitId, out CaravanUnitSnapshot unit)
                => _wagons.TryGetValue(unitId, out unit);
            public bool CanCommand(string ownerId, string unitId, out string reason)
            {
                reason = null;
                return _wagons.TryGetValue(unitId, out var u)
                    && string.Equals(u.OwnerId, ownerId, System.StringComparison.Ordinal);
            }
            public bool CanAccessWarehouse(string unitId, Vector2Int origin, out string reason)
            {
                reason = null;
                return true;
            }
            public Task<CaravanTransferResult> MoveToWarehouseAsync(
                string unitId, Vector2Int origin, CancellationToken token)
            {
                _moves++;
                if (_moves == 1 && HangFirstMove || _moves == 2 && HangSecondMove)
                    return Task.Delay(Timeout.Infinite, token)
                        .ContinueWith(_ => CaravanTransferResult.Rejected("interrupted"),
                            TaskContinuationOptions.OnlyOnCanceled);
                Move(unitId, origin);
                return Task.FromResult(CaravanTransferResult.Success());
            }
            public bool TryMeasureWarehouseRoute(string ownerId, Vector2Int sourceOrigin,
                Vector2Int targetOrigin, out float distance)
            {
                distance = Vector2Int.Distance(sourceOrigin, targetOrigin);
                return true;
            }
        }
    }
}
