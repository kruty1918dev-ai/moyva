using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Signals;
using Zenject;

namespace Kruty1918.Moyva.Economy.Runtime
{
    /// <summary>
    /// Building integration service: handles construction events and building-settlement relationships.
    /// </summary>
    internal interface IEconomyBuildingIntegration
    {
        /// <summary>
        /// Handle building placement - assign to settlement or create a new one.
        /// Returns newly created settlement when town hall/castle creates one; otherwise null.
        /// </summary>
        EconomySettlementState OnBuildingPlaced(BuildingPlacedSignal signal, ISettlementRegistry registry, SignalBus signalBus, EconomyDatabaseSO database, IBuildingRegistry buildingRegistry);

        /// <summary>Handle building demolition - remove from settlement or deactivate if townhall.</summary>
        void OnBuildingDemolished(BuildingDemolishedSignal signal, ISettlementRegistry registry, SignalBus signalBus, EconomyDatabaseSO database, IBuildingRegistry buildingRegistry);
    }
}
