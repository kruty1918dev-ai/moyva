using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitCombatService : IUnitCombatService
    {
        private readonly IUnitService _unitService;
        private readonly IUnitClassConfig _unitClassConfig;
        private readonly IHealthRegistry _healthRegistry;
        private readonly IUnitOwnershipQuery _ownership;
        private readonly IGridService _grid;
        private readonly IUnitAttackAvailabilityQuery _attackAvailability;

        public event Action<string, string> AttackStarted;
        public event Action<UnitAttackResult> AttackResolved;

        public UnitCombatService(IUnitService unitService, IUnitClassConfig unitClassConfig)
            : this(unitService, unitClassConfig, null, null, null, null) { }

        [Inject]
        public UnitCombatService(
            IUnitService unitService,
            IUnitClassConfig unitClassConfig,
            [InjectOptional] IHealthRegistry healthRegistry = null,
            [InjectOptional] IUnitOwnershipQuery ownership = null,
            [InjectOptional] IGridService grid = null,
            [InjectOptional] IUnitAttackAvailabilityQuery attackAvailability = null)
        {
            _unitService = unitService;
            _unitClassConfig = unitClassConfig;
            _healthRegistry = healthRegistry;
            _ownership = ownership;
            _grid = grid;
            _attackAvailability = attackAvailability;
        }

        public bool TryPreviewAttack(string attackerUnitId, string defenderUnitId, out UnitCombatBreakdown breakdown)
        {
            breakdown = default;
            UnitClassConfig attacker, defender;
            if (!TryGetConfig(attackerUnitId, out attacker) || !TryGetConfig(defenderUnitId, out defender))
                return false;
            breakdown = UnitCombatCalculator.CalculateAttack(attacker, defender);
            return true;
        }

        public bool TryPreviewDuel(string attackerUnitId, string defenderUnitId, out UnitCombatDuel duel)
        {
            duel = default;
            UnitClassConfig attacker, defender;
            if (!TryGetConfig(attackerUnitId, out attacker) || !TryGetConfig(defenderUnitId, out defender))
                return false;
            duel = UnitCombatCalculator.CalculateDuel(attacker, defender);
            return true;
        }

        public bool CanAttack(string attackerUnitId, string targetUnitId, out UnitAttackRejectReason reason)
        {
            reason = UnitAttackRejectReason.None;
            Vector2Int attackerPosition, targetPosition;

            if (string.IsNullOrWhiteSpace(attackerUnitId) || _unitService == null
                || !_unitService.TryGetUnitPosition(attackerUnitId, out attackerPosition))
            { reason = UnitAttackRejectReason.AttackerNotFound; return false; }

            if (string.IsNullOrWhiteSpace(targetUnitId)
                || !_unitService.TryGetUnitPosition(targetUnitId, out targetPosition))
            { reason = UnitAttackRejectReason.TargetNotFound; return false; }

            if (string.Equals(attackerUnitId, targetUnitId, StringComparison.Ordinal))
            { reason = UnitAttackRejectReason.SameOwner; return false; }

            if (_healthRegistry == null)
            { reason = UnitAttackRejectReason.AttackUnavailable; return false; }

            IHealth attackerHealth, targetHealth;
            if (!_healthRegistry.TryGet(attackerUnitId, out attackerHealth) || attackerHealth == null
                || attackerHealth.IsDestroyed || attackerHealth.CurrentHp <= 0)
            { reason = UnitAttackRejectReason.AttackerDead; return false; }

            if (!_healthRegistry.TryGet(targetUnitId, out targetHealth) || targetHealth == null
                || targetHealth.IsDestroyed || targetHealth.CurrentHp <= 0)
            { reason = UnitAttackRejectReason.TargetDead; return false; }

            if (_ownership == null)
            { reason = UnitAttackRejectReason.AttackUnavailable; return false; }

            string attackerOwner = Normalize(_ownership.GetUnitOwnerId(attackerUnitId));
            string targetOwner = Normalize(_ownership.GetUnitOwnerId(targetUnitId));
            if (string.IsNullOrEmpty(attackerOwner) || string.IsNullOrEmpty(targetOwner))
            { reason = UnitAttackRejectReason.AttackUnavailable; return false; }

            if (string.Equals(attackerOwner, targetOwner, StringComparison.Ordinal))
            { reason = UnitAttackRejectReason.SameOwner; return false; }

            UnitClassConfig attacker, ignored;
            if (!TryGetConfig(attackerUnitId, out attacker) || !TryGetConfig(targetUnitId, out ignored))
            { reason = UnitAttackRejectReason.TargetNotAttackable; return false; }

            if (GridDistance(attackerPosition, targetPosition) > Mathf.Max(1, attacker.AttackRange))
            { reason = UnitAttackRejectReason.TargetOutOfRange; return false; }

            int rawDamage = Mathf.Max(0, attacker.CuttingDamage)
                          + Mathf.Max(0, attacker.PenetratingDamage)
                          + Mathf.Max(0, attacker.CrushingDamage);
            if (rawDamage <= 0)
            { reason = UnitAttackRejectReason.TargetNotAttackable; return false; }

            string availabilityReason;
            if (_attackAvailability != null
                && !_attackAvailability.CanAttack(attackerUnitId, out availabilityReason))
            { reason = UnitAttackRejectReason.AttackUnavailable; return false; }

            return true;
        }

        public IReadOnlyList<string> GetAttackableTargets(string attackerUnitId)
        {
            var result = new List<string>();
            if (_unitService == null) return result;
            IReadOnlyCollection<string> ids = _unitService.GetAllUnitIds();
            if (ids == null) return result;
            foreach (string id in ids)
            {
                UnitAttackRejectReason reason;
                if (CanAttack(attackerUnitId, id, out reason)) result.Add(id);
            }
            result.Sort(StringComparer.Ordinal);
            return result;
        }

        public IReadOnlyList<Vector2Int> GetAttackableTiles(string attackerUnitId)
        {
            var result = new List<Vector2Int>();
            Vector2Int origin;
            UnitClassConfig attacker;
            if (_unitService == null || !_unitService.TryGetUnitPosition(attackerUnitId, out origin)
                || !TryGetConfig(attackerUnitId, out attacker))
                return result;

            int range = Mathf.Max(1, attacker.AttackRange);
            for (int y = origin.y - range; y <= origin.y + range; y++)
            for (int x = origin.x - range; x <= origin.x + range; x++)
            {
                var candidate = new Vector2Int(x, y);
                if (candidate == origin || GridDistance(origin, candidate) > range) continue;
                if (_grid != null && !_grid.ContainsCell(candidate)) continue;
                result.Add(candidate);
            }

            result.Sort((a,b) => a.y != b.y ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));
            return result;
        }

        public bool TryGetHealth(string unitId, out UnitHealthSnapshot health)
        {
            health = default;
            IHealth h;
            if (_healthRegistry == null || string.IsNullOrWhiteSpace(unitId)
                || !_healthRegistry.TryGet(unitId, out h) || h == null)
                return false;
            health = new UnitHealthSnapshot(unitId, h.CurrentHp, h.MaxHp);
            return true;
        }

        public bool TryAttack(string attackerUnitId, string targetUnitId, out UnitAttackResult result)
        {
            UnitAttackRejectReason rejectReason;
            if (!CanAttack(attackerUnitId, targetUnitId, out rejectReason))
            {
                result = UnitAttackResult.Rejected(attackerUnitId, targetUnitId, rejectReason);
                AttackResolved?.Invoke(result);
                return false;
            }

            UnitCombatBreakdown breakdown;
            IHealth targetHealth;
            if (!TryPreviewAttack(attackerUnitId, targetUnitId, out breakdown)
                || !_healthRegistry.TryGet(targetUnitId, out targetHealth) || targetHealth == null)
            {
                result = UnitAttackResult.Rejected(
                    attackerUnitId, targetUnitId, UnitAttackRejectReason.TargetNotAttackable);
                AttackResolved?.Invoke(result);
                return false;
            }

            int hpBefore = targetHealth.CurrentHp;
            AttackStarted?.Invoke(attackerUnitId, targetUnitId);
            targetHealth.TakeDamage(Mathf.Max(0, breakdown.TotalDamage));
            int hpAfter = targetHealth.CurrentHp;

            result = new UnitAttackResult(
                true, UnitAttackRejectReason.None,
                attackerUnitId, targetUnitId,
                Mathf.Max(0, hpBefore - hpAfter),
                hpBefore, hpAfter, hpAfter <= 0);
            AttackResolved?.Invoke(result);
            return true;
        }

        internal static int GridDistance(Vector2Int a, Vector2Int b)
            => Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));

        private bool TryGetConfig(string unitId, out UnitClassConfig config)
        {
            config = null;
            string typeId = _unitService?.GetUnitTypeId(unitId);
            if (string.IsNullOrWhiteSpace(typeId)) return false;
            config = _unitClassConfig?.GetConfig(typeId);
            return config != null;
        }

        private static string Normalize(string value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }
}
