using Kruty1918.Moyva.Economy.API;

namespace Kruty1918.Moyva.Economy.Runtime
{
    public enum EconomySettlementLifecycleState
    {
        Active = 0,
        Deactivated = 1,
    }

    public sealed class EconomySettlementLifecycleService
    {
        public EconomySettlementLifecycleState ResolveState(EconomyRulesConfigSO rules, bool townHallDestroyed, int population)
        {
            if (townHallDestroyed)
                return EconomySettlementLifecycleState.Deactivated;

            if (rules?.Settlement == null)
                return EconomySettlementLifecycleState.Active;

            if (rules.Settlement.DeactivateSettlementWhenPopulationIsZero && population <= 0)
                return EconomySettlementLifecycleState.Deactivated;

            return EconomySettlementLifecycleState.Active;
        }

        public bool CanCreateSettlement(EconomyRulesConfigSO rules, int currentSettlements)
        {
            if (rules?.Settlement == null)
                return true;

            return currentSettlements < rules.Settlement.MaxSettlements;
        }
    }
}
