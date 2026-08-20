namespace Kruty1918.Moyva.Units.API
{
    public interface IUnitCombatService
    {
        bool TryPreviewAttack(string attackerUnitId, string defenderUnitId, out UnitCombatBreakdown breakdown);
        bool TryPreviewDuel(string attackerUnitId, string defenderUnitId, out UnitCombatDuel duel);
    }
}