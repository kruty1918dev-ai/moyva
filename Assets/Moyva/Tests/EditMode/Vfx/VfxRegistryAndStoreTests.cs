using Kruty1918.Moyva.Vfx.API;
using Kruty1918.Moyva.Vfx.Runtime;
using Kruty1918.Vfx;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Vfx.Tests
{
    /// <summary>
    /// VfxDefinitionRegistry: eventName/context resolution semantics;
    /// VfxUnitSnapshotStore: death-position mirror + spawn dedupe.
    /// </summary>
    public sealed class VfxRegistryAndStoreTests
    {
        private static VfxEffectRule Rule(string eventName, string context = null)
            => new VfxEffectRule
            {
                eventName = eventName,
                context = context ?? string.Empty,
            };

        [Test]
        public void Resolve_BaseEvent_ReturnsRule()
        {
            var config = new VfxCatalogConfig
            {
                effects = new[] { Rule("building-placed") },
            };
            var registry = VfxDefinitionRegistry.Build(config?.effects);

            Assert.NotNull(registry.Resolve("building-placed"));
            Assert.IsNull(registry.Resolve("unit-destroyed"));
        }

        [Test]
        public void Resolve_NullConfig_ReturnsNull()
        {
            var registry = VfxDefinitionRegistry.Build(null);
            Assert.IsNull(registry.Resolve("building-placed"));
        }

        [Test]
        public void Resolve_ContextRule_WinsOverBase()
        {
            var baseRule = Rule("unit-move-dust");
            var contextRule = Rule("unit-move-dust", "tile:water");
            var config = new VfxCatalogConfig { effects = new[] { baseRule, contextRule } };
            var registry = VfxDefinitionRegistry.Build(config?.effects);

            Assert.AreSame(contextRule, registry.Resolve("unit-move-dust", "tile:water"));
            Assert.AreSame(baseRule, registry.Resolve("unit-move-dust", "tile:grass"));
            Assert.AreSame(baseRule, registry.Resolve("unit-move-dust"));
        }

        [Test]
        public void Resolve_IsCaseInsensitive()
        {
            var config = new VfxCatalogConfig { effects = new[] { Rule("building-placed") } };
            var registry = VfxDefinitionRegistry.Build(config?.effects);
            Assert.NotNull(registry.Resolve("Building-Placed"));
        }

        [Test]
        public void SnapshotStore_DeathPosition_SurvivesUntilRemoved()
        {
            var store = new VfxUnitSnapshotStore();
            store.TrackCreated("u1", new Vector2Int(3, 7), "warrior", "f-a", 0f);
            store.TrackMoved("u1", new Vector2Int(4, 8), "f-a");

            Assert.IsTrue(store.TryGet("u1", out var pos, out var type, out var owner));
            Assert.AreEqual(new Vector2Int(4, 8), pos);
            Assert.AreEqual("warrior", type);
            Assert.AreEqual("f-a", owner);

            store.Remove("u1");
            Assert.IsFalse(store.TryGet("u1", out _, out _, out _));
        }

        [Test]
        public void SnapshotStore_SpawnEffect_DedupesWithinWindow()
        {
            var store = new VfxUnitSnapshotStore { SpawnDedupeWindow = 2f };
            Assert.IsTrue(store.TryConsumeSpawnEffect("u1", 0f));
            Assert.IsFalse(store.TryConsumeSpawnEffect("u1", 1f));
            Assert.IsTrue(store.TryConsumeSpawnEffect("u1", 3f));
            Assert.IsTrue(store.TryConsumeSpawnEffect("u2", 3f));
        }
    }
}
