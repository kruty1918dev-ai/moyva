using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Save-модуль для системи будівництва.
    /// Зберігає всі будівлі, підтверджені гравцем (playerPlacedBuildings).
    ///
    /// Формат блоку:
    ///   int32  — кількість записів
    ///   для кожного:
    ///     int32  — X позиції тайлу
    ///     int32  — Y позиції тайлу
    ///     string — buildingId (UTF-8 з length prefix via BinaryWriter)
    /// </summary>
    [SaveModuleId("Kruty1918.Moyva.Construction.Runtime.ConstructionSaveModule")]
    internal sealed class ConstructionSaveModule : ISaveModule
    {
        private const int SchemaMagic =
            unchecked((int)0xC0535632);
        private const int SchemaVersion = 3;
        private const int MaxStatePayloadBytes =
            16 * 1024 * 1024;

        private readonly IConstructionSaveSnapshotSource _placementSnapshots;
        private readonly IConstructionSaveRestorer _placementRestorer;
        private readonly IConstructionSessionCommands _session;
        private readonly List<IConstructionModuleStatePersistence>
            _stateProviders;

        [Inject]
        public ConstructionSaveModule(
            IConstructionSaveSnapshotSource placementSnapshots,
            IConstructionSaveRestorer placementRestorer,
            IConstructionSessionCommands session,
            [InjectOptional]
            List<IConstructionModuleStatePersistence>
                stateProviders = null)
        {
            _placementSnapshots = placementSnapshots;
            _placementRestorer = placementRestorer;
            _session = session;
            _stateProviders =
                stateProviders
                ?? new List<IConstructionModuleStatePersistence>();

            ValidateStateProviders();
        }

        private void ValidateStateProviders()
        {
            var seen =
                new HashSet<string>(
                    StringComparer.Ordinal);
            int duplicates = 0;
            int invalid = 0;

            for (int index = 0;
                 index < _stateProviders.Count;
                 index++)
            {
                IConstructionModuleStatePersistence provider =
                    _stateProviders[index];
                string key = provider?.StateKey?.Trim();

                if (provider == null
                    || string.IsNullOrWhiteSpace(key))
                {
                    invalid++;
                    continue;
                }

                if (!seen.Add(key))
                {
                    duplicates++;
                    continue;
                }

            }

            if (duplicates > 0 || invalid > 0)
            {
                Debug.LogError(
                    $"[ConstructionSave] Invalid module state providers: " +
                    $"duplicates={duplicates}, invalid={invalid}.");
            }
        }

        public void OnSave(ISaveContext context)
        {
            IReadOnlyList<ConstructionSavedPlacement> placements =
                _placementSnapshots.GetSavedPlacements();

            context.Writer.Write(SchemaMagic);
            context.Writer.Write(SchemaVersion);
            context.Writer.Write(placements.Count);

            for (int index = 0;
                 index < placements.Count;
                 index++)
            {
                ConstructionSavedPlacement placement =
                    placements[index];
                context.Writer.Write(placement.Position.x);
                context.Writer.Write(placement.Position.y);
                context.Writer.Write(
                    placement.BuildingId ?? string.Empty);
                context.Writer.Write(
                    placement.OwnerId ?? string.Empty);
                context.Writer.Write((byte)placement.Rotation);
            }

            var providers =
                new List<IConstructionModuleStatePersistence>();
            var seenKeys =
                new HashSet<string>(
                    StringComparer.Ordinal);

            for (int index = 0;
                 index < _stateProviders.Count;
                 index++)
            {
                IConstructionModuleStatePersistence provider =
                    _stateProviders[index];
                string key = provider?.StateKey?.Trim();
                if (provider == null
                    || string.IsNullOrWhiteSpace(key)
                    || !seenKeys.Add(key))
                {
                    continue;
                }

                providers.Add(provider);
            }

            providers.Sort(
                (left, right) =>
                    string.CompareOrdinal(
                        left.StateKey,
                        right.StateKey));

            context.Writer.Write(providers.Count);

            for (int index = 0;
                 index < providers.Count;
                 index++)
            {
                IConstructionModuleStatePersistence provider =
                    providers[index];
                byte[] payload;
                try
                {
                    payload =
                        provider.CaptureState()
                        ?? Array.Empty<byte>();
                }
                catch (Exception)
                {
                    payload = Array.Empty<byte>();
                }

                context.Writer.Write(provider.StateKey);
                context.Writer.Write(payload.Length);
                if (payload.Length > 0)
                    context.Writer.Write(payload);
            }
        }

        public void OnLoad(ISaveContext context)
        {
            int markerOrLegacyCount =
                context.Reader.ReadInt32();

            if (markerOrLegacyCount >= 0)
            {
                RestoreLegacyPlacements(
                    context,
                    markerOrLegacyCount);
                return;
            }

            if (markerOrLegacyCount != SchemaMagic)
            {
                return;
            }

            int version = context.Reader.ReadInt32();
            if (version != 2 && version != SchemaVersion)
            {
                return;
            }

            int placementCount =
                Math.Max(0, context.Reader.ReadInt32());
            for (int index = 0;
                 index < placementCount;
                 index++)
            {
                int x = context.Reader.ReadInt32();
                int y = context.Reader.ReadInt32();
                string buildingId =
                    context.Reader.ReadString();
                string ownerId =
                    context.Reader.ReadString();
                ConstructionRotation rotation = version >= 3
                    ? ConstructionRotationUtility.Normalize(
                        context.Reader.ReadByte())
                    : ConstructionRotation.Degrees0;
                var position = new Vector2Int(x, y);

                _placementRestorer.RestoreFromSave(
                    position,
                    buildingId,
                    ownerId,
                    rotation);
            }

            var providersByKey =
                new Dictionary<
                    string,
                    IConstructionModuleStatePersistence>(
                    StringComparer.Ordinal);
            for (int index = 0;
                 index < _stateProviders.Count;
                 index++)
            {
                IConstructionModuleStatePersistence provider =
                    _stateProviders[index];
                string key = provider?.StateKey?.Trim();
                if (provider == null
                    || string.IsNullOrWhiteSpace(key)
                    || providersByKey.ContainsKey(key))
                {
                    continue;
                }

                providersByKey[key] = provider;
            }

            int stateCount =
                Math.Max(0, context.Reader.ReadInt32());

            for (int index = 0;
                 index < stateCount;
                 index++)
            {
                string key = context.Reader.ReadString();
                int length = context.Reader.ReadInt32();

                if (length < 0
                    || length > MaxStatePayloadBytes)
                {
                    return;
                }

                byte[] payload =
                    context.Reader.ReadBytes(length);
                if (payload.Length != length)
                {
                    return;
                }

                if (!providersByKey.TryGetValue(
                        key,
                        out IConstructionModuleStatePersistence provider))
                {
                    continue;
                }

                try
                {
                    provider.RestoreState(payload);
                }
                catch (Exception)
                {
                }
            }
        }

        private void RestoreLegacyPlacements(
            ISaveContext context,
            int count)
        {
            for (int index = 0;
                 index < count;
                 index++)
            {
                int x = context.Reader.ReadInt32();
                int y = context.Reader.ReadInt32();
                string buildingId =
                    context.Reader.ReadString();

                _placementRestorer.RestoreFromSave(
                    new Vector2Int(x, y),
                    buildingId,
                    _session.GetActiveOwner());
            }
        }
    }
}
