using System.Collections.Generic;

namespace Kruty1918.Moyva.Construction.API
{

    public sealed class ConstructionResourceProjection
    {
        public static ConstructionResourceProjection Empty { get; } =
            new ConstructionResourceProjection(null, null, null, false, false, string.Empty, new List<ConstructionResourceBalance>());

        public ConstructionResourceProjection(
            string ownerId,
            string settlementId,
            string settlementName,
            bool hasSettlement,
            bool hasDeficit,
            string message,
            IReadOnlyList<ConstructionResourceBalance> balances)
        {
            OwnerId = ownerId;
            SettlementId = settlementId;
            SettlementName = settlementName;
            HasSettlement = hasSettlement;
            HasDeficit = hasDeficit;
            Message = message;
            Balances = balances ?? new List<ConstructionResourceBalance>();
        }

        public string OwnerId { get; }
        public string SettlementId { get; }
        public string SettlementName { get; }
        public bool HasSettlement { get; }
        public bool HasDeficit { get; }
        public string Message { get; }
        public IReadOnlyList<ConstructionResourceBalance> Balances { get; }
    }
}
