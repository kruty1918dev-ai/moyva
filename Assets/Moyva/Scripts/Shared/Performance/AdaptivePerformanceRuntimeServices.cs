using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Shared.Graphics;
using UnityEngine;
using UnityEngine.U2D;
using Zenject;
using Debug = UnityEngine.Debug;

namespace Kruty1918.Moyva.Shared.Performance
{
    public struct FrameTimeSnapshot
    {
        public float P50Ms;
        public float P95Ms;
        public float P99Ms;
        public float CpuBudgetMs;
        public float GpuBudgetMs;
        public string LastDegradationReason;
    }

    public interface IFrameBudgetMonitorService
    {
        FrameTimeSnapshot GetSnapshot();
        void SetDegradationReason(string reason);
    }

    public sealed class FrameBudgetMonitorService : IFrameBudgetMonitorService, ITickable
    {
        private readonly FrameBudgetSettings _budget;
        private readonly Queue<float> _frameTimes;
        private readonly float[] _sortedBuffer;
        private string _lastDegradationReason;

        public FrameBudgetMonitorService()
        {
            _budget = AdaptivePerformanceDefaultsProvider.LoadFrameBudget();
            _frameTimes = new Queue<float>(_budget.PercentileWindow);
            _sortedBuffer = new float[_budget.PercentileWindow];
        }

        public void Tick()
        {
            float frameMs = Mathf.Max(0.1f, Time.unscaledDeltaTime * 1000f);
            if (_frameTimes.Count >= _budget.PercentileWindow)
                _frameTimes.Dequeue();

            _frameTimes.Enqueue(frameMs);

            if (frameMs > _budget.AlertCpuFrameMs)
                Debug.LogWarning($"[PerfBudget] CPU frame alert: {frameMs:0.00}ms (threshold={_budget.AlertCpuFrameMs:0.00}ms)");
        }

        public void SetDegradationReason(string reason)
        {
            _lastDegradationReason = reason ?? string.Empty;
        }

        public FrameTimeSnapshot GetSnapshot()
        {
            int count = _frameTimes.Count;
            if (count == 0)
            {
                return new FrameTimeSnapshot
                {
                    CpuBudgetMs = _budget.CpuFrameBudgetMs,
                    GpuBudgetMs = _budget.GpuFrameBudgetMs,
                    LastDegradationReason = _lastDegradationReason,
                };
            }

            int index = 0;
            foreach (var item in _frameTimes)
                _sortedBuffer[index++] = item;

            Array.Sort(_sortedBuffer, 0, count);

            return new FrameTimeSnapshot
            {
                P50Ms = Percentile(_sortedBuffer, count, 0.50f),
                P95Ms = Percentile(_sortedBuffer, count, 0.95f),
                P99Ms = Percentile(_sortedBuffer, count, 0.99f),
                CpuBudgetMs = _budget.CpuFrameBudgetMs,
                GpuBudgetMs = _budget.GpuFrameBudgetMs,
                LastDegradationReason = _lastDegradationReason,
            };
        }

        private static float Percentile(float[] values, int count, float percentile)
        {
            if (count <= 0)
                return 0f;

            int rank = Mathf.Clamp(Mathf.CeilToInt(count * percentile) - 1, 0, count - 1);
            return values[rank];
        }
    }

    public interface IAdaptiveQualityPolicyService
    {
        bool IsThrottled();
    }

    public sealed class AdaptiveQualityPolicyService : IAdaptiveQualityPolicyService, ITickable
    {
        private readonly IGraphicsSettingsService _graphics;
        private readonly IFrameBudgetMonitorService _frameBudget;
        private readonly RenderScalePolicySettings _renderScalePolicy;
        private readonly MobilePerformanceThresholds _thresholds;

        private float _cooldown;

