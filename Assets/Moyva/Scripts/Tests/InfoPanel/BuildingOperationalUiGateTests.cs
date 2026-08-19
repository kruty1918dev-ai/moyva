using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.InfoPanel
{
    [TestFixture]
    public sealed class BuildingOperationalUiGateTests : ZenjectUnitTestFixture
    {
        private sealed class FakeUnitService : IUnitService
        {
            public float GetStamina(string unitId) => 0f;
            public void SetStamina(string unitId, float stamina) { }
            public bool TryGetUnitPosition(string unitId, out Vector2Int position)
            {
                position = default;
                return false;
            }
            public GameObject GetUnitObject(string unitId) => null;
            public IReadOnlyCollection<string> GetAllUnitIds()
                => Array.Empty<string>();
            public string GetUnitTypeId(string unitId) => null;
        }

        private sealed class FixedLifecycle : IConstructionLifecycle
        {
            private readonly bool _isOperational;

            public FixedLifecycle(bool isOperational)
            {
                _isOperational = isOperational;
            }

            public bool IsOperational(Vector2Int position)
                => _isOperational;

            public bool TryGetProgress(
                Vector2Int position,
                out int completedTurns,
                out int requiredTurns)
            {
                completedTurns = _isOperational ? 1 : 0;
                requiredTurns = 1;
                return !_isOperational;
            }
        }

        [Test]
        public void UnderConstructionBuilding_DoesNotOpenFunctionalUI()
        {
            AssertBuildingInfoRequestEmitted(
                isOperational: false,
                expectedSelectionEvents: 0);
        }

        [Test]
        public void OperationalBuilding_OpensFunctionalUI()
        {
            AssertBuildingInfoRequestEmitted(
                isOperational: true,
                expectedSelectionEvents: 1);
        }

        private void AssertBuildingInfoRequestEmitted(
            bool isOperational,
            int expectedSelectionEvents)
        {
            SetupSignals();
            SignalBus signalBus = Container.Resolve<SignalBus>();
            var events = new List<WorldInfoSelectionChangedSignal>();
            signalBus.Subscribe<WorldInfoSelectionChangedSignal>(
                signal => events.Add(signal));

            object coordinator = CreateCoordinator(
                signalBus,
                new FixedLifecycle(isOperational));
            coordinator.GetType().GetMethod("Initialize")?.Invoke(
                coordinator,
                null);

            try
            {
                signalBus.Fire(new BuildingInfoPanelRequestedSignal
                {
                    BuildingId = "barrack",
                    Position = new Vector2Int(4, 5),
                });

                Assert.AreEqual(expectedSelectionEvents, events.Count);
                if (expectedSelectionEvents > 0)
                {
                    Assert.AreEqual(WorldInfoSelectionKind.Building, events[0].Kind);
                    Assert.AreEqual("barrack", events[0].ObjectId);
                    Assert.AreEqual(new Vector2Int(4, 5), events[0].Position);
                }
            }
            finally
            {
                coordinator.GetType().GetMethod("Dispose")?.Invoke(
                    coordinator,
                    null);
            }
        }

        private void SetupSignals()
        {
            Zenject.SignalBusInstaller.Install(Container);
            Container.DeclareSignal<BuildingInfoPanelRequestedSignal>();
            Container.DeclareSignal<UnitInfoPanelRequestedSignal>();
            Container.DeclareSignal<MapObjectInfoPanelRequestedSignal>();
            Container.DeclareSignal<WorldInfoPanelClosedSignal>();
            Container.DeclareSignal<WorldInfoSelectionChangedSignal>();
            Container.DeclareSignal<UnitMovedSignal>();
            Container.DeclareSignal<EconomyTickCompletedSignal>();
            Container.DeclareSignal<SettlementResourceChangedSignal>();
        }

        private static object CreateCoordinator(
            SignalBus signalBus,
            IConstructionLifecycle lifecycle)
        {
            Type coordinatorType = Type.GetType(
                "Kruty1918.Moyva.Interactions.Runtime.WorldInfoSelectionCoordinator, Kruty1918.Moyva.Interactions.API");
            Assert.NotNull(coordinatorType);
            object coordinator = Activator.CreateInstance(
                coordinatorType,
                signalBus,
                new FakeUnitService(),
                lifecycle);
            Assert.NotNull(coordinator);
            return coordinator;
        }
    }
}
