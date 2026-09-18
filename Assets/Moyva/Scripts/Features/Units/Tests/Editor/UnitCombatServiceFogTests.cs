using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Units
{
    /// <summary>
    /// Authoritative Fog of War gate on unit attacks: a unit may not attack a
    /// target cell its owner cannot currently see.
    /// </summary>
    [TestFixture]
    public sealed class UnitCombatServiceFogTests
    {
        private FakeUnitService _units;
        private FakeOwnership _ownership;
        private FakeHealthRegistry _health;
        private FakeFog _fog;
        private FakeUnitClassConfig _configs;
        private UnitCombatService _combat;

        [SetUp]
        public void SetUp()
        {
            _units = new FakeUnitService();
            _ownership = new FakeOwnership();
            _health = new FakeHealthRegistry();
            _fog = new FakeFog();
            _configs = new FakeUnitClassConfig();

            _units.Add("attacker", Vector2Int.zero, "raider");
            _units.Add("target", new Vector2Int(2, 0), "raider");
            _ownership.Set("attacker", "p1");
            _ownership.Set("target", "p2");
            _health.Add("attacker");
            _health.Add("target");
            _configs.Set("raider", new UnitClassConfig
            {
                AttackRange = 5,
                CuttingDamage = 10,
            });

            _combat = new UnitCombatService(
                _units,
                _configs,
                _health,
                _ownership,
                grid: null,
                attackAvailability: null,
                ownerFog: _fog);
        }

        [Test]
        public void CanAttack_HiddenTargetPosition_RejectedAsNotVisible()
        {
            _fog.Visible = false;

            bool canAttack = _combat.CanAttack(
                "attacker", "target", out UnitAttackRejectReason reason);

            Assert.IsFalse(canAttack);
            Assert.AreEqual(UnitAttackRejectReason.TargetNotVisible, reason);
        }

        [Test]
        public void CanAttack_VisibleTargetPosition_PassesFogGate()
        {
            _fog.Visible = true;

            bool canAttack = _combat.CanAttack(
                "attacker", "target", out UnitAttackRejectReason reason);

            Assert.IsTrue(canAttack, $"unexpected rejection: {reason}");
            Assert.AreEqual(UnitAttackRejectReason.None, reason);
        }

        [Test]
        public void CanAttack_NoFogService_KeepsPreviousBehavior()
        {
            var combat = new UnitCombatService(
                _units, _configs, _health, _ownership);

            bool canAttack = combat.CanAttack(
                "attacker", "target", out UnitAttackRejectReason reason);

            Assert.IsTrue(canAttack, $"unexpected rejection: {reason}");
        }

        [Test]
        public void CanAttack_ChecksVisibilityForAttackerOwnerNotTargetOwner()
        {
            // Fog returns visibility only for a different owner: the attacker
            // owner's sight is what matters.
            _fog.VisibleForOwners = new HashSet<string>(StringComparer.Ordinal) { "p2" };

            bool canAttack = _combat.CanAttack(
                "attacker", "target", out UnitAttackRejectReason reason);

            Assert.IsFalse(canAttack);
            Assert.AreEqual(UnitAttackRejectReason.TargetNotVisible, reason);
        }

        // ── Fakes ────────────────────────────────────────────────────

        private sealed class FakeUnitService : IUnitService
        {
            private readonly Dictionary<string, Vector2Int> _positions = new();
            private readonly Dictionary<string, string> _types = new();

            public void Add(string unitId, Vector2Int position, string typeId = "warrior")
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
            public FakeHealth(string entityId, int maxHp = 10)
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

            public void Add(string entityId) => _entries[entityId] = new FakeHealth(entityId);

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

        private sealed class FakeFog : IFogOwnerStateReader
        {
            public bool Visible = true;
            public HashSet<string> VisibleForOwners;

            public FogStateType GetFogState(string ownerId, Vector2Int position)
                => IsVisible(ownerId, position) ? FogStateType.Visible : FogStateType.Unexplored;

            public bool IsVisible(string ownerId, Vector2Int position)
                => VisibleForOwners != null
                    ? VisibleForOwners.Contains(ownerId)
                    : Visible;

            public bool IsExplored(string ownerId, Vector2Int position)
                => IsVisible(ownerId, position);
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
