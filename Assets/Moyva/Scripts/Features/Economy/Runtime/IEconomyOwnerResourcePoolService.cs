using System.Collections.Generic;
using Kruty1918.Moyva.Signals;
using Zenject;

namespace Kruty1918.Moyva.Economy.Runtime
{
    internal interface IEconomyOwnerResourcePoolService
    {
        void AddOwnerResource(
            Dictionary<string, Dictionary<string, float>> pools,
            string ownerId,
            string resourceId,
            float amount,
            SignalBus signalBus);

        void TransferOwnerResourcesToSettlement(
            Dictionary<string, Dictionary<string, float>> pools,
            string ownerId,
            EconomySettlementState state,
            SignalBus signalBus);

        Dictionary<string, float> GetOwnerResourceTotals(
            Dictionary<string, Dictionary<string, float>> pools,
            IReadOnlyDictionary<string, EconomySettlementState> settlements,
            string ownerId);

        Dictionary<string, Dictionary<string, float>> GetOwnerResourcePoolsSnapshot(
            Dictionary<string, Dictionary<string, float>> pools);

        void RestoreOwnerResourcePools(
            Dictionary<string, Dictionary<string, float>> pools,
            Dictionary<string, Dictionary<string, float>> snapshot,
            SignalBus signalBus);

        void TransferOwnerResourcesToExistingSettlements(
            Dictionary<string, Dictionary<string, float>> pools,
            IReadOnlyDictionary<string, EconomySettlementState> settlements,
            SignalBus signalBus);
    }
}