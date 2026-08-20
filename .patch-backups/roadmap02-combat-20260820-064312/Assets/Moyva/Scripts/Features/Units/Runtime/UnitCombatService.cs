using Kruty1918.Moyva.Units.API;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitCombatService : IUnitCombatService
    {
        private readonly IUnitService _unitService;
        private readonly IUnitClassConfig _unitClassConfig;

        public UnitCombatService(IUnitService unitService, IUnitClassConfig unitClassConfig)
        {
            _unitService = unitService;
            _unitClassConfig = unitClassConfig;
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