using System;
using System.Collections.Generic;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;

namespace Kruty1918.Moyva.Economy.Runtime
{
    internal sealed class EconomySaveModule : ISaveModule
    {
        private const int SchemaVersion = 1;
        private readonly EconomyManager _economyManager;

        public EconomySaveModule(EconomyManager economyManager)
        {
            _economyManager = economyManager;
        }

        public void OnSave(ISaveContext context)
        {
            // Завдання: збереження має переживати обидві фази стартової економіки:
            // до першого складу ресурси можуть бути в owner-pool, після складу - у settlement/warehouse storage.
            var ownerPools = _economyManager?.GetOwnerResourceTotalsSnapshot()
                             ?? new Dictionary<string, Dictionary<string, float>>(StringComparer.Ordinal);

            context.Writer.Write(SchemaVersion);
            context.Writer.Write(ownerPools.Count);

            foreach (var ownerPair in ownerPools)
            {
                context.Writer.Write(ownerPair.Key ?? string.Empty);

                var resources = ownerPair.Value;
                int resourceCount = resources?.Count ?? 0;
                context.Writer.Write(resourceCount);

                if (resources == null)
                    continue;

                foreach (var resourcePair in resources)
                {
                    context.Writer.Write(resourcePair.Key ?? string.Empty);
                    context.Writer.Write(resourcePair.Value);
                }
            }

            Debug.Log($"[EconomySave] Збережено owner-пули: {ownerPools.Count}.");
        }

        public void OnLoad(ISaveContext context)
        {
            int version = context.Reader.ReadInt32();
            if (version != SchemaVersion)
            {
                Debug.LogWarning($"[EconomySave] Непідтримувана версія блоку: {version}. Очікується {SchemaVersion}. Owner-пули не відновлено.");
                return;
            }

            int ownerCount = context.Reader.ReadInt32();
            var restored = new Dictionary<string, Dictionary<string, float>>(StringComparer.Ordinal);

            for (int ownerIndex = 0; ownerIndex < ownerCount; ownerIndex++)
            {
                string ownerId = context.Reader.ReadString();
                int resourceCount = context.Reader.ReadInt32();

                var pool = new Dictionary<string, float>(StringComparer.Ordinal);
                for (int resourceIndex = 0; resourceIndex < resourceCount; resourceIndex++)
                {
                    string resourceId = context.Reader.ReadString();
                    float amount = context.Reader.ReadSingle();

                    if (string.IsNullOrWhiteSpace(resourceId) || amount <= 0f)
                        continue;

                    pool[resourceId.Trim()] = amount;
                }

                if (!string.IsNullOrWhiteSpace(ownerId) && pool.Count > 0)
                    restored[ownerId.Trim()] = pool;
            }

            // Завдання: після load ранні owner-pool ресурси або лишаються owner-level, або одразу переїжджають у перший склад,
            // якщо settlement/warehouse вже відновлені. Це запобігає повторній видачі starter pack на load-game.
            _economyManager?.RestoreOwnerResourcePools(restored);
            Debug.Log($"[EconomySave] Відновлено owner-пули: {restored.Count}.");
        }
    }
}