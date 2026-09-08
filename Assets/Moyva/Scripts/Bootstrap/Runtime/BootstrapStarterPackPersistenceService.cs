using System;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal interface IBootstrapStarterPackPersistenceService
    {
        bool HasPersistedEconomyBlock(int slot);
        bool TryPersistStarterGrant(int slot, string ownerId, string contextLabel, bool hasStarterEntries);
    }

    internal sealed class BootstrapStarterPackPersistenceService : IBootstrapStarterPackPersistenceService
    {
        private const string EconomySaveModuleFullName = "Kruty1918.Moyva.Economy.Runtime.EconomySaveModule";

        private readonly ISaveService _saveService;
        private readonly BootstrapStarterPackState _starterPackState;

    #pragma warning disable CS0649
        [InjectOptional] private ISaveInspectorService _saveInspectorService;
    #pragma warning restore CS0649

        public BootstrapStarterPackPersistenceService(
            ISaveService saveService,
            BootstrapStarterPackState starterPackState)
        {
            _saveService = saveService;
            _starterPackState = starterPackState;
        }

        public bool HasPersistedEconomyBlock(int slot)
            => _saveInspectorService != null
               && _saveInspectorService.HasBlock(slot, EconomySaveModuleFullName);

        public bool TryPersistStarterGrant(int slot, string ownerId, string contextLabel, bool hasStarterEntries)
        {
            // The grant already mutated gameplay. A failed save must never authorize
            // another grant, and the same snapshot must include this marker.
            _starterPackState.MarkGranted(ownerId);
            if (!GameLaunchContext.IsAutoSaveEnabled())
                return true;

            try
            {
                _saveService?.Save(slot);
                return _saveService != null;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Bootstrap][StarterPack] Could not save grant for '{ownerId}' ({contextLabel}): {exception.Message}");
                return false;
            }
        }

    }
}
