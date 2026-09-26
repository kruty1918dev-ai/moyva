using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Floating
{
    /// <summary>
    /// Cheap analytic buoyancy: replicates the vertical part of the water
    /// shader's Gerstner wave sum on the CPU (height only — steepness-only XZ
    /// displacement is skipped). Four height samples per floater yield the
    /// surface height and a wave normal; transforms ease toward that pose so
    /// motion reads as physical floating without rigidbodies.
    /// Updates are round-robin sliced by <see cref="WaterFloatRules.FrameStride"/>,
    /// so per-frame cost stays flat even on low-end phones.
    /// </summary>
    internal sealed class WaterFloatService : IWaterFloatService
    {
        private const float Gravity = 9.8f;
        private const float TwoPi = Mathf.PI * 2f;

        private struct WaveTerm
        {
            public Vector2 Direction;
            public float K;      // angular wave number 2pi / (length * frequency)
            public float Omega;  // sqrt(g * k), deep-water dispersion
            public float Amplitude;
        }

        private struct Floater
        {
            public Transform Transform;
            public float RestY;
            public float Radius;
            public float SmoothedY;
            public Vector3 SmoothedNormal;
            public Quaternion BaseRotation;
        }

        private readonly List<Floater> _floaters = new List<Floater>();
        private readonly WaveTerm[] _waves;
        private readonly float _speed;
        private readonly float _smoothing;
        private readonly float _tiltStrength;
        private readonly int _stride;
        private int _cursor;
        private WaterFloatDriver _driver;

        public WaterFloatService(
            [Zenject.InjectOptional] EnvironmentDecorationConfig config = null)
        {
            WaterFloatRules rules = config?.Floating ?? new WaterFloatRules();
            Enabled = rules.Enabled && rules.Waves != null && rules.Waves.Length > 0;
            _smoothing = Mathf.Max(0.01f, rules.Smoothing);
            _tiltStrength = rules.TiltStrength;
            _stride = Mathf.Max(1, rules.FrameStride);
            _speed = rules.WaveSpeed;

            var waves = new List<WaveTerm>(rules.Waves?.Length ?? 0);
            if (rules.Waves != null)
            {
                foreach (WaterFloatWave wave in rules.Waves)
                {
                    if (wave == null || !wave.Enabled
                        || wave.Amplitude <= 0f || wave.WaveLength <= 0f)
                    {
                        continue;
                    }

                    float k = TwoPi / (wave.WaveLength * Mathf.Max(0.0001f, rules.WaveFrequency));
                    float directionRad = wave.DirectionDegrees * Mathf.Deg2Rad;
                    waves.Add(new WaveTerm
                    {
                        Direction = new Vector2(
                            Mathf.Sin(directionRad) * rules.DirectionX,
                            Mathf.Cos(directionRad) * rules.DirectionY),
                        K = k,
                        Omega = Mathf.Sqrt(Gravity * k),
                        Amplitude = wave.Amplitude * rules.WaveHeight,
                    });
                }
            }

            _waves = waves.ToArray();
            Enabled = Enabled && _waves.Length > 0;
        }

        public bool Enabled { get; }
        public int Count => _floaters.Count;

        public void Register(Transform transform, float restWorldY, float sampleRadius)
        {
            if (!Enabled || transform == null)
                return;

            _floaters.Add(new Floater
            {
                Transform = transform,
                RestY = restWorldY,
                Radius = Mathf.Max(0.05f, sampleRadius),
                SmoothedY = restWorldY,
                SmoothedNormal = Vector3.up,
                BaseRotation = transform.rotation,
            });
            EnsureDriver();
        }

        public void Tick(float time, float deltaTime)
        {
            int count = _floaters.Count;
            if (count == 0 || _waves.Length == 0)
                return;

            if (_cursor >= count)
                _cursor = 0;

            int slice = Mathf.Max(1, (count + _stride - 1) / _stride);
            int processed = 0;
            // Entries update once per stride window, so the easing step spans
            // the whole window instead of a single frame.
            float blend = 1f - Mathf.Exp(-deltaTime * _stride / _smoothing);
            float phase = -_speed * time;

            for (int i = _cursor; i < count && processed < slice; i++, processed++)
            {
                Floater f = _floaters[i];
                if (f.Transform == null)
                {
                    // Spawned objects can be destroyed by cell-clearing; swap-remove.
                    int last = count - 1;
                    _floaters[i] = _floaters[last];
                    _floaters.RemoveAt(last);
                    count--;
                    i--;
                    continue;
                }

                Vector3 p = f.Transform.position;
                float r = f.Radius;
                float left = WaveY(p.x - r, p.z, phase);
                float right = WaveY(p.x + r, p.z, phase);
                float down = WaveY(p.x, p.z - r, phase);
                float up = WaveY(p.x, p.z + r, phase);

                float targetY = f.RestY + (left + right + down + up) * 0.25f;
                float invDiameter = 0.5f / r;
                var targetNormal = new Vector3(
                    (left - right) * invDiameter * _tiltStrength,
                    1f,
                    (down - up) * invDiameter * _tiltStrength).normalized;

                f.SmoothedY += (targetY - f.SmoothedY) * blend;
                f.SmoothedNormal = Vector3
                    .Lerp(f.SmoothedNormal, targetNormal, blend)
                    .normalized;

                p.y = f.SmoothedY;
                f.Transform.SetPositionAndRotation(
                    p,
                    Quaternion.FromToRotation(Vector3.up, f.SmoothedNormal)
                        * f.BaseRotation);
                _floaters[i] = f;
            }

            _cursor += processed;
        }

        public void Clear()
        {
            _floaters.Clear();
            _cursor = 0;
        }

        private float WaveY(float x, float z, float phase)
        {
            float sum = 0f;
            for (int i = 0; i < _waves.Length; i++)
            {
                WaveTerm w = _waves[i];
                sum += Mathf.Sin((w.Direction.x * x + w.Direction.y * z) * w.K
                    + w.Omega * phase) * w.Amplitude;
            }
            return sum;
        }

        private void EnsureDriver()
        {
            if (_driver != null || !Application.isPlaying)
                return;

            var go = new GameObject("WaterFloatDriver");
            _driver = go.AddComponent<WaterFloatDriver>();
            _driver.Initialize(this);
        }
    }

    /// <summary>
    /// Object/preset ids that read as free-floating on water. Rooted plants
    /// (reeds, sedges) and structural water builds (docks, mills) stay fixed.
    /// </summary>
    internal static class WaterFloatIdRules
    {
        public static bool IsFloatingId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            id = id.ToLowerInvariant();
            return id.Contains("lily")
                || id.Contains("waterplant")
                || id.Contains("boat")
                || id.Contains("buoy")
                || id.Contains("raft")
                || id.Contains("float");
        }
    }
}
