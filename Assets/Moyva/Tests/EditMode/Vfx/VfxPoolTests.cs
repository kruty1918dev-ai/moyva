using System.Collections.Generic;
using Kruty1918.Moyva.Vfx.API;
using Kruty1918.Moyva.Vfx.Runtime;
using Kruty1918.Vfx;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Vfx.Tests
{
    /// <summary>
    /// VfxPool: spawn → timed auto-return → reuse; per-prefab concurrency
    /// accounting; eviction; dispose. Uses real GameObjects created in the
    /// editor test scene and destroyed in TearDown.
    /// </summary>
    public sealed class VfxPoolTests
    {
        private readonly List<GameObject> _leaks = new List<GameObject>();
        private float _now;

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject go in _leaks)
            {
                if (go != null)
                    Object.DestroyImmediate(go);
            }
            _leaks.Clear();
        }

        private GameObject Prefab()
        {
            var prefab = new GameObject("vfx-test-prefab");
            prefab.AddComponent<VfxEffect>().duration = 0.5f;
            prefab.SetActive(false);
            _leaks.Add(prefab);
            return prefab;
        }

        private VfxPool Pool(Transform root)
            => new VfxPool(root, () => _now);

        private static VfxSpawnRequest Request()
            => new VfxSpawnRequest
            {
                Position = Vector3.zero,
                Rotation = Quaternion.identity,
                Scale = 1f,
                CountScale = 1f,
            };

        [Test]
        public void TrySpawn_NullPrefab_ReturnsFalse()
        {
            var root = new GameObject("root").transform;
            _leaks.Add(root.gameObject);
            var pool = Pool(root);

            Assert.IsFalse(pool.TrySpawn(
                new VfxEffectRule { eventName = "x" }, Request()));
        }

        [Test]
        public void TrySpawn_Activates_AndTickReleasesAfterDuration()
        {
            var root = new GameObject("root").transform;
            _leaks.Add(root.gameObject);
            var pool = Pool(root);
            GameObject prefab = Prefab();
            var rule = new VfxEffectRule { eventName = "hit", prefab = prefab };

            Assert.IsTrue(pool.TrySpawn(rule, Request()));
            Assert.AreEqual(1, pool.ActiveCount);
            Assert.AreEqual(1, pool.ActiveCountForPrefab(prefab));

            _now += 0.6f;
            pool.Tick();
            Assert.AreEqual(0, pool.ActiveCount);
            Assert.AreEqual(0, pool.ActiveCountForPrefab(prefab));
        }

        [Test]
        public void ReleasedInstance_IsReused_WithoutStaleState()
        {
            var root = new GameObject("root").transform;
            _leaks.Add(root.gameObject);
            var pool = Pool(root);
            GameObject prefab = Prefab();
            var rule = new VfxEffectRule { eventName = "hit", prefab = prefab };

            pool.TrySpawn(rule, Request());
            GameObject first = root.GetChild(0).gameObject;
            first.transform.localScale = Vector3.one * 9f; // stale

            _now += 1f;
            pool.Tick();
            Assert.IsFalse(first.activeSelf);
            Assert.AreEqual(Vector3.one, first.transform.localScale); // reset

            Assert.IsTrue(pool.TrySpawn(rule, Request()));
            Assert.AreSame(first, root.GetChild(0).gameObject);
        }

        [Test]
        public void EvictOldest_ReturnsEffect_Early()
        {
            var root = new GameObject("root").transform;
            _leaks.Add(root.gameObject);
            var pool = Pool(root);
            var rule = new VfxEffectRule { eventName = "hit", prefab = Prefab() };

            pool.TrySpawn(rule, Request());
            Assert.AreEqual(1, pool.ActiveCount);
            pool.EvictOldest();
            Assert.AreEqual(0, pool.ActiveCount);
        }

        [Test]
        public void Prewarm_CreatesReusableInstances()
        {
            var root = new GameObject("root").transform;
            _leaks.Add(root.gameObject);
            var pool = Pool(root);
            GameObject prefab = Prefab();

            pool.Prewarm(prefab, 3);
            var rule = new VfxEffectRule { eventName = "hit", prefab = prefab };
            for (int i = 0; i < 3; i++)
                Assert.IsTrue(pool.TrySpawn(rule, Request()));
            Assert.AreEqual(3, pool.ActiveCount);
            Assert.AreEqual(3, root.childCount); // no extra instantiation
        }

        [Test]
        public void Dispose_DestroysPooledObjects()
        {
            var root = new GameObject("root").transform;
            _leaks.Add(root.gameObject);
            var pool = Pool(root);
            var rule = new VfxEffectRule { eventName = "hit", prefab = Prefab() };
            pool.TrySpawn(rule, Request());

            pool.Dispose();
            Assert.AreEqual(0, root.childCount);
        }
    }
}
