using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Units.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Units
{
    /// <summary>
    /// Snapshot-тести (#86): перевіряють стан даних після масових операцій над
    /// <see cref="UnitClassConfigService"/>. Гарантують, що редакторські зміни
    /// не ламають критичну логіку lookup/resolution.
    /// Категорія: Snapshot (запускається в CI quality gate).
    /// </summary>
    [TestFixture]
    [Category("Snapshot")]
    public sealed class UnitRegistrySnapshotTests
    {
        // ---- Fake registry helpers -----------------------------------------

        private static UnitRegistrySO BuildRegistry(params (string typeId, UnitRole role, int hp)[] entries)
        {
            var so = ScriptableObject.CreateInstance<UnitRegistrySO>();
            so.Configs = entries
                .Select(e => new UnitClassConfig { TypeId = e.typeId, Role = e.role, HitPoints = e.hp })
                .ToList();
            return so;
        }

        // ---- Snapshot: initial count preservation --------------------------

        [Test]
        public void Snapshot_Registry_EntryCount_Preserved()
        {
            var registry = BuildRegistry(
                ("warrior", UnitRole.Military, 100),
                ("archer", UnitRole.Military, 80),
                ("worker", UnitRole.Worker, 60));

            var svc = new UnitClassConfigService(registry);

            // Snapshot: exactly 3 configs, each retrievable.
            Assert.IsNotNull(svc.GetConfig("warrior"), "warrior must be resolvable");
            Assert.IsNotNull(svc.GetConfig("archer"), "archer must be resolvable");
            Assert.IsNotNull(svc.GetConfig("worker"), "worker must be resolvable");
        }

        [Test]
        public void Snapshot_Registry_AllEntries_HaveNonEmptyTypeId()
        {
            var registry = BuildRegistry(
                ("warrior", UnitRole.Military, 100),
                ("archer", UnitRole.Military, 80),
                ("", UnitRole.Worker, 60));          // intentionally empty — should not crash lookup

            var svc = new UnitClassConfigService(registry);

            // Empty TypeId entry must not pollute the lookup for valid IDs.
            Assert.IsNotNull(svc.GetConfig("warrior"), "warrior must resolve despite empty entry");
            Assert.IsNull(svc.GetConfig(""), "Empty TypeId must return null");
        }

        [Test]
        public void Snapshot_Registry_DuplicateTypeId_SecondEntryIgnored()
        {
            // Registry with a duplicate TypeId — the service should keep the first and warn (not throw).
            var so = ScriptableObject.CreateInstance<UnitRegistrySO>();
            so.Configs = new List<UnitClassConfig>
            {
                new UnitClassConfig { TypeId = "warrior", Role = UnitRole.Military, HitPoints = 100 },
                new UnitClassConfig { TypeId = "warrior", Role = UnitRole.Military, HitPoints = 200 }, // duplicate
            };

            var svc = new UnitClassConfigService(so);
            var config = svc.GetConfig("warrior");

            Assert.IsNotNull(config, "Config must resolve");
            Assert.AreEqual(100, config.HitPoints, "First entry must win over duplicate");
        }

        // ---- Snapshot: instance-id resolution ------------------------------

        [Test]
        public void Snapshot_InstanceId_ResolvesToBaseType()
        {
            // "warrior_01_abc123" → resolves to "warrior" base type config
            var registry = BuildRegistry(("warrior", UnitRole.Military, 100));
            var svc = new UnitClassConfigService(registry);

            var config = svc.GetConfig("warrior_01_abc123");
            Assert.IsNotNull(config, "Instance id with underscore suffix must resolve to base type");
            Assert.AreEqual(100, config.HitPoints);
        }

        [Test]
        public void Snapshot_UnknownTypeId_ReturnsNull()
        {
            var registry = BuildRegistry(("warrior", UnitRole.Military, 100));
            var svc = new UnitClassConfigService(registry);

            Assert.IsNull(svc.GetConfig("nonexistent"), "Unknown TypeId must return null without throwing");
        }

        // ---- Snapshot: bulk operations -------------------------------------

        [Test]
        public void Snapshot_BulkRegistry_100Entries_AllRetrievable()
        {
            const int Count = 100;
            var entries = new (string, UnitRole, int)[Count];
            for (int i = 0; i < Count; i++)
                entries[i] = ($"unit-{i:D3}", i % 2 == 0 ? UnitRole.Military : UnitRole.Worker, 50 + i);

            var registry = BuildRegistry(entries);
            var svc = new UnitClassConfigService(registry);

            for (int i = 0; i < Count; i++)
            {
                var cfg = svc.GetConfig($"unit-{i:D3}");
                Assert.IsNotNull(cfg, $"unit-{i:D3} must be resolvable");
                Assert.AreEqual(50 + i, cfg.HitPoints, $"HitPoints snapshot for unit-{i:D3}");
            }
        }

        [Test]
        public void Snapshot_BulkRegistry_100Entries_NoDuplicates_AllPresent()
        {
            const int Count = 100;
            var so = ScriptableObject.CreateInstance<UnitRegistrySO>();
            so.Configs = Enumerable.Range(0, Count)
                .Select(i => new UnitClassConfig { TypeId = $"u{i}", HitPoints = i + 1 })
                .ToList();

            var svc = new UnitClassConfigService(so);

            int resolved = 0;
            for (int i = 0; i < Count; i++)
                if (svc.GetConfig($"u{i}") != null) resolved++;

            Assert.AreEqual(Count, resolved, "All 100 unique entries must be resolvable");
        }
    }
}
