using System;
using Kruty1918.Moyva.Audio.API;
using Kruty1918.Moyva.GameAudio.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.GameAudio.Runtime
{
    /// <summary>
    /// Випадкові 2D-стингери амбієнсу (птах, комаха, порив вітру) з JSON-інтервалами.
    /// Гейтяться zoom-діапазоном кожного правила.
    /// </summary>
    public sealed class AmbienceOneShotService : IInitializable, ITickable
    {
        private readonly IAudioService _audio;
        private readonly AudioAmbienceConfig _config;
        private readonly AudioZoomFocusService _zoom;
        private float[] _nextAt;

        public AmbienceOneShotService(
            [InjectOptional] IAudioService audio,
            [InjectOptional] AudioAmbienceConfig config,
            [InjectOptional] AudioZoomFocusService zoom)
        {
            _audio = audio;
            _config = config;
            _zoom = zoom;
        }

        public void Initialize()
        {
            if (_config?.oneShots == null)
                return;

            _nextAt = new float[_config.oneShots.Length];
            for (int i = 0; i < _nextAt.Length; i++)
                _nextAt[i] = Time.unscaledTime + NextDelay(_config.oneShots[i]);
        }

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
                    || string.IsNullOrWhiteSpace(shot.soundKey))
                    continue;

                _audio.Play(shot.soundKey, new AudioPlayOptions(volumeScale: shot.volume));
            }
        }

        private static float NextDelay(AudioAmbienceOneShot shot)
        {
            float min = Mathf.Max(0.05f, shot?.minInterval ?? 4f);
            float max = Mathf.Max(min, shot?.maxInterval ?? 12f);
            return UnityEngine.Random.Range(min, max);
        }
    }
}
