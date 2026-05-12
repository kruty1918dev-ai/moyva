namespace Kruty1918.Moyva.Units.API
{
    public interface IUnitCombatService
    {
        bool CanUnitAttackNow(string attackerUnitId);
        bool TryPreviewAttack(string attackerUnitId, string defenderUnitId, out UnitCombatBreakdown breakdown);
        bool TryPreviewDuel(string attackerUnitId, string defenderUnitId, out UnitCombatDuel duel);
        bool TryExecuteAttack(string attackerUnitId, string defenderUnitId, out UnitCombatBreakdown breakdown);
    }
}