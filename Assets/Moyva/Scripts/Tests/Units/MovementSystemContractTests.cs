using Kruty1918.Moyva.Units.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Units
{
    public sealed class MovementSystemContractTests
    {
        [Test]
        public void UnitClassConfig_HasTacticalMovementBudgetByDefault()
        {
            var config = new UnitClassConfig();
            Assert.Greater(config.MovementPointsPerTurn, 0f);
            Assert.LessOrEqual(config.MovementPointsPerTurn, 20f);
        }

        [Test]
        public void MovementSnapshot_PreservesReachabilityAndCost()
        {
            var snapshot = new UnitMovementTileSnapshot(
                new UnityEngine.Vector2Int(3, 4),
                true,
                2.5f);

            Assert.IsTrue(snapshot.IsReachable);
            Assert.AreEqual(2.5f, snapshot.Cost, 0.0001f);
        }
    }
}