        public AdaptiveQualityPolicyService(
            [InjectOptional] IGraphicsSettingsService graphics,
            [InjectOptional] IFrameBudgetMonitorService frameBudget)
        {
            _graphics = graphics;
            _frameBudget = frameBudget;
            _renderScalePolicy = AdaptivePerformanceDefaultsProvider.LoadRenderScalePolicy();
            _thresholds = AdaptivePerformanceDefaultsProvider.LoadMobileThresholds();
        }

        public bool IsThrottled() => _cooldown > 0f;

        public void Tick()
        {
            if (_graphics == null || _frameBudget == null)
                return;

            if (_cooldown > 0f)
            {
                _cooldown -= Time.unscaledDeltaTime;
                return;
            }

            var snapshot = _frameBudget.GetSnapshot();
            if (snapshot.P95Ms <= 1000f / Mathf.Max(1f, _thresholds.LowFpsThreshold))
                return;

            ApplyNextDegradationStep();
            _cooldown = _renderScalePolicy.CooldownSeconds;
        }

        private void ApplyNextDegradationStep()
        {
            var settings = _graphics.Settings;

            if (settings.Shadows)
            {
                _graphics.SetShadows(false);
                _frameBudget.SetDegradationReason("AdaptivePolicy: disable shadows");
                return;
            }

            if (settings.AnisotropicFiltering)
            {
                _graphics.SetAnisotropicFiltering(false);
                _frameBudget.SetDegradationReason("AdaptivePolicy: disable anisotropic filtering");
                return;
            }

            if (settings.TextureMipmapLimit < 2)
            {
                _graphics.SetTextureMipmapLimit(settings.TextureMipmapLimit + 1);
                _frameBudget.SetDegradationReason("AdaptivePolicy: increase texture mipmap limit");
                return;
            }

            if (settings.LodBias > 0.75f)
            {
                _graphics.SetLodBias(Mathf.Max(0.75f, settings.LodBias - 0.1f));
                _frameBudget.SetDegradationReason("AdaptivePolicy: reduce LOD bias");
                return;
            }

            float nextScale = Mathf.Clamp(settings.RenderScale - _renderScalePolicy.Step, _renderScalePolicy.MinimumScale, _renderScalePolicy.MaximumScale);
            if (nextScale < settings.RenderScale - 0.001f)
            {
                _graphics.SetRenderScale(nextScale);
                _frameBudget.SetDegradationReason($"AdaptivePolicy: reduce render scale to {nextScale:0.00}");
            }
        }
    }

    public interface IStartupPrewarmService
    {
        Task PrewarmAsync(CancellationToken ct = default);
    }

    public interface IScenePreActivationInitializer
    {
        Task InitializeBeforeActivationAsync(CancellationToken ct = default);
    }

    public sealed class StartupPrewarmService : IStartupPrewarmService, IScenePreActivationInitializer
    {
        private readonly PrewarmSettings _settings;

        public StartupPrewarmService()
        {
            _settings = AdaptivePerformanceDefaultsProvider.LoadPrewarmSettings();
        }

        public Task InitializeBeforeActivationAsync(CancellationToken ct = default)
        {
            return PrewarmAsync(ct);
        }

        public async Task PrewarmAsync(CancellationToken ct = default)
        {
            await LoadResourcesAsync<Shader>(_settings.ShaderResourcePaths, ct);
            await LoadResourcesAsync<Material>(_settings.MaterialResourcePaths, ct);
            await LoadResourcesAsync<SpriteAtlas>(_settings.CriticalSpriteAtlasResourcePaths, ct);

            if (_settings.WarmupAllShaders)
                Shader.WarmupAllShaders();
        }

        private static async Task LoadResourcesAsync<T>(string[] paths, CancellationToken ct) where T : UnityEngine.Object
        {
            if (paths == null || paths.Length == 0)
                return;

            for (int i = 0; i < paths.Length; i++)
            {
                ct.ThrowIfCancellationRequested();
                string path = paths[i];
                if (string.IsNullOrWhiteSpace(path))
                    continue;

                ResourceRequest request = Resources.LoadAsync<T>(path);
                while (!request.isDone)
                {
                    ct.ThrowIfCancellationRequested();
                    await Task.Yield();
                }
            }
        }
    }

