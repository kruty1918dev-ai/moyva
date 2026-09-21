using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Vfx.API;
using UnityEngine;

namespace Kruty1918.Moyva.Vfx.Runtime
{
    /// <summary>Пул VFX-ефектів: переuse інстансів префабів, ліміти активних ефектів, витіснення найстаріших.</summary>
    public sealed class VfxPool : IVfxSpawner, IDisposable
    {
        /// <summary>Запис про активний ефект у пулі.</summary>
        private struct ActiveEntry
        {
            /// <summary>Екземпляр ефекту.</summary>
            public VfxEffect Effect;
            /// <summary>Час повернення ефекту в пул.</summary>
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

        /// <summary>Створює пул із коренем для інстансів і опційним джерелом часу.</summary>
        public VfxPool(Transform root, Func<float> clock = null)
        {
            _root = root;
            _clock = clock ?? (() => Time.time);
        }

        /// <summary>Кількість активних ефектів.</summary>
        public int ActiveCount => _active.Count;
        /// <summary>Загальна кількість відкинутих спавнів.</summary>
        public int TotalDropped => _dropped;

        /// <summary>Намагається заспавнити ефект за правилом і запитом.</summary>
        public bool TrySpawn(VfxEffectRule rule, in VfxSpawnRequest request)
        {
            if (rule == null || rule.prefab == null || _root == null)
                return false;

            VfxEffect effect = Acquire(rule.prefab);
            if (effect == null)
                return false;

            effect.gameObject.SetActive(true);
            effect.Play(request);
            _active.Add(new ActiveEntry
            {
                Effect = effect,
                ReleaseAt = _clock() + ResolveDuration(rule, effect, request),
            });
            _activePerPrefab[rule.prefab] =
                _activePerPrefab.TryGetValue(rule.prefab, out int count) ? count + 1 : 1;
            return true;
        }

        /// <summary>Кількість активних ефектів для конкретного префаба.</summary>
        public int ActiveCountForPrefab(GameObject prefab)
            => prefab != null
               && _activePerPrefab.TryGetValue(prefab, out int count)
                ? count
                : 0;

        /// <summary>Повертає в пул ефекти, чий час вийшов.</summary>
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

        /// <summary>Витісняє найстаріший активний ефект у пул.</summary>
        public void EvictOldest()
        {
            if (_active.Count == 0)
                return;

            VfxEffect effect = _active[0].Effect;
            _active.RemoveAt(0);
            Release(effect);
        }

        /// <summary>Попередньо прогріває пул заданою кількістю інстансів.</summary>
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

        /// <summary>Знищує всі інстанси пула.</summary>
        public void Dispose()
        {
            for (int i = 0; i < _active.Count; i++)
            {
                if (_active[i].Effect != null)
                    DestroyUnityObject(_active[i].Effect.gameObject);
            }
            _active.Clear();

            foreach (var pair in _free)
            {
                foreach (VfxEffect effect in pair.Value)
                {
                    if (effect != null)
                        DestroyUnityObject(effect.gameObject);
                }
            }
            _free.Clear();
        }

        private static void DestroyUnityObject(UnityEngine.Object unityObject)
        {
            if (unityObject == null)
                return;

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(unityObject);
            else
                UnityEngine.Object.DestroyImmediate(unityObject);
        }

        private VfxEffect Acquire(GameObject prefab)
        {
            if (_free.TryGetValue(prefab, out Queue<VfxEffect> queue))
            {
                while (queue.Count > 0)
                {
                    VfxEffect candidate = queue.Dequeue();
                    if (candidate != null)
                        return candidate;
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
                DestroyUnityObject(instance);
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
                DestroyUnityObject(effect.gameObject);
                return;
            }

            if (!_free.TryGetValue(key, out Queue<VfxEffect> queue))
            {
                queue = new Queue<VfxEffect>(4);
                _free[key] = queue;
            }
            queue.Enqueue(effect);
        }

        private float ResolveDuration(
            VfxEffectRule rule, VfxEffect effect, in VfxSpawnRequest request)
        {
            if (request.DurationOverride > 0f)
                return request.DurationOverride;
            if (rule.duration > 0f)
                return rule.duration;
            return Mathf.Max(0.05f, effect.Duration);
        }
    }
}
