using System;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitCombatCommandService : ICombatCommandService
    {
        private readonly IUnitCombatService _combat;
        private readonly IUnitService _units;
        private readonly IUnitClassConfig _unitClasses;
        private readonly IHealthRegistry _health;
        private readonly IConstructionBuildingCombatTargetQuery _buildingTargets;
        private readonly IUnitOwnershipQuery _ownership;
        private readonly ITurnService _turns;
        private readonly IUnitTurnActionStateService _actionState;

        [Inject]
        public UnitCombatCommandService(
            IUnitCombatService combat,
            [InjectOptional] IUnitService units = null,
            [InjectOptional] IUnitClassConfig unitClasses = null,
            [InjectOptional] IHealthRegistry health = null,
            [InjectOptional] IConstructionBuildingCombatTargetQuery buildingTargets = null,
            [InjectOptional] IUnitOwnershipQuery ownership = null,
            [InjectOptional] ITurnService turns = null,
            [InjectOptional] IUnitTurnActionStateService actionState = null)
        {
            _combat = combat;
            _units = units;
            _unitClasses = unitClasses;
            _health = health;
            _buildingTargets = buildingTargets;
            _ownership = ownership;
            _turns = turns;
            _actionState = actionState;
        }

        public bool TryPreview(
            string attackerEntityId,
            string targetEntityId,
            out CombatCommandPreview preview,
            out string reason)
        {
            preview = default;
            if (_combat == null)
            {
                reason = "Unit combat service unavailable.";
                return false;
            }

            if (_combat.CanAttack(attackerEntityId, targetEntityId, out UnitAttackRejectReason rejectReason))
            {
                if (!_combat.TryPreviewAttack(attackerEntityId, targetEntityId, out UnitCombatBreakdown breakdown)
                    || !_combat.TryGetHealth(targetEntityId, out UnitHealthSnapshot health))
                {
                    reason = "Combat preview unavailable.";
                    return false;
                }

                preview = new CombatCommandPreview(
                    attackerEntityId,
                    targetEntityId,
                    breakdown.TotalDamage,
                    health.CurrentHp - breakdown.TotalDamage <= 0);
                reason = null;
                return true;
            }

            if (TryPreviewBuildingAttack(attackerEntityId, targetEntityId, out preview, out reason))
                return true;

            if (string.IsNullOrWhiteSpace(reason))
                reason = rejectReason.ToString();
            return false;
        }

        private bool TryPreviewBuildingAttack(
            string attackerEntityId,
            string targetEntityId,
            out CombatCommandPreview preview,
            out string reason)
        {
            preview = default;
            if (!TryValidateBuildingAttack(
                    attackerEntityId,
                    targetEntityId,
                    out IHealth targetHealth,
                    out int damage,
                    out reason))
            {
                return false;
            }

            preview = new CombatCommandPreview(
                attackerEntityId,
                targetEntityId,
                damage,
                targetHealth.CurrentHp - damage <= 0);
            reason = null;
            return true;
        }

        public Task<CombatCommandResult> ExecuteAsync(
            string requesterOwnerId,
            string attackerEntityId,
            string targetEntityId,
            CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
                return Task.FromResult(CombatCommandResult.Rejected(attackerEntityId, targetEntityId, "Combat command cancelled."));

            if (!CanRequesterAttack(requesterOwnerId, attackerEntityId, out string reason))
                return Task.FromResult(CombatCommandResult.Rejected(attackerEntityId, targetEntityId, reason));

            if (_combat == null)
            {
                return Task.FromResult(CombatCommandResult.Rejected(
                    attackerEntityId,
                    targetEntityId,
                    "Unit combat service unavailable."));
            }

            if (_buildingTargets != null
                && _buildingTargets.TryGetCombatTarget(targetEntityId, out _))
            {
                TryAttackBuilding(attackerEntityId, targetEntityId, out var buildingResult);
                return Task.FromResult(buildingResult);
            }

            if (_combat.TryAttack(attackerEntityId, targetEntityId, out UnitAttackResult result))
            {
                _actionState?.RecordAttack(attackerEntityId);
                return Task.FromResult(new CombatCommandResult(
                    true,
                    attackerEntityId,
                    targetEntityId,
                    result.DamageApplied,
                    result.TargetDied,
                    null));
            }

            return Task.FromResult(CombatCommandResult.Rejected(
                attackerEntityId,
                targetEntityId,
                result.RejectReason.ToString()));
        }

        private bool TryAttackBuilding(
            string attackerEntityId,
            string targetEntityId,
            out CombatCommandResult result)
        {
            result = default;
            if (!TryValidateBuildingAttack(
                    attackerEntityId,
                    targetEntityId,
                    out IHealth targetHealth,
                    out int damage,
                    out string reason))
            {
                result = CombatCommandResult.Rejected(
                    attackerEntityId,
                    targetEntityId,
                    reason);
                return false;
            }

            int hpBefore = targetHealth.CurrentHp;
            targetHealth.TakeDamage(damage);
            int hpAfter = targetHealth.CurrentHp;
            _actionState?.RecordAttack(attackerEntityId);
            result = new CombatCommandResult(
                true,
                attackerEntityId,
                targetEntityId,
                Mathf.Max(0, hpBefore - hpAfter),
                hpAfter <= 0,
                null);
            return true;
        }

        private bool TryValidateBuildingAttack(
            string attackerEntityId,
            string targetEntityId,
            out IHealth targetHealth,
            out int damage,
            out string reason)
        {
            targetHealth = null;
            damage = 0;
            if (_buildingTargets == null || _units == null || _unitClasses == null || _health == null)
            {
                reason = "Building combat is unavailable.";
                return false;
            }

            if (!_buildingTargets.TryGetCombatTarget(targetEntityId, out var target))
            {
                reason = "Target is not an attackable building.";
                return false;
            }

            if (!_units.TryGetUnitPosition(attackerEntityId, out Vector2Int attackerPosition))
            {
                reason = "Attacker not found.";
                return false;
            }

            string attackerOwner = Normalize(_ownership?.GetUnitOwnerId(attackerEntityId));
            string targetOwner = Normalize(target.OwnerId);
            if (string.IsNullOrEmpty(attackerOwner) || string.IsNullOrEmpty(targetOwner))
            {
                reason = "Combat ownership is unavailable.";
                return false;
            }

            if (string.Equals(attackerOwner, targetOwner, StringComparison.Ordinal))
            {
                reason = "Cannot attack your own building.";
                return false;
            }

            string unitTypeId = _units.GetUnitTypeId(attackerEntityId);
            UnitClassConfig attacker = _unitClasses.GetConfig(unitTypeId);
            if (attacker == null)
            {
                reason = "Attacker combat profile is unavailable.";
                return false;
            }

            int range = Mathf.Max(1, attacker.AttackRange);
            int distance = Mathf.Max(
                Mathf.Abs(attackerPosition.x - target.Position.x),
                Mathf.Abs(attackerPosition.y - target.Position.y));
            if (distance > range)
            {
                reason = "Target is out of range.";
                return false;
            }

            damage = Mathf.Max(0, attacker.CuttingDamage)
                + Mathf.Max(0, attacker.PenetratingDamage)
                + Mathf.Max(0, attacker.CrushingDamage);
            if (damage <= 0)
            {
                reason = "Attacker cannot damage buildings.";
                return false;
            }

            if (!_health.TryGet(target.EntityId, out targetHealth)
                || targetHealth == null
                || targetHealth.IsDestroyed)
            {
                reason = "Building is already destroyed.";
                return false;
            }

            reason = null;
            return true;
        }

        private bool CanRequesterAttack(string requesterOwnerId, string attackerEntityId, out string reason)
        {
            string owner = Normalize(requesterOwnerId);
            if (string.IsNullOrEmpty(owner))
            {
                reason = "Requester owner is empty.";
                return false;
            }

            if (_turns != null)
            {
                if (_turns.Phase != TurnPhase.AwaitingInput)
                {
                    reason = $"Combat requires AwaitingInput, current phase is {_turns.Phase}.";
                    return false;
                }

                if (!string.Equals(Normalize(_turns.ActiveOwnerId), owner, StringComparison.Ordinal))
                {
                    reason = "Requester owner is not active.";
                    return false;
                }

                if (!_turns.CanOwnerAct(owner, out reason))
                {
                    reason ??= "Requester owner cannot act.";
                    return false;
                }
            }

            if (_ownership != null
                && !string.Equals(Normalize(_ownership.GetUnitOwnerId(attackerEntityId)), owner, StringComparison.Ordinal))
            {
                reason = "Requester does not own attacker.";
                return false;
            }

            reason = null;
            return true;
        }

        private static string Normalize(string value)
            => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }
}
