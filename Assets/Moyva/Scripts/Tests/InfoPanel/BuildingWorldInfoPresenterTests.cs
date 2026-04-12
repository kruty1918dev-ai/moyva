using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.InfoPanel
{
    [TestFixture]
    public sealed class BuildingWorldInfoPresenterTests : ZenjectUnitTestFixture
    {
        private sealed class FakeBuildingRegistry : IBuildingRegistry
        {
            private readonly Dictionary<string, BuildingDefinition> _definitions;

            public FakeBuildingRegistry(Dictionary<string, BuildingDefinition> definitions)
            {
                _definitions = definitions;
            }

            public BuildingDefinition[] GetAll() => throw new NotImplementedException();
            public BuildingDefinition[] GetByCategory(BuildingCategory category) => throw new NotImplementedException();
            public WallCollectionDefinition[] GetWallCollections() => throw new NotImplementedException();
            public WallCollectionDefinition GetWallCollectionByBuildingId(string buildingId) => null;

            public BuildingDefinition GetById(string id)
            {
                if (string.IsNullOrWhiteSpace(id))
                    return null;

                _definitions.TryGetValue(id, out var value);
                return value;
            }
        }

        private SignalBus _signalBus;
        private object _presenter;

        public override void Setup()
        {
            base.Setup();
            Zenject.SignalBusInstaller.Install(Container);
            Container.DeclareSignal<BuildingInfoPanelRequestedSignal>();
            Container.DeclareSignal<WorldInfoPanelRequestedSignal>();

            var registry = new FakeBuildingRegistry(new Dictionary<string, BuildingDefinition>
            {
                {
                    "sawmill",
                    new BuildingDefinition
                    {
                        Id = "sawmill",
                        DisplayName = "Лісопилка",
                        Category = BuildingCategory.Industrial,
                        RequiredWorkers = 2,
                        EconomyPriority = 100
                    }
                }
            });

            Container.Bind<IBuildingRegistry>().FromInstance(registry).AsSingle();
            _signalBus = Container.Resolve<SignalBus>();

            var presenterType = typeof(IBuildingRegistry).Assembly
                .GetType("Kruty1918.Moyva.Construction.Runtime.BuildingWorldInfoPresenter");
            Assert.NotNull(presenterType);

            _presenter = Activator.CreateInstance(presenterType, _signalBus, registry, null);
            Assert.NotNull(_presenter);

            presenterType.GetMethod("Initialize")?.Invoke(_presenter, null);
        }

        public override void Teardown()
        {
            _presenter?.GetType().GetMethod("Dispose")?.Invoke(_presenter, null);
            base.Teardown();
        }

        [Test]
        public void BuildingInfoRequest_FiresWorldInfoSignal_WithFallbackPayload()
        {
            WorldInfoPanelRequestedSignal? payload = null;
            _signalBus.Subscribe<WorldInfoPanelRequestedSignal>(signal => payload = signal);

            _signalBus.Fire(new BuildingInfoPanelRequestedSignal
            {
                BuildingId = "sawmill",
                Position = new Vector2Int(10, 5)
            });

            Assert.IsTrue(payload.HasValue);
            Assert.AreEqual("Лісопилка", payload.Value.Title);
            StringAssert.Contains("Будівля", payload.Value.Subtitle);
            StringAssert.Contains("Базова інформація", payload.Value.Content);
        }

        [Test]
        public void UnknownBuilding_DoesNotFireWorldInfoSignal()
        {
            bool raised = false;
            _signalBus.Subscribe<WorldInfoPanelRequestedSignal>(_ => raised = true);

            _signalBus.Fire(new BuildingInfoPanelRequestedSignal
            {
                BuildingId = "unknown",
                Position = Vector2Int.zero
            });

            Assert.IsFalse(raised);
        }
    }
}
