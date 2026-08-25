using System;
using System.Collections.Generic;
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

    public interface IStartupPrewarmService
    {
        Task PrewarmAsync(CancellationToken ct = default);
    }

    public interface IScenePreActivationInitializer
    {
        Task InitializeBeforeActivationAsync(
            CancellationToken ct = default);
    }

    public sealed class StartupPrewarmService : IStartupPrewarmService
    {
        private readonly PrewarmSettings _settings;

        public StartupPrewarmService()
        {
            _settings = AdaptivePerformanceDefaultsProvider.LoadPrewarmSettings();
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
