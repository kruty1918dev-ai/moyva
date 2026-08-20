using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using NUnit.Framework;
using UnityEngine;

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

        [Test]
        public void TurnActionState_BlocksSecondAttackUntilCleared()
        {
            var state = new UnitTurnActionStateService();

            Assert.IsTrue(state.CanAttack("unit-1", out _));

            state.RecordAttack("unit-1");

            Assert.IsFalse(state.CanAttack("unit-1", out string reason));
            Assert.AreEqual("Unit has already attacked this turn.", reason);
            Assert.IsTrue(state.Get("unit-1").HasAttacked);

            state.ClearAll();

            Assert.IsTrue(state.CanAttack("unit-1", out _));
        }

        [Test]
        public async Task CombatCommand_RecordsSuccessfulAttack()
        {
            var combat = new FakeUnitCombatService();
            var state = new UnitTurnActionStateService();
            var command = new UnitCombatCommandService(
                combat,
                new FakeOwnershipQuery("bot"),
                turns: null,
                actionState: state);

            CombatCommandResult result = await command.ExecuteAsync("bot", "attacker", "target");

            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(17, result.DamageApplied);
            Assert.IsTrue(state.Get("attacker").HasAttacked);
        }

        [Test]
        public async Task CombatCommand_RejectsRequesterThatDoesNotOwnAttacker()
        {
            var combat = new FakeUnitCombatService();
            var state = new UnitTurnActionStateService();
            var command = new UnitCombatCommandService(
                combat,
                new FakeOwnershipQuery("bot"),
                turns: null,
                actionState: state);

            CombatCommandResult result = await command.ExecuteAsync("human", "attacker", "target");

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual("Requester does not own attacker.", result.Reason);
            Assert.IsFalse(combat.AttackInvoked);
            Assert.IsFalse(state.Get("attacker").HasAttacked);
        }

        private sealed class FakeOwnershipQuery : IUnitOwnershipQuery
        {
            private readonly string _ownerId;

            public FakeOwnershipQuery(string ownerId)
            {
                _ownerId = ownerId;
            }

            public string GetUnitOwnerId(string unitId) => _ownerId;
        }

        private sealed class FakeUnitCombatService : IUnitCombatService
        {
            public event Action<string, string> AttackStarted;
            public event Action<UnitAttackResult> AttackResolved;

            public bool AttackInvoked { get; private set; }

            public bool CanAttack(string attackerUnitId, string targetUnitId, out UnitAttackRejectReason reason)
            {
                reason = UnitAttackRejectReason.None;
                return true;
            }

            public IReadOnlyList<string> GetAttackableTargets(string attackerUnitId)
                => Array.Empty<string>();

            public IReadOnlyList<Vector2Int> GetAttackableTiles(string attackerUnitId)
                => Array.Empty<Vector2Int>();

            public bool TryGetHealth(string unitId, out UnitHealthSnapshot health)
            {
                health = new UnitHealthSnapshot(unitId, 25, 25);
                return true;
            }

            public bool TryPreviewAttack(string attackerUnitId, string defenderUnitId, out UnitCombatBreakdown breakdown)
            {
                breakdown = new UnitCombatBreakdown(17, 0, 0, 0, 0, 0, 17, 0, 0, 1f, 17, 25);
                return true;
            }

            public bool TryPreviewDuel(string attackerUnitId, string defenderUnitId, out UnitCombatDuel duel)
            {
                TryPreviewAttack(attackerUnitId, defenderUnitId, out UnitCombatBreakdown attack);
                TryPreviewAttack(defenderUnitId, attackerUnitId, out UnitCombatBreakdown counter);
                duel = new UnitCombatDuel(attack, counter);
                return true;
            }

            public bool TryAttack(string attackerUnitId, string targetUnitId, out UnitAttackResult result)
            {
                AttackInvoked = true;
                AttackStarted?.Invoke(attackerUnitId, targetUnitId);
                result = new UnitAttackResult(
                    true,
                    UnitAttackRejectReason.None,
                    attackerUnitId,
                    targetUnitId,
                    17,
                    25,
                    8,
                    false);
                AttackResolved?.Invoke(result);
                return true;
            }
        }
    }
}
