using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Economy
{
    [TestFixture]
    public sealed class EconomySettlementLifecycleServiceTests
    {
        [Test]
        public void ResolveState_ShouldDeactivate_WhenPopulationIsZero()
        {
            var rules = ScriptableObject.CreateInstance<EconomyRulesConfigSO>();
            var service = new EconomySettlementLifecycleService();

            var state = service.ResolveState(rules, townHallDestroyed: false, population: 0);
            Assert.AreEqual(EconomySettlementLifecycleState.Deactivated, state);
        }

        [Test]
        public void CanCreateSettlement_ShouldRespectMaxSettlements()
        {
            var rules = ScriptableObject.CreateInstance<EconomyRulesConfigSO>();
            var service = new EconomySettlementLifecycleService();

            Assert.IsTrue(service.CanCreateSettlement(rules, 2));
            Assert.IsFalse(service.CanCreateSettlement(rules, 3));
        }
    }
}
