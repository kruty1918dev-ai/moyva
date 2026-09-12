using System;
using UnityEngine;

namespace Kruty1918.Moyva.AI.Training
{
    public sealed class TrainingPerformanceController : IDisposable
    {
        private readonly float _timeScale = Time.timeScale;
        private readonly int _vSync = QualitySettings.vSyncCount;
        private readonly int _frameRate = Application.targetFrameRate;
        private readonly bool _audioPause = AudioListener.pause;
        private bool _disposed;
        public TrainingPerformanceController(TrainingConfig config, TrainingPresentationMode mode)
        {
            SetSpeed(mode == TrainingPresentationMode.Visual ? config.visualTimeScale : config.headlessTimeScale);
            AudioListener.pause = mode != TrainingPresentationMode.Visual || !config.enableAudioInVisualMode;
            if (mode == TrainingPresentationMode.HeadlessFast)
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = -1;
            }
        }
        public void SetSpeed(float value)
        {
            if (_disposed || !TrainingConfig.Finite(value)) return;
            Time.timeScale = Mathf.Clamp(value, 0.1f, 20f);
        }
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Time.timeScale = _timeScale;
            QualitySettings.vSyncCount = _vSync;
            Application.targetFrameRate = _frameRate;
            AudioListener.pause = _audioPause;
        }
    }
}
