using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Vfx.API;
using UnityEngine;

namespace Kruty1918.Moyva.Vfx.Runtime
{
    /// <summary>
    /// Prefab-instance pool for VFX. Acquire → place → play → time-based
    /// auto-return on Tick. Keeps zero managed allocations on the hot path
    /// after warm-up (struct active list, queues reused per prefab).
    /// </summary>
    public sealed class VfxPool : IVfxSpawner, IDisposable
    {
        private struct ActiveEntry
        {
            public VfxEffect Effect;
            public float ReleaseAt;
        }

        private readonly Transform _root;
        private readonly Dictionary<GameObject, Queue<VfxEffect>> _free =
            new Dictionary<GameObject, Queue<VfxEffect>>();
        private readonly Dictionary<GameObject, int> _activePerPrefab =
            new Dictionary<GameObject, int>();
        private readonly List<ActiveEntry> _active = new List<ActiveEntry>(64);
        private readonly Func<float> _clock;

        private int _dropped;

        public VfxPool(Transform root, Func<float> clock = null)
        {
            _root = root;
            _clock = clock ?? (() => Time.time);
        }

        public int ActiveCount => _active.Count;
        public int TotalDropped => _dropped;

        public bool TrySpawn(VfxEffectRule rule, in VfxSpawnRequest request)
        {
            if (rule == null || rule.prefab == null || _root == null)
                return false;

            VfxEffect effect = Acquire(rule.prefab);
            if (effect == null)
                return false;

            effect.Play(request);
            _active.Add(new ActiveEntry
            {
                Effect = effect,
                ReleaseAt = _clock() + ResolveDuration(rule, effect),
            });
            _activePerPrefab[rule.prefab] =
                _activePerPrefab.TryGetValue(rule.prefab, out int count) ? count + 1 : 1;
            return true;
        }

        /// <summary>Active instances spawned from a specific prefab (per-rule budget checks).</summary>
        public int ActiveCountForPrefab(GameObject prefab)
            => prefab != null
               && _activePerPrefab.TryGetValue(prefab, out int count)
                ? count
                : 0;

        /// <summary>Returns finished effects to their free queues. Call once per frame.</summary>
        public void Tick()
        {
            float now = _clock();
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                if (_active[i].ReleaseAt > now)
                    continue;

                Release(_active[i].Effect);
                _active.RemoveAt(i);
            }
        }

        /// <summary>Drops the oldest active instance (used when the global budget is hit).</summary>
        public void EvictOldest()
        {
            if (_active.Count == 0)
                return;

            VfxEffect effect = _active[0].Effect;
            _active.RemoveAt(0);
            Release(effect);
        }

        /// <summary>Pre-instantiates pooled instances so first gameplay spawns do not hitch.</summary>
        public void Prewarm(GameObject prefab, int count)
        {
            if (prefab == null || count <= 0)
                return;

            for (int i = 0; i < count; i++)
            {
                VfxEffect effect = CreateInstance(prefab);
                if (effect == null)
                    break;
                Release(effect);
            }
        }

        /// <summary>Stops everything and destroys pooled objects (scene unload / dispose).</summary>
        public void Dispose()
        {
            for (int i = 0; i < _active.Count; i++)
            {
                if (_active[i].Effect != null)
                    UnityEngine.Object.Destroy(_active[i].Effect.gameObject);
            }
            _active.Clear();

            foreach (var pair in _free)
            {
                foreach (VfxEffect effect in pair.Value)
                {
                    if (effect != null)
                        UnityEngine.Object.Destroy(effect.gameObject);
                }
            }
            _free.Clear();

            if (_root != null)
                UnityEngine.Object.Destroy(_root.gameObject);
        }

        private VfxEffect Acquire(GameObject prefab)
        {
            if (_free.TryGetValue(prefab, out Queue<VfxEffect> queue))
            {
                while (queue.Count > 0)
                {
                    VfxEffect candidate = queue.Dequeue();
                    if (candidate != null)
                    {
                        candidate.gameObject.SetActive(true);
                        return candidate;
                    }
                }
            }

            return CreateInstance(prefab);
        }

        private VfxEffect CreateInstance(GameObject prefab)
        {
            GameObject instance = UnityEngine.Object.Instantiate(prefab, _root, false);
            if (instance == null)
            {
                _dropped++;
                return null;
            }

            var effect = instance.GetComponent<VfxEffect>();
            if (effect == null)
            {
                UnityEngine.Object.Destroy(instance);
                _dropped++;
                return null;
            }

            effect.SourcePrefab = prefab;
            instance.SetActive(false);
            return effect;
        }

        private void Release(VfxEffect effect)
        {
            if (effect == null)
                return;

            effect.Stop();
            effect.gameObject.SetActive(false);
            GameObject key = effect.SourcePrefab;
            if (key != null
                && _activePerPrefab.TryGetValue(key, out int remaining))
            {
                if (remaining <= 1) _activePerPrefab.Remove(key);
                else _activePerPrefab[key] = remaining - 1;
            }

            if (key == null)
            {
                UnityEngine.Object.Destroy(effect.gameObject);
                return;
            }

            if (!_free.TryGetValue(key, out Queue<VfxEffect> queue))
            {
                queue = new Queue<VfxEffect>(4);
                _free[key] = queue;
            }
            queue.Enqueue(effect);
        }

        private float ResolveDuration(VfxEffectRule rule, VfxEffect effect)
        {
            if (rule.duration > 0f)
                return rule.duration;
            return Mathf.Max(0.05f, effect.Duration);
        }
    }
}
