#if MOYVA_LEGACY_SCRIPTABLEOBJECT_TESTS
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using NUnit.Framework;
using UnityEngine;

using Kruty1918.Moyva.Jsonization;
namespace Kruty1918.Moyva.Tests.Economy
{
    [TestFixture]
    public sealed class EconomyMarketPricingServiceTests
    {
        [Test]
        public void CalculateUnitPrice_ShouldIncrease_WhenStockIsLow()
        {
            var rules = MoyvaJsonObjectFactory.Create<EconomyRulesConfigSO>();
            var service = new EconomyMarketPricingService();

            var lowStockPrice = service.CalculateUnitPrice(rules, "Wood", 10, 50);
            var highStockPrice = service.CalculateUnitPrice(rules, "Wood", 400, 50);

            Assert.Greater(lowStockPrice, highStockPrice);
            Assert.GreaterOrEqual(lowStockPrice, 1);
        }
    }
}

#endif
