using System.Collections.Generic;
using Kruty1918.Moyva.Signals;
using Zenject;
using System;

namespace Kruty1918.Moyva.Economy.Runtime
{
    internal interface IEconomyOwnerResourcePoolService
    {
        void AddOwnerResource(
            string ownerId,
            string resourceId,
            float amount,
            SignalBus signalBus);

        void TransferOwnerResourcesToWarehouse(
            string ownerId,
            EconomySettlementState state,
            string warehouseKey,
            SignalBus signalBus);

        bool TransferOwnerResourcesToFirstWarehouse(
            string ownerId,
            IReadOnlyDictionary<string, EconomySettlementState> settlements,
            SignalBus signalBus,
            string logTag);

        void TransferOwnerResourcesToExistingWarehouses(
            IReadOnlyDictionary<string, EconomySettlementState> settlements,
            SignalBus signalBus,
            string logTag);

        bool TryConsumeOwnerPoolResources(
            string ownerId,
            IReadOnlyDictionary<string, float> resourceCosts,
            Func<string, string> resolveResourceDisplayName,
            SignalBus signalBus,
            out string errorMessage);

        bool OwnerHasAnyWarehouse(
            string ownerId,
            IReadOnlyDictionary<string, EconomySettlementState> settlements);

        Dictionary<string, float> GetOwnerPoolResourceTotals(string ownerId);

        Dictionary<string, float> GetOwnerResourceTotals(
            IReadOnlyDictionary<string, EconomySettlementState> settlements,
            string ownerId);

        Dictionary<string, Dictionary<string, float>> GetOwnerResourceTotalsSnapshot(
            IReadOnlyDictionary<string, EconomySettlementState> settlements);

        Dictionary<string, Dictionary<string, float>> GetOwnerResourcePoolsSnapshot();

        void RestoreOwnerResourcePools(
            Dictionary<string, Dictionary<string, float>> snapshot,
            SignalBus signalBus);
    }
}