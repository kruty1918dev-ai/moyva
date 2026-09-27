using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Editor.Economy
{
    /// <summary>
    /// C22: the zero-resource recovery path is foraging, not a hidden fallback.
    /// Authored defaults: walnut every 5 turns at base 2 + 0.05/resident, plus
    /// food/material yields on other intervals. Zero residents deactivates the
    /// settlement, so recovery is real but not unconditional.
    /// </summary>
    public sealed class C22AcceptanceTests
    {
        private EconomyForagingService _foraging;
        private EconomyRulesConfigSO _rules;

        [SetUp]
        public void SetUp()
        {
            _foraging = new EconomyForagingService();
            _rules = new EconomyRulesConfigSO();
        }

        private static EconomySettlementState Settlement(int residents, bool active = true)
        {
            var state = new EconomySettlementState
            {
                SettlementId = "s-1",
                OwnerId = "owner-a",
                IsActive = active,
            };
            for (int i = 0; i < residents; i++)
                state.Residents.Add(new EconomyResidentState(age: 30, hp: 100, comfort: 60f, houseCollapsed: false));
            return state;
        }

        [Test]
        public void Walnut_FiresExactlyOnIntervalTurns()
        {
            var state = Settlement(residents: 1);
            for (int turn = 1; turn <= 10; turn++)
            {
                state.CurrentTurn = turn;
                var granted = _foraging.Tick(state, _rules);
                bool walnut = granted.Exists(kv => kv.Key == "walnut-wood-materials-resources");
                Assert.AreEqual(turn % 5 == 0, walnut, $"Walnut interval correctness at turn {turn}.");
            }
        }

        [Test]
        public void Yield_Scales_BasePlusPerResident()
        {
            var one = Settlement(residents: 1);
            var fifteen = Settlement(residents: 15);
            one.CurrentTurn = fifteen.CurrentTurn = 5;

            float walnutOne = 0f, walnutFifteen = 0f;
            foreach (var kv in _foraging.Tick(one, _rules))
                if (kv.Key == "walnut-wood-materials-resources") walnutOne = kv.Value;
            foreach (var kv in _foraging.Tick(fifteen, _rules))
                if (kv.Key == "walnut-wood-materials-resources") walnutFifteen = kv.Value;

            Assert.AreEqual(2f + 0.05f * 1, walnutOne, 0.001f);
            Assert.AreEqual(2f + 0.05f * 15, walnutFifteen, 0.001f);
            Assert.AreEqual(walnutOne, one.GetResource("walnut-wood-materials-resources"), 0.001f,
                "Granted amounts land in the settlement pool.");
        }

        [Test]
        public void ZeroResidents_NothingForaged()
        {
            var state = Settlement(residents: 0);
            state.CurrentTurn = 5;
            var granted = _foraging.Tick(state, _rules);
            Assert.IsEmpty(granted, "MinResidents(1) blocks foraging with no residents — " +
                "recovery from a dead settlement is not unconditional.");
        }

        [Test]
        public void InactiveSettlement_NothingForaged()
        {
            var state = Settlement(residents: 15, active: false);
            state.CurrentTurn = 5;
            Assert.IsEmpty(_foraging.Tick(state, _rules));
        }

        [Test]
        public void FoodYields_Exist_InAuthoredDefaults()
        {
            var state = Settlement(residents: 5);
            state.CurrentTurn = 3; // raspberry interval
            var granted = _foraging.Tick(state, _rules);
            Assert.IsTrue(granted.Exists(kv => kv.Key == "raspberries-food-resources"),
                "Authored defaults include a food forage path for zero-stock recovery.");
        }

        [Test]
        public void RecoveryFromZero_AccumulatesOverIntervals()
        {
            var state = Settlement(residents: 3);
            float food = 0f, materials = 0f;
            for (int turn = 1; turn <= 20; turn++)
            {
                state.CurrentTurn = turn;
                foreach (var kv in _foraging.Tick(state, _rules))
                {
                    if (kv.Key.Contains("food")) food += kv.Value;
                    else materials += kv.Value;
                }
            }
            Assert.Greater(food, 0f, "Food foraging restores a starving economy.");
            Assert.Greater(materials, 0f, "Material foraging restores build capacity.");
            Assert.AreEqual(state.GetResource("walnut-wood-materials-resources") > 0f, true);
        }

        [Test]
        public void TurnZero_NothingForaged()
        {
            var state = Settlement(residents: 5);
            state.CurrentTurn = 0;
            Assert.IsEmpty(_foraging.Tick(state, _rules), "CurrentTurn <= 0 grants nothing.");
        }
    }
}
