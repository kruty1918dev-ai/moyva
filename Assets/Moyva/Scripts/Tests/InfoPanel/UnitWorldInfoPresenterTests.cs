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
    public sealed class UnitWorldInfoPresenterTests : ZenjectUnitTestFixture
    {
        private sealed class FakeUnitService : IUnitService
        {
            public readonly Dictionary<string, string> TypeByUnitId = new Dictionary<string, string>(StringComparer.Ordinal);
            public readonly Dictionary<string, float> StaminaByUnitId = new Dictionary<string, float>(StringComparer.Ordinal);

            public float GetStamina(string unitId)
                => StaminaByUnitId.TryGetValue(unitId, out var value) ? value : 0f;

            public void SetStamina(string unitId, float stamina)
                => StaminaByUnitId[unitId] = stamina;

            public bool TryGetUnitPosition(string unitId, out Vector2Int position)
            {
                position = default;
                return false;
            }

            public GameObject GetUnitObject(string unitId) => null;
            public IReadOnlyCollection<string> GetAllUnitIds() => TypeByUnitId.Keys;

            public string GetUnitTypeId(string unitId)
                => TypeByUnitId.TryGetValue(unitId, out var value) ? value : null;
        }

        private sealed class FakeUnitClassConfig : IUnitClassConfig
        {
            private readonly Dictionary<string, UnitClassConfig> _configs;

            public FakeUnitClassConfig(Dictionary<string, UnitClassConfig> configs)
            {
                _configs = configs;
            }

            public UnitClassConfig GetConfig(string typeId)
            {
                if (string.IsNullOrWhiteSpace(typeId))
                    return null;

                _configs.TryGetValue(typeId, out var config);
                return config;
            }
        }

        private sealed class FakeEconomyInfoMediator : IEconomyInfoMediator
        {
            public bool TryGetSettlementContext(Vector2Int position, out EconomySettlementContext context)
            {
                context = new EconomySettlementContext("settlement-1", "Поселення №1", "player_0");
                return true;
            }

            public bool TryResolveConstructionSettlement(Vector2Int position, string ownerId, out EconomySettlementContext context)
            {
                if (ownerId == "player_0")
                {
                    context = new EconomySettlementContext("settlement-1", "Поселення №1", "player_0");
                    return true;
                }
                context = default;
                return false;
            }

            public bool TryGetBuildingContext(Vector2Int position, out string buildingId, out string ownerId)
            {
                buildingId = null;
                ownerId = "player_0";
                return false;
            }

            public bool TryConsumeSettlementResources(string settlementId, IReadOnlyDictionary<string, float> resourceCosts, out string errorMessage)
            {
                // Для тесту просто дозволяємо найменування ресурсів
                errorMessage = string.Empty;
                return true;
            }

            public bool TryConsumeOwnerPoolResources(string ownerId, IReadOnlyDictionary<string, float> resourceCosts, out string errorMessage)
            {
                errorMessage = string.Empty;
                return true;
            }

            public bool OwnerHasAnyWarehouse(string ownerId)
                => false;

            public IReadOnlyDictionary<string, float> GetWarehouseResourceTotals(Vector2Int warehousePosition)
                => new Dictionary<string, float>(StringComparer.Ordinal);

            public IReadOnlyDictionary<string, float> GetSettlementWarehousesTotal(string settlementId)
                => new Dictionary<string, float>(StringComparer.Ordinal);

            public IReadOnlyDictionary<string, float> GetSettlementResourceTotals(string settlementId)
                => new Dictionary<string, float>(StringComparer.Ordinal)
                {
                    ["Food"] = 12f,
                    ["Wood"] = 7f,
                };

            public IReadOnlyDictionary<string, float> GetOwnerPoolResourceTotals(string ownerId)
                => new Dictionary<string, float>(StringComparer.Ordinal)
                {
                    ["Food"] = 33f,
                };

            public IReadOnlyDictionary<string, float> GetOwnerResourceTotals(string ownerId)
                => new Dictionary<string, float>(StringComparer.Ordinal)
                {
                    ["Food"] = 33f,
                };

            public string GetResourceDisplayName(string resourceId)
                => resourceId;
        }

        private SignalBus _signalBus;
        private object _presenter;

        public override void Setup()
        {
            base.Setup();
            Zenject.SignalBusInstaller.Install(Container);
            Container.DeclareSignal<UnitInfoPanelRequestedSignal>();
            Container.DeclareSignal<WorldInfoPanelRequestedSignal>();

            var unitService = new FakeUnitService();
            unitService.TypeByUnitId["unit_1"] = "worker";
            unitService.StaminaByUnitId["unit_1"] = 42f;

            var classConfig = new FakeUnitClassConfig(new Dictionary<string, UnitClassConfig>
            {
                ["worker"] = new UnitClassConfig
                {
                    TypeId = "worker",
                    Role = UnitRole.Worker,
                    BaseStamina = 50f,
                    VisionRange = 3,
                },
            });

            Container.Bind<IUnitService>().FromInstance(unitService).AsSingle();
            Container.Bind<IUnitClassConfig>().FromInstance(classConfig).AsSingle();
            Container.Bind<IEconomyInfoMediator>().To<FakeEconomyInfoMediator>().AsSingle();

            _signalBus = Container.Resolve<SignalBus>();

            var presenterType = typeof(IUnitService).Assembly
                .GetType("Kruty1918.Moyva.Units.Runtime.UnitWorldInfoPresenter");
            Assert.NotNull(presenterType);

            _presenter = Activator.CreateInstance(
                presenterType,
                _signalBus,
                Container.Resolve<IUnitClassConfig>(),
                Container.Resolve<IUnitService>(),
                Container.Resolve<IEconomyInfoMediator>());
            Assert.NotNull(_presenter);

            presenterType.GetMethod("Initialize")?.Invoke(_presenter, null);
        }

        public override void Teardown()
        {
            _presenter?.GetType().GetMethod("Dispose")?.Invoke(_presenter, null);
            base.Teardown();
        }

        [Test]
        public void UnitInfoRequest_FiresWorldInfoSignal_WithUnitAndEconomyData()
        {
            WorldInfoPanelRequestedSignal? payload = null;
            _signalBus.Subscribe<WorldInfoPanelRequestedSignal>(signal => payload = signal);

            _signalBus.Fire(new UnitInfoPanelRequestedSignal
            {
                UnitId = "unit_1",
                Position = new Vector2Int(3, 4),
            });

            Assert.IsTrue(payload.HasValue);
            Assert.AreEqual("worker", payload.Value.Title);
            StringAssert.Contains("Робітник", payload.Value.Subtitle);
            StringAssert.Contains("ID: unit_1", payload.Value.Content);
            StringAssert.Contains("Поточна стаміна: 42", payload.Value.Content);
            StringAssert.Contains("Ресурси поселення", payload.Value.Content);
            StringAssert.Contains("Food", payload.Value.Content);
        }

        [Test]
        public void EmptyUnitId_DoesNotFireWorldInfoSignal()
        {
            var raised = false;
            _signalBus.Subscribe<WorldInfoPanelRequestedSignal>(_ => raised = true);

            _signalBus.Fire(new UnitInfoPanelRequestedSignal
            {
                UnitId = " ",
                Position = Vector2Int.zero,
            });

            Assert.IsFalse(raised);
        }
    }
}
