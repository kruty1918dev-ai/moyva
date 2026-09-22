using System;
using Kruty1918.Audio;
using Kruty1918.Calendar.Core;
using Kruty1918.Calendar.Domain;
using Kruty1918.Moyva.GameAudio.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.GameAudio.Runtime
{
    /// <summary>
    /// Випадкові 2D-стингери амбієнсу (птах, комаха, порив вітру) з JSON-інтервалами.
    /// Гейтяться zoom-діапазоном кожного правила та (опційно) фазою доби.
    /// </summary>
    public sealed class AmbienceOneShotService : IInitializable, ITickable, IDisposable
    {
        private readonly IAudioService _audio;
        private readonly AudioAmbienceConfig _config;
        private readonly AudioZoomFocusService _zoom;
        private readonly ICalendarService _calendar;
        private float[] _nextAt;

        private DayPhase _phase = DayPhase.Day;

        /// <summary>Створює сервіс разових амбієнтних звуків із залежностями аудіо.</summary>
        public AmbienceOneShotService(
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

        /// <summary>Підписує сервіс на розклад амбієнтних подій.</summary>
        public void Initialize()
        {
            if (_calendar != null)
            {
                _phase = _calendar.CurrentDayPhase;
                _calendar.OnDayPhaseChanged += OnDayPhaseChanged;
            }

            if (_config?.oneShots == null)
                return;

            _nextAt = new float[_config.oneShots.Length];
            for (int i = 0; i < _nextAt.Length; i++)
                _nextAt[i] = Time.unscaledTime + NextDelay(_config.oneShots[i]);
        }

        /// <summary>Відписує сервіс.</summary>
        public void Dispose()
        {
            if (_calendar != null)
                _calendar.OnDayPhaseChanged -= OnDayPhaseChanged;
        }

        /// <summary>Програє заплановані разові звуки.</summary>
        public void Tick()
        {
            if (_nextAt == null || _audio == null)
                return;

            float zoomT = _zoom?.ZoomT ?? 0f;
            for (int i = 0; i < _config.oneShots.Length; i++)
            {
                var shot = _config.oneShots[i];
                if (shot == null || Time.unscaledTime < _nextAt[i])
                    continue;

                _nextAt[i] = Time.unscaledTime + NextDelay(shot);
                if (zoomT < shot.minZoom || zoomT > shot.maxZoom
                    || string.IsNullOrWhiteSpace(shot.soundKey)
                    || !DayPhaseAudioGate.Allows(shot.dayPhases, _phase))
                    continue;

                _audio.Play(shot.soundKey, new AudioPlayOptions(volumeScale: shot.volume));
            }
        }

        private void OnDayPhaseChanged(DayPhase phase)
            => _phase = phase;

        private static float NextDelay(AudioAmbienceOneShot shot)
        {
            float min = Mathf.Max(0.05f, shot?.minInterval ?? 4f);
            float max = Mathf.Max(min, shot?.maxInterval ?? 12f);
            return UnityEngine.Random.Range(min, max);
        }
    }
}
