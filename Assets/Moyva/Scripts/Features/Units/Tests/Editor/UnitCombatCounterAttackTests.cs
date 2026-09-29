using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.EntityHealth;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Units
{
    /// <summary>
    /// Civ-style combat rebalance: defender counterattack (×0.75, only when the
    /// attacker is inside defender range) and data-driven counter bonuses
    /// (UnitClassConfig.CombatBonuses).
    /// </summary>
    [TestFixture]
    public sealed class UnitCombatCounterAttackTests
    {
        private FakeUnitService _units;
        private FakeOwnership _ownership;
        private FakeHealthRegistry _health;
        private FakeUnitClassConfig _configs;
        private UnitCombatService _combat;

        [SetUp]
        public void SetUp()
        {
            _units = new FakeUnitService();
            _ownership = new FakeOwnership();
            _health = new FakeHealthRegistry();
            _configs = new FakeUnitClassConfig();
            _combat = new UnitCombatService(_units, _configs, _health, _ownership);
        }

        private void AddPair(
            Vector2Int attackerPos, Vector2Int targetPos,
            UnitClassConfig attackerCfg, UnitClassConfig targetCfg,
            int attackerHp = 100, int targetHp = 100)
        {
            _units.Add("attacker", attackerPos, "attacker-type");
            _units.Add("target", targetPos, "target-type");
            _ownership.Set("attacker", "p1");
            _ownership.Set("target", "p2");
            _health.Add("attacker", attackerHp);
            _health.Add("target", targetHp);
            _configs.Set("attacker-type", attackerCfg);
            _configs.Set("target-type", targetCfg);
        }

        [Test]
        public void TryAttack_DefenderInRangeAndAlive_AppliesReducedCounterDamage()
        {
            AddPair(
                Vector2Int.zero, new Vector2Int(1, 0),
                new UnitClassConfig { AttackRange = 1, CuttingDamage = 20, CuttingDefense = 0 },
                new UnitClassConfig { AttackRange = 1, CuttingDamage = 16, CuttingDefense = 0 });

            bool ok = _combat.TryAttack("attacker", "target", out UnitAttackResult result);

            Assert.IsTrue(ok);
            Assert.AreEqual(20, result.DamageApplied);
            // 16 raw * 0.75 counter multiplier = 12
            Assert.AreEqual(12, result.CounterDamageApplied);
            Assert.AreEqual(100, result.AttackerHpBefore);
            Assert.AreEqual(88, result.AttackerHpAfter);
            Assert.IsFalse(result.AttackerDied);
        }

        [Test]
        public void TryAttack_AttackerOutsideDefenderRange_NoCounterAttack()
        {
            AddPair(
                Vector2Int.zero, new Vector2Int(3, 0),
                new UnitClassConfig { AttackRange = 3, PenetratingDamage = 16 },
                new UnitClassConfig { AttackRange = 1, CuttingDamage = 20 });

            bool ok = _combat.TryAttack("attacker", "target", out UnitAttackResult result);

            Assert.IsTrue(ok);
            Assert.AreEqual(16, result.DamageApplied);
            Assert.AreEqual(0, result.CounterDamageApplied);
        }

        [Test]
        public void TryAttack_TargetDies_NoCounterAttack()
        {
            AddPair(
                Vector2Int.zero, new Vector2Int(1, 0),
                new UnitClassConfig { AttackRange = 1, CuttingDamage = 50 },
                new UnitClassConfig { AttackRange = 1, CuttingDamage = 100 },
                targetHp: 30);

            bool ok = _combat.TryAttack("attacker", "target", out UnitAttackResult result);

            Assert.IsTrue(ok);
            Assert.IsTrue(result.TargetDied);
            Assert.AreEqual(0, result.CounterDamageApplied);
            Assert.IsFalse(result.AttackerDied);
        }

        [Test]
        public void TryAttack_CargoDefender_NoCounterAttack()
        {
            AddPair(
                Vector2Int.zero, new Vector2Int(1, 0),
                new UnitClassConfig { AttackRange = 1, CuttingDamage = 10 },
                new UnitClassConfig
                {
                    AttackRange = 1,
                    CuttingDamage = 50,
                    CanTransportCargo = true,
                });

            bool ok = _combat.TryAttack("attacker", "target", out UnitAttackResult result);

            Assert.IsTrue(ok);
            Assert.AreEqual(0, result.CounterDamageApplied);
        }

        [Test]
        public void TryAttack_CounterAttackCanKillAttacker()
        {
            AddPair(
                Vector2Int.zero, new Vector2Int(1, 0),
                new UnitClassConfig { AttackRange = 1, CuttingDamage = 5 },
                new UnitClassConfig { AttackRange = 1, CrushingDamage = 100 },
                attackerHp: 40);

            bool ok = _combat.TryAttack("attacker", "target", out UnitAttackResult result);

            Assert.IsTrue(ok);
            Assert.AreEqual(40, result.CounterDamageApplied);
            Assert.AreEqual(0, result.AttackerHpAfter);
            Assert.IsTrue(result.AttackerDied);
        }

        [Test]
        public void CalculateAttack_CounterBonus_MultipliesDamageVsMatchingCombatType()
        {
            var spearman = new UnitClassConfig
            {
                PenetratingDamage = 16,
                CombatBonuses =
                {
                    new UnitCombatBonus
                    {
                        TargetCombatType = UnitCombatType.Cavalry,
                        DamageMultiplier = 1.5f,
                    },
                },
            };
            var cavalry = new UnitClassConfig { CombatType = UnitCombatType.Cavalry, HitPoints = 90 };
            var infantry = new UnitClassConfig { CombatType = UnitCombatType.Infantry, HitPoints = 90 };

            UnitCombatBreakdown vsCavalry = UnitCombatCalculator.CalculateAttack(spearman, cavalry);
            UnitCombatBreakdown vsInfantry = UnitCombatCalculator.CalculateAttack(spearman, infantry);

            Assert.AreEqual(24, vsCavalry.TotalDamage);
            Assert.AreEqual(1.5f, vsCavalry.CounterBonusMultiplier, 0.001f);
            Assert.AreEqual(16, vsInfantry.TotalDamage);
            Assert.AreEqual(1f, vsInfantry.CounterBonusMultiplier, 0.001f);
        }

        [Test]
        public void CalculateDuel_DefenderCounterAttack_UsesReducedMultiplier()
        {
            var attacker = new UnitClassConfig { CuttingDamage = 20, HitPoints = 110 };
            var defender = new UnitClassConfig { CuttingDamage = 16, HitPoints = 100 };

            UnitCombatDuel duel = UnitCombatCalculator.CalculateDuel(attacker, defender);

            Assert.AreEqual(20, duel.AttackerAttack.TotalDamage);
            Assert.AreEqual(12, duel.DefenderCounterAttack.TotalDamage);
        }

        [Test]
        public void CalculateAttack_DefenseSubtractsBeforeMultipliers()
        {
            var attacker = new UnitClassConfig { CuttingDamage = 20 };
            var defender = new UnitClassConfig { CuttingDefense = 3, HitPoints = 100 };

            UnitCombatBreakdown b = UnitCombatCalculator.CalculateAttack(attacker, defender);

            Assert.AreEqual(17, b.TotalDamage);
        }

        // ── Fakes ────────────────────────────────────────────────────

        private sealed class FakeUnitService : IUnitService
        {
            private readonly Dictionary<string, Vector2Int> _positions = new();
            private readonly Dictionary<string, string> _types = new();

            public void Add(string unitId, Vector2Int position, string typeId)
            {
                _positions[unitId] = position;
                _types[unitId] = typeId;
            }

            public float GetStamina(string unitId) => 1f;
            public void SetStamina(string unitId, float stamina) { }
            public bool TryGetUnitPosition(string unitId, out Vector2Int position)
                => _positions.TryGetValue(unitId, out position);
            public GameObject GetUnitObject(string unitId) => null;
            public IReadOnlyCollection<string> GetAllUnitIds() => _positions.Keys.ToArray();
            public string GetUnitTypeId(string unitId)
                => _types.TryGetValue(unitId, out string typeId) ? typeId : null;
        }

        private sealed class FakeOwnership : IUnitOwnershipQuery
        {
            private readonly Dictionary<string, string> _owners = new();
            public void Set(string unitId, string ownerId) => _owners[unitId] = ownerId;
            public string GetUnitOwnerId(string unitId)
                => _owners.TryGetValue(unitId, out string ownerId) ? ownerId : null;
        }

        private sealed class FakeHealth : IHealth
        {
            public FakeHealth(string entityId, int maxHp)
            {
                EntityId = entityId;
                MaxHp = maxHp;
                CurrentHp = maxHp;
            }

            public string EntityId { get; }
            public int CurrentHp { get; private set; }
            public int MaxHp { get; }
            public bool IsDestroyed => CurrentHp <= 0;

            public event Action<string, int, int, int> OnHealthChanged;
            public event Action<string> OnDestroyed;

            public void TakeDamage(int amount)
            {
                int previous = CurrentHp;
                CurrentHp = Math.Max(0, CurrentHp - Math.Max(0, amount));
                OnHealthChanged?.Invoke(EntityId, previous, CurrentHp, MaxHp);
                if (CurrentHp == 0 && previous > 0)
                    OnDestroyed?.Invoke(EntityId);
            }

            public void Heal(int amount)
            {
                int previous = CurrentHp;
                CurrentHp = Math.Min(MaxHp, CurrentHp + Math.Max(0, amount));
                if (CurrentHp != previous)
                    OnHealthChanged?.Invoke(EntityId, previous, CurrentHp, MaxHp);
            }

            public void Kill() => TakeDamage(CurrentHp);
            public void Initialize(string entityId, int maxHp) { }
        }

        private sealed class FakeHealthRegistry : IHealthRegistry
        {
            private readonly Dictionary<string, FakeHealth> _entries = new();

            public void Add(string entityId, int maxHp) => _entries[entityId] = new FakeHealth(entityId, maxHp);

            public IHealth Get(string entityId)
                => _entries.TryGetValue(entityId, out FakeHealth health) ? health : null;

            public bool TryGet(string entityId, out IHealth health)
            {
                if (_entries.TryGetValue(entityId, out FakeHealth found) && !found.IsDestroyed)
                {
                    health = found;
                    return true;
                }
                health = null;
                return false;
            }

            public IReadOnlyCollection<IHealth> GetAll()
                => _entries.Values.Where(h => !h.IsDestroyed).Cast<IHealth>().ToArray();

            public IReadOnlyCollection<IHealth> GetMany(IEnumerable<string> entityIds)
                => entityIds?
                    .Select(id => Get(id))
                    .Where(h => h != null && !h.IsDestroyed)
                    .ToArray()
                ?? (IReadOnlyCollection<IHealth>)Array.Empty<IHealth>();

            public int Count => _entries.Count;

            public void Register(IHealth health)
            {
                if (health != null)
                    _entries[health.EntityId] = health as FakeHealth
                        ?? new FakeHealth(health.EntityId, health.MaxHp);
            }

            public void Unregister(string entityId) => _entries.Remove(entityId);
        }

        private sealed class FakeUnitClassConfig : IUnitClassConfig
        {
            private readonly Dictionary<string, UnitClassConfig> _configs = new();
            public void Set(string typeId, UnitClassConfig config) => _configs[typeId] = config;
            public UnitClassConfig GetConfig(string typeId)
                => _configs.TryGetValue(typeId, out UnitClassConfig config) ? config : null;
        }
    }
}
