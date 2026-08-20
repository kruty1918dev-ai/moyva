using System;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitCombatCommandService : ICombatCommandService
    {
        private readonly IUnitCombatService _combat;
        private readonly IUnitOwnershipQuery _ownership;
        private readonly ITurnService _turns;
        private readonly IUnitTurnActionStateService _actionState;

        [Inject]
        public UnitCombatCommandService(
            IUnitCombatService combat,
            [InjectOptional] IUnitOwnershipQuery ownership = null,
            [InjectOptional] ITurnService turns = null,
            [InjectOptional] IUnitTurnActionStateService actionState = null)
        {
            _combat = combat;
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

            if (!_combat.CanAttack(attackerEntityId, targetEntityId, out UnitAttackRejectReason rejectReason))
            {
                reason = rejectReason.ToString();
                return false;
            }

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

            if (!_combat.TryAttack(attackerEntityId, targetEntityId, out UnitAttackResult result))
            {
                return Task.FromResult(CombatCommandResult.Rejected(
                    attackerEntityId,
                    targetEntityId,
                    result.RejectReason.ToString()));
            }

            _actionState?.RecordAttack(attackerEntityId);
            return Task.FromResult(new CombatCommandResult(
                true,
                attackerEntityId,
                targetEntityId,
                result.DamageApplied,
                result.TargetDied,
                null));
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