    public interface IExpensiveServiceBudgetScheduler
    {
        bool ShouldRun(string key, float intervalSeconds);
    }

    public sealed class ExpensiveServiceBudgetScheduler : IExpensiveServiceBudgetScheduler
    {
        private readonly Dictionary<string, float> _nextRunByKey = new Dictionary<string, float>(StringComparer.Ordinal);

        public bool ShouldRun(string key, float intervalSeconds)
        {
            if (string.IsNullOrWhiteSpace(key))
                return true;

            float now = Time.unscaledTime;
            if (_nextRunByKey.TryGetValue(key, out var nextRun) && now < nextRun)
                return false;

            _nextRunByKey[key] = now + Mathf.Max(0f, intervalSeconds);
            return true;
        }
    }

    public interface IObjectPoolingPolicyService
    {
        void Warmup(GameObject prefab, int count);
        GameObject Rent(GameObject prefab);
        void Return(GameObject prefab, GameObject instance);
    }

    public sealed class ObjectPoolingPolicyService : IObjectPoolingPolicyService, IDisposable
    {
        private readonly Dictionary<int, Queue<GameObject>> _poolByPrefabId = new Dictionary<int, Queue<GameObject>>();
        private readonly Dictionary<int, Transform> _roots = new Dictionary<int, Transform>();

        public void Warmup(GameObject prefab, int count)
        {
            if (prefab == null || count <= 0)
                return;

            var queue = GetPool(prefab);
            for (int i = 0; i < count; i++)
            {
                var item = UnityEngine.Object.Instantiate(prefab);
                item.SetActive(false);
                item.transform.SetParent(GetRoot(prefab), false);
                queue.Enqueue(item);
            }
        }

        public GameObject Rent(GameObject prefab)
        {
            if (prefab == null)
                return null;

            var queue = GetPool(prefab);
            if (queue.Count == 0)
                Warmup(prefab, 1);

            var instance = queue.Dequeue();
            if (instance != null)
                instance.SetActive(true);

            return instance;
        }

        public void Return(GameObject prefab, GameObject instance)
        {
            if (prefab == null || instance == null)
                return;

            instance.SetActive(false);
            instance.transform.SetParent(GetRoot(prefab), false);
            GetPool(prefab).Enqueue(instance);
        }

        public void Dispose()
        {
            foreach (var queue in _poolByPrefabId.Values)
            {
                while (queue.Count > 0)
                {
                    var obj = queue.Dequeue();
                    if (obj != null)
                        UnityEngine.Object.Destroy(obj);
                }
            }

            foreach (var root in _roots.Values)
            {
                if (root != null)
                    UnityEngine.Object.Destroy(root.gameObject);
            }

            _poolByPrefabId.Clear();
            _roots.Clear();
        }

        private Queue<GameObject> GetPool(GameObject prefab)
        {
            int id = prefab.GetInstanceID();
            if (!_poolByPrefabId.TryGetValue(id, out var queue))
            {
                queue = new Queue<GameObject>();
                _poolByPrefabId[id] = queue;
            }

            return queue;
        }

        private Transform GetRoot(GameObject prefab)
        {
            int id = prefab.GetInstanceID();
            if (_roots.TryGetValue(id, out var root) && root != null)
                return root;

            var go = new GameObject($"Pool_{prefab.name}");
            root = go.transform;
            _roots[id] = root;
            return root;
        }
    }

    public sealed class GcAllocationMonitorService : ITickable
    {
        private const float WarningThrottleSeconds = 30f;

        private readonly GcMonitorSettings _settings;
        private long _lastTotalMemory;
        private float _nextSampleAt;
        private float _nextWarningAt;
        private int _suppressedBurstWarnings;

        public GcAllocationMonitorService()
        {
            _settings = AdaptivePerformanceDefaultsProvider.LoadGcMonitorSettings();
            _lastTotalMemory = GC.GetTotalMemory(false);
            _nextSampleAt = Time.unscaledTime + _settings.SampleIntervalSeconds;
        }

