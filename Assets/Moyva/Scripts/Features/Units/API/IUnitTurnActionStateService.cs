namespace Kruty1918.Moyva.Units.API
{
    public readonly struct UnitTurnActionState
    {
        public UnitTurnActionState(bool hasAttacked)
        {
            HasAttacked = hasAttacked;
        }

        public bool HasAttacked { get; }
        public bool CanAttack => !HasAttacked;
    }

    public interface IUnitTurnActionStateService : IUnitAttackAvailabilityQuery
    {
        UnitTurnActionState Get(string unitId);
        void RecordAttack(string unitId);
        void ClearAll();
    }
}
