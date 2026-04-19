using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Economy
{
    [TestFixture]
    public sealed class EconomyComfortAndMortalityServiceTests
    {
        [Test]
        public void CalculateComfort_ShouldUseConfiguredWeights()
        {
            var rules = ScriptableObject.CreateInstance<EconomyRulesConfigSO>();
            var service = new EconomyComfortAndMortalityService();

            var comfort = service.CalculateComfort(rules, new EconomyComfortInput(100f, 100f, 100f, 100f, 100f, 100f));
            Assert.AreEqual(100f, comfort, 0.001f);
        }

        [Test]
        public void CalculateDeathChance_ShouldBeImmediate_WhenHouseCollapsed()
        {
            var rules = ScriptableObject.CreateInstance<EconomyRulesConfigSO>();
            var service = new EconomyComfortAndMortalityService();

            var resident = new EconomyResidentState(30, 100f, 80f, true);
            var chance = service.CalculateDeathChance(rules, resident, new EconomyNeedSnapshot(0f, 0f, 0f, 0f));

            Assert.AreEqual(1f, chance, 0.0001f);
        }
    }
}
