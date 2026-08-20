using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Units.API
{
    public enum UnitAttackRejectReason
    {
        None = 0,
        AttackerNotFound,
        TargetNotFound,
        AttackerDead,
        TargetDead,
        SameOwner,
        TargetOutOfRange,
        TargetNotVisible,
        TargetNotAttackable,
        AttackUnavailable,
    }

    public readonly struct UnitHealthSnapshot
    {
        public UnitHealthSnapshot(string unitId, int currentHp, int maxHp)
        {
            UnitId = unitId ?? string.Empty;
            CurrentHp = Mathf.Max(0, currentHp);
            MaxHp = Mathf.Max(1, maxHp);
        }

        public string UnitId { get; }
        public int CurrentHp { get; }
        public int MaxHp { get; }
        public bool IsAlive => CurrentHp > 0;
    }

    public readonly struct UnitAttackResult
    {
        public UnitAttackResult(
            bool succeeded,
            UnitAttackRejectReason rejectReason,
            string attackerUnitId,
            string targetUnitId,
            int damageApplied,
            int targetHpBefore,
            int targetHpAfter,
            bool targetDied)
        {
            Succeeded = succeeded;
            RejectReason = rejectReason;
            AttackerUnitId = attackerUnitId ?? string.Empty;
            TargetUnitId = targetUnitId ?? string.Empty;
            DamageApplied = Mathf.Max(0, damageApplied);
            TargetHpBefore = Mathf.Max(0, targetHpBefore);
            TargetHpAfter = Mathf.Max(0, targetHpAfter);
            TargetDied = targetDied;
        }

        public bool Succeeded { get; }
        public UnitAttackRejectReason RejectReason { get; }
        public string AttackerUnitId { get; }
        public string TargetUnitId { get; }
        public int DamageApplied { get; }
        public int TargetHpBefore { get; }
        public int TargetHpAfter { get; }
        public bool TargetDied { get; }

        public static UnitAttackResult Rejected(
            string attackerUnitId,
            string targetUnitId,
            UnitAttackRejectReason reason)
            => new UnitAttackResult(
                false, reason, attackerUnitId, targetUnitId,
                0, 0, 0, false);
    }

    public interface IUnitAttackAvailabilityQuery
    {
        bool CanAttack(string attackerUnitId, out string reason);
    }

    public interface IUnitCombatQuery
    {
        bool CanAttack(string attackerUnitId, string targetUnitId, out UnitAttackRejectReason reason);
        IReadOnlyList<string> GetAttackableTargets(string attackerUnitId);
        IReadOnlyList<Vector2Int> GetAttackableTiles(string attackerUnitId);
        bool TryGetHealth(string unitId, out UnitHealthSnapshot health);
    }

    public interface IUnitCombatService : IUnitCombatQuery
    {
        event Action<string, string> AttackStarted;
        event Action<UnitAttackResult> AttackResolved;

        bool TryPreviewAttack(string attackerUnitId, string defenderUnitId, out UnitCombatBreakdown breakdown);
        bool TryPreviewDuel(string attackerUnitId, string defenderUnitId, out UnitCombatDuel duel);
        bool TryAttack(string attackerUnitId, string targetUnitId, out UnitAttackResult result);
    }
}