        public void Tick()
        {
            if (!_settings.Enabled)
                return;

            if (Time.unscaledTime < _nextSampleAt)
                return;

            _nextSampleAt = Time.unscaledTime + _settings.SampleIntervalSeconds;
            long current = GC.GetTotalMemory(false);
            long delta = current - _lastTotalMemory;
            _lastTotalMemory = current;

            if (delta <= _settings.BurstThresholdBytes)
                return;

            if (Time.unscaledTime < _nextWarningAt)
            {
                _suppressedBurstWarnings++;
                return;
            }

            string suppressed = _suppressedBurstWarnings > 0
                ? $" suppressed={_suppressedBurstWarnings}"
                : string.Empty;
            _suppressedBurstWarnings = 0;
            _nextWarningAt = Time.unscaledTime + WarningThrottleSeconds;

            Debug.LogWarning($"[PerfGC] Allocation burst detected: +{delta / 1024f:0.0}KB in {_settings.SampleIntervalSeconds:0.0}s.{suppressed}");
        }
    }

    public sealed class CpuHotspotSamplerService : ITickable
    {
        private readonly HotspotSamplingSettings _settings;
        private readonly Dictionary<string, SampleBucket> _samples = new Dictionary<string, SampleBucket>(StringComparer.Ordinal);
        private float _nextFlushAt;

        public CpuHotspotSamplerService()
        {
            _settings = AdaptivePerformanceDefaultsProvider.LoadHotspotSampling();
            _nextFlushAt = Time.unscaledTime + _settings.SampleIntervalSeconds;
        }

        public void Tick()
        {
            if (!IsEnabled())
                return;

            if (Time.unscaledTime < _nextFlushAt)
                return;

            _nextFlushAt = Time.unscaledTime + _settings.SampleIntervalSeconds;

            foreach (var pair in _samples)
            {
                if (pair.Value.MaxMs >= _settings.HotspotThresholdMs)
                {
                    Debug.LogWarning($"[PerfHotspot] scope={pair.Key} max={pair.Value.MaxMs:0.00}ms avg={pair.Value.AverageMs:0.00}ms samples={pair.Value.Count}");
                }
            }

            _samples.Clear();
        }

        public void Report(string scope, float elapsedMs)
        {
            if (!IsEnabled() || string.IsNullOrWhiteSpace(scope))
                return;

            if (!_samples.TryGetValue(scope, out var bucket))
                bucket = new SampleBucket();

            bucket.Count++;
            bucket.TotalMs += elapsedMs;
            bucket.MaxMs = Mathf.Max(bucket.MaxMs, elapsedMs);
            _samples[scope] = bucket;
        }

        private bool IsEnabled()
        {
            return _settings.Enabled && Debug.isDebugBuild;
        }

        private struct SampleBucket
        {
            public int Count;
            public float TotalMs;
            public float MaxMs;
            public float AverageMs => Count <= 0 ? 0f : TotalMs / Count;
        }
    }

    public static class PerformanceScope
    {
        private static CpuHotspotSamplerService _sampler;

        [Inject]
        public static void InstallSampler([InjectOptional] CpuHotspotSamplerService sampler)
        {
            _sampler = sampler;
        }

        public static IDisposable Measure(string scope)
        {
            return new ScopeToken(scope, _sampler);
        }

        private sealed class ScopeToken : IDisposable
        {
            private readonly string _scope;
            private readonly CpuHotspotSamplerService _sampler;
            private readonly Stopwatch _watch;

            public ScopeToken(string scope, CpuHotspotSamplerService sampler)
            {
                _scope = scope;
                _sampler = sampler;
                _watch = Stopwatch.StartNew();
            }

            public void Dispose()
            {
                _watch.Stop();
                _sampler?.Report(_scope, (float)_watch.Elapsed.TotalMilliseconds);
            }
        }
    }

