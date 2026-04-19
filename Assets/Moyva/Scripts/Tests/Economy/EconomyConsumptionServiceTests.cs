using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Economy
{
    [TestFixture]
    public sealed class EconomyConsumptionServiceTests
    {
        [Test]
        public void ResolveConsumption_ShouldReturnAgeConfiguredProfile()
        {
            var rules = ScriptableObject.CreateInstance<EconomyRulesConfigSO>();
            var service = new EconomyConsumptionService();

            var child = service.ResolveConsumption(rules, 10);
            var adult = service.ResolveConsumption(rules, 30);
            var elder = service.ResolveConsumption(rules, 75);

            Assert.AreEqual(0.6f, child.FoodPerTurn, 0.001f);
            Assert.AreEqual(1f, adult.FoodPerTurn, 0.001f);
            Assert.AreEqual(0.8f, elder.FoodPerTurn, 0.001f);
        }
    }
}
