using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.InfoPanel
{
    [TestFixture]
    public sealed class WorldInfoSelectionCoordinatorTests : ZenjectUnitTestFixture
    {
        private sealed class FakeUnitService : IUnitService
        {
            public readonly Dictionary<string, Vector2Int> Positions = new(StringComparer.Ordinal);

            public float GetStamina(string unitId) => 0f;
            public void SetStamina(string unitId, float stamina) { }
            public bool TryGetUnitPosition(string unitId, out Vector2Int position) => Positions.TryGetValue(unitId, out position);
            public GameObject GetUnitObject(string unitId) => null;
            public IReadOnlyCollection<string> GetAllUnitIds() => Positions.Keys;
            public string GetUnitTypeId(string unitId) => null;
        }

        private SignalBus _signalBus;
        private object _coordinator;
        private FakeUnitService _unitService;

        public override void Setup()
        {
            base.Setup();

            Zenject.SignalBusInstaller.Install(Container);
            Container.DeclareSignal<BuildingInfoPanelRequestedSignal>();
            Container.DeclareSignal<UnitInfoPanelRequestedSignal>();
            Container.DeclareSignal<WorldInfoPanelClosedSignal>();
            Container.DeclareSignal<WorldInfoSelectionChangedSignal>();
            Container.DeclareSignal<UnitMovedSignal>();
            Container.DeclareSignal<EconomyTickCompletedSignal>();
            Container.DeclareSignal<SettlementResourceChangedSignal>();

            _unitService = new FakeUnitService();
            _unitService.Positions["unit_1"] = new Vector2Int(2, 3);

            Container.Bind<IUnitService>().FromInstance(_unitService).AsSingle();
            _signalBus = Container.Resolve<SignalBus>();

            var coordinatorType = Type.GetType("Kruty1918.Moyva.Interactions.Runtime.WorldInfoSelectionCoordinator, Kruty1918.Moyva.Interactions.API");
            Assert.NotNull(coordinatorType);

            _coordinator = Activator.CreateInstance(coordinatorType, _signalBus, _unitService);
            Assert.NotNull(_coordinator);

            coordinatorType.GetMethod("Initialize")?.Invoke(_coordinator, null);
        }

        public override void Teardown()
        {
            _coordinator?.GetType().GetMethod("Dispose")?.Invoke(_coordinator, null);
            base.Teardown();
        }

        [Test]
        public void UnitSelection_FiresSelectionChanged_AndClearsOnPanelClose()
        {
            var events = new List<WorldInfoSelectionChangedSignal>();
            _signalBus.Subscribe<WorldInfoSelectionChangedSignal>(signal => events.Add(signal));

            _signalBus.Fire(new UnitInfoPanelRequestedSignal
            {
                UnitId = "unit_1",
                Position = new Vector2Int(2, 3),
            });

            _signalBus.Fire<WorldInfoPanelClosedSignal>();

            Assert.AreEqual(2, events.Count);
            Assert.AreEqual(WorldInfoSelectionKind.Unit, events[0].Kind);
            Assert.AreEqual("unit_1", events[0].ObjectId);
            Assert.AreEqual(WorldInfoSelectionKind.None, events[1].Kind);
        }

        [Test]
        public void SelectedUnitMove_RefreshesPanelRequest_WithNewPosition()
        {
            _signalBus.Fire(new UnitInfoPanelRequestedSignal
            {
                UnitId = "unit_1",
                Position = new Vector2Int(2, 3),
            });

            UnitInfoPanelRequestedSignal? refresh = null;
            _signalBus.Subscribe<UnitInfoPanelRequestedSignal>(signal => refresh = signal);

            _unitService.Positions["unit_1"] = new Vector2Int(6, 8);
            _signalBus.Fire(new UnitMovedSignal
            {
                UnitId = "unit_1",
                NewPosition = new Vector2Int(6, 8),
                Cost = 1f,
            });

            Assert.IsTrue(refresh.HasValue);
            Assert.AreEqual("unit_1", refresh.Value.UnitId);
            Assert.AreEqual(new Vector2Int(6, 8), refresh.Value.Position);
        }
    }
}