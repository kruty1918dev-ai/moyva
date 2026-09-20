using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Audio.API;
using Kruty1918.Moyva.GameAudio.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.GameAudio.Runtime
{
    /// <summary>
    /// Глобальні циклічні шари амбієнсу (вітер, птахи, комахи).
    /// Кожен шар має near/far ваги — мікс плавно змінюється за zoom камери,
    /// а шари з lowpassWithZoom отримують спільний AudioLowPassFilter.
    /// </summary>
    public sealed class AmbienceBedService : IInitializable, ITickable, IDisposable
    {
        private sealed class Bed
        {
            public AudioAmbienceBed Config;
            public AudioHandle Handle;
            public AudioLowPassFilter LowPass;
            public float CurrentScale = 1f;
        }

        private readonly IAudioService _audio;
        private readonly AudioAmbienceConfig _config;
        private readonly AudioZoomFocusService _zoom;
        private readonly List<Bed> _beds = new List<Bed>();

        public AmbienceBedService(
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

                var entry = new Bed { Config = bed, Handle = handle };
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

            ApplyWeights();
        }

        public void Tick()
        {
            if (_beds.Count == 0)
                return;

            ApplyWeights();
        }

        public void Dispose()
        {
            foreach (var bed in _beds)
                bed.Handle.Stop();
            _beds.Clear();
        }

        private void ApplyWeights()
        {
            float t = _zoom?.ZoomT ?? 0f;
            float sharedCutoff = _zoom?.EvaluateBedCutoff() ?? 22000f;

            foreach (var bed in _beds)
            {
                var cfg = bed.Config;
                float weight = Mathf.Lerp(cfg.nearWeight, cfg.farWeight, t);
                if (!Mathf.Approximately(bed.CurrentScale, weight))
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
