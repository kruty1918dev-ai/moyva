using System.Collections.Generic;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.Economy
{
    /// <summary>
    /// Construction supply orders: dispatch reserves stock at the source
    /// warehouse, the wagon route carries it physically, delivery reserves it
    /// at the target until the placement confirms or cancels.
    /// </summary>
    [TestFixture]
    public class ConstructionSupplyServiceTests
    {
        private static readonly Vector2Int TargetPosition = new(10, 10);
        private static readonly Vector2Int PlacementPosition = new(10, 11);
        private static readonly Vector2Int SourcePosition = new(0, 0);
        private const string TargetWarehouseKey = "10:10";
        private const string SourceWarehouseKey = "0:0";

        private DiContainer _container;
        private SignalBus _signals;
        private EconomySettlementRegistryService _registry;
        private EconomyManager _economy;
        private FakeCaravanGameplayAccess _gameplay;
        private FakeUnitService _units;
        private CaravanService _caravans;
        private ConstructionSupplyService _supply;
        private EconomySettlementState _target;
        private EconomySettlementState _source;

        [SetUp]
        public void SetUp()
        {
            _container = new DiContainer();
            Zenject.SignalBusInstaller.Install(_container);
            _container.DeclareSignal<SettlementResourceChangedSignal>().OptionalSubscriber();
            _container.DeclareSignal<CaravanDeliveryCompletedSignal>().OptionalSubscriber();
            _container.DeclareSignal<ConstructionSupplyReadySignal>().OptionalSubscriber();
            _container.DeclareSignal<ConstructionSupplyOrderClosedSignal>().OptionalSubscriber();
            _signals = _container.Resolve<SignalBus>();

            _registry = new EconomySettlementRegistryService();
            _economy = new EconomyManager(null, _signals, null, null, null, _registry, null, null);
            _gameplay = new FakeCaravanGameplayAccess();
            _units = new FakeUnitService();
            _container.Bind<ICaravanGameplayAccess>().FromInstance(_gameplay);
            _container.Bind<ConstructionSupplyService>().FromMethod(() => _supply);
            var gameplayLazy = new LazyInject<ICaravanGameplayAccess>(_container,
                new InjectContext(_container, typeof(ICaravanGameplayAccess)));
            var supplyLazy = new LazyInject<ConstructionSupplyService>(_container,
                new InjectContext(_container, typeof(ConstructionSupplyService)));
            _caravans = new CaravanService(_registry, _signals, gameplayLazy,
                constructionSupply: supplyLazy);
            _supply = new ConstructionSupplyService(_registry, _economy, _caravans,
                _signals, gameplayLazy, _units);
            _supply.Initialize();

            _target = new EconomySettlementState
            {
                SettlementId = "target",
                OwnerId = "player_0",
                SettlementName = "Target",
            };
            _target.EnsureWarehousePool(TargetWarehouseKey);
            _registry.RegisterSettlement(_target, TargetPosition);

            _source = new EconomySettlementState
            {
                SettlementId = "source",
                OwnerId = "player_0",
                SettlementName = "Source",
            };
            _source.EnsureWarehousePool(SourceWarehouseKey);
            _source.AddResource("wood", 200f, SourceWarehouseKey);
            _registry.RegisterSettlement(_source, SourcePosition);

            AddWagon("wagon-1", new Vector2Int(1, 0), 120f);
        }

        [TearDown]
        public void TearDown() => _supply.Dispose();

        private void AddWagon(string unitId, Vector2Int position, float capacity)
        {
            _units.Add(unitId, position, "caravan-wagon");
            _gameplay.Wagons[unitId] = new CaravanUnitSnapshot("player_0", position, capacity);
        }

        private static Dictionary<string, float> Required(float wood)
            => new() { ["wood"] = wood };

        private CaravanTransferResult Dispatch(string unitId = "wagon-1", float wood = 50f)
            => _supply.DispatchSupply(new ConstructionSupplyDispatchRequest(
                "player_0", "lumber-camp", PlacementPosition,
                "source", SourceWarehouseKey, unitId), Required(wood));

        private EconomySettlementState AddSourceSettlement(string id, string name,
            Vector2Int position, string warehouseKey, float wood = 200f)
        {
            var settlement = new EconomySettlementState
            {
                SettlementId = id,
                OwnerId = "player_0",
                SettlementName = name,
            };
            settlement.EnsureWarehousePool(warehouseKey);
            settlement.AddResource("wood", wood, warehouseKey);
            _registry.RegisterSettlement(settlement, position);
            return settlement;
        }

        [Test]
        public void Evaluate_ExcludesUnreachableSource_EvenWhenGeometricallyNearer()
        {
            // Near warehouse across impassable terrain; the farther one keeps an open route.
            AddSourceSettlement("near", "Near", new Vector2Int(9, 10), "9:10", wood: 500f);
            _gameplay.UnreachableWarehouses.Add(new Vector2Int(9, 10));

            var evaluation = _supply.Evaluate("player_0", "lumber-camp",
                PlacementPosition, Required(50f));

            Assert.AreEqual(1, evaluation.Sources.Count);
            Assert.AreEqual("source", evaluation.Sources[0].SettlementId,
                "The reachable farther source must be offered instead of the blocked nearer one.");
            Assert.Greater(evaluation.Sources[0].RouteDistance, 0f);
            Assert.AreEqual(1, evaluation.UnreachableSources,
                "The blocked warehouse must be reported so the UI can explain it.");
        }

        [Test]
        public void Evaluate_OrdersSourcesByRouteDistance_ThenSettlementName()
        {
            // "far" sits closer to the target warehouse (8) than "source" (~14.1);
            // "alpha" and "zeta" tie at distance 2 and must resolve by name.
            AddSourceSettlement("far", "Far", new Vector2Int(18, 10), "18:10", wood: 100f);
            AddSourceSettlement("zeta", "Zeta", new Vector2Int(8, 10), "8:10");
            AddSourceSettlement("alpha", "Alpha", new Vector2Int(12, 10), "12:10");

            var evaluation = _supply.Evaluate("player_0", "lumber-camp",
                PlacementPosition, Required(50f));

            Assert.AreEqual(4, evaluation.Sources.Count);
            Assert.AreEqual("alpha", evaluation.Sources[0].SettlementId);
            Assert.AreEqual("zeta", evaluation.Sources[1].SettlementId);
            Assert.AreEqual("far", evaluation.Sources[2].SettlementId);
            Assert.AreEqual("source", evaluation.Sources[3].SettlementId);
            for (int i = 1; i < evaluation.Sources.Count; i++)
                Assert.LessOrEqual(evaluation.Sources[i - 1].RouteDistance,
                    evaluation.Sources[i].RouteDistance + 0.001f);
        }

        [Test]
        public void Evaluate_ReportsLocalDeficit_AndListsSourceWarehouses()
        {
            var evaluation = _supply.Evaluate("player_0", "lumber-camp",
                PlacementPosition, Required(50f));

            Assert.IsTrue(evaluation.Resolved);
            Assert.AreEqual("target", evaluation.SettlementId);
            Assert.AreEqual(1, evaluation.Resources.Count);
            Assert.That(evaluation.Resources[0].Deficit, Is.EqualTo(50f).Within(0.001f));
            Assert.IsTrue(evaluation.HasDeficit);
            Assert.AreEqual(1, evaluation.Sources.Count);
            Assert.AreEqual("source", evaluation.Sources[0].SettlementId);
            Assert.That(evaluation.Sources[0].Available["wood"], Is.EqualTo(200f).Within(0.001f));
            Assert.AreEqual(1, evaluation.Wagons.Count);
            Assert.IsFalse(evaluation.Wagons[0].Busy);
        }

        [Test]
        public void PreviewShipment_ReportsPlannedCargo_WithoutReserving()
        {
            AddWagon("wagon-small", new Vector2Int(2, 0), 30f);
            var request = new ConstructionSupplyDispatchRequest("player_0", "lumber-camp",
                PlacementPosition, "source", SourceWarehouseKey, "wagon-small");

            var preview = _supply.PreviewShipment(request, Required(50f));

            Assert.AreEqual(30f, preview["wood"], 0.001f,
                "Preview clamps the shipment to the wagon's free capacity.");
            Assert.AreEqual(0f, _source.GetTotalReservedResource("wood"), 0.001f,
                "Preview must not reserve source stock.");
            Assert.IsFalse(_caravans.TryGetRoute("player_0", "wagon-small", out _),
                "Preview must not start a route.");
            Assert.IsFalse(_supply.TryGetOrderAt(PlacementPosition, out _),
                "Preview must not create an order.");
        }

        [Test]
        public void PreviewShipment_ReturnsEmpty_ForForeignSource()
        {
            var foreign = AddSourceSettlement("enemy", "Enemy",
                new Vector2Int(8, 10), "8:10");
            foreign.OwnerId = "player_1";

            var preview = _supply.PreviewShipment(new ConstructionSupplyDispatchRequest(
                "player_0", "lumber-camp", PlacementPosition,
                "enemy", "8:10", "wagon-1"), Required(50f));

            Assert.AreEqual(0, preview.Count);
        }

        [Test]
        public void DispatchSupply_ReservesSourceStock_AndStartsRoute()
        {
            var result = Dispatch();

            Assert.IsTrue(result.Succeeded, result.Reason);
            Assert.AreEqual(50f, _source.GetReservedResourceAt(SourceWarehouseKey, "wood"), 0.001f);
            Assert.AreEqual(150f, _source.GetAvailableResource("wood"), 0.001f,
                "Reserved stock must not be spendable by other consumers.");
            Assert.IsTrue(_caravans.TryGetRoute("player_0", "wagon-1", out _));
            Assert.IsTrue(_supply.TryGetOrderAt(PlacementPosition, out var order));
            Assert.AreEqual(ConstructionSupplyOrderStatus.Active, order.Status);
            Assert.AreEqual(50f, order.Remaining["wood"], 0.001f);
            Assert.That(order.WagonIds, Has.Member("wagon-1"));
        }

        [Test]
        public void RouteDelivery_ReservesStockAtTarget_AndMarksOrderReady()
        {
            Assert.IsTrue(Dispatch().Succeeded);

            _caravans.Tick();

            Assert.AreEqual(150f, _source.GetResource("wood"), 0.001f,
                "The load leg consumed the reserved source stock.");
            Assert.AreEqual(0f, _source.GetTotalReservedResource("wood"), 0.001f,
                "The source reservation is released when the wagon loads.");
            Assert.AreEqual(50f, _target.GetResource("wood"), 0.001f);
            Assert.AreEqual(50f, _target.GetTotalReservedResource("wood"), 0.001f,
                "Delivered stock is reserved for the pending placement.");
            Assert.IsTrue(_supply.TryGetOrderAt(PlacementPosition, out var order));
            Assert.AreEqual(ConstructionSupplyOrderStatus.Ready, order.Status);

            Assert.IsFalse(
                _economy.TryConsumeSettlementResources("target", Required(50f), out _),
                "Generic consumption must not spend stock reserved for the placement.");
            var spendable = _supply.GetResourcesForPlacement("target", PlacementPosition);
            Assert.AreEqual(50f, spendable["wood"], 0.001f);
        }

        [Test]
        public void CancelOrder_ReleasesReservations_AndStopsRoute()
        {
            Assert.IsTrue(Dispatch().Succeeded);

            _supply.CancelOrderAt(PlacementPosition);

            Assert.AreEqual(0f, _source.GetTotalReservedResource("wood"), 0.001f);
            Assert.AreEqual(200f, _source.GetAvailableResource("wood"), 0.001f);
            Assert.IsFalse(_supply.TryGetOrderAt(PlacementPosition, out _));
            Assert.IsFalse(_caravans.TryGetRoute("player_0", "wagon-1", out _));
        }

        [Test]
        public void DispatchSupply_ClampsShipmentToWagonCapacity()
        {
            AddWagon("wagon-small", new Vector2Int(2, 0), 30f);

            var result = Dispatch("wagon-small", 50f);

            Assert.IsTrue(result.Succeeded, result.Reason);
            Assert.IsTrue(_caravans.TryGetRoute("player_0", "wagon-small", out var route));
            Assert.AreEqual(30f, route.Request.Resources["wood"], 0.001f);
            Assert.IsTrue(route.Request.Repeat,
                "Partial shipments must repeat until the deficit is covered.");
        }

        [Test]
        public void DispatchSupply_SecondOrderCannotShipReservedStock()
        {
            // Leave 60 wood, then the first order's shipment reserves all of it.
            Assert.IsTrue(_source.ConsumeResource("wood", 140f));
            Assert.IsTrue(Dispatch(wood: 60f).Succeeded);
            var secondPosition = new Vector2Int(10, 12);
            AddWagon("wagon-2", new Vector2Int(3, 0), 120f);

            var result = _supply.DispatchSupply(new ConstructionSupplyDispatchRequest(
                "player_0", "lumber-camp", secondPosition,
                "source", SourceWarehouseKey, "wagon-2"), Required(50f));

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual("The source warehouse cannot ship any missing resource.",
                result.Reason);
        }

        [Test]
        public void DispatchSupply_RejectsBusyWagon()
        {
            Assert.IsTrue(Dispatch().Succeeded);

            var secondPosition = new Vector2Int(10, 12);
            var result = _supply.DispatchSupply(new ConstructionSupplyDispatchRequest(
                "player_0", "lumber-camp", secondPosition,
                "source", SourceWarehouseKey, "wagon-1"), Required(50f));

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual("This wagon already runs a route.", result.Reason);
        }

        [Test]
        public void DispatchSupply_RejectsOtherOwnersWagon()
        {
            _gameplay.Wagons["enemy-wagon"] =
                new CaravanUnitSnapshot("player_1", new Vector2Int(5, 5), 120f);

            var result = _supply.DispatchSupply(new ConstructionSupplyDispatchRequest(
                "player_0", "lumber-camp", PlacementPosition,
                "source", SourceWarehouseKey, "enemy-wagon"), Required(50f));

            Assert.IsFalse(result.Succeeded);
        }
    }
}
