using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Audio.API;
using Kruty1918.Calendar.Core;
using Kruty1918.Calendar.Domain;
using Kruty1918.Moyva.GameAudio.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.GameAudio.Runtime
{
    /// <summary>
    /// Глобальні циклічні шари амбієнсу (вітер, птахи, комахи).
    /// Кожен шар має near/far ваги — мікс плавно змінюється за zoom камери,
    /// а шари з lowpassWithZoom отримують спільний AudioLowPassFilter.
    /// Шари з dayPhases додатково згасають поза своєю фазою доби (нічні цвіркуни тощо).
    /// </summary>
    public sealed class AmbienceBedService : IInitializable, ITickable, IDisposable
    {
        /// <summary>Швидкість fade-переходу при зміні фази доби (повний перехід ~2с).</summary>
        private const float PhaseFadeSpeed = 0.5f;

        /// <summary>Стан однієї амбієнтної бази: конфіг, хендл відтворення, фільтр і масштаби гучності.</summary>
        private sealed class Bed
        {
            /// <summary>Конфігурація бази.</summary>
            public AudioAmbienceBed Config;
            /// <summary>Хендл активного відтворення.</summary>
            public AudioHandle Handle;
            /// <summary>Lowpass-фільтр на еміттері бази.</summary>
            public AudioLowPassFilter LowPass;
            /// <summary>Поточний масштаб гучності бази.</summary>
            public float CurrentScale = 1f;
            /// <summary>Масштаб гучності за фазою дня.</summary>
            public float PhaseScale = 1f;
        }

        private readonly IAudioService _audio;
        private readonly AudioAmbienceConfig _config;
        private readonly AudioZoomFocusService _zoom;
        private readonly ICalendarService _calendar;
        private readonly List<Bed> _beds = new List<Bed>();

        private DayPhase _phase = DayPhase.Day;
        private bool _hasPhaseGatedBeds;

        /// <summary>Створює сервіс із залежностями аудіо та зуму.</summary>
        public AmbienceBedService(
            [InjectOptional] IAudioService audio,
            [InjectOptional] AudioAmbienceConfig config,
            [InjectOptional] AudioZoomFocusService zoom,
            [InjectOptional] ICalendarService calendar)
        {
            _audio = audio;
            _config = config;
            _zoom = zoom;
            _calendar = calendar;
        }

        /// <summary>Запускає амбієнтні бази з конфігурації.</summary>
        public void Initialize()
        {
            if (_calendar != null)
            {
                _phase = _calendar.CurrentDayPhase;
                _calendar.OnDayPhaseChanged += OnDayPhaseChanged;
            }

            if (_audio == null || _config?.beds == null)
                return;

            foreach (var bed in _config.beds)
            {
                if (bed == null || string.IsNullOrWhiteSpace(bed.soundKey))
                    continue;

                var handle = _audio.Play(bed.soundKey,
                    new AudioPlayOptions(volumeScale: bed.volume, loopOverride: true));
                if (!handle.IsValid || handle.Source == null)
                    continue;

                var entry = new Bed
                {
                    Config = bed,
                    Handle = handle,
                    PhaseScale = DayPhaseAudioGate.Allows(bed.dayPhases, _phase) ? 1f : 0f,
                };
                if (bed.dayPhases != null && bed.dayPhases.Length > 0)
                    _hasPhaseGatedBeds = true;

                if (bed.lowpassWithZoom)
                {
                    // GetComponent may return Unity's fake-null object; '??' does
                    // not catch it and the .enabled setter throws. '==' does.
                    var lowPass = handle.Source.GetComponent<AudioLowPassFilter>();
                    if (lowPass == null)
                        lowPass = handle.Source.gameObject.AddComponent<AudioLowPassFilter>();
                    entry.LowPass = lowPass;
                    entry.LowPass.enabled = true;
                }

                _beds.Add(entry);
            }

            ApplyWeights(force: true);
        }

        /// <summary>Оновлює гучність і фільтри баз за зумом та фазою дня.</summary>
        public void Tick()
        {
            if (_beds.Count == 0)
                return;

            if (_hasPhaseGatedBeds)
            {
                float step = PhaseFadeSpeed * Time.unscaledDeltaTime;
                foreach (var bed in _beds)
                {
                    float target = DayPhaseAudioGate.Allows(bed.Config.dayPhases, _phase) ? 1f : 0f;
                    bed.PhaseScale = Mathf.MoveTowards(bed.PhaseScale, target, step);
                }
            }

            ApplyWeights(force: false);
        }

        /// <summary>Зупиняє всі бази та звільняє ресурси.</summary>
        public void Dispose()
        {
            if (_calendar != null)
                _calendar.OnDayPhaseChanged -= OnDayPhaseChanged;

            foreach (var bed in _beds)
                bed.Handle.Stop();
            _beds.Clear();
        }

        private void OnDayPhaseChanged(DayPhase phase)
            => _phase = phase;

        private void ApplyWeights(bool force)
        {
            float t = _zoom?.ZoomT ?? 0f;
            float sharedCutoff = _zoom?.EvaluateBedCutoff() ?? 22000f;

            foreach (var bed in _beds)
            {
                var cfg = bed.Config;
                float weight = Mathf.Lerp(cfg.nearWeight, cfg.farWeight, t) * bed.PhaseScale;
                if (force || !Mathf.Approximately(bed.CurrentScale, weight))
                {
                    bed.CurrentScale = weight;
                    _audio.SetPlaybackScale(bed.Handle, weight);
                }

                if (bed.LowPass != null)
                {
                    float cutoff = cfg.farCutoff > 0f
                        ? (_zoom?.EvaluateBedCutoff(cfg.farCutoff) ?? sharedCutoff)
                        : sharedCutoff;
                    bed.LowPass.cutoffFrequency = cutoff;
                }
            }
        }
    }
}
