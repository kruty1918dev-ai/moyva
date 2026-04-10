using System;
using Kruty1918.Moyva.Calendar.Core;
using Kruty1918.Moyva.Calendar.Domain;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Visuals
{
    /// <summary>
    /// Converts calendar time into global shader parameters for day/night visuals.
    /// </summary>
    public sealed class DayNightShaderController : IInitializable, ITickable, IDisposable
    {
        private static readonly int DayNightLerpId = Shader.PropertyToID("_Moyva_DayNightLerp");
        private static readonly int DayPhaseId = Shader.PropertyToID("_Moyva_DayPhase");
        private static readonly int TimeOfDay01Id = Shader.PropertyToID("_Moyva_TimeOfDay01");
        private const float TransitionDurationSeconds = 2.2f;
        private const float Epsilon = 0.0005f;

        private readonly ICalendarService _calendar;

        private float _currentDayNightLerp;
        private float _targetDayNightLerp;
        private float _currentTimeOfDay01;
        private float _targetTimeOfDay01;
        private float _currentPhaseEncoded;
        private float _targetPhaseEncoded;
        private bool _isInitialized;

        public DayNightShaderController([InjectOptional] ICalendarService calendar = null)
        {
            _calendar = calendar;
        }

        public void Initialize()
        {
            if (_calendar == null)
            {
                Shader.SetGlobalFloat(DayNightLerpId, 1f);
                Shader.SetGlobalFloat(DayPhaseId, EncodePhase(DayPhase.Day));
                Shader.SetGlobalFloat(TimeOfDay01Id, 0.5f);
                Debug.LogWarning("[DayNight] ICalendarService не знайдено. Використовую статичний денний стан.");
                return;
            }

            _calendar.OnHourChanged += Apply;
            _calendar.OnDayPhaseChanged += OnDayPhaseChanged;
            Apply();
            PushToShader();
        }

        public void Tick()
        {
            if (_calendar == null)
                return;

            float smoothingFactor = 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(0.01f, TransitionDurationSeconds));

            _currentDayNightLerp = Mathf.Lerp(_currentDayNightLerp, _targetDayNightLerp, smoothingFactor);
            _currentTimeOfDay01 = Mathf.Lerp(_currentTimeOfDay01, _targetTimeOfDay01, smoothingFactor);
            _currentPhaseEncoded = Mathf.Lerp(_currentPhaseEncoded, _targetPhaseEncoded, smoothingFactor);

            if (Mathf.Abs(_currentDayNightLerp - _targetDayNightLerp) <= Epsilon)
                _currentDayNightLerp = _targetDayNightLerp;

            if (Mathf.Abs(_currentTimeOfDay01 - _targetTimeOfDay01) <= Epsilon)
                _currentTimeOfDay01 = _targetTimeOfDay01;

            if (Mathf.Abs(_currentPhaseEncoded - _targetPhaseEncoded) <= Epsilon)
                _currentPhaseEncoded = _targetPhaseEncoded;

            PushToShader();
        }

        public void Dispose()
        {
            if (_calendar == null)
                return;

            _calendar.OnHourChanged -= Apply;
            _calendar.OnDayPhaseChanged -= OnDayPhaseChanged;
        }

        private void OnDayPhaseChanged(DayPhase _)
        {
            Apply();
        }

        private void Apply()
        {
            var cfg = _calendar.Config;
            int hour = _calendar.Current.Hour;

            _targetDayNightLerp = ComputeDayNightLerp(cfg, hour);
            _targetTimeOfDay01 = cfg.HoursInDay > 0
                ? Mathf.Clamp01(hour / (float)cfg.HoursInDay)
                : 0f;
            _targetPhaseEncoded = EncodePhase(_calendar.CurrentDayPhase);

            // First update should not jump from zeroed shader state.
            if (!_isInitialized)
            {
                _currentDayNightLerp = _targetDayNightLerp;
                _currentTimeOfDay01 = _targetTimeOfDay01;
                _currentPhaseEncoded = _targetPhaseEncoded;
                _isInitialized = true;
            }
        }

        private void PushToShader()
        {
            Shader.SetGlobalFloat(DayNightLerpId, _currentDayNightLerp);
            Shader.SetGlobalFloat(DayPhaseId, _currentPhaseEncoded);
            Shader.SetGlobalFloat(TimeOfDay01Id, _currentTimeOfDay01);
        }

        private static float EncodePhase(DayPhase phase)
        {
            return phase switch
            {
                DayPhase.Night => 0f,
                DayPhase.Dawn => 1f,
                DayPhase.Day => 2f,
                DayPhase.Dusk => 3f,
                _ => 0f
            };
        }

        private static float ComputeDayNightLerp(Kruty1918.Moyva.Calendar.Config.CalendarConfig cfg, int hour)
        {
            int dayStart = cfg.DayStartHour;
            int nightStart = cfg.NightStartHour;
            int dawnDuration = Mathf.Max(0, cfg.DawnDurationHours);
            int duskDuration = Mathf.Max(0, cfg.DuskDurationHours);

            int dawnStart = dayStart - dawnDuration;
            int duskStart = nightStart - duskDuration;

            if (dawnDuration > 0 && hour >= dawnStart && hour < dayStart)
                return Mathf.InverseLerp(dawnStart, dayStart, hour);

            if (hour >= dayStart && hour < duskStart)
                return 1f;

            if (duskDuration > 0 && hour >= duskStart && hour < nightStart)
                return 1f - Mathf.InverseLerp(duskStart, nightStart, hour);

            return 0f;
        }

    }
}