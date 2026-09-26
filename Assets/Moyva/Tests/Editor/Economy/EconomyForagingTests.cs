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
    /// Anti-softlock foraging: residents periodically gather basic resources
    /// without buildings, and pre-warehouse spending can draw from settlement
    /// pools when the starter owner pool is empty.
    /// </summary>
    [TestFixture]
    public class EconomyForagingTests
    {
        private static EconomySettlementState CreateState(int residents)
        {
            var state = new EconomySettlementState
            {
                SettlementId = "settlement-a",
                SettlementName = "Alpha",
                OwnerId = "player_0",
            };
            for (int i = 0; i < residents; i++)
                state.Residents.Add(new EconomyResidentState(age: 30, hp: 100f, comfort: 50f, houseCollapsed: false));
            return state;
        }

        [Test]
        public void Tick_GrantsResource_WhenTurnMatchesInterval()
        {
            var state = CreateState(residents: 10);
            var rules = new EconomyRulesConfigSO();
            var foraging = new EconomyForagingService();

            state.CurrentTurn = 5; // walnut-wood interval
            var granted = foraging.Tick(state, rules);

            float expected = 2f + 0.05f * 10;
            Assert.AreEqual(expected, state.GetResource("walnut-wood-materials-resources"), 0.0001f);
            Assert.IsTrue(granted.Exists(e => e.Key == "walnut-wood-materials-resources"));
        }

        [Test]
        public void Tick_GrantsNothing_OnOffIntervalTurn()
        {
            var state = CreateState(residents: 10);
            var rules = new EconomyRulesConfigSO();
            var foraging = new EconomyForagingService();

            state.CurrentTurn = 7;
            var granted = foraging.Tick(state, rules);

            Assert.AreEqual(0, granted.Count);
            Assert.AreEqual(0f, state.GetResource("walnut-wood-materials-resources"), 0.0001f);
        }

        [Test]
        public void Tick_GrantsNothing_WithoutResidents()
        {
            var state = CreateState(residents: 0);
            var rules = new EconomyRulesConfigSO();
            var foraging = new EconomyForagingService();

            state.CurrentTurn = 5;
            var granted = foraging.Tick(state, rules);

            Assert.AreEqual(0, granted.Count);
        }

        [Test]
        public void Orchestrator_ReportsForagedResources()
        {
            var state = CreateState(residents: 1);
            var rules = new EconomyRulesConfigSO();
            var orchestrator = new EconomyTickOrchestrator();

            state.CurrentTurn = 4; // next tick lands on turn 5
            var result = orchestrator.Tick(state, database: null, rules);

            Assert.IsNotNull(result.ForagedResources);
            Assert.IsTrue(result.ForagedResources.Exists(e => e.Key == "walnut-wood-materials-resources"));
        }

        [Test]
        public void TryConsumeOwnerPoolResources_FallsBackToSettlementPools()
        {
            var container = new DiContainer();
            Zenject.SignalBusInstaller.Install(container);
            container.DeclareSignal<SettlementResourceChangedSignal>().OptionalSubscriber();
            var signals = container.Resolve<SignalBus>();

            var registry = new EconomySettlementRegistryService();
            var economy = new EconomyManager(null, signals, null, null, null, registry, null, null);

            var state = CreateState(residents: 5);
            state.AddResource("walnut-wood-materials-resources", 8f);
            registry.RegisterSettlement(state, new Vector2Int(0, 0));

            var costs = new Dictionary<string, float> { ["walnut-wood-materials-resources"] = 5f };
            Assert.IsTrue(economy.TryConsumeOwnerPoolResources("player_0", costs, out string error), error);
            Assert.AreEqual(3f, state.GetResource("walnut-wood-materials-resources"), 0.0001f);
        }

        [Test]
        public void TryConsumeOwnerPoolResources_FailsWhenCombinedFundsInsufficient()
        {
            var container = new DiContainer();
            Zenject.SignalBusInstaller.Install(container);
            container.DeclareSignal<SettlementResourceChangedSignal>().OptionalSubscriber();
            var signals = container.Resolve<SignalBus>();

            var registry = new EconomySettlementRegistryService();
            var economy = new EconomyManager(null, signals, null, null, null, registry, null, null);

            var state = CreateState(residents: 5);
            state.AddResource("walnut-wood-materials-resources", 2f);
            registry.RegisterSettlement(state, new Vector2Int(0, 0));

            var costs = new Dictionary<string, float> { ["walnut-wood-materials-resources"] = 5f };
            Assert.IsFalse(economy.TryConsumeOwnerPoolResources("player_0", costs, out _));
            Assert.AreEqual(2f, state.GetResource("walnut-wood-materials-resources"), 0.0001f,
                "Failed consume must not drain settlement resources.");
        }
    }
}
