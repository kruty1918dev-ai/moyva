using System;
using System.Collections.Generic;
using Stopwatch = System.Diagnostics.Stopwatch;
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
    internal sealed class ConstructionSaveModule : ISaveModule
    {
        private const int SchemaMagic =
            unchecked((int)0xC0535632);
        private const int SchemaVersion = 2;
        private const string ModuleLogTag =
            "[MoyvaConstructionModules]";
        private const string PerfLogTag =
            "[MoyvaConstructionPerf]";
        private const double SlowSaveThresholdMs = 2d;
        private const int MaxStatePayloadBytes =
            16 * 1024 * 1024;

        private readonly IConstructionService _constructionService;
        private readonly List<IConstructionModuleStatePersistence>
            _stateProviders;

        [Inject]
        public ConstructionSaveModule(
            IConstructionService constructionService,
            [InjectOptional]
            List<IConstructionModuleStatePersistence>
                stateProviders = null)
        {
            _constructionService = constructionService;
            _stateProviders =
                stateProviders
                ?? new List<IConstructionModuleStatePersistence>();

            AuditStateProviders("init");
        }

        private void AuditStateProviders(string phase)
        {
            var keys =
                new List<string>();
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

                keys.Add(key);
            }

            keys.Sort(StringComparer.Ordinal);

            string keySummary =
                keys.Count == 0
                    ? "none"
                    : string.Join(",", keys);

            if (duplicates > 0 || invalid > 0)
            {
                Debug.LogError(
                    $"{ModuleLogTag} state-provider-audit " +
                    $"phase={phase} " +
                    $"schema={BuildingDefinitionCapabilities.RuntimeStateSchemaVersion} " +
                    $"providers={keys.Count} duplicates={duplicates} " +
                    $"invalid={invalid} keys=[{keySummary}]");
                return;
            }

            Debug.Log(
                $"{ModuleLogTag} state-provider-audit " +
                $"phase={phase} " +
                $"schema={BuildingDefinitionCapabilities.RuntimeStateSchemaVersion} " +
                $"providers={keys.Count} keys=[{keySummary}]");
        }

        public void OnSave(ISaveContext context)
        {
            long startedAt = Stopwatch.GetTimestamp();

            IReadOnlyList<ConstructionSavedPlacement> placements =
                (_constructionService
                    as IConstructionSaveSnapshotSource)
                    ?.GetSavedPlacements();

            if (placements == null)
            {
                var legacy =
                    _constructionService.GetPlayerPlacedBuildings();
                var fallback =
                    new List<ConstructionSavedPlacement>(
                        legacy.Count);
                foreach (var pair in legacy)
                {
                    fallback.Add(
                        new ConstructionSavedPlacement(
                            pair.Key,
                            pair.Value,
                            _constructionService.GetActiveOwner()));
                }
                placements = fallback;
            }

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

            int savedStates = 0;
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
                catch (Exception ex)
                {
                    Debug.LogWarning(
                        $"{ModuleLogTag} save-state failed " +
                        $"key={provider.StateKey} " +
                        $"error={ex.GetType().Name}:{ex.Message}");
                    payload = Array.Empty<byte>();
                }

                context.Writer.Write(provider.StateKey);
                context.Writer.Write(payload.Length);
                if (payload.Length > 0)
                {
                    context.Writer.Write(payload);
                    savedStates++;
                }
            }

            double elapsedMs =
                (Stopwatch.GetTimestamp() - startedAt)
                * 1000d
                / Stopwatch.Frequency;

            Debug.Log(
                $"{ModuleLogTag} save schema={SchemaVersion} " +
                $"placements={placements.Count} " +
                $"providers={providers.Count} " +
                $"states={savedStates} " +
                $"elapsedMs={elapsedMs:0.###}");

            if (Debug.isDebugBuild
                && elapsedMs >= SlowSaveThresholdMs)
            {
                Debug.Log(
                    $"{PerfLogTag} construction-save " +
                    $"placements={placements.Count} " +
                    $"providers={providers.Count} " +
                    $"elapsedMs={elapsedMs:0.###}");
            }
        }

        public void OnLoad(ISaveContext context)
        {
            long startedAt = Stopwatch.GetTimestamp();

            int markerOrLegacyCount =
                context.Reader.ReadInt32();

            if (markerOrLegacyCount >= 0)
            {
                RestoreLegacyPlacements(
                    context,
                    markerOrLegacyCount);

                Debug.Log(
                    $"{ModuleLogTag} load legacy " +
                    $"placements={markerOrLegacyCount}");
                return;
            }

            if (markerOrLegacyCount != SchemaMagic)
            {
                Debug.LogWarning(
                    $"{ModuleLogTag} load rejected " +
                    $"reason=unknown-schema-marker " +
                    $"marker={markerOrLegacyCount}");
                return;
            }

            int version = context.Reader.ReadInt32();
            if (version != SchemaVersion)
            {
                Debug.LogWarning(
                    $"{ModuleLogTag} load rejected " +
                    $"reason=unsupported-version " +
                    $"version={version} expected={SchemaVersion}");
                return;
            }

            int placementCount =
                Math.Max(0, context.Reader.ReadInt32());
            IConstructionSaveRestorer restorer =
                _constructionService as IConstructionSaveRestorer;

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
                var position = new Vector2Int(x, y);

                if (restorer != null)
                {
                    restorer.RestoreFromSave(
                        position,
                        buildingId,
                        ownerId);
                }
                else
                {
                    _constructionService.RestoreFromSave(
                        position,
                        buildingId);
                }
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
            int restoredStates = 0;

            for (int index = 0;
                 index < stateCount;
                 index++)
            {
                string key = context.Reader.ReadString();
                int length = context.Reader.ReadInt32();

                if (length < 0
                    || length > MaxStatePayloadBytes)
                {
                    Debug.LogWarning(
                        $"{ModuleLogTag} load-state rejected " +
                        $"key={key} length={length}");
                    return;
                }

                byte[] payload =
                    context.Reader.ReadBytes(length);
                if (payload.Length != length)
                {
                    Debug.LogWarning(
                        $"{ModuleLogTag} load-state truncated " +
                        $"key={key} expected={length} " +
                        $"actual={payload.Length}");
                    return;
                }

                if (!providersByKey.TryGetValue(
                        key,
                        out IConstructionModuleStatePersistence provider))
                {
                    Debug.LogWarning(
                        $"{ModuleLogTag} load-state skipped " +
                        $"key={key} reason=provider-missing");
                    continue;
                }

                try
                {
                    provider.RestoreState(payload);
                    restoredStates++;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(
                        $"{ModuleLogTag} load-state failed " +
                        $"key={key} " +
                        $"error={ex.GetType().Name}:{ex.Message}");
                }
            }

            double elapsedMs =
                (Stopwatch.GetTimestamp() - startedAt)
                * 1000d
                / Stopwatch.Frequency;

            Debug.Log(
                $"{ModuleLogTag} load schema={version} " +
                $"placements={placementCount} " +
                $"states={restoredStates}/{stateCount} " +
                $"elapsedMs={elapsedMs:0.###}");

            if (Debug.isDebugBuild
                && elapsedMs >= SlowSaveThresholdMs)
            {
                Debug.Log(
                    $"{PerfLogTag} construction-load " +
                    $"placements={placementCount} " +
                    $"states={stateCount} " +
                    $"elapsedMs={elapsedMs:0.###}");
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

                _constructionService.RestoreFromSave(
                    new Vector2Int(x, y),
                    buildingId);
            }
        }
    }
}