    public sealed class FrameTimeDeveloperHudService : IInitializable, ITickable, IDisposable
    {
        private static readonly Type KeyboardType = Type.GetType("UnityEngine.InputSystem.Keyboard, Unity.InputSystem");
        private static readonly PropertyInfo KeyboardCurrentProperty = KeyboardType?.GetProperty("current", BindingFlags.Public | BindingFlags.Static);
        private static readonly PropertyInfo KeyboardF10KeyProperty = KeyboardType?.GetProperty("f10Key", BindingFlags.Public | BindingFlags.Instance);
        private static readonly PropertyInfo ButtonWasPressedThisFrameProperty = Type.GetType("UnityEngine.InputSystem.Controls.ButtonControl, Unity.InputSystem")?.GetProperty("wasPressedThisFrame", BindingFlags.Public | BindingFlags.Instance);

        private readonly IFrameBudgetMonitorService _monitor;
        private FrameTimeHudBehaviour _hud;

        public FrameTimeDeveloperHudService([InjectOptional] IFrameBudgetMonitorService monitor)
        {
            _monitor = monitor;
        }

        public void Initialize()
        {
            if (!Debug.isDebugBuild)
                return;

            var go = new GameObject("MoyvaFrameTimeHUD");
            UnityEngine.Object.DontDestroyOnLoad(go);
            _hud = go.AddComponent<FrameTimeHudBehaviour>();
            _hud.SetVisible(false);
        }

        public void Tick()
        {
            if (_hud == null || _monitor == null)
                return;

            if (IsF10PressedThisFrame())
                _hud.SetVisible(!_hud.Visible);

            _hud.UpdateSnapshot(_monitor.GetSnapshot());
        }

        public void Dispose()
        {
            if (_hud != null)
                UnityEngine.Object.Destroy(_hud.gameObject);
        }

        private static bool IsF10PressedThisFrame()
        {
            if (KeyboardCurrentProperty != null && KeyboardF10KeyProperty != null && ButtonWasPressedThisFrameProperty != null)
            {
                var keyboard = KeyboardCurrentProperty.GetValue(null);
                if (keyboard != null)
                {
                    var f10Key = KeyboardF10KeyProperty.GetValue(keyboard);
                    if (f10Key != null && ButtonWasPressedThisFrameProperty.GetValue(f10Key) is bool pressed)
                        return pressed;
                }
            }

#if ENABLE_LEGACY_INPUT_MANAGER
            try
            {
                return Input.GetKeyDown(KeyCode.F10);
            }
            catch (InvalidOperationException)
            {
                return false;
            }
#else
            return false;
#endif
        }

        private sealed class FrameTimeHudBehaviour : MonoBehaviour
        {
            private FrameTimeSnapshot _snapshot;
            public bool Visible { get; private set; }

            public void SetVisible(bool value)
            {
                Visible = value;
            }

            public void UpdateSnapshot(FrameTimeSnapshot snapshot)
            {
                _snapshot = snapshot;
            }

            private void OnGUI()
            {
                if (!Visible)
                    return;

                GUI.color = new Color(1f, 1f, 1f, 0.95f);
                GUILayout.BeginArea(new Rect(12f, 12f, 380f, 140f), "FrameTime", GUI.skin.window);
                GUILayout.Label($"p50: {_snapshot.P50Ms:0.00} ms");
                GUILayout.Label($"p95: {_snapshot.P95Ms:0.00} ms");
                GUILayout.Label($"p99: {_snapshot.P99Ms:0.00} ms");
                GUILayout.Label($"CPU budget: {_snapshot.CpuBudgetMs:0.00} ms");
                GUILayout.Label($"GPU budget: {_snapshot.GpuBudgetMs:0.00} ms");
                if (!string.IsNullOrWhiteSpace(_snapshot.LastDegradationReason))
                    GUILayout.Label($"last degrade: {_snapshot.LastDegradationReason}");
                GUILayout.EndArea();
            }
        }
    }
}
