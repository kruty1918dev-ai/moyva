using System;
using UnityEngine;

namespace Kruty1918.Moyva.Vfx.Runtime
{
    /// <summary>Екземпляр VFX-ефекту на сцені: керує часом життя, тінтом частинок та поверненням у пул.</summary>
    [DisallowMultipleComponent]
    public sealed class VfxEffect : MonoBehaviour
    {
        [Tooltip("Seconds until the pool auto-returns this effect. Authored to cover the longest child system.")]
        [Min(0.05f)] public float duration = 1f;

        /// <summary>Частинки, що отримують тінт фракції.</summary>
        [Tooltip("Particle systems tinted by faction color when the rule requests it.")]
        public ParticleSystem[] tintTargets = Array.Empty<ParticleSystem>();

        private ParticleSystem[] _systems;
        private float[] _baseRateOverTime;
        private float[] _baseRateOverDistance;
        private ParticleSystem.Burst[][] _baseBursts;
        private ParticleSystem.MinMaxGradient[] _baseTintColors;
        private Vector3 _baseLocalScale;
        private bool _captured;

        /// <summary>Префаб-джерело цього інстанса.</summary>
        public GameObject SourcePrefab { get; internal set; }

        /// <summary>Тривалість ефекту в секундах.</summary>
        public float Duration => duration;

        /// <summary>Запускає ефект із параметрами запиту.</summary>
        public void Play(in API.VfxSpawnRequest request)
        {
            Capture();

            Transform t = transform;
            t.SetPositionAndRotation(request.Position, request.Rotation);
            t.localScale = _baseLocalScale * Mathf.Max(0.01f, request.Scale);

            float countScale = Mathf.Clamp(request.CountScale, 0f, 4f);
            bool applyCount = Mathf.Abs(countScale - 1f) > 0.001f;
            bool applyTint = request.HasTint && tintTargets != null && tintTargets.Length > 0;

            for (int i = 0; i < _systems.Length; i++)
            {
                ParticleSystem system = _systems[i];
                if (system == null)
                    continue;

                if (applyCount)
                {
                    var emission = system.emission;
                    emission.rateOverTimeMultiplier = _baseRateOverTime[i] * countScale;
                    emission.rateOverDistanceMultiplier = _baseRateOverDistance[i] * countScale;
                    if (_baseBursts[i] != null)
                    {
                        var bursts = new ParticleSystem.Burst[_baseBursts[i].Length];
                        for (int b = 0; b < bursts.Length; b++)
                        {
                            bursts[b] = _baseBursts[i][b];
                            var burstCount = bursts[b].count;
                            burstCount.curveMultiplier = _baseBursts[i][b].count.curveMultiplier * countScale;
                            bursts[b].count = burstCount;
                        }
                        emission.SetBursts(bursts);
                    }
                }
            }

            if (applyTint)
            {
                for (int i = 0; i < tintTargets.Length; i++)
                {
                    ParticleSystem system = tintTargets[i];
                    if (system == null)
                        continue;

                    int baseIndex = Array.IndexOf(_systems, system);
                    if (baseIndex < 0)
                        continue;

                    var main = system.main;
                    main.startColor = Multiply(_baseTintColors[i], request.Tint);
                }
            }

            for (int i = 0; i < _systems.Length; i++)
            {
                ParticleSystem system = _systems[i];
                if (system == null)
                    continue;

                system.Clear(true);
                system.Play(true);
            }
        }

        /// <summary>Зупиняє ефект і готує до повернення в пул.</summary>
        public void Stop()
        {
            if (!_captured)
                return;

            for (int i = 0; i < _systems.Length; i++)
            {
                ParticleSystem system = _systems[i];
                if (system == null)
                    continue;

                system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                var emission = system.emission;
                emission.rateOverTimeMultiplier = _baseRateOverTime[i];
                emission.rateOverDistanceMultiplier = _baseRateOverDistance[i];
                if (_baseBursts[i] != null)
                    emission.SetBursts(_baseBursts[i]);
            }

            if (tintTargets != null)
            {
                for (int i = 0; i < tintTargets.Length; i++)
                {
                    ParticleSystem system = tintTargets[i];
                    if (system == null)
                        continue;

                    int baseIndex = Array.IndexOf(_systems, system);
                    if (baseIndex < 0)
                        continue;

                    var main = system.main;
                    main.startColor = _baseTintColors[baseIndex];
                }
            }

            Transform t = transform;
            t.localScale = _baseLocalScale;
            t.localRotation = Quaternion.identity;
        }

        private void Capture()
        {
            if (_captured)
                return;

            _systems = GetComponentsInChildren<ParticleSystem>(true);
            int count = _systems.Length;
            _baseRateOverTime = new float[count];
            _baseRateOverDistance = new float[count];
            _baseBursts = new ParticleSystem.Burst[count][];
            _baseTintColors = new ParticleSystem.MinMaxGradient[count];

            for (int i = 0; i < count; i++)
            {
                ParticleSystem system = _systems[i];
                var emission = system.emission;
                _baseRateOverTime[i] = emission.rateOverTimeMultiplier;
                _baseRateOverDistance[i] = emission.rateOverDistanceMultiplier;

                int burstCount = emission.burstCount;
                if (burstCount > 0)
                {
                    var bursts = new ParticleSystem.Burst[burstCount];
                    emission.GetBursts(bursts);
                    _baseBursts[i] = bursts;
                }

                _baseTintColors[i] = system.main.startColor;
            }

            _baseLocalScale = transform.localScale;
            _captured = true;
        }

        private static ParticleSystem.MinMaxGradient Multiply(ParticleSystem.MinMaxGradient source, Color tint)
        {
            switch (source.mode)
            {
                case ParticleSystemGradientMode.Color:
                    return new ParticleSystem.MinMaxGradient(source.color * tint);
                case ParticleSystemGradientMode.TwoColors:
                    return new ParticleSystem.MinMaxGradient(source.colorMin * tint, source.colorMax * tint);
                case ParticleSystemGradientMode.TwoGradients:
                    return new ParticleSystem.MinMaxGradient(
                        MultiplyGradient(source.gradientMin, tint),
                        MultiplyGradient(source.gradientMax, tint));
                case ParticleSystemGradientMode.Gradient:
                    return new ParticleSystem.MinMaxGradient(MultiplyGradient(source.gradient, tint));
                default:
                    return source;
            }
        }

        private static Gradient MultiplyGradient(Gradient source, Color tint)
        {
            if (source == null)
                return null;

            var gradient = new Gradient();
            GradientColorKey[] colorKeys = source.colorKeys;
            var tinted = new GradientColorKey[colorKeys.Length];
            for (int i = 0; i < colorKeys.Length; i++)
                tinted[i] = new GradientColorKey(colorKeys[i].color * tint, colorKeys[i].time);

            gradient.SetKeys(tinted, source.alphaKeys);
            gradient.mode = source.mode;
            return gradient;
        }
    }
}
