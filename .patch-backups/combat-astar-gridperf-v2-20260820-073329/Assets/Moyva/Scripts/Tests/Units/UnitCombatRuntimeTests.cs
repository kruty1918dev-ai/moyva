using Kruty1918.Moyva.Combat.Runtime;
using Kruty1918.Moyva.Units.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Units
{
    public sealed class UnitCombatRuntimeTests
    {
        [Test]
        public void Health_StartsAtMax_AndClampsAtZero()
        {
            var health = new HealthComponent();
            health.Initialize("unit-a", 100);
            Assert.AreEqual(100, health.CurrentHp);
            health.TakeDamage(130);
            Assert.AreEqual(0, health.CurrentHp);
            Assert.IsTrue(health.IsDestroyed);
        }

        [Test]
        public void CombatCalculator_IsDeterministic()
        {
            var attacker = new UnitClassConfig
            {
                CuttingDamage = 25,
                PenetratingDamage = 5,
                BaseLevel = 1,
                AttackRange = 1,
            };
            var defender = new UnitClassConfig
            {
                HitPoints = 100,
                CuttingDefense = 5,
                PenetratingDefense = 2,
                BaseLevel = 1,
            };
            var a = UnitCombatCalculator.CalculateAttack(attacker, defender);
            var b = UnitCombatCalculator.CalculateAttack(attacker, defender);
            Assert.AreEqual(a.TotalDamage, b.TotalDamage);
            Assert.AreEqual(23, a.TotalDamage);
        }

        [Test]
        public void AttackRange_IsDataDriven()
        {
            Assert.AreEqual(1, new UnitClassConfig { AttackRange = 1 }.AttackRange);
            Assert.AreEqual(3, new UnitClassConfig { AttackRange = 3 }.AttackRange);
        }
    }
}
