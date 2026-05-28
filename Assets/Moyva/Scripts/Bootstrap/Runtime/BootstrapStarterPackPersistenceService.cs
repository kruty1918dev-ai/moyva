using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal interface IBootstrapStarterPackPersistenceService
    {
        bool HasPersistedEconomyBlock(int slot);
        bool HasPersistedStarterResources(int slot, string ownerId);
        bool TryPersistStarterGrant(int slot, string ownerId, string contextLabel, bool hasStarterEntries);
    }

    internal sealed class BootstrapStarterPackPersistenceService : IBootstrapStarterPackPersistenceService
    {
        private const string EconomySaveModuleFullName = "Kruty1918.Moyva.Economy.Runtime.EconomySaveModule";

        private readonly BootstrapGameSettings _settings;
        private readonly ISaveService _saveService;
        private readonly BootstrapStarterPackState _starterPackState;

    #pragma warning disable CS0649
        [InjectOptional] private ISaveInspectorService _saveInspectorService;
    #pragma warning restore CS0649

        public BootstrapStarterPackPersistenceService(
            BootstrapGameSettings settings,
            ISaveService saveService,
            BootstrapStarterPackState starterPackState)
        {
            _settings = settings;
            _saveService = saveService;
            _starterPackState = starterPackState;
        }

        public bool HasPersistedEconomyBlock(int slot)
            => _saveInspectorService != null
               && _saveInspectorService.HasBlock(slot, EconomySaveModuleFullName);

        public bool HasPersistedStarterResources(int slot, string ownerId)
        {
            if (!BootstrapStarterPackResourceUtility.HasEntries(_settings.InitialResources))
                return true;

            if (!TryReadEconomyOwnerResources(slot, ownerId, out var resources))
                return false;

            bool hasExpectedEntry = false;
            var entries = _settings.InitialResources;
            for (int index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                if (entry == null || string.IsNullOrWhiteSpace(entry.ResourceId) || entry.Amount <= 0f)
                    continue;

                hasExpectedEntry = true;
                string resourceId = entry.ResourceId.Trim();
                if (!resources.TryGetValue(resourceId, out float amount) || amount + 0.0001f < entry.Amount)
                    return false;
            }

            return hasExpectedEntry;
        }

        public bool TryPersistStarterGrant(int slot, string ownerId, string contextLabel, bool hasStarterEntries)
        {
            if (!GameLaunchContext.IsAutoSaveEnabled())
            {
                _starterPackState.MarkGranted(ownerId);
                return true;
            }

            try
            {
                _saveService?.Save(slot);
                Debug.Log($"[Bootstrap] Автосейв {contextLabel} у слот {slot}.");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Bootstrap] Не вдалося зберегти {contextLabel}: {ex}");
                return false;
            }

            if (hasStarterEntries && !HasPersistedStarterResources(slot, ownerId))
            {
                Debug.LogWarning($"[Bootstrap] Після автосейву economy-блок у слоті {slot} не містить очікуваних стартових ресурсів owner '{ownerId}'. Маркер granted не записано.");
                return false;
            }

            _starterPackState.MarkGranted(ownerId);

            try
            {
                _saveService?.Save(slot);
                Debug.Log($"[Bootstrap] Маркер стартового пакета збережено для owner '{ownerId}' у слот {slot}.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Bootstrap] Не вдалося зберегти маркер стартового пакета для owner '{ownerId}': {ex}");
                return false;
            }
        }

        private bool TryReadEconomyOwnerResources(int slot, string ownerId, out Dictionary<string, float> resources)
        {
            resources = new Dictionary<string, float>(StringComparer.Ordinal);
            if (_saveInspectorService == null || !_saveInspectorService.TryGetBlockPayload(slot, EconomySaveModuleFullName, out byte[] payload))
                return false;

            string normalizedOwnerId = NormalizeOwnerId(ownerId);
            try
            {
                using var stream = new MemoryStream(payload);
                using var reader = new BinaryReader(stream);

                int schemaVersion = reader.ReadInt32();
                if (schemaVersion != 1)
                    return false;

                int ownerCount = reader.ReadInt32();
                for (int ownerIndex = 0; ownerIndex < ownerCount; ownerIndex++)
                {
                    string savedOwnerId = NormalizeOwnerId(reader.ReadString());
                    int resourceCount = reader.ReadInt32();
                    bool isTargetOwner = string.Equals(savedOwnerId, normalizedOwnerId, StringComparison.Ordinal);

                    for (int resourceIndex = 0; resourceIndex < resourceCount; resourceIndex++)
                    {
                        string resourceId = reader.ReadString();
                        float amount = reader.ReadSingle();
                        if (!isTargetOwner || string.IsNullOrWhiteSpace(resourceId) || amount <= 0f)
                            continue;

                        string normalizedResourceId = resourceId.Trim();
                        if (resources.TryGetValue(normalizedResourceId, out float current))
                            resources[normalizedResourceId] = current + amount;
                        else
                            resources[normalizedResourceId] = amount;
                    }
                }

                return resources.Count > 0;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Bootstrap] Не вдалося прочитати economy-блок слота {slot}: {ex.Message}");
                resources.Clear();
                return false;
            }
        }

        private static string NormalizeOwnerId(string ownerId)
            => string.IsNullOrWhiteSpace(ownerId) ? "player_0" : ownerId.Trim();
    }
}