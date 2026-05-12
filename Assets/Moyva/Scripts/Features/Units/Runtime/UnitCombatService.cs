using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Faction.API;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitCombatService : IUnitCombatService
    {
        private readonly IUnitService _unitService;
        private readonly IUnitClassConfig _unitClassConfig;
        private readonly IFactionOwnershipService _factionOwnershipService;
        private readonly IFactionRegistry _factionRegistry;

        private const float AttackActionCost = 1f;

        private enum AttackTurnSide
        {
            Player = 0,
            Enemy = 1,
        }

        private AttackTurnSide _currentAttackTurn = AttackTurnSide.Player;

        public UnitCombatService(
            IUnitService unitService,
            IUnitClassConfig unitClassConfig,
            IFactionOwnershipService factionOwnershipService,
            IFactionRegistry factionRegistry)
        {
            _unitService = unitService;
            _unitClassConfig = unitClassConfig;
            _factionOwnershipService = factionOwnershipService;
            _factionRegistry = factionRegistry;
        }

        public bool CanUnitAttackNow(string attackerUnitId)
        {
            return ResolveAttackTurnSide(attackerUnitId) == _currentAttackTurn;
        }

        public bool TryPreviewAttack(string attackerUnitId, string defenderUnitId, out UnitCombatBreakdown breakdown)
        {
            breakdown = default;
            if (!TryGetConfig(attackerUnitId, out var attacker) || !TryGetConfig(defenderUnitId, out var defender))
                return false;

            breakdown = UnitCombatCalculator.CalculateAttack(attacker, defender);
            return true;
        }

        public bool TryPreviewDuel(string attackerUnitId, string defenderUnitId, out UnitCombatDuel duel)
        {
            duel = default;
            if (!TryGetConfig(attackerUnitId, out var attacker) || !TryGetConfig(defenderUnitId, out var defender))
                return false;

            duel = UnitCombatCalculator.CalculateDuel(attacker, defender);
            return true;
        }

        public bool TryExecuteAttack(string attackerUnitId, string defenderUnitId, out UnitCombatBreakdown breakdown)
        {
            breakdown = default;

            if (string.IsNullOrWhiteSpace(attackerUnitId) || string.IsNullOrWhiteSpace(defenderUnitId))
                return false;

            if (string.Equals(attackerUnitId, defenderUnitId, System.StringComparison.Ordinal))
                return false;

            if (!CanUnitAttackNow(attackerUnitId))
                return false;

            if (!TryGetConfig(attackerUnitId, out var attacker) || !TryGetConfig(defenderUnitId, out var defender))
                return false;

            var attackerOwner = _factionOwnershipService.GetOwner(attackerUnitId);
            var defenderOwner = _factionOwnershipService.GetOwner(defenderUnitId);
            if (!attackerOwner.IsEmpty && attackerOwner == defenderOwner)
                return false;

            if (_unitService.GetStamina(attackerUnitId) < AttackActionCost)
                return false;

            breakdown = UnitCombatCalculator.CalculateAttack(attacker, defender);

            _unitService.SetStamina(attackerUnitId, _unitService.GetStamina(attackerUnitId) - AttackActionCost);

            _unitService.TryApplyDamage(defenderUnitId, breakdown.TotalDamage, out _, out _);

            SwitchAttackTurn();
            return true;
        }

        private void SwitchAttackTurn()
        {
            _currentAttackTurn = _currentAttackTurn == AttackTurnSide.Player
                ? AttackTurnSide.Enemy
                : AttackTurnSide.Player;
        }

        private AttackTurnSide ResolveAttackTurnSide(string unitId)
        {
            var owner = _factionOwnershipService.GetOwner(unitId);
            if (owner.IsEmpty)
                return AttackTurnSide.Enemy;

            if (_factionRegistry.TryGet(owner, out var def) && def != null)
                return def.FactionType == FactionType.Human
                    ? AttackTurnSide.Player
                    : AttackTurnSide.Enemy;

            return AttackTurnSide.Enemy;
        }

        private bool TryGetConfig(string unitId, out UnitClassConfig config)
        {
            config = null;
            string typeId = _unitService?.GetUnitTypeId(unitId);
            if (string.IsNullOrWhiteSpace(typeId))
                return false;

            config = _unitClassConfig?.GetConfig(typeId);
            return config != null;
        }
    }
}