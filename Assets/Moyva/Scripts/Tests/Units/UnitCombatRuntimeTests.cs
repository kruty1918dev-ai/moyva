using Kruty1918.Moyva.Units.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Units
{
    public sealed class UnitCombatRuntimeTests
    {
        [Test]
        public void CombatCalculator_IsDeterministic()
        {
            var attacker = new UnitClassConfig
            {
                CuttingDamage = 25,
                PenetratingDamage = 5,
                CrushingDamage = 0,
                BaseLevel = 1,
                AttackRange = 1,
            };

            var defender = new UnitClassConfig
            {
                HitPoints = 100,
                CuttingDefense = 5,
                PenetratingDefense = 2,
                CrushingDefense = 0,
                BaseLevel = 1,
            };

            UnitCombatBreakdown first =
                UnitCombatCalculator.CalculateAttack(attacker, defender);
            UnitCombatBreakdown second =
                UnitCombatCalculator.CalculateAttack(attacker, defender);

            Assert.AreEqual(first.TotalDamage, second.TotalDamage);
            Assert.AreEqual(23, first.TotalDamage);
        }

        [Test]
        public void AttackRange_IsDataDriven()
        {
            Assert.AreEqual(1, new UnitClassConfig { AttackRange = 1 }.AttackRange);
            Assert.AreEqual(3, new UnitClassConfig { AttackRange = 3 }.AttackRange);
        }
    }
}
